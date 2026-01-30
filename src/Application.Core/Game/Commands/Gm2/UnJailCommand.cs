using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class UnJailCommand : CommandBase
{
    readonly AdminService _adminService;
    public UnJailCommand(AdminService adminService) : base(2, "unjail")
    {
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.UnJailCommand_Syntax));
            return;
        }

        _adminService.UnjailPlayer(player, paramsValue[0]);
    }
}
