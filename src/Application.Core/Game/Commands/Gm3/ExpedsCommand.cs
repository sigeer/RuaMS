using Application.Core.scripting.npc;
using net.server;
using server.expeditions;

namespace Application.Core.Game.Commands.Gm3;

public class ExpedsCommand : CommandBase
{
    public ExpedsCommand() : base(3, "expeds")
    {
        Description = "Show all ongoing boss expeditions.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (TempConversation.TryCreate(c, out var p))
        {
            p.RegisterTalk(c.CurrentServer.GetExpeditionInfo());
        }
    }
}
