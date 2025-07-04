using Application.Core.Channel.DataProviders;
using Application.Core.EF.Entities.Gachapons;
using Application.Core.Game.Gachapon;

namespace Application.Core.Managers
{
    public class GachaponManager
    {
        public static List<GachaponPool> GetGachaponType()
        {
            return GachaponStorage.Instance.GetGachaponType();
        }

        public static string[] GetLootInfo()
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            var allGachaponType = GachaponStorage.Instance.GetGachaponType();
            string[] strList = new string[allGachaponType.Count + 1];

            string menuStr = "";
            int j = 0;
            foreach (var gacha in allGachaponType)
            {
                menuStr += "#L" + j + "#" + gacha.Name + "#l\r\n";
                j++;

                string str = "";
                foreach (var chance in GachaponStorage.Instance.GetPoolLevelList(gacha.Id))
                {
                    var gachaItems = GachaponStorage.Instance.GetItems(gacha.Id, chance.Level);

                    if (gachaItems.Count > 0)
                    {
                        str += ("  #rTier " + chance.Level + "#k:\r\n");
                        foreach (var item in gachaItems)
                        {
                            var itemName = ii.getName(item.ItemId);
                            if (itemName == null)
                            {
                                itemName = "MISSING NAME #" + item.ItemId;
                            }

                            str += ("    " + itemName + "\r\n");
                        }

                        str += "\r\n";
                    }
                }
                str += "\r\n";

                strList[j] = str;
            }
            strList[0] = menuStr;

            return strList;
        }
    }
}
