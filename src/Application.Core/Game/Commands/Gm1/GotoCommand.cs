using server.maps;

namespace Application.Core.Game.Commands.Gm1;



public class GotoCommand : CommandBase
{
    // "goto" command for players
    static Dictionary<string, int> GOTO_TOWNS = new Dictionary<string, int>()
    {
        {"southperry", 60000 },
        {"amherst", 1000000},
        {"henesys", 100000000},
        {"ellinia", 101000000},
        {"perion", 102000000},
        {"kerning", 103000000},
        {"lith", 104000000},
        {"sleepywood", 105040300},
        {"florina", 110000000},
        {"nautilus", 120000000},
        {"ereve", 130000000},
        {"rien", 140000000},
        {"orbis", 200000000},
        {"happy", 209000000},
        {"elnath", 211000000},
        {"ludi", 220000000},
        {"aqua", 230000000},
        {"leafre", 240000000},
        {"mulung", 250000000},
        {"herb", 251000000},
        {"omega", 221000000},
        {"korean", 222000000},
        {"ellin", 300000000},
        {"nlc", 600000000},
        {"showa", 801000000},
        {"shrine", 800000000},
        {"ariant", 260000000},
        {"magatia", 261000000},
        {"singapore", 540000000},
        {"quay", 541000000},
        {"kampung", 551000000},
        {"amoria", 680000000},
        {"temple", 270000100},
        {"square", 103040000},
        {"neo", 240070000},
        {"mushking", 106020000}
    };

    // "goto" command for only-GMs
    static Dictionary<string, int> GOTO_AREAS = new Dictionary<string, int>() {
        {"gmmap", 180000000},
{"excavation", 990000000},
{"mushmom", 100000005},
{"griffey", 240020101},
{"manon", 240020401},
{"horseman", 682000001},
{"balrog", 105090900},
{"zakum", 211042300},
{"papu", 220080001},
{"guild", 200000301},
{"skelegon", 240040511},
{"hpq", 100000200},
{"pianus", 230040420},
{"horntail", 240050400},
{"pinkbean", 270050000},
{"keep", 610020006},
{"dojo", 925020001},
{"bosspq", 970030000},
{"fm", 910000000},
 };
    public GotoCommand() : base(1, "goto")
    {
        Description = "Warp to a predefined map.";

        var towns = GOTO_TOWNS.OrderBy(x => x.Value).ToList();

        try
        {
            // thanks shavit for noticing goto areas getting loaded from wz needlessly only for the name retrieval

            foreach (var e in towns)
            {
                GOTO_TOWNS_INFO += ("'" + e.Key + "' - #b" + (MapFactory.loadPlaceName(e.Value)) + "#k\r\n");
            }

            var areas = GOTO_AREAS.OrderBy(x => x.Value).ToArray();
            foreach (var e in areas)
            {
                GOTO_AREAS_INFO += ("'" + e.Key + "' - #b" + (MapFactory.loadPlaceName(e.Value)) + "#k\r\n");
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());

            GOTO_TOWNS_INFO = "(none)";
            GOTO_AREAS_INFO = "(none)";
        }

    }

    public string GOTO_TOWNS_INFO = "";
    public string GOTO_AREAS_INFO = "";

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            string sendStr = "Syntax: #b@goto <map name>#k. Available areas:\r\n\r\n#rTowns:#k\r\n" + GOTO_TOWNS_INFO;
            if (player.isGM())
            {
                sendStr += ("\r\n#rAreas:#k\r\n" + GOTO_AREAS_INFO);
            }

            player.getAbstractPlayerInteraction().npcTalk(NpcId.SPINEL, sendStr);
            return;
        }

        if (!player.isAlive())
        {
            player.dropMessage(1, "This command cannot be used when you're dead.");
            return;
        }

        if (!player.isGM())
        {
            if (player.getEventInstance() != null || MiniDungeonInfo.isDungeonMap(player.getMapId()) || FieldLimit.CANNOTMIGRATE.check(player.getMap().getFieldLimit()))
            {
                player.dropMessage(1, "This command can not be used in this map.");
                return;
            }
        }

        Dictionary<string, int> gotomaps;
        if (player.isGM())
        {
            gotomaps = new(GOTO_AREAS);     // distinct map registry for GM/users suggested thanks to Vcoc
            gotomaps.putAll(GOTO_TOWNS);  // thanks Halcyon (UltimateMors) for pointing out duplicates on listed entries functionality
        }
        else
        {
            gotomaps = GOTO_TOWNS;
        }

        if (gotomaps.TryGetValue(paramsValue[0], out var map))
        {
            var target = c.getChannelServer().getMapFactory().getMap(map);

            // expedition issue with this command detected thanks to Masterrulax
            Portal targetPortal = target.getRandomPlayerSpawnpoint();
            player.saveLocationOnWarp();
            player.changeMap(target, targetPortal);
        }
        else
        {
            // detailed info on goto available areas suggested thanks to Vcoc
            string sendStr = "Area '#r" + paramsValue[0] + "#k' is not available. Available areas:\r\n\r\n#rTowns:#k" + GOTO_TOWNS_INFO;
            if (player.isGM())
            {
                sendStr += ("\r\n#rAreas:#k\r\n" + GOTO_AREAS_INFO);
            }

            player.getAbstractPlayerInteraction().npcTalk(NpcId.SPINEL, sendStr);
        }
    }
}
