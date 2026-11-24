using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net.Packets;
using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.Mappers;
using Application.Resources.Messages;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 拍卖行（MTS / ITC）的频道侧逻辑
    /// <para>商品数据由中心服务器的 MTSManager 保存，这里只负责背包操作与封包</para>
    /// </summary>
    public class MTSManager
    {
        readonly ILogger<MTSManager> _logger;
        readonly WorldChannelServer _server;
        /// <summary>
        /// 物品转移的统一调度（占位 → 本地背包 → 提交，失败回滚），四个业务共用
        /// </summary>
        readonly ItemTransferRunner _transferRunner;

        public MTSManager(WorldChannelServer server, ILogger<MTSManager> logger)
        {
            _server = server;
            _logger = logger;
            _transferRunner = new ItemTransferRunner(logger);
        }

        ProtoModel.MTSQueryProto GetQuery(Player chr)
        {
            return new ProtoModel.MTSQueryProto
            {
                MasterId = chr.Id,
                Tab = chr.getCurrentTab(),
                Type = chr.getCurrentType(),
                Page = chr.getCurrentPage(),
            };
        }

        async Task SendCash(Player chr)
        {
            await chr.SendPacket(MTSPacketCreator.ShowMTSCash(chr.getCashShop().NxPrepaid, chr.getCashShop().MaplePoint));
        }

        /// <summary>
        /// 发送一页商品 + 我的上架列表 + 待领取列表
        /// </summary>
        async Task SendPageData(Player chr, ProtoModel.PagedItemsProto page, ProtoModel.PlayerMTSInfoProto myInfo)
        {
            await chr.SendPacket(MTSPacketCreator.SendMTS([.. page.Items], page.Tab, page.Type, page.Page, page.TotalPages));
            await chr.SendPacket(MTSPacketCreator.TransferInventory([.. myInfo.InTransfer]));
            await chr.SendPacket(MTSPacketCreator.NotYetSoldInv([.. myInfo.OnSale]));
        }

        #region 查询

        /// <summary>
        /// 查询某一页。搜索条件沿用玩家身上已保存的状态（由 op 6 设置）。
        /// </summary>
        public Task Query(Player chr, int tab, int type, int page)
        {
            return Query(chr, tab, type, page, chr.getCurrentCI(), chr.getSearch());
        }

        /// <summary>
        /// 查询某一页，并带上客户端本次的搜索状态。
        /// <para>
        /// 客户端 op 5（翻页 / 切换类型）会把当前搜索条件一起发上来；切换 tab / type 时
        /// 客户端会重置为空，这里同样清掉，保证两端状态一致。
        /// </para>
        /// </summary>
        public async Task Query(Player chr, int tab, int type, int page, int searchType, string? searchText)
        {
            if (tab != chr.getCurrentTab() || type != chr.getCurrentType())
            {
                chr.setSearch(null);
                searchText = null;
            }

            chr.changeCI(searchType);
            if (!string.IsNullOrEmpty(searchText))
            {
                chr.setSearch(searchText);
            }

            var request = new ProtoService.ChangePageRequest
            {
                MasterId = chr.Id,
                Tab = tab,
                Type = type,
                Page = page,
                SearchType = chr.getCurrentCI(),
            };
            var search = chr.getSearch();
            if (request.SearchType != 0 && !string.IsNullOrEmpty(search))
            {
                request.SearchText = search;
                request.FilterItemId.AddRange(ItemInformationProvider.getInstance().SearchItemIdsByName(search));
            }

            var res = await _server.Transport.ChangeMTSPage(request);

            chr.changeTab(tab);
            chr.changeType(type);
            chr.changePage(page);

            var pageData = new ProtoModel.PagedItemsProto
            {
                Tab = res.Tab,
                Type = res.Type,
                Page = res.Page,
                TotalPages = res.TotalPages,
            };
            pageData.Items.AddRange(res.Items);

            await SendPageData(chr, pageData, res.MyMTSInfo);
            await chr.enableCSActions();
            await SendCash(chr);
        }

        public async Task Search(Player chr, int tab, int type, int searchType, string? searchText)
        {
            chr.changeCI(searchType);
            chr.setSearch(searchText);

            await Query(chr, tab, type, 0);
        }

        #endregion

        #region 上架 / 下架

        /// <summary>
        /// 把背包里指定格子的物品上架
        /// </summary>
        public async Task SaleItem(IChannelClient c, InventoryType invType, short slot, int price)
        {
            var chr = c.OnlinedCharacter;
            var inv = chr.getInventory(invType);
            var item = inv.getItem(slot);
            if (item == null || price <= 0)
            {
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (item.isUntradeable() || ItemInformationProvider.getInstance().isUnmerchable(item.getItemId()))
            {
                // 飞镖/子弹等不可交易的消耗品，客户端有专门文案（'R'），其余用 'O'
                await chr.SendPacket(MTSPacketCreator.MTSFailError(ItemConstants.isRechargeable(item.getItemId())
                    ? MTSClientErrorCode.ThrowingStarsNotTradable
                    : MTSClientErrorCode.ItemMayNotBeSold));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.getLevel() < 10)
            {
                // 客户端规则：10 级以上才能在拍卖行出售（错误码 'S'）
                await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.LevelTooLow));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.getMeso() < MTSConstants.SALE_FEE)
            {
                await chr.Pink(ClientMessage.MTS_NotEnoughMeso, MTSConstants.SALE_FEE);
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            var toSale = item.copy();
            var itemProto = _server.Mapper.Map<ProtoModel.ItemProto>(toSale);
            var quantity = item.getQuantity();
            ProtoService.SaleItemResponse? last = null;

            var outcome = await _transferRunner.RunAsync(new ItemTransfer
            {
                // 中心服先按这个价格把物品暂存进我的待领取列表（市场上还看不到）
                Prepare = async token =>
                {
                    var res = await _server.Transport.SaleMTSItem(new ProtoService.SaleItemRequest
                    {
                        MasterId = chr.Id,
                        Item = itemProto,
                        Price = price,
                        SellerName = c.AccountName,
                        Tab = MTSConstants.SALE_TAB,
                        Type = 0,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Prepare,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                ApplyLocal = async () =>
                {
                    await InventoryManipulator.removeFromSlot(c, invType, slot, quantity, true, false);
                    return true;
                },
                Commit = async token =>
                {
                    var res = await _server.Transport.SaleMTSItem(new ProtoService.SaleItemRequest
                    {
                        MasterId = chr.Id,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Commit,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                Abort = token => _server.Transport.SaleMTSItem(new ProtoService.SaleItemRequest
                {
                    MasterId = chr.Id,
                    Token = token,
                    Stage = ProtoModel.TransferStage.Abort,
                }),
                RollbackLocal = async () =>
                {
                    // 退回背包；万一背包已满，物品还在中心服的暂存行里，玩家能从待领取取回
                    await InventoryManipulator.addFromDrop(c, toSale, false);
                },
            });

            if (outcome == TransferOutcome.Done)
            {
                await chr.SendPacket(MTSPacketCreator.MTSConfirmSell());
            }
            else if (last?.Code == (int)MTSResponseCode.LimitReached)
            {
                // 客户端没有“上架数量超限”的专用包，这里用提示框说明
                await chr.Pink(ClientMessage.MTS_ListingLimit, MTSConstants.MAX_SALE_ITEMS);
            }

            if (last != null)
            {
                await SendPageData(chr, last.PageData, last.MyMTSInfo);
            }
            await chr.enableCSActions();
            await SendCash(chr);
        }

        public async Task CancelSaleItem(Player chr, int productId)
        {
            var res = await _server.Transport.CancelMTSSale(new ProtoService.CancelSaleItemRequest
            {
                MasterId = chr.Id,
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                // 商品已售出/已下架，无法再取消（客户端 'Q'）
                await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.AlreadySoldCannotCancel));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            // op 37：取消出售成功
            await chr.SendPacket(MTSPacketCreator.MTSCancelSell());
            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        #endregion

        #region 购物车

        public async Task AddCartItem(Player chr, int productId)
        {
            var res = await _server.Transport.AddMTSCartItem(new ProtoService.AddItemToCartRequest
            {
                Query = GetQuery(chr),
                ProductId = productId,
            });

            // op 41 / op 42：加入购物车的结果
            await chr.SendPacket(MTSPacketCreator.MTSCartAdded(res.Code == (int)MTSResponseCode.Success));
            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        public async Task RemoveCartItem(Player chr, int productId)
        {
            var query = GetQuery(chr);
            query.Tab = MTSConstants.CART_TAB;
            query.Type = 0;
            query.Page = 0;

            var res = await _server.Transport.RemoveMTSCartItem(new ProtoService.RemoveItemFromCartRequest
            {
                Query = query,
                ProductId = productId,
            });

            // op 43 / op 44：移出购物车的结果
            await chr.SendPacket(MTSPacketCreator.MTSCartRemoved(res.Code == (int)MTSResponseCode.Success));
            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        #endregion

        #region 领取 / 购买

        /// <summary>
        /// 从“待领取”列表把物品放回背包。
        /// <para>
        /// 走 <see cref="ItemTransferRunner"/> 的统一流程：中心服占位 → 物品进背包 → 提交。
        /// 提交失败时回滚背包、撤销占位；背包满时撤销占位让商品留在列表里，不丢东西。
        /// </para>
        /// </summary>
        public async Task TakeItem(IChannelClient c, int productId)
        {
            var chr = c.OnlinedCharacter;

            Item? item = null;
            short pos = -1;
            var inventoryFull = false;
            ProtoService.TakeItemResponse? committed = null;

            var outcome = await _transferRunner.RunAsync(new ItemTransfer
            {
                // 只校验归属并占位，把商品信息带回来（此时中心服不会移除它）
                Prepare = async token =>
                {
                    var peek = await _server.Transport.TakeMTSItem(new ProtoService.TakeItemRequest
                    {
                        Query = GetQuery(chr),
                        ProductId = productId,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Prepare,
                    });
                    if (peek.Code != (int)MTSResponseCode.Success || peek.Item == null)
                    {
                        return false;
                    }

                    item = ProtoMapper.MapItem(peek.Item);
                    return item != null;
                },
                ApplyLocal = async () =>
                {
                    var (added, slot) = await AddToInventory(c, item!);
                    inventoryFull = !added;
                    pos = slot;
                    return added;
                },
                Commit = async token =>
                {
                    var res = await _server.Transport.TakeMTSItem(new ProtoService.TakeItemRequest
                    {
                        Query = GetQuery(chr),
                        ProductId = productId,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Commit,
                    });
                    committed = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                Abort = token => _server.Transport.TakeMTSItem(new ProtoService.TakeItemRequest
                {
                    Query = GetQuery(chr),
                    ProductId = productId,
                    Token = token,
                    Stage = ProtoModel.TransferStage.Abort,
                }),
                RollbackLocal = () => RollbackAddedItem(c, item!, pos),
            });

            if (outcome == TransferOutcome.Done)
            {
                // op 39: int 背包页(1=装备..5=现金) + int 格子
                await chr.SendPacket(MTSPacketCreator.MTSConfirmTransfer((int)item!.getInventoryType(), pos));
                if (committed != null)
                {
                    await SendPageData(chr, committed.PageData, committed.MyMTSInfo);
                }
                await chr.enableCSActions();
                return;
            }

            await chr.SendPacket(inventoryFull
                ? MTSPacketCreator.MTSFailError(MTSClientErrorCode.InventoryFull)
                : MTSPacketCreator.MTSFailBuy());
            await chr.SendPacket(PacketCreator.enableActions());
        }

        /// <summary>
        /// 把物品放进背包，并返回它真正落到的格子（客户端 op 39 要选中这一格）
        /// </summary>
        async Task<(bool Added, short Slot)> AddToInventory(IChannelClient c, Item item)
        {
            if (!await InventoryManipulator.addFromDrop(c, item, true))
            {
                return (false, -1);
            }

            // addFromDrop 落到新格子时会把 position 写回传入的 item（Inventory.AddItem → SetItemPosition）；
            // 可叠加物品并进已有格时存的是另一个实例，item 上的 position 还是上架时的旧值，
            // 这时按背包里的实际情况找
            var pos = item.getPosition();
            var inv = c.OnlinedCharacter.getInventory(item.getInventoryType());
            if (pos <= 0 || !ReferenceEquals(inv.getItem(pos), item))
            {
                pos = inv.listById(item.getItemId()).LastOrDefault()?.getPosition() ?? -1;
            }

            return (true, pos);
        }

        /// <summary>
        /// 回滚 <see cref="AddToInventory"/>：把刚放进去的那一份取出来
        /// </summary>
        async Task RollbackAddedItem(IChannelClient c, Item item, short slot)
        {
            if (slot <= 0)
            {
                _logger.LogError("拍卖行物品 {ItemId} 找不到落点，无法回滚，玩家 {CharacterName}", item.getItemId(), c.OnlinedCharacter.getName());
                return;
            }

            await InventoryManipulator.removeFromSlot(c, item.getInventoryType(), slot, item.getQuantity(), false, false);
        }

        public async Task Buy(IChannelClient c, int productId, bool fromCart)
        {
            var chr = c.OnlinedCharacter;
            var res = await _server.Transport.BuyMTSItem(new ProtoService.BuyRequest
            {
                Query = GetQuery(chr),
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                // NX 不足客户端有专门文案（'B'），其余走通用的购买失败
                if (res.Code == (int)MTSResponseCode.Buy_CostNotEnough)
                {
                    await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.NotEnoughNX));
                }
                else
                {
                    await chr.SendPacket(MTSPacketCreator.MTSFailBuy());
                }
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            chr.getCashShop().NxPrepaid = res.UpdatedValue;

            var page = fromCart ? res.CartItems : res.PageData;
            await SendPageData(chr, page, res.MyMTSInfo);
            await chr.SendPacket(MTSPacketCreator.MTSConfirmBuy());
            await SendCash(chr);
            await chr.SendPacket(PacketCreator.enableActions());
        }

        #endregion

        #region 求购

        /// <summary>
        /// 登记求购（op 4）。求购单本身没有实体物品，只登记意愿，成交时才真正交货。
        /// </summary>
        public async Task RegisterWanted(Player chr, int itemId, int price, int quantity, int durationHours)
        {
            if (itemId <= 0 || price <= 0 || quantity <= 0)
            {
                await chr.SendPacket(MTSPacketCreator.MTSWantedRegistered(false));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            var res = await _server.Transport.RegisterMTSWanted(new ProtoService.WantedRequest
            {
                Query = GetQuery(chr),
                ItemId = itemId,
                Price = price,
                Quantity = quantity,
                Tab = MTSConstants.WANTED_TAB,
                Type = 0,
                BuyerName = chr.getName(),
                DurationHours = durationHours,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                if (res.Code == (int)MTSResponseCode.LimitReached)
                {
                    await chr.Pink(ClientMessage.MTS_ListingLimit, MTSConstants.MAX_SALE_ITEMS);
                }
                await chr.SendPacket(MTSPacketCreator.MTSWantedRegistered(false));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            await chr.SendPacket(MTSPacketCreator.MTSWantedRegistered(true));
            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        /// <summary>
        /// 撤销求购（op 13）
        /// </summary>
        public async Task CancelWanted(Player chr, int productId)
        {
            var res = await _server.Transport.CancelMTSWanted(new ProtoService.ProductRequest
            {
                Query = GetQuery(chr),
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await chr.SendPacket(MTSPacketCreator.MTSWantedCanceled(false));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            await chr.SendPacket(MTSPacketCreator.MTSWantedCanceled(true));
            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        /// <summary>
        /// 与求购单成交（op 3 / op 11 / op 12）。
        /// <para>
        /// 客户端只带商品id（op 3 还额外带了一份物品编码），服务端统一按求购单的物品id
        /// 从背包里取货，避免依赖 <c>GW_ItemSlotBase</c> 的可变长度编码。
        /// </para>
        /// </summary>
        /// <param name="fromOffer">true = op 3 路径（结果走 op 31/32），false = op 11/12 路径（走 op 47/48）</param>
        public async Task FulfillWanted(IChannelClient c, int productId, bool fromOffer)
        {
            var chr = c.OnlinedCharacter;

            var peek = await _server.Transport.LoadMTSWanted(new ProtoService.ProductRequest
            {
                Query = GetQuery(chr),
                ProductId = productId,
            });

            var wanted = peek.Product?.Item;
            if (peek.Code != (int)MTSResponseCode.Success || wanted == null || wanted.Quantity <= 0)
            {
                await chr.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, false));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            var invType = ItemConstants.getInventoryType(wanted.Itemid);
            var owned = chr.getInventory(invType).listById(wanted.Itemid).FirstOrDefault();
            if (owned == null || owned.getQuantity() < wanted.Quantity)
            {
                await chr.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, false));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            var toDeliver = owned.copy();
            toDeliver.setQuantity((short)wanted.Quantity);
            var wantedQuantity = wanted.Quantity;
            var itemProto = _server.Mapper.Map<ProtoModel.ItemProto>(toDeliver);
            var invTypeItemId = wanted.Itemid;
            ProtoService.MTSOperationResponse? last = null;

            var outcome = await _transferRunner.RunAsync(new ItemTransfer
            {
                // 中心服校验求购单与求购人余额，并把交付的物品先暂存进自己的待领取列表
                Prepare = async token =>
                {
                    var res = await _server.Transport.FulfillMTSWanted(new ProtoService.FulfillWantedRequest
                    {
                        Query = GetQuery(chr),
                        ProductId = productId,
                        Item = itemProto,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Prepare,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                ApplyLocal = async () =>
                {
                    await InventoryManipulator.removeById(c, invType, invTypeItemId, wantedQuantity, true, false);
                    return true;
                },
                Commit = async token =>
                {
                    var res = await _server.Transport.FulfillMTSWanted(new ProtoService.FulfillWantedRequest
                    {
                        Query = GetQuery(chr),
                        ProductId = productId,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Commit,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                Abort = token => _server.Transport.FulfillMTSWanted(new ProtoService.FulfillWantedRequest
                {
                    Query = GetQuery(chr),
                    ProductId = productId,
                    Token = token,
                    Stage = ProtoModel.TransferStage.Abort,
                }),
                RollbackLocal = async () =>
                {
                    await InventoryManipulator.addFromDrop(c, toDeliver, false);
                },
            });

            if (outcome == TransferOutcome.Done)
            {
                if (last != null)
                {
                    chr.getCashShop().NxPrepaid = last.UpdatedValue;
                }
                await chr.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, true));
                if (last != null)
                {
                    await SendPageData(chr, last.PageData, last.MyMTSInfo);
                }
                await SendCash(chr);
                await chr.enableCSActions();
                return;
            }

            if (last?.Code == (int)MTSResponseCode.Buy_CostNotEnough)
            {
                // 求购人抵用券不足，只有服务端知道，客户端统一显示“成交失败”
                await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.NotEnoughNX));
            }
            await chr.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, false));
            await chr.SendPacket(PacketCreator.enableActions());
        }

        #endregion

        #region 拍卖

        /// <summary>
        /// 拍卖上架（op 18）。
        /// <para>
        /// 卖家在 <c>CRegisterAuctionEntryDlg</c> 里填的是三个彼此独立的价：
        /// 起拍价 <paramref name="reservePrice"/>、一口价 <paramref name="buyNow"/>、
        /// 加价幅度 <paramref name="bidIncrement"/>，外加时长 <paramref name="durationHours"/>。
        /// 一口价不是服务端算出来的，客户端也会校验 <c>一口价 &gt;= 起拍价 + 加价幅度</c>。
        /// </para>
        /// </summary>
        public async Task RegisterAuction(IChannelClient c, InventoryType invType, short slot, int reservePrice, int buyNow, int bidIncrement, int durationHours)
        {
            var chr = c.OnlinedCharacter;
            var item = chr.getInventory(invType).getItem(slot);
            if (item == null || reservePrice <= 0 || buyNow <= 0)
            {
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (item.isUntradeable() || ItemInformationProvider.getInstance().isUnmerchable(item.getItemId()))
            {
                await chr.SendPacket(MTSPacketCreator.MTSFailError(ItemConstants.isRechargeable(item.getItemId())
                    ? MTSClientErrorCode.ThrowingStarsNotTradable
                    : MTSClientErrorCode.ItemMayNotBeSold));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.getLevel() < 10)
            {
                await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.LevelTooLow));
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.getMeso() < MTSConstants.SALE_FEE)
            {
                await chr.Pink(ClientMessage.MTS_NotEnoughMeso, MTSConstants.SALE_FEE);
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            var toSale = item.copy();
            var itemProto = _server.Mapper.Map<ProtoModel.ItemProto>(toSale);
            var quantity = item.getQuantity();
            ProtoService.SaleItemResponse? last = null;

            var outcome = await _transferRunner.RunAsync(new ItemTransfer
            {
                Prepare = async token =>
                {
                    var res = await _server.Transport.RegisterMTSAuction(new ProtoService.SaleItemRequest
                    {
                        MasterId = chr.Id,
                        Item = itemProto,
                        Price = reservePrice,
                        SellerName = c.AccountName,
                        Tab = MTSConstants.AUCTION_TAB,
                        Type = 0,
                        DurationHours = durationHours,
                        BuyNow = buyNow,
                        BidIncrement = bidIncrement,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Prepare,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                ApplyLocal = async () =>
                {
                    await InventoryManipulator.removeFromSlot(c, invType, slot, quantity, true, false);
                    return true;
                },
                Commit = async token =>
                {
                    var res = await _server.Transport.RegisterMTSAuction(new ProtoService.SaleItemRequest
                    {
                        MasterId = chr.Id,
                        Token = token,
                        Stage = ProtoModel.TransferStage.Commit,
                    });
                    last = res;
                    return res.Code == (int)MTSResponseCode.Success;
                },
                Abort = token => _server.Transport.RegisterMTSAuction(new ProtoService.SaleItemRequest
                {
                    MasterId = chr.Id,
                    Token = token,
                    Stage = ProtoModel.TransferStage.Abort,
                }),
                RollbackLocal = async () =>
                {
                    await InventoryManipulator.addFromDrop(c, toSale, false);
                },
            });

            if (outcome == TransferOutcome.Done)
            {
                await chr.SendPacket(MTSPacketCreator.MTSConfirmSell());
            }
            else if (last?.Code == (int)MTSResponseCode.LimitReached)
            {
                await chr.Pink(ClientMessage.MTS_ListingLimit, MTSConstants.MAX_SALE_ITEMS);
            }

            if (last != null)
            {
                await SendPageData(chr, last.PageData, last.MyMTSInfo);
            }
            await chr.enableCSActions();
            await SendCash(chr);
        }

        /// <summary>
        /// 拍卖出价（op 19）。
        /// <para>
        /// 出价本身不扣抵用券，成功后刷新列表即可让客户端看到新的当前价；
        /// 真正的扣款发生在按即买价成交（op 20）时。
        /// </para>
        /// </summary>
        public async Task BidAuction(Player chr, int productId, int bid)
        {
            var res = await _server.Transport.BidMTSAuction(new ProtoService.BidRequest
            {
                Query = GetQuery(chr),
                ProductId = productId,
                Bid = bid,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                switch (res.Code)
                {
                    case (int)MTSResponseCode.Bid_Consecutive:
                        // 客户端有专门文案：不能连续出价
                        await chr.SendPacket(MTSPacketCreator.MTSConsecutiveBid());
                        break;
                    default:
                        await chr.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.Unknown));
                        break;
                }
                await chr.SendPacket(PacketCreator.enableActions());
                return;
            }

            await SendPageData(chr, res.PageData, res.MyMTSInfo);
            await chr.enableCSActions();
        }

        #endregion
    }
}
