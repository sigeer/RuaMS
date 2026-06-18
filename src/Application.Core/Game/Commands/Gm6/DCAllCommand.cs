using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm6;

public class DCAllCommand : CommandBase
{
    readonly AdminService _adminService;
    public DCAllCommand(AdminService adminService) : base(6, "dcall")
    {
        _adminService = adminService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await _adminService.DisconnectAll(c.OnlinedCharacter);

    }
}
