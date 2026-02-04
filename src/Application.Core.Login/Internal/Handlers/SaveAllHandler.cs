using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Internal.Handlers
{
    internal class SaveAllHandler : InternalSessionMasterEmptyHandler
    {
        public SaveAllHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.SaveAll;

        protected override void HandleMessage(Empty message)
        {
            _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.SaveAll);
        }
    }
}
