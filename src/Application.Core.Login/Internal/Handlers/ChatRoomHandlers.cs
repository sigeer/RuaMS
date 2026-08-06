using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class ChatRoomHandlers
    {

        internal class ChatRoomCreatedHandler : InternalSessionMasterHandler<ProtoService.CreateChatRoomRequest>
        {
            public ChatRoomCreatedHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.CreateChatRoom;

            protected override Task HandleMessage(ProtoService.CreateChatRoomRequest message)
            {
                return _server.ChatRoomManager.CreateChatRoom(message);
            }

            protected override ProtoService.CreateChatRoomRequest Parse(ByteString content) => ProtoService.CreateChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberJoinHandler : InternalSessionMasterHandler<ProtoService.JoinChatRoomRequest>
        {
            public ChatRoomMemberJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.JoinChatRoom;

            protected override Task HandleMessage(ProtoService.JoinChatRoomRequest message)
            {
                return _server.ChatRoomManager.JoinChatRoom(message);
            }

            protected override ProtoService.JoinChatRoomRequest Parse(ByteString content) => ProtoService.JoinChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMemberLeaveHandler : InternalSessionMasterHandler<ProtoService.LeaveChatRoomRequest>
        {
            public ChatRoomMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveChatRoom;

            protected override Task HandleMessage(ProtoService.LeaveChatRoomRequest message)
            {
                return _server.ChatRoomManager.LeaveChatRoom(message);
            }

            protected override ProtoService.LeaveChatRoomRequest Parse(ByteString content) => ProtoService.LeaveChatRoomRequest.Parser.ParseFrom(content);
        }

        internal class ChatRoomMessageSentHandler : InternalSessionMasterHandler<ProtoService.SendChatRoomMessageRequest>
        {
            public ChatRoomMessageSentHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SendChatRoomMessage;

            protected override Task HandleMessage(ProtoService.SendChatRoomMessageRequest message)
            {
                return _server.ChatRoomManager.SendMessage(message);
            }

            protected override ProtoService.SendChatRoomMessageRequest Parse(ByteString content) => ProtoService.SendChatRoomMessageRequest.Parser.ParseFrom(content);
        }
    }
}
