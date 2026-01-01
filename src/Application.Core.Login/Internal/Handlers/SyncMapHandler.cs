using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using SyncProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class SyncMapHandler : InternalSessionMasterHandler<MapBatchSyncDto>
    {
        public SyncMapHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => ChannelSendCode.SyncMap;

        protected override async Task HandleAsync(MapBatchSyncDto message, CancellationToken cancellationToken = default)
        {
            await _server.CharacterManager.BatchUpdateMap(message.List.ToList());
        }

        protected override MapBatchSyncDto Parse(ByteString content) => MapBatchSyncDto.Parser.ParseFrom(content);
    }
}
