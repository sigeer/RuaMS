using Application.Core.Channel.Net.Packets;
using DueyDto;

namespace Application.Core.Channel.Commands.Duey
{
    internal class InvokeDueyNotifyCallbackCommand : IWorldChannelCommand
    {
        DueyNotificationDto data;

        public InvokeDueyNotifyCallbackCommand(DueyNotificationDto data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var receiver = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.ReceiverId);
            if (receiver != null)
            {
                receiver.sendPacket(DueyPacketCreator.sendDueyParcelNotification(data.Type));
            }
        }
    }
}
