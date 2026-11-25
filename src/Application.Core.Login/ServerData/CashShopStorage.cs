using Application.Shared.Constants;
using Application.Shared.Constants.Item;
using Application.Utility.Configs;
using Google.Protobuf.Collections;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 中心服侧的现金仓操作。
    /// <para>
    /// 现金仓按 <see cref="Shared.Items.ItemType"/> 分成四份，存在账号数据
    /// （<c>AccountGame.Data.CashExplorerItems</c> 等）里；背包存在角色数据
    /// （<c>Character.Data.Bag</c>）里。商城期间频道侧的 <c>Player</c> 仍然存在，
    /// 但状态以中心服为准：频道收到 op 0x0D / 0x0E / 0x05 / 0x06 / 0x07 / 0x08
    /// 只负责转发，真正的修改在这里完成，再按返回值镜像回频道。
    /// </para>
    /// <para>
    /// 和 <see cref="MtsBagStorage"/> 一样<b>不做叠加合并</b>：中心服没有
    /// <c>ItemInformationProvider</c>（slotMax 在 WZ 模板里），猜一个上限可能造出超限的堆。
    /// </para>
    /// </summary>
    public static class CashShopStorage
    {
        const int ExplorerJobType = 0;
        const int CygnusJobType = 1;
        const int AranJobType = 2;

        /// <summary>
        /// 现金道具的唯一 id。客户端就是拿它当 cashId 用的，所以生成之后必须一路传下去：
        /// 中心服落仓用它，频道生成真物品时也沿用它，两边才对得上。
        /// </summary>
        public static long NextCashUniqueId() => Yitter.IdGenerator.YitIdHelper.NextId();

        /// <summary>
        /// 当前角色用于现金仓的那一份。判定规则和频道侧 <c>CashShop</c> 构造函数一致：
        /// 未开启共享现金仓时按职业分支（冒险家 / 骑士团 / 战神），否则全部共用一份。
        /// </summary>
        public static RepeatedField<ProtoModel.ItemProto>? GetCashInventory(ProtoModel.CharacterProto chr, ProtoModel.AccountGameProto? accountGame)
        {
            var data = accountGame?.Data;
            if (data == null)
            {
                return null;
            }

            if (YamlConfig.config.server.USE_JOINT_CASHSHOP_INVENTORY)
            {
                return data.CashOverallItems;
            }

            return (chr.JobId / 1000) switch
            {
                ExplorerJobType => data.CashExplorerItems,
                CygnusJobType => data.CashCygnusItems,
                AranJobType => data.CashAranItems,
                _ => data.CashOverallItems,
            };
        }

        /// <summary>
        /// 按现金 id 在现金仓里找到物品（现金 id 就是 <c>ItemProto.UniqueId</c>）
        /// </summary>
        public static ProtoModel.ItemProto? FindByCashId(RepeatedField<ProtoModel.ItemProto>? inventory, long cashId)
        {
            return inventory?.FirstOrDefault(x => x.UniqueId == cashId);
        }

        /// <summary>
        /// 现金仓 → 背包，落点和背包页都由中心服按物品本身推算（不信频道传上来的）。
        /// </summary>
        /// <returns>落点格子、背包页与取出的条目，失败时位置为 0</returns>
        public static (short Position, int InventoryType, ProtoModel.ItemProto? Item) MoveToBag(ProtoModel.CharacterProto chr, ProtoModel.AccountGameProto? accountGame, long cashId)
        {
            var cashInv = GetCashInventory(chr, accountGame);
            var item = FindByCashId(cashInv, cashId);
            if (item == null)
            {
                return (0, 0, null);
            }

            // 现金仓里的物品有可能是装备，不能只按 id 分页
            var invType = MtsBagStorage.GetInventoryType(item);
            if (invType <= 0 || invType > MtsBagStorage.Cash)
            {
                return (0, 0, null);
            }

            // 先出仓再进包：中间失败要能回滚，不能让物品同时挂在两处
            if (!cashInv!.Remove(item))
            {
                return (0, 0, null);
            }

            var position = MtsBagStorage.Add(chr, invType, item);
            if (position <= 0)
            {
                // 背包满了：物品放回现金仓，调用方按失败回复
                cashInv.Add(item);
                return (0, 0, null);
            }

            return (position, invType, item);
        }

        /// <summary>
        /// 背包 → 现金仓。物品按现金 id 在背包各页里找（现金 id = <c>UniqueId</c>）。
        /// </summary>
        /// <returns>现金仓里的位置与收进去的那件，失败返回 0 / null</returns>
        public static (int Position, ProtoModel.ItemProto? Item) MoveToCash(ProtoModel.CharacterProto chr, ProtoModel.AccountGameProto? accountGame, long cashId)
        {
            var cashInv = GetCashInventory(chr, accountGame);
            if (cashInv == null)
            {
                return (0, null);
            }

            // 只在前四页里找：现金页是本操作的目标仓，不可能已经放着同一件物品
            for (int invType = MtsBagStorage.Equip; invType <= MtsBagStorage.Etc; invType++)
            {
                var bagInv = MtsBagStorage.GetInventory(chr.Data?.Bag, invType);
                var item = bagInv?.FirstOrDefault(x => x.UniqueId == cashId);
                if (item == null)
                {
                    continue;
                }

                // 业务规则也在中心服判断：频道那份已经不再改动背包，它看不到哪些宠物正被带着
                if (item.PetInfo != null && item.PetInfo.PetSlot > -1)
                {
                    return (-1, item);
                }

                if (ItemId.isWeddingRing(item.Itemid) || ItemId.isWeddingToken(item.Itemid))
                {
                    return (-2, item);
                }

                bagInv!.Remove(item);
                cashInv.Add(item);
                return (cashInv.Count, item);
            }

            return (0, null);
        }

        /// <summary>
        /// 判断背包能否再加若干格（装备 / 消耗 / 设置 / 其他）
        /// </summary>
        public static bool CanGainSlots(ProtoModel.CharacterProto chr, int inventoryType, int slots)
        {
            return MtsBagStorage.GetSlotLimit(chr, inventoryType) + slots <= DefaultConfigs.BagMaxSize;
        }

        /// <summary>
        /// 背包扩容。中心服是唯一权威：频道侧不再自行 <c>gainSlots</c>。
        /// </summary>
        /// <returns>扩容后的上限，失败返回 0</returns>
        public static int GainSlots(ProtoModel.CharacterProto chr, int inventoryType, int slots)
        {
            if (slots <= 0 || !CanGainSlots(chr, inventoryType, slots))
            {
                return 0;
            }

            switch (inventoryType)
            {
                case MtsBagStorage.Equip:
                    chr.Equipslots += slots;
                    return chr.Equipslots;
                case MtsBagStorage.Use:
                    chr.Useslots += slots;
                    return chr.Useslots;
                case MtsBagStorage.Setup:
                    chr.Setupslots += slots;
                    return chr.Setupslots;
                case MtsBagStorage.Etc:
                    chr.Etcslots += slots;
                    return chr.Etcslots;
                default:
                    return 0;
            }
        }
    }
}
