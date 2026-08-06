using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class SyncMapHandler : InternalSessionMasterHandler<ProtoModel.MapBatchSyncProto>
    {
        public SyncMapHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.SyncMap;

        protected override Task HandleMessage(ProtoModel.MapBatchSyncProto message)
        {
            return _server.CharacterManager.BatchUpdateMap(message.List.ToList());
        }

        protected override ProtoModel.MapBatchSyncProto Parse(ByteString content) => ProtoModel.MapBatchSyncProto.Parser.ParseFrom(content);
    }
}
