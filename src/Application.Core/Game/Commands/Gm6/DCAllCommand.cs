using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm6;

public class DCAllCommand : CommandBase
{
    readonly AdminService _adminService;
    public DCAllCommand(AdminService adminService) : base(6, "dcall")
    {
        _adminService = adminService;
        Description = "Disconnect all players (online or logged in).";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        _adminService.DisconnectAll(c.OnlinedCharacter);

    }
}
