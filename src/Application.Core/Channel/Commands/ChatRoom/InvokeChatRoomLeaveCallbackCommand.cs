using Dto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeChatRoomLeaveCallbackCommand : IWorldChannelCommand
    {
        LeaveChatRoomResponse data;

        public InvokeChatRoomLeaveCallbackCommand(LeaveChatRoomResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var leftPlayer = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.LeftPlayerID);
            if (leftPlayer != null)
            {
                leftPlayer.ChatRoomId = 0;
            }

            foreach (var member in data.Room.Members)
            {
                if (member.PlayerInfo == null)
                    continue;

                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.PlayerInfo.Character.Id);
                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.removeMessengerPlayer(data.LeftPosition));
                }
            }
        }
    }
}
