namespace server;

public class MapleLeafLogger
{
    private static ILogger _log = LogFactory.GetLogger("MapleLeafLogger");

    public static void log(Player player, bool gotPrize, string operation)
    {
        string action = gotPrize ? " used a maple leaf to buy " + operation : " redeemed " + operation + " VP for a leaf";
        _log.Information("{CharacterName} {Action}", player.getName(), action);
    }
}
