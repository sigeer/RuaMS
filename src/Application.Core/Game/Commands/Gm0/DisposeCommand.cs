using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm0;

public class DisposeCommand : CommandBase
{
    public DisposeCommand() : base(0, "dispose")
    {

    }
    public override Task Execute(IChannelClient c, string[] paramValues)
    {
        c.NPCConversationManager?.dispose();
        c.sendPacket(PacketCreator.enableActions());
        c.removeClickedNPC();
        c.OnlinedCharacter.MessageI18N(nameof(ClientMessage.DisposeCommand_Message1));
        return Task.CompletedTask;
    }
}
