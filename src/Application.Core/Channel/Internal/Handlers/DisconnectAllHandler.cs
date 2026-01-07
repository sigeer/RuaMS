using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionChannelEmptyHandler
    {
        public DisconnectAllHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DisconnectAll;

        protected override Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            foreach (var chr in _server.PlayerStorage.getAllCharacters())
            {
                if (!chr.isGM())
                {
                    chr.Client.Disconnect(false);
                }
            }
            return Task.CompletedTask;
        }
    }
}
