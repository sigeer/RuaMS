using Application.Shared.Internal;
using Application.Shared.Message;
using BuddyProto;
using Dto;
using Google.Protobuf;
using MessageProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BuddyHandlers
    {
        internal class BuddyAddHandler : InternalSessionMasterHandler<AddBuddyRequest>
        {
            public BuddyAddHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AddBuddy;

            protected override void HandleMessage(AddBuddyRequest message)
            {
                _ = _server.BuddyManager.AddBuddyByName(message);
            }

            protected override AddBuddyRequest Parse(ByteString content) => AddBuddyRequest.Parser.ParseFrom(content);
        }

        internal class BuddyAddByIdHandler : InternalSessionMasterHandler<AddBuddyByIdRequest>
        {
            public BuddyAddByIdHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AddBuddyById;

            protected override void HandleMessage(AddBuddyByIdRequest message)
            {
                _ = _server.BuddyManager.AddBuddyById(message);
            }

            protected override AddBuddyByIdRequest Parse(ByteString content) => AddBuddyByIdRequest.Parser.ParseFrom(content);
        }

        internal class BuddyRemoveHandler : InternalSessionMasterHandler<DeleteBuddyRequest>
        {
            public BuddyRemoveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveBuddy;

            protected override void HandleMessage(DeleteBuddyRequest message)
            {
                _ = _server.BuddyManager.DeleteBuddy(message);
            }

            protected override DeleteBuddyRequest Parse(ByteString content) => DeleteBuddyRequest.Parser.ParseFrom(content);
        }

        internal class BuddyNoticeHandler : InternalSessionMasterHandler<SendBuddyNoticeMessageDto>
        {
            public BuddyNoticeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropBuddyMessage;

            protected override void HandleMessage(SendBuddyNoticeMessageDto message)
            {
                _ = _server.BuddyManager.BroadcastNoticeMessage(message);
            }

            protected override SendBuddyNoticeMessageDto Parse(ByteString content) => SendBuddyNoticeMessageDto.Parser.ParseFrom(content);
        }

        internal class BuddyLocationHandler : InternalSessionMasterHandler<GetLocationRequest>
        {
            public BuddyLocationHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.GetLocation;

            protected override void HandleMessage(GetLocationRequest message)
            {
                _ = _server.BuddyManager.GetLocation(message);
            }

            protected override GetLocationRequest Parse(ByteString content) => GetLocationRequest.Parser.ParseFrom(content);
        }

        internal class WhisperHandler : InternalSessionMasterHandler<SendWhisperMessageRequest>
        {
            public WhisperHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SendWhisper;

            protected override void HandleMessage(SendWhisperMessageRequest message)
            {
                _ = _server.BuddyManager.SendWhisper(message);
            }

            protected override SendWhisperMessageRequest Parse(ByteString content) => SendWhisperMessageRequest.Parser.ParseFrom(content);
        }
    }

}
