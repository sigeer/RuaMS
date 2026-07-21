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

        protected override Task HandleMessage(Empty message)
        {
            return _server.Transport.BroadcastMessageN(ChannelRecvCode.SaveAll);
        }
    }
}
