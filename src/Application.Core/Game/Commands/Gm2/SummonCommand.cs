using Application.Core.Channel.Services;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class SummonCommand : CommandBase
{
    readonly AdminService _adminService;
    public SummonCommand(AdminService adminService) : base(2, "warphere", "summon")
    {
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.SummonCommand_Syntax), CurrentCommand);
            return;
        }

        _adminService.SummonPlayerByName(c.OnlinedCharacter, paramsValue[0]);
    }
}
