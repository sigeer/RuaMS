using Application.Shared.Languages;

namespace constants.String;

/**
 * @author Drago (Dragohe4rt)
 */
public class LanguageConstants
{
    public static string[] CPQBlue = new string[3];
    public static string[] CPQError = new string[3];
    public static string[] CPQEntry = new string[3];
    public static string[] CPQFindError = new string[3];
    public static string[] CPQRed = new string[3];
    public static string[] CPQPlayerExit = new string[3];
    public static string[] CPQEntryLobby = new string[3];
    public static string[] CPQPickRoom = new string[3];
    public static string[] CPQExtendTime = new string[3];
    public static string[] CPQLeaderNotFound = new string[3];
    public static string[] CPQChallengeRoomAnswer = new string[3];
    public static string[] CPQChallengeRoomSent = new string[3];
    public static string[] CPQChallengeRoomDenied = new string[3];

    static LanguageConstants()
    {
        int lang;
        lang = (int)LanguageEnum.enUS;
        LanguageConstants.CPQBlue[lang] = "Maple Blue";
        LanguageConstants.CPQRed[lang] = "Maple Red";
        LanguageConstants.CPQPlayerExit[lang] = " left the Carnival of Monsters.";
        LanguageConstants.CPQExtendTime[lang] = "The time has been extended.";
        LanguageConstants.CPQLeaderNotFound[lang] = "Could not find the Leader.";
        LanguageConstants.CPQError[lang] = "There was a problem. Please re-create a room.";
        LanguageConstants.CPQPickRoom[lang] = "Sign up for the Monster Festival!\r\n";
        LanguageConstants.CPQChallengeRoomAnswer[lang] = "The group is currently facing a challenge.";
        LanguageConstants.CPQChallengeRoomSent[lang] = "A challenge has been sent to the group in the room. Please wait a while.";
        LanguageConstants.CPQChallengeRoomDenied[lang] = "The group in the room canceled your challenge.";
        LanguageConstants.CPQFindError[lang] = "We could not find a group in this room.\r\nProbably the group was scrapped inside the room!";
        LanguageConstants.CPQEntryLobby[lang] = "You will now receive challenges from other groups. If you do not accept a challenge within 3 minutes, you will be taken out.";
        LanguageConstants.CPQEntry[lang] = "You can select \"Summon Monsters\", \"Ability\", or \"Protector\" as your tactic during the Monster Carnival. Use Tab and F1 ~ F12 for quick access!";


    }

    public static string getMessage(Player chr, string[] message)
    {
        return message[chr.Client.Language];
    }
}

