using Application.Core.Channel.Net.Packets;
using Application.Core.ServerTransports;
using AutoMapper;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class ChatRoomService
    {
        readonly ILogger<ChatRoomService> _logger;
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;

        public ChatRoomService(ILogger<ChatRoomService> logger, IMapper mapper, IChannelServerTransport transport, WorldChannelServer server)
        {
            _logger = logger;
            _mapper = mapper;
            _transport = transport;
            _server = server;
        }

        public void CreateChatRoom(IPlayer chr)
        {
            _transport.SendCreateChatRoom(new Dto.CreateChatRoomRequest { MasterId = chr.Id });
        }


        public void JoinChatRoom(IPlayer chr, int roomId)
        {
            _transport.SendPlayerJoinChatRoom(new Dto.JoinChatRoomRequest { MasterId = chr.Id, RoomId = roomId });
        }

        public void OnPlayerJoinChatRoom(Dto.JoinChatRoomResponse data)
        {
            var code = (JoinChatRoomResult)data.Code;
            if (code == JoinChatRoomResult.Success)
            {
                var newComer = data.Room.Members[data.NewComerPosition - 1];
                foreach (var member in data.Room.Members)
                {
                    if (member == null)
                        continue;
                    var chr = _server.FindPlayerById(member.Character.Id);
                    if (chr != null)
                    {
                        if (chr.Id != newComer.Character.Id)
                        {
                            chr.sendPacket(ChatRoomPacket.addMessengerPlayer(newComer.Character.Name, newComer, data.NewComerPosition, (byte)(newComer.Channel - 1)));
                        }
                    }
                }

                var newComerChr = _server.FindPlayerById(newComer.Character.Id);
                if (newComerChr != null)
                {
                    newComerChr.ChatRoomId = data.Room.RoomId;
                    for (int position = 1; position < data.Room.Members.Count + 1; position++)
                    {
                        var member = data.Room.Members[position - 1];
                        if (member != null)
                        {
                            if (newComerChr.Id != member.Character.Id)
                                newComerChr.sendPacket(ChatRoomPacket.addMessengerPlayer(member.Character.Name, member, position, (byte)(member.Channel - 1)));
                            else
                                newComerChr.sendPacket(ChatRoomPacket.joinMessenger(position));
                        }
                    }
                }
            }
        }

        public void LeftChatRoom(IPlayer chr)
        {
            _transport.SendPlayerLeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = chr.Id });
        }

        public void OnPlayerLeaveChatRoom(Dto.LeaveChatRoomResponse data)
        {
            foreach (var member in data.Room.Members)
            {
                var chr = _server.FindPlayerById(member.Character.Id);
                if (chr != null && chr.IsOnlined)
                {
                    chr.sendPacket(PacketCreator.removeMessengerPlayer(data.LeftPosition));
                }
            }
        }

        public void SendMessage(IPlayer chr, string text)
        {
            _transport.SendChatRoomMesage(new Dto.SendChatRoomMessageRequest { MasterId = chr.Id, Text = text });
        }

        public void OnReceiveMessage(Dto.SendChatRoomMessageResponse data)
        {
            foreach (var member in data.Members)
            {
                var chr = _server.FindPlayerById(member);
                if (chr != null && chr.IsOnlined)
                {
                    chr.sendPacket(PacketCreator.messengerChat(data.Text));
                }
            }
        }
    }
}
