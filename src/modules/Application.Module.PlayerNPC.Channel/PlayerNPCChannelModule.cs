using Application.Core.Channel;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Module.PlayerNPC.Common;
using Application.Shared.MapObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace Application.Module.PlayerNPC.Channel
{
    public class PlayerNPCChannelModule : AbstractChannelModule, IPlayerNPCService
    {
        readonly PlayerNPCManager _manager;
        readonly Configs _config;
        public PlayerNPCChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger, PlayerNPCManager manager, IOptions<Configs> options) : base(server, logger)
        {
            _manager = manager;
            _config = options.Value;
        }

        public override void Initialize()
        {
            base.Initialize();
            MessageDispatcher.Register<LifeProto.UpdateMapPlayerNPCResponse>(BroadcastMessage.OnMapPlayerNpcUpdate, _manager.OnRefreshMapPlayerNPC);
            MessageDispatcher.Register<LifeProto.RemoveAllPlayerNPCResponse>(BroadcastMessage.OnClearPlayerNpc, _manager.OnPlayerNPCClear);
            MessageDispatcher.Register<LifeProto.RemovePlayerNPCResponse>(BroadcastMessage.OnRemovePlayerNpc, _manager.OnPlayerNPCRemoved);

            _manager.LoadAllData();
        }

        public bool CanSpawnHonor(IMap map, string targetName)
        {
            if (_config.PLAYERNPC_AUTODEPLOY)
                return false;

            var mmoList = map.GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC);
            return !mmoList.Any(x => x.GetName() == targetName);
        }

        public void LoadPlayerNpc(IMap map)
        {
            var list = _manager.GetMapPlayerNPCList(map.Id);
            foreach (var o in list)
            {
                map.addPlayerNPCMapObject(o);
            }
        }

        public void SpawnPlayerNPCByHonor(Player chr)
        {
            _manager.SpawnPlayerNPCByHonor(chr);
        }

        public void SpawnPlayerNPCHere(int mapId, Point position, Player chr)
        {
            _manager.SpawnPlayerNPCHere(mapId, position, chr);
        }

        public override void OnPlayerLevelUp(SyncProto.PlayerFieldChange arg)
        {
            var chr = _server.FindPlayerById(arg.Id);
            if (chr != null)
            {
                if (arg.Level == chr.getMaxClassLevel())
                {
                    if (!chr.isGM())
                    {
                        if (_config.PLAYERNPC_AUTODEPLOY)
                        {
                            SpawnPlayerNPCByHonor(chr);
                        }
                    }
                }
            }

        }
    }
}
