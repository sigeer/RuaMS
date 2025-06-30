using client.inventory;
using client.inventory.manipulator;

namespace Application.Core.Channel.Services
{
    public interface IItemDistributeService
    {
        void Distribute(IPlayer chr, List<Item> items, int meso, string? title = null);
    }

    public class DefaultItemDistributeService : IItemDistributeService
    {
        public void Distribute(IPlayer chr, List<Item> items, int meso, string? title = null)
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

            if (chr.canHoldMeso(meso))
                chr.gainMeso(meso, false);
            else
            {
                needNotice = true;
                chr.getMap().spawnMesoDrop(meso, chr.getPosition(), chr, chr, true, 0);
            }

            if (needNotice)
            {
                chr.dropMessage($"你的背包满了，物品掉落在{chr.getMap().getStreetName()}");
            }
        }
    }
}
