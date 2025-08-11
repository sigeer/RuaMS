using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm6;

public class SaveAllCommand : CommandBase
{
    readonly AdminService _adminService;
    public SaveAllCommand(AdminService adminService) : base(6, "saveall")
    {
        _adminService = adminService;
        Description = "Save all characters.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        _adminService.SavelAll();
    }
}
