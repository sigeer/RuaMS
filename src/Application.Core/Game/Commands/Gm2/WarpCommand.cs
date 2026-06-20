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

    public override async Task Execute(IChannelClient c, string[] paramsValue)
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

                    await TempConversation.CreateScope(c, async ctx =>
                    {
                        var idx = await ctx.AskMenu(messages.ToString());
                        var mapItem = findResult.MatchedItems[idx];
                        if (await ctx.AskYesNo($"你确定要前往地图 {mapItem.Id} - {mapItem.StreetName} - {mapItem.Name}？"))
                        {
                            var target = await mapFactory.getMap(mapItem.Id);
                            if (target == null)
                            {
                                await player.Yellow(nameof(ClientMessage.MapNotFound), mapId.ToString());
                                return;
                            }
                            await player.changeMap(target, target.getRandomPlayerSpawnpoint());
                        }
                    });
                    return;
                }
                else
                {
                    await player.Yellow(nameof(ClientMessage.MapNotFound), paramsValue[0]);
                    return;
                }
            }


            var target = await mapFactory.getMap(mapId);
            if (target == null)
            {
                await player.Yellow(nameof(ClientMessage.MapNotFound), mapId.ToString());
                return;
            }

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
            await player.changeMap(target, portal ?? target.getRandomPlayerSpawnpoint());

        }
        catch (Exception ex)
        {
            log.Warning(ex.ToString());
            await player.Yellow(nameof(ClientMessage.MapNotFound), paramsValue[0]);
        }
    }
}
