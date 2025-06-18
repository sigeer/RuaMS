using Acornima;
using Application.Core.Channel.Net.Packets;
using Application.Core.ServerTransports;
using AutoMapper;
using AutoMapper.Execution;
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
                var newComer = data.Room.Members[data.NewComerPosition];
                foreach (var member in data.Room.Members)
                {
                    if (member.PlayerInfo == null)
                        continue;
                    var chr = _server.FindPlayerById(member.PlayerInfo.Character.Id);
                    if (chr != null)
                    {
                        if (chr.Id != newComer.PlayerInfo.Character.Id)
                        {
                            chr.sendPacket(ChatRoomPacket.addMessengerPlayer(newComer.PlayerInfo.Character.Name, newComer.PlayerInfo, data.NewComerPosition, (byte)(newComer.PlayerInfo.Channel - 1)));
                        }
                    }
                }

                var newComerChr = _server.FindPlayerById(newComer.PlayerInfo.Character.Id);
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

        public void LeftChatRoom(IPlayer chr)
        {
            _transport.SendPlayerLeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = chr.Id });
        }

        public void OnPlayerLeaveChatRoom(Dto.LeaveChatRoomResponse data)
        {
            var leftPlayer = _server.FindPlayerById(data.LeftPlayerID);
            if (leftPlayer != null)
            {
                leftPlayer.ChatRoomId = 0;
            }    
            foreach (var member in data.Room.Members)
            {
                if (member.PlayerInfo == null)
                    continue;

                var chr = _server.FindPlayerById(member.PlayerInfo.Character.Id);
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
