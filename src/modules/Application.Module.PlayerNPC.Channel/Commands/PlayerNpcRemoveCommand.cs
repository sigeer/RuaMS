using Application.Core.Client;
using Application.Core.Game.Commands;
using Application.Core.Game.Life;

namespace Application.Module.PlayerNPC.Channel.Commands.Gm4;

public class PlayerNpcRemoveCommand : CommandBase
{
    readonly PlayerNPCManager _manager;
    public PlayerNpcRemoveCommand(PlayerNPCManager manager) : base(4, "playernpcremove")
    {
        Description = "Remove a player NPC.";
        _manager = manager;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !playernpcremove <playername>");
            return Task.CompletedTask;
        }
        _manager.RemovePlayerNPC(paramsValue[0]);
        return Task.CompletedTask;
    }
}
