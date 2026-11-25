using Application.Shared.Constants;
using Application.Shared.Items;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 现金商城操作的中心服实现（op 0x05 / 0x06 / 0x07 / 0x08 / 0x0D / 0x0E）。
    /// <para>
    /// 频道侧只解析封包并转发；改背包、改现金仓、扣点券、扩容全部在这里完成。
    /// 商城期间频道侧的 <c>Player</c> 仍然存在且仍会改动背包，所以两边必须同时改：
    /// 频道先按自己的背包算好参数（例如落点格子），中心服按同样的语义改自己那份，
    /// 这样之后的同步不会互相覆盖。
    /// </para>
    /// </summary>
    public class CashShopManager
    {
        readonly MasterServer _server;
        readonly ILogger<CashShopManager> _logger;

        public CashShopManager(MasterServer server, ILogger<CashShopManager> logger)
        {
            _server = server;
            _logger = logger;
        }

        #region 状态查询

        /// <summary>
        /// 商城的全部状态：余额、现金仓、格数、愿望单。
        /// <para>
        /// 频道侧不缓存这些值（商城期间它们都由中心服持有），每次要显示界面时查一次；
        /// 现金仓返回的是中心服存的轻量条目，频道拿到后按 SN 物化成真物品再发给客户端。
        /// </para>
        /// </summary>
        public ProtoService.CashShopStateResponse GetState(ProtoService.CashShopStateRequest request)
        {
            var res = new ProtoService.CashShopStateResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var character = chr?.Character;
            if (character == null)
            {
                return res;
            }

            var accountGame = GetAccountGame(character.AccountId);
            if (accountGame != null)
            {
                FillCash(res, accountGame, character.Meso);
                res.StorageSlots = accountGame.Data?.Storage?.Slots ?? 0;

                var cashInv = CashShopStorage.GetCashInventory(character, accountGame);
                if (cashInv != null)
                {
                    res.CashItems.AddRange(cashInv);
                }
            }

            var account = _server.AccountManager.GetAccountDto(character.AccountId);
            if (account != null)
            {
                res.CharacterSlots = account.Characterslots;
            }

            if (character.Data != null)
            {
                res.WishList.AddRange(character.Data.WishItems);
            }

            res.EquipSlots = character.Equipslots;
            res.UseSlots = character.Useslots;
            res.SetupSlots = character.Setupslots;
            res.EtcSlots = character.Etcslots;

            res.Code = (int)MTSResponseCode.Success;
            return res;
        }

        /// <summary>
        /// 增减点数（拾取、Duey、礼包、GM 命令）。账本在中心服，频道不持有余额。
        /// <para>三种点数在同一次请求里提交，全有或全无：任一项越界则整笔拒绝。</para>
        /// </summary>
        public ProtoService.GainCashResponse GainCash(ProtoService.GainCashRequest request)
        {
            var res = new ProtoService.GainCashResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (accountGame == null)
            {
                return res;
            }

            if (!TryPayAll(accountGame, request.NxCredit, request.MaplePoint, request.NxPrepaid))
            {
                return res;
            }

            _server.AccountGameManager.UpdateAccountGame(accountGame);
            FillCash(res, accountGame, chr?.Character?.Meso ?? 0);
            res.Code = (int)MTSResponseCode.Success;
            return res;
        }

        #endregion

        #region 现金仓 <-> 背包

        /// <summary>
        /// op 0x0D：从现金仓取出到背包
        /// </summary>
        public ProtoService.TakeCashItemResponse TakeCashItem(ProtoService.TakeCashItemRequest request)
        {
            var res = new ProtoService.TakeCashItemResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character.Data?.Bag == null || accountGame == null)
            {
                return res;
            }

            var (position, _, item) = CashShopStorage.MoveToBag(chr.Character, accountGame, request.CashId);
            if (position <= 0 || item == null)
            {
                return res;
            }

            _server.CharacterManager.SetState(chr);
            _server.AccountGameManager.UpdateAccountGame(accountGame);
            res.Code = (int)MTSResponseCode.Success;
            // 落点已经写在 item.Position 里，不用再单独回一个 position
            res.Item = item.Clone();
            return res;
        }

        /// <summary>
        /// op 0x0E：从背包放进现金仓
        /// </summary>
        public ProtoService.PutCashItemResponse PutCashItem(ProtoService.PutCashItemRequest request)
        {
            var res = new ProtoService.PutCashItemResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character.Data?.Bag == null || accountGame == null)
            {
                return res;
            }

            // 只在四个常规页里按现金 id 找，不信任客户端给的页（改过的包可以随便填）
            var (position, item) = CashShopStorage.MoveToCash(chr.Character, accountGame, request.CashId);
            if (position <= 0 || item == null)
            {
                // -1 = 带着的宠物，-2 = 关系物品；其余是找不到 / 现金仓不可用。
                // 客户端文案在频道侧，按这两个码回不同的提示
                res.Code = position switch
                {
                    -1 => CashShopMessage.PetEquipped,
                    -2 => CashShopMessage.RelationshipItem,
                    _ => res.Code,
                };
                return res;
            }

            _server.CharacterManager.SetState(chr);
            _server.AccountGameManager.UpdateAccountGame(accountGame);
            res.Code = (int)MTSResponseCode.Success;
            res.Position = position;
            // 频道用它回 op 0x6A（现金仓条目：cashId / itemId / sn / 数量 / 有效期）
            res.Item = item.Clone();
            return res;
        }

        #endregion

        /// <summary>
        /// 把频道用模板生成好的物品落进中心服的现金仓或背包。
        /// <para>
        /// 商城期间以中心服为准、频道那份不保存，所以凡是「生成了新物品」的地方
        /// （戒指回赠、金币购买任务物品）都必须在这里落一份，否则退出商城就丢了。
        /// </para>
        /// </summary>
        /// <summary>
        /// 金币购买任务物品（op 0x20）：扣金币、找空格、落背包都在这里，
        /// 退出商城时频道那份不保存，只在频道扣金币等于没扣。
        /// </summary>
        public ProtoService.BuyQuestItemResponse BuyQuestItem(ProtoService.BuyQuestItemRequest request)
        {
            var res = new ProtoService.BuyQuestItemResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || chr.Character.Meso < request.Price || request.Price < 0)
            {
                return res;
            }

            chr.Character.Meso -= request.Price;

            var item = new ProtoModel.ItemProto
            {
                UniqueId = CashShopStorage.NextCashUniqueId(),
                Itemid = request.ItemId,
                Quantity = 1,
                Sn = request.CashItemSn,
            };
            var position = MtsBagStorage.Add(chr.Character, MtsBagStorage.GetInventoryType(item), item);
            if (position <= 0)
            {
                // 背包正好满了：金币退回去
                chr.Character.Meso += request.Price;
                return res;
            }

            _server.CharacterManager.SetState(chr);
            _server.AccountGameManager.UpdateAccountGame(accountGame);

            res.Code = (int)MTSResponseCode.Success;
            res.Position = position;
            res.Item = item.Clone();
            FillCash(res, accountGame, chr.Character.Meso);
            return res;
        }

        public ProtoService.AddCashItemsResponse AddCashItems(ProtoService.AddCashItemsRequest request)
        {
            var res = new ProtoService.AddCashItemsResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || accountGame == null)
            {
                return res;
            }

            if (request.ToCashShop)
            {
                var cashInv = CashShopStorage.GetCashInventory(chr.Character, accountGame);
                if (cashInv == null)
                {
                    return res;
                }

                cashInv.AddRange(request.Items);
                res.Code = (int)MTSResponseCode.Success;
            }
            else
            {
                foreach (var item in request.Items)
                {
                    var invType = MtsBagStorage.GetInventoryType(item);
                    var position = invType <= 0 ? 0 : MtsBagStorage.Add(chr.Character, invType, item);
                    if (position <= 0)
                    {
                        // 背包满了：已经进去的保留，剩下的如实报告
                        return res;
                    }
                    res.Positions.Add(position);
                }

                res.Code = (int)MTSResponseCode.Success;
                _server.CharacterManager.SetState(chr);
            }

            _server.AccountGameManager.UpdateAccountGame(accountGame);
            return res;
        }

        /// <summary>
        /// 现金扭蛋：消耗一件「现金商城惊喜」，新增一件抽中的商品。
        /// <para>
        /// 频道只负责抽奖（随机池在 WZ 模板里），消耗与新增都在这里完成，
        /// 这样不会出现「频道消耗了、中心服不知道」的分歧。
        /// </para>
        /// </summary>
        public ProtoService.UseCashShopSurpriseResponse UseCashShopSurprise(ProtoService.UseCashShopSurpriseRequest request)
        {
            var res = new ProtoService.UseCashShopSurpriseResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || accountGame == null)
            {
                return res;
            }

            var cashInv = CashShopStorage.GetCashInventory(chr.Character, accountGame);
            var used = CashShopStorage.FindByCashId(cashInv, request.CashId);
            if (cashInv == null || used == null || request.Reward == null)
            {
                return res;
            }

            // 先扣再接；数量归零时把条目整个移除
            used.Quantity -= 1;
            if (used.Quantity <= 0)
            {
                cashInv.Remove(used);
            }

            var reward = new ProtoModel.ItemProto
            {
                UniqueId = request.Reward.CashId > 0 ? request.Reward.CashId : CashShopStorage.NextCashUniqueId(),
                Itemid = request.Reward.ItemId,
                Quantity = request.Reward.Quantity <= 0 ? 1 : request.Reward.Quantity,
                Sn = request.Reward.Sn,
                Expiration = request.Reward.Expiration,
            };
            cashInv.Add(reward);

            _server.AccountGameManager.UpdateAccountGame(accountGame);
            res.Code = (int)MTSResponseCode.Success;
            res.RemainingQuantity = Math.Max(0, used.Quantity);
            res.Reward = new ProtoService.CashItemPlaceholderProto
            {
                Sn = reward.Sn,
                ItemId = reward.Itemid,
                Quantity = reward.Quantity,
                CashId = reward.UniqueId,
                Expiration = reward.Expiration,
            };
            return res;
        }

        #region 愿望单

        /// <summary>
        /// op 0x05：覆盖愿望单。中心服只存数据，是否在售由频道的模板判断。
        /// </summary>
        public ProtoService.UpdateWishListResponse UpdateWishList(ProtoService.UpdateWishListRequest request)
        {
            var res = new ProtoService.UpdateWishListResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr?.Character.Data == null)
            {
                return res;
            }

            var wish = chr.Character.Data.WishItems;
            wish.Clear();
            wish.AddRange(request.SnList);

            _server.CharacterManager.SetState(chr);
            res.Code = (int)MTSResponseCode.Success;
            res.SnList.AddRange(wish);
            return res;
        }

        #endregion

        #region 栏位扩容

        /// <summary>
        /// op 0x06：背包扩容。改完之后以中心服为准，频道侧不再自行 <c>gainSlots</c>。
        /// </summary>
        public ProtoService.GainSlotResponse GainInventorySlots(ProtoService.GainInventorySlotsRequest request)
        {
            var res = new ProtoService.GainSlotResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || accountGame == null)
            {
                return res;
            }

            if (request.Price > 0 && !TryPay(accountGame, request.CashType, request.Price))
            {
                return res;
            }

            var slots = CashShopStorage.GainSlots(chr.Character, request.InventoryType, request.Slots);
            if (slots <= 0)
            {
                // 上限到了：把刚才扣的钱退回去
                if (request.Price > 0)
                {
                    TryPay(accountGame, request.CashType, -request.Price);
                }
                return res;
            }

            _server.CharacterManager.SetState(chr);
            _server.AccountGameManager.UpdateAccountGame(accountGame);
            FillCash(res, accountGame, chr.Character.Meso);
            res.Code = (int)MTSResponseCode.Success;
            res.Slots = slots;
            return res;
        }

        /// <summary>
        /// op 0x07：仓库扩容。仓库挂在账号上（<c>AccountGame.Data.Storage</c>）。
        /// </summary>
        public ProtoService.GainSlotResponse GainStorageSlots(ProtoService.GainStorageSlotsRequest request)
        {
            var res = new ProtoService.GainSlotResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || accountGame?.Data?.Storage == null)
            {
                return res;
            }

            var storage = accountGame.Data.Storage;
            if (request.Slots <= 0 || storage.Slots + request.Slots > Limits.MaxStorageSlots)
            {
                return res;
            }

            if (request.Price > 0 && !TryPay(accountGame, request.CashType, request.Price))
            {
                return res;
            }

            storage.Slots += request.Slots;

            _server.CharacterManager.SetState(chr);
            _server.AccountGameManager.UpdateAccountGame(accountGame);
            FillCash(res, accountGame, chr.Character.Meso);
            res.Code = (int)MTSResponseCode.Success;
            res.Slots = storage.Slots;
            return res;
        }

        /// <summary>
        /// op 0x08：角色栏位扩容。账号字段由 <c>AccountManager</c> 持有。
        /// </summary>
        public ProtoService.GainSlotResponse GainCharacterSlots(ProtoService.GainCharacterSlotsRequest request)
        {
            var res = new ProtoService.GainSlotResponse { MasterId = request.MasterId };

            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = GetAccountGame(chr?.Character.AccountId);
            if (chr?.Character == null || accountGame == null)
            {
                return res;
            }

            var account = _server.AccountManager.GetAccountDto(chr.Character.AccountId);
            if (account == null || account.Characterslots >= Limits.MaxCharacterSlots)
            {
                return res;
            }

            if (request.Price > 0 && !TryPay(accountGame, request.CashType, request.Price))
            {
                return res;
            }

            if (!_server.AccountManager.GainCharacterSlot(account.Id))
            {
                if (request.Price > 0)
                {
                    TryPay(accountGame, request.CashType, -request.Price);
                }
                return res;
            }

            _server.AccountGameManager.UpdateAccountGame(accountGame);
            FillCash(res, accountGame, chr.Character.Meso);
            res.Code = (int)MTSResponseCode.Success;
            res.Slots = account.Characterslots;
            return res;
        }

        #endregion

        #region 内部

        ProtoModel.AccountGameProto? GetAccountGame(int? accountId)
        {
            return accountId == null ? null : _server.AccountGameManager.GetAccountGameData(accountId.Value);
        }

        /// <summary>
        /// 三种点数一起增减，全有或全无：先全部校验，再一次性写入。
        /// </summary>
        static bool TryPayAll(ProtoModel.AccountGameProto accountGame, int nxCredit, int maplePoint, int nxPrepaid)
        {
            if (!CanPay(accountGame.NxCredit, nxCredit)
                || !CanPay(accountGame.MaplePoint, maplePoint)
                || !CanPay(accountGame.NxPrepaid, nxPrepaid))
            {
                return false;
            }

            accountGame.NxCredit += nxCredit;
            accountGame.MaplePoint += maplePoint;
            accountGame.NxPrepaid += nxPrepaid;
            return true;
        }

        /// <summary>
        /// 余额不能为负、不能超过上限
        /// </summary>
        static bool CanPay(int current, int delta) => current + delta >= 0 && current + delta <= Limits.MaxCash;

        /// <summary>
        /// 增减点券 / 抵用券 / 信用卡，余额不能为负、不能超过上限
        /// </summary>
        static bool TryPay(ProtoModel.AccountGameProto accountGame, int cashType, int delta)
        {
            switch (cashType)
            {
                case CashType.NX_CREDIT:
                    if (accountGame.NxCredit + delta < 0 || accountGame.NxCredit + delta > Limits.MaxCash)
                        return false;
                    accountGame.NxCredit += delta;
                    return true;
                case CashType.MAPLE_POINT:
                    if (accountGame.MaplePoint + delta < 0 || accountGame.MaplePoint + delta > Limits.MaxCash)
                        return false;
                    accountGame.MaplePoint += delta;
                    return true;
                case CashType.NX_PREPAID:
                    if (accountGame.NxPrepaid + delta < 0 || accountGame.NxPrepaid + delta > Limits.MaxCash)
                        return false;
                    accountGame.NxPrepaid += delta;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 金币（角色上）+ 三种点数（账号上）。两边都可能为空，缺省按 0 回。
        /// </summary>
        static ProtoModel.PlayerCashProto ToPlayerCash(ProtoModel.AccountGameProto? accountGame, int meso)
        {
            return new ProtoModel.PlayerCashProto
            {
                Meso = meso,
                NxCredit = accountGame?.NxCredit ?? 0,
                MaplePoint = accountGame?.MaplePoint ?? 0,
                NxPrepaid = accountGame?.NxPrepaid ?? 0,
            };
        }

        static void FillCash(ProtoService.GainSlotResponse res, ProtoModel.AccountGameProto accountGame, int meso)
            => res.Cash = ToPlayerCash(accountGame, meso);

        static void FillCash(ProtoService.CashShopStateResponse res, ProtoModel.AccountGameProto accountGame, int meso)
            => res.Cash = ToPlayerCash(accountGame, meso);

        static void FillCash(ProtoService.BuyQuestItemResponse res, ProtoModel.AccountGameProto accountGame, int meso)
            => res.Cash = ToPlayerCash(accountGame, meso);

        static void FillCash(ProtoService.GainCashResponse res, ProtoModel.AccountGameProto accountGame, int meso)
            => res.Cash = ToPlayerCash(accountGame, meso);

        #endregion
    }
}
