namespace constants.inventory;

/**
 * @author The Spookster (The Real Spookster)
 */
public class EquipSlot : EnumClass
{

    public static readonly EquipSlot HAT = new EquipSlot("Cp", -1);
    public static readonly EquipSlot SPECIAL_HAT = new EquipSlot("HrCp", -1);
    public static readonly EquipSlot FACE_ACCESSORY = new EquipSlot("Af", -2);
    public static readonly EquipSlot EYE_ACCESSORY = new EquipSlot("Ay", -3);
    public static readonly EquipSlot EARRINGS = new EquipSlot("Ae", -4);
    public static readonly EquipSlot TOP = new EquipSlot("Ma", -5);
    public static readonly EquipSlot OVERALL = new EquipSlot("MaPn", -5);
    public static readonly EquipSlot PANTS = new EquipSlot("Pn", -6);
    public static readonly EquipSlot SHOES = new EquipSlot("So", -7);
    public static readonly EquipSlot GLOVES = new EquipSlot("GlGw", -8);
    public static readonly EquipSlot CASH_GLOVES = new EquipSlot("Gv", -8);
    public static readonly EquipSlot CAPE = new EquipSlot("Sr", -9);
    public static readonly EquipSlot SHIELD = new EquipSlot("Si", -10);
    public static readonly EquipSlot WEAPON = new EquipSlot("Wp", -11);
    public static readonly EquipSlot WEAPON_2 = new EquipSlot("WpSi", -11);
    public static readonly EquipSlot LOW_WEAPON = new EquipSlot("WpSp", -11);
    public static readonly EquipSlot RING = new EquipSlot("Ri", -12, -13, -15, -16);
    public static readonly EquipSlot PENDANT = new EquipSlot("Pe", -17);
    public static readonly EquipSlot TAMED_MOB = new EquipSlot("Tm", -18);
    public static readonly EquipSlot SADDLE = new EquipSlot("Sd", -19);
    public static readonly EquipSlot MEDAL = new EquipSlot("Me", -49);
    public static readonly EquipSlot BELT = new EquipSlot("Be", -50);
    public static readonly EquipSlot PET_EQUIP = new EquipSlot();

    private string? text;
    private int[] allowed;

    EquipSlot()
    {
        allowed = new int[] { };
    }

    private EquipSlot(string wz_name, params int[] alloweds) : this()
    {
        this.text = wz_name;
        allowed = alloweds;
    }

    public string? getName()
    {
        return text;
    }

    public bool isAllowed(int slot, bool cash)
    {
        if (slot < 0)
        {
            if (allowed != null)
            {
                foreach (int allow in allowed)
                {
                    int condition = cash ? allow - 100 : allow;
                    if (slot == condition)
                    {
                        return true;
                    }
                }
            }
        }
        return cash && slot < 0;
    }

    public static EquipSlot getFromTextSlot(string? slot)
    {
        if (!string.IsNullOrEmpty(slot))
        {
            foreach (EquipSlot c in values<EquipSlot>())
            {
                if (c.getName() == slot)
                {
                    return c;
                }
            }
        }
        return PET_EQUIP;
    }
}
