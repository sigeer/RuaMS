using Application.Core.Channel.Commands;
using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeWorldConfigUpdateHandler : InternalSessionChannelHandler<WorldConfig>
    {
        public InvokeWorldConfigUpdateHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.OnWorldConfigUpdate;

        protected override void HandleMessage(WorldConfig res)
        {
            _server.UpdateWorldConfig(res);
            _server.PushChannelCommand(new InvokeWorldConfigUpdateCommand(res));
        }

        protected override WorldConfig Parse(ByteString data) => WorldConfig.Parser.ParseFrom(data);
    }
}
