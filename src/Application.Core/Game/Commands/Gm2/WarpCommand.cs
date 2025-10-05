using Application.Core.Channel.ServerData;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using server.maps;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class WarpCommand : ParamsCommandBase
{
    readonly WzStringQueryService _wzManager;
    public WarpCommand(WzStringQueryService wzManager) : base(["<mapid>"], 2, "warp")
    {
        _wzManager = wzManager;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var mapFactory = c.CurrentServer.getMapFactory();
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
                                WarpMapById(player, mapFactory, mapItem.Id);
                                ctx.dispose();
                            });
                        });
                    return;
                }
                else
                {
                    player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), paramsValue[0]);
                    return;
                }
            }

            WarpMapById(player, mapFactory, mapId);
        }
        catch (Exception ex)
        {
            log.Warning(ex.ToString());
            player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), paramsValue[0]);
        }
    }

    private void WarpMapById(IPlayer admin, MapManager mapManager, int mapId)
    {
        var target = mapManager.getMap(mapId);
        if (target == null)
        {
            admin.YellowMessageI18N(nameof(ClientMessage.MapNotFound), mapId.ToString());
            return;
        }

        admin.changeMap(target, target.getRandomPlayerSpawnpoint());
    }
}
