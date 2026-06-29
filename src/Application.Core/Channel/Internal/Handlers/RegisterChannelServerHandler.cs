using Application.Shared.Message;
using Config;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class RegisterChannelServerHandler : InternalSessionChannelHandler<RegisterServerResult>
    {
        public override int MessageId => (int)ChannelRecvCode.RegisterChannel;
        public RegisterChannelServerHandler(WorldChannelServer server) : base(server)
        {
        }

        protected override Task HandleMessage(RegisterServerResult data)
        {
            return _server.HandleServerRegistered(data);
        }

        protected override RegisterServerResult Parse(ByteString content)
        {
            return RegisterServerResult.Parser.ParseFrom(content);
        }
    }
}
