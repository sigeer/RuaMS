using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm3;

public class ExpedsCommand : CommandBase
{
    readonly AdminService _adminService;
    public ExpedsCommand(AdminService adminService) : base(3, "expeds")
    {
        _adminService = adminService;
        Description = "Show all ongoing boss expeditions.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await player.Dialog(_adminService.QueryExpeditionInfo(c.OnlinedCharacter));
    }
}
