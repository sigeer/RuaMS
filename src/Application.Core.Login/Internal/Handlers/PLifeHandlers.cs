using Application.Shared.Message;
using Google.Protobuf;
using LifeProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class PLifeHandlers
    {
        internal class CreateHandler : InternalSessionMasterHandler<CreatePLifeRequest>
        {
            public CreateHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.CreatePLife;

            protected override Task HandleMessage(CreatePLifeRequest message)
            {
                return _server.ResourceDataManager.CreatePLife(message);
            }

            protected override CreatePLifeRequest Parse(ByteString content) => CreatePLifeRequest.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionMasterHandler<RemovePLifeRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemovePLife;

            protected override Task HandleMessage(RemovePLifeRequest message)
            {
                return _server.ResourceDataManager.RemovePLife(message);
            }

            protected override RemovePLifeRequest Parse(ByteString content) => RemovePLifeRequest.Parser.ParseFrom(content);
        }
    }
}
