using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net.Packets;
using Application.Core.Client;
using Application.Core.Client.inventory;
using Application.Core.Game.Trades;
using Application.Core.Mappers;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using client.inventory;
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
        public MTSManager(WorldChannelServer server, ILogger<MTSManager> logger)
        {
            _server = server;
            _logger = logger;
        }

        /// <summary>
        /// 是否在拍卖行里。会话状态统一记在 <see cref="IChannelClient.TradeServer"/> 上，
        /// 进出时由 <see cref="Handlers.EnterMTSHandler"/> / <see cref="ChannelClient.LeaveTradeServer"/> 维护。
        /// </summary>
        public bool IsOpened(IChannelClient c) => c.TradeServer is MtsServer;

        /// <summary>
        /// 刷新当前页（同时把现金余额带上）
        /// </summary>
        public Task RefreshPage(IChannelClient c)
        {
            var data = GetServer(c);
            return data == null
                ? Task.CompletedTask
                : Query(c, data.CurrentTab, data.CurrentType, data.CurrentPage);
        }

        /// <summary>
        /// MTS 的浏览状态挂在客户端的 <see cref="MtsServer"/> 上（进入拍卖行时创建）
        /// </summary>
        static MtsServer? GetServer(IChannelClient c) => c.TradeServer as MtsServer;

        ProtoModel.MTSQueryProto GetQuery(IChannelClient c)
        {
            var data = GetServer(c);
            return new ProtoModel.MTSQueryProto
            {
                MasterId = c.CharacterId,
                Tab = data?.CurrentTab ?? MTSConstants.SALE_TAB,
                Type = data?.CurrentType ?? 0,
                Page = data?.CurrentPage ?? 0,
            };
        }

        async Task SendCash(IChannelClient c, ProtoModel.PlayerCashProto? cash)
        {
            await c.SendPacket(MTSPacketCreator.ShowMTSCash(cash?.NxPrepaid ?? 0, cash?.MaplePoint ?? 0));
        }

        /// <summary>
        /// 只刷新拍卖行余额（op <see cref="MTSPacketCreator.ShowMTSCash"/>）。
        /// 客户端 <c>0xFC</c>（拍卖行窗口的「查询」按钮）只要求这个，不涉及列表。
        /// 余额不在频道缓存，现查中心服。
        /// </summary>
        public async Task ShowCash(IChannelClient c)
        {
            var state = _server.Transport.GetCashShopState(new ProtoService.CashShopStateRequest { MasterId = c.CharacterId });
            await SendCash(c, state.Cash);
        }

        /// <summary>
        /// 发送一页商品 + 我的上架列表 + 待领取列表
        /// </summary>
        async Task SendPageData(IChannelClient c, ProtoModel.PagedItemsProto page, ProtoModel.PlayerMTSInfoProto myInfo)
        {
            await c.SendPacket(MTSPacketCreator.SendMTS([.. page.Items], page.Tab, page.Type, page.Page, page.TotalPages));
            await c.SendPacket(MTSPacketCreator.TransferInventory([.. myInfo.InTransfer]));
            await c.SendPacket(MTSPacketCreator.NotYetSoldInv([.. myInfo.OnSale]));
        }

        /// <summary>
        /// 界面可用状态 + 余额。拍卖行只认 <see cref="MTSPacketCreator.ShowMTSCash"/>（op MTS_OPERATION2），
        /// 客户端的商城余额窗口（<c>QUERY_CASH_RESULT</c>）在这里不起作用。
        /// 余额随 <see cref="ProtoModel.PlayerMTSInfoProto"/> 一起下发，不需要再查中心服。
        /// </summary>
        async Task EnableActions(IChannelClient c, ProtoModel.PlayerMTSInfoProto? info)
        {
            await SendCash(c, info?.Cash);
        }

        #region 查询

        /// <summary>
        /// 查询某一页，沿用本会话上次的搜索条件。
        /// </summary>
        public Task Query(IChannelClient c, int tab, int type, int page)
        {
            var data = GetServer(c);
            return data == null
                ? Task.CompletedTask
                : Query(c, tab, type, page, data.CurrentSearchType, data.CurrentSearch);
        }

        /// <summary>
        /// 查询某一页，并带上客户端本次的搜索状态。
        /// <para>
        /// 客户端 op 5（翻页 / 切换类型）会把当前搜索条件一起发上来；切换 tab / type 时
        /// 客户端会重置为空，这里同样清掉，保证两端状态一致。
        /// </para>
        /// </summary>
        public async Task Query(IChannelClient c, int tab, int type, int page, int searchType, string? searchText)
        {
            if (c.TradeServer is not MtsServer data)
            {
                return;
            }
            if (tab != data.CurrentTab || type != data.CurrentType)
            {
                searchType = 0;
                searchText = null;
            }

            var request = new ProtoService.ChangePageRequest
            {
                MasterId = c.CharacterId,
                Tab = tab,
                Type = type,
                Page = page,
                SearchType = searchType,
            };
            if (searchType != 0 && !string.IsNullOrEmpty(searchText))
            {
                request.SearchText = searchText;
                request.FilterItemId.AddRange(ItemInformationProvider.getInstance().SearchItemIdsByName(searchText));
            }

            var res = await _server.Transport.ChangeMTSPage(request);

            data.ChangeTab(tab);
            data.ChangeType(type);
            data.ChangePage(page);
            data.ChangeSearch(searchType, searchText);

            var pageData = new ProtoModel.PagedItemsProto
            {
                Tab = res.Tab,
                Type = res.Type,
                Page = res.Page,
                TotalPages = res.TotalPages,
            };
            pageData.Items.AddRange(res.Items);

            await SendPageData(c, pageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        public Task Search(IChannelClient c, int tab, int type, int searchType, string? searchText)
        {
            return Query(c, tab, type, 0, searchType, searchText);
        }

        #endregion

        #region 上架 / 下架

        /// <summary>
        /// 转发上架请求。校验（等级 / 手续费 / 是否可交易）和扣背包都在中心服，
        /// 这里只把「哪一格、卖多少钱」送过去，再按结果发包。
        /// </summary>
        public async Task SaleItem(IChannelClient c, InventoryType invType, short slot, int price)
        {
            if (price <= 0)
            {
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            var res = await _server.Transport.SaleMTSItem(new ProtoService.SaleItemRequest
            {
                MasterId = c.CharacterId,
                Price = price,
                SellerName = c.AccountName,
                Tab = MTSConstants.SALE_TAB,
                Type = 0,
                InventoryType = (int)invType,
                Slot = slot,
            });

            await OnListingResult(c, res.Code, invType, slot);
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        /// <summary>
        /// 上架 / 拍卖上架的结果：成功就告诉客户端那一格空了，失败按中心服给的原因提示
        /// </summary>
        async Task OnListingResult(IChannelClient c, int code, InventoryType invType, short slot)
        {
            switch ((MTSResponseCode)code)
            {
                case MTSResponseCode.Success:
                    await c.SendPacket(PacketCreator.InventoryOperation(true, [new InventoryRemove(invType, slot)]));
                    await c.SendPacket(MTSPacketCreator.MTSConfirmSell());
                    break;
                case MTSResponseCode.LimitReached:
                    await c.SendPacket(MTSPacketCreator.MTSOperationResult(MTSOperationCode.Unknown));
                    break;
                case MTSResponseCode.LevelTooLow:
                    await c.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.LevelTooLow));
                    break;
                case MTSResponseCode.ItemNotTradable:
                    await c.SendPacket(MTSPacketCreator.MTSOperationResult(MTSOperationCode.ThrowingStarsNotTradable));
                    break;
                case MTSResponseCode.NotEnoughMeso:
                    await c.SendPacket(MTSPacketCreator.MTSOperationResult(MTSOperationCode.NotEnoughMeso));
                    break;
            }
        }

        public async Task CancelSaleItem(IChannelClient c, int productId)
        {
            var res = await _server.Transport.CancelMTSSale(new ProtoService.CancelSaleItemRequest
            {
                MasterId = c.CharacterId,
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                // 商品已售出/已下架，无法再取消（客户端 'Q'）
                await c.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.AlreadySoldCannotCancel));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            // op 37：取消出售成功
            await c.SendPacket(MTSPacketCreator.MTSCancelSell());
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        #endregion

        #region 购物车

        public async Task AddCartItem(IChannelClient c, int productId)
        {
            var res = await _server.Transport.AddMTSCartItem(new ProtoService.AddItemToCartRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
            });

            // op 41 / op 42：加入购物车的结果
            await c.SendPacket(MTSPacketCreator.MTSCartAdded(res.Code == (int)MTSResponseCode.Success));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        public async Task RemoveCartItem(IChannelClient c, int productId)
        {
            var query = GetQuery(c);
            query.Tab = MTSConstants.CART_TAB;
            query.Type = 0;
            query.Page = 0;

            var res = await _server.Transport.RemoveMTSCartItem(new ProtoService.RemoveItemFromCartRequest
            {
                Query = query,
                ProductId = productId,
            });

            // op 43 / op 44：移出购物车的结果
            await c.SendPacket(MTSPacketCreator.MTSCartRemoved(res.Code == (int)MTSResponseCode.Success));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        #endregion

        #region 领取 / 购买

        /// <summary>
        /// 从“待领取”列表把物品放回背包。
        /// <para>
        /// 玩家在 MTS 期间背包由中心服持有，所以「物品进背包」也在中心服完成，
        /// 这里只把落点格子告诉客户端（op 39）。
        /// </para>
        /// </summary>
        public async Task TakeItem(IChannelClient c, int productId)
        {
            var res = await _server.Transport.TakeMTSItem(new ProtoService.TakeItemRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await c.SendPacket(res.Code == (int)MTSResponseCode.InventoryFull
                    ? MTSPacketCreator.MTSFailError(MTSClientErrorCode.InventoryFull)
                    : MTSPacketCreator.MTSFailBuy());
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            // 客户端背包里得先有这件物品，op 39 才有东西可选：
            // 这里只发显示用的 INVENTORY_OPERATION，频道那份 Player.Bag 不动（仍以中心服为准）。
            var item = ProtoMapper.MapItem(res.Item);
            if (item != null)
            {
                await c.SendPacket(PacketCreator.InventoryOperation(true,
                    [new InventoryAdd(item.getInventoryType(), item, (short)res.Position)]));
            }

            // op 39: int 背包页(1=装备..5=现金) + int 格子
            var invType = res.Item == null ? 0 : (int)ItemConstants.getInventoryType(res.Item.Itemid);
            await c.SendPacket(MTSPacketCreator.MTSConfirmTransfer(invType, res.Position));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        public async Task Buy(IChannelClient c, int productId, bool fromCart)
        {
            var res = await _server.Transport.BuyMTSItem(new ProtoService.BuyRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                // NX 不足客户端有专门文案（'B'），其余走通用的购买失败
                if (res.Code == (int)MTSResponseCode.Buy_CostNotEnough)
                {
                    await c.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.NotEnoughNX));
                }
                else
                {
                    await c.SendPacket(MTSPacketCreator.MTSFailBuy());
                }
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            var page = fromCart ? res.CartItems : res.PageData;
            await SendPageData(c, page, res.MyMTSInfo);
            await c.SendPacket(MTSPacketCreator.MTSConfirmBuy());
            await SendCash(c, res.MyMTSInfo?.Cash);
            await c.SendPacket(PacketCreator.enableActions());
        }

        #endregion

        #region 求购

        /// <summary>
        /// 登记求购（op 4）。求购单本身没有实体物品，只登记意愿，成交时才真正交货。
        /// </summary>
        public async Task RegisterWanted(IChannelClient c, int itemId, int price, int quantity, int durationHours)
        {
            if (itemId <= 0 || price <= 0 || quantity <= 0)
            {
                await c.SendPacket(MTSPacketCreator.MTSWantedRegistered(false));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            var res = await _server.Transport.RegisterMTSWanted(new ProtoService.WantedRequest
            {
                Query = GetQuery(c),
                ItemId = itemId,
                Price = price,
                Quantity = quantity,
                Tab = MTSConstants.WANTED_TAB,
                Type = 0,
                BuyerName = c.AccountName,
                DurationHours = durationHours,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                if (res.Code == (int)MTSResponseCode.LimitReached)
                {
                    await c.SendPacket(PacketCreator.serverNotice(5, $"最多只能同时登记 {MTSConstants.MAX_SALE_ITEMS} 条求购"));
                }
                await c.SendPacket(MTSPacketCreator.MTSWantedRegistered(false));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            await c.SendPacket(MTSPacketCreator.MTSWantedRegistered(true));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        /// <summary>
        /// 撤销求购（op 13）
        /// </summary>
        public async Task CancelWanted(IChannelClient c, int productId)
        {
            var res = await _server.Transport.CancelMTSWanted(new ProtoService.ProductRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await c.SendPacket(MTSPacketCreator.MTSWantedCanceled(false));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            await c.SendPacket(MTSPacketCreator.MTSWantedCanceled(true));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        /// <summary>
        /// 与求购单成交（op 3 / op 12）。
        /// <para>
        /// 客户端只带商品id；交付物由中心服按求购单的物品 id 从卖方的背包副本里扣。
        /// 频道侧只负责把结果告诉客户端。
        /// </para>
        /// </summary>
        /// <param name="fromOffer">true = op 3 路径（结果走 op 31/32），false = op 12 路径（走 op 47/48）</param>
        public async Task FulfillWanted(IChannelClient c, int productId, bool fromOffer)
        {
            var peek = await _server.Transport.LoadMTSWanted(new ProtoService.ProductRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
            });

            var wanted = peek.Product?.Item;
            if (peek.Code != (int)MTSResponseCode.Success || wanted == null || wanted.Quantity <= 0)
            {
                // 求购单已经不在了：客户端 code 80 = 「已被取消或售出」（sub_5A46F0）
                await c.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, false, 80));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            var invType = ItemConstants.getInventoryType(wanted.Itemid);

            var res = await _server.Transport.FulfillMTSWanted(new ProtoService.FulfillWantedRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
                InventoryType = (int)invType,
                ItemId = wanted.Itemid,
                Quantity = wanted.Quantity,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                if (res.Code == (int)MTSResponseCode.Buy_CostNotEnough)
                {
                    // 求购人抵用券不足，只有服务端知道，客户端统一显示“成交失败”
                    await c.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.NotEnoughNX));
                }
                // code 82 = 飞镖/子弹不可交易，其余走通用失败（见 sub_5A46F0）
                var reason = res.Code == (int)MTSResponseCode.ItemNotTradable ? (byte)82 : (byte)0;
                await c.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, false, reason));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            await c.SendPacket(MTSPacketCreator.MTSWantedOffered(fromOffer, true));
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        #endregion

        #region 拍卖

        /// <summary>
        /// 转发拍卖上架（op 18）。卖家在 <c>CRegisterAuctionEntryDlg</c> 里填的是三个彼此独立的价：
        /// 起拍价、一口价、加价幅度，外加时长 —— 校验和扣背包都在中心服。
        /// </summary>
        public async Task RegisterAuction(IChannelClient c, InventoryType invType, short slot, int reservePrice, int buyNow, int bidIncrement, int durationHours)
        {
            if (reservePrice <= 0 || buyNow <= 0)
            {
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            var res = await _server.Transport.RegisterMTSAuction(new ProtoService.SaleItemRequest
            {
                MasterId = c.CharacterId,
                Price = reservePrice,
                SellerName = c.AccountName,
                Tab = MTSConstants.AUCTION_TAB,
                Type = 0,
                DurationHours = durationHours,
                BuyNow = buyNow,
                BidIncrement = bidIncrement,
                InventoryType = (int)invType,
                Slot = slot,
            });

            await OnListingResult(c, res.Code, invType, slot);
            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        /// <summary>
        /// 拍卖出价（op 19）。出价本身不扣抵用券，成功后刷新列表即可看到新的当前价；
        /// 真正的扣款发生在按一口价成交（op 20）或到期结算时。
        /// </summary>
        public async Task BidAuction(IChannelClient c, int productId, int bid)
        {
            var res = await _server.Transport.BidMTSAuction(new ProtoService.BidRequest
            {
                Query = GetQuery(c),
                ProductId = productId,
                Bid = bid,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                switch (res.Code)
                {
                    case (int)MTSResponseCode.Bid_Consecutive:
                        // 客户端有专门文案：不能连续出价
                        await c.SendPacket(MTSPacketCreator.MTSConsecutiveBid());
                        break;
                    default:
                        await c.SendPacket(MTSPacketCreator.MTSFailError(MTSClientErrorCode.Unknown));
                        break;
                }
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            await SendPageData(c, res.PageData, res.MyMTSInfo);
            await EnableActions(c, res.MyMTSInfo);
        }

        #endregion
    }
}
