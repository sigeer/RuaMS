using LifeProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokePLifeRemoveCommand : IWorldChannelCommand
    {
        LifeProto.RemovePLifeResponse res;

        public InvokePLifeRemoveCommand(RemovePLifeResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            Player? chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.MasterId);

            foreach (var data in res.RemovedItems)
            {
                if (ctx.WorldChannel.getMapFactory().isMapLoaded(data.MapId))
                {
                    var map = ctx.WorldChannel.getMapFactory().getMap(data.MapId);
                    if (data.Type == LifeType.NPC)
                    {
                        map.destroyNPC(data.LifeId);
                    }
                    else if (data.Type == LifeType.Monster)
                    {
                        map.removeMonsterSpawn(data.LifeId, data.X, data.Y);
                    }
                }
            }

            if (chr != null)
            {
                if (res.LifeType == LifeType.NPC)
                {
                    chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pNPC placements.");
                }
                if (res.LifeType == LifeType.Monster)
                {
                    chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pmob placements.");
                }
            }

            ctx.WorldChannel.NodeService.DataService.LoadAllPLife();
        }
    }
}
