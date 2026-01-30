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

            public override int MessageId => (int)ChannelSendCode.CreateChatRoom;

            protected override void HandleMessage(CreateChatRoomRequest message)
            {
                _ = _server.ChatRoomManager.CreateChatRoom(message);
            }

            protected override CreateChatRoomRequest Parse(ByteString content) => CreateChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberJoinHandler : InternalSessionMasterHandler<Dto.JoinChatRoomRequest>
        {
            public ChatRoomMemberJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.JoinChatRoom;

            protected override void HandleMessage(JoinChatRoomRequest message)
            {
                _ = _server.ChatRoomManager.JoinChatRoom(message);
            }

            protected override JoinChatRoomRequest Parse(ByteString content) => JoinChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberLeaveHandler : InternalSessionMasterHandler<Dto.LeaveChatRoomRequst>
        {
            public ChatRoomMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveChatRoom;

            protected override void HandleMessage(LeaveChatRoomRequst message)
            {
                _ = _server.ChatRoomManager.LeaveChatRoom(message);
            }

            protected override LeaveChatRoomRequst Parse(ByteString content) => LeaveChatRoomRequst.Parser.ParseFrom(content);
        }

        internal class ChatRoomMessageSentHandler : InternalSessionMasterHandler<SendChatRoomMessageRequest>
        {
            public ChatRoomMessageSentHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SendChatRoomMessage;

            protected override void HandleMessage(SendChatRoomMessageRequest message)
            {
                _ = _server.ChatRoomManager.SendMessage(message);
            }

            protected override SendChatRoomMessageRequest Parse(ByteString content) => SendChatRoomMessageRequest.Parser.ParseFrom(content);
        }
    }
}
