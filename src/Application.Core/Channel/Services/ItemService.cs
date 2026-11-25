using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Items;
using Application.Core.Game.Trades;
using Application.Core.Server;
using Application.Core.ServerTransports;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Services
{
    /// <summary>
    /// 「中心服核销 → 频道发放」类操作：CDK 兑换、NPC 补发、喇叭 / TV 这类消耗现金道具的即时效果。
    /// <para>
    /// 商城本身的逻辑（现金仓、格数、购买、礼物、扭蛋）在 <see cref="CashShopManager"/>，
    /// 拍卖行在 <see cref="MTSManager"/>。
    /// </para>
    /// </summary>
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;
        readonly CashShopManager _cashShopManager;
        /// <summary>
        /// 「中心服核销 → 频道发放」类操作的统一调度（CDK / NPC 补发）
        /// </summary>
        readonly ItemTransferRunner _transferRunner;

        public ItemService(IMapper mapper,
            IChannelServerTransport transport,
            ILogger<ItemService> logger,
            WorldChannelServer server,
            CashShopManager cashShopManager)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
            _cashShopManager = cashShopManager;
            _transferRunner = new ItemTransferRunner(logger);
        }

        public List<SpecialCashItem> GetSpecialCashItems()
        {
            return _mapper.Map<List<SpecialCashItem>>(_transport.RequestSpecialCashItems().Items);
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

            // 现金道具是提交成功后才补进中心服现金仓的
            await PushRewardCashItems(chr, grant);
            await _cashShopManager.EnableActions(chr.Client);
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
        /// 中心服持有可领取的记录，频道持有背包。
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
            if (chr.IsInExtraServer())
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

            // 点数余额在中心服，现查一次：加起来会不会越界由这里先挡，真正的加减仍在中心服
            var cash = _cashShopManager.GetState(chr.Client).Cash;
            if (CashShopServer.GetCash(cash, CashShopServer.NX_CREDIT) + virItems.Where(x => x.ItemId == ItemId.NxCredit).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }
            if (CashShopServer.GetCash(cash, CashShopServer.MAPLE_POINT) + virItems.Where(x => x.ItemId == ItemId.MaplePoint).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }
            if (CashShopServer.GetCash(cash, CashShopServer.NX_PREPAID) + virItems.Where(x => x.ItemId == ItemId.NxPrepare).Sum(x => x.Quantity) > DefaultConfigs.MaxCredits)
            {
                return false;
            }

            return true;
        }

        async Task GainPackage(Player chr, IEnumerable<ProtoModel.CdkRewordPackageProto> items, RewardGrant grant)
        {
            // 三种点数边累加边记，循环结束后一次提交：礼包经常同时发点券和抵用券，
            // 分两次请求既多一跳，也可能一边成功一边失败
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
                    grant.AddNxCredit(quantity);
                }
                else if (pair.ItemId == ItemId.MaplePoint)
                {
                    grant.AddMaplePoint(quantity);
                }
                else if (pair.ItemId == ItemId.NxPrepare)
                {
                    grant.AddNxPrepaid(quantity);
                }
                else
                {
                    short qty = quantity > short.MaxValue ? short.MaxValue
                        : quantity < 1 ? (short)1
                        : (short)quantity;

                    if (ItemInformationProvider.getInstance().isCash(pair.ItemId))
                    {
                        // 现金道具进中心服的现金仓。不在这里落仓：本地发放一旦失败要回滚，
                        // 而中心服的现金仓没有「按条目撤销」的接口，所以等提交成功后再一起送上去
                        grant.CashItems.Add(_cashShopManager.CreateCouponItemProto(pair.ItemId, qty));
                    }
                    else
                    {
                        await chr.GainItem(pair.ItemId, qty);
                        grant.Items.Add((pair.ItemId, qty));
                    }
                }
            }

            // 三种点数一次提交，全有或全无
            chr.GainCash(grant.NxCredit, grant.MaplePoint, grant.NxPrepaid);
        }

        /// <summary>
        /// 把 <see cref="GainPackage"/> 发下去的东西收回。金币也一并退，
        /// 只回滚道具而留下钱同样是白送。点数与现金道具走中心服，见 <see cref="RewardGrant"/>。
        /// </summary>
        async Task RollbackPackage(Player chr, RewardGrant? grant)
        {
            if (grant == null)
            {
                return;
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
            // 三种点数一次退，与发放时是同一次请求的逆操作
            chr.GainCash(-grant.NxCredit, -grant.MaplePoint, -grant.NxPrepaid);

            _logger.LogWarning("奖励提交被拒，已回滚本次发放（玩家 {CharacterName}）", chr.getName());
        }

        /// <summary>
        /// 提交成功后，把奖励里的现金道具补进中心服的现金仓。
        /// 条目带着频道生成的 UniqueId，客户端之后拿这个 id 去取，两边才对得上。
        /// </summary>
        async Task PushRewardCashItems(Player chr, RewardGrant grant)
        {
            if (grant.CashItems.Count == 0)
            {
                return;
            }

            await _cashShopManager.PushCashItems(chr.Id, grant.CashItems);
        }
    }
}
