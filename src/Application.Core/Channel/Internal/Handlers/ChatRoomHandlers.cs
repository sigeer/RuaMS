using Application.Core.Channel.Net.Packets;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class ChatRoomHandlers
    {
        public class OnJoinChatRoom : InternalSessionChannelHandler<JoinChatRoomResponse>
        {
            public OnJoinChatRoom(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnJoinChatRoom;

            protected override async Task HandleMessage(JoinChatRoomResponse data)
            {
                var code = (JoinChatRoomResult)data.Code;
                if (code == JoinChatRoomResult.Success)
                {
                    var newComer = data.Room.Members[data.NewComerPosition];
                    var allMembers = data.Room.Members.Where(x => x.PlayerInfo != null).Select(x => x.PlayerInfo.Character.Id);
                    await _server.SendToPlayersAsync(allMembers, async chr =>
                    {
                        chr.ChatRoomId = data.Room.RoomId;
                        if (chr.Id != newComer.PlayerInfo.Character.Id)
                        {
                            await chr.SendPacket(ChatRoomPacket.addMessengerPlayer(newComer.PlayerInfo.Character.Name, newComer.PlayerInfo, data.NewComerPosition, (byte)(newComer.PlayerInfo.Channel - 1)));
                        }
                        else
                        {
                            foreach (var member in data.Room.Members)
                            {
                                if (member.PlayerInfo == null)
                                    continue;

                                if (chr.Id != member.PlayerInfo.Character.Id)
                                    await chr.SendPacket(ChatRoomPacket.addMessengerPlayer(member.PlayerInfo.Character.Name, member.PlayerInfo, member.Position, (byte)(member.PlayerInfo.Channel - 1)));
                                else
                                    await chr.SendPacket(ChatRoomPacket.joinMessenger(member.Position));
                            }
                        }
                    });
                }
            }

            protected override JoinChatRoomResponse Parse(ByteString data) => JoinChatRoomResponse.Parser.ParseFrom(data);
        }

        public class OnLeaveChatRoom : InternalSessionChannelHandler<LeaveChatRoomResponse>
        {
            public OnLeaveChatRoom(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnLeaveChatRoom;

            protected override async Task HandleMessage(LeaveChatRoomResponse data)
            {
                var allMembers = data.Room.Members.Where(x => x.PlayerInfo != null).Select(x => x.PlayerInfo.Character.Id);
                await _server.SendToPlayersAsync([data.LeftPlayerID, .. allMembers], async chr =>
                {
                    if (data.LeftPlayerID == chr.Id)
                    {
                        chr.ChatRoomId = 0;
                    }
                    else
                    {
                        await chr.SendPacket(PacketCreator.removeMessengerPlayer(data.LeftPosition));
                    }
                });
            }

            protected override LeaveChatRoomResponse Parse(ByteString data) => LeaveChatRoomResponse.Parser.ParseFrom(data);
        }

        public class ReceiveMessage : InternalSessionChannelHandler<SendChatRoomMessageResponse>
        {
            public ReceiveMessage(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnChatRoomMessageReceived;

            protected override async Task HandleMessage(SendChatRoomMessageResponse res)
            {
                await _server.SendToPlayersAsync(res.Members, async chr =>
                {
                    await chr.SendPacket(PacketCreator.messengerChat(res.Text));
                });
            }

            protected override SendChatRoomMessageResponse Parse(ByteString data) => SendChatRoomMessageResponse.Parser.ParseFrom(data);
        }
    }
}
