using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadMapCommand : CommandBase
{
    public ReloadMapCommand() : base(3, "reloadmap")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var newMap = await c.getChannelServer().getMapFactory().resetMap(player.getMapId());
        int callerid = c.OnlinedCharacter.getId();

        var characters = player.getMap().getAllPlayers();

        foreach (var chr in characters)
        {
            chr.saveLocationOnWarp();
            await chr.changeMap(newMap);
            if (chr.getId() != callerid)
            {
                await chr.Notice(nameof(ClientMessage.ReloadMapCommand_Message1));
            }
        }
        await newMap.respawn();
    }
}
