using Application.Shared.Events;
using Application.Shared.Message;
using Google.Protobuf;
using SyncProto;
using System;
using System.Collections.Generic;
using System.Text;
using SystemProto;

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

            protected override async Task HandleAsync(SyncPlayerRequest message, CancellationToken cancellationToken = default)
            {
                await _server.CharacterManager.Update(message.Data, (SyncCharacterTrigger)message.Trigger);
                if (message.SaveDb)
                    await _server.ServerManager.CommitAllImmediately();
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

            protected override async Task HandleAsync(BatchSyncPlayerRequest message, CancellationToken cancellationToken = default)
            {
                await _server.CharacterManager.BatchUpdate(message.List.ToList());
                if (message.SaveDb)
                    await _server.ServerManager.CommitAllImmediately();
            }

            protected override BatchSyncPlayerRequest Parse(ByteString data)
            {
                return BatchSyncPlayerRequest.Parser.ParseFrom(data);
            }
        }
    }
}
