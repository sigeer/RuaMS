using client.inventory;

namespace client.creator;




public class MakeCharInfo
{
    private static ILogger log = LogFactory.GetLogger("MakeCharInfo");
    private const string FACE_ID = "0";
    private const string HAIR_ID = "1";
    private const string HAIR_COLOR_ID = "2";
    private const string SKIN_ID = "3";
    private const string TOP_ID = "4";
    private const string BOTTOM_ID = "5";
    private const string SHOE_ID = "6";
    private const string WEAPON_ID = "7";

    private HashSet<int> charFaces = new();
    private HashSet<int> charHairs = new();
    private HashSet<int> charHairColors = new();
    private HashSet<int> charSkins = new();
    private HashSet<int> charTops = new();
    private HashSet<int> charBottoms = new();
    private HashSet<int> charShoes = new();
    private HashSet<int> charWeapons = new();

    public MakeCharInfo(Data charInfoData)
    {
        foreach (Data data in charInfoData.getChildren())
        {
            switch (data.getName())
            {
                case FACE_ID:
                    foreach (Data faceData in data)
                    {
                        charFaces.Add(DataTool.getInt(faceData));
                    }
                    break;
                case HAIR_ID:
                    foreach (Data hairData in data)
                    {
                        charHairs.Add(DataTool.getInt(hairData));
                    }
                    break;
                case HAIR_COLOR_ID:
                    foreach (Data hairColorData in data)
                    {
                        charHairColors.Add(DataTool.getInt(hairColorData));
                    }
                    break;
                case SKIN_ID:
                    foreach (Data skinData in data)
                    {
                        charSkins.Add(DataTool.getInt(skinData));
                    }
                    break;
                case TOP_ID:
                    foreach (Data topData in data)
                    {
                        charTops.Add(DataTool.getInt(topData));
                    }
                    break;
                case BOTTOM_ID:
                    foreach (Data bottomData in data)
                    {
                        charBottoms.Add(DataTool.getInt(bottomData));
                    }
                    break;
                case SHOE_ID:
                    foreach (Data shoeData in data)
                    {
                        charShoes.Add(DataTool.getInt(shoeData));
                    }
                    break;
                case WEAPON_ID:
                    foreach (Data weaponData in data)
                    {
                        charWeapons.Add(DataTool.getInt(weaponData));
                    }
                    break;
                default:
                    log.Error("Unhandled node inside MakeCharInfo.img.xml: '" + data.getName() + "'");
                    break;
            }
        }
    }

    public bool verifyFaceId(int id)
    {
        return this.charFaces.Contains(id);
    }

    public bool verifyHairId(int id)
    {
        if (id % 10 != 0)
        {
            return this.charHairs.Contains(id - (id % 10));
        }
        return this.charHairs.Contains(id);
    }

    public bool verifyHairColorId(int id)
    {
        return this.charHairColors.Contains(id % 10);
    }

    public bool verifySkinId(int id)
    {
        return this.charSkins.Contains(id);
    }

    public bool verifyTopId(int id)
    {
        return this.charTops.Contains(id);
    }

    public bool verifyBottomId(int id)
    {
        return this.charBottoms.Contains(id);
    }

    public bool verifyShoeId(int id)
    {
        return this.charShoes.Contains(id);
    }

    public bool verifyWeaponId(int id)
    {
        return this.charWeapons.Contains(id);
    }

    public bool verifyCharacter(IPlayer character)
    {
        if (!verifyFaceId(character.getFace())) return false;
        if (!verifyHairId(character.getHair())) return false;
        if (!verifyHairColorId(character.getHair())) return false;
        if (!verifySkinId((int)character.getSkinColor())) return false;

        // Here we only verify the equipment if the character that's being created is of type 'Beginner'
        // This is because when the Maple Life A or Maple Life B items are used, the client does not send any data
        // regarding what equipment the character should be wearing (as it's all handled server-side)
        Job characterJob = character.getJob();
        if (characterJob == Job.BEGINNER || characterJob == Job.NOBLESSE || characterJob == Job.LEGEND)
        {
            if (!verifyTopId(character.getInventory(InventoryType.EQUIPPED).getItem(-5)!.getItemId()))
                return false;
            if (!verifyBottomId(character.getInventory(InventoryType.EQUIPPED).getItem(-6)!.getItemId()))
                return false;
            if (!verifyShoeId(character.getInventory(InventoryType.EQUIPPED).getItem(-7)!.getItemId()))
                return false;
            if (!verifyWeaponId(character.getInventory(InventoryType.EQUIPPED).getItem(-11)!.getItemId()))
                return false;
        }

        return true;
    }
}
