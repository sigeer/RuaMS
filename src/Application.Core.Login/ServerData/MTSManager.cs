using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Utility;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 拍卖行（MTS / ITC）
    /// <para>商品存放在中心服务器，所有频道通过 gRPC 访问</para>
    /// </summary>
    public class MTSManager : DataStorageBase<int, ProtoModel.MTSItemProto, MtsItemEntity>
    {
        readonly MasterServer _server;
        /// <summary>
        /// 购物车：角色id -> 商品id
        /// </summary>
        readonly ConcurrentDictionary<int, HashSet<int>> _carts = new();
        /// <summary>
        /// 在途物品转移的占位日志，四类操作（领取 / 上架 / 拍卖上架 / 求购成交）共用
        /// </summary>
        readonly ItemTransferJournal _transfers = new();

        public MTSManager(ILogger<MTSManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
            : base(StorageCategory.MTS, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(ProtoModel.MTSItemProto model) => model.Id;

        public override async Task InitializeAsync(DBContext dbContext)
        {
            await base.InitializeAsync(dbContext);

            var entities = await dbContext.Set<MtsItemEntity>().ToListAsync();
            foreach (var entity in entities)
            {
                SetCache(MapModel(entity));
            }
            _logger.LogInformation("拍卖行载入商品 {Count} 件", entities.Count);
        }

        #region 数据映射

        protected override ProtoModel.MTSItemProto MapModel(MtsItemEntity entity)
        {
            return new ProtoModel.MTSItemProto
            {
                Id = entity.Id,
                SellerId = entity.Seller,
                // 表里不再冗余存出售者名字，统一从角色数据里取（离线角色也在 CharacterManager 里，
                // 且它比本对象先初始化）
                SellerName = _server.CharacterManager.GetPlayerName(entity.Seller),
                Price = entity.Price,
                ReleaseDate = entity.ReleaseDate,
                SellEnds = entity.SellEnds,
                Tab = entity.Tab,
                Type = entity.Type,
                InTransfer = entity.Transfer != 0,
                // 拍卖三件套：Price 是当前价（起拍价 -> 最高出价），BuyNow 是卖家填的一口价，
                // BidIncre 是加价幅度，Bidder 是最高出价者。普通出售只用 Price。
                BuyNow = entity.Tab == MTSConstants.AUCTION_TAB ? entity.BuyNow : 0,
                BidIncrement = entity.Tab == MTSConstants.AUCTION_TAB ? entity.BidIncre : 0,
                BidderId = entity.Tab == MTSConstants.AUCTION_TAB ? entity.Bidder : 0,
                BidCount = entity.Tab == MTSConstants.AUCTION_TAB ? entity.BidCount : 0,
                Item = ParseItem(entity)
            };
        }

        /// <summary>
        /// 物品本体是整块序列化的 <see cref="ProtoModel.ItemProto"/>。
        /// 单条数据损坏不该让整个拍卖行起不来，解析失败时记日志并当作空物品（该条目会在下发时被跳过）。
        /// </summary>
        ProtoModel.ItemProto? ParseItem(MtsItemEntity entity)
        {
            if (entity.ItemBlob == null || entity.ItemBlob.Length == 0)
                return null;

            try
            {
                return ProtoModel.ItemProto.Parser.ParseFrom(entity.ItemBlob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拍卖行商品 {ProductId} 的物品数据无法解析，已跳过", entity.Id);
                return null;
            }
        }

        protected override MtsItemEntity MapEntity(ProtoModel.MTSItemProto model)
        {
            var item = model.Item ?? new ProtoModel.ItemProto();

            return new MtsItemEntity(
                model.Tab,
                model.Type,
                model.SellerId,
                model.Price,
                model.ReleaseDate,
                model.SellEnds,
                item.ToByteArray())
            {
                Id = model.Id,
                Transfer = model.InTransfer ? 1 : 0,
                Bidder = model.Tab == MTSConstants.AUCTION_TAB ? model.BidderId : 0,
                BidCount = model.Tab == MTSConstants.AUCTION_TAB ? model.BidCount : 0,
                // 拍卖：一口价 / 加价幅度落到原本就有的 buy_now、bid_incre 两列
                BuyNow = model.Tab == MTSConstants.AUCTION_TAB ? model.BuyNow : 0,
                BidIncre = model.Tab == MTSConstants.AUCTION_TAB ? model.BidIncrement : 0,
            };
        }

        protected override MtsItemEntity MapExsitedEntity(ProtoModel.MTSItemProto localModel, MtsItemEntity dbModel)
        {
            var mapped = MapEntity(localModel);
            dbModel.Id = mapped.Id;
            dbModel.Tab = mapped.Tab;
            dbModel.Type = mapped.Type;
            dbModel.ItemBlob = mapped.ItemBlob;
            dbModel.Seller = mapped.Seller;
            dbModel.Price = mapped.Price;
            dbModel.ReleaseDate = mapped.ReleaseDate;
            dbModel.SellEnds = mapped.SellEnds;
            dbModel.Transfer = mapped.Transfer;
            dbModel.BuyNow = mapped.BuyNow;
            dbModel.BidIncre = mapped.BidIncre;
            dbModel.Bidder = mapped.Bidder;
            dbModel.BidCount = mapped.BidCount;
            return dbModel;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 过期的商品自动转入“待领取”列表，由卖家自行取回
        /// </summary>
        List<ProtoModel.MTSItemProto> LoadLocal()
        {
            var now = _server.getCurrentTime();
            var list = QueryLocal();
            foreach (var item in list)
            {
                if (!item.InTransfer && item.SellEnds > 0 && item.SellEnds <= now)
                {
                    SettleExpired(item);
                }
            }

            // 下发时条目数量是先写出去的，而无法映射成物品的条目会被 MTSPacketCreator 静默跳过，
            // 那样客户端读到的条目数就和包里的数量对不上、后面全部错位。
            // 在这里统一滤掉（损坏的数据已经由 ParseItem 记过日志）。
            return list.Where(x => x.Item != null && x.Item.Itemid > 0).ToList();
        }

        /// <summary>
        /// 商品到期结算。
        /// <para>
        /// 普通出售 / 求购单：直接进登记人的“待领取”列表，由本人取回。
        /// 拍卖：如果有出价就成交给最高出价者 —— 出价时并没有预扣抵用券（见 <see cref="BidAuction"/>），
        /// 所以钱在这一刻结算；出价者付不出钱时商品退回卖家。
        /// </para>
        /// </summary>
        void SettleExpired(ProtoModel.MTSItemProto item)
        {
            item.InTransfer = true;

            if (item.Tab != MTSConstants.AUCTION_TAB || item.BidderId <= 0 || item.BidderId == item.SellerId)
            {
                SetDirty(item);
                return;
            }

            var bidderId = item.BidderId;
            var sellerId = item.SellerId;
            // 卖家实得成交价（= 最高出价），买家实付 500 + 1.07 × 成交价
            var salePrice = item.Price;
            var cost = MTSConstants.BuyerTotalCost(salePrice);

            if (!TryChangeNx(bidderId, -cost, out _))
            {
                _logger.LogWarning("拍卖商品 {ProductId} 到期时出价者 {BidderId} 抵用券不足，商品退回卖家 {SellerId}",
                    item.Id, bidderId, sellerId);
                item.BidderId = 0;
                SetDirty(item);
                return;
            }

            if (!TryChangeNx(sellerId, salePrice, out _))
            {
                // 卖家账号读不出来，回滚出价者的扣款
                TryChangeNx(bidderId, cost, out _);
                item.BidderId = 0;
                SetDirty(item);
                return;
            }

            // 物品归最高出价者，等他来“待领取”列表里拿
            item.SellerId = bidderId;
            item.SellerName = ResolveName(bidderId);
            item.BidderId = 0;
            SetDirty(item);
        }

        ProtoModel.PlayerMTSInfoProto LoadPlayerInfo(int sellerId)
        {
            var myItems = LoadLocal().Where(x => x.SellerId == sellerId);

            var res = new ProtoModel.PlayerMTSInfoProto();
            res.InTransfer.AddRange(myItems.Where(x => x.InTransfer));
            res.OnSale.AddRange(myItems.Where(x => !x.InTransfer));
            return res;
        }

        /// <summary>
        /// 列表排序：新上架的排在前面。以 <c>ReleaseDate</c> 为主键，同一毫秒内的再按 id 兜底。
        /// </summary>
        static IEnumerable<ProtoModel.MTSItemProto> OrderForDisplay(IEnumerable<ProtoModel.MTSItemProto> items)
            => items.OrderByDescending(x => x.ReleaseDate).ThenByDescending(x => x.Id);

        static ProtoModel.PagedItemsProto ToPage(List<ProtoModel.MTSItemProto> all, int tab, int type, int page)
        {
            var res = new ProtoModel.PagedItemsProto
            {
                Tab = tab,
                Type = type,
                Page = page,
                TotalPages = (int)Math.Ceiling(all.Count / (double)MTSConstants.PAGE_SIZE),
            };
            res.Items.AddRange(all.Skip(page * MTSConstants.PAGE_SIZE).Take(MTSConstants.PAGE_SIZE));
            return res;
        }

        ProtoModel.PagedItemsProto LoadPage(int tab, int type, int page)
        {
            var all = OrderForDisplay(LoadLocal()
                .Where(x => !x.InTransfer && x.Tab == tab && (type == 0 || x.Type == type)))
                .ToList();
            return ToPage(all, tab, type, page);
        }

        HashSet<int> GetCart(int ownerId) => _carts.GetOrAdd(ownerId, _ => []);

        /// <summary>
        /// 把一份物品暂存成一条“待领取”记录 —— 整套物品转移机制的兜底手段。
        /// <para>
        /// <c>InTransfer = true</c> 的记录不会出现在任何商品列表里（<see cref="LoadPage"/>、<see cref="LoadCart"/>
        /// 都按 <c>!InTransfer</c> 过滤），只会出现在它主人自己的待领取列表里
        /// （<see cref="LoadPlayerInfo"/>）。
        /// </para>
        /// <para>
        /// 于是上架 / 拍卖 / 求购成交的 <c>PREPARE</c> 都可以先把物品落到这里：
        /// 频道侧随后就算崩了、来不及 <c>COMMIT</c> 也来不及 <c>ABORT</c>，物品也稳稳躺在
        /// 玩家自己的待领取列表里，玩家能领回去 —— 不需要任何补偿事务代码。
        /// 正常路径下 <c>COMMIT</c> 会把它变成正式上架或转给买家，<c>ABORT</c> 会把它删掉。
        /// </para>
        /// </summary>
        ProtoModel.MTSItemProto ParkItem(int sellerId, string sellerName, ProtoModel.ItemProto? item, int tab, int type)
        {
            var model = new ProtoModel.MTSItemProto
            {
                Id = Interlocked.Increment(ref _localId),
                SellerId = sellerId,
                SellerName = sellerName,
                ReleaseDate = NowMs(),
                Tab = tab,
                Type = type,
                InTransfer = true,
            };
            if (item != null)
            {
                model.Item = item;
            }

            SetDirty(model);
            return model;
        }

        ProtoModel.PagedItemsProto LoadCart(int ownerId)
        {
            var cart = GetCart(ownerId);
            var all = OrderForDisplay(LoadLocal()
                .Where(x => !x.InTransfer && cart.Contains(x.Id)))
                .ToList();
            return ToPage(all, 4, 0, 0);
        }

        public ProtoService.ChangePageResponse ChangePage(ProtoService.ChangePageRequest request)
        {
            var all = LoadLocal();
            IEnumerable<ProtoModel.MTSItemProto> query;

            if (request.Tab == MTSConstants.CART_TAB && request.Type == 0)
            {
                var cart = GetCart(request.MasterId);
                query = all.Where(x => !x.InTransfer && cart.Contains(x.Id));
            }
            else if (request.Tab == MTSConstants.MY_WANTED_TAB && request.Type != 0)
            {
                // 客户端 tab 4 在同一窗口里用 type 区分“购物车”和“我的求购”（sub_5BC1D5 的 v4[26]）
                query = all.Where(x => !x.InTransfer && x.Tab == MTSConstants.WANTED_TAB && x.SellerId == request.MasterId);
            }
            else
            {
                query = all.Where(x => !x.InTransfer && x.Tab == request.Tab && (request.Type == 0 || x.Type == request.Type));

                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    if (request.SearchType == 0)
                    {
                        query = query.Where(x => x.SellerName.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (request.FilterItemId.Count > 0)
                    {
                        var filter = request.FilterItemId.ToHashSet();
                        query = query.Where(x => x.Item != null && filter.Contains(x.Item.Itemid));
                    }
                }
            }

            var list = OrderForDisplay(query).ToList();
            var res = new ProtoService.ChangePageResponse
            {
                MasterId = request.MasterId,
                Tab = request.Tab,
                Type = request.Type,
                Page = request.Page,
                TotalPages = (int)Math.Ceiling(list.Count / (double)MTSConstants.PAGE_SIZE),
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
            };
            res.Items.AddRange(list.Skip(request.Page * MTSConstants.PAGE_SIZE).Take(MTSConstants.PAGE_SIZE));
            return res;
        }

        #endregion

        #region 上架 / 下架

        /// <summary>
        /// 上架（op 2）。<c>PREPARE</c> 先把物品暂存进卖家自己的待领取列表，
        /// <c>COMMIT</c> 才让它出现在商品列表里，<c>ABORT</c> 把暂存行删掉。
        /// </summary>
        public ProtoService.SaleItemResponse SaleItem(ProtoService.SaleItemRequest request)
        {
            return request.Stage switch
            {
                ProtoModel.TransferStage.Commit => CommitSaleItem(request),
                ProtoModel.TransferStage.Abort => AbortSaleItem(request),
                _ => PrepareSaleItem(request),
            };
        }

        /// <summary>
        /// 上架 <c>PREPARE</c> 的重放守卫：网络重试会再发一次同一个 token，
        /// 这里必须先认出“已经暂存过了”，否则每重试一次就多留一条暂存行。
        /// </summary>
        ProtoService.SaleItemResponse OkSaleItem(int masterId, int tab, int type) => new()
        {
            Code = (int)MTSResponseCode.Success,
            MasterId = masterId,
            MyMTSInfo = LoadPlayerInfo(masterId),
            PageData = LoadPage(tab, type, 0),
        };

        ProtoService.SaleItemResponse PrepareSaleItem(ProtoService.SaleItemRequest request)
        {
            if (_transfers.Get(request.Token, request.MasterId) != null)
            {
                return OkSaleItem(request.MasterId,
                    request.Tab == 0 ? MTSConstants.SALE_TAB : request.Tab, request.Type);
            }

            if (request.Item == null || request.Price <= 0)
            {
                return FailSaleItem(MTSResponseCode.InvalidRequest, request.MasterId);
            }

            // 暂存行也会挂在卖家名下，所以只数真正在架上的
            var onSale = LoadLocal().Count(x => !x.InTransfer && x.SellerId == request.MasterId);
            if (onSale >= MTSConstants.MAX_SALE_ITEMS)
            {
                return FailSaleItem(MTSResponseCode.LimitReached, request.MasterId);
            }

            var tab = request.Tab == 0 ? MTSConstants.SALE_TAB : request.Tab;
            var parked = ParkItem(request.MasterId, request.SellerName, request.Item, tab, request.Type);

            parked.Price = request.Price;
            parked.SellEnds = (_server.GetCurrentTimeDateTimeOffset() + MTSConstants.SALE_DURATION).ToUnixTimeMilliseconds();
            SetDirty(parked);

            if (_transfers.Prepare(request.Token, request.MasterId, parked.Id, NowMs()) == null)
            {
                SetRemoved(parked);
                return FailSaleItem(MTSResponseCode.InProgress, request.MasterId);
            }

            _transfers.AttachParked(request.Token, request.MasterId, parked.Id);

            return new ProtoService.SaleItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = request.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
                PageData = LoadPage(tab, request.Type, 0),
            };
        }

        ProtoService.SaleItemResponse CommitSaleItem(ProtoService.SaleItemRequest request)
        {
            var entry = _transfers.Get(request.Token, request.MasterId);
            if (entry == null)
            {
                return FailSaleItem(MTSResponseCode.InProgress, request.MasterId);
            }

            var parked = Find(entry.ParkedId);
            if (parked == null)
            {
                // 暂存行没了（被卖家自己从待领取取回）。重放已提交的情况另走下面那条
                _transfers.Drop(request.Token, request.MasterId);
                return FailSaleItem(MTSResponseCode.NotFound, request.MasterId);
            }

            if (entry.Stage != ProtoModel.TransferStage.Commit)
            {
                // 上架时间从真正生效的这一刻算起，而不是从 PREPARE
                parked.InTransfer = false;
                if (parked.Tab == MTSConstants.AUCTION_TAB)
                {
                    var hours = parked.DurationHours <= 0 ? MTSConstants.AUCTION_MIN_HOURS : parked.DurationHours;
                    parked.SellEnds = NowMs() + hours * 3600L * 1000L;
                }
                else
                {
                    parked.SellEnds = (_server.GetCurrentTimeDateTimeOffset() + MTSConstants.SALE_DURATION).ToUnixTimeMilliseconds();
                }
                parked.ReleaseDate = NowMs();
                SetDirty(parked);

                _transfers.Reach(request.Token, request.MasterId, ProtoModel.TransferStage.Commit);
            }

            return new ProtoService.SaleItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = request.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
                PageData = LoadPage(parked.Tab, parked.Type, 0),
            };
        }

        ProtoService.SaleItemResponse AbortSaleItem(ProtoService.SaleItemRequest request)
        {
            var entry = _transfers.Get(request.Token, request.MasterId);
            if (entry != null && entry.Stage != ProtoModel.TransferStage.Commit)
            {
                // 频道侧还在，物品也还/已退回背包，暂存行必须删掉，否则玩家会多出一份
                var parked = Find(entry.ParkedId);
                if (parked != null)
                {
                    SetRemoved(parked);
                }
            }

            _transfers.Drop(request.Token, request.MasterId);

            return new ProtoService.SaleItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = request.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
                PageData = LoadPage(request.Tab == 0 ? MTSConstants.SALE_TAB : request.Tab, request.Type, 0),
            };
        }

        static ProtoService.SaleItemResponse FailSaleItem(MTSResponseCode code, int masterId) => new()
        {
            Code = (int)code,
            MasterId = masterId,
        };

        public ProtoService.CancelSaleItemResponse CancelSaleItem(ProtoService.CancelSaleItemRequest request)
        {
            var product = Find(request.ProductId);
            if (product == null || product.InTransfer)
            {
                return new ProtoService.CancelSaleItemResponse
                {
                    Code = (int)MTSResponseCode.NotFound,
                    MasterId = request.MasterId,
                };
            }

            if (product.SellerId != request.MasterId)
            {
                return new ProtoService.CancelSaleItemResponse
                {
                    Code = (int)MTSResponseCode.NoAccess,
                    MasterId = request.MasterId,
                };
            }

            product.InTransfer = true;
            SetDirty(product);
            GetCart(request.MasterId).Remove(product.Id);

            return new ProtoService.CancelSaleItemResponse
            {
                MasterId = request.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
                PageData = LoadPage(product.Tab, product.Type, 0),
            };
        }

        #endregion

        #region 购物车

        public ProtoService.AddItemToCartResponse AddItemToCart(ProtoService.AddItemToCartRequest request)
        {
            var product = Find(request.ProductId);
            if (product == null || product.InTransfer)
            {
                return new ProtoService.AddItemToCartResponse
                {
                    Code = (int)MTSResponseCode.NotFound,
                    MasterId = request.Query.MasterId,
                };
            }

            GetCart(request.Query.MasterId).Add(product.Id);
            return new ProtoService.AddItemToCartResponse
            {
                MasterId = request.Query.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
            };
        }

        public ProtoService.RemoveItemFromCartResponse RemoveItemFromCart(ProtoService.RemoveItemFromCartRequest request)
        {
            GetCart(request.Query.MasterId).Remove(request.ProductId);
            return new ProtoService.RemoveItemFromCartResponse
            {
                MasterId = request.Query.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                PageData = LoadCart(request.Query.MasterId),
            };
        }

        #endregion

        #region 领取 / 购买

        /// <summary>
        /// 从“待领取”列表取回物品（取消出售、购买后领取）
        /// <para>
        /// 按 <see cref="ProtoModel.TransferStage"/> 分三段：<c>PREPARE</c> 只占位并返回物品
        /// （频道侧要先确认背包放得下），<c>COMMIT</c> 才真正移除，<c>ABORT</c> 释放占位。
        /// 占位是必需的：没有它，同一件商品被并发探两次会各自往背包里塞一份，
        /// 而提交只会成功一次 —— 多出来的那份就是凭空复制的道具。
        /// </para>
        /// </summary>
        public ProtoService.TakeItemResponse TakeItem(ProtoService.TakeItemRequest request)
        {
            return request.Stage switch
            {
                ProtoModel.TransferStage.Commit => CommitTakeItem(request),
                ProtoModel.TransferStage.Abort => AbortTakeItem(request),
                _ => PrepareTakeItem(request),
            };
        }

        ProtoService.TakeItemResponse PrepareTakeItem(ProtoService.TakeItemRequest request)
        {
            var masterId = request.Query.MasterId;

            var product = Find(request.ProductId);
            if (product == null || !product.InTransfer)
            {
                // 已经不在了（被别人领走 / 到期结算 / 取消）
                return FailTakeItem(MTSResponseCode.NotFound, masterId);
            }

            if (product.SellerId != masterId)
            {
                return FailTakeItem(MTSResponseCode.NoAccess, masterId);
            }

            if (_transfers.Prepare(request.Token, masterId, product.Id, NowMs()) == null)
            {
                // 这件商品已经有别的请求在途
                return FailTakeItem(MTSResponseCode.InProgress, masterId);
            }

            return new ProtoService.TakeItemResponse
            {
                MasterId = masterId,
                Item = product.Item,
                MyMTSInfo = LoadPlayerInfo(masterId),
                CartItems = LoadCart(masterId),
                PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
            };
        }

        ProtoService.TakeItemResponse CommitTakeItem(ProtoService.TakeItemRequest request)
        {
            var masterId = request.Query.MasterId;

            var entry = _transfers.Get(request.Token, masterId);
            if (entry == null)
            {
                // 占位已过期或压根没占：让频道侧从头来一次
                return FailTakeItem(MTSResponseCode.InProgress, masterId);
            }

            if (entry.Stage == ProtoModel.TransferStage.Commit)
            {
                // 重放：上一趟其实已经成功了，结论原样返回
                return new ProtoService.TakeItemResponse
                {
                    Code = (int)MTSResponseCode.Success,
                    MasterId = masterId,
                    MyMTSInfo = LoadPlayerInfo(masterId),
                    CartItems = LoadCart(masterId),
                    PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
                };
            }

            var product = Find(entry.ProductId);
            if (product == null || !product.InTransfer)
            {
                _transfers.Drop(request.Token, masterId);
                return FailTakeItem(MTSResponseCode.NotFound, masterId);
            }

            if (product.SellerId != masterId)
            {
                return FailTakeItem(MTSResponseCode.NoAccess, masterId);
            }

            SetRemoved(product);
            _transfers.Reach(request.Token, masterId, ProtoModel.TransferStage.Commit);
            GetCart(masterId).Remove(product.Id);

            return new ProtoService.TakeItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = masterId,
                Item = product.Item,
                MyMTSInfo = LoadPlayerInfo(masterId),
                CartItems = LoadCart(masterId),
                PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
            };
        }

        ProtoService.TakeItemResponse AbortTakeItem(ProtoService.TakeItemRequest request)
        {
            var masterId = request.Query.MasterId;

            // 待领取的那一行本来就是玩家自己的东西，放弃领取不需要动它
            _transfers.Drop(request.Token, masterId);

            return new ProtoService.TakeItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = masterId,
                MyMTSInfo = LoadPlayerInfo(masterId),
                CartItems = LoadCart(masterId),
                PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
            };
        }

        static ProtoService.TakeItemResponse FailTakeItem(MTSResponseCode code, int masterId) => new()
        {
            Code = (int)code,
            MasterId = masterId,
        };

        public ProtoService.BuyResponse Buy(ProtoService.BuyRequest request)
        {
            var product = Find(request.ProductId);
            if (product == null || product.InTransfer)
            {
                return new ProtoService.BuyResponse
                {
                    Code = (int)MTSResponseCode.NotFound,
                    MasterId = request.Query.MasterId,
                };
            }

            if (product.SellerId == request.Query.MasterId)
            {
                return new ProtoService.BuyResponse
                {
                    Code = (int)MTSResponseCode.Buy_CannotBuyOwn,
                    MasterId = request.Query.MasterId,
                };
            }

            var buyer = _server.CharacterManager.FindPlayerById(request.Query.MasterId);
            if (buyer == null)
            {
                return new ProtoService.BuyResponse
                {
                    Code = (int)MTSResponseCode.NotFound,
                    MasterId = request.Query.MasterId,
                };
            }

            var buyerData = _server.AccountGameManager.GetAccountGameData(buyer.Character.AccountId);
            // 卖东西的人可能早就下线了，账号要从数据库里捞
            var sellerAccountId = ResolveAccountId(product.SellerId);
            var sellerData = sellerAccountId == null ? null : _server.AccountGameManager.GetAccountGameData(sellerAccountId.Value);
            if (buyerData == null || sellerData == null)
            {
                return new ProtoService.BuyResponse
                {
                    Code = (int)MTSResponseCode.NotFound,
                    MasterId = request.Query.MasterId,
                };
            }

            // 卖家实得的钱：拍卖按一口价成交（走到这里只有“一口价直接买下”这一种），
            // 普通出售 / 求购按标价
            int salePrice = product.Tab == MTSConstants.AUCTION_TAB && product.BuyNow > 0 ? product.BuyNow : product.Price;
            // 买家实付 = 成交价 + 税费（100 + 成交价/10），拍卖与直售是同一套费率
            int finalCost = MTSConstants.BuyerTotalCost(salePrice);
            if (buyerData.NxPrepaid < finalCost)
            {
                return new ProtoService.BuyResponse
                {
                    Code = (int)MTSResponseCode.Buy_CostNotEnough,
                    MasterId = request.Query.MasterId,
                };
            }

            buyerData.NxPrepaid -= finalCost;
            sellerData.NxPrepaid += salePrice;
            _server.AccountGameManager.UpdateAccountGame(buyerData);
            _server.AccountGameManager.UpdateAccountGame(sellerData);

            // 商品转入买家待领取列表
            product.SellerId = request.Query.MasterId;
            product.SellerName = buyer.Character.Name;
            product.BidderId = 0;
            product.InTransfer = true;
            SetDirty(product);
            GetCart(request.Query.MasterId).Remove(product.Id);

            return new ProtoService.BuyResponse
            {
                MasterId = request.Query.MasterId,
                Item = product.Item,
                UpdatedValue = buyerData.NxPrepaid,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                CartItems = LoadCart(request.Query.MasterId),
                PageData = LoadPage(request.Query.Tab, request.Query.Type, request.Query.Page),
            };
        }

        #endregion

        #region 求购

        long NowMs() => _server.getCurrentTime();

        static ProtoService.MTSOperationResponse OpFail(MTSResponseCode code, int masterId) =>
            new() { Code = (int)code, MasterId = masterId };

        /// <summary>
        /// 解析角色所属账号。中心服的账本（<see cref="AccountGameManager"/>）按账号id索引，
        /// 离线的角色也要能查到，否则拍卖行里对手不在线时就没法结算。
        /// </summary>
        int? ResolveAccountId(int masterId)
        {
            var onlined = _server.CharacterManager.FindPlayerById(masterId);
            if (onlined != null)
                return onlined.Character.AccountId;

            return _server.CharacterManager.GetCharacterDto(masterId)?.AccountId;
        }

        /// <summary>
        /// 角色名，离线时从数据库读
        /// </summary>
        string ResolveName(int masterId)
        {
            var onlined = _server.CharacterManager.FindPlayerById(masterId);
            if (onlined != null)
                return onlined.Character.Name;

            return _server.CharacterManager.GetPlayerName(masterId);
        }

        /// <summary>
        /// 直接增减某个角色的抵用券，返回是否成功及增减后的余额
        /// </summary>
        bool TryChangeNx(int masterId, int delta, out int balance)
        {
            balance = 0;

            var accountId = ResolveAccountId(masterId);
            if (accountId == null)
                return false;

            var data = _server.AccountGameManager.GetAccountGameData(accountId.Value);
            if (data == null || data.NxPrepaid + delta < 0)
                return false;

            data.NxPrepaid += delta;
            _server.AccountGameManager.UpdateAccountGame(data);
            balance = data.NxPrepaid;
            return true;
        }

        /// <summary>
        /// 登记求购（op 4）。求购条目本身没有真实物品，只放一个虚拟物品供客户端画图标。
        /// </summary>
        public ProtoService.MTSOperationResponse RegisterWanted(ProtoService.WantedRequest request)
        {
            if (request.ItemId <= 0 || request.Price <= 0 || request.Quantity <= 0)
            {
                return OpFail(MTSResponseCode.InvalidRequest, request.Query.MasterId);
            }

            var myWanted = LoadLocal().Count(x => !x.InTransfer && x.Tab == MTSConstants.WANTED_TAB && x.SellerId == request.Query.MasterId);
            if (myWanted >= MTSConstants.MAX_SALE_ITEMS)
            {
                return OpFail(MTSResponseCode.LimitReached, request.Query.MasterId);
            }

            var hours = request.DurationHours <= 0 ? (int)MTSConstants.SALE_DURATION.TotalHours : request.DurationHours;
            var model = new ProtoModel.MTSItemProto
            {
                Id = Interlocked.Increment(ref _localId),
                Item = new ProtoModel.ItemProto
                {
                    Itemid = request.ItemId,
                    Quantity = request.Quantity,
                },
                SellerId = request.Query.MasterId,
                SellerName = request.BuyerName ?? string.Empty,
                BuyerName = request.BuyerName ?? string.Empty,
                Price = request.Price,
                ReleaseDate = NowMs(),
                SellEnds = NowMs() + hours * 3600L * 1000L,
                Tab = request.Tab == 0 ? MTSConstants.WANTED_TAB : request.Tab,
                Type = request.Type,
                DurationHours = hours,
                InTransfer = false,
            };
            SetDirty(model);

            return new ProtoService.MTSOperationResponse
            {
                MasterId = request.Query.MasterId,
                Product = model,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                PageData = LoadPage(model.Tab, model.Type, 0),
            };
        }

        /// <summary>
        /// 撤销求购（op 13）。求购单没有实体物品，直接删除。
        /// </summary>
        public ProtoService.MTSOperationResponse CancelWanted(ProtoService.ProductRequest request)
        {
            var product = Find(request.ProductId);
            if (product == null || product.InTransfer || product.Tab != MTSConstants.WANTED_TAB || product.SellerId != request.Query.MasterId)
            {
                return OpFail(MTSResponseCode.NotFound, request.Query.MasterId);
            }

            SetRemoved(product);

            return new ProtoService.MTSOperationResponse
            {
                MasterId = request.Query.MasterId,
                Product = product,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                PageData = LoadPage(MTSConstants.WANTED_TAB, 0, 0),
            };
        }

        /// <summary>
        /// 与求购单成交（op 3 / op 11 / op 12）。
        /// <para>
        /// 求购单的 <c>SellerId</c> 存的是**求购人**（买家）。成交时卖方交付物品、拿走抵用券，
        /// 物品落进买家的“待领取”列表（买家可能不在线，所以不能直接进背包）。
        /// </para>
        /// </summary>
        /// <summary>
        /// 与求购单成交（op 3 / op 12）。卖方交付的物品 <c>PREPARE</c> 时先暂存进卖方自己的
        /// 待领取列表，<c>COMMIT</c> 时才扣款、付款并把那一行转给买家。
        /// </summary>
        public ProtoService.MTSOperationResponse FulfillWanted(ProtoService.FulfillWantedRequest request)
        {
            return request.Stage switch
            {
                ProtoModel.TransferStage.Commit => CommitFulfillWanted(request),
                ProtoModel.TransferStage.Abort => AbortFulfillWanted(request),
                _ => PrepareFulfillWanted(request),
            };
        }

        ProtoService.MTSOperationResponse PrepareFulfillWanted(ProtoService.FulfillWantedRequest request)
        {
            var sellerId = request.Query.MasterId;

            // 重放守卫：同一个 token 再来一次说明上一趟已经暂存过了，不能再 Park 一份
            if (_transfers.Get(request.Token, sellerId) != null)
            {
                return new ProtoService.MTSOperationResponse
                {
                    Code = (int)MTSResponseCode.Success,
                    MasterId = sellerId,
                    MyMTSInfo = LoadPlayerInfo(sellerId),
                    PageData = LoadPage(MTSConstants.WANTED_TAB, 0, 0),
                };
            }

            if (request.Item == null)
                return OpFail(MTSResponseCode.InvalidRequest, sellerId);

            var product = Find(request.ProductId);
            if (product == null || product.InTransfer || product.Tab != MTSConstants.WANTED_TAB)
                return OpFail(MTSResponseCode.NotFound, sellerId);

            // SellerId 是求购人，其余任何人都不能来成交
            if (product.SellerId == sellerId)
                return OpFail(MTSResponseCode.Buy_CannotBuyOwn, sellerId);

            var buyerId = product.SellerId;
            var quantity = product.Item?.Quantity ?? 0;
            if (quantity <= 0)
                return OpFail(MTSResponseCode.InvalidRequest, sellerId);

            // 这里只校验、不扣钱：资金往来放到 COMMIT，免得 PREPARE 之后走 ABORT 还要退款
            if (!TryChangeNx(buyerId, 0, out var buyerBalance) || buyerBalance < product.Price * quantity)
                return OpFail(MTSResponseCode.Buy_CostNotEnough, sellerId);

            // 卖方交付的物品先落进他自己的待领取列表；走不到 COMMIT 时这就是玩家的退路
            var parked = ParkItem(sellerId, ResolveName(sellerId), request.Item, MTSConstants.WANTED_TAB, product.Type);

            if (_transfers.Prepare(request.Token, sellerId, product.Id, NowMs()) == null)
            {
                SetRemoved(parked);
                return OpFail(MTSResponseCode.InProgress, sellerId);
            }
            _transfers.AttachParked(request.Token, sellerId, parked.Id);

            return new ProtoService.MTSOperationResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = sellerId,
                MyMTSInfo = LoadPlayerInfo(sellerId),
                PageData = LoadPage(MTSConstants.WANTED_TAB, 0, 0),
            };
        }

        ProtoService.MTSOperationResponse CommitFulfillWanted(ProtoService.FulfillWantedRequest request)
        {
            var sellerId = request.Query.MasterId;

            var entry = _transfers.Get(request.Token, sellerId);
            if (entry == null)
                return OpFail(MTSResponseCode.InProgress, sellerId);

            if (entry.Stage != ProtoModel.TransferStage.Commit)
            {
                var wanted = Find(entry.ProductId);
                if (wanted == null || wanted.InTransfer || wanted.Tab != MTSConstants.WANTED_TAB)
                {
                    _transfers.Drop(request.Token, sellerId);
                    return OpFail(MTSResponseCode.NotFound, sellerId);
                }

                var parked = Find(entry.ParkedId);
                if (parked == null)
                {
                    // 卖方在中间把暂存的那一行从待领取领回去了
                    _transfers.Drop(request.Token, sellerId);
                    return OpFail(MTSResponseCode.NotFound, sellerId);
                }

                var buyerId = wanted.SellerId;
                var total = wanted.Price * (parked.Item?.Quantity ?? 0);
                if (!TryChangeNx(buyerId, -total, out _))
                    return OpFail(MTSResponseCode.Buy_CostNotEnough, sellerId);

                if (!TryChangeNx(sellerId, total, out _))
                {
                    // 卖方账号读不出来，回滚买家的扣款
                    TryChangeNx(buyerId, total, out _);
                    return OpFail(MTSResponseCode.NotFound, sellerId);
                }

                // 求购单消失，交付的那一行转成买家的待领取
                var salePrice = wanted.Price;
                SetRemoved(wanted);
                parked.SellerId = buyerId;
                parked.Tab = wanted.Tab;
                parked.Type = wanted.Type;
                SetDirty(parked);

                _transfers.Reach(request.Token, sellerId, ProtoModel.TransferStage.Commit);
                _logger.LogInformation("求购成交：卖方 {SellerId} 交付 {Quantity} 件，单价 {Price}，买家 {BuyerId}",
                    sellerId, parked.Item?.Quantity ?? 0, salePrice, buyerId);
            }

            TryChangeNx(sellerId, 0, out var balance);

            return new ProtoService.MTSOperationResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = sellerId,
                Product = Find(entry.ParkedId),
                UpdatedValue = balance,
                MyMTSInfo = LoadPlayerInfo(sellerId),
                PageData = LoadPage(MTSConstants.WANTED_TAB, 0, 0),
            };
        }

        ProtoService.MTSOperationResponse AbortFulfillWanted(ProtoService.FulfillWantedRequest request)
        {
            var sellerId = request.Query.MasterId;

            var entry = _transfers.Get(request.Token, sellerId);
            if (entry != null && entry.Stage != ProtoModel.TransferStage.Commit)
            {
                // 频道侧已把物品退回背包，暂存行必须删掉，否则玩家会多出一份
                var parked = Find(entry.ParkedId);
                if (parked != null)
                {
                    SetRemoved(parked);
                }
            }

            _transfers.Drop(request.Token, sellerId);

            return new ProtoService.MTSOperationResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = sellerId,
                MyMTSInfo = LoadPlayerInfo(sellerId),
                PageData = LoadPage(MTSConstants.WANTED_TAB, 0, 0),
            };
        }

        /// <summary>
        /// 求购单的物品id与数量，供频道侧从背包里找对应物品
        /// </summary>
        public ProtoModel.MTSItemProto? FindWanted(int productId)
        {
            var product = Find(productId);
            return product != null && !product.InTransfer && product.Tab == MTSConstants.WANTED_TAB ? product : null;
        }

        /// <summary>
        /// 只读取求购单（op 11 / op 12 只有商品id，频道侧要先拿到求购的物品）
        /// </summary>
        public ProtoService.MTSOperationResponse LoadWanted(ProtoService.ProductRequest request)
        {
            var product = FindWanted(request.ProductId);
            if (product == null)
                return OpFail(MTSResponseCode.NotFound, request.Query.MasterId);

            return new ProtoService.MTSOperationResponse
            {
                MasterId = request.Query.MasterId,
                Product = product,
            };
        }

        #endregion

        #region 拍卖

        /// <summary>
        /// 拍卖上架（op 18）。
        /// <para>
        /// <c>Price</c> 是起拍价，<c>BuyNow</c> 是卖家另外填的一口价，<c>BidIncrement</c> 是加价幅度。
        /// 客户端 <c>CRegisterAuctionEntryDlg::Confirm</c> 就是让卖家填这几个值，并校验
        /// <c>一口价 &gt;= 起拍价 + 加价幅度</c>（否则弹
        /// <c>THE_BUY_NOW_PRICE_IS_LOWER_THAN_THE_STARTING_BID</c>），服务端同样再校验一次。
        /// </para>
        /// </summary>
        public ProtoService.SaleItemResponse RegisterAuction(ProtoService.SaleItemRequest request)
        {
            return request.Stage switch
            {
                ProtoModel.TransferStage.Commit => CommitSaleItem(request),
                ProtoModel.TransferStage.Abort => AbortSaleItem(request),
                _ => PrepareAuction(request),
            };
        }

        ProtoService.SaleItemResponse PrepareAuction(ProtoService.SaleItemRequest request)
        {
            if (_transfers.Get(request.Token, request.MasterId) != null)
            {
                return OkSaleItem(request.MasterId, MTSConstants.AUCTION_TAB, request.Type);
            }

            // 加价幅度至少 1，否则出价举步维艰
            var increment = request.BidIncrement <= 0 ? 1 : request.BidIncrement;

            if (request.Item == null || request.Price <= 0 || request.BuyNow <= 0 || request.BuyNow < request.Price + increment)
            {
                return FailSaleItem(MTSResponseCode.InvalidRequest, request.MasterId);
            }

            var onSale = LoadLocal().Count(x => !x.InTransfer && x.SellerId == request.MasterId);
            if (onSale >= MTSConstants.MAX_SALE_ITEMS)
            {
                return FailSaleItem(MTSResponseCode.LimitReached, request.MasterId);
            }

            // 客户端 CRegisterAuctionEntryDlg 的可选时长来自 SET_ITC 下发的 24 / 168，这里再夹一次
            var hours = Math.Clamp(request.DurationHours <= 0 ? MTSConstants.AUCTION_MIN_HOURS : request.DurationHours,
                MTSConstants.AUCTION_MIN_HOURS, MTSConstants.AUCTION_MAX_HOURS);

            var parked = ParkItem(request.MasterId, request.SellerName, request.Item, MTSConstants.AUCTION_TAB, request.Type);

            parked.Price = request.Price;
            parked.BuyNow = request.BuyNow;
            parked.BidIncrement = increment;
            parked.BidderId = 0;
            parked.DurationHours = hours;
            parked.SellEnds = NowMs() + hours * 3600L * 1000L;
            SetDirty(parked);

            if (_transfers.Prepare(request.Token, request.MasterId, parked.Id, NowMs()) == null)
            {
                SetRemoved(parked);
                return FailSaleItem(MTSResponseCode.InProgress, request.MasterId);
            }
            _transfers.AttachParked(request.Token, request.MasterId, parked.Id);

            return new ProtoService.SaleItemResponse
            {
                Code = (int)MTSResponseCode.Success,
                MasterId = request.MasterId,
                MyMTSInfo = LoadPlayerInfo(request.MasterId),
                PageData = LoadPage(MTSConstants.AUCTION_TAB, request.Type, 0),
            };
        }

        /// <summary>
        /// 拍卖出价（op 19）。
        /// <para>
        /// 出价只做“承诺”，不在此时扣款 —— 出价人可能被更高价顶掉，而退款要碰对方账号，
        /// 对方离线时容易出错。真正的扣款发生在按一口价成交（op 20）或者到期结算的时候。
        /// </para>
        /// </summary>
        public ProtoService.MTSOperationResponse BidAuction(ProtoService.BidRequest request)
        {
            var product = Find(request.ProductId);
            if (product == null || product.InTransfer || product.Tab != MTSConstants.AUCTION_TAB)
                return OpFail(MTSResponseCode.NotFound, request.Query.MasterId);

            if (product.SellerId == request.Query.MasterId)
                return OpFail(MTSResponseCode.Buy_CannotBuyOwn, request.Query.MasterId);

            // 客户端 'YOU_CANNOT_MAKE_A_CONSECUTIVE_BID'（op 60）对应的规则
            if (product.BidderId == request.Query.MasterId)
                return OpFail(MTSResponseCode.Bid_Consecutive, request.Query.MasterId);

            // 出价下限是“当前价 + 加价幅度”：客户端出价对话框就是拿条目 +0x10（当前价）
            // 和 +0x0E（加价幅度）初始化的，上下箭头也按加价幅度步进（sub_5C28F9 / sub_5C373E）。
            var minBid = product.Price + (product.BidIncrement <= 0 ? 1 : product.BidIncrement);
            if (request.Bid < minBid)
                return OpFail(MTSResponseCode.Bid_TooLow, request.Query.MasterId);

            // 够到一口价时客户端不再发 op 19，而是弹“是否直接买下”（sub_5C373E 的 YesNo 分支）。
            // 真收到这种包说明客户端被改过，拒绝掉，避免同一笔交易出现两条入账路径。
            if (product.BuyNow > 0 && request.Bid >= product.BuyNow)
                return OpFail(MTSResponseCode.InvalidRequest, request.Query.MasterId);

            product.Price = request.Bid;
            product.BidderId = request.Query.MasterId;
            // 出价人数：客户端列表里这一项是按 WORD 读的（entry+0x0C），这里只累计成功出价的次数
            product.BidCount++;
            SetDirty(product);

            return new ProtoService.MTSOperationResponse
            {
                MasterId = request.Query.MasterId,
                Product = product,
                MyMTSInfo = LoadPlayerInfo(request.Query.MasterId),
                PageData = LoadPage(MTSConstants.AUCTION_TAB, product.Type, 0),
            };
        }

        #endregion
    }
}
