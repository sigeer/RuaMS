using Dto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeChatRoomMessageCommand : IWorldChannelCommand
    {
        SendChatRoomMessageResponse data;

        public InvokeChatRoomMessageCommand(SendChatRoomMessageResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var member in data.Members)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member);
                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.messengerChat(data.Text));
                }
            }
        }
    }
}
