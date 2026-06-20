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

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.BanCommand_Syntax));
            return;
        }

        string ign = paramsValue[0];
        if (int.TryParse(paramsValue[1], out var day))
        {
            await TempConversation.CreateScope(c, async ctx =>
            {
                var reason = await ctx.AskText(player.GetMessageByKey(nameof(ClientMessage.BanReason)));
                await _adminService.Ban(c.OnlinedCharacter.Id, paramsValue[0], (int)BanReason.GM, reason, day);
            });
        }
        else
        {
            await player.Yellow(nameof(ClientMessage.BanCommand_Syntax));
            return;
        }
    }
}
