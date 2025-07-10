using Application.Core.Client;
using Application.Core.Game.Commands;
using Application.Core.Game.Life;

namespace Application.Module.PlayerNPC.Channel.Commands.Gm6;

public class EraseAllPNpcsCommand : CommandBase
{
    readonly PlayerNPCManager _manager;
    public EraseAllPNpcsCommand(PlayerNPCManager manager) : base(6, "eraseallpnpcs")
    {
        Description = "Remove all player NPCs.";
        _manager = manager;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        _manager.RemoveAllPlayerNPC();
    }
}
