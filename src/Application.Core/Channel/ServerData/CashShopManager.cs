using Application.Core.Channel.DataProviders;
using Application.Core.Client;
using Application.Core.Client.inventory;
using Application.Core.Game.Trades;
using Application.Core.Mappers;
using Application.Core.Server;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Templates.Etc;
using Application.Templates.Item.Pet;
using client.inventory;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 商城的频道侧逻辑。商城内所有操作（op 0x05 / 0x06 / 0x07 / 0x08 / 0x0D / 0x0E / 0x20 / 0x03 / 0x04 等）
    /// 都收敛在这里。
    /// <para>
    /// 和拍卖行一样：商城期间不碰频道侧的 <c>Player</c>。背包、现金仓、金币、点数、
    /// 格数的改动全部发生在中心服，这里只解析参数转发，再按返回值发客户端要的封包。
    /// 退出商城时频道这份不保存，回到主世界重新向中心服拉取，所以本地不做镜像也就不会分叉。
    /// </para>
    /// <para>
    /// 余额 / 现金仓 / 格数都不在频道侧缓存，需要时用
    /// <see cref="IChannelServerTransport.GetCashShopState"/> 现查。
    /// </para>
    /// </summary>
    public class CashShopManager
    {
        readonly ILogger<CashShopManager> _logger;
        readonly WorldChannelServer _server;
        readonly IChannelServerTransport _transport;
        readonly CashItemProvider _cashItemProvider;

        public CashShopManager(WorldChannelServer server, IChannelServerTransport transport, CashItemProvider cashItemProvider, ILogger<CashShopManager> logger)
        {
            _server = server;
            _transport = transport;
            _cashItemProvider = cashItemProvider;
            _logger = logger;
        }

        /// <summary>
        /// 商城的全部状态（余额、现金仓、格数、愿望单）。频道不缓存，每次需要时查一次。
        /// </summary>
        public ProtoService.CashShopStateResponse GetState(IChannelClient c) => _transport.GetCashShopState(new ProtoService.CashShopStateRequest { MasterId = c.CharacterId });

        #region 界面

        /// <summary>
        /// 让商城界面重新可用，并刷新余额。余额在中心服，现查一次。
        /// <para>只有商城（含商城内打开的 coupon 窗口）会用到；拍卖行有自己的余额封包。</para>
        /// </summary>
        public async Task EnableActions(IChannelClient c)
        {
            var state = GetState(c);
            await SendCash(c, state.Cash);
        }

        async Task SendCash(IChannelClient c, ProtoModel.PlayerCashProto? cash)
        {
            await c.SendPacket(PacketCreator.showCash(
                cash?.NxCredit ?? 0,
                cash?.MaplePoint ?? 0,
                cash?.NxPrepaid ?? 0));
        }

        #endregion

        #region 现金仓 <-> 背包

        /// <summary>
        /// op 0x0D：现金仓 → 背包
        /// </summary>
        public async Task TakeFromCashInventory(IChannelClient c, long cashId)
        {
            var res = await _transport.TakeCashItem(new ProtoService.TakeCashItemRequest
            {
                MasterId = c.CharacterId,
                CashId = cashId,
            });

            if (res.Code != (int)MTSResponseCode.Success || res.Item == null)
            {
                await EnableActions(c);
                return;
            }

            // 物品已经由中心服放进它那份背包，频道不碰 Player。
            // 封包里要写完整物品信息，这里生成一份只为发封包；UniqueId 和落点都沿用 proto 里的值
            var item = ProtoMapper.MapItem(res.Item);
            if (item == null)
            {
                await EnableActions(c);
                return;
            }

            await c.SendPacket(PacketCreator.takeFromCashInventory(item));
        }

        /// <summary>
        /// op 0x0E：背包 → 现金仓
        /// </summary>
        public async Task PutIntoCashInventory(IChannelClient c, long cashId, sbyte invType)
        {
            if (invType < 1 || invType > 5)
            {
                await c.Disconnect(false, false);
                return;
            }

            // 客户端声明的页只做范围校验；真正在哪一页由中心服按现金 id 自己找，
            // 找件、校验（宠物 / 关系物品）、移出背包都在那边完成
            var res = await _transport.PutCashItem(new ProtoService.PutCashItemRequest
            {
                MasterId = c.CharacterId,
                CashId = cashId,
            });

            if (res.Code == CashShopMessage.PetEquipped)
            {
                await c.OnlinedCharacter.Popup(ClientMessage.Cash_PutPetIntoCashInventory);
                await EnableActions(c);
                return;
            }
            if (res.Code == CashShopMessage.RelationshipItem)
            {
                await c.OnlinedCharacter.Popup(ClientMessage.Cash_PutRelationshipItemIntoCashInventory);
                await EnableActions(c);
                return;
            }
            if (res.Code != (int)MTSResponseCode.Success || res.Item == null)
            {
                await EnableActions(c);
                return;
            }

            await c.SendPacket(PacketCreator.putIntoCashInventory(res.Item, c.AccountEntity!.Id));
        }

        #endregion

        #region 现金仓条目 <-> 物品

        /// <summary>
        /// 一张虚拟的商品模板：「商城之外发出去的现金道具」用它凑一个 SN 和有效期。
        /// </summary>
        public Item GenerateCouponItem(int itemId, short quantity)
        {
            var it = new CashCommodityTemplate(77777777)
            {
                CashItemSN = 77777777,
                ItemID = itemId,
                Price = 7777,
                Period = ItemConstants.isPet(itemId) ? 30 : 0,
                Count = quantity,
                OnSale = true
            };
            return CashItem2Item(it);
        }

        /// <summary>
        /// 商品模板 → 真物品（有效期、SN 都按模板补上）。
        /// </summary>
        public Item CashItem2Item(CashCommodityTemplate cashItem, long uniqueId = 0)
        {
            var item = ItemInformationProvider.getInstance().GenerateVirtualItemById(cashItem.ItemID, cashItem.Count, uniqueId: uniqueId);

            item.setExpiration(GetExpiration(cashItem));
            item.setSN(cashItem.CashItemSN);
            return item;
        }

        /// <summary>
        /// 按模板推算到期时间。单独抽出来是因为「有效期」在购买时就要定下来并送到中心服，
        /// 那时还没有生成物品。
        /// </summary>
        public long GetExpiration(CashCommodityTemplate cashItem)
        {
            if (cashItem.Period == 1)
            {
                return cashItem.ItemID switch
                {
                    // 4 Hour 2X coupons, the period is 1, but we don't want them to last a day.
                    ItemId.DROP_COUPON_2X_4H or ItemId.EXP_COUPON_2X_4H
                        => _server.getCurrentTime() + (long)TimeSpan.FromHours(4).TotalMilliseconds,
                    ItemId.EXP_COUPON_3X_2H
                        => _server.getCurrentTime() + (long)TimeSpan.FromHours(2).TotalMilliseconds,
                    _ => _server.getCurrentTime() + (long)TimeSpan.FromDays(1).TotalMilliseconds,
                };
            }

            // 宠物的有效期看宠物模板的寿命，普通道具看商城的 Period
            if (ItemInformationProvider.getInstance().GetItemTemplate(cashItem.ItemID) is PetItemTemplate petTemplate)
            {
                return _server.getCurrentTime() + (long)TimeSpan.FromDays(petTemplate.Life).TotalMilliseconds;
            }

            return _server.getCurrentTime() + (long)TimeSpan.FromDays(cashItem.Period).TotalMilliseconds;
        }

        static ProtoService.CashItemPlaceholderProto ToPlaceholder(CashCommodityTemplate template, long expiration, long cashId = 0)
        {
            return new ProtoService.CashItemPlaceholderProto
            {
                Sn = template.CashItemSN,
                ItemId = template.ItemID,
                Quantity = template.Count <= 0 ? 1 : template.Count,
                Expiration = expiration,
                CashId = cashId,
            };
        }

        /// <summary>
        /// 商品模板 → 轻量条目（<see cref="ProtoModel.ItemProto"/>）。
        /// 中心服只有这种结构，现金仓、礼物、背包同步共用。
        /// </summary>
        static ProtoModel.ItemProto ToItemProto(CashCommodityTemplate template, long expiration, string? giftFrom = null)
        {
            return new ProtoModel.ItemProto
            {
                Itemid = template.ItemID,
                Quantity = template.Count <= 0 ? 1 : template.Count,
                Sn = template.CashItemSN,
                Expiration = expiration,
                GiftFrom = giftFrom ?? string.Empty,
            };
        }

        /// <summary>
        /// 中心服回填的条目 → 显示用的轻量条目（cashId 由中心服生成的那份）
        /// </summary>
        static ProtoModel.ItemProto ToItemProto(ProtoService.CashItemPlaceholderProto entry)
        {
            return new ProtoModel.ItemProto
            {
                UniqueId = entry.CashId,
                Itemid = entry.ItemId,
                Quantity = entry.Quantity,
                Sn = entry.Sn,
                Expiration = entry.Expiration,
            };
        }

        /// <summary>
        /// 已经生成的物品 → 轻量条目。
        /// 有效期、赠送人这些模板里没有的信息要显式带走。
        /// </summary>
        public static ProtoModel.ItemProto ToProto(Item item)
        {
            return new ProtoModel.ItemProto
            {
                UniqueId = item.UniqueId,
                Itemid = item.getItemId(),
                Quantity = item.getQuantity(),
                Sn = item.getSN(),
                Expiration = item.getExpiration(),
                GiftFrom = item.getGiftFrom() ?? string.Empty,
            };
        }

        /// <summary>
        /// 商城之外（CDK / NPC 补发）发出去的一件现金道具：生成物品只是为了拿到它那份唯一 id，
        /// 送回中心服时只要模板信息。
        /// </summary>
        public ProtoModel.ItemProto CreateCouponItemProto(int itemId, short quantity)
        {
            return ToProto(GenerateCouponItem(itemId, quantity));
        }

        /// <summary>
        /// 把已经生成好的现金道具补进中心服的现金仓。条目带着频道生成的 UniqueId，
        /// 客户端之后拿这个 id 去取，两边才对得上。
        /// </summary>
        public async Task PushCashItems(int masterId, IEnumerable<ProtoModel.ItemProto> items)
        {
            var request = new ProtoService.AddCashItemsRequest
            {
                MasterId = masterId,
                ToCashShop = true,
            };
            request.Items.AddRange(items);

            var res = await _transport.AddCashItems(request);
            if (res.Code != 0)
            {
                _logger.LogError("现金道具未能落进中心服现金仓，角色 {CharacterId}", masterId);
            }
        }

        #endregion

        #region 愿望单

        /// <summary>
        /// op 0x05：覆盖愿望单。中心服存数据，是否在售由频道的模板判断。
        /// </summary>
        public async Task UpdateWishList(IChannelClient c, List<int> snList)
        {
            var request = new ProtoService.UpdateWishListRequest { MasterId = c.CharacterId };
            request.SnList.AddRange(snList);

            var res = await _transport.UpdateCashWishList(request);

            await c.SendPacket(PacketCreator.showWishList(
                res.Code == (int)MTSResponseCode.Success ? res.SnList : snList,
                true));
        }

        #endregion

        #region 栏位扩容

        /// <summary>
        /// op 0x06：背包扩容。余额与上限判断、扣费全在中心服，频道只发结果。
        /// </summary>
        public async Task GainInventorySlots(IChannelClient c, int cashType, int price, int inventoryType, int slots)
        {
            var res = await _transport.GainInventorySlots(new ProtoService.GainInventorySlotsRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = price,
                InventoryType = inventoryType,
                Slots = slots,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await EnableActions(c);
                return;
            }

            // 格数由中心服改，频道不碰 Player 的背包；回主世界时会重新拉取
            await c.SendPacket(PacketCreator.showBoughtInventorySlots(inventoryType, (short)res.Slots));
            await SendCash(c, res.Cash);
        }

        /// <summary>
        /// op 0x07：仓库扩容（仓库挂在账号上）
        /// </summary>
        public async Task GainStorageSlots(IChannelClient c, int cashType, int price, int slots)
        {
            var res = await _transport.GainStorageSlots(new ProtoService.GainStorageSlotsRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = price,
                Slots = slots,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await EnableActions(c);
                return;
            }

            await c.SendPacket(PacketCreator.showBoughtStorageSlots((short)res.Slots));
            await SendCash(c, res.Cash);
        }

        /// <summary>
        /// op 0x08：角色栏位扩容
        /// </summary>
        public async Task GainCharacterSlots(IChannelClient c, int cashType, int price)
        {
            var res = await _transport.GainCharacterSlots(new ProtoService.GainCharacterSlotsRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = price,
            });

            if (res.Code != (int)MTSResponseCode.Success)
            {
                await c.OnlinedCharacter.Popup("You have already used up all 12 extra character slots.");
                await EnableActions(c);
                return;
            }

            await c.SendPacket(PacketCreator.showBoughtCharacterSlot((short)res.Slots));
            await SendCash(c, res.Cash);
        }

        #endregion

        #region 购买

        /// <summary>
        /// 金币购买任务物品（op 0x20）。扣金币、找空格、落背包都由中心服完成，
        /// 频道只发结果封包。
        /// </summary>
        public async Task BuyCashItemForQuest(IChannelClient c, CashCommodityTemplate cItem)
        {
            var res = await _transport.BuyQuestItem(new ProtoService.BuyQuestItemRequest
            {
                MasterId = c.CharacterId,
                CashItemSn = cItem.CashItemSN,
                ItemId = cItem.ItemID,
                Price = cItem.Price,
            });

            if (res.Code != (int)MTSResponseCode.Success || res.Position <= 0)
            {
                await EnableActions(c);
                return;
            }

            // 金币与背包都由中心服改；频道只把结果告诉客户端
            await c.SendPacket(PacketCreator.showBoughtQuestItem(cItem.ItemID));
            await SendCash(c, res.Cash);
        }

        /// <summary>
        /// op 0x03 / 0x1E：购买。扣费、落仓全在中心服，频道不生成本次购买的任何物品：
        /// 只把「买了哪几件商品」送上去，真物品等玩家回到主世界时按 SN 用模板生成
        /// （见 <see cref="Application.Core.Mappers.ProtoMapper.MapItem"/>）。
        /// </summary>
        public async Task BuyCashItem(IChannelClient c, int cashType, CashCommodityTemplate cItem)
        {
            var isPackage = _cashItemProvider.isPackage(cItem.ItemID);
            var boughtItems = isPackage
                ? _cashItemProvider.GetPackage(cItem.ItemID)
                    .Select(x => _cashItemProvider.GetItemTrust(x))
                    .ToList()
                : [cItem];

            var request = new ProtoService.BuyCashItemRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = cItem.Price,
                CashItemId = cItem.ItemID,
                CashItemSn = cItem.CashItemSN,
            };
            request.Items.AddRange(boughtItems.Select(x => ToPlaceholder(x, GetExpiration(x))));

            var data = _transport.SendBuyCashItem(request);

            if (data.Code != 0)
            {
                await c.SendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                return;
            }

            // 回显直接用中心服落仓的那几条，cashId 也是它生成的，两边对得上
            if (isPackage)
            {
                await c.SendPacket(PacketCreator.showBoughtCashPackage(data.Created.Select(ToItemProto), c.AccountId));
            }
            else if (data.Created.Count > 0)
            {
                await c.SendPacket(PacketCreator.showBoughtCashItem(ToItemProto(data.Created[0]), c.AccountId));
            }

            await SendCash(c, data.Cash);
        }

        /// <summary>
        /// 买一件「立即消耗、不进现金仓」的商品（改名券）。
        /// 物品不产生，只有扣费与统计，但扣费同样必须发生在中心服。
        /// </summary>
        public async Task BuyConsumedCashItem(IChannelClient c, int cashType, CashCommodityTemplate cItem)
        {
            var data = _transport.SendBuyCashItem(new ProtoService.BuyCashItemRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = cItem.Price,
                CashItemId = cItem.ItemID,
                CashItemSn = cItem.CashItemSN,
            });

            if (data.Code != 0)
            {
                await c.SendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                return;
            }

            await SendCash(c, data.Cash);
        }

        /// <summary>
        /// 改名券：名字由中心服登记，成功后频道这份也跟着改（封包里要显示新名字）。
        /// </summary>
        public bool RegisterNameChange(IChannelClient c, string newName)
        {
            ProtoService.NameChangeResponse res = _transport.ReigsterNameChange(new ProtoService.NameChangeRequest { MasterId = c.CharacterId, NewName = newName });
            if (res.Code == 0)
            {
                c.OnlinedCharacter.Name = newName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 送礼路径：礼物本身由中心的 <c>GiftManager</c> 落库。
        /// <para>
        /// 戒指会回赠送礼人自己那一份，中心服在拿到戒指 id 后直接把它写进现金仓，
        /// 所以频道照常把商品信息放进 <c>Items</c> 一起送上去就行 —— 不需要先生成物品再补一次落仓。
        /// </para>
        /// </summary>
        public async Task BuyCashItemForGift(IChannelClient c, int cashType, CashCommodityTemplate cItem, string toName, string message, bool createRing = false)
        {
            var request = new ProtoService.BuyCashItemRequest
            {
                MasterId = c.CharacterId,
                CashType = cashType,
                Price = cItem.Price,
                CashItemId = cItem.ItemID,
                CashItemSn = cItem.CashItemSN,
                GiftInfo = new ProtoModel.GiftInfoProto
                {
                    Message = message,
                    Recipient = toName,
                    CreateRing = createRing
                }
            };

            // 戒指那份的商品信息也放进来：中心服会拿戒指 id 当 cashId 落进现金仓
            if (createRing)
            {
                request.Items.Add(ToPlaceholder(cItem, GetExpiration(cItem)));
            }

            var data = _transport.SendBuyCashItem(request);
            if (data.Code != 0)
            {
                await c.SendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                return;
            }

            await SendGiftResult(c, cItem, data);
        }

        /// <summary>
        /// 送礼的回显。戒指那份已经由中心服落仓，这里只按它回的条目发封包，不生成物品。
        /// </summary>
        async Task SendGiftResult(IChannelClient c, CashCommodityTemplate cItem, ProtoService.BuyCashItemResponse data)
        {
            if (data.GiftInfo != null)
            {
                if (data.GiftInfo.RingSource != null && data.Created.Count > 0)
                {
                    // 原代码中 crush ring 用的是 showBoughtCashItem
                    await c.SendPacket(PacketCreator.showBoughtCashRing(
                        ToItemProto(data.Created[0]), data.GiftInfo.Recipient, c.AccountId));
                }

                await c.SendPacket(PacketCreator.showGiftSucceed(data.GiftInfo.Recipient, cItem));
            }

            await SendCash(c, data.Cash);
        }

        /// <summary>
        /// 模板在售且余额足够。中心服还会再校验一次（它才是权威）。
        /// </summary>
        public bool CanBuy(IChannelClient c, CashCommodityTemplate? item, int cash)
        {
            if (item != null && item.OnSale && item.Price <= cash)
            {
                _logger.LogDebug("Chr {CharacterId} bought cash item {ItemName} (SN {ItemSN}) for {ItemPrice}",
                    c.CharacterId,
                    ClientCulture.SystemCulture.GetItemName(item.ItemID),
                    item.CashItemSN,
                    item.Price);
                return true;
            }

            return false;
        }

        public CashCommodityTemplate? GetItem(int sn) => _cashItemProvider.getItem(sn);

        #endregion

        #region 礼物 / 扭蛋

        /// <summary>
        /// 领出礼物：礼物本身由中心的 <c>GiftManager</c> 落库，领出后要进现金仓。
        /// <para>
        /// 现金仓在中心服，所以这里只把「领的是哪几件商品」送上去，
        /// 由中心服落仓；频道持有的只是用来发封包的显示数据。
        /// </para>
        /// </summary>
        public async Task<List<ItemMessagePair>> LoadPlayerGifts(IChannelClient c)
        {
            List<ItemMessagePair> gifts = new();
            try
            {
                var dataList = _transport.LoadPlayerGifts(new ProtoService.GetMyGiftsRequest { MasterId = c.CharacterId }).List;

                // 礼包按旧实现展开成里面的每一件（礼包里不会有戒指），各占一条现金仓记录
                var added = new List<ProtoModel.ItemProto>();
                var firstOfGift = new List<ProtoModel.ItemProto>();
                foreach (var rs in dataList)
                {
                    var cItem = _cashItemProvider.GetItemTrust(rs.Sn);
                    var entries = _cashItemProvider.isPackage(cItem.ItemID)
                        ? _cashItemProvider.GetPackage(cItem.ItemID)
                            .Select(x => _cashItemProvider.GetItemTrust(x))
                            .ToList()
                        : [cItem];

                    firstOfGift.Add(ToItemProto(entries[0], GetExpiration(entries[0]), rs.FromName));
                    added.AddRange(entries.Select(x => ToItemProto(x, GetExpiration(x), rs.FromName)));
                }

                if (added.Count > 0)
                {
                    var res = await _transport.AddCashItems(new ProtoService.AddCashItemsRequest
                    {
                        MasterId = c.CharacterId,
                        ToCashShop = true,
                        Items = { added },
                    });
                    if (res.Code != 0)
                    {
                        // 没落进中心服就不能核销，否则礼物直接消失
                        _logger.LogError("角色的礼物未能落进中心服现金仓，角色 {CharacterId}", c.CharacterId);
                        return gifts;
                    }

                    _transport.ClearGifts(dataList.Select(x => x.Id).ToArray());
                    (c.TradeServer as CashShopServer)!.Notes += dataList.Count;
                }

                for (var i = 0; i < dataList.Count; i++)
                {
                    gifts.Add(new ItemMessagePair(firstOfGift[i], dataList[i].Message));
                }
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }

            return gifts;
        }

        /// <summary>
        /// 现金扭蛋：消耗一个「现金商城惊喜」换一件随机现金道具。
        /// <para>
        /// 频道只负责抽奖（随机池在 WZ 模板里）：消耗与新增都在中心服完成，
        /// 商城期间现金仓以中心服为准、频道那份退出时不保存，所以扣了但没回写等于白送。
        /// </para>
        /// </summary>
        public async Task<CashShopSurpriseResult?> OpenCashShopSurpriseSynced(IChannelClient c, long cashId)
        {
            var state = GetState(c);
            var used = state.CashItems.FirstOrDefault(x => x.UniqueId == cashId);
            if (used == null
                || used.Itemid != ItemId.CASH_SHOP_SURPRISE
                || used.Quantity <= 0)
            {
                return null;
            }

            if (state.CashItems.Count >= 100)
            {
                return null;
            }

            var rewardTemplate = _cashItemProvider.getRandomCashItem();
            if (rewardTemplate == null)
            {
                return null;
            }

            var res = await _transport.UseCashShopSurprise(new ProtoService.UseCashShopSurpriseRequest
            {
                MasterId = c.CharacterId,
                CashId = cashId,
                Reward = ToPlaceholder(rewardTemplate, GetExpiration(rewardTemplate)),
            });

            if (res.Code != 0 || res.Reward == null)
            {
                _logger.LogError("现金扭蛋未能结算在中心服，角色 {CharacterId}", c.CharacterId);
                return null;
            }

            return new CashShopSurpriseResult(used, res.RemainingQuantity, ToItemProto(res.Reward));
        }

        #endregion
    }
}
