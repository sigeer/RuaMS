/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using client;
using tools;

namespace server.gachapon;

/**
 * @author Alan (SharpAceX)
 */
public class Gachapon
{
    private static ILogger _log = LogFactory.GetLogger("Gachapon");
    private static Gachapon instance = new Gachapon();

    public static Gachapon getInstance()
    {
        return instance;
    }

    public class GachaponType : EnumClass
    {

        public static readonly GachaponType GLOBAL = new(-1, -1, -1, -1, new Global());
        public static readonly GachaponType HENESYS = new(9100100, 90, 8, 2, new Henesys());
        public static readonly GachaponType ELLINIA = new(9100101, 90, 8, 2, new Ellinia());
        public static readonly GachaponType PERION = new(9100102, 90, 8, 2, new Perion());
        public static readonly GachaponType KERNING_CITY = new(9100103, 90, 8, 2, new KerningCity());
        public static readonly GachaponType SLEEPYWOOD = new(9100104, 90, 8, 2, new Sleepywood());
        public static readonly GachaponType MUSHROOM_SHRINE = new(9100105, 90, 8, 2, new MushroomShrine());
        public static readonly GachaponType SHOWA_SPA_MALE = new(9100106, 90, 8, 2, new ShowaSpaMale());
        public static readonly GachaponType SHOWA_SPA_FEMALE = new(9100107, 90, 8, 2, new ShowaSpaFemale());
        public static readonly GachaponType LUDIBRIUM = new(9100108, 90, 8, 2, new Ludibrium());
        public static readonly GachaponType NEW_LEAF_CITY = new(9100109, 90, 8, 2, new NewLeafCity());
        public static readonly GachaponType EL_NATH = new(9100110, 90, 8, 2, new ElNath());
        public static readonly GachaponType NAUTILUS_HARBOR = new(9100117, 90, 8, 2, new NautilusHarbor());

        private static GachaponType[] values = GachaponType.values<GachaponType>();

        private GachaponItems gachapon;
        private int npcId;
        private int common;
        private int uncommon;
        private int rare;

        private GachaponType(int npcid, int c, int u, int r, GachaponItems g)
        {
            this.npcId = npcid;
            this.gachapon = g;
            this.common = c;
            this.uncommon = u;
            this.rare = r;
        }

        public int getTier()
        {
            int chance = Randomizer.nextInt(common + uncommon + rare) + 1;
            if (chance > common + uncommon)
            {
                return 2; //Rare
            }
            else if (chance > common)
            {
                return 1; //Uncommon
            }
            else
            {
                return 0; //Common
            }
        }

        public int[] getItems(int tier)
        {
            return gachapon.getItems(tier);
        }

        public int getItem(int tier)
        {
            int[] gacha = getItems(tier);
            int[] global = GLOBAL.getItems(tier);
            int chance = Randomizer.nextInt(gacha.Length + global.Length);
            return chance < gacha.Length ? gacha[chance] : global[chance - gacha.Length];
        }

        public static GachaponType getByNpcId(int npcId)
        {
            foreach (GachaponType gacha in values)
            {
                if (npcId == gacha.npcId)
                {
                    return gacha;
                }
            }
            return null;
        }

        public static string[] getLootInfo()
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            string[] strList = new string[values.Length + 1];

            string menuStr = "";
            int j = 0;
            foreach (GachaponType gacha in values)
            {
                menuStr += "#L" + j + "#" + gacha.name() + "#l\r\n";
                j++;

                string str = "";
                for (int i = 0; i < 3; i++)
                {
                    int[] gachaItems = gacha.getItems(i);

                    if (gachaItems.Length > 0)
                    {
                        str += ("  #rTier " + i + "#k:\r\n");
                        foreach (int itemid in gachaItems)
                        {
                            string itemName = ii.getName(itemid);
                            if (itemName == null)
                            {
                                itemName = "MISSING NAME #" + itemid;
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

    public GachaponItem process(int npcId)
    {
        GachaponType gacha = GachaponType.getByNpcId(npcId);
        int tier = gacha.getTier();
        int item = gacha.getItem(tier);
        return new GachaponItem(tier, item);
    }

    public class GachaponItem
    {
        private int id;
        private int tier;

        public GachaponItem(int t, int i)
        {
            id = i;
            tier = t;
        }

        public int getTier()
        {
            return tier;
        }

        public int getId()
        {
            return id;
        }
    }

    public static void log(Character player, int itemId, string map)
    {
        string itemName = ItemInformationProvider.getInstance().getName(itemId);
        _log.Information("{CharacterName} got a {ItemName} ({ItemId}) from the {MapName} gachapon.", player.getName(), itemName, itemId, map);
    }
}
