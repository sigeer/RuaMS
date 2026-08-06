using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class RegisterChannelServerHandler : InternalSessionChannelHandler<ProtoModel.RegisterServerResultProto>
    {
        public override int MessageId => (int)ChannelRecvCode.RegisterChannel;
        public RegisterChannelServerHandler(WorldChannelServer server) : base(server)
        {
        }

        protected override Task HandleMessage(ProtoModel.RegisterServerResultProto data)
        {
            return _server.HandleServerRegistered(data);
        }

        protected override ProtoModel.RegisterServerResultProto Parse(ByteString content)
        {
            return ProtoModel.RegisterServerResultProto.Parser.ParseFrom(content);
        }
    }
}
