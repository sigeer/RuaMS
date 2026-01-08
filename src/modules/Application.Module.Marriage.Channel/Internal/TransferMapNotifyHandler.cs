using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Channel.Net;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using Humanizer;
using MarriageProto;
using System;
using System.Collections.Generic;
using System.Text;

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

        protected override Task HandleAsync(PlayerTransferDto res, CancellationToken cancellationToken = default)
        {
            var chr = _server.FindPlayerById(res.ToPlayerId);
            if (chr != null)
            {
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(res.PlayerId, res.MapId));
            }
            return Task.CompletedTask;
        }

        protected override PlayerTransferDto Parse(ByteString data) => PlayerTransferDto.Parser.ParseFrom(data);
    }
}
