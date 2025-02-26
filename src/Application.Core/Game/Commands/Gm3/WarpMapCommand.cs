using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.Core.scripting.npc;
using constants.id;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.Commands.Gm3;

public class WarpMapCommand : CommandBase
{
    public WarpMapCommand() : base(3, "warpmap")
    {
        Description = "Warp all characters on current map to a new map.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !warpmap <mapid>");
            return;
        }

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
                    var messages = new StringBuilder("找到了这些相似项：");
                    for (int i = 0; i < findResult.MatchedItems.Count; i++)
                    {
                        var item = findResult.MatchedItems[i];
                        messages.Append($"\r\n#L{i}# {item.Id} - {item.StreetName} - {item.Name} #l");
                    }

                    c.NPCConversationManager?.dispose();

                    var tempConversation = new TempConversation(c, NpcId.MAPLE_ADMINISTRATOR);
                    tempConversation.RegisterSelect(messages.ToString(), (idx, ctx) =>
                    {
                        var mapItem = findResult.MatchedItems[idx];
                        ctx.RegisterYesOrNo($"你确定要前往地图 {mapItem.Id} - {mapItem.StreetName} - {mapItem.Name}？", ctx =>
                        {
                            WarpMapById(c, mapItem.Id);
                            ctx.dispose();
                        });
                    });
                    c.NPCConversationManager = tempConversation;
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

    private void WarpMapById(IClient c, int mapId)
    {
        var player = c.OnlinedCharacter;
        var target = c.getChannelServer().getMapFactory().getMap(mapId);
        if (target == null)
        {
            player.yellowMessage("Map ID " + mapId + " is invalid.");
            return;
        }
        var characters = player.getMap().getAllPlayers();

        foreach (var victim in characters)
        {
            victim.saveLocationOnWarp();
            victim.changeMap(target, target.getRandomPlayerSpawnpoint());
        }
    }
}
