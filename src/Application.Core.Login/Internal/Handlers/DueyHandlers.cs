using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class DueyHandlers
    {
        internal class GetHandler : InternalSessionMasterHandler<ProtoService.GetPlayerDueyPackageRequest>
        {
            public GetHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LoadDueyPackage;

            protected override Task HandleMessage(ProtoService.GetPlayerDueyPackageRequest message)
            {
                return _server.DueyManager.GetPlayerDueyPackages(message);
            }

            protected override ProtoService.GetPlayerDueyPackageRequest Parse(ByteString content) => ProtoService.GetPlayerDueyPackageRequest.Parser.ParseFrom(content);
        }


        internal class RemoveHandler : InternalSessionMasterHandler<ProtoService.RemovePackageRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveDueyPackage;

            protected override Task HandleMessage(ProtoService.RemovePackageRequest message)
            {
                return _server.DueyManager.RemovePackage(message);
            }

            protected override ProtoService.RemovePackageRequest Parse(ByteString content) => ProtoService.RemovePackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeHandler : InternalSessionMasterHandler<ProtoService.TakeDueyPackageRequest>
        {
            public TakeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.TakeDueyPackage;

            protected override Task HandleMessage(ProtoService.TakeDueyPackageRequest message)
            {
                return _server.DueyManager.TakeDueyPackage(message);
            }

            protected override ProtoService.TakeDueyPackageRequest Parse(ByteString content) => ProtoService.TakeDueyPackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeCallbackHandler : InternalSessionMasterHandler<ProtoService.TakeDueyPackageCommitRequest>
        {
            public TakeCallbackHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.TakeDueyPackageCallback;

            protected override Task HandleMessage(ProtoService.TakeDueyPackageCommitRequest message)
            {
                return _server.DueyManager.TakeDueyPackageCommit(message);
            }

            protected override ProtoService.TakeDueyPackageCommitRequest Parse(ByteString content) => ProtoService.TakeDueyPackageCommitRequest.Parser.ParseFrom(content);
        }
    }
}
