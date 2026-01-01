using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm2;

public class ReachCommand : CommandBase
{
    readonly AdminService _adminService;
    public ReachCommand(AdminService adminService) : base(2, "reach", "follow", "warpto")
    {
        _adminService = adminService;
        Description = "Warp to a player.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !reach <playername>");
            return ;
        }

        await _adminService.WarpPlayerByName(c.OnlinedCharacter, paramsValue[0]);
        return;
    }
}
