using client.inventory;
using client.inventory.manipulator;

namespace Application.Core.Channel.Services
{
    public interface IItemDistributeService
    {
        /// <summary>
        /// 发放道具/金币，不能发放的部分以其他方式给予
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="items"></param>
        /// <param name="meso"></param>
        /// <param name="title"></param>
        void Distribute(Player chr, List<Item> items, int meso, int cashType, int cashValue, string? title = null);
    }

    /// <summary>
    /// 不能发放的部分作为掉落物发放（但是有些物品不可拾取，比如固定道具）
    /// </summary>
    public class DefaultItemDistributeService : IItemDistributeService
    {
        public void Distribute(Player chr, List<Item> items, int meso, int cashType, int cashValue, string? title = null)
        {
            bool needNotice = false;
            foreach (var item in items)
            {
                if (chr.canHold(item.getItemId(), item.getQuantity()))
                    InventoryManipulator.addFromDrop(chr.Client, item, false);
                else
                {
                    needNotice = true;
                    chr.getMap().spawnItemDrop(chr, chr, item, chr.getPosition(), false, true);
                }
            }

            if (meso != 0)
            {
                if (chr.canHoldMeso(meso))
                    chr.gainMeso(meso, false);
                else
                {
                    needNotice = true;
                    chr.getMap().spawnMesoDrop(meso, chr.getPosition(), chr, chr, true, 0);
                }
            }

            chr.getCashShop().gainCash(cashType, cashValue);
            if (needNotice)
            {
                chr.dropMessage($"你的背包满了，物品掉落在{chr.getMap().getStreetName()}");
            }
        }
    }
}
