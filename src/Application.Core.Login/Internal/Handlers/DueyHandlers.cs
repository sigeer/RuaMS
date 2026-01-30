using Application.Shared.Message;
using DueyDto;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class DueyHandlers
    {
        internal class GetHandler : InternalSessionMasterHandler<GetPlayerDueyPackageRequest>
        {
            public GetHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LoadDueyPackage;

            protected override void HandleMessage(GetPlayerDueyPackageRequest message)
            {
                _ = _server.DueyManager.GetPlayerDueyPackages(message);
            }

            protected override GetPlayerDueyPackageRequest Parse(ByteString content) => GetPlayerDueyPackageRequest.Parser.ParseFrom(content);
        }


        internal class RemoveHandler : InternalSessionMasterHandler<RemovePackageRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.RemoveDueyPackage;

            protected override void HandleMessage(RemovePackageRequest message)
            {
                _ = _server.DueyManager.RemovePackage(message);
            }

            protected override RemovePackageRequest Parse(ByteString content) => RemovePackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeHandler : InternalSessionMasterHandler<TakeDueyPackageRequest>
        {
            public TakeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.TakeDueyPackage;

            protected override void HandleMessage(TakeDueyPackageRequest message)
            {
                _ = _server.DueyManager.TakeDueyPackage(message);
            }

            protected override TakeDueyPackageRequest Parse(ByteString content) => TakeDueyPackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeCallbackHandler : InternalSessionMasterHandler<TakeDueyPackageCommit>
        {
            public TakeCallbackHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.TakeDueyPackageCallback;

            protected override void HandleMessage(TakeDueyPackageCommit message)
            {
                _ = _server.DueyManager.TakeDueyPackageCommit(message);
            }

            protected override TakeDueyPackageCommit Parse(ByteString content) => TakeDueyPackageCommit.Parser.ParseFrom(content);
        }
    }
}
