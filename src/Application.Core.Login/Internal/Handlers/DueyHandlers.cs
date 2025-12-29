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

            public override int MessageId => ChannelSendCode.LoadDueyPackage;

            protected override async Task HandleAsync(GetPlayerDueyPackageRequest message, CancellationToken cancellationToken = default)
            {
                await _server.DueyManager.GetPlayerDueyPackages(message);
            }

            protected override GetPlayerDueyPackageRequest Parse(ByteString content) => GetPlayerDueyPackageRequest.Parser.ParseFrom(content);
        }

        internal class CreateHandler : InternalSessionMasterHandler<CreatePackageRequest>
        {
            public CreateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.CreateDueyPackage;

            protected override async Task HandleAsync(CreatePackageRequest message, CancellationToken cancellationToken = default)
            {
                await _server.DueyManager.CreateDueyPackage(message);
            }

            protected override CreatePackageRequest Parse(ByteString content) => CreatePackageRequest.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionMasterHandler<RemovePackageRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.RemoveDueyPackage;

            protected override async Task HandleAsync(RemovePackageRequest message, CancellationToken cancellationToken = default)
            {
                await _server.DueyManager.RemovePackage(message);
            }

            protected override RemovePackageRequest Parse(ByteString content) => RemovePackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeHandler : InternalSessionMasterHandler<TakeDueyPackageRequest>
        {
            public TakeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.TakeDueyPackage;

            protected override async Task HandleAsync(TakeDueyPackageRequest message, CancellationToken cancellationToken = default)
            {
                await _server.DueyManager.TakeDueyPackage(message);
            }

            protected override TakeDueyPackageRequest Parse(ByteString content) => TakeDueyPackageRequest.Parser.ParseFrom(content);
        }

        internal class TakeCallbackHandler : InternalSessionMasterHandler<TakeDueyPackageCommit>
        {
            public TakeCallbackHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.TakeDueyPackageCallback;

            protected override async Task HandleAsync(TakeDueyPackageCommit message, CancellationToken cancellationToken = default)
            {
                await _server.DueyManager.TakeDueyPackageCommit(message);
            }

            protected override TakeDueyPackageCommit Parse(ByteString content) => TakeDueyPackageCommit.Parser.ParseFrom(content);
        }
    }
}
