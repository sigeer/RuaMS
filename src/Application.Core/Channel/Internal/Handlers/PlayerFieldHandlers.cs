using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using SyncProto;
using XmlWzReader;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class PlayerFieldHandlers
    {
        public class ChangeJob : InternalSessionChannelHandler<PlayerFieldChange>
        {
            public ChangeJob(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerJobChanged;

            protected override Task HandleAsync(PlayerFieldChange res, CancellationToken cancellationToken = default)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerChangeJob(res);
                }
                return Task.CompletedTask;
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeLevel : InternalSessionChannelHandler<PlayerFieldChange>
        {
            public ChangeLevel(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerLevelChanged;

            protected override Task HandleAsync(PlayerFieldChange res, CancellationToken cancellationToken = default)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerLevelUp(res);
                }
                return Task.CompletedTask;
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeServer : InternalSessionChannelHandler<PlayerFieldChange>
        {
            public ChangeServer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerServerChanged;

            protected override Task HandleAsync(PlayerFieldChange res, CancellationToken cancellationToken = default)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerServerChanged(res);
                }

                return Task.CompletedTask;
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }
    }
}
