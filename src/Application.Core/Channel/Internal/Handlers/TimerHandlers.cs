using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class TimerHandlers
    {
        public class SetTimerHandler : InternalSessionChannelHandler<ProtoModel.SetTimer>
        {
            public SetTimerHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleSetTimer;

            protected override Task HandleMessage(ProtoModel.SetTimer res)
            {
                return _server.PushChannelCommandAsync(new InvokeChannelBroadcastCommand([-1], PacketCreator.getClock(res.Seconds)));
            }

            protected override ProtoModel.SetTimer Parse(ByteString data) => ProtoModel.SetTimer.Parser.ParseFrom(data);
        }

        public class RemoveTimerHandler : InternalSessionChannelHandler<ProtoModel.RemoveTimer>
        {
            public RemoveTimerHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleRemoveTimer;

            protected override Task HandleMessage(ProtoModel.RemoveTimer res)
            {
                return _server.PushChannelCommandAsync(new InvokeChannelBroadcastCommand([-1], PacketCreator.removeClock()));
            }

            protected override ProtoModel.RemoveTimer Parse(ByteString data) => ProtoModel.RemoveTimer.Parser.ParseFrom(data);
        }
    }
}
