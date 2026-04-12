using Application.Core.Channel.Actor;
using Application.Core.Channel.Commands;
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

            protected override void HandleMessage(JoinChatRoomResponse data)
            {
                if (data.Code != 0)
                {
                    return;
                }
                _server.Broadcast(w =>
                {
                    var code = (JoinChatRoomResult)data.Code;
                    if (code == JoinChatRoomResult.Success)
                    {
                        var newComer = data.Room.Members[data.NewComerPosition];
                        foreach (var member in data.Room.Members)
                        {
                            if (member.PlayerInfo == null)
                                continue;
                            var chr = w.getPlayerStorage().GetCharacterClientById(member.PlayerInfo.Character.Id);
                            if (chr != null)
                            {
                                if (member.PlayerInfo.Character.Id != newComer.PlayerInfo.Character.Id)
                                {
                                    chr.sendPacket(ChatRoomPacket.addMessengerPlayer(newComer.PlayerInfo.Character.Name, newComer.PlayerInfo, data.NewComerPosition, (byte)(newComer.PlayerInfo.Channel - 1)));
                                }
                            }
                        }

                        w.getPlayerStorage().GetCharacterActor(newComer.PlayerInfo.Character.Id)?.Send(m =>
                        {
                            var newComerChr = m.getCharacterById(newComer.PlayerInfo.Character.Id);
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
                        });

                    }
                });
            }

            protected override JoinChatRoomResponse Parse(ByteString data) => JoinChatRoomResponse.Parser.ParseFrom(data);
        }

        public class OnLeaveChatRoom : InternalSessionChannelHandler<LeaveChatRoomResponse>
        {
            public OnLeaveChatRoom(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnLeaveChatRoom;

            protected override void HandleMessage(LeaveChatRoomResponse data)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.LeftPlayerID)?.Send(m =>
                    {
                        var leftPlayer = m.getCharacterById(data.LeftPlayerID);
                        leftPlayer?.ChatRoomId = 0;
                    });

                    foreach (var member in data.Room.Members)
                    {
                        if (member.PlayerInfo == null)
                            continue;

                        var chr = w.getPlayerStorage().GetCharacterClientById(member.PlayerInfo.Character.Id);
                        chr?.sendPacket(PacketCreator.removeMessengerPlayer(data.LeftPosition));
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

            protected override void HandleMessage(SendChatRoomMessageResponse res)
            {
                _server.Broadcast((worldChannel) =>
                {
                    foreach (var member in res.Members)
                    {
                        var actor = worldChannel.getPlayerStorage().GetCharacterActor(member);
                        actor?.Send(map =>
                        {
                            map.getCharacterById(member)?.sendPacket(PacketCreator.messengerChat(res.Text));
                        });
                    }
                });
            }

            protected override SendChatRoomMessageResponse Parse(ByteString data) => SendChatRoomMessageResponse.Parser.ParseFrom(data);
        }
    }
}
