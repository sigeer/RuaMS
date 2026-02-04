using Application.Core.Channel.Net.Packets;
using DueyDto;

namespace Application.Core.Channel.Commands.Duey
{
    internal class InvokeDueyCreateCallbackCommand : IWorldChannelCommand
    {
        CreatePackageBroadcast data;

        public InvokeDueyCreateCallbackCommand(CreatePackageBroadcast data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var receiver = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Package.ReceiverId);
            if (receiver != null)
            {
                receiver.sendPacket(DueyPacketCreator.sendDueyParcelReceived(data.Package.SenderName, data.Package.Type));
            }
        }
    }
}
