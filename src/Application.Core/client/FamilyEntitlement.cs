namespace client;

public class FamilyEntitlement : EnumClass
{
    public static readonly FamilyEntitlement FAMILY_REUINION = new FamilyEntitlement(1, 300, "Family Reunion", "[Target] Me\\n[Effect] Teleport directly to the Family member of your choice.");
    public static readonly FamilyEntitlement SUMMON_FAMILY = new FamilyEntitlement(1, 500, "Summon Family", "[Target] 1 Family member\\n[Effect] Summon a Family member of choice to the map you're in.");
    public static readonly FamilyEntitlement SELF_DROP_1_5 = new FamilyEntitlement(1, 700, "My Drop Rate 1.5x (15 min)", "[Target] Me\\n[Time] 15 min.\\n[Effect] Monster drop rate will be increased #c1.5x#.\\n*  If the Drop Rate event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement SELF_EXP_1_5 = new FamilyEntitlement(1, 800, "My EXP 1.5x (15 min)", "[Target] Me\\n[Time] 15 min.\\n[Effect] EXP earned from hunting will be increased #c1.5x#.\\n* If the EXP event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement FAMILY_BONDING = new FamilyEntitlement(1, 1000, "Family Bonding (30 min)", "[Target] At least 6 Family members online that are below me in the Pedigree\\n[Time] 30 min.\\n[Effect] Monster drop rate and EXP earned will be increased #c2x#. \\n* If the EXP event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement SELF_DROP_2 = new FamilyEntitlement(1, 1200, "My Drop Rate 2x (15 min)", "[Target] Me\\n[Time] 15 min.\\n[Effect] Monster drop rate will be increased #c2x#.\\n* If the Drop Rate event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement SELF_EXP_2 = new FamilyEntitlement(1, 1500, "My EXP 2x (15 min)", "[Target] Me\\n[Time] 15 min.\\n[Effect] EXP earned from hunting will be increased #c2x#.\\n* If the EXP event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement SELF_DROP_2_30MIN = new FamilyEntitlement(1, 2000, "My Drop Rate 2x (30 min)", "[Target] Me\\n[Time] 30 min.\\n[Effect] Monster drop rate will be increased #c2x#.\\n* If the Drop Rate event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement SELF_EXP_2_30MIN = new FamilyEntitlement(1, 2500, "My EXP 2x (30 min)", "[Target] Me\\n[Time] 30 min.\\n[Effect] EXP earned from hunting will be increased #c2x#. \\n* If the EXP event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement PARTY_DROP_2_30MIN = new FamilyEntitlement(1, 4000, "My Party Drop Rate 2x (30 min)", "[Target] My party\\n[Time] 30 min.\\n[Effect] Monster drop rate will be increased #c2x#.\\n* If the Drop Rate event is in progress, this will be nullified.");
    public static readonly FamilyEntitlement PARTY_EXP_2_30MIN = new FamilyEntitlement(1, 5000, "My Party EXP 2x (30 min)", "[Target] My party\\n[Time] 30 min.\\n[Effect] EXP earned from hunting will be increased #c2x#.\\n* If the EXP event is in progress, this will be nullified.");

    private int usageLimit, repCost;
    private string text, description;

    FamilyEntitlement(int usageLimit, int repCost, string text, string description)
    {
        this.usageLimit = usageLimit;
        this.repCost = repCost;
        this.text = text;
        this.description = description;
    }

    public int getUsageLimit()
    {
        return usageLimit;
    }

    public int getRepCost()
    {
        return repCost;
    }

    public string getName()
    {
        return text;
    }

    public string getDescription()
    {
        return description;
    }
}
