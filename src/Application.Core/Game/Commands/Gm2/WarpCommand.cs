using Application.Core.Channel.ServerData;
using Application.Core.Game.Maps;
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
                    
                    var messages = new StringBuilder();
                    messages.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.FindSimilarItem))).Append("\r\n");
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
                                if (TryGetMapModel(player, mapFactory, mapItem.Id, out var target))
                                    player.changeMap(target, target.getRandomPlayerSpawnpoint());
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

            if (TryGetMapModel(player, mapFactory, mapId, out var target))
            {
                Portal? portal = null;
                if (paramsValue.Length == 2 && !string.IsNullOrEmpty(paramsValue[1]))
                {
                    if (int.TryParse(paramsValue[1], out var portalId))
                    {
                        portal = target.getPortal(portalId);
                    }
                    else
                    {
                        portal = target.getPortal(paramsValue[1]);
                    }
                }

                player.changeMap(target, portal ?? target.getRandomPlayerSpawnpoint());
            }
        }
        catch (Exception ex)
        {
            log.Warning(ex.ToString());
            player.YellowMessageI18N(nameof(ClientMessage.MapNotFound), paramsValue[0]);
        }
    }

    bool TryGetMapModel(Player admin, MapManager mapManager, int mapId, out IMap target)
    {
        target = mapManager.getMap(mapId);
        if (target == null)
        {
            admin.YellowMessageI18N(nameof(ClientMessage.MapNotFound), mapId.ToString());
            return false;
        }

        return true;
    }
}
