using Application.Core.ServerTransports;
using Application.Shared.Invitations;
using Microsoft.Extensions.Logging;

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

        public Task CreateChatRoom(Player chr)
        {
            return _transport.SendCreateChatRoom(new ProtoService.CreateChatRoomRequest { MasterId = chr.Id });
        }


        public Task JoinChatRoom(Player chr, int roomId)
        {
            return _transport.SendPlayerJoinChatRoom(new ProtoService.JoinChatRoomRequest { MasterId = chr.Id, RoomId = roomId });
        }


        public Task LeftChatRoom(Player chr)
        {
            return _transport.SendPlayerLeaveChatRoom(new ProtoService.LeaveChatRoomRequest { MasterId = chr.Id });
        }

        public Task SendMessage(Player chr, string text)
        {
            return _transport.SendChatRoomMesage(new ProtoService.SendChatRoomMessageRequest { MasterId = chr.Id, Text = text });
        }

        internal Task CreateInvite(Player player, string input)
        {
            return _server.Transport.SendInvitation(new ProtoService.CreateInviteRequest { FromId = player.Id, Type = InviteTypes.Messenger, ToName = input });
        }

        internal Task AnswerInvite(Player player, int roomId, bool v)
        {
            return _server.Transport.AnswerInvitation(new ProtoService.AnswerInviteRequest { MasterId = player.Id, Ok = v, Type = InviteTypes.Messenger, CheckKey = roomId });
        }
    }
}
