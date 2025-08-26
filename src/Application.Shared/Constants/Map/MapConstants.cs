using System.Text;

namespace Application.Shared.Constants.Map
{
    public class MapConstants
    {
        public static string GetMapDataName(int mapid)
        {
            StringBuilder builder = new StringBuilder();
            if (mapid < 100000000)
            {
                builder.Append(MapleLand.Maple);
            }
            else if (mapid >= 100000000 && mapid < MapId.ORBIS)
            {
                builder.Append(MapleLand.Victoria);
            }
            else if (mapid >= MapId.ORBIS && mapid < MapId.ELLIN_FOREST)
            {
                builder.Append(MapleLand.Ossyria);
            }
            else if (mapid >= MapId.ELLIN_FOREST && mapid < 400000000)
            {
                builder.Append(MapleLand.Elin);
            }
            else if (mapid >= MapId.SINGAPORE && mapid < 560000000)
            {
                builder.Append(MapleLand.Singapore);
            }
            else if (mapid >= MapId.NEW_LEAF_CITY && mapid < 620000000)
            {
                builder.Append(MapleLand.MasteriaGL);
            }
            else if (mapid >= 677000000 && mapid < 677100000)
            {
                builder.Append(MapleLand.Episode1GL);
            }
            else if (mapid >= 670000000 && mapid < 682000000)
            {
                if ((mapid >= 674030000 && mapid < 674040000) || (mapid >= 680100000 && mapid < 680200000))
                {
                    builder.Append(MapleLand.Etc);
                }
                else
                {
                    builder.Append(MapleLand.WeddingGL);
                }
            }
            else if (mapid >= 682000000 && mapid < 683000000)
            {
                builder.Append(MapleLand.HalloweenGL);
            }
            else if (mapid >= 683000000 && mapid < 684000000)
            {
                builder.Append(MapleLand.Event);
            }
            else if (mapid >= MapId.MUSHROOM_SHRINE && mapid < 900000000)
            {
                if ((mapid >= 889100000 && mapid < 889200000))
                {
                    builder.Append(MapleLand.Etc);
                }
                else
                {
                    builder.Append(MapleLand.JP);
                }
            }
            else
            {
                builder.Append(MapleLand.Etc);
            }
            builder.Append("/").Append(mapid);
            return builder.ToString();
        }

        public static bool IsCPQMap(int mapId)
        {
            switch (mapId)
            {
                case 980000101:
                case 980000201:
                case 980000301:
                case 980000401:
                case 980000501:
                case 980000601:

                case 980031100:
                case 980032100:
                case 980033100:
                    return true;
            }
            return false;
        }
    }
}
