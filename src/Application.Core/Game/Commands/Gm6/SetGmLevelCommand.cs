using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm6;

public class SetGmLevelCommand : CommandBase
{
    readonly AdminService _adminService;
    public SetGmLevelCommand(AdminService adminService) : base(6, "setgmlevel")
    {
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.YellowMessageI18N(nameof(ClientMessage.SetGmLevelCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[1], out var newLevel))
        {
            player.YellowMessageI18N(nameof(ClientMessage.SetGmLevelCommand_Syntax));
            return;
        }
        _adminService.SetGmLevel(c.OnlinedCharacter, paramsValue[0], newLevel);
    }
}
