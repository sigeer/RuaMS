using Application.Core.Channel.Net.Packets;
using Dto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeJoinChatRoomCallbackCommand : IWorldChannelCommand
    {
        JoinChatRoomResponse data;

        public InvokeJoinChatRoomCallbackCommand(JoinChatRoomResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var code = (JoinChatRoomResult)data.Code;
            if (code == JoinChatRoomResult.Success)
            {
                var newComer = data.Room.Members[data.NewComerPosition];
                foreach (var member in data.Room.Members)
                {
                    if (member.PlayerInfo == null)
                        continue;
                    var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.PlayerInfo.Character.Id);
                    if (chr != null)
                    {
                        if (chr.Id != newComer.PlayerInfo.Character.Id)
                        {
                            chr.sendPacket(ChatRoomPacket.addMessengerPlayer(newComer.PlayerInfo.Character.Name, newComer.PlayerInfo, data.NewComerPosition, (byte)(newComer.PlayerInfo.Channel - 1)));
                        }
                    }
                }

                var newComerChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(newComer.PlayerInfo.Character.Id);
                if (newComerChr != null)
                {
                    newComerChr.ChatRoomId = data.Room.RoomId;
                    foreach (var member in data.Room.Members)
                    {
                        if (member.PlayerInfo == null)
                            continue;

                        if (newComerChr.Id != member.PlayerInfo.Character.Id)
                            newComerChr.sendPacket(ChatRoomPacket.addMessengerPlayer(member.PlayerInfo.Character.Name, member.PlayerInfo, member.Position, (byte)(member.PlayerInfo.Channel - 1)));
                        else
                            newComerChr.sendPacket(ChatRoomPacket.joinMessenger(member.Position));
                    }
                }
            }
        }
    }
}
