using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class PlayerFieldHandlers
    {
        public class ChangeJob : InternalSessionChannelHandler<ProtoModel.PlayerFieldChange>
        {
            public ChangeJob(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerJobChanged;

            protected override Task HandleMessage(ProtoModel.PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerChangeJob(res);
                }

                return Task.CompletedTask;
            }

            protected override ProtoModel.PlayerFieldChange Parse(ByteString data) => ProtoModel.PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeLevel : InternalSessionChannelHandler<ProtoModel.PlayerFieldChange>
        {
            public ChangeLevel(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerLevelChanged;

            protected override Task HandleMessage(ProtoModel.PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerLevelUp(res);
                }

                return Task.CompletedTask;
            }

            protected override ProtoModel.PlayerFieldChange Parse(ByteString data) => ProtoModel.PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeServer : InternalSessionChannelHandler<ProtoModel.PlayerFieldChange>
        {
            public ChangeServer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerServerChanged;

            protected override Task HandleMessage(ProtoModel.PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerServerChanged(res);
                }

                return Task.CompletedTask;
            }

            protected override ProtoModel.PlayerFieldChange Parse(ByteString data) => ProtoModel.PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class RemoveDoor : InternalSessionChannelHandler<ProtoService.RemoveDoorResponse>
        {
            public RemoveDoor(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnDoorRemoved;

            protected override Task HandleMessage(ProtoService.RemoveDoorResponse res)
            {
                return _server.PushChannelCommandAsync(new InvokeRemoveDoorCommand(res.OwnerId));
            }

            protected override ProtoService.RemoveDoorResponse Parse(ByteString data) => ProtoService.RemoveDoorResponse.Parser.ParseFrom(data);
        }
    }
}
