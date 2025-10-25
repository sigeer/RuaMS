using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Module.PlayerNPC.Channel.Models;
using Application.Module.PlayerNPC.Channel.Net;
using Application.Module.PlayerNPC.Common;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Map;
using Application.Shared.Constants.Npc;
using Application.Shared.MapObjects;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using AutoMapper;
using client.inventory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerNPCProto;
using Serilog;
using server.life;
using System.Drawing;
using ZLinq;

namespace Application.Module.PlayerNPC.Channel
{
    public class PlayerNPCManager
    {
        readonly IChannelTransport _transport;
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;
        readonly ILogger<PlayerNPCManager> _logger;
        readonly Configs _config;
        Dictionary<int, List<PlayerNpc>> _cache;

        public PlayerNPCManager(IChannelTransport transport, IMapper mapper, WorldChannelServer server, ILogger<PlayerNPCManager> logger, IOptions<Configs> options)
        {
            _transport = transport;
            _mapper = mapper;
            _server = server;
            _logger = logger;
            _config = options.Value;
            _cache = new(); ;
        }

        public void LoadAllData()
        {
            _cache = _mapper.Map<List<PlayerNpc>>(_transport.GetAllPlayerNPCList(new GetAllPlayerNPCDataRequest()).List).GroupBy(x => x.Map).ToDictionary(x => x.Key, x=>x.ToList());
        }

        public List<PlayerNpc> GetMapPlayerNPCList(int mapId)
        {
            return _cache.GetValueOrDefault(mapId, []);

        }

        public void SpawnPlayerNPCByHonor(IPlayer chr)
        {
            var mapId = GameConstants.getHallOfFameMapid(chr.getJob());
            CreatePlayerNPCInternal(chr.getClient().getChannelServer().getMapFactory().getMap(mapId), null, chr, true);
        }

        public void SpawnPlayerNPCHere(int mapId, Point position, IPlayer chr)
        {
            CreatePlayerNPCInternal(chr.getClient().getChannelServer().getMapFactory().getMap(mapId), position, chr, false);
        }


        private Dictionary<Byte, List<int>> availablePlayerNpcScriptIds = new();
        private int getNextScriptId(byte branch, List<int> usedScriptIds)
        {
            var availablesBranch = availablePlayerNpcScriptIds.GetValueOrDefault(branch);

            if (availablesBranch == null)
            {
                availablesBranch = new(20);
                availablePlayerNpcScriptIds[branch] = availablesBranch;
            }

            if (availablesBranch.Count == 0)
            {
                fetchAvailableScriptIdsFromDb(branch, availablesBranch, usedScriptIds);

                if (availablesBranch.Count == 0)
                {
                    return -1;
                }
            }

            return availablesBranch.remove(availablesBranch.Count - 1);
        }

        private void fetchAvailableScriptIdsFromDb(byte branch, List<int> list, List<int> usedScriptIds)
        {
            try
            {
                int branchLen = (branch < 26) ? 100 : 400;
                int branchSid = NpcId.PLAYER_NPC_BASE + (branch * 100);
                int nextBranchSid = branchSid + branchLen;

                List<int> availables = new(20);

                int j = 0;
                for (int i = branchSid; i < nextBranchSid; i++)
                {
                    if (!usedScriptIds.Contains(i))
                    {
                        if (PlayerNPCFactory.isExistentScriptid(i))
                        {  // thanks Ark, Zein, geno, Ariel, JrCl0wn for noticing client crashes due to use of missing scriptids
                            availables.Add(i);
                            j++;

                            if (j == 20)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;  // after this point no more scriptids expected...
                        }
                    }
                }


                for (int i = availables.Count - 1; i >= 0; i--)
                {
                    list.Add(availables[i]);
                }
            }
            catch (Exception sqle)
            {
                Log.Logger.Error(sqle.ToString());
            }
        }

        Lock proLock = new Lock();


