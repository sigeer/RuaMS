using Application.Core.Login.Commands;
using Application.Shared.Events;
using Application.Shared.Message;
using Google.Protobuf;
using SyncProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class SyncPlayerHandlers
    {
        internal class SingleHandler : InternalSessionMasterHandler<SyncPlayerRequest>
        {
            public SingleHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SyncPlayer;

            protected override void HandleMessage(SyncPlayerRequest message)
            {
                _ = _server.CharacterManager.Update(message.Data, (SyncCharacterTrigger)message.Trigger)
                    .ContinueWith(t =>
                    {
                        if (message.SaveDb)
                        {
                            _server.Post(new CommitDBCommand());
                        }
                    });
            }

            protected override SyncPlayerRequest Parse(ByteString data)
            {
                return SyncPlayerRequest.Parser.ParseFrom(data);
            }
        }

        internal class MultipleHandler : InternalSessionMasterHandler<BatchSyncPlayerRequest>
        {
            public MultipleHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.BatchSyncPlayer;

            protected override void HandleMessage(BatchSyncPlayerRequest message)
            {
                _server.CharacterManager.BatchUpdateOrSave(message.List.ToList(), message.SaveDb);
            }

            protected override BatchSyncPlayerRequest Parse(ByteString data)
            {
                return BatchSyncPlayerRequest.Parser.ParseFrom(data);
            }
        }
    }
}
