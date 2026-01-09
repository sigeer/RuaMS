using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class JailCommand : CommandBase
{
    readonly AdminService _adminService;
    public JailCommand(AdminService adminService) : base(2, "jail")
    {
        _adminService = adminService;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.JailCommand_Syntax));
            return;
        }

        int minutesJailed = 5;
        if (paramsValue.Length >= 2)
        {
            if (!int.TryParse(paramsValue[1], out minutesJailed))
            {
                player.YellowMessageI18N(nameof(ClientMessage.JailCommand_Syntax));
                return;
            }
        }

        await _adminService.JailPlayer(player, paramsValue[0], minutesJailed);
    }
}