        internal void CreatePlayerNPCInternal(IMap map, Point? pos, IPlayer chr, bool createByHonor)
        {
            lock (proLock)
            {
                int mapId = map.getId();
                byte branch = GameConstants.getHallOfFameBranch(chr.getJob(), mapId);

                int branchLen = (branch < 26) ? 100 : 400;
                int branchSid = NpcId.PLAYER_NPC_BASE + (branch * 100);
                int nextBranchSid = branchSid + branchLen;
                var request = new PlayerNPCProto.CreatePlayerNPCPreRequest
                {
                    MapId = mapId,
                    BranchSidStart = branchSid,
                    BranchSidEnd = nextBranchSid
                };
                var res = _transport.PreCreatePlayerNPC(request);
                if (res.Code == 1)
                    return;

                int scriptId = getNextScriptId(branch, res.UsedScriptIdList.ToList());
                if (scriptId == -1)
                    return;

                IPlayerPositioner? playerPositioner = null;
                if (pos == null)
                {
                    if (GameConstants.isPodiumHallOfFameMap(map.getId()))
                    {
                        var nextStep = res.NextPositionData < 0 ? 1 : res.NextPositionData;
                        playerPositioner = new PlayerNPCPodium(_config, nextStep);

                    }
                    else
                    {
                        var nextStep = res.NextPositionData < 0 ? 0 : res.NextPositionData;
                        playerPositioner = new PlayerNPCPositioner(_config, nextStep);
                    }

                    pos = playerPositioner.GetNextPlayerNpcPosition(map);
                    if (pos == null)
                    {
                        return;
                    }
                }

                if (YamlConfig.config.server.USE_DEBUG)
                {
                    _logger.LogDebug("GOT SID {ScriptId}, POS {Position}", scriptId, pos);
                }

                var createRequest = new CreatePlayerNPCRequest { };
                createRequest.NextStepData = playerPositioner?.NextPositionData ?? -1;
                createRequest.MapId = mapId;

                int jobId = (chr.getJob().getId() / 100) * 100;
                var newData = new PlayerNPCDto()
                {
                    PlayerId = chr.Id,
                    Name = chr.getName(),
                    Hair = chr.getHair(),
                    Face = chr.getFace(),
                    Skin = (int)chr.getSkinColor(),
                    Gender = chr.getGender(),
                    X = pos.Value.X,
                    Cy = pos.Value.Y,
                    MapId = mapId,
                    ScriptId = scriptId,
                    Dir = 1,
                    Fh = map.Footholds.FindBelowFoothold(pos.Value).getId(),
                    Rx0 = pos.Value.X + 50,
                    Rx1 = pos.Value.X - 50,
                    JobId = jobId,
                    IsHonor = createByHonor,
                };


                foreach (Item equip in chr.getInventory(InventoryType.EQUIPPED))
                {
                    int position = Math.Abs(equip.getPosition());
                    if ((position < 12 && position > 0) || (position > 100 && position < 112))
                    {
                        newData.Equips.Add(new PlayerNPCEquip()
                        {
                            ItemId = equip.getItemId(),
                            Position = equip.getPosition()
                        });

                    }
                }
                createRequest.NewData = newData;

                // 可能会刷新已存在的playernpc坐标
                var existed = _mapper.Map<PlayerNPCDto[]>(
                    map.GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC).OfType<PlayerNpc>());
                createRequest.UpdatedList.AddRange(existed);
                _transport.CreatePlayerNPC(createRequest);

            }
        }

        public void OnRefreshMapPlayerNPC(UpdateMapPlayerNPCResponse data)
        {
            var updatedList = _mapper.Map<PlayerNpc[]>(data.UpdatedList);
            var newData = _mapper.Map<PlayerNpc>(data.NewData);
            foreach (var ch in _server.Servers.Values)
            {
                var chr = ch.Players.getCharacterById(newData.PlayerId);
                if (chr != null)
                {
                    chr.dropMessage($"PlayerNpc创建成功");
                }

                var mapFactory = ch.getMapFactory();
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
                        map.broadcastMessage(PlayerNPCPacketCreator.SpawnPlayerNPC(pn));
                        map.broadcastMessage(PlayerNPCPacketCreator.GetPlayerNPC(pn));
                    }
                    map.addPlayerNPCMapObject(newData);
                    map.broadcastMessage(PlayerNPCPacketCreator.SpawnPlayerNPC(newData));
                    map.broadcastMessage(PlayerNPCPacketCreator.GetPlayerNPC(newData));
                }
            }
            LoadAllData();
        }


        public void RemovePlayerNPC(string target)
        {
            _transport.RemovePlayerNPC(new RemovePlayerNPCRequest { TargetName = target });
        }

        public void OnPlayerNPCRemoved(RemovePlayerNPCResponse data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                var mapFactory = ch.getMapFactory();

                foreach (var item in data.List)
                {
                    if (mapFactory.TryGetMap(item.MapId, out var map))
                    {
                        map.removeMapObject(item.ObjectId);
                        map.broadcastMessage(PlayerNPCPacketCreator.RemoveNPCController(item.ObjectId));
                        map.broadcastMessage(PlayerNPCPacketCreator.RemovePlayerNPC(item.ObjectId));
                    }
                }
            }
            LoadAllData();
        }

        public void RemoveAllPlayerNPC()
        {
            _transport.RemoveAllPlayerNPC();
        }

        public void OnPlayerNPCClear(RemoveAllPlayerNPCResponse data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                var mapFactory = ch.getMapFactory();

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
            LoadAllData();
        }
    }
}
