using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Module.Marriage.Channel.Commands;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Internal
{
    internal class MarriageBrokenHandler : InternalSessionChannelHandler<MarriageProto.BreakMarriageResponse>
    {
        readonly WeddingManager _weddingManager;
        readonly MarriageManager _marriageManager;
        public MarriageBrokenHandler(WorldChannelServer server, WeddingManager weddingManager, MarriageManager marriageManager) : base(server)
        {
            _weddingManager = weddingManager;
            _marriageManager = marriageManager;
        }

        public override int MessageId => MasterSend.MarriageBroken;

        protected override Task HandleMessage(BreakMarriageResponse res, CancellationToken cancellationToken = default)
        {
            _marriageManager.RemoveLocalData(res.Request.MasterId, res.MasterPartnerId);

            _server.PushChannelCommand(new InvokeMarriageBrokenCommand(res));
            return Task.CompletedTask;
        }

        protected override BreakMarriageResponse Parse(ByteString data) => BreakMarriageResponse.Parser.ParseFrom(data);
    }
}
