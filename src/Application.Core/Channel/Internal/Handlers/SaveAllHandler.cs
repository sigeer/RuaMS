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

        public override int MessageId => ChannelRecvCode.SaveAll;

        protected override async Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            _server.DataService.BatchSyncChar(_server.PlayerStorage.getAllCharacters(), true);
            await _server.SendDropGMMessage(5, "玩家数据已同步");
        }
    }
}
