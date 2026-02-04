using Application.Core.Channel.Commands;
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

            protected override void HandleMessage(SetTimer res)
            {
                _server.PushChannelCommand(new InvokeChannelBroadcastCommand([-1], PacketCreator.getClock(res.Seconds)));
            }

            protected override SetTimer Parse(ByteString data) => SetTimer.Parser.ParseFrom(data);
        }

        public class RemoveTimerHandler : InternalSessionChannelHandler<RemoveTimer>
        {
            public RemoveTimerHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleRemoveTimer;

            protected override void HandleMessage(RemoveTimer res)
            {
                _server.PushChannelCommand(new InvokeChannelBroadcastCommand([-1], PacketCreator.removeClock()));
            }

            protected override RemoveTimer Parse(ByteString data) => RemoveTimer.Parser.ParseFrom(data);
        }
    }
}
