using Application.Shared.Message;
using Google.Protobuf;
using SystemProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class AdminHandlers
    {
        internal class SetGmLevelHandler : InternalSessionMasterHandler<SetGmLevelRequest>
        {
            public SetGmLevelHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.SetGmLevel;

            protected override async Task HandleAsync(SetGmLevelRequest message, CancellationToken cancellationToken = default)
            {
                await _server.AccountManager.SetGmLevel(message);
            }

            protected override SetGmLevelRequest Parse(ByteString content) => SetGmLevelRequest.Parser.ParseFrom(content);
        }
        internal class BanHandler : InternalSessionMasterHandler<BanRequest>
        {
            public BanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.Ban;

            protected override async Task HandleAsync(BanRequest message, CancellationToken cancellationToken = default)
            {
                await _server.AccountBanManager.Ban(message);
            }

            protected override BanRequest Parse(ByteString content) => BanRequest.Parser.ParseFrom(content);
        }

        internal class UnbanHandler : InternalSessionMasterHandler<UnbanRequest>
        {
            public UnbanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.Ban;

            protected override async Task HandleAsync(UnbanRequest message, CancellationToken cancellationToken = default)
            {
                await _server.AccountBanManager.Unban(message);
            }

            protected override UnbanRequest Parse(ByteString content) => UnbanRequest.Parser.ParseFrom(content);
        }

        internal class WarpPlayerHandler : InternalSessionMasterHandler<WrapPlayerByNameRequest>
        {
            public WarpPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.WarpPlayer;

            protected override async Task HandleAsync(WrapPlayerByNameRequest message, CancellationToken cancellationToken = default)
            {
                await _server.CrossServerService.WarpPlayerByName(message);
            }

            protected override WrapPlayerByNameRequest Parse(ByteString content) => WrapPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SummonPlayerHandler : InternalSessionMasterHandler<SummonPlayerByNameRequest>
        {
            public SummonPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.SummonPlayer;

            protected override async Task HandleAsync(SummonPlayerByNameRequest message, CancellationToken cancellationToken = default)
            {
                await _server.CrossServerService.SummonPlayerByName(message);
            }

            protected override SummonPlayerByNameRequest Parse(ByteString content) => SummonPlayerByNameRequest.Parser.ParseFrom(content);
        }
    }
}
