using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class NewYearCardHandlers
    {
        internal class ReceiveHandler : InternalSessionMasterHandler<ProtoService.ReceiveNewYearCardRequest>
        {
            public ReceiveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.ReceiveNewYearCard;

            protected override Task HandleMessage(ProtoService.ReceiveNewYearCardRequest message)
            {
                return _server.NewYearCardManager.ReceiveNewYearCard(message);
            }

            protected override ProtoService.ReceiveNewYearCardRequest Parse(ByteString content) => ProtoService.ReceiveNewYearCardRequest.Parser.ParseFrom(content);
        }

        internal class DiscardHandler : InternalSessionMasterHandler<ProtoService.DiscardNewYearCardRequest>
        {
            public DiscardHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.DiscardNewYearCard;

            protected override Task HandleMessage(ProtoService.DiscardNewYearCardRequest message)
            {
                return _server.NewYearCardManager.DiscardNewYearCard(message);
            }

            protected override ProtoService.DiscardNewYearCardRequest Parse(ByteString content) => ProtoService.DiscardNewYearCardRequest.Parser.ParseFrom(content);
        }
        internal class SendHandler : InternalSessionMasterHandler<ProtoService.SendNewYearCardRequest>
        {
            public SendHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SendNewYearCard;

            protected override Task HandleMessage(ProtoService.SendNewYearCardRequest message)
            {
                return _server.NewYearCardManager.SendNewYearCard(message);
            }

            protected override ProtoService.SendNewYearCardRequest Parse(ByteString content) => ProtoService.SendNewYearCardRequest.Parser.ParseFrom(content);
        }
    }
}
