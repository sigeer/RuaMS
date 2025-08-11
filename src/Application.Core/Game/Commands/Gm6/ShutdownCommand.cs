using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm6;

public class ShutdownCommand : CommandBase
{
    readonly AdminService _adminService;
    public ShutdownCommand(AdminService adminService) : base(6, "shutdown")
    {
        _adminService = adminService;
        Description = "Shut down the server.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !shutdown [<seconds>| -1]");
            return;
        }


        if (!int.TryParse(paramsValue[0], out var seconds))
        {
            player.yellowMessage("Syntax: !shutdown [<seconds>| -1]");
            return;
        }

        _adminService.ShutdownMaster(player, seconds);
    }
}
