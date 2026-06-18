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

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !unban <playername>");
            return;
        }

        _adminService.Unban(c.OnlinedCharacter, paramsValue[0]);
    }
}
