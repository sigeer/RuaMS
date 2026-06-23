/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Channel;
using Application.Core.Channel.Actor;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Life;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.Game.Skills;
using Application.Core.scripting.Events.Instances;
using Application.Resources.Messages;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using Application.Templates.Mob;
using Application.Templates.Npc;
using Application.Utility.Performance;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;
using client.autoban;
using client.inventory;
using client.status;
using net.server.coordinator.world;
using server;
using server.events.gm;
using server.life;
using server.maps;
using System.Threading.Tasks;
using tools;
using ZLinq;


namespace Application.Core.Game.Maps;

public class MapleMap : IMap, INamedInstance
{
    public string InstanceName { get; }

    protected ILogger log;

    private Dictionary<int, IMapObject> mapobjects = new();
    private HashSet<int> selfDestructives = new();
    protected List<SpawnPoint> monsterSpawn = new();
    protected Dictionary<string, AreaBossSpawnPoint> _bossSp = [];
    private AtomicInteger spawnedMonstersOnMap = new AtomicInteger(0);

    private Dictionary<int, Portal> portals;
    private Dictionary<string, int> environment = new();


    private List<Rectangle> areas;
    public FootholdTree Footholds { get; }
    private KeyValuePair<int, int> xLimits;  // caches the min and max x's with available footholds
    private Rectangle mapArea;
    private int mapid;
    private int runningOid = 1000000001;

    private bool docked = false;
    public bool IsPirateDocked { get; private set; }
    public AbstractEventInstanceManager? EventInstanceManager { get; private set; }
    public MapEffect? MapEffect { get; set; }

    private bool dropsOn = true;

    private MonsterAggroCoordinator aggroMonitor;   // aggroMonitor activity in sync with itemMonitor

    public TimeMob? TimeMob { get; set; }
    private bool _allowSummons = true; // All maps should have this true at the beginning
    private Player? mapOwner = null;
    private long mapOwnerLastActivityTime = long.MaxValue;

    // events
    private bool eventstarted = false, _isMuted = false;


    private bool _isOxQuiz = false;
    public OxQuiz? Ox { get; set; }
    float _monsterRate;
    public float MonsterRate
    {
        get => _monsterRate;
        set
        {
            _monsterRate = value;
            UpdateMapActualMobRate();
        }
    }
    public float ActualMonsterRate { get; private set; }
    public int Id { get; }
    public WorldChannel ChannelServer { get; }
    public XiGuai? XiGuai { get; set; }
    public MapTemplate SourceTemplate { get; }
    public CommandLoop<IMap> CommandLoop { get; }
    public bool IsLargeMap { get; }
    public bool UseRangedView => IsLargeMap;
    public MapleMap(MapTemplate mapTemplate, WorldChannel worldChannel, AbstractEventInstanceManager? eim)
    {
        SourceTemplate = mapTemplate;
        Id = mapTemplate.TemplateId;
        this.mapid = mapTemplate.TemplateId;
        ChannelServer = worldChannel;
        this.MonsterRate = 1;
        aggroMonitor = new MonsterAggroCoordinator(this);

        EventInstanceManager = eim;
        var range = new RangeNumberGenerator(Id, 100000000);
        log = LogFactory.GetLogger($"Map/{range}");

        if (EventInstanceManager == null)
            InstanceName = $"{worldChannel.InstanceName}_EventInstance:None_Map:{Id}({GetHashCode()})";
        else
            InstanceName = $"{worldChannel.InstanceName}_EventInstance:{EventInstanceManager.getName()}_Map:{Id}({GetHashCode()})";

        #region portals
        portals = new();
        PortalFactory portalFactory = new PortalFactory();
        foreach (var item in mapTemplate.Portals)
        {
            var portal = portalFactory.makePortal(item.nPortalType, item);
            portals[portal.getId()] = portal;
        }
        #endregion

        #region areas
        areas = new();
        foreach (var item in mapTemplate.Areas)
        {
            areas.Add(new Rectangle(item.X1, item.Y1, item.X2 - item.X1, item.Y2 - item.Y1));
        }
        #endregion

        #region area
        mapArea = mapTemplate.GetMapRectangle();
        #endregion

        #region foothold
        Footholds = FootholdTree.FromTemplate(mapTemplate);
        #endregion

        if (mapTemplate.TimeMob != null)
        {
            TimeMob = new TimeMob(mapTemplate.TimeMob.Id,
                mapTemplate.TimeMob.Message ?? "",
                mapTemplate.TimeMob.StartHour,
                mapTemplate.TimeMob.EndHour);
        }

        ChannelServer.OnWorldMobRateChanged += UpdateMapActualMobRate;

        droppedItems = new(YamlConfig.config.server.ITEM_LIMIT_ON_MAP);
        droppedItems.OnOverWrite += async (s, o) => await makeDisappearItemFromMap(o);

        IsLargeMap = mapArea.Width >= ChannelServer.NodeService.ServerConfig.SystemConfig.ClientWidth * 2
            || mapArea.Height >= ChannelServer.NodeService.ServerConfig.SystemConfig.ClientHeight * 2;

        CommandLoop = new CommandLoop<IMap>(this);
        CommandLoop.Start();
    }

    void UpdateMapActualMobRate()
    {
        ActualMonsterRate = MonsterRate * ChannelServer.WorldMobRate;
    }


    public AbstractEventInstanceManager? getEventInstance()
    {
        return EventInstanceManager;
    }

    public Rectangle getMapArea()
    {
        return mapArea;
    }


    public void toggleDrops()
    {
        this.dropsOn = !dropsOn;
    }


    public int getId()
    {
        return mapid;
    }

    public WorldChannel getChannelServer()
    {
        return ChannelServer;
    }

    public async Task<IMap> getReturnMap()
    {
        if (SourceTemplate.ReturnMap == MapId.NONE)
        {
            return this;
        }
        return await getChannelServer().getMapFactory().getMap(SourceTemplate.ReturnMap);
    }

    public int getReturnMapId() => SourceTemplate.ReturnMap;
    public async Task<IMap> getForcedReturnMap()
    {
        return await getChannelServer().getMapFactory().getMap(SourceTemplate.ForcedReturn);
    }

    public int getForcedReturnId() => SourceTemplate.ForcedReturn;

    public int getTimeLimit()
    {
        return SourceTemplate.TimeLimit;
    }


    bool addMapObject(IMapObject mapobject, bool allocateMabObjectId = true)
    {
        if (allocateMabObjectId)
        {
            int curOID = getUsableOID();

            mapobject.setObjectId(curOID);
        }

        if (this.mapobjects.TryAdd(mapobject.getObjectId(), mapobject))
        {
            if (mapobject is MapItem mapItem)
            {
                droppedItems.Add(mapItem);
            }

            if (mapobject is Player player)
            {
                characters.Add(player.Id, player);
            }

            if (mapobject is Monster mob)
            {
                spawnedMonstersOnMap.incrementAndGet();
            }

            mapobject.OnMounted(this);

            return true;
        }
        return false;
    }

    public async Task<bool> AddMapObject(IMapObject mapobject, Func<IChannelClient, Task>? packetbakery, bool allocateMabObjectId = true)
    {
        if (addMapObject(mapobject, allocateMabObjectId))
        {
            int exceptPlayerId = -1;
            if (mapobject is Player p)
            {
                exceptPlayerId = p.Id;
            }

            foreach (Player chr in getAllPlayers())
            {
                if (chr.Id == exceptPlayerId)
                {
                    continue;
                }

                if (mapobject.IsVisibleForPlayer(chr))
                {
                    await SetPlayerVisibleObject(chr, mapobject, false);

                    if (packetbakery != null)
                    {
                        await packetbakery.Invoke(chr.Client);
                    }
                }
            }
            return true;
        }

        log.Error("MapleMap.AddMapObject Duplicated: {Type}, {ObjectId}", mapobject.getType(), mapobject.getObjectId());
        return false;

    }

    private int getUsableOID()
    {
        int curOid;

        // clashes with playernpc on curOid >= 2147000000, developernpc uses >= 2147483000
        do
        {
            if ((curOid = ++runningOid) >= 2147000000)
            {
                runningOid = curOid = 1000000001;
            }
        } while (mapobjects.ContainsKey(curOid));

        return curOid;
    }

    bool removeMapObject(int objectId)
    {
        if (mapobjects.Remove(objectId, out var mapObj))
        {
            mapObj.OnUnmounted();

            if (mapObj is MapItem mapItem)
            {
                droppedItems.Remove(mapItem);
            }

            if (mapObj is Player player)
            {
                characters.Remove(player.Id);
                _chrVisibleMapObjects.Remove(player);
            }

            if (mapObj is Monster mob)
            {
                spawnedMonstersOnMap.decrementAndGet();
            }

            return true;
        }
        return false;
    }

    public async Task<bool> RemoveMapObject(IMapObject obj, Func<Player, Task>? removePacketAction)
    {
        if (removeMapObject(obj.getObjectId()))
        {
            foreach (var chr in getAllPlayers())
            {
                if (removePacketAction != null)
                {
                    if (IsMapObjectVisibleForPlayerCached(chr, obj))
                    {
                        await removePacketAction.Invoke(chr);
                    }
                }

                await SetPlayerInvisibleObject(chr, obj, false);
            }
            return true;
        }
        return false;
    }


    public void generateMapDropRangeCache()
    {
        var bounds = MapGlobalData.dropBoundsCache.GetValueOrDefault(Id);

        if (bounds != null)
        {
            xLimits = bounds.Value;
        }
        else
        {
            // assuming MINIMAP always have an equal-greater picture representation of the map area (players won't walk beyond the area known by the minimap).
            Point lp = new Point(mapArea.X, mapArea.Y);
            Point rp = new Point(mapArea.X + mapArea.Width, mapArea.Y);
            Point fallback = new Point(mapArea.X + (mapArea.Width / 2), mapArea.Y);

            lp = bsearchDropPos(lp, fallback);  // approximated leftmost fh node position
            rp = bsearchDropPos(rp, fallback);  // approximated rightmost fh node position

            xLimits = new(lp.X + 14, rp.X - 14);
            MapGlobalData.dropBoundsCache.TryAdd(Id, xLimits);
        }
    }

