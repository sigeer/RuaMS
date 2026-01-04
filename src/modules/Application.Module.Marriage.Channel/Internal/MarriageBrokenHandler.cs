using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class MarriageBrokenHandler : InternalSessionChannelHandler<MarriageProto.BreakMarriageResponse>
    {
        readonly WeddingManager _weddingManager;
        public MarriageBrokenHandler(WorldChannelServer server, WeddingManager weddingManager) : base(server)
        {
            _weddingManager = weddingManager;
        }

        public override int MessageId => MasterSend.MarriageBroken;

        protected override Task HandleAsync(BreakMarriageResponse res, CancellationToken cancellationToken = default)
        {
            _weddingManager.OnMarriageBroken(res);
            return Task.CompletedTask;
        }

        protected override BreakMarriageResponse Parse(ByteString data) => BreakMarriageResponse.Parser.ParseFrom(data);
    }
}
