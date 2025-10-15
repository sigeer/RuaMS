namespace client.creator;

public class MakeCharInfoValidator
{
    public static MakeCharInfo charFemale;
    public static MakeCharInfo charMale;
    public static MakeCharInfo orientCharFemale;
    public static MakeCharInfo orientCharMale;
    public static MakeCharInfo premiumCharFemale;
    public static MakeCharInfo premiumCharMale;

    static MakeCharInfoValidator()
    {
        Data data = DataProviderFactory.getDataProvider(WZFiles.ETC).getData("MakeCharInfo.img");
        charFemale = new MakeCharInfo(data.getChildByPath("Info/CharFemale")!);
        charMale = new MakeCharInfo(data.getChildByPath("Info/CharMale")!);
        orientCharFemale = new MakeCharInfo(data.getChildByPath("OrientCharFemale")!);
        orientCharMale = new MakeCharInfo(data.getChildByPath("OrientCharMale")!);
        premiumCharFemale = new MakeCharInfo(data.getChildByPath("PremiumCharFemale")!);
        premiumCharMale = new MakeCharInfo(data.getChildByPath("PremiumCharMale")!);
    }

    private static MakeCharInfo? getMakeCharInfo(IPlayer character)
    {
        if (character.JobModel.Type == JobType.Adventurer)
            return character.isMale() ? charMale : charFemale;


        return character.JobModel.Type switch
        {
            JobType.Adventurer => character.isMale() ? charMale : charFemale,
            JobType.Cygnus => character.isMale() ? premiumCharMale : premiumCharFemale,
            JobType.Legend => character.isMale() ? orientCharMale : orientCharFemale,
            _ => null
        };
    }

    public static bool isNewCharacterValid(IPlayer character)
    {
        var makeCharInfo = getMakeCharInfo(character);
        if (makeCharInfo == null)
            return false;

        return makeCharInfo.verifyCharacter(character);
    }
}
