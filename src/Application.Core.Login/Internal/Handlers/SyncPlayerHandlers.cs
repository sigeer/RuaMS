using Application.Core.Login.Commands;
using Application.Shared.Events;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class SyncPlayerHandlers
    {
        internal class SingleHandler : InternalSessionMasterHandler<ProtoService.SyncPlayerRequest>
        {
            public SingleHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SyncPlayer;

            protected override async Task HandleMessage(ProtoService.SyncPlayerRequest message)
            {
                await _server.CharacterManager.Update(message.Data, (SyncCharacterTrigger)message.Trigger);
                if (message.SaveDb)
                {
                    await _server.Send(new CommitDBCommand());
                }
            }

            protected override ProtoService.SyncPlayerRequest Parse(ByteString data)
            {
                return ProtoService.SyncPlayerRequest.Parser.ParseFrom(data);
            }
        }

        internal class MultipleHandler : InternalSessionMasterHandler<ProtoService.BatchSyncPlayerRequest>
        {
            public MultipleHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.BatchSyncPlayer;

            protected override Task HandleMessage(ProtoService.BatchSyncPlayerRequest message)
            {
                return _server.CharacterManager.BatchUpdateOrSave(message.List.ToList(), message.SaveDb);
            }

            protected override ProtoService.BatchSyncPlayerRequest Parse(ByteString data)
            {
                return ProtoService.BatchSyncPlayerRequest.Parser.ParseFrom(data);
            }
        }
    }
}
