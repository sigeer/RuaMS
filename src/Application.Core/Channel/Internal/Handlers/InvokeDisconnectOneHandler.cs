using Application.Core.Channel.Commands;
using Application.Resources.Messages;
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

        protected override void HandleMessage(DisconnectPlayerByNameResponse res)
        {
            if (res.Code != 0)
            {
                _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.MasterId, 5, nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim));
            }
            else
            {
                _server.PushChannelCommand(new InvokePlayerDisconnectCommand(res.TargetId));
            }
        }

        protected override DisconnectPlayerByNameResponse Parse(ByteString data) => DisconnectPlayerByNameResponse.Parser.ParseFrom(data);
    }
}
