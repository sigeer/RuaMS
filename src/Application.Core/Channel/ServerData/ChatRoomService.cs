using Application.Core.Channel.Net.Packets;
using Application.Core.ServerTransports;
using Application.Shared.Invitations;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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

        public async Task CreateChatRoom(Player chr)
        {
            await _transport.SendCreateChatRoom(new Dto.CreateChatRoomRequest { MasterId = chr.Id });
        }


        public async Task JoinChatRoom(Player chr, int roomId)
        {
            await _transport.SendPlayerJoinChatRoom(new Dto.JoinChatRoomRequest { MasterId = chr.Id, RoomId = roomId });
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

        public async Task LeftChatRoom(Player chr)
        {
            await _transport.SendPlayerLeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = chr.Id });
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
                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.removeMessengerPlayer(data.LeftPosition));
                }
            }
        }

        public async Task SendMessage(Player chr, string text)
        {
            await _transport.SendChatRoomMesage(new Dto.SendChatRoomMessageRequest { MasterId = chr.Id, Text = text });
        }

        public void OnReceiveMessage(Dto.SendChatRoomMessageResponse data)
        {
            foreach (var member in data.Members)
            {
                var chr = _server.FindPlayerById(member);
                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.messengerChat(data.Text));
                }
            }
        }

        internal async Task CreateInvite(Player player, string input)
        {
           await  _server.Transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = player.Id, Type = InviteTypes.Messenger, ToName = input });
        }

        internal async Task AnswerInvite(Player player, int roomId, bool v)
        {
            await _server.Transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = player.Id, Ok = v, Type = InviteTypes.Messenger, CheckKey = roomId });
        }
    }
}
