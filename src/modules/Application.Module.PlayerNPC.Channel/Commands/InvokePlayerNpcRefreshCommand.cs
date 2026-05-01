using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Life;
using Application.Shared.MapObjects;
using AutoMapper;
using LifeProto;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Module.PlayerNPC.Channel.Commands
{
    internal class InvokePlayerNpcRefreshCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokePlayerNpcRefreshCommand);
        UpdateMapPlayerNPCResponse data;

        public InvokePlayerNpcRefreshCommand(UpdateMapPlayerNPCResponse data)
        {
            this.data = data;
        }

        public void Execute(WorldChannel ctx)
        {
            var _mapper = ctx.NodeService.ServiceProvider.GetRequiredService<IMapper>();
            var updatedList = _mapper.Map<PlayerNpc[]>(data.UpdatedList);
            var newData = _mapper.Map<PlayerNpc>(data.NewData);

            var chr = ctx.Players.getCharacterById(newData.PlayerId);
            if (chr != null)
            {
                chr.dropMessage($"PlayerNpc创建成功");
            }

            var mapFactory = ctx.getMapFactory();
            if (mapFactory.TryGetMap(data.MapId, out var map))
            {
                var playerNpcs =
                    map.GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC).OfType<PlayerNpc>()
                    .OrderBy(x => x.GetSourceId()).ToList();

                foreach (var pn in playerNpcs)
                {
                    map.removeMapObject(pn);
                    map.broadcastMessage(PlayerNPCPacketCreator.RemoveNPCController(pn.getObjectId()));
                    map.broadcastMessage(PlayerNPCPacketCreator.RemovePlayerNPC(pn.getObjectId()));
                }

                foreach (var pn in updatedList)
                {
                    map.addPlayerNPCMapObject(pn);
                    map.broadcastMessage(PlayerNPCPacketCreator.SpawnPlayerNPCController(pn));
                    map.broadcastMessage(PlayerNPCPacketCreator.GetPlayerNPC(pn));
                }
                map.addPlayerNPCMapObject(newData);
                map.broadcastMessage(PlayerNPCPacketCreator.SpawnPlayerNPCController(newData));
                map.broadcastMessage(PlayerNPCPacketCreator.GetPlayerNPC(newData));
            }
        }
    }
}
