using Application.Shared.Constants;
using Google.Protobuf.Collections;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 中心服侧的背包操作，只服务于 MTS。
    /// <para>
    /// 玩家进 MTS 之后，频道侧的 Player 已经脱离世界（<c>WorldChannel.PlayersAway</c>），
    /// 背包以中心服缓存的那份为准 —— 上架、领取、成交都直接改这里；玩家回到主世界时
    /// 频道会重新向中心服请求角色数据（<c>LoginService.PlayerLogin</c> 返回的
    /// <c>PlayerGetterProto</c> 里带着 <c>Character.Data.Bag</c>），背包自然是最新的。
    /// </para>
    /// <para>
    /// 注意这里<b>不做叠加合并</b>：中心服没有 <c>ItemInformationProvider</c>（slotMax 在 WZ 模板里），
    /// 猜一个上限反而可能造出超过上限的堆。所以进包一律找空格新建一摞；
    /// 背包满了就失败，由调用方把物品留在待领取列表里。
    /// </para>
    /// </summary>
    public static class MtsBagStorage
    {
        public const int Equip = 1;
        public const int Use = 2;
        public const int Setup = 3;
        public const int Etc = 4;
        public const int Cash = 5;

        public static RepeatedField<ProtoModel.ItemProto>? GetInventory(ProtoModel.CharacterBagDataProto? bag, int inventoryType) => inventoryType switch        {
            Equip => bag?.EquipInv,
            Use => bag?.UseInv,
            Setup => bag?.SetupInv,
            Etc => bag?.EtcInv,
            Cash => bag?.CashInv,
            _ => null,
        };

        public static int GetSlotLimit(ProtoModel.CharacterProto chr, int inventoryType) => inventoryType switch
        {
            Equip => chr.Equipslots,
            Use => chr.Useslots,
            Setup => chr.Setupslots,
            Etc => chr.Etcslots,
            Cash => DefaultConfigs.BagCashSize,
            _ => 0,
        };

        /// <summary>
        /// 按格子取出一件物品（上架 / 放进现金仓）。数量为 0 或超过该格数量时取全部。
        /// </summary>
        public static ProtoModel.ItemProto? TakeBySlot(ProtoModel.CharacterProto chr, int inventoryType, int slot, int quantity)
        {
            var inv = GetInventory(chr.Data?.Bag, inventoryType);
            if (inv == null)
            {
                return null;
            }

            var item = inv.FirstOrDefault(x => x.Position == slot);
            if (item == null)
            {
                return null;
            }

            if (quantity <= 0 || quantity >= item.Quantity)
            {
                inv.Remove(item);
                return item;
            }

            // 只取一部分：拆一件出来，原格减数量
            var taken = item.Clone();
            taken.Quantity = quantity;
            item.Quantity -= quantity;
            return taken;
        }

        /// <summary>
        /// 按物品 id 取出一件（求购成交时卖方的交付物）。数量不足返回 null。
        /// </summary>
        public static ProtoModel.ItemProto? TakeById(ProtoModel.CharacterProto chr, int inventoryType, int itemId, int quantity)
        {
            var inv = GetInventory(chr.Data?.Bag, inventoryType);
            if (inv == null)
            {
                return null;
            }

            var item = inv.FirstOrDefault(x => x.Itemid == itemId && x.Quantity >= quantity);
            if (item == null)
            {
                return null;
            }

            if (quantity <= 0 || quantity >= item.Quantity)
            {
                inv.Remove(item);
                return item;
            }

            var taken = item.Clone();
            taken.Quantity = quantity;
            item.Quantity -= quantity;
            return taken;
        }

        /// <summary>
        /// 放进背包，返回落点格子；放不下返回 <c>0</c>。
        /// </summary>
        public static short Add(ProtoModel.CharacterProto chr, int inventoryType, ProtoModel.ItemProto item)
        {
            var inv = GetInventory(chr.Data?.Bag, inventoryType);
            if (inv == null)
            {
                return 0;
            }

            var limit = GetSlotLimit(chr, inventoryType);
            if (limit <= 0)
            {
                return 0;
            }

            var used = inv.Select(x => x.Position).ToHashSet();
            for (short pos = 1; pos <= limit; pos++)
            {
                if (used.Contains(pos))
                {
                    continue;
                }

                item.Position = pos;
                inv.Add(item);
                return pos;
            }

            return 0;
        }

        /// <summary>
        /// 按物品 id 推断背包页（装备优先，其次消耗/设置/其他/现金）
        /// </summary>
        public static int GetInventoryType(ProtoModel.ItemProto? item)
        {
            if (item == null)
            {
                return 0;
            }

            if (item.EquipInfo != null)
            {
                return Equip;
            }

            var id = item.Itemid;
            var thousand = id / 1000000;
            return thousand switch
            {
                1 => Equip,
                2 => Use,
                3 => Setup,
                4 => Etc,
                5 => Cash,
                _ => Etc,
            };
        }

        /// <summary>
        /// 背包是否还放得下（只按空格算，不合并）
        /// </summary>
        public static bool HasRoom(ProtoModel.CharacterProto chr, int inventoryType)
        {
            var inv = GetInventory(chr.Data?.Bag, inventoryType);
            if (inv == null)
            {
                return false;
            }

            return inv.Count < GetSlotLimit(chr, inventoryType);
        }
    }
}
