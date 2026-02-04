using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using SystemProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class SetGmLevelHandler : InternalSessionChannelHandler<SystemProto.SetGmLevelResponse>
    {
        public SetGmLevelHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeSetGmLevel;

        protected override void HandleMessage(SetGmLevelResponse res)
        {
            _server.PushChannelCommand(new InvokeSetGmLevelCommand(res));
        }

        protected override SetGmLevelResponse Parse(ByteString content) => SetGmLevelResponse.Parser.ParseFrom(content);
    }
}
