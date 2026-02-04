using Application.Core.Channel.Services;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Shared.Login;

namespace Application.Core.Game.Commands.Gm3;

public class BanCommand : CommandBase
{
    readonly AdminService _adminService;
    public BanCommand(AdminService adminService) : base(3, "ban")
    {
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.YellowMessageI18N(nameof(ClientMessage.BanCommand_Syntax));
            return;
        }

        string ign = paramsValue[0];
        if (int.TryParse(paramsValue[1], out var day))
        {
            TempConversation.Create(c)?.RegisterInput(player.GetMessageByKey(nameof(ClientMessage.BanReason)), async (evt, conversation) =>
            {
                _adminService.Ban(c.OnlinedCharacter, paramsValue[0], (int)BanReason.GM, evt, day);
            });
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.BanCommand_Syntax));
            return;
        }
    }
}
