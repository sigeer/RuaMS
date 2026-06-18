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
        c.NPCConversationManager?.DisposeAsync();
        await c.SendPacket(PacketCreator.enableActions());
        c.removeClickedNPC();
        await c.OnlinedCharacter.Pink(nameof(ClientMessage.DisposeCommand_Message1));
    }
}
