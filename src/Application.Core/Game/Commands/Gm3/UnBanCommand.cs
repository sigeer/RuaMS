using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm3;


public class UnBanCommand : CommandBase
{
    readonly AdminService _adminService;
    public UnBanCommand(AdminService adminService) : base(3, "unban")
    {
        Description = "Unban a player.";
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !unban <playername>");
            return;
        }

        _adminService.Unban(c.OnlinedCharacter, paramsValue[0]);
    }
}
