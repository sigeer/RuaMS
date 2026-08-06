using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class BuddyHandlers
    {
        internal class BuddyAddHandler : InternalSessionMasterHandler<ProtoService.AddBuddyRequest>
        {
            public BuddyAddHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AddBuddy;

            protected override Task HandleMessage(ProtoService.AddBuddyRequest message)
            {
                return _server.BuddyManager.AddBuddyByName(message);
            }

            protected override ProtoService.AddBuddyRequest Parse(ByteString content) => ProtoService.AddBuddyRequest.Parser.ParseFrom(content);
        }

        internal class BuddyAddByIdHandler : InternalSessionMasterHandler<ProtoService.AddBuddyByIdRequest>
        {
            public BuddyAddByIdHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AddBuddyById;

            protected override Task HandleMessage(ProtoService.AddBuddyByIdRequest message)
            {
                return _server.BuddyManager.AddBuddyById(message);
            }

            protected override ProtoService.AddBuddyByIdRequest Parse(ByteString content) => ProtoService.AddBuddyByIdRequest.Parser.ParseFrom(content);
        }

        internal class BuddyRemoveHandler : InternalSessionMasterHandler<ProtoService.DeleteBuddyRequest>
        {
            public BuddyRemoveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveBuddy;

            protected override Task HandleMessage(ProtoService.DeleteBuddyRequest message)
            {
                return _server.BuddyManager.DeleteBuddy(message);
            }

            protected override ProtoService.DeleteBuddyRequest Parse(ByteString content) => ProtoService.DeleteBuddyRequest.Parser.ParseFrom(content);
        }

        internal class BuddyNoticeHandler : InternalSessionMasterHandler<ProtoModel.SendBuddyNoticeMessageProto>
        {
            public BuddyNoticeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropBuddyMessage;

            protected override Task HandleMessage(ProtoModel.SendBuddyNoticeMessageProto message)
            {
                return _server.BuddyManager.BroadcastNoticeMessage(message);
            }

            protected override ProtoModel.SendBuddyNoticeMessageProto Parse(ByteString content) => ProtoModel.SendBuddyNoticeMessageProto.Parser.ParseFrom(content);
        }

        internal class BuddyLocationHandler : InternalSessionMasterHandler<ProtoService.GetLocationRequest>
        {
            public BuddyLocationHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.GetLocation;

            protected override Task HandleMessage(ProtoService.GetLocationRequest message)
            {
                return _server.BuddyManager.GetLocation(message);
            }

            protected override ProtoService.GetLocationRequest Parse(ByteString content) => ProtoService.GetLocationRequest.Parser.ParseFrom(content);
        }

        internal class WhisperHandler : InternalSessionMasterHandler<ProtoService.SendWhisperMessageRequest>
        {
            public WhisperHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SendWhisper;

            protected override Task HandleMessage(ProtoService.SendWhisperMessageRequest message)
            {
                return _server.BuddyManager.SendWhisper(message);
            }

            protected override ProtoService.SendWhisperMessageRequest Parse(ByteString content) => ProtoService.SendWhisperMessageRequest.Parser.ParseFrom(content);
        }
    }

}
