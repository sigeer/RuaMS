using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class ChatRoomHandlers
    {

        internal class ChatRoomCreatedHandler : InternalSessionMasterHandler<Dto.CreateChatRoomRequest>
        {
            public ChatRoomCreatedHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.CreateChatRoom;

            protected override async Task HandleAsync(CreateChatRoomRequest message, CancellationToken cancellationToken = default)
            {
                await _server.ChatRoomManager.CreateChatRoom(message);
            }

            protected override CreateChatRoomRequest Parse(ByteString content) => CreateChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberJoinHandler : InternalSessionMasterHandler<Dto.JoinChatRoomRequest>
        {
            public ChatRoomMemberJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.JoinChatRoom;

            protected override async Task HandleAsync(JoinChatRoomRequest message, CancellationToken cancellationToken = default)
            {
                await _server.ChatRoomManager.JoinChatRoom(message);
            }

            protected override JoinChatRoomRequest Parse(ByteString content) => JoinChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberLeaveHandler : InternalSessionMasterHandler<Dto.LeaveChatRoomRequst>
        {
            public ChatRoomMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.LeaveChatRoom;

            protected override async Task HandleAsync(LeaveChatRoomRequst message, CancellationToken cancellationToken = default)
            {
                await _server.ChatRoomManager.LeaveChatRoom(message);
            }

            protected override LeaveChatRoomRequst Parse(ByteString content) => LeaveChatRoomRequst.Parser.ParseFrom(content);
        }

        internal class ChatRoomMessageSentHandler : InternalSessionMasterHandler<SendChatRoomMessageRequest>
        {
            public ChatRoomMessageSentHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.SendChatRoomMessage;

            protected override async Task HandleAsync(SendChatRoomMessageRequest message, CancellationToken cancellationToken = default)
            {
                await _server.ChatRoomManager.SendMessage(message);
            }

            protected override SendChatRoomMessageRequest Parse(ByteString content) => SendChatRoomMessageRequest.Parser.ParseFrom(content);
        }
    }
}
