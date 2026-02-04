using Application.Core.Channel.Commands;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class SaveAllHandler : InternalSessionChannelEmptyHandler
    {
        public SaveAllHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.SaveAll;

        protected override void HandleMessage(Empty message)
        {
            _server.PushChannelCommand(new InvokeStartSyncAllPlayerCommand(true));
            _server.SendDropMessage(5, "玩家数据已同步", true);
        }
    }
}
