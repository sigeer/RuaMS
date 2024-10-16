using constants.game;
using constants.id;
using server.maps;

namespace Application.Core.Game.Commands.Gm1;



public class GotoCommand : CommandBase
{
    public GotoCommand() : base(1, "goto")
    {
        Description = "Warp to a predefined map.";

        var towns = GameConstants.GOTO_TOWNS.OrderBy(x => x.Value).ToList();

        try
        {
            // thanks shavit for noticing goto areas getting loaded from wz needlessly only for the name retrieval

            foreach (var e in towns)
            {
                GOTO_TOWNS_INFO += ("'" + e.Key + "' - #b" + (MapFactory.loadPlaceName(e.Value)) + "#k\r\n");
            }

            var areas = GameConstants.GOTO_AREAS.OrderBy(x => x.Value).ToArray();
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

    public override void Execute(IClient c, string[] paramsValue)
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
            gotomaps = new(GameConstants.GOTO_AREAS);     // distinct map registry for GM/users suggested thanks to Vcoc
            gotomaps.putAll(GameConstants.GOTO_TOWNS);  // thanks Halcyon (UltimateMors) for pointing out duplicates on listed entries functionality
        }
        else
        {
            gotomaps = GameConstants.GOTO_TOWNS;
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
