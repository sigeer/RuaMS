using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class BroadcastWeddingHandler : InternalSessionChannelHandler<MarriageProto.BroadcastWeddingDto>
    {
        readonly WeddingManager _weddingManager;
        public BroadcastWeddingHandler(WorldChannelServer server, WeddingManager weddingManager) : base(server)
        {
            _weddingManager = weddingManager;
        }

        public override int MessageId => MasterSend.WeddingBroadcast;

        protected override Task HandleAsync(BroadcastWeddingDto res, CancellationToken cancellationToken = default)
        {
            _weddingManager.BroadcastWedding(res);
            return Task.CompletedTask;
        }

        protected override BroadcastWeddingDto Parse(ByteString data) => BroadcastWeddingDto.Parser.ParseFrom(data);
    }
}
