using Application.Core.Login;
using Application.Core.Login.Internal;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;

namespace Application.Module.Marriage.Master.Internal
{
    internal class BreakMarriageHandler : InternalSessionMasterHandler<MarriageProto.BreakMarriageRequest>
    {
        readonly MarriageManager _marriageManager;
        public BreakMarriageHandler(MasterServer server, MarriageManager marriageManager) : base(server)
        {
            _marriageManager = marriageManager;
        }

        public override int MessageId => ChannelSend.BreakMarriage;

        protected override async Task HandleMessage(BreakMarriageRequest res, CancellationToken cancellationToken = default)
        {
            await _marriageManager.BreakMarriage(res);
        }

        protected override BreakMarriageRequest Parse(ByteString data) => BreakMarriageRequest.Parser.ParseFrom(data);
    }
}
