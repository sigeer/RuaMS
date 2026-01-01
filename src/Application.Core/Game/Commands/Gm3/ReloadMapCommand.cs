using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadMapCommand : CommandBase
{
    public ReloadMapCommand() : base(3, "reloadmap")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var newMap = c.getChannelServer().getMapFactory().resetMap(player.getMapId(), out var oldMap);
        int callerid = c.OnlinedCharacter.getId();

        var characters = player.getMap().getAllPlayers();

        foreach (var chr in characters)
        {
            chr.saveLocationOnWarp();
            chr.changeMap(newMap);
            if (chr.getId() != callerid)
            {
                chr.Notice(nameof(ClientMessage.ReloadMapCommand_Message1));
            }
        }
        newMap.respawn();
        oldMap?.Dispose();
        return Task.CompletedTask;
    }
}
