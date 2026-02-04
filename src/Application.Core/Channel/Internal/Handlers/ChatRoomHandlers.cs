using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;

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

            protected override void HandleMessage(JoinChatRoomResponse res)
            {
                _server.PushChannelCommand(new InvokeJoinChatRoomCallbackCommand(res));
            }

            protected override JoinChatRoomResponse Parse(ByteString data) => JoinChatRoomResponse.Parser.ParseFrom(data);
        }

        public class OnLeaveChatRoom : InternalSessionChannelHandler<LeaveChatRoomResponse>
        {
            public OnLeaveChatRoom(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnLeaveChatRoom;

            protected override void HandleMessage(LeaveChatRoomResponse res)
            {
                _server.PushChannelCommand(new InvokeChatRoomLeaveCallbackCommand(res));
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
                _server.PushChannelCommand(new InvokeChatRoomMessageCommand(res));
            }

            protected override SendChatRoomMessageResponse Parse(ByteString data) => SendChatRoomMessageResponse.Parser.ParseFrom(data);
        }
    }
}
