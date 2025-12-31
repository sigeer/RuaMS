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

        protected override Task HandleAsync(MapBatchSyncDto message, CancellationToken cancellationToken = default)
        {
            _server.CharacterManager.BatchUpdateMap(message.List.ToList());
            return Task.CompletedTask;
        }

        protected override MapBatchSyncDto Parse(ByteString content) => MapBatchSyncDto.Parser.ParseFrom(content);
    }
}
