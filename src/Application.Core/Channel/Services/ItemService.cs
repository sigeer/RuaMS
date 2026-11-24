using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Items;
using Application.Core.Mappers;
using Application.Core.Model;
using Application.Core.Server;
using Application.Core.ServerTransports;
using Application.Templates.Etc;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;
using static Application.Core.Server.CashShop;

namespace Application.Core.Channel.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;
        readonly CashItemProvider _cashItemProvider;
        /// <summary>
        /// 「中心服核销 → 频道发放」类操作的统一调度（CDK / NPC 补发），与 MTS 共用
        /// </summary>
        readonly ItemTransferRunner _transferRunner;

        public ItemService(IMapper mapper,
            IChannelServerTransport transport,
            ILogger<ItemService> logger,
            WorldChannelServer server,
            CashItemProvider cashItemProvider)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
            _cashItemProvider = cashItemProvider;
            _transferRunner = new ItemTransferRunner(logger);
        }

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
        public Item CashItem2Item(CashCommodityTemplate cashItem, long uniqueId = 0)
        {
            var item = ItemInformationProvider.getInstance().GenerateVirtualItemById(cashItem.ItemID, cashItem.Count, uniqueId: uniqueId);

            if (cashItem.Period == 1)
            {
                switch (cashItem.ItemID)
                {
                    case ItemId.DROP_COUPON_2X_4H:
                    case ItemId.EXP_COUPON_2X_4H: // 4 Hour 2X coupons, the period is 1, but we don't want them to last a day.
                        item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromHours(4).TotalMilliseconds);
                        /*
                        } else if(itemId == 5211047 || itemId == 5360014) { // 3 Hour 2X coupons, unused as of now
                                item.setExpiration(Server.getInstance().getCurrentTime() + HOURS.toMillis(3));
                        */
                        break;
                    case ItemId.EXP_COUPON_3X_2H:
                        item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromHours(2).TotalMilliseconds);
                        break;
                    default:
                        item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(1).TotalMilliseconds);
                        break;
                }
            }
            else
            {
                if (item is Pet subPet)
                    item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(subPet.SourceTemplate.Life).TotalMilliseconds);
                else
                    item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(cashItem.Period).TotalMilliseconds);
            }

            item.setSN(cashItem.CashItemSN);
            return item;
        }

        public List<Item> getPackage(int itemId)
        {
            return _cashItemProvider.GetPackage(itemId).Select(x => CashItem2Item(_cashItemProvider.GetItemTrust(x))).ToList();
        }

        public List<SpecialCashItem> GetSpecialCashItems()
        {
            return _mapper.Map<List<SpecialCashItem>>(_transport.RequestSpecialCashItems().Items);
        }

        public List<ItemMessagePair> LoadPlayerGifts(Player chr)
        {
            var cashShop = chr.getCashShop();
            if (cashShop == null)
                return [];

            List<ItemMessagePair> gifts = new();
            try
            {
                var dataList = _mapper.Map<GiftModel[]>(_transport.LoadPlayerGifts(new ProtoService.GetMyGiftsRequest { MasterId = chr.Id }).List);
                foreach (var rs in dataList)
                {
                    cashShop.Notes++;
                    var cItem = _cashItemProvider.GetItemTrust(rs.Sn);
                    Item item = CashItem2Item(cItem);

                    item.setGiftFrom(rs.FromName);
                    if (item is Equip equip)
                    {
                        gifts.Add(new ItemMessagePair(equip, rs.Message));
                    }
                    else
                    {
                        gifts.Add(new(item, rs.Message));
                    }

                    if (_cashItemProvider.isPackage(cItem.ItemID))
                    {
                        var packages = _cashItemProvider.GetPackage(cItem.ItemID).Select(x => CashItem2Item(_cashItemProvider.GetItemTrust(x)));
                        //Packages never contains a ring
                        foreach (Item packageItem in packages)
                        {
                            packageItem.setGiftFrom(rs.FromName);
                            cashShop.addToInventory(packageItem);
                        }
                    }
                    else
                    {
                        cashShop.addToInventory(item);
                    }
                }

                _transport.ClearGifts(dataList.Select(x => x.Id).ToArray());
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }

            return gifts;
        }

        public CashShopSurpriseResult? OpenCashShopSurprise(CashShop cs, long cashId)
        {
            // 移除了这里的锁
            Item? maybeCashShopSurprise = cs.getItemByCashId(cashId);
            if (maybeCashShopSurprise == null ||
                    maybeCashShopSurprise.getItemId() != ItemId.CASH_SHOP_SURPRISE)
            {
                return null;
            }

            Item cashShopSurprise = maybeCashShopSurprise;
            if (cashShopSurprise.getQuantity() <= 0)
            {
                return null;
            }

            if (cs.getItemsSize() >= 100)
            {
                return null;
            }

            var cashItemReward = _cashItemProvider.getRandomCashItem();
            if (cashItemReward == null)
            {
                return null;
            }

            short newQuantity = (short)(cashShopSurprise.getQuantity() - 1);
            cashShopSurprise.setQuantity(newQuantity);
            if (newQuantity <= 0)
            {
                cs.removeFromInventory(cashShopSurprise);
            }

            Item itemReward = CashItem2Item(cashItemReward);
            cs.addToInventory(itemReward);

            return new CashShopSurpriseResult(cashShopSurprise, itemReward);
        }


        internal bool UseCash_TV(Player player, string? victim, List<string> messages, int tvType, bool showEar)
        {
            var request = new ProtoService.CreateTVMessageRequest
            {
                MasterId = player.Id,
                PartnerName = victim,
                Type = tvType,
                ShowEar = showEar,
            };
            request.MessageList.AddRange(messages);
            var r = _transport.BroadcastTV(request);
            return r.Code == 0;
        }


        internal bool UseCash_ItemMegaphone(Player player, Item? item, string message, bool isWishper)
        {
            var request = new ProtoService.UseItemMegaphoneRequest
            {
                MasterId = player.Id,
                Message = message,
                Item = _server.Mapper.Map<ProtoModel.ItemProto>(item),
                IsWishper = isWishper,
            };
            var r = _transport.SendItemMegaphone(request);
            return r.Code == 0;
        }

        public async Task BuyCashItem(Player chr, int cashType, CashCommodityTemplate cItem)
        {
            await chr.BuyCashItem(cashType, cItem, async () =>
            {
                var data = _transport.SendBuyCashItem(new ProtoService.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.ItemID,
                    CashItemSn = cItem.CashItemSN,
                });

                if (data.Code != 0)
                {
                    await chr.SendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                    return false;
                }

                await BuyCashItemCallback(chr, cItem, data);
                return true;
            });
        }

        async Task BuyCashItemCallback(Player chr, CashCommodityTemplate cItem, ProtoService.BuyCashItemResponse data)
        {
            if (data.GiftInfo != null)
            {
                var ringUniqueId = data.GiftInfo.RingSource == null ? 0 : (data.GiftInfo.RingSource.CharacterId1 == chr.Id ? data.GiftInfo.RingSource.RingId1 : data.GiftInfo.RingSource.RingId2);
                Item item = CashItem2Item(cItem, uniqueId: ringUniqueId);
                if (data.GiftInfo.RingSource != null && item is Equip equip)
                {
                    // 也许没必要？在现金仓库中没必要更新，回到主世界后会重新加载
                    chr.addPlayerRing(data.GiftInfo.RingSource);
                    // 原代码中 crush ring 用的是showBoughtCashItem
                    await chr.SendPacket(PacketCreator.showBoughtCashRing(item, data.GiftInfo.Recipient, chr.Client.AccountId));
                    chr.getCashShop().addToInventory(item);
                }


                await chr.SendPacket(PacketCreator.showGiftSucceed(data.GiftInfo.Recipient, cItem));
            }
            else
            {
                if (_cashItemProvider.isPackage(cItem.ItemID))
                {
                    var cashPackage = getPackage(cItem.ItemID);
                    foreach (var item in cashPackage)
                    {
                        chr.getCashShop().addToInventory(item);
                    }
                    await chr.SendPacket(PacketCreator.showBoughtCashPackage(cashPackage, chr.Client.AccountId));
                }
                else
                {
                    Item item = CashItem2Item(cItem);

                    await chr.SendPacket(PacketCreator.showBoughtCashItem(item, chr.Client.AccountId));
                    chr.getCashShop().addToInventory(item);

                }

            }

            await chr.SendPacket(PacketCreator.showCash(chr));
        }

        public async Task BuyCashItemForGift(Player chr, int cashType, CashCommodityTemplate cItem, string toName, string message, bool createRing = false)
        {
            await chr.BuyCashItem(cashType, cItem, async () =>
            {
                var data = _transport.SendBuyCashItem(new ProtoService.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.ItemID,
                    CashItemSn = cItem.CashItemSN,
                    GiftInfo = new ProtoModel.GiftInfoProto
                    {
                        Message = message,
                        Recipient = toName,
                        CreateRing = createRing
                    }
                });
                if (data.Code != 0)
                {
                    await chr.SendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                    return false;
                }

                await BuyCashItemCallback(chr, cItem, data);
                return true;
            });
        }

        public bool RegisterNameChange(Player chr, string newName)
        {
            ProtoService.NameChangeResponse res = _transport.ReigsterNameChange(new ProtoService.NameChangeRequest { MasterId = chr.Id, NewName = newName });
            if (res.Code == 0)
            {
                chr.Name = newName;
                return true;
            }

            return false;
        }

        internal async Task UseCouponCode(Player chr, string cdk)
        {
            if (!chr.attemptCsCoupon())
            {
                await chr.SendPacket(PacketCreator.showCashShopMessage((byte)UseCdkResponseCode.TooManyError));
                return;
            }

            var (code, grant) = await ClaimReward(chr, (token, stage) => Task.FromResult(_transport.UseCdk(new ProtoService.UseCdkRequest
            {
                MasterId = chr.Id,
                Cdk = cdk,
                Token = token,
                Stage = stage,
            })));

            if (code != UseCdkResponseCode.Success)
            {
                await chr.SendPacket(PacketCreator.showCashShopMessage((byte)code));
                return;
            }

            chr.resetCsCoupon();

            var cashItems = grant!.CashItems;
            if (cashItems.Count > 255)
            {
                cashItems = cashItems.Take(255).ToList();
            }
            if (grant.NxCredit != 0 || grant.NxPrepaid != 0)
            { //coupon packet can only show maple points (afaik)
                await chr.SendPacket(PacketCreator.showBoughtQuestItem(0));
            }
            else
            {
                await chr.SendPacket(PacketCreator.showCouponRedeemedItems(chr.Client.AccountEntity!.Id,
                    grant.MaplePoint, grant.Meso, cashItems, [.. grant.Items.Select(x => new ItemQuantity(x.ItemId, x.Quantity))]));
            }
            await chr.enableCSActions();
        }

        /// <summary>
        /// 领取 NPC 补发的奖励（n9010000）。走和 CDK 同一条流程。
        /// </summary>
        public async Task<UseCdkResponseCode> ClaimReward(Player chr, int rewardId)
        {
            var (code, grant) = await ClaimReward(chr, (token, stage) => _transport.TakeReward(new ProtoService.UseIdRequest
            {
                MasterId = chr.Id,
                Id = rewardId,
                Token = token,
                Stage = stage,
            }));

            if (code == UseCdkResponseCode.Success && grant != null)
            {
                foreach (var item in grant.Items)
                {
                    await chr.GainItemShowMessage(item.ItemId, item.Quantity, GainItemShow.ShowInChat);
                }
            }

            return code;
        }

        /// <summary>
        /// 「中心服核销 → 频道发到背包」类操作的统一入口（CDK 兑换、NPC 补发）。
        /// <para>
        /// 和 MTS 的领取是同一个模式：中心服持有可领取的记录，频道持有背包。
        /// 旧实现里中心服先标记已领、频道再发道具，发失败也没人管 —— 背包一满奖励就没了。
        /// 现在按 <c>PREPARE → 发放 → COMMIT</c> 走：发放失败就 <c>ABORT</c> 释放占位，
        /// 提交被拒就把已经发下去的东西收回来。
        /// </para>
        /// </summary>
        async Task<(UseCdkResponseCode Code, RewardGrant? Grant)> ClaimReward(Player chr,
            Func<long, ProtoModel.TransferStage, Task<ProtoService.UseCdkResponse>> request)
        {
            RewardGrant? grant = null;
            ProtoService.UseCdkResponse? last = null;

            var outcome = await _transferRunner.RunAsync(new ItemTransfer
            {
                Prepare = async token =>
                {
                    var res = await request(token, ProtoModel.TransferStage.Prepare);
                    last = res;
                    return res.Code == (int)UseCdkResponseCode.Success;
                },
                ApplyLocal = async () =>
                {
                    if (last == null)
                    {
                        return false;
                    }

                    // 先整体预检：能放下才发。否则发到一半失败要回滚混合内容（金币/点券/道具）
                    if (!await CanGainPackage(chr, last.Items))
                    {
                        return false;
                    }

                    grant = new RewardGrant();
                    await GainPackage(chr, last.Items, grant);
                    return true;
                },
                Commit = async token =>
                {
                    var res = await request(token, ProtoModel.TransferStage.Commit);
                    last = res;
                    return res.Code == (int)UseCdkResponseCode.Success;
                },
                Abort = token => request(token, ProtoModel.TransferStage.Abort),
                RollbackLocal = () => RollbackPackage(chr, grant),
            });

            var code = outcome switch
            {
                TransferOutcome.Done => UseCdkResponseCode.Success,
                // 中心服明确拒绝：把它的原因原样带回给客户端
                TransferOutcome.Rejected => (UseCdkResponseCode)(last?.Code ?? (int)UseCdkResponseCode.FetalError),
                // 本地放不下 / 结果未知：都算失败，背包提示已经单独发过
                _ => UseCdkResponseCode.FetalError,
            };
            return (code, grant);
        }

        /// <summary>
        /// 奖励包里所有道具是否都放得下（金币/点券不占格子，直接通过）
        /// </summary>
        async Task<bool> CanGainPackage(Player chr, IEnumerable<ProtoModel.CdkRewordPackageProto> items)
        {
            if (chr.getCashShop().isOpened())
            {
                return true;
            }

            var acItems = items.Where(x => x.ItemId > 0);
            var virItems = items.Where(x => x.ItemId <= 0);

            if (!Inventory.checkSpots(chr, acItems.Select(x => new TypedItemQuantity((sbyte)ItemConstants.getInventoryType(x.ItemId), new ItemQuantity(x.ItemId, x.Quantity)))))
            {
                await chr.Popup("Your inventory is full.");
                return false;
            }

            if (chr.MesoValue + virItems.Where(x => x.ItemId == ItemId.Meso).Sum(x => (long)x.Quantity) > int.MaxValue)
            {
                return false;
            }

            if (chr.getCashShop().NxCredit + virItems.Where(x => x.ItemId == ItemId.Meso).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }
            if (chr.getCashShop().NxCredit + virItems.Where(x => x.ItemId == ItemId.MaplePoint).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }
            if (chr.getCashShop().NxCredit + virItems.Where(x => x.ItemId == ItemId.NxPrepare).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }

            return true;
        }

        async Task GainPackage(Player chr, IEnumerable<ProtoModel.CdkRewordPackageProto> items, RewardGrant grant)
        {
            var cs = chr.getCashShop();
            foreach (var pair in items)
            {
                int quantity = pair.Quantity;
                if (pair.ItemId == ItemId.Meso)
                {
                    await chr.GainMeso(quantity);
                    grant.AddMeso(quantity);
                }
                else if (pair.ItemId == ItemId.NxCredit)
                {
                    cs.gainCash(1, quantity);
                    grant.AddNxCredit(quantity);
                }
                else if (pair.ItemId == ItemId.MaplePoint)
                {
                    cs.gainCash(2, quantity);
                    grant.AddMaplePoint(quantity);
                }
                else if (pair.ItemId == ItemId.NxPrepare)
                {
                    cs.gainCash(4, quantity);
                    grant.AddNxPrepaid(quantity);
                }
                else
                {
                    short qty = quantity > short.MaxValue ? short.MaxValue
                        : quantity < 1 ? (short)1
                        : (short)quantity;

                    if (ItemInformationProvider.getInstance().isCash(pair.ItemId))
                    {
                        var it = GenerateCouponItem(pair.ItemId, qty);
                        cs.addToInventory(it);
                        grant.CashItems.Add(it);
                    }
                    else
                    {
                        await chr.GainItem(pair.ItemId, qty);
                        grant.Items.Add((pair.ItemId, qty));
                    }
                }
            }
        }

        /// <summary>
        /// 把 <see cref="GainPackage"/> 发下去的东西收回。现金/点券/金币也一并退，
        /// 只回滚道具而留下钱同样是白送。
        /// </summary>
        async Task RollbackPackage(Player chr, RewardGrant? grant)
        {
            if (grant == null)
            {
                return;
            }

            var cs = chr.getCashShop();
            foreach (var cashItem in grant.CashItems)
            {
                cs.removeFromInventory(cashItem);
            }

            foreach (var (itemId, quantity) in grant.Items)
            {
                var invType = ItemConstants.getInventoryType(itemId);
                await InventoryManipulator.removeById(chr.Client, invType, itemId, quantity, false, false);
            }

            if (grant.Meso != 0)
            {
                await chr.GainMeso(-grant.Meso);
            }
            if (grant.NxCredit != 0)
            {
                cs.gainCash(1, -grant.NxCredit);
            }
            if (grant.MaplePoint != 0)
            {
                cs.gainCash(2, -grant.MaplePoint);
            }
            if (grant.NxPrepaid != 0)
            {
                cs.gainCash(4, -grant.NxPrepaid);
            }

            _logger.LogWarning("奖励提交被拒，已回滚本次发放（玩家 {CharacterName}）", chr.getName());
        }
    }
}