    private Point bsearchDropPos(Point initial, Point fallback)
    {
        Point? res;
        Point? dropPos = null;

        int awayx = fallback.X;
        int homex = initial.X;

        int y = initial.Y - 85;

        do
        {
            int distx = awayx - homex;
            int dx = distx / 2;

            int searchx = homex + dx;
            if ((res = calcPointBelow(new Point(searchx, y))) != null)
            {
                awayx = searchx;
                dropPos = res;
            }
            else
            {
                homex = searchx;
            }
        } while (Math.Abs(homex - awayx) > 5);

        return (dropPos != null) ? dropPos.Value : fallback;
    }

    public Point calcDropPos(Point initial, Point fallback)
    {
        if (initial.X < xLimits.Key)
        {
            initial.X = xLimits.Key;
        }
        else if (initial.X > xLimits.Value)
        {
            initial.X = xLimits.Value;
        }

        Point? ret = calcPointBelow(new Point(initial.X, initial.Y - 85));   // actual drop ranges: default - 120, explosive - 360
        if (ret == null)
        {
            ret = bsearchDropPos(initial, fallback);
        }

        if (!mapArea.Contains(ret.Value))
        { // found drop pos outside the map :O
            return fallback;
        }

        return ret.Value;
    }




    public bool TryGetEffectiveDoorPortal(out MysticDoorPortal? portal)
    {
        portal = null;
        foreach (var item in portals.Values)
        {
            if (item is MysticDoorPortal p && p.Door == null)
            {
                portal = p;
                return true;
            }
        }
        return false;
    }

    public void addSelfDestructive(Monster mob)
    {
        if (mob.getStats().selfDestruction() != null)
        {
            this.selfDestructives.Add(mob.getObjectId());
        }
    }

    public bool removeSelfDestructive(int mapobjectid)
    {
        return this.selfDestructives.Remove(mapobjectid);
    }

    public Monster? getMonsterById(int id)
    {
        return GetRequiredMapObjects<Monster>(MapObjectType.MONSTER, x => x.getId() == id).FirstOrDefault();
    }
    public int countMonster(int id)
    {
        return countMonster(id, id);
    }

    public int countMonster(int minid, int maxid)
    {
        return GetMapObjects(x => x.getType() == MapObjectType.MONSTER && x.GetSourceId() >= minid && x.GetSourceId() <= maxid).Count;
    }

    public int countMonsters()
    {
        return GetMapObjects(x => x.getType() == MapObjectType.MONSTER).Count;
    }

    public int countBosses()
    {
        return GetRequiredMapObjects<Monster>(MapObjectType.MONSTER, x => x.isBoss()).Count;
    }

    public Task broadcastHorntailVictory()
    {
        return ChannelServer.NodeActor.Send(s =>
        {
            s.SendDropMessage(6, "[Victory] To the crew that have finally conquered Horned Tail after numerous attempts, I salute thee! You are the true heroes of Leafre!!", false);
        });
    }


    public Task broadcastPinkBeanVictory(int channel)
    {
        return ChannelServer.NodeActor.Send(s =>
        {
            s.SendDropMessage(6,
                "[Victory] In a swift stroke of sorts, the crew that has attempted Pink Bean at channel " + channel + " has ultimately defeated it. The Temple of Time shines radiantly once again, the day finally coming back, as the crew that managed to finally conquer it returns victoriously from the battlefield!!", false);
        });
    }

