using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncPlayerShopCommand : IChannelCommand
    {
        public string Name => nameof(InvokeSyncPlayerShopCommand);
        DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest> _data;

        public InvokeSyncPlayerShopCommand(DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest> data)
        {
            _data = data;
        }

        public void Execute(WorldChannelServer ctx)
        {
            if (ctx.SyncPlayerShopSession == null)
                ctx.SyncPlayerShopSession = ctx.CreateSyncPlayerShopSession();

            if (ctx.SyncPlayerShopSession.CompleteChunk(_data))
            {
                var request = new ItemProto.BatchSyncPlayerShopRequest();
                request.List.AddRange(ctx.SyncPlayerShopSession.Chunks);
                _ = ctx.Transport.BatchSyncPlayerShop(request);
                ctx.SyncPlayerSession = null;
            }

        }
    }
}
