using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm6;

public class ShutdownCommand : CommandBase
{
    readonly AdminService _adminService;
    public ShutdownCommand(AdminService adminService) : base(6, "shutdown")
    {
        _adminService = adminService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ShutdownCommand_Syntax));
            return;
        }


        if (!int.TryParse(paramsValue[0], out var seconds))
        {
            player.YellowMessageI18N(nameof(ClientMessage.ShutdownCommand_Syntax));
            return;
        }

        await _adminService.ShutdownMaster(player, seconds);
    }
}
