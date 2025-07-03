using Application.Core.Channel.DataProviders;
using client.inventory;

namespace Application.Core.Game.Items
{
    public class InventorySorter
    {
        public static List<Item> Sort(List<Item> inventoryItems, int sort, int thenSort)
        {
            if (inventoryItems.Count == 0)
                return inventoryItems;

            var inventoryType = inventoryItems[0].getInventoryType();

            var p = inventoryItems.OrderBy(x => x.getItemId());
            var ii = ItemInformationProvider.getInstance();
            if (sort == 1)
                p = inventoryItems.OrderBy(x => x.getQuantity());
            if (sort == 2)
                p = inventoryItems.OrderBy(x => ii.getName(x.getItemId()));
            if (sort == 3)
                p = inventoryItems.OrderBy(x => ((Equip)x).getLevel());

            if (inventoryType == InventoryType.USE)
                p = p.ThenByDescending(x => ii.getWatkForProjectile(x.getItemId()));

            if (thenSort == 1)
                p = p.ThenBy(x => x.getQuantity());
            if (thenSort == 2)
                p = p.ThenBy(x => ii.getName(x.getItemId()));
            if (thenSort == 3)
                p = p.ThenBy(x => ((Equip)x).getLevel());

            return p.ToList();
        }
    }
}
