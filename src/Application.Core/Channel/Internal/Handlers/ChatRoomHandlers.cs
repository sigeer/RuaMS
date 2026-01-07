using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

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

            protected override Task HandleAsync(JoinChatRoomResponse res, CancellationToken cancellationToken = default)
            {
                _server.ChatRoomService.OnPlayerJoinChatRoom(res);
                return Task.CompletedTask;
            }

            protected override JoinChatRoomResponse Parse(ByteString data) => JoinChatRoomResponse.Parser.ParseFrom(data);
        }

        public class OnLeaveChatRoom : InternalSessionChannelHandler<LeaveChatRoomResponse>
        {
            public OnLeaveChatRoom(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnLeaveChatRoom;

            protected override Task HandleAsync(LeaveChatRoomResponse res, CancellationToken cancellationToken = default)
            {
                _server.ChatRoomService.OnPlayerLeaveChatRoom(res);
                return Task.CompletedTask;
            }

            protected override LeaveChatRoomResponse Parse(ByteString data) => LeaveChatRoomResponse.Parser.ParseFrom(data);
        }

        public class ReceiveMessage : InternalSessionChannelHandler<SendChatRoomMessageResponse>
        {
            public ReceiveMessage(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnChatRoomMessageReceived;

            protected override Task HandleAsync(SendChatRoomMessageResponse res, CancellationToken cancellationToken = default)
            {
                _server.ChatRoomService.OnReceiveMessage(res);
                return Task.CompletedTask;
            }

            protected override SendChatRoomMessageResponse Parse(ByteString data) => SendChatRoomMessageResponse.Parser.ParseFrom(data);
        }
    }
}
