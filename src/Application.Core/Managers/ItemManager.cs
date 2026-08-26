using Application.Core.Channel.DataProviders;
using client.inventory;
using static client.inventory.Equip;

namespace Application.Core.Managers
{
    public class ItemManager
    {

        public static string ShowEquipFeatures(Player chr, Equip equip)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            if (!equip.SourceTemplate.IsUpgradeable())
            {
                return "";
            }

            var eqpName = chr.Client.CurrentCulture.GetItemName(equip.getItemId());
            var eqpInfo = equip.ReachedMaxLevel() ? " #e#rMAX LEVEL#k#n" : (" EXP: #e#b" + (int)equip.getItemExp() + "#k#n / " + ExpTable.getEquipExpNeededForLevel(equip.getItemLevel()));

            return "'" + eqpName + "' -> LV: #e#b" + equip.getItemLevel() + "#k#n    " + eqpInfo + "\r\n";
        }
    }
}
