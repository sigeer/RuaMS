using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionChannelEmptyHandler
    {
        public DisconnectAllHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DisconnectAll;

        protected override void HandleMessage(Empty message)
        {
            _server.PushChannelCommand(new InvokeDisconnectAllCommand(false));
        }
    }
}
