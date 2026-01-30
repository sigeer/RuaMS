using Application.Core.Channel.Commands;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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

            protected override void HandleMessage(PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerChangeJob(res);
                }
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeLevel : InternalSessionChannelHandler<PlayerFieldChange>
        {
            public ChangeLevel(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerLevelChanged;

            protected override void HandleMessage(PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerLevelUp(res);
                }
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class ChangeServer : InternalSessionChannelHandler<PlayerFieldChange>
        {
            public ChangeServer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlayerServerChanged;

            protected override void HandleMessage(PlayerFieldChange res)
            {
                foreach (var module in _server.Modules)
                {
                    module.OnPlayerServerChanged(res);
                }
            }

            protected override PlayerFieldChange Parse(ByteString data) => PlayerFieldChange.Parser.ParseFrom(data);
        }

        public class RemoveDoor : InternalSessionChannelHandler<Dto.RemoveDoorResponse>
        {
            public RemoveDoor(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnDoorRemoved;

            protected override void HandleMessage(Dto.RemoveDoorResponse res)
            {
                _server.PushChannelCommand(new InvokeRemoveDoorCommand(res.OwnerId));
            }

            protected override Dto.RemoveDoorResponse Parse(ByteString data) => Dto.RemoveDoorResponse.Parser.ParseFrom(data);
        }
    }
}
