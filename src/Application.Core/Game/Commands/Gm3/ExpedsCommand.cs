using Application.Core.Channel.Services;
using Application.Core.scripting.npc;

namespace Application.Core.Game.Commands.Gm3;

public class ExpedsCommand : CommandBase
{
    readonly AdminService _adminService;
    public ExpedsCommand(AdminService adminService) : base(3, "expeds")
    {
        _adminService = adminService;
        Description = "Show all ongoing boss expeditions.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (TempConversation.TryCreate(c, out var p))
        {
            p.RegisterTalk(_adminService.QueryExpeditionInfo(c.OnlinedCharacter));
        }
    }
}
