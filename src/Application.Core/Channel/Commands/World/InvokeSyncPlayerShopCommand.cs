using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncPlayerShopCommand : IChannelCommand
    {
        DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest> _data;

        public InvokeSyncPlayerShopCommand(DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest> data)
        {
            _data = data;
        }

        public void Execute(ChannelNodeCommandContext ctx)
        {
            if (ctx.Server.SyncPlayerShopSession == null)
                ctx.Server.SyncPlayerShopSession = ctx.Server.CreateSyncPlayerShopSession();

            if (ctx.Server.SyncPlayerShopSession.CompleteChunk(_data))
            {
                var request = new ItemProto.BatchSyncPlayerShopRequest();
                request.List.AddRange(ctx.Server.SyncPlayerShopSession.Chunks);
                _ = ctx.Server.Transport.BatchSyncPlayerShop(request);
                ctx.Server.SyncPlayerSession = null;
            }

        }
    }
}
