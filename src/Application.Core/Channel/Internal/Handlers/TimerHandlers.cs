using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class TimerHandlers
    {
        public class SetTimerHandler : InternalSessionChannelHandler<SetTimer>
        {
            public SetTimerHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleSetTimer;

            protected override Task HandleAsync(SetTimer res, CancellationToken cancellationToken = default)
            {
                _server.BroadcastPacket(PacketCreator.getClock(res.Seconds));
                return Task.CompletedTask;
            }

            protected override SetTimer Parse(ByteString data) => SetTimer.Parser.ParseFrom(data);
        }

        public class RemoveTimerHandler : InternalSessionChannelHandler<RemoveTimer>
        {
            public RemoveTimerHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleRemoveTimer;

            protected override Task HandleAsync(RemoveTimer res, CancellationToken cancellationToken = default)
            {
                _server.BroadcastPacket(PacketCreator.removeClock());
                return Task.CompletedTask;
            }

            protected override RemoveTimer Parse(ByteString data) => RemoveTimer.Parser.ParseFrom(data);
        }
    }
}
