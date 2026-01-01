using Application.Core.Channel.Services;

namespace Application.Core.Game.Commands.Gm2;

public class DcCommand : CommandBase
{
    readonly AdminService _adminService;
    public DcCommand(AdminService adminService) : base(2, "dc")
    {
        _adminService = adminService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !dc <playername>");
            return ;
        }

        await _adminService.DisconnectPlayerByName(c.OnlinedCharacter, paramsValue[0]);
        return;
    }
}
