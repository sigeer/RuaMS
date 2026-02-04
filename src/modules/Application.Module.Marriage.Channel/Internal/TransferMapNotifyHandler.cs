using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Channel.Commands;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class TransferMapNotifyHandler : InternalSessionChannelHandler<MarriageProto.PlayerTransferDto>
    {
        readonly MarriageManager _marriageManager;
        public TransferMapNotifyHandler(WorldChannelServer server, MarriageManager weddingManager) : base(server)
        {
            _marriageManager = weddingManager;
        }

        public override int MessageId => MasterSend.NotifyPartnerWhenTransfer;

        protected override Task HandleMessage(PlayerTransferDto res, CancellationToken cancellationToken = default)
        {
            _server.PushChannelCommand(new InvokeMapTransferCommand(res));
            return Task.CompletedTask;
        }

        protected override PlayerTransferDto Parse(ByteString data) => PlayerTransferDto.Parser.ParseFrom(data);
    }
}
