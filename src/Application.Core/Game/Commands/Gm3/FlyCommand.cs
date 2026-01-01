using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm3;
public class FlyCommand : ParamsCommandBase
{
    readonly AdminService _adminService;
    public FlyCommand(AdminService adminService) : base(["[on|off]"], 3, "fly")
    {
        _adminService = adminService;
        Description = "Enable/disable fly feature.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        _adminService.SetFly(c.OnlinedCharacter, GetParamByIndex(0)?.Equals("on", StringComparison.OrdinalIgnoreCase) ?? false);
        return Task.CompletedTask;
    }
}
