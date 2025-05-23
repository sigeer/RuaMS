using Application.Core.Managers;
using Application.Core.scripting.npc;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class WarpCommand : ParamsCommandBase
{
    public WarpCommand() : base(["<mapid>"], 2, "warp")
    {
        Description = "Warp to a map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        try
        {
            if (!int.TryParse(paramsValue[0], out var mapId))
            {
                var findResult = ResManager.FindMapIdByName(paramsValue[0]);
                if (findResult.BestMatch != null)
                {
                    mapId = findResult.BestMatch.Id;
                }
                else if (findResult.MatchedItems.Count > 0)
                {
                    var messages = new StringBuilder("找到了这些相似项：\r\n");
                    for (int i = 0; i < findResult.MatchedItems.Count; i++)
                    {
                        var item = findResult.MatchedItems[i];
                        messages.Append($"\r\n#L{i}# {item.Id} - {item.StreetName} - {item.Name} #l");
                    }

                    TempConversation.Create(c)?
                        .RegisterSelect(messages.ToString(), (idx, ctx) =>
                        {
                            var mapItem = findResult.MatchedItems[idx];
                            ctx.RegisterYesOrNo($"你确定要前往地图 {mapItem.Id} - {mapItem.StreetName} - {mapItem.Name}？", ctx =>
                            {
                                WarpMapById(c, mapItem.Id);
                                ctx.dispose();
                            });
                        });
                    return;
                }
                else
                {
                    player.yellowMessage("Map ID " + paramsValue[0] + " is invalid.");
                    return;
                }
            }

            WarpMapById(c, mapId);
        }
        catch (Exception ex)
        {
            log.Warning(ex.ToString());
            player.yellowMessage("Map ID " + paramsValue[0] + " is invalid.");
        }
    }

    private void WarpMapById(IChannelClient c, int mapId)
    {
        var player = c.OnlinedCharacter;

        var target = c.getChannelServer().getMapFactory().getMap(mapId);
        if (target == null)
        {
            player.yellowMessage("Map ID " + mapId + " is invalid.");
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
                player.dropMessage(1, "This command cannot be used in this map.");
                return;
            }
        }

        // expedition issue with this command detected thanks to Masterrulax
        player.saveLocationOnWarp();
        player.changeMap(target, target.getRandomPlayerSpawnpoint());
    }
}
