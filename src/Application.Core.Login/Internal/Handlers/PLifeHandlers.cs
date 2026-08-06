using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class PLifeHandlers
    {
        internal class CreateHandler : InternalSessionMasterHandler<ProtoService.CreatePLifeRequest>
        {
            public CreateHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.CreatePLife;

            protected override Task HandleMessage(ProtoService.CreatePLifeRequest message)
            {
                return _server.ResourceDataManager.CreatePLife(message);
            }

            protected override ProtoService.CreatePLifeRequest Parse(ByteString content) => ProtoService.CreatePLifeRequest.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionMasterHandler<ProtoService.RemovePLifeRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemovePLife;

            protected override Task HandleMessage(ProtoService.RemovePLifeRequest message)
            {
                return _server.ResourceDataManager.RemovePLife(message);
            }

            protected override ProtoService.RemovePLifeRequest Parse(ByteString content) => ProtoService.RemovePLifeRequest.Parser.ParseFrom(content);
        }
    }
}
