using Application.Core.Channel.ServerData;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using System.Text;

namespace Application.Core.Game.Commands.Gm3;

public class WarpMapCommand : CommandBase
{
    readonly WzStringQueryService _wzManager;
    public WarpMapCommand(WzStringQueryService wzManager) : base(3, "warpmap")
    {
        _wzManager = wzManager;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.WarpMapCommand_Syntax));
            return Task.CompletedTask;
        }

        try
        {
            if (!int.TryParse(paramsValue[0], out var mapId))
            {
                var findResult = _wzManager.FindMapIdByName(c, paramsValue[0]);
                if (findResult.BestMatch != null)
                {
                    mapId = findResult.BestMatch.Id;
                }
                else if (findResult.MatchedItems.Count > 0)
                {
                    var messages = new StringBuilder(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.FindSimilarItem)));
                    for (int i = 0; i < findResult.MatchedItems.Count; i++)
                    {
                        var item = findResult.MatchedItems[i];
                        messages.Append($"\r\n#L{i}# {item.Id} - {item.StreetName} - {item.Name} #l");
                    }

                    TempConversation.Create(c, NpcId.MAPLE_ADMINISTRATOR)?
                        .RegisterSelect(messages.ToString(), (idx, ctx) =>
                        {
                            var mapItem = findResult.MatchedItems[idx];
                            ctx.RegisterYesOrNo($"你确定要前往地图 {mapItem.Id} - {mapItem.StreetName} - {mapItem.Name}？", ctx =>
                            {
                                WarpMapById(c, mapItem.Id);
                                ctx.dispose();
                            });
                        });
                    return Task.CompletedTask;
                }
                else
                {
                    player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), paramsValue[0]);
                    return Task.CompletedTask;
                }
            }

            WarpMapById(c, mapId);
        }
        catch (Exception ex)
        {
            log.Warning(ex.ToString());
            player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), paramsValue[0]);
        }
        return Task.CompletedTask;
    }

    private void WarpMapById(IChannelClient c, int mapId)
    {
        var player = c.OnlinedCharacter;
        var target = c.getChannelServer().getMapFactory().getMap(mapId);
        if (target == null)
        {
            player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), mapId.ToString());
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
