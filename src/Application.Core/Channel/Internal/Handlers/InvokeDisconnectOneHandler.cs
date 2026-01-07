using Application.Resources.Messages;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using SystemProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeDisconnectOneHandler : InternalSessionChannelHandler<SystemProto.DisconnectPlayerByNameResponse>
    {
        public InvokeDisconnectOneHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeDisconnectPlayer;

        protected override Task HandleAsync(DisconnectPlayerByNameResponse res, CancellationToken cancellationToken = default)
        {
            var chr = _server.FindPlayerById(res.Request.MasterId);
            if (chr != null)
            {
                if (res.Code != 0)
                    chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                else
                    chr.Client.Disconnect(false);
            }
            return Task.CompletedTask;
        }

        protected override DisconnectPlayerByNameResponse Parse(ByteString data) => DisconnectPlayerByNameResponse.Parser.ParseFrom(data);
    }
}
