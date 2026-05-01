using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Life;
using Application.Shared.MapObjects;
using LifeProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.PlayerNPC.Channel.Commands
{
    internal class InvokePlayerNpcClearCommand: IWorldChannelCommand
    {
        public string Name => nameof(InvokePlayerNpcClearCommand);
        RemoveAllPlayerNPCResponse data;

        public InvokePlayerNpcClearCommand(RemoveAllPlayerNPCResponse data)
        {
            this.data = data;
        }

        public void Execute(WorldChannel ctx)
        {
            var mapFactory = ctx.getMapFactory();

            foreach (var mapId in data.MapIdList)
            {
                if (mapFactory.TryGetMap(mapId, out var map))
                {
                    var playerNpcs = map.GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC).OfType<PlayerNpc>().ToList();

                    foreach (var pn in playerNpcs)
                    {
                        map.removeMapObject(pn);
                        map.broadcastMessage(PlayerNPCPacketCreator.RemoveNPCController(pn.getObjectId()));
                        map.broadcastMessage(PlayerNPCPacketCreator.RemovePlayerNPC(pn.getObjectId()));
                    }
                }
            }
        }
    }
}
