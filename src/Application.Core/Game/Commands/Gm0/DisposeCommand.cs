using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm0;

public class DisposeCommand : CommandBase
{
    public DisposeCommand() : base(0, "dispose")
    {

    }
    public override async Task Execute(IChannelClient c, string[] paramValues)
    {
        if (c.NPCConversationManager != null)
        {
            await c.NPCConversationManager.DisposeAsync();
        }
        await c.OnlinedCharacter.Pink(nameof(ClientMessage.DisposeCommand_Message1));
    }
}
