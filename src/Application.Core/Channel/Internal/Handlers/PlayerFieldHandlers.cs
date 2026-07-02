using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using SyncProto;

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

            protected override Task HandleMessage(PlayerFieldChange res)
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

            protected override Task HandleMessage(PlayerFieldChange res)
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

            protected override Task HandleMessage(PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerServerChanged(res);
                }

                return Task.CompletedTask;
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class RemoveDoor : InternalSessionChannelHandler<Dto.RemoveDoorResponse>
        {
            public RemoveDoor(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnDoorRemoved;

            protected override Task HandleMessage(Dto.RemoveDoorResponse res)
            {
                return _server.PushChannelCommandAsync(new InvokeRemoveDoorCommand(res.OwnerId));
            }

            protected override Dto.RemoveDoorResponse Parse(ByteString data) => Dto.RemoveDoorResponse.Parser.ParseFrom(data);
        }
    }
}
