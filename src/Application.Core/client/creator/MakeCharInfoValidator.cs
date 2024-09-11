using provider;
using provider.wz;

namespace client.creator;

public class MakeCharInfoValidator
{
    private static MakeCharInfo charFemale;
    private static MakeCharInfo charMale;
    private static MakeCharInfo orientCharFemale;
    private static MakeCharInfo orientCharMale;
    private static MakeCharInfo premiumCharFemale;
    private static MakeCharInfo premiumCharMale;

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
        return character.getJob() switch
        {
            Job.BEGINNER or Job.WARRIOR or Job.MAGICIAN or Job.BOWMAN or Job.THIEF or Job.PIRATE => character.isMale() ? charMale : charFemale,
            Job.NOBLESSE => character.isMale() ? premiumCharMale : premiumCharFemale,
            Job.LEGEND => character.isMale() ? orientCharMale : orientCharFemale,
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
