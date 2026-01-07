using Application.Shared.Internal;
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

        protected override async Task HandleAsync(RegisterServerResult data, CancellationToken cancellationToken = default)
        {
            await _server.HandleServerRegistered(data, cancellationToken);
        }

        protected override RegisterServerResult Parse(ByteString content)
        {
            return RegisterServerResult.Parser.ParseFrom(content);
        }
    }
}
