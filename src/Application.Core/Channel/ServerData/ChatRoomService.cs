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

        public void CreateChatRoom(Player chr)
        {
            _ = _transport.SendCreateChatRoom(new Dto.CreateChatRoomRequest { MasterId = chr.Id });
        }


        public void JoinChatRoom(Player chr, int roomId)
        {
            _ = _transport.SendPlayerJoinChatRoom(new Dto.JoinChatRoomRequest { MasterId = chr.Id, RoomId = roomId });
        }


        public void LeftChatRoom(Player chr)
        {
            _ = _transport.SendPlayerLeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = chr.Id });
        }

        public void SendMessage(Player chr, string text)
        {
            _ = _transport.SendChatRoomMesage(new Dto.SendChatRoomMessageRequest { MasterId = chr.Id, Text = text });
        }

        internal void CreateInvite(Player player, string input)
        {
            _ = _server.Transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = player.Id, Type = InviteTypes.Messenger, ToName = input });
        }

        internal void AnswerInvite(Player player, int roomId, bool v)
        {
            _ =  _server.Transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = player.Id, Ok = v, Type = InviteTypes.Messenger, CheckKey = roomId });
        }
    }
}