    async Task<bool> TryRemoveMapMonsterObject(Monster monster, Packet p)
    {
        if (!await RemoveMapObject(monster, mapChr => mapChr.SendPacket(p)))
        {
            return false;
        }

        if (monster.hasBossHPBar())
        {
            // thanks resinate for noticing boss HPbar not clearing after mob defeat in certain scenarios
            await broadcastBossHpMessage(monster, monster.GetHashCode(), monster.makeBossHPBarPacket(), monster.getPosition());
        }

        return true;
    }
    public async Task RemoveMob(Monster? monster, ICombatantObject? killer, bool withDrops, int animation = 1, short dropDelay = 0)
    {
        if (monster == null)
        {
            return;
        }


        if (await TryRemoveMapMonsterObject(monster, PacketCreator.killMonster(monster.getObjectId(), animation)))
        {
            try
            {
                if (killer is Player chr)
                {
                    if (monster.getStats().getLevel() >= chr.getLevel() + 30 && !chr.isGM())
                    {
                        ChannelServer.NodeService.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, " for killing a " + monster.getName() + " which is over 30 levels higher.");
                    }

                    int buff = monster.getBuffToGive();
                    if (buff > -1)
                    {
                        ItemInformationProvider mii = ItemInformationProvider.getInstance();
                        foreach (var character in getAllPlayers())
                        {
                            if (character.isAlive())
                            {
                                var statEffect = mii.getItemEffect(buff)!;
                                await character.SendPacket(PacketCreator.showOwnBuffEffect(buff, 1));
                                await character.BroadcastMap(PacketCreator.showBuffEffect(character.getId(), buff, 1), character.Id);
                                await statEffect.applyTo(character);
                            }
                        }
                    }


                    var dropOwner = await monster.killBy(chr);
                    if (withDrops && dropOwner != null)
                    {
                        await dropFromMonster(dropOwner, monster, false, dropDelay);
                    }

                    if (monster.hasBossHPBar())
                    {
                        foreach (Player mc in this.getAllPlayers())
                        {
                            if (mc.getTargetHpBarHash() == monster.GetHashCode())
                            {
                                await mc.resetPlayerAggro();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {
                // thanks resinate for pointing out a memory leak possibly from an exception thrown
                await monster.dispatchMonsterKilled(killer);
            }
        }
    }

    public async Task killFriendlies(Monster mob)
    {
        await this.RemoveMob(mob, getAllPlayers().ElementAtOrDefault(0), false);
    }

    public async Task killMonster(int mobId, bool withDrops = false)
    {
        Player? chr = getAllPlayers().ElementAtOrDefault(0);

        await ProcessMonster(async mob =>
        {
            if (mob.getId() == mobId)
            {
                await this.RemoveMob(mob, chr, withDrops);
            }
        });
    }

    public async Task killAllMonstersNotFriendly()
    {
        closeMapSpawnPoints();

        await ProcessMonster(async monster =>
        {
            if (monster.getStats().isFriendly())
            {
                return;
            }

            await RemoveMob(monster, null, false);
        });
    }

    public async Task killAllMonsters()
    {
        closeMapSpawnPoints();

        await ProcessMonster(async monster =>
        {
            await RemoveMob(monster, null, false);
        });
    }


    #region NPC
    public NPC CreateNPC(NpcTemplate template, Point pos)
    {
        return new NPC(template, this, pos);
    }
    public async Task SpawnNpc(int npcId, Point pos)
    {
        var npc = CreateNPC(LifeFactory.Instance.GetNPCTemplateTrust(npcId), pos);
        if (npc != null)
        {
            npc.setCy(pos.Y);
            npc.setRx0(pos.X + 50);
            npc.setRx1(pos.X - 50);
            npc.setFh(Footholds.FindBelowFoothold(pos)!.getId());

            await AddMapObject(npc, async c =>
            {
                await c.SendPacket(PacketCreator.spawnNPC(npc));
            });
        }
    }
    public NPC? getNPCById(int id)
    {
        foreach (IMapObject obj in getMapObjects())
        {
            if (obj.getType() == MapObjectType.NPC)
            {
                NPC npc = (NPC)obj;
                if (npc.getId() == id)
                {
                    return npc;
                }
            }
        }

        return null;
    }

    public bool containsNPC(int npcid)
    {
        foreach (var obj in getMapObjects())
        {
            if (obj.getType() == MapObjectType.NPC)
            {
                if (((NPC)obj).getId() == npcid)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public async Task destroyNPC(int npcid)
    {
        // assumption: there's at most one of the same NPC in a map.
        var npcs = GetMapObjects(x => x.getType() == MapObjectType.NPC);

        foreach (IMapObject obj in npcs)
        {
            if (((NPC)obj).getId() == npcid)
            {
                await RemoveMapObject(obj, async mapChr =>
                {
                    await mapChr.SendPacket(PacketCreator.removeNPCController(obj.getObjectId()));
                    await mapChr.SendPacket(PacketCreator.removeNPC(obj.getObjectId()));
                });
            }
        }
    }

    #endregion

    public IMapObject? getMapObject(int oid)
    {
        return mapobjects.GetValueOrDefault(oid);
    }


    #region Objects:Reactor
    public Task spawnReactor(Reactor reactor)
    {
        return AddMapObject(reactor, async c => await c.SendPacket(reactor.makeSpawnData()));
    }

    public int countReactors()
    {
        return getReactors().Count;
    }

    public List<IMapObject> getReactors()
    {
        return GetMapObjects(x => x.getType() == MapObjectType.REACTOR);
    }
    public List<Reactor> getAllReactors()
    {
        return GetRequiredMapObjects<Reactor>(MapObjectType.REACTOR);
    }
    public Reactor? getReactorByOid(int oid)
    {
        IMapObject? mmo = getMapObject(oid);
        return mmo as Reactor;
    }

    public Reactor? getReactorById(int Id)
    {
        foreach (var obj in getMapObjects())
        {
            if (obj.getType() == MapObjectType.REACTOR)
            {
                if (((Reactor)obj).getId() == Id)
                {
                    return (Reactor)obj;
                }
            }
        }
        return null;
    }

    public List<Reactor> getReactorsByIdRange(int first, int last)
    {
        List<Reactor> list = new();


        foreach (var obj in getMapObjects())
        {
            if (obj.getType() == MapObjectType.REACTOR)
            {
                Reactor mr = (Reactor)obj;

                if (mr.getId() >= first && mr.getId() <= last)
                {
                    list.Add(mr);
                }
            }
        }

        return list;
    }

    public Reactor? getReactorByName(string name)
    {

        foreach (IMapObject obj in getMapObjects())
        {
            if (obj.getType() == MapObjectType.REACTOR)
            {
                if (((Reactor)obj).getName().Equals(name))
                {
                    return (Reactor)obj;
                }
            }
        }
        return null;
    }

    public bool CanHitReactor(MapItem mapItem)
    {
        foreach (var item in mapobjects.Values.ToArray())
        {
            if (item is Reactor r && r.CheckHitItem(mapItem))
            {
                return true;
            }
        }
        return false;
    }
    public async Task TryHitReactorByMapItem(MapItem mapItem)
    {
        foreach (var item in mapobjects.Values.ToArray())
        {
            if (item is Reactor r && r.CheckHitItem(mapItem))
            {
                await r.HitByMapItem(mapItem);
                return;
            }
        }
    }

    public async Task destroyReactors(int first, int last)
    {
        List<Reactor> toDestroy = new();
        var reactors = getReactors();

        foreach (IMapObject obj in reactors)
        {
            Reactor mr = (Reactor)obj;
            if (mr.getId() >= first && mr.getId() <= last)
            {
                toDestroy.Add(mr);
            }
        }

        foreach (Reactor mr in toDestroy)
        {
            await destroyReactor(mr.getObjectId());
        }
    }

    public async Task destroyReactor(int oid)
    {
        var reactor = getReactorByOid(oid);

        if (reactor != null)
        {
            if (await reactor.destroy())
            {
                await RemoveMapObject(reactor, null);
            }
        }
    }

    public async Task resetReactors()
    {
        List<Reactor> list = new();

        foreach (IMapObject o in getMapObjects())
        {
            if (o.getType() == MapObjectType.REACTOR)
            {
                Reactor r = ((Reactor)o);
                list.Add(r);
            }
        }

        await resetReactors(list);
    }

    public async Task resetReactors(List<Reactor> list)
    {
        foreach (Reactor r in list)
        {
            if (r.forceDelayedRespawn())
            {  // thanks Conrad for suggesting reactor with delay respawning immediately
                continue;
            }

            await r.resetReactorActions(0);
            r.setAlive(true);
            await r.BroadcastMap(PacketCreator.triggerReactor(r, 0));
        }
    }

    public void shuffleReactors(int first = 0, int last = int.MaxValue)
    {
        List<Point> points = new();
        var reactors = getReactors();
        List<Reactor> targets = new();

        foreach (var obj in reactors)
        {
            Reactor mr = (Reactor)obj;
            if (mr.getId() >= first && mr.getId() <= last)
            {
                points.Add(mr.getPosition());
                targets.Add(mr);
            }
        }
        Collections.shuffle(points);
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].setPosition(points[i]);
        }
    }

    public void shuffleReactors(List<object> list)
    {
        List<Point> points = new();
        List<IMapObject> listObjects = new();
        List<Reactor> targets = new();

        foreach (object ob in list)
        {
            if (ob is IMapObject mmo)
            {
                if (mapobjects.ContainsValue(mmo) && mmo.getType() == MapObjectType.REACTOR)
                {
                    listObjects.Add(mmo);
                }
            }
        }

        foreach (var obj in listObjects)
        {
            Reactor mr = (Reactor)obj;

            points.Add(mr.getPosition());
            targets.Add(mr);
        }
        Collections.shuffle(points);
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].setPosition(points[i]);
        }
    }

    public async Task setReactorState()
    {
        foreach (IMapObject o in getMapObjects())
        {
            if (o.getType() == MapObjectType.REACTOR && o is Reactor mr)
            {
                if (mr.getState() < 1)
                {
                    await mr.resetReactorActions(1);
                    await mr.BroadcastMap(PacketCreator.triggerReactor(mr, 1));
                }
            }
        }
    }

    public bool isAllReactorState(int reactorId, int state)
    {
        foreach (var r in getAllReactors())
        {
            if (r.getId() == reactorId && r.getState() != state)
            {
                return false;
            }
        }
        return true;
    }

    #endregion



    /// <summary>
    /// 生成地图boss
    /// </summary>
    /// <param name="bossId"></param>
    /// <param name="mobTime"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="spawnMessage"></param>
    public async Task SetupAreaBoss(string name, int bossId, int mobTime, List<RandomPoint> rawList, string spawnMessage)
    {
        if (_bossSp.TryGetValue(name, out var sp))
        {
            if (sp.shouldForceSpawn())
            {
                await sp.SpawnMonster();
            }
            return;
        }

        sp = new AreaBossSpawnPoint(name, this, bossId, rawList, mobTime, SourceTemplate.CreateMobInterval, spawnMessage);
        _bossSp[name] = sp;
        await sp.SpawnMonster();
    }

    public void ClearAreaBoss(string name)
    {
        _bossSp.Remove(name);
    }

    public int FindFh(Point pos)
    {
        var fh = Footholds.FindBelowFoothold(pos);
        if (fh != null)
            return fh.getId();

        var ladderRope = SourceTemplate.LadderRopes.FirstOrDefault(x => x.Contains(pos));
        if (ladderRope != null)
            return -ladderRope.Index;

        return 0;
    }

    private Point? calcPointBelow(Point initial)
    {
        return Footholds.FindBelowPoint(initial);
    }
    public Point getGroundBelow(Point pos)
    {
        Point spos = new Point(pos.X, pos.Y - 14); // Using -14 fixes spawning pets causing a lot of issues.
        var calcedPos = calcPointBelow(spos);
        if (calcedPos != null)
        {
            spos = calcedPos.Value;
        }
        spos.Y--;//shouldn't be null!
        return spos;
    }

    public Point? getPointBelow(Point pos)
    {
        return calcPointBelow(pos);
    }


    #region Monster
    public Monster? getMonsterByOid(int oid)
    {
        IMapObject? mmo = getMapObject(oid);
        return mmo as Monster;
    }
    public async Task spawnFakeMonsterOnGroundBelow(MobTemplate mobData, Point pos, Action<Monster>? handleMob = null)
    {
        Point spos = getGroundBelow(pos);
        var mob = CreateMonster(mobData, spos);
        handleMob?.Invoke(mob);
        await spawnFakeMonster(mob);
    }
    public async Task spawnFakeMonster(Monster monster)
    {
        monster.setFake(true);
        await AddMapObject(monster, c => c.SendPacket(PacketCreator.spawnFakeMonster(monster, 0)));


        XiGuai?.ApplyMonster(monster);
        addSelfDestructive(monster);
    }
    public async Task makeMonsterReal(Monster monster)
    {
        if (!monster.isFake())
            return;
        monster.setFake(false);
        await monster.BroadcastMap(PacketCreator.makeMonsterReal(monster));
        await monster.aggroUpdateController();
        await updateBossSpawn(monster);
    }
    public async Task spawnMonsterOnGroundBelow(int id, int x, int y)
    {
        var mob = LifeFactory.Instance.GetMonsterTrust(id);
        await spawnMonsterOnGroundBelow(mob, new Point(x, y));
    }

    public Monster CreateMonster(MobTemplate mobData, Point pos)
    {
        return new Monster(this, pos, mobData);
    }

    public async Task spawnMonsterOnGroundBelow(MobTemplate mobData, Point pos, Action<Monster>? handleMob = null)
    {
        Point spos = new Point(pos.X, pos.Y - 1);
        var calcedPos = calcPointBelow(spos);
        if (calcedPos != null)
        {
            spos = calcedPos.Value;
            spos.Y--;

            var mob = CreateMonster(mobData, pos);
            handleMob?.Invoke(mob);
            await spawnMonster(mob);
        }

    }
    private List<SpawnPoint> getMonsterSpawn()
    {
        return new(monsterSpawn);
    }

    public async Task spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false)
    {
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            if (sp.getMonsterId() == id && sp.shouldForceSpawn())
            {
                await sp.SpawnMonster(difficulty, isPq);
            }
        }
    }


    public async Task spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false)
    {
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            await sp.SpawnMonster(difficulty, isPq);
        }
    }


    public async Task spawnMonster(Monster monster, int difficulty = 1, bool isPq = false)
    {
        if (SourceTemplate.FixedMobCapacity != -1 && SourceTemplate.FixedMobCapacity == spawnedMonstersOnMap.get())
        {
            return;//PyPQ
        }

        monster.changeDifficulty(difficulty, isPq);
        await AddMapObject(monster, c => c.SendPacket(PacketCreator.spawnMonster(monster, true)));

        EventInstanceManager?.registerMonster(monster);

        await monster.aggroUpdateController();
        await updateBossSpawn(monster);

        await SetMonsterInfo(monster);

        XiGuai?.ApplyMonster(monster);

        addSelfDestructive(monster);
    }

    public async Task spawnDojoMonster(MobTemplate monster)
    {
        Point[] pts = { new Point(140, 0), new Point(190, 7), new Point(187, 7) };
        await spawnMonsterOnGroundBelow(monster, Randomizer.Select(pts), mob =>
        {
            mob.setSpawnEffect(15);
            mob.setBoss(false);
        });
    }

    public async Task ProcessMonster(Func<Monster, Task> action)
    {
        await ProcessMapObject(x => x.getType() == MapObjectType.MONSTER, async o =>
        {
            await action((Monster)o);
        });
    }

    public Task spawnHorntailOnGroundBelow(Point targetPoint)
    {
        // ayy lmao
        var htIntro = LifeFactory.Instance.GetMonsterTrust(MobId.SUMMON_HORNTAIL)!;
        return spawnMonsterOnGroundBelow(htIntro, targetPoint);    // htintro spawn animation converting into horntail detected thanks to Arnah
    }

    public async Task SpawnZakumOnGroundBelow(Point pos)
    {
        var main = CreateMonster(LifeFactory.Instance.GetMonsterTrust(MobId.ZAKUM_1), pos);
        await spawnFakeMonster(main);

        for (int mobId = MobId.ZAKUM_ARM_1; mobId <= MobId.ZAKUM_ARM_8; mobId++)
        {
            var bodyPart = CreateMonster(LifeFactory.Instance.GetMonsterTrust(mobId), pos);
            bodyPart.ChaindMobOId = main.getObjectId();

            await spawnMonster(bodyPart);
        }
    }
    #endregion

    public async Task spawnDoor(DoorObject door)
    {
        await AddMapObject(door, c => door.sendSpawnData(c, false));
    }

    public Portal? getDoorPortal(int doorid)
    {
        Portal? doorPortal = portals.GetValueOrDefault(0x80 + doorid);
        if (doorPortal == null)
        {
            log.Warning("does not contain door portalid {DoorId}", doorid);
            return portals.GetValueOrDefault(0x80);
        }

        return doorPortal;
    }

    public async Task spawnSummon(Summon summon)
    {
        await AddMapObject(summon, c => c.SendPacket(PacketCreator.spawnSummon(summon, true)));
    }

    public async Task spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery)
    {
        await AddMapObject(mist, c => c.SendPacket(fake ? mist.makeFakeSpawnData(30) : mist.makeSpawnData()));
    }

    public async Task spawnKite(Kite kite)
    {
        await AddMapObject(kite, c => c.SendPacket(kite.makeSpawnData()));
    }

    #region Objects:MapItem
    public Task spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, bool ffaDrop, bool playerDrop)
    {
        return spawnItemDrop(dropper, owner, item, pos, (DropType)(ffaDrop ? 2 : 0), playerDrop);
    }

    private async Task spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, DropType dropType, bool playerDrop)
    {
        if (FieldLimit.DROP_LIMIT.check(this.getFieldLimit()))
        {
            // thanks Conrad for noticing some maps shouldn't have loots available
            await DropItemDestroy(item.getItemId(), dropper.getPosition());
            return;
        }

        Point droppos = calcDropPos(pos, dropper.getPosition());
        MapItem mapItem = new MapItem(this, item, droppos, dropper, owner, dropType, playerDrop);

        await AddMapObject(mapItem, c =>
        {
            return c.SendPacket(PacketCreator.dropItemFromMapObject(c.OnlinedCharacter, mapItem, dropper.getPosition(), droppos, (int)DropEnterFieldType.SpawnMapItem, 0));
        });

        /// 不明，对比<see cref="spawnDrop"/> 多出来的一段 是否可以移除？
        await broadcastItemDropMessage(mapItem, dropper.getPosition(), droppos, 0);
    }
    private Task spawnDrop(Item idrop, Point dropPos, IMapObject dropper, Player chr, bool playerDrop, DropType droptype, short questid, short dropDelay)
    {
        var validPos = calcDropPos(dropPos, dropper.getPosition());

        MapItem mapItem = new MapItem(this, idrop, validPos, dropper, chr, droptype, playerDrop, questid);
        return AddMapObject(mapItem, c =>
        {
            return c.SendPacket(PacketCreator.dropItemFromMapObject(c.OnlinedCharacter, mapItem, dropper.getPosition(), validPos, (int)DropEnterFieldType.SpawnMapItem, dropDelay));
        });
    }

    public async Task spawnMesoDrop(int meso, Point position, IMapObject dropper, Player owner, bool playerDrop, DropType droptype, short dropDelay = 0)
    {
        var validPos = calcDropPos(position, dropper.getPosition());

        MapItem mapItem = new MapItem(this, meso, validPos, dropper, owner, droptype, playerDrop);
        await AddMapObject(mapItem, c =>
        {
            return c.SendPacket(PacketCreator.dropItemFromMapObject(c.OnlinedCharacter, mapItem, dropper.getPosition(), validPos, (int)DropEnterFieldType.SpawnMapItem, dropDelay));
        });
    }

    private async Task<byte> dropItemsFromMonsterOnMap(List<DropEntry> dropEntry, byte dIndex, float chRate, DropType droptype, Player chr, IMapObject dropper, short dropDelay)
    {
        if (dropEntry.Count == 0)
        {
            return dIndex;
        }

        Collections.shuffle(dropEntry);

        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        var itemPos = dropper.getPosition();
        var startPosX = itemPos.X;
        foreach (var de in dropEntry)
        {
            int dropChance = de.Chance;
            if (chRate != 0)
            {
                float cardRate = chr.getCardRate(de.ItemId);
                dropChance = (int)Math.Min((float)de.Chance * chRate * cardRate, int.MaxValue);
            }

            if (de.CanDrop(dropChance))
            {
                itemPos.X = de.GetDropPosX(droptype, startPosX, dIndex);
                if (de.ItemId == 0)
                {
                    // meso
                    float mesos = de.GetRandomCount();
                    if (mesos > 0)
                    {
                        var mesoUpBuff = chr.getBuffedValue(BuffStat.MESOUP);
                        if (mesoUpBuff != null)
                        {
                            mesos = (mesos * mesoUpBuff.Value / 100f);
                        }
                        mesos = mesos * chr.getMesoRate();
                        if (mesos <= 0)
                        {
                            log.Warning("Character {CharacterName}, Id = {CharacterId}, MesoRate {MesoRate}", chr, chr.Id, chr.getMesoRate());
                        }
                        else
                        {
                            await spawnMesoDrop((int)mesos, itemPos, dropper, chr, false, droptype, dropDelay);
                            dIndex++;
                        }
                    }
                }
                else
                {
                    var idrop = ii.GenerateVirtualItemById(de.ItemId, (short)de.GetRandomCount(), true);
                    if (idrop == null)
                    {
                        log.Warning("{Mob}尝试掉落不存在的物品：{ItemId}", dropper.GetName(), de.ItemId);
                        continue;
                    }

                    await spawnDrop(idrop, itemPos, dropper, chr, false, droptype, de.QuestId, dropDelay);
                    dIndex++;
                }

            }
        }

        return dIndex;
    }

    public async Task DropMesoFromPlayer(Player chr, int meso)
    {
        if (meso > 0)
        {
            await chr.GainMeso(-meso);

            await spawnMesoDrop(meso, chr.getPosition(), chr, chr, true, DropType.FreeForAll);
        }
    }

    private async Task dropFromMonster(Player chr, Monster mob, bool useBaseRate, short dropDelay)
    {
        if (mob.dropsDisabled() || !dropsOn)
        {
            return;
        }

        DropType dropType = DropType.OnlyOwner;
        if (mob.getStats().isExplosiveReward())
            dropType = DropType.FreeForAll_Explosive;
        else if (mob.getStats().isFfaLoot())
            dropType = DropType.FreeForAll;
        else if (chr.Party > 0)
            dropType = DropType.OwnerWithTeam;

        float chrRate = 1;
        if (!useBaseRate)
        {
            chrRate = !mob.isBoss() ? chr.getDropRate() : chr.getBossDropRate();

            var stati = mob.getStati(MonsterStatus.SHOWDOWN);
            if (stati != null)
            {
                chrRate *= (stati.getStati().GetValueOrDefault(MonsterStatus.SHOWDOWN) / 100.0f + 1.0f);
            }
        }

        MonsterInformationProvider mi = MonsterInformationProvider.getInstance();
        List<DropEntry> globalEntry = mi.getRelevantGlobalDrops(this.getId());

        List<DropEntry> lootEntry = mob.GetDropEntryList();
        if (lootEntry.Count == 0 && globalEntry.Count == 0)
        {
            return;
        }
        DropEntry.ClassifyDropEntries(lootEntry, out var dropEntry, out var visibleQuestEntry, out var otherQuestEntry, chr);     // thanks Articuno, Limit, Rohenn for noticing quest loots not showing up in only-quest item drops scenario

        byte index = 1;
        // Normal Drops
        index = await dropItemsFromMonsterOnMap(dropEntry, index, chrRate, dropType, chr, mob, dropDelay);
        // Global Drops
        index = await dropItemsFromMonsterOnMap(globalEntry, index, 0, dropType, chr, mob, dropDelay);
        // Quest Drops
        index = await dropItemsFromMonsterOnMap(visibleQuestEntry, index, chrRate, dropType, chr, mob, dropDelay);
        await dropItemsFromMonsterOnMap(otherQuestEntry, index, chrRate, dropType, chr, mob, dropDelay);
    }

    /// <summary>
    /// 偷窃获得掉落物
    /// </summary>
    /// <param name="list"></param>
    /// <param name="chr"></param>
    /// <param name="mob"></param>
    /// <param name="dropDelay"></param>
    public async Task DropItemFromMonsterBySteal(List<DropEntry> list, Player chr, Monster mob, short dropDelay)
    {
        if (mob.dropsDisabled() || !dropsOn)
        {
            return;
        }

        DropType droptype = (chr.Party > 0 ? DropType.OwnerWithTeam : DropType.OnlyOwner);
        int chRate = 1000000;   // 偷窃成功概率已经计算
        byte d = 1;

        await dropItemsFromMonsterOnMap(list, d, chRate, droptype, chr, mob, dropDelay);
    }

    public async Task dropFromFriendlyMonster(Player chr, Monster mob)
    {
        await dropFromMonster(chr, mob, true, 0);
    }

    public async Task dropFromReactor(Player chr, Reactor reactor, Item drop, Point dropPos, short questid, short dropDelay = 0)
    {
        await spawnDrop(drop, dropPos, reactor, chr, false, (chr.Party > 0 ? DropType.OwnerWithTeam : DropType.OnlyOwner), questid, dropDelay);
    }


    public async Task<List<MapItem>> updatePlayerItemDropsToParty(int partyid, int charid, List<Player> partyMembers, Player? partyLeaver)
    {
        List<MapItem> partyDrops = new();

        foreach (MapItem mdrop in getDroppedItems())
        {
            if (mdrop.getOwnerId() == charid)
            {
                if (mdrop.isPickedUp())
                {
                    continue;
                }

                mdrop.setPartyOwnerId(partyid);

                Packet removePacket = PacketCreator.silentRemoveItemFromMap(mdrop.getObjectId());
                Packet updatePacket = PacketCreator.updateMapItemObject(mdrop, partyLeaver == null);

                foreach (Player mc in partyMembers)
                {
                    if (this.Equals(mc.getMap()))
                    {
                        await mc.SendPacket(removePacket);

                        if (mc.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                        {
                            await mc.SendPacket(updatePacket);
                        }
                    }
                }

                if (partyLeaver != null)
                {
                    if (this.Equals(partyLeaver.getMap()))
                    {
                        await partyLeaver.SendPacket(removePacket);

                        if (partyLeaver.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                        {
                            await partyLeaver.SendPacket(PacketCreator.updateMapItemObject(mdrop, true));
                        }
                    }
                }
            }
            else if (partyid != -1 && mdrop.getPartyOwnerId() == partyid)
            {
                partyDrops.Add(mdrop);
            }
        }

        return partyDrops;
    }

    public async Task updatePartyItemDropsToNewcomer(Player newcomer, List<MapItem> partyItems)
    {
        foreach (MapItem mdrop in partyItems)
        {
            if (mdrop.isPickedUp())
            {
                continue;
            }

            Packet removePacket = PacketCreator.silentRemoveItemFromMap(mdrop.getObjectId());
            Packet updatePacket = PacketCreator.updateMapItemObject(mdrop, true);

            if (newcomer != null)
            {
                if (this.Equals(newcomer.getMap()))
                {
                    await newcomer.SendPacket(removePacket);

                    if (newcomer.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                    {
                        await newcomer.SendPacket(updatePacket);
                    }
                }
            }
        }
    }

    private Task broadcastItemDropMessage(MapItem mdrop, Point dropperPos, Point dropPos, byte mod, Point? rangedFrom = null, short dropDelay = 0)
    {
        return Broadcast(-1, rangedFrom == null ? double.PositiveInfinity : ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance(), rangedFrom, async chr =>
        {
            await chr.SendPacket(PacketCreator.dropItemFromMapObject(chr, mdrop, dropperPos, dropPos, mod, dropDelay));
        });
    }

    public Task DropItemDestroy(int itemId, Point dropperPos)
    {
        return Broadcast(-1, ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance(), dropperPos, async chr =>
        {
            await chr.SendPacket(PacketCreator.DropItemDestroy(itemId, dropperPos));
        });
    }
    #endregion


    public async Task startMapEffect(string msg, int itemId, long time = 30000)
    {
        if (MapEffect != null)
        {
            return;
        }
        MapEffect = new MapEffect(msg, itemId, ChannelServer.Node.getCurrentTime() + time);
        await broadcastMessage(MapEffect.makeStartData());
    }

    public Portal getRandomPlayerSpawnpoint()
    {
        var portal = Randomizer.Select(portals.Values.Where(x => x.getType() >= 0 && x.getType() <= 1 && x.getTargetMapId() == MapId.NONE));
        return portal ?? getPortal(0)!;
    }

    public Portal? findClosestTeleportPortal(Point from)
    {
        return portals.Values.AsValueEnumerable()
            .OrderBy(x => x.getPosition().distanceSq(from))
            .Where(x => x.getType() == PortalConstants.TELEPORT_PORTAL && x.getTargetMapId() != MapId.NONE)
            .FirstOrDefault();
    }

    public Portal? findClosestPlayerSpawnpoint(Point from)
    {
        return portals.Values.AsValueEnumerable()
            .OrderBy(x => x.getPosition().distanceSq(from))
            .Where(x => x.getType() >= 0 && x.getType() <= 1 && x.getTargetMapId() == MapId.NONE)
            .FirstOrDefault();
    }

    public Portal? findClosestPortal(Point from)
    {
        return portals.Values.AsValueEnumerable().OrderBy(x => x.getPosition().distanceSq(from)).FirstOrDefault();
    }

    public Portal? findMarketPortal()
    {
        return portals.Values.AsValueEnumerable().FirstOrDefault(x => x.getScriptName()?.Contains("market") == true);
    }

    /*
    public Collection<Portal> getPortals() {
        return Collections.unmodifiableCollection(portals.values());
    }
    */

    public async Task addPlayerPuppet(Player player)
    {
        await ProcessMonster(async mm =>
        {
            await mm.aggroAddPuppet(player);
        });
    }

    public async Task removePlayerPuppet(Player player)
    {
        await ProcessMonster(async mm =>
        {
            await mm.aggroRemovePuppet(player);
        });
    }

    public Task broadcastMessage(Packet packet)
    {
        return Broadcast(-1, double.PositiveInfinity, null, e => e.SendPacket(packet));
    }


    /**
     * Ranged and repeat according to parameters.
     *
     * @param source
     * @param packet
     * @param repeatToSource
     * @param ranged
     */
    public Task broadcastMessage(Player source, Packet packet, bool repeatToSource, bool ranged = false)
    {
        return broadcastMessage(repeatToSource ? null : source, packet, ranged ? ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance() : double.PositiveInfinity, source.getPosition());
    }


    /**
     * Always ranged from point. Does not repeat to source.
     *
     * @param source
     * @param packet
     * @param rangedFrom
     */
    public async Task broadcastMessage(Player? source, Packet packet, Point rangedFrom)
    {
        await broadcastMessage(source, packet, ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance(), rangedFrom);
    }

    private async Task broadcastMessage(Player? source, Packet packet, double rangeSq, Point? rangedFrom)
    {
        await Broadcast(source?.Id ?? -1, rangeSq, rangedFrom, chr => chr.SendPacket(packet));
    }

    private async Task updateBossSpawn(Monster monster)
    {
        if (monster.hasBossHPBar())
        {
            await broadcastBossHpMessage(monster, monster.GetHashCode(), monster.makeBossHPBarPacket(), monster.getPosition());
        }
        if (monster.isBoss())
        {
            if (unclaimOwnership() != null)
            {
                await BroadcastAll(e => e.Pink(nameof(ClientMessage.Map_Ownership_Boss), e.Client.CurrentCulture.GetMobName(monster.getId())));
            }
        }
    }

    public async Task broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null)
    {
        await Broadcast(-1, ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance(), rangedFrom, async chr =>
        {
            await chr.getClient().announceBossHpBar(mm, bossHash, packet);
        });
    }



    public List<IMapObject> getMapObjects()
    {
        return new(mapobjects.Values);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="codition"></param>
    /// <param name="action"></param>
    public async Task ProcessMapObject(Func<IMapObject, bool> codition, Func<IMapObject, Task> action)
    {
        var list = getMapObjects();
        foreach (var item in list)
        {
            if (codition(item))
                await action(item);
        }
    }

    public List<IMapObject> GetMapObjects(Func<IMapObject, bool> func)
    {
        return mapobjects.Values.AsValueEnumerable().Where(func).ToList();
    }
    List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type) where TObject : IMapObject
    {
        return mapobjects.Values.AsValueEnumerable()
            .Where(x => x.getType() == type)
            .Cast<TObject>()
            .ToList();
    }
    public List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type, Func<TObject, bool> func) where TObject : IMapObject
    {
        return mapobjects.Values.AsValueEnumerable()
            .Where(x => x.getType() == type)
            .Cast<TObject>()
            .Where(func)
            .ToList();
    }



    public List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, HashSet<MapObjectType> types)
    {
        return GetMapObjects(x => MapGlobalData.IsObjectInRange(x, from, rangeSq) && types.Contains(x.getType()));
    }

    public List<IMapObject> getMapObjectsInBox(Rectangle box, HashSet<MapObjectType> types)
    {
        List<IMapObject> ret = new();

        foreach (IMapObject l in getMapObjects())
        {
            if (types.Contains(l.getType()))
            {
                if (box.Contains(l.getPosition()))
                {
                    ret.Add(l);
                }
            }
        }
        return ret;
    }

    public Portal? getPortal(string portalname)
    {
        return portals.Values.AsValueEnumerable().FirstOrDefault(x => x.getName() == portalname);
    }

    public Portal? getPortal(int portalid)
    {
        return portals.GetValueOrDefault(portalid);
    }

    public List<Rectangle> getAreas()
    {
        return new(areas);
    }

    public Rectangle getArea(int index)
    {
        return areas[index];
    }

    public MonsterAggroCoordinator getAggroCoordinator()
    {
        return aggroMonitor;
    }

    /**
     * it's threadsafe, gtfo :D
     *
     * @param monster
     * @param mobTime
     */
    public async Task addMonsterSpawn(int mobId, Point pos, int cy, int f, int fh, int rx0, int rx1, int mobTime, bool hide, int team)
    {
        Point newpos = calcPointBelow(pos)!.Value;
        newpos.Y -= 1;
        SpawnPoint sp = new SpawnPoint(this, mobId, pos, cy, f, fh, rx0, rx1, hide, team, mobTime, SourceTemplate.CreateMobInterval);
        monsterSpawn.Add(sp);

        if (sp.shouldSpawn() || sp.CanInitialSpawn)
        {
            // -1 does not respawn and should not either but force ONE spawn
            await sp.SpawnMonster();
        }
    }

    public async Task addMonsterSpawn(int mobId, Point pos, int mobTime, int team)
    {
        await addMonsterSpawn(mobId, pos, 0, 0, 0, 0, 0, mobTime, false, team);
    }

    public void removeMonsterSpawn(int mobId, int x, int y)
    {
        // assumption: spawn points identifies by tuple (lifeid, x, y)

        Point checkpos = calcPointBelow(new Point(x, y))!.Value;
        checkpos.Y -= 1;

        List<SpawnPoint> toRemove = new();
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            Point pos = sp.getPosition();
            if (sp.getMonsterId() == mobId && checkpos.Equals(pos))
            {
                toRemove.Add(sp);
            }
        }

        if (toRemove.Count > 0)
        {
            foreach (SpawnPoint sp in toRemove)
            {
                monsterSpawn.Remove(sp);
            }
        }
    }

    public async Task reportMonsterSpawnPoints(Player chr)
    {
        await chr.dropMessage(6, "Mob spawnpoints on map " + getId() + ", with available Mob SPs " + monsterSpawn.Count() + ", used " + spawnedMonstersOnMap.get() + ":");
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            await chr.dropMessage(6, "  id: " + sp.getMonsterId() + " canSpawn: " + !sp.getDenySpawn() + " numSpawned: " + sp.getSpawned() + " x: " + sp.getPosition().X + " y: " + sp.getPosition().Y + " time: " + sp.getMobTime() + " team: " + sp.getTeam());
        }
    }


    public async Task MoveMapObject(AbstractAnimatedMapObject mapObject)
    {
        if (!UseRangedView)
        {
            return;
        }

        foreach (Player chr in getAllPlayers())
        {
            var isObjectNowVisible = mapObject.IsVisibleForPlayer(chr);

            if (isObjectNowVisible)
            {
                await SetPlayerVisibleObject(chr, mapObject);
            }
            else
            {
                await SetPlayerInvisibleObject(chr, mapObject);
            }
        }
    }


    public async Task toggleEnvironment(string ms)
    {
        var env = getEnvironment();

        if (env.TryGetValue(ms, out var value))
        {
            await moveEnvironment(ms, value == 1 ? 2 : 1);
        }
        else
        {
            await moveEnvironment(ms, 1);
        }
    }

    public async Task moveEnvironment(string ms, int type)
    {
        await broadcastMessage(PacketCreator.environmentMove(ms, type));

        environment.AddOrUpdate(ms, type);
    }

    public IDictionary<string, int> getEnvironment()
    {
        return new Dictionary<string, int>(environment);
    }

    public string getMapName()
    {
        return ClientCulture.SystemCulture.GetMapName(mapid);
    }

    public string getStreetName()
    {
        return ClientCulture.SystemCulture.GetMapStreetName(mapid);
    }


    public bool hasClock() => SourceTemplate.HasClock;

    public bool isMuted()
    {
        return _isMuted;
    }

    public void setMuted(bool mute)
    {
        _isMuted = mute;
    }

    public bool getEverlast() => SourceTemplate.Everlast;

    public int getSpawnedMonstersOnMap()
    {
        return spawnedMonstersOnMap.get();
    }

    // not really costly to keep generating imo
    public async Task sendNightEffect(Player chr)
    {
        foreach (var types in SourceTemplate.Backs)
        {
            if (types.Type >= 3)
            { // 3 is a special number
                await chr.SendPacket(PacketCreator.changeBackgroundEffect(true, types.Index, 0));
            }
        }
    }

    public async Task broadcastNightEffect()
    {
        foreach (Player chr in this.getAllPlayers())
        {
            await sendNightEffect(chr);
        }
    }

    public Player? getCharacterByName(string name)
    {
        return getAllPlayers().FirstOrDefault(x => x.getName().Equals(name, StringComparison.OrdinalIgnoreCase));
    }




    private async Task instanceMapFirstSpawn()
    {
        foreach (SpawnPoint spawnPoint in getMonsterSpawn())
        {
            if (spawnPoint.shouldSpawn() || spawnPoint.CanInitialSpawn)
            {
                //just those allowed to be spawned only once
                await spawnPoint.SpawnMonster();
            }
        }
    }

    /// <summary>
    /// EventInstance的map不会参与MapRespawn，通过该方法刷怪
    /// </summary>
    public async Task instanceMapRespawn()
    {
        if (!_allowSummons)
        {
            return;
        }

        int numShouldSpawn = (short)((monsterSpawn.Count - spawnedMonstersOnMap.get()));//Fking lol'd
        if (numShouldSpawn > 0)
        {
            var randomSpawn = getMonsterSpawn().ToList();
            Collections.shuffle(randomSpawn);
            int spawned = 0;
            foreach (SpawnPoint spawnPoint in randomSpawn)
            {
                if (spawnPoint.shouldSpawn())
                {
                    await spawnPoint.SpawnMonster();
                    spawned++;
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }
    }

    public async Task instanceMapForceRespawn()
    {
        if (!_allowSummons)
        {
            return;
        }

        int numShouldSpawn = (short)((monsterSpawn.Count - spawnedMonstersOnMap.get()));//Fking lol'd
        if (numShouldSpawn > 0)
        {
            var randomSpawn = getMonsterSpawn().ToList();
            Collections.shuffle(randomSpawn);
            int spawned = 0;
            foreach (SpawnPoint spawnPoint in randomSpawn)
            {
                if (spawnPoint.shouldForceSpawn())
                {
                    await spawnPoint.SpawnMonster();
                    spawned++;
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }
    }

    public void closeMapSpawnPoints()
    {
        foreach (SpawnPoint spawnPoint in getMonsterSpawn())
        {
            spawnPoint.setDenySpawn(true);
        }
    }

    public void restoreMapSpawnPoints()
    {
        foreach (SpawnPoint spawnPoint in getMonsterSpawn())
        {
            spawnPoint.setDenySpawn(false);
        }
    }

    public void setAllowSpawnPointInBox(bool allow, Rectangle box)
    {
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            if (box.Contains(sp.getPosition()))
            {
                sp.setDenySpawn(!allow);
            }
        }
    }
    public SpawnPoint? findClosestSpawnpoint(Point from)
    {
        SpawnPoint? closest = null;
        double shortestDistance = double.PositiveInfinity;
        foreach (SpawnPoint sp in getMonsterSpawn())
        {
            double distance = sp.getPosition().distanceSq(from);
            if (distance < shortestDistance)
            {
                closest = sp;
                shortestDistance = distance;
            }
        }
        return closest;
    }

    /// <summary>
    /// 6人在当前地图时，怪才会刷满
    /// </summary>
    /// <param name="numPlayers"></param>
    /// <returns></returns>
    private static double getCurrentSpawnRate(int numPlayers)
    {
        return 0.70 + (0.05 * Math.Min(6, numPlayers));
    }

    private int getNumShouldSpawn(int numPlayers)
    {
        /*
        Console.WriteLine("----------------------------------");
        foreach(SpawnPoint spawnPoint in getMonsterSpawn()) {
            Console.WriteLine("sp " + spawnPoint.getPosition().getX() + ", " + spawnPoint.getPosition().getY() + ": " + spawnPoint.getDenySpawn());
        }
        Console.WriteLine("try " + monsterSpawn.Count + " - " + spawnedMonstersOnMap.get());
        Console.WriteLine("----------------------------------");
        */

        if (YamlConfig.config.server.USE_ENABLE_FULL_RESPAWN)
        {
            return (GetMaxMobCount() - spawnedMonstersOnMap.get());
        }

        int maxNumShouldSpawn = (int)Math.Ceiling(getCurrentSpawnRate(numPlayers) * GetMaxMobCount());
        return maxNumShouldSpawn - spawnedMonstersOnMap.get();
    }

    private int GetMaxMobCount()
    {
        return (int)Math.Ceiling(monsterSpawn.Count * ActualMonsterRate);
    }

    public async Task respawn()
    {
        if (!_allowSummons)
        {
            return;
        }


        int numPlayers;

        numPlayers = characters.Count;

        if (numPlayers == 0)
        {
            return;
        }

        int numShouldSpawn = getNumShouldSpawn(numPlayers);
        if (numShouldSpawn > 0)
        {
            List<SpawnPoint> randomSpawn = new(getMonsterSpawn());
            Collections.shuffle(randomSpawn);
            short spawned = 0;
            foreach (SpawnPoint spawnPoint in randomSpawn)
            {
                if (spawnPoint.shouldSpawn())
                {
                    await spawnPoint.SpawnMonster();
                    spawned++;

                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }

        foreach (var sp in _bossSp.Values)
        {
            if (sp.shouldSpawn())
                await sp.SpawnMonster();
        }
    }

    public long Period { get; } = YamlConfig.config.server.RESPAWN_INTERVAL;
    public long Next { get; set; }
    public TickableStatus Status { get; protected set; }

    public long RespawnInterval { get; set; } = YamlConfig.config.server.RESPAWN_INTERVAL;
    long RespawnNext;

    public async Task OnTick(long now)
    {
        if (this.IsAvailable())
        {
            await Send(new MapTickCommand(async m =>
            {
                if (RespawnNext <= now)
                {
                    await respawn();

                    RespawnNext = now + RespawnInterval;
                }

                foreach (var item in getMapObjects())
                {
                    if (item is ITickable tickable)
                    {
                        await tickable.OnTick(now);

                        if (tickable.Status == TickableStatus.Remove)
                        {
                            if (item is Kite || item is Mist)
                            {
                                await RemoveMapObject(item, chr => item.sendDestroyData(chr.Client));
                            }

                            else if (item is MapItem mapItem)
                            {
                                await makeDisappearItemFromMap(mapItem);
                            }
                        }
                    }
                }

                if (MapEffect != null)
                {
                    await MapEffect.OnTick(now);

                    if (MapEffect.Status == TickableStatus.Remove)
                    {
                        await BroadcastAll(chr => chr.SendPacket(MapEffect.makeDestroyData()));
                        MapEffect = null;
                    }
                }

                await aggroMonitor.OnTick(now);
            }));
        }
    }

    public int getHPDec() => SourceTemplate.DecHP;
    public int getHPDecProtect() => SourceTemplate.ProtectItem;

    public float getRecovery() => SourceTemplate.RecoveryRate;

    private int hasBoat()
    {
        return !SourceTemplate.HasShip ? 0 : (docked ? 1 : 2);
    }

    public void setDocked(bool isDocked)
    {
        this.docked = isDocked;
    }


    public int getSeats() => SourceTemplate.SeatCount;

    public void setOxQuiz(bool b)
    {
        this._isOxQuiz = b;
    }

    public bool isOxQuiz()
    {
        return _isOxQuiz;
    }

    private bool hasForcedEquip()
    {
        return SourceTemplate.FieldType == 81 || SourceTemplate.FieldType == 82;
    }

    public async Task clearDrops()
    {
        await ProcessMapObject(x => x.getType() == MapObjectType.ITEM, async i =>
        {
            await pickItemDrop(PacketCreator.removeItemFromMap(i.getObjectId(), DropLeaveFieldType.Expired, 0), (MapItem)i);
        });
    }

    public int getFieldLimit() => SourceTemplate.FieldLimit;

    public void allowSummonState(bool b)
    {
        _allowSummons = b;
    }

    public bool getSummonState()
    {
        return _allowSummons;
    }

    public async Task warpEveryone(int to)
    {
        List<Player> players = new(getAllPlayers());

        foreach (Player chr in players)
        {
            await chr.changeMap(to);
        }
    }

    public async Task warpEveryone(int to, int pto)
    {
        List<Player> players = new(getAllPlayers());

        foreach (Player chr in players)
        {
            await chr.changeMap(to, pto);
        }
    }

    private bool specialEquip()
    {
        //Maybe I shouldn't use fieldType :\
        return SourceTemplate.FieldType == 4 || SourceTemplate.FieldType == 19;
    }

    public async Task warpOutByTeam(int team, int mapid)
    {
        List<Player> chars = new(getAllPlayers());
        foreach (Player chr in chars)
        {
            if (chr != null)
            {
                if (chr.getTeam() == team)
                {
                    await chr.changeMap(mapid);
                }
            }
        }
    }

    public virtual async Task startEvent(Player chr)
    {
        if (this.mapid == MapId.EVENT_PHYSICAL_FITNESS)
        {
            chr.Fitness = new Fitness(chr);
            await chr.Fitness.startFitness();
        }
        else if (this.mapid == MapId.EVENT_OLA_OLA_1 || this.mapid == MapId.EVENT_OLA_OLA_2 ||
                this.mapid == MapId.EVENT_OLA_OLA_3 || this.mapid == MapId.EVENT_OLA_OLA_4)
        {
            chr.Ola = new Ola(chr);
            await chr.Ola.startOla();
        }
    }

    public bool eventStarted()
    {
        return eventstarted;
    }

    public void startEvent()
    {
        setEventStarted(true);
    }

    public void setEventStarted(bool @event)
    {
        this.eventstarted = @event;
    }

    public bool isStartingEventMap()
    {
        return this.mapid == MapId.EVENT_PHYSICAL_FITNESS || this.mapid == MapId.EVENT_OX_QUIZ ||
                this.mapid == MapId.EVENT_FIND_THE_JEWEL || this.mapid == MapId.EVENT_OLA_OLA_0 || this.mapid == MapId.EVENT_OLA_OLA_1;
    }

    public async Task clearMapObjects()
    {
        await clearDrops();
        await killAllMonsters();
        await resetReactors();
    }


    public async Task resetPQ(int difficulty = 1)
    {
        await resetMapObjects(difficulty, true);
    }

    public async Task resetMapObjects()
    {
        await resetMapObjects(1, false);
    }

    async Task resetMapObjects(int difficulty, bool isPq)
    {
        await clearMapObjects();

        restoreMapSpawnPoints();
        await instanceMapFirstSpawn();
    }

    public async Task broadcastShip(bool state)
    {
        await broadcastMessage(PacketCreator.boatPacket(state));
        this.setDocked(state);
    }

    public async Task broadcastEnemyShip(bool state)
    {
        await broadcastMessage(PacketCreator.crogBoatPacket(state));
        IsPirateDocked = state;
        this.setDocked(state);
    }

    public bool isHorntailDefeated()
    {   // all parts of dead horntail can be found here?
        for (int i = MobId.DEAD_HORNTAIL_MIN; i <= MobId.DEAD_HORNTAIL_MAX; i++)
        {
            if (getMonsterById(i) == null)
            {
                return false;
            }
        }

        return true;
    }



    public bool claimOwnership(Player chr)
    {
        if (mapOwner == null)
        {
            this.mapOwner = chr;
            chr.setOwnedMap(this);

            mapOwnerLastActivityTime = ChannelServer.Node.getCurrentTime();

            getChannelServer().MapOwnershipManager.RegisterOwnedMap(this);
            return true;
        }
        else
        {
            return chr == mapOwner;
        }
    }

    public Player? unclaimOwnership()
    {
        var lastOwner = this.mapOwner;
        return unclaimOwnership(lastOwner) ? lastOwner : null;
    }

    public bool unclaimOwnership(Player? chr)
    {
        if (chr != null && mapOwner == chr)
        {
            this.mapOwner = null;
            chr.setOwnedMap(null);

            mapOwnerLastActivityTime = long.MaxValue;

            getChannelServer().MapOwnershipManager.UnregisterOwnedMap(this);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void refreshOwnership()
    {
        mapOwnerLastActivityTime = ChannelServer.Node.getCurrentTime();
    }

    public async Task<bool> isOwnershipRestricted(Player chr)
    {
        Player? owner = mapOwner;

        if (owner != null)
        {
            if (owner != chr && !owner.isPartyMember(chr))
            {    // thanks Vcoc & BHB for suggesting the map ownership feature
                await chr.showMapOwnershipInfo(owner);
                return true;
            }
            else
            {
                this.refreshOwnership();
            }
        }

        return false;
    }

    public async Task checkMapOwnerActivity()
    {
        long timeNow = ChannelServer.Node.getCurrentTime();
        if (timeNow - mapOwnerLastActivityTime > 60000)
        {
            if (unclaimOwnership() != null)
            {
                await Pink(nameof(ClientMessage.Map_Ownership_Free));
            }
        }
    }

    protected virtual Task SetMonsterInfo(Monster monster)
    {
        return Task.CompletedTask;
    }

    public bool isCPQMap()
    {
        switch (this.getId())
        {
            case 980000101:
            case 980000201:
            case 980000301:
            case 980000401:
            case 980000501:
            case 980000601:

            case 980031100:
            case 980032100:
            case 980033100:
                return true;
        }
        return false;
    }

    public bool isCPQMap2()
    {
        switch (this.getId())
        {
            case 980031100:
            case 980032100:
            case 980033100:
                return true;
        }
        return false;
    }

    public bool isCPQLobby()
    {
        switch (this.getId())
        {
            case 980000100:
            case 980000200:
            case 980000300:
            case 980000400:
            case 980000500:
            case 980000600:
                return true;
        }
        return false;
    }

    public bool isBlueCPQMap()
    {
        switch (this.getId())
        {
            case 980000501:
            case 980000601:
            case 980031200:
            case 980032200:
            case 980033200:
                return true;
        }
        return false;
    }

    public bool isPurpleCPQMap()
    {
        switch (this.getId())
        {
            case 980000301:
            case 980000401:
            case 980031200:
            case 980032200:
            case 980033200:
                return true;
        }
        return false;
    }


    public bool isCPQWinnerMap()
    {
        switch (this.getId())
        {
            case 980000103:
            case 980000203:
            case 980000303:
            case 980000403:
            case 980000503:
            case 980000603:
            case 980031300:
            case 980032300:
            case 980033300:
                return true;
        }
        return false;
    }

    public bool isCPQLoserMap()
    {
        switch (this.getId())
        {
            case 980000104:
            case 980000204:
            case 980000304:
            case 980000404:
            case 980000504:
            case 980000604:
            case 980031400:
            case 980032400:
            case 980033400:
                return true;
        }
        return false;
    }


    bool disposed = false;
    public virtual async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;
        Status = TickableStatus.Remove;

        await clearMapObjects();

        ChannelServer.OnWorldMobRateChanged -= UpdateMapActualMobRate;

        EventInstanceManager = null;
        portals.Clear();
        MapEffect = null;

        monsterSpawn.Clear();
        _bossSp.Clear();
        _chrVisibleMapObjects.Clear();

        aggroMonitor.dispose();

        await CommandLoop.DisposeAsync();
    }

    #region Objects: Player
    private Dictionary<int, Player> characters = new();

    public async Task movePlayer(Player player, Point newPosition)
    {
        player.setPosition(newPosition);

        if (!UseRangedView)
            return;

        foreach (var mo in getMapObjects())
        {
            if (mo is Player mapChr)
            {
                if (mapChr.IsVisibleForPlayer(player))
                {
                    await SetPlayerVisibleObject(player, mapChr, false);
                    await SetPlayerVisibleObject(mapChr, player, false);
                }
                else
                {
                    await SetPlayerInvisibleObject(player, mapChr, false);
                    await SetPlayerInvisibleObject(mapChr, player, false);
                }
            }
            else
            {
                if (mo.IsVisibleForPlayer(player))
                {
                    await SetPlayerVisibleObject(player, mo);
                }
                else
                {
                    await SetPlayerInvisibleObject(player, mo);
                }
            }

        }
    }


    public int countPlayers()
    {
        return getAllPlayers().Count;
    }

    public List<Player> getAllPlayers()
    {
        return characters.Values.ToList();
    }

    public int getNumPlayersInArea(int index)
    {
        return getNumPlayersInRect(getArea(index));
    }

    public int getNumPlayersInRect(Rectangle rect)
    {
        return getAllPlayers().Count(x => rect.Contains(x.getPosition()));
    }

    protected virtual async Task OnPlayerEnter(Player chr)
    {
        ChannelServer.NodeService.TeamManager.ChannelNotify(chr);

        if (IsPirateDocked)
        {
            await chr.SendPacket(PacketCreator.musicChange("Bgm04/ArabPirate"));
            await chr.SendPacket(PacketCreator.crogBoatPacket(true));
        }

        chr.visitMap(this);

        // 恢复当前地图的状态
        EventInstanceManager?.recoverOpenedGate(chr, Id);

        await chr.SendPacket(PacketCreator.environmentMoveList(getEnvironment()));

        // 可能离开了副本，新的地图没有EventInstanceManager
        chr.getEventInstance()?.afterChangedMap(chr, Id);
    }

    public async Task addPlayer(Player chr)
    {
        var isChrHidden = chr.isHidden();
        if (!await AddMapObject(chr, async c =>
        {
            await c.SendPacket(PacketCreator.spawnPlayerMapObject(chr.Client, chr, true));
            if (isChrHidden)
            {
                await c.SendPacket(PacketCreator.giveForeignBuff(chr.getId(), new BuffStatValue(BuffStat.DARKSIGHT, 0)));
            }
        }, false))
        {
            return;
        }

        if (isChrHidden)
            await chr.SendPacket(PacketCreator.getGMEffect(0x10, 1));

        GameMetrics.MapPlayerCount.Add(1, new KeyValuePair<string, object?>("Channel", ChannelServer.InstanceName), new KeyValuePair<string, object?>("Map", InstanceName));

        await chr.updateActiveEffects();

        chr.MapDamageNext = ChannelServer.Node.getCurrentTime() + chr.MapDamagePeriod;

        if (!string.IsNullOrEmpty(SourceTemplate.OnFirstUserEnter))
        {
            if (!chr.hasEntered(Id))
            {
                await chr.getClient().CurrentServer.NodeService.PluginManager.MapFirstEnterScript(chr.getClient(), this);
                chr.enteredScript(Id);
            }
        }

        if (!string.IsNullOrEmpty(SourceTemplate.OnUserEnter))
        {
            if (SourceTemplate.OnUserEnter.Equals("cygnusTest") && !MapId.isCygnusIntro(mapid))
            {
                chr.SaveLocation(SavedLocationType.INTRO);
            }

            await chr.getClient().CurrentServer.NodeService.PluginManager.MapEnterScript(chr.getClient(), this);
        }

        if (FieldLimit.CANNOTUSEMOUNTS.check(SourceTemplate.FieldLimit) && chr.getBuffedValue(BuffStat.MONSTER_RIDING) != null)
        {
            await chr.cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);
            await chr.cancelBuffStats(BuffStat.MONSTER_RIDING);
        }


        if (MiniDungeonInfo.isDungeonMap(mapid))
        {
            var mmd = chr.getClient().getChannelServer().getMiniDungeon(mapid);
            if (mmd != null)
            {
                await mmd.registerPlayer(chr);
            }
        }
        else if (GameConstants.isAriantColiseumArena(mapid))
        {
            int pqTimer = (int)TimeSpan.FromMinutes(10).TotalSeconds;
            await chr.SendPacket(PacketCreator.getClock(pqTimer));
        }

        await chr.removeSandboxItems();

        if (chr.getChalkboard() != null)
        {
            if (!GameConstants.isFreeMarketRoom(mapid))
            {
                await chr.SendPacket(PacketCreator.useChalkboard(chr, false)); // update player's chalkboard when changing maps found thanks to Vcoc
            }
            else
            {
                chr.setChalkboard(null);
            }
        }


        if (isStartingEventMap() && !eventStarted())
        {
            chr.getMap().getPortal("join00")!.setPortalStatus(false);
        }
        if (hasForcedEquip())
        {
            await chr.SendPacket(PacketCreator.showForcedEquip(-1));
        }
        if (specialEquip())
        {
            await chr.SendPacket(PacketCreator.coconutScore(0, 0));
            await chr.SendPacket(PacketCreator.showForcedEquip(chr.getTeam()));
        }

        // 访问商店/开店时应该没办法切换地图
        //if (chr.VisitingShop != null)
        //{
        //    addMapObject(chr.VisitingShop);
        //}

        var dragon = chr.getDragon();
        if (dragon != null)
        {
            dragon.setPosition(chr.getPosition());
            await this.AddMapObject(dragon, c => c.SendPacket(PacketCreator.spawnDragon(dragon)));
        }

        StatEffect? summonStat = chr.getStatForBuff(BuffStat.SUMMON);
        if (summonStat != null)
        {
            var summon = chr.getSummonByKey(summonStat.getSourceId())!;
            summon.setPosition(chr.getPosition());
            await spawnSummon(summon);
        }
        if (MapEffect != null)
        {
            await MapEffect.sendStartData(chr.getClient());
        }
        await chr.SendPacket(PacketCreator.resetForcedStats());
        if (MapId.isGodlyStatMap(mapid))
        {
            await chr.SendPacket(PacketCreator.aranGodlyStats());
        }
        if (chr.getEventInstance() != null && chr.getEventInstance()!.isTimerStarted())
        {
            await chr.SendPacket(PacketCreator.getClock((int)(chr.getEventInstance()!.getTimeLeft() / 1000)));
        }
        if (chr.Fitness != null && chr.Fitness.isTimerStarted())
        {
            await chr.SendPacket(PacketCreator.getClock((int)(chr.Fitness.getTimeLeft() / 1000)));
        }

        if (chr.Ola != null && chr.Ola.isTimerStarted())
        {
            await chr.SendPacket(PacketCreator.getClock((int)(chr.Ola.getTimeLeft() / 1000)));
        }

        if (mapid == MapId.EVENT_SNOWBALL)
        {
            await chr.SendPacket(PacketCreator.rollSnowBall());
        }

        if (hasClock())
        {
            var cal = ChannelServer.Node.GetCurrentTimeDateTimeOffset().ToLocalTime();
            await chr.SendPacket(PacketCreator.getClockTime(cal.Hour, cal.Minute, cal.Second));
        }
        if (hasBoat() > 0)
        {
            await chr.SendPacket(PacketCreator.boatPacket(hasBoat() == 1));
        }
        await sendObjectPlacement(chr);
        await OnPlayerEnter(chr);
    }
    private async Task sendObjectPlacement(Player chr)
    {
        var allMapObjects = getMapObjects();

        foreach (var o in allMapObjects)
        {
            if (o.IsVisibleForPlayer(chr))
            {
                await SetPlayerVisibleObject(chr, o, o is not MapPet);

                if (o is Monster monster && !monster.isFake())
                    await monster.aggroUpdateController();
            }
            else
            {
                // 没必要
                // SetPlayerInvisibleObject(chr, o);
            }
        }
    }



    Dictionary<Player, HashSet<IMapObject>> _chrVisibleMapObjects = new();
    public bool IsMapObjectVisibleForPlayerCached(Player player, IMapObject mapObj)
    {
        return _chrVisibleMapObjects.GetValueOrDefault(player)?.Contains(mapObj) ?? false;
    }
    public async Task SetPlayerVisibleObject(Player chr, IMapObject mapObj, bool sendSpawnData = true)
    {
        if (_chrVisibleMapObjects.TryGetValue(chr, out var list))
        {
            if (list.Add(mapObj))
            {
                if (sendSpawnData)
                {
                    await mapObj.sendSpawnData(chr.Client);
                }
            }
        }
        else
        {
            _chrVisibleMapObjects[chr] = [mapObj];

            if (sendSpawnData)
            {
                await mapObj.sendSpawnData(chr.Client);
            }
        }



    }
    public async Task SetPlayerInvisibleObject(Player chr, IMapObject mapObj, bool sendDestroyData = true)
    {
        if (_chrVisibleMapObjects.TryGetValue(chr, out var list))
        {
            if (list.Remove(mapObj))
            {
                if (sendDestroyData)
                {
                    await mapObj.sendDestroyData(chr.Client);
                }
            }
        }
    }

    public async Task removePlayer(Player chr)
    {
        if (XiGuai?.Controller == chr)
            XiGuai = null;

        if (MiniDungeonInfo.isDungeonMap(mapid))
        {
            var mmd = ChannelServer.getMiniDungeon(mapid);
            if (mmd != null)
            {
                if (!await mmd.unregisterPlayer(chr))
                {
                    ChannelServer.removeMiniDungeon(mapid);
                }
            }
        }

        await RemoveMapObject(chr, async mapChr =>
        {
            await mapChr.SendPacket(PacketCreator.removePlayerFromMap(chr.getId()));
        });
        GameMetrics.MapPlayerCount.Add(-1, new KeyValuePair<string, object?>("Channel", ChannelServer.InstanceName), new KeyValuePair<string, object?>("Map", InstanceName));
    }

    public Dictionary<int, Player> getMapPlayers()
    {
        return characters;
    }

    public Player? getCharacterById(int id)
    {
        return characters.GetValueOrDefault(id);
    }

    public List<Player> getPlayersInRange(Rectangle box)
    {
        List<Player> character = new();

        foreach (Player chr in getAllPlayers())
        {
            if (box.Contains(chr.getPosition()))
            {
                character.Add(chr);
            }
        }
        return character;
    }

    public int countAlivePlayers()
    {
        return getAllPlayers().Count(x => x.isAlive());
    }
    #endregion

    #region Objects: MapItem
    private BoundedCollection<MapItem> droppedItems;

    private List<MapItem> getDroppedItems()
    {
        return new(droppedItems);
    }

    public int getDroppedItemsCountById(int itemid)
    {
        int count = 0;
        foreach (MapItem mmi in getDroppedItems())
        {
            if (mmi.getItemId() == itemid)
            {
                count++;
            }
        }

        return count;
    }

    public Task pickItemDrop(Packet pickupPacket, MapItem mdrop)
    {
        // mdrop must be already locked and not-pickedup checked at this point
        mdrop.setPickedUp(true);

        return RemoveMapObject(mdrop, mapChr => mapChr.SendPacket(pickupPacket));
    }

    public int countItems()
    {
        return droppedItems.Count;
    }

    public List<MapItem> getItems()
    {
        return getDroppedItems();
    }

    public async Task<bool> makeDisappearItemFromMap(MapItem? mapitem)
    {
        if (mapitem != null && mapitem == getMapObject(mapitem.getObjectId()))
        {
            if (mapitem.isPickedUp())
            {
                return true;
            }

            await pickItemDrop(PacketCreator.removeItemFromMap(mapitem.getObjectId(), DropLeaveFieldType.Expired, 0), mapitem);
            return true;
        }

        return false;
    }
    #endregion

    public async Task Broadcast(int exceptChrId, double rangeSq, Point? rangedFrom, Func<Player, Task> effectPlayer)
    {
        foreach (Player chr in getAllPlayers())
        {
            if (chr.Id != exceptChrId)
            {
                if (rangeSq < double.PositiveInfinity)
                {
                    if (rangedFrom != null && MapGlobalData.IsObjectInRange(chr, rangedFrom.Value, rangeSq))
                    {
                        await effectPlayer(chr);
                    }
                }
                else
                {
                    await effectPlayer(chr);
                }
            }
        }
    }
    public async Task BroadcastAll(Func<Player, Task> effectPlayer, int exceptId = -1)
    {
        await Broadcast(exceptId, double.PositiveInfinity, null, effectPlayer);
    }

    public async Task TypedMessage(int type, string messageKey, params string[] param)
    {
        await BroadcastAll(e => e.TypedMessage(type, messageKey, param));
    }
    public Task Notice(string key, params string[] param) => TypedMessage(0, key, param);

    public Task Popup(string key, params string[] param) => TypedMessage(1, key, param);

    public Task TopScrolling(string key, params string[] param) => TypedMessage(4, key, param);

    public Task Pink(string key, params string[] param) => TypedMessage(5, key, param);

    public Task LightBlue(string key, params string[] param) => TypedMessage(6, key, param);

    public Task Yellow(string key, params string[] param) => TypedMessage(-1, key, param);
    public Task EarnTitle(string key, params string[] param) => TypedMessage(-2, key, param);
    public Task Dialog(string key, params string[] param) => TypedMessage(-3, key, param);

    public Task LightBlue(Func<ClientCulture, string> action)
    {
        return BroadcastAll(e => e.LightBlue(action));
    }

    public Task Send(ICommand command) => CommandLoop.Register(command);

    public Task Send(Action<IMap> action) => Send(new MapDelegateCommand(action));

    public Task Send(Func<IMap, Task> action) => Send(new AsyncMapDelegateCommand(action));
}
