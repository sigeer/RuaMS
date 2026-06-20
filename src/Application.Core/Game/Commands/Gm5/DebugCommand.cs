using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using server.maps;

namespace Application.Core.Game.Commands.Gm5;


public class DebugCommand : CommandBase
{
    private static string[] debugTypes = { "monster", "packet", "portal", "spawnpoint", "pos", "map", "mobsp", "event", "areas", "reactors", "servercoupons", "playercoupons", "marriage", "buff", "" };

    public DebugCommand() : base(5, "debug")
    {
        Description = "Show a debug message.";
    }


    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !debug <type>");
            return;
        }

        switch (paramsValue[0])
        {
            case "type":
            case "help":
                string msgTypes = "Available #bdebug types#k:\r\n\r\n";
                for (int i = 0; i < debugTypes.Length; i++)
                {
                    msgTypes += ("#L" + i + "#" + debugTypes[i] + "#l\r\n");
                }

                await c.getAbstractPlayerInteraction().npcTalk(NpcId.STEWARD, msgTypes);
                break;

            case "monster":
                List<IMapObject> monsters = player.getMap().GetMapObjects(x => x.getType() == MapObjectType.MONSTER);
                foreach (var monstermo in monsters)
                {
                    var monster = (Monster)monstermo;
                    var controller = monster.getController();
                    await player.Pink("Monster ID: " + monster.getId() + " Aggro target: " + ((controller != null) ? controller.getName() + " Has aggro: " + monster.isControllerHasAggro() + " Knowns aggro: " + monster.isControllerKnowsAboutAggro() : "<none>"));
                }
                break;

            case "packet":
                //player.getMap().broadcastMessage(PacketCreator.customPacket(joinStringFrom(params, 1)));
                break;

            case "portal":
                var portal = player.getMap().findClosestPortal(player.getPosition());
                if (portal != null)
                {
                    await player.LightBlue("Closest portal: " + portal.getId() + " '" + portal.getName() + "' Type: " + portal.getType() + " --> toMap: " + portal.getTargetMapId() + " scriptname: '" + portal.getScriptName() + "' state: " + (portal.getPortalState() ? 1 : 0) + ".");
                }
                else
                {
                    await player.LightBlue("There is no portal on this map.");
                }
                break;

            case "spawnpoint":
                var sp = player.getMap().findClosestSpawnpoint(player.getPosition());
                if (sp != null)
                {
                    await player.LightBlue("Closest mob spawn point: " + " Position: x " + sp.getPosition().X + " y " + sp.getPosition().Y + " Spawns mobid: '" + sp.getMonsterId() + "' --> canSpawn: " + !sp.getDenySpawn() + " canSpawnRightNow: " + sp.shouldSpawn() + ".");
                }
                else
                {
                    await player.LightBlue("There is no mob spawn point on this map.");
                }
                break;

            case "pos":
                await player.LightBlue("Current map position: (" + player.getPosition().X + ", " + player.getPosition().Y + ").");
                break;

            case "map":
                await player.LightBlue("Current map id " + player.getMap().getId() + ", event: '" + (player.getMap().getEventInstance()?.getName() ?? "null") + "'; Players: " + player.getMap().getAllPlayers().Count + ", Mobs: " + player.getMap().countMonsters() + ", Reactors: " + player.getMap().countReactors() + ", Items: " + player.getMap().countItems() + ", Objects: " + player.getMap().getMapObjects().Count + ".");
                break;

            case "mobsp":
                await player.getMap().reportMonsterSpawnPoints(player);
                break;

            case "event":
                if (player.getEventInstance() == null)
                {
                    await player.LightBlue("Player currently not in an event.");
                }
                else
                {
                    await player.LightBlue("Current event name: " + player.getEventInstance()!.getName() + ".");
                }
                break;

            case "areas":
                await player.LightBlue("Configured areas on map " + player.getMapId() + ":");

                byte index = 0;
                foreach (Rectangle rect in player.getMap().getAreas())
                {
                    await player.LightBlue("Id: " + index + " -> posX: " + rect.X + " posY: '" + rect.Y + "' dX: " + rect.Width + " dY: " + rect.Height + ".");
                    index++;
                }
                break;

            case "reactors":
                await player.LightBlue("Current reactor states on map " + player.getMapId() + ":");

                foreach (var mmo in player.getMap().getReactors())
                {
                    Reactor mr = (Reactor)mmo;
                    await player.LightBlue("Id: " + mr.getId() + " Oid: " + mr.getObjectId() + " name: '" + mr.getName() + "' -> Type: " + mr.getReactorType() + " State: " + mr.getState() + " Event State: " + mr.getEventState() + " Position: x " + mr.getPosition().X + " y " + mr.getPosition().Y + ".");
                }
                break;

            case "coupons":
                string st = "Currently active PLAYER coupons: ";
                foreach (int i in player.getActiveCoupons())
                {
                    st += (i + " ");
                }

                await player.LightBlue(st);
                break;

            //case "marriage":
            //    c.getChannelServer().WeddingInstance.DebugMarriageStatus();
            //    break;

            case "stats":
                c.OnlinedCharacter.PrintStatsUpdated();
                break;

            case "buff":
                c.OnlinedCharacter.debugListAllBuffs();
                break;

            case "disease":
                await c.OnlinedCharacter.DebugListAllDisease();
                break;
        }
    }
}
