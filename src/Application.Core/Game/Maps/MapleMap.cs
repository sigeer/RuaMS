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
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Performance;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.Game.Skills;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using client.autoban;
using client.inventory;
using client.status;
using net.server.coordinator.world;
using net.server.services.task.channel;
using scripting.Event;
using scripting.map;
using server;
using server.events.gm;
using server.life;
using server.maps;
using System.Collections.Concurrent;
using System.Threading.Channels;
using tools;
using ZLinq;


namespace Application.Core.Game.Maps;

public class MapleMap : IMap
{
    public string InstanceName { get; }

    protected ILogger log;


    private Dictionary<int, IMapObject> mapobjects = new();
    private HashSet<int> selfDestructives = new();
    protected List<SpawnPoint> monsterSpawn = new();
    private List<SpawnPoint> allMonsterSpawn = new();
    private AtomicInteger spawnedMonstersOnMap = new AtomicInteger(0);

    private Dictionary<int, Portal> portals;
    private Dictionary<string, int> environment = new();

    private Queue<WeakReference<IMapObject>> registeredDrops = new();
    private System.Threading.Channels.Channel<Action> statUpdateRunnables;

    private List<Rectangle> areas;
    public FootholdTree Footholds { get; }
    private KeyValuePair<int, int> xLimits;  // caches the min and max x's with available footholds
    private Rectangle mapArea;
    private int mapid;
    private int runningOid = 1000000001;

    private bool docked = false;
    bool pirateDocked = false;
    public AbstractEventInstanceManager? EventInstanceManager { get; private set; }
    public bool IsTrackedByEvent { get; set; }

    private string mapName;
    private string streetName;
    private MapEffect? mapEffect = null;

    private bool dropsOn = true;
    private string onFirstUserEnter;
    private string onUserEnter;
    private MonsterAggroCoordinator aggroMonitor;   // aggroMonitor activity in sync with itemMonitor
    private ScheduledFuture? itemMonitor = null;
    private ScheduledFuture? expireItemsTask = null;
    private ScheduledFuture? characterStatUpdateTask = null;
    private short itemMonitorTimeout;
    public TimeMob? TimeMob { get; set; }
    private bool _allowSummons = true; // All maps should have this true at the beginning
    private IPlayer? mapOwner = null;
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

    //locks
    ReaderWriterLockSlim chrLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    ReaderWriterLockSlim objectLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    // due to the nature of loadMapFromWz (synchronized), sole function that calls 'generateMapDropRangeCache', this lock remains optional.
    private static object bndLock = new object();
    public WorldChannel ChannelServer { get; }
    public XiGuai? XiGuai { get; set; }
    public MapTemplate SourceTemplate { get; }
    public MapleMap(MapTemplate mapTemplate, WorldChannel worldChannel, AbstractEventInstanceManager? eim)
    {
        SourceTemplate = mapTemplate;
        Id = mapTemplate.TemplateId;
        this.mapid = mapTemplate.TemplateId;
        ChannelServer = worldChannel;
        this.MonsterRate = 1;
        aggroMonitor = new MonsterAggroCoordinator(this);
        onFirstUserEnter = string.IsNullOrEmpty(mapTemplate.OnFirstUserEnter) ? Id.ToString() : mapTemplate.OnFirstUserEnter;
        onUserEnter = string.IsNullOrEmpty(mapTemplate.OnUserEnter) ? Id.ToString() : mapTemplate.OnUserEnter;
        mapName = "";
        streetName = "";
        EventInstanceManager = eim;
        var range = new RangeNumberGenerator(Id, 100000000);
        log = LogFactory.GetLogger($"Map/{range}");

        if (EventInstanceManager == null)
            InstanceName = $"Channel:{worldChannel.getId()}_EventInstance:None_Map:{Id}";
        else
            InstanceName = $"Channel:{worldChannel.getId()}_EventInstance:{EventInstanceManager.getName()}_Map:{Id}_{GetHashCode()}";

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

        statUpdateRunnables = System.Threading.Channels.Channel.CreateUnbounded<Action>();
        Task.Run(async () =>
        {
            while (await statUpdateRunnables.Reader.WaitToReadAsync())
            {
                while (statUpdateRunnables.Reader.TryRead(out var action))
                {
                    action();
                }
            }
        });
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

    public void broadcastPacket(IPlayer source, Packet packet)
    {
        broadcastPacket(packet, chr => chr != source);
    }

    public void broadcastGMPacket(IPlayer source, Packet packet)
    {
        broadcastPacket(packet, chr => chr != source && chr.gmLevel() >= source.gmLevel());
    }

    private void broadcastPacket(Packet packet, Func<IPlayer, bool> chrFilter)
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (var item in getAllPlayers())
            {
                if (chrFilter(item))
                    item.sendPacket(packet);
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public void toggleDrops()
    {
        this.dropsOn = !dropsOn;
    }

    private static double getRangedDistance()
    {
        return YamlConfig.config.server.USE_MAXRANGE ? double.PositiveInfinity : 722500;
    }

    public int getId()
    {
        return mapid;
    }

    public WorldChannel getChannelServer()
    {
        return ChannelServer;
    }

    public IMap getReturnMap()
    {
        if (SourceTemplate.ReturnMap == MapId.NONE)
        {
            return this;
        }
        return getChannelServer().getMapFactory().getMap(SourceTemplate.ReturnMap);
    }

    public int getReturnMapId() => SourceTemplate.ReturnMap;
    public IMap getForcedReturnMap()
    {
        return getChannelServer().getMapFactory().getMap(SourceTemplate.ForcedReturn);
    }

    public int getForcedReturnId() => SourceTemplate.ForcedReturn;

    public int getTimeLimit()
    {
        return SourceTemplate.TimeLimit;
    }

    public void setReactorState()
    {
        foreach (IMapObject o in getMapObjects())
        {
            if (o.getType() == MapObjectType.REACTOR && o is Reactor mr)
            {
                if (mr.getState() < 1)
                {
                    mr.lockReactor();
                    try
                    {
                        mr.resetReactorActions(1);
                        broadcastMessage(PacketCreator.triggerReactor((Reactor)o, 1));
                    }
                    finally
                    {
                        mr.unlockReactor();
                    }
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

    public void addPlayerNPCMapObject(IMapObject pnpcobject)
    {
        objectLock.EnterWriteLock();
        try
        {
            this.mapobjects.AddOrUpdate(pnpcobject.getObjectId(), pnpcobject);
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    public void addMapObject(IMapObject mapobject)
    {
        int curOID = getUsableOID();

        objectLock.EnterWriteLock();
        try
        {
            mapobject.setObjectId(curOID);
            this.mapobjects.AddOrUpdate(curOID, mapobject);
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
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

    private void spawnAndAddRangedMapObject(IMapObject mapobject, Action<IChannelClient>? packetbakery, Func<IPlayer, bool>? condition = null)
    {
        List<IPlayer> inRangeCharacters = new();
        int curOID = getUsableOID();

        chrLock.EnterReadLock();
        objectLock.EnterWriteLock();
        try
        {
            mapobject.setObjectId(curOID);
            this.mapobjects.AddOrUpdate(curOID, mapobject);
            foreach (IPlayer chr in getAllPlayers())
            {
                if (condition == null || condition.Invoke(chr))
                {
                    if (chr.getPosition().distanceSq(mapobject.getPosition()) <= getRangedDistance())
                    {
                        inRangeCharacters.Add(chr);
                        chr.addVisibleMapObject(mapobject);
                    }
                }
            }
        }
        finally
        {
            objectLock.ExitWriteLock();
            chrLock.ExitReadLock();
        }

        foreach (IPlayer chr in inRangeCharacters)
        {
            packetbakery?.Invoke(chr.Client);
        }
    }

    //private void spawnRangedMapObject(IMapObject mapobject, DelayedPacketCreation packetbakery, SpawnCondition condition)
    //{
    //    List<IPlayer> inRangeCharacters = new();

    //    chrLock.EnterReadLock();
    //    try
    //    {
    //        int curOID = getUsableOID();
    //        mapobject.setObjectId(curOID);
    //        foreach (IPlayer chr in characters)
    //        {
    //            if (condition == null || (condition.canSpawn?.Invoke(chr) ?? false))
    //            {
    //                if (chr.getPosition().distanceSq(mapobject.getPosition()) <= getRangedDistance())
    //                {
    //                    inRangeCharacters.Add(chr);
    //                    chr.addVisibleMapObject(mapobject);
    //                }
    //            }
    //        }
    //    }
    //    finally
    //    {
    //        chrLock.ExitReadLock();
    //    }

    //    foreach (IPlayer chr in inRangeCharacters)
    //    {
    //        packetbakery.sendPackets?.Invoke(chr.getClient());
    //    }
    //}

    private int getUsableOID()
    {
        objectLock.EnterReadLock();
        try
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
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    public void removeMapObject(int objectId)
    {
        objectLock.EnterWriteLock();
        try
        {
            mapobjects.Remove(objectId);
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    void RemoveMapObjects(IEnumerable<int> objectIds)
    {
        objectLock.EnterWriteLock();
        try
        {
            foreach (var objectId in objectIds)
            {
                mapobjects.Remove(objectId);
            }
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    public void removeMapObject(IMapObject obj)
    {
        removeMapObject(obj.getObjectId());
    }

    private Point? calcPointBelow(Point initial)
    {
        return Footholds.FindBelowPoint(initial);
    }

    public void generateMapDropRangeCache()
    {
        Monitor.Enter(bndLock);
        try
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
                MapGlobalData.dropBoundsCache.Add(Id, xLimits);
            }
        }
        finally
        {
            Monitor.Exit(bndLock);
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

    public bool canDeployDoor(Point pos)
    {
        Point? toStep = calcPointBelow(pos);
        return toStep != null && toStep.Value.distance(pos) <= 42;
    }

    /**
     * Fetches angle relative between spawn and door points where 3 O'Clock is 0
     * and 12 O'Clock is 270 degrees
     *
     * @param spawnPoint
     * @param doorPoint
     * @return angle in degress from 0-360.
     */
    private static double getAngle(Point doorPoint, Point spawnPoint)
    {
        double dx = doorPoint.X - spawnPoint.X;
        // Minus to correct for coord re-mapping
        double dy = -(doorPoint.Y - spawnPoint.Y);

        double inRads = Math.Atan2(dy, dx);

        // We need to map to coord system when 0 degree is at 3 O'clock, 270 at 12 O'clock
        if (inRads < 0)
        {
            inRads = Math.Abs(inRads);
        }
        else
        {
            inRads = 2 * Math.PI - inRads;
        }

        return (180 / Math.PI) * inRads;
    }

    /**
     * Converts angle in degrees to rounded cardinal coordinate.
     *
     * @param angle
     * @return correspondent coordinate.
     */
    public static string getRoundedCoordinate(double angle)
    {
        string[] directions = { "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E" };
        return directions[(int)Math.Round(((angle % 360) / 45))];
    }

    public KeyValuePair<string, int>? getDoorPositionStatus(Point pos)
    {
        var portal = findClosestPlayerSpawnpoint(pos);
        if (portal == null)
            return null;

        double angle = getAngle(portal.getPosition(), pos);
        double distn = pos.distanceSq(portal.getPosition());

        if (distn <= 777777.7)
        {
            return null;
        }

        distn = Math.Sqrt(distn);
        return new(getRoundedCoordinate(angle), (int)distn);
    }

    private byte dropItemsFromMonsterOnMap(List<DropEntry> dropEntry, byte dIndex, float chRate, DropType droptype, IPlayer chr, Monster mob, short dropDelay)
    {
        if (dropEntry.Count == 0)
        {
            return dIndex;
        }

        Collections.shuffle(dropEntry);

        Item idrop;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        var itemPos = mob.getPosition();
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
                            spawnMesoDrop((int)mesos, itemPos, mob, chr, false, droptype, dropDelay);
                        }
                    }
                }
                else
                {
                    if (ItemConstants.getInventoryType(de.ItemId) == InventoryType.EQUIP)
                    {
                        idrop = ii.randomizeStats(ii.getEquipById(de.ItemId));
                    }
                    else
                    {
                        idrop = new Item(de.ItemId, 0, (short)de.GetRandomCount());
                    }
                    spawnDrop(idrop, itemPos, mob, chr, false, droptype, de.QuestId, dropDelay);
                }
                dIndex++;
            }
        }

        return dIndex;
    }

    private void dropFromMonster(IPlayer chr, Monster mob, bool useBaseRate, short dropDelay)
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
        else if (chr.getParty() != null)
            dropType = DropType.OnwerWithTeam;

        float chrRate = 1;
        if (!useBaseRate)
        {
            chrRate = !mob.isBoss() ? chr.getDropRate() : chr.getBossDropRate();

            var stati = mob.getStati(MonsterStatus.SHOWDOWN);
            if (stati != null)
            {
                chrRate = (int)(chrRate * (stati.getStati().GetValueOrDefault(MonsterStatus.SHOWDOWN) / 100.0 + 1.0));
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
        index = dropItemsFromMonsterOnMap(dropEntry, index, chrRate, dropType, chr, mob, dropDelay);
        // Global Drops
        index = dropItemsFromMonsterOnMap(globalEntry, index, 0, dropType, chr, mob, dropDelay);
        // Quest Drops
        index = dropItemsFromMonsterOnMap(visibleQuestEntry, index, chrRate, dropType, chr, mob, dropDelay);
        dropItemsFromMonsterOnMap(otherQuestEntry, index, chrRate, dropType, chr, mob, dropDelay);
    }

    /// <summary>
    /// 偷窃获得掉落物
    /// </summary>
    /// <param name="list"></param>
    /// <param name="chr"></param>
    /// <param name="mob"></param>
    /// <param name="dropDelay"></param>
    public void DropItemFromMonsterBySteal(List<DropEntry> list, IPlayer chr, Monster mob, short dropDelay)
    {
        if (mob.dropsDisabled() || !dropsOn)
        {
            return;
        }

        DropType droptype = (DropType)(chr.getParty() != null ? 1 : 0);
        int chRate = 1000000;   // 偷窃成功概率已经计算
        byte d = 1;

        dropItemsFromMonsterOnMap(list, d, chRate, droptype, chr, mob, dropDelay);
    }

    public void dropFromFriendlyMonster(IPlayer chr, Monster mob)
    {
        dropFromMonster(chr, mob, true, 0);
    }

    public void dropFromReactor(IPlayer chr, Reactor reactor, Item drop, Point dropPos, short questid, short dropDelay = 0)
    {
        spawnDrop(drop, dropPos, reactor, chr, false, (DropType)(chr.getParty() != null ? 1 : 0), questid, dropDelay);
    }

    private void stopItemMonitor()
    {
        itemMonitor?.cancel(false);
        itemMonitor = null;

        expireItemsTask?.cancel(false);
        expireItemsTask = null;

        characterStatUpdateTask?.cancel(false);
        characterStatUpdateTask = null;
    }

    private void cleanItemMonitor()
    {
        objectLock.EnterWriteLock();
        try
        {
            registeredDrops = new(registeredDrops.Where(x => x != null));
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    private void startItemMonitor()
    {
        chrLock.EnterWriteLock();
        try
        {
            if (itemMonitor != null)
            {
                return;
            }

            itemMonitor = ChannelServer.Container.TimerManager.register(new MapTaskBase(this, "ItemMonitor", () =>
            {
                chrLock.EnterWriteLock();
                try
                {
                    if (characters.Count == 0)
                    {
                        if (itemMonitorTimeout == 0)
                        {
                            if (itemMonitor != null)
                            {
                                stopItemMonitor();
                                aggroMonitor.stopAggroCoordinator();
                            }

                            return;
                        }
                        else
                        {
                            itemMonitorTimeout--;
                        }
                    }
                    else
                    {
                        itemMonitorTimeout = 1;
                    }
                }
                finally
                {
                    chrLock.ExitWriteLock();
                }

                bool tryClean;
                objectLock.EnterReadLock();
                try
                {
                    tryClean = registeredDrops.Count > 70;
                }
                finally
                {
                    objectLock.ExitReadLock();
                }

                if (tryClean)
                {
                    cleanItemMonitor();
                }
            }), YamlConfig.config.server.ITEM_MONITOR_TIME, YamlConfig.config.server.ITEM_MONITOR_TIME);

            expireItemsTask = ChannelServer.Container.TimerManager.register(new MapTaskBase(this, "MapItemExpireCheck", makeDisappearExpiredItemDrops),
                YamlConfig.config.server.ITEM_EXPIRE_CHECK,
                YamlConfig.config.server.ITEM_EXPIRE_CHECK);

            itemMonitorTimeout = 1;
        }
        finally
        {
            chrLock.ExitWriteLock();
        }
    }

    private bool hasItemMonitor()
    {
        chrLock.EnterReadLock();
        try
        {
            return itemMonitor != null;
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    private void makeDisappearExpiredItemDrops()
    {
        List<MapItem> toDisappear = new();

        objectLock.EnterReadLock();
        try
        {
            long timeNow = ChannelServer.Container.getCurrentTime();

            foreach (var it in getDroppedItems())
            {
                if (it.ExpiredTime < timeNow)
                {
                    toDisappear.Add(it);
                }
            }
        }
        finally
        {
            objectLock.ExitReadLock();
        }

        foreach (MapItem mmi in toDisappear)
        {
            makeDisappearItemFromMap(mmi);
        }
    }

    public List<MapItem> updatePlayerItemDropsToParty(int partyid, int charid, List<IPlayer> partyMembers, IPlayer? partyLeaver)
    {
        List<MapItem> partyDrops = new();

        foreach (MapItem mdrop in getDroppedItems())
        {
            if (mdrop.getOwnerId() == charid)
            {
                mdrop.lockItem();
                try
                {
                    if (mdrop.isPickedUp())
                    {
                        continue;
                    }

                    mdrop.setPartyOwnerId(partyid);

                    Packet removePacket = PacketCreator.silentRemoveItemFromMap(mdrop.getObjectId());
                    Packet updatePacket = PacketCreator.updateMapItemObject(mdrop, partyLeaver == null);

                    foreach (IPlayer mc in partyMembers)
                    {
                        if (this.Equals(mc.getMap()))
                        {
                            mc.sendPacket(removePacket);

                            if (mc.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                            {
                                mc.sendPacket(updatePacket);
                            }
                        }
                    }

                    if (partyLeaver != null)
                    {
                        if (this.Equals(partyLeaver.getMap()))
                        {
                            partyLeaver.sendPacket(removePacket);

                            if (partyLeaver.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                            {
                                partyLeaver.sendPacket(PacketCreator.updateMapItemObject(mdrop, true));
                            }
                        }
                    }
                }
                finally
                {
                    mdrop.unlockItem();
                }
            }
            else if (partyid != -1 && mdrop.getPartyOwnerId() == partyid)
            {
                partyDrops.Add(mdrop);
            }
        }

        return partyDrops;
    }

    public void updatePartyItemDropsToNewcomer(IPlayer newcomer, List<MapItem> partyItems)
    {
        foreach (MapItem mdrop in partyItems)
        {
            mdrop.lockItem();
            try
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
                        newcomer.sendPacket(removePacket);

                        if (newcomer.needQuestItem(mdrop.getQuest(), mdrop.getItemId()))
                        {
                            newcomer.sendPacket(updatePacket);
                        }
                    }
                }
            }
            finally
            {
                mdrop.unlockItem();
            }
        }
    }

    private void spawnDrop(Item idrop, Point dropPos, IMapObject dropper, IPlayer chr, bool playerDrop, DropType droptype, short questid, short dropDelay)
    {
        var validPos = calcDropPos(dropPos, dropper.getPosition());

        MapItem mdrop = new MapItem(idrop, validPos, dropper, chr, droptype, playerDrop, questid);
        spawnAndAddRangedMapObject(mdrop, c =>
        {
            var chr1 = c.OnlinedCharacter;

            if (chr1.needQuestItem(questid, idrop.getItemId()))
            {
                mdrop.lockItem();
                try
                {
                    c.sendPacket(PacketCreator.dropItemFromMapObject(chr1, mdrop, dropper.getPosition(), validPos, 1, dropDelay));
                }
                finally
                {
                    mdrop.unlockItem();
                }
            }
        }, null);

        instantiateItemDrop(mdrop);
        activateItemReactors(mdrop, chr.getClient());
    }

    public void spawnMesoDrop(int meso, Point position, IMapObject dropper, IPlayer owner, bool playerDrop, DropType droptype, short dropDelay = 0)
    {
        var validPos = calcDropPos(position, dropper.getPosition());

        MapItem mdrop = new MapItem(meso, validPos, dropper, owner, droptype, playerDrop);
        spawnAndAddRangedMapObject(mdrop, c =>
        {
            mdrop.lockItem();
            try
            {
                c.sendPacket(PacketCreator.dropItemFromMapObject(c.OnlinedCharacter, mdrop, dropper.getPosition(), validPos, 1, dropDelay));
            }
            finally
            {
                mdrop.unlockItem();
            }
        }, null);

        instantiateItemDrop(mdrop);
    }

    public void disappearingItemDrop(IMapObject dropper, IPlayer owner, Item item, Point pos)
    {
        Point droppos = calcDropPos(pos, dropper.getPosition());
        MapItem mdrop = new MapItem(item, droppos, dropper, owner, DropType.OnwerWithTeam, false);

        mdrop.lockItem();
        try
        {
            broadcastItemDropMessage(mdrop, dropper.getPosition(), droppos, 3, rangedFrom: mdrop.getPosition());
        }
        finally
        {
            mdrop.unlockItem();
        }
    }

    public void disappearingMesoDrop(int meso, IMapObject dropper, IPlayer owner, Point pos)
    {
        Point droppos = calcDropPos(pos, dropper.getPosition());
        MapItem mdrop = new MapItem(meso, droppos, dropper, owner, DropType.OnwerWithTeam, false);

        mdrop.lockItem();
        try
        {
            broadcastItemDropMessage(mdrop, dropper.getPosition(), droppos, 3, rangedFrom: mdrop.getPosition());
        }
        finally
        {
            mdrop.unlockItem();
        }
    }

    public Monster? getMonsterById(int id)
    {
        return GetRequiredMapObjects<Monster>(MapObjectType.MONSTER, x => x.getId() == id).FirstOrDefault();
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

    public bool damageMonster(IPlayer chr, Monster monster, int damage, short dropDelay = 0)
    {
        if (monster.getId() == MobId.ZAKUM_1)
        {
            if (countMonster(MobId.ZAKUM_ARM_1, MobId.ZAKUM_ARM_8) > 0)
                return true;
        }
        if (monster.isAlive())
        {
            bool killed = monster.damage(chr, damage, false);

            var selfDestr = monster.getStats().selfDestruction();
            if (selfDestr != null && selfDestr.Hp > -1)
            {
                // should work ;p
                if (monster.getHp() <= selfDestr.Hp)
                {
                    killMonster(monster, chr, true, selfDestr.Action);
                    return true;
                }
            }
            if (killed)
            {
                killMonster(monster, chr, true, dropDelay);
            }
            return true;
        }
        return false;
    }

    public void broadcastBalrogVictory(string leaderName)
    {
        ChannelServer.Container.SendDropMessage(6,
            "[Victory] " + leaderName + "'s party has successfully defeated the Balrog! Praise to them, they finished with " + countAlivePlayers() + " players alive.");
    }

    public void broadcastHorntailVictory()
    {
        ChannelServer.Container.SendDropMessage(6,
            "[Victory] To the crew that have finally conquered Horned Tail after numerous attempts, I salute thee! You are the true heroes of Leafre!!");
    }

    public void broadcastZakumVictory()
    {
        ChannelServer.Container.SendDropMessage(6,
            "[Victory] At last, the tree of evil that for so long overwhelmed Ossyria has fallen. To the crew that managed to finally conquer Zakum, after numerous attempts, victory! You are the true heroes of Ossyria!!");
    }

    public void broadcastPinkBeanVictory(int channel)
    {
        ChannelServer.Container.SendDropMessage(6,
            "[Victory] In a swift stroke of sorts, the crew that has attempted Pink Bean at channel " + channel + " has ultimately defeated it. The Temple of Time shines radiantly once again, the day finally coming back, as the crew that managed to finally conquer it returns victoriously from the battlefield!!");
    }

    public bool removeKilledMonsterObject(Monster monster)
    {
        monster.lockMonster();
        try
        {
            if (monster.getHp() < 0)
            {
                return false;
            }

            spawnedMonstersOnMap.decrementAndGet();
            removeMapObject(monster);
            monster.disposeMapObject();
            if (monster.hasBossHPBar())
            {
                // thanks resinate for noticing boss HPbar not clearing after mob defeat in certain scenarios
                broadcastBossHpMessage(monster, monster.GetHashCode(), monster.makeBossHPBarPacket(), monster.getPosition());
            }

            return true;
        }
        finally
        {
            monster.unlockMonster();
        }
    }
    public void killMonster(Monster? monster, IPlayer? chr, bool withDrops, short dropDelay = 0) => killMonster(monster, chr, withDrops, 1, dropDelay);
    public void killMonster(Monster? monster, IPlayer? chr, bool withDrops, int animation, short dropDelay)
    {
        if (monster == null)
        {
            return;
        }

        if (chr == null)
        {
            if (removeKilledMonsterObject(monster))
            {
                monster.dispatchMonsterKilled(false);
                broadcastMessage(PacketCreator.killMonster(monster.getObjectId(), animation), monster.getPosition());
                monster.aggroSwitchController(null, false);
            }
        }
        else
        {
            if (removeKilledMonsterObject(monster))
            {
                try
                {
                    if (monster.getStats().getLevel() >= chr.getLevel() + 30 && !chr.isGM())
                    {
                        ChannelServer.Container.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, " for killing a " + monster.getName() + " which is over 30 levels higher.");
                    }

                    /*if (chr.getQuest(Quest.getInstance(29400)).getStatus().Equals(QuestStatus.Status.STARTED)) {
                     if (chr.getLevel() >= 120 && monster.getStats().getLevel() >= 120) {
                     //FIX MEDAL SHET
                     } else if (monster.getStats().getLevel() >= chr.getLevel()) {
                     }
                     }*/

                    int buff = monster.getBuffToGive();
                    if (buff > -1)
                    {
                        ItemInformationProvider mii = ItemInformationProvider.getInstance();
                        foreach (IMapObject mmo in getAllPlayers())
                        {
                            IPlayer character = (IPlayer)mmo;
                            if (character.isAlive())
                            {
                                var statEffect = mii.getItemEffect(buff)!;
                                character.sendPacket(PacketCreator.showOwnBuffEffect(buff, 1));
                                broadcastMessage(character, PacketCreator.showBuffEffect(character.getId(), buff, 1), false);
                                statEffect.applyTo(character);
                            }
                        }
                    }

                    if (MobId.isZakumArm(monster.getId()))
                    {
                        bool makeZakReal = true;
                        var objects = getMapObjects();
                        foreach (IMapObject mapObj in objects)
                        {
                            Monster? mons = getMonsterByOid(mapObj.getObjectId());
                            if (mons != null)
                            {
                                if (MobId.isZakumArm(mons.getId()))
                                {
                                    makeZakReal = false;
                                    break;
                                }
                            }
                        }
                        if (makeZakReal)
                        {
                            var map = chr.getMap();

                            foreach (IMapObject mapObj in objects)
                            {
                                Monster? mons = map.getMonsterByOid(mapObj.getObjectId());
                                if (mons != null)
                                {
                                    if (mons.getId() == MobId.ZAKUM_1)
                                    {
                                        makeMonsterReal(mons);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    var dropOwner = monster.killBy(chr);
                    if (withDrops && !monster.dropsDisabled())
                    {
                        if (dropOwner == null)
                        {
                            dropOwner = chr;
                        }
                        dropFromMonster(dropOwner, monster, false, dropDelay);
                    }

                    if (monster.hasBossHPBar())
                    {
                        foreach (IPlayer mc in this.getAllPlayers())
                        {
                            if (mc.getTargetHpBarHash() == monster.GetHashCode())
                            {
                                mc.resetPlayerAggro();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
                finally
                {     // thanks resinate for pointing out a memory leak possibly from an exception thrown
                    monster.dispatchMonsterKilled(true);
                    broadcastMessage(PacketCreator.killMonster(monster.getObjectId(), animation), monster.getPosition());
                }
            }
        }
    }

    public void killFriendlies(Monster mob)
    {
        this.killMonster(mob, getAllPlayers().ElementAtOrDefault(0), false);
    }

    public void killMonster(int mobId)
    {
        IPlayer? chr = getAllPlayers().ElementAtOrDefault(0);

        ProcessMonster(mob =>
        {
            if (mob.getId() == mobId)
            {
                this.killMonster(mob, chr, false);
            }
        });
    }

    public void killMonsterWithDrops(int mobId)
    {
        Dictionary<int, IPlayer> mapChars = this.getMapPlayers();

        if (mapChars.Count > 0)
        {
            IPlayer defaultChr = mapChars.FirstOrDefault().Value;

            ProcessMonster(mob =>
            {
                if (mob.getId() == mobId)
                {
                    var chr = mapChars.GetValueOrDefault(mob.getHighestDamagerId()) ?? defaultChr;
                    this.killMonster(mob, chr, true);
                }
            });
        }
    }

    public void killAllMonstersNotFriendly()
    {
        closeMapSpawnPoints();

        ProcessMonster(monster =>
        {
            if (monster.getStats().isFriendly())
            {
                return;
            }

            killMonster(monster, null, false);
        });
    }

    /// <summary>
    /// 直接杀死怪物，不会掉落物品，不会继续重生怪物
    /// </summary>
    public void killAllMonsters()
    {
        closeMapSpawnPoints();

        ProcessMonster(monster =>
        {
            killMonster(monster, null, false);
        });
    }

    public void destroyReactors(int first, int last)
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
            destroyReactor(mr.getObjectId());
        }
    }

    public void destroyReactor(int oid)
    {
        var reactor = getReactorByOid(oid);

        if (reactor != null)
        {
            if (reactor.destroy())
            {
                removeMapObject(reactor);
            }
        }
    }

    public void resetReactors()
    {
        List<Reactor> list = new();

        objectLock.EnterReadLock();
        try
        {
            foreach (IMapObject o in getMapObjects())
            {
                if (o.getType() == MapObjectType.REACTOR)
                {
                    Reactor r = ((Reactor)o);
                    list.Add(r);
                }
            }
        }
        finally
        {
            objectLock.ExitReadLock();
        }

        resetReactors(list);
    }

    public void resetReactors(List<Reactor> list)
    {
        foreach (Reactor r in list)
        {
            if (r.forceDelayedRespawn())
            {  // thanks Conrad for suggesting reactor with delay respawning immediately
                continue;
            }

            r.lockReactor();
            try
            {
                r.resetReactorActions(0);
                r.setAlive(true);
                broadcastMessage(PacketCreator.triggerReactor(r, 0));
            }
            finally
            {
                r.unlockReactor();
            }
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

        objectLock.EnterReadLock();
        try
        {
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
        }
        finally
        {
            objectLock.ExitReadLock();
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

    private Dictionary<int, IMapObject> getCopyMapObjects()
    {
        objectLock.EnterReadLock();
        try
        {
            return new(mapobjects);
        }
        finally
        {
            objectLock.ExitReadLock();
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
        objectLock.EnterReadLock();
        try
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
        }
        finally
        {
            objectLock.ExitReadLock();
        }
        return false;
    }

    public void destroyNPC(int npcid)
    {
        // assumption: there's at most one of the same NPC in a map.
        var npcs = GetMapObjects(x => x.getType() == MapObjectType.NPC);

        chrLock.EnterReadLock();
        objectLock.EnterWriteLock();
        try
        {
            foreach (IMapObject obj in npcs)
            {
                if (((NPC)obj).getId() == npcid)
                {
                    broadcastMessage(PacketCreator.removeNPCController(obj.getObjectId()));
                    broadcastMessage(PacketCreator.removeNPC(obj.getObjectId()));

                    removeMapObject(obj.getObjectId());
                }
            }
        }
        finally
        {
            objectLock.ExitWriteLock();
            chrLock.ExitReadLock();
        }
    }

    public IMapObject? getMapObject(int oid)
    {
        objectLock.EnterReadLock();
        try
        {
            return mapobjects.GetValueOrDefault(oid);
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    /**
     * returns a monster with the given oid, if no such monster exists returns
     * null
     *
     * @param oid
     * @return
     */
    public Monster? getMonsterByOid(int oid)
    {
        IMapObject? mmo = getMapObject(oid);
        return mmo as Monster;
    }

    public Reactor? getReactorByOid(int oid)
    {
        IMapObject? mmo = getMapObject(oid);
        return mmo as Reactor;
    }

    public Reactor? getReactorById(int Id)
    {
        objectLock.EnterReadLock();
        try
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
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    public List<Reactor> getReactorsByIdRange(int first, int last)
    {
        List<Reactor> list = new();

        objectLock.EnterReadLock();
        try
        {
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
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    public Reactor? getReactorByName(string name)
    {
        objectLock.EnterReadLock();
        try
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
        }
        finally
        {
            objectLock.ExitReadLock();
        }
        return null;
    }

    public void spawnMonsterOnGroundBelow(int id, int x, int y)
    {
        var mob = LifeFactory.Instance.getMonster(id);
        spawnMonsterOnGroundBelow(mob, new Point(x, y));
    }

    public void spawnMonsterOnGroundBelow(Monster? mob, Point pos)
    {
        if (mob == null)
            return;

        Point spos = new Point(pos.X, pos.Y - 1);
        var calcedPos = calcPointBelow(spos);
        if (calcedPos != null)
        {
            spos = calcedPos.Value;
            spos.Y--;
            mob.setPosition(spos);
            spawnMonster(mob);
        }

    }

    private void monsterItemDrop(Monster m, long delay)
    {
        m.dropFromFriendlyMonster(delay);
    }

    public void spawnFakeMonsterOnGroundBelow(Monster mob, Point pos)
    {
        Point spos = getGroundBelow(pos);
        mob.setPosition(spos);
        spawnFakeMonster(mob);
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

    public void spawnRevives(Monster monster)
    {
        monster.setMap(this);
        if (getEventInstance() != null)
        {
            getEventInstance()!.registerMonster(monster);
        }

        spawnAndAddRangedMapObject(monster, c => c.sendPacket(PacketCreator.spawnMonster(monster, false)));

        monster.aggroUpdateController();
        updateBossSpawn(monster);

        spawnedMonstersOnMap.incrementAndGet();
        addSelfDestructive(monster);
        applyRemoveAfter(monster);
    }

    private void applyRemoveAfter(Monster monster)
    {
        var selfDestruction = monster.getStats().selfDestruction();
        if (monster.getStats().removeAfter() > 0 || selfDestruction != null && selfDestruction.Hp < 0)
        {
            Action removeAfterAction;

            if (selfDestruction == null)
            {
                removeAfterAction = () =>
                {
                    killMonster(monster, null, false);
                };

                registerMapSchedule(removeAfterAction, monster.getStats().removeAfter() * 1000);
            }
            else
            {
                removeAfterAction = () =>
                {
                    killMonster(monster, null, false, selfDestruction.Action);
                };

                registerMapSchedule(removeAfterAction, selfDestruction.RemoveAfter * 1000);
            }

            monster.pushRemoveAfterAction(removeAfterAction);
        }
    }

    public void dismissRemoveAfter(Monster monster)
    {
        var removeAfterAction = monster.popRemoveAfterAction();
        if (removeAfterAction != null)
        {
            OverallService service = this.getChannelServer().OverallService;
            service.forceRunOverallAction(mapid, removeAfterAction);
        }
    }

    private List<SpawnPoint> getMonsterSpawn()
    {
        lock (monsterSpawn)
        {
            return new(monsterSpawn);
        }
    }

    private List<SpawnPoint> getAllMonsterSpawn()
    {
        lock (allMonsterSpawn)
        {
            return new(allMonsterSpawn);
        }
    }

    public void spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false)
    {
        foreach (SpawnPoint sp in getAllMonsterSpawn())
        {
            if (sp.getMonsterId() == id && sp.shouldForceSpawn())
            {
                sp.SpawnMonster(difficulty, isPq);
            }
        }
    }


    public void spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false)
    {
        foreach (SpawnPoint sp in getAllMonsterSpawn())
        {
            sp.SpawnMonster(difficulty, isPq);
        }
    }


    public void spawnMonster(Monster monster, int difficulty = 1, bool isPq = false)
    {
        if (SourceTemplate.FixedMobCapacity != -1 && SourceTemplate.FixedMobCapacity == spawnedMonstersOnMap.get())
        {
            return;//PyPQ
        }

        monster.changeDifficulty(difficulty, isPq);

        monster.setMap(this);
        EventInstanceManager?.registerMonster(monster);

        spawnAndAddRangedMapObject(monster, c => c.sendPacket(PacketCreator.spawnMonster(monster, true)), null);

        monster.aggroUpdateController();
        updateBossSpawn(monster);

        SetMonsterInfo(monster);

        if (monster.getDropPeriodTime() > 0)
        {
            //9300102 - Watchhog, 9300061 - Moon Bunny (HPQ), 9300093 - Tylus
            if (monster.getId() == MobId.WATCH_HOG)
            {
                monsterItemDrop(monster, monster.getDropPeriodTime());
            }
            else if (monster.getId() == MobId.MOON_BUNNY)
            {
                monsterItemDrop(monster, monster.getDropPeriodTime() / 3);
            }
            else if (monster.getId() == MobId.TYLUS)
            {
                monsterItemDrop(monster, monster.getDropPeriodTime());
            }
            else if (monster.getId() == MobId.GIANT_SNOWMAN_LV5_EASY || monster.getId() == MobId.GIANT_SNOWMAN_LV5_MEDIUM || monster.getId() == MobId.GIANT_SNOWMAN_LV5_HARD)
            {
                monsterItemDrop(monster, monster.getDropPeriodTime());
            }
            else
            {
                log.Error("UNCODED TIMED MOB DETECTED: {MonsterId}", monster.getId());
            }
        }

        spawnedMonstersOnMap.incrementAndGet();
        XiGuai?.ApplyMonster(monster);
        addSelfDestructive(monster);
        applyRemoveAfter(monster);  // thanks LightRyuzaki for pointing issues with spawned CWKPQ mobs not applying this
    }

    public void spawnDojoMonster(Monster monster)
    {
        Point[] pts = { new Point(140, 0), new Point(190, 7), new Point(187, 7) };
        spawnMonsterWithEffect(monster, 15, pts[Randomizer.nextInt(3)]);
    }

    public void spawnMonsterWithEffect(Monster monster, int effect, Point pos)
    {
        monster.setMap(this);
        Point spos = new Point(pos.X, pos.Y - 1);
        var d = calcPointBelow(spos);
        if (d == null)
        {
            return;
        }
        spos = d.Value;

        if (getEventInstance() != null)
        {
            getEventInstance()!.registerMonster(monster);
        }

        spos.Y--;
        monster.setPosition(spos);
        monster.setSpawnEffect(effect);

        spawnAndAddRangedMapObject(monster, c => c.sendPacket(PacketCreator.spawnMonster(monster, true, effect)));

        monster.aggroUpdateController();
        updateBossSpawn(monster);

        spawnedMonstersOnMap.incrementAndGet();
        XiGuai?.ApplyMonster(monster);
        addSelfDestructive(monster);
        applyRemoveAfter(monster);
    }

    public void spawnFakeMonster(Monster monster)
    {
        monster.setMap(this);
        monster.setFake(true);
        spawnAndAddRangedMapObject(monster, c => c.sendPacket(PacketCreator.spawnFakeMonster(monster, 0)));

        spawnedMonstersOnMap.incrementAndGet();
        XiGuai?.ApplyMonster(monster);
        addSelfDestructive(monster);
    }

    public void makeMonsterReal(Monster monster)
    {
        monster.setFake(false);
        broadcastMessage(PacketCreator.makeMonsterReal(monster));
        monster.aggroUpdateController();
        updateBossSpawn(monster);
    }

    public void spawnReactor(Reactor reactor)
    {
        reactor.setMap(this);
        spawnAndAddRangedMapObject(reactor, c => c.sendPacket(reactor.makeSpawnData()));
    }

    public void spawnDoor(DoorObject door)
    {
        spawnAndAddRangedMapObject(door, c =>
        {
            var chr = c.OnlinedCharacter;
            if (chr != null)
            {
                door.sendSpawnData(c, false);
                chr.addVisibleMapObject(door);
            }
        }, chr => chr.getMapId() == door.getFrom().getId());
    }

    public Portal? getDoorPortal(int doorid)
    {
        Portal? doorPortal = portals.GetValueOrDefault(0x80 + doorid);
        if (doorPortal == null)
        {
            log.Warning("[Door] {MapName} ({MapId}) does not contain door portalid {DoorId}", mapName, mapid, doorid);
            return portals.GetValueOrDefault(0x80);
        }

        return doorPortal;
    }

    public void spawnSummon(Summon summon)
    {
        spawnAndAddRangedMapObject(summon, c =>
        {
            if (summon != null)
            {
                c.sendPacket(PacketCreator.spawnSummon(summon, true));
            }
        }, null);
    }

    public void spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery)
    {
        addMapObject(mist);
        broadcastMessage(fake ? mist.makeFakeSpawnData(30) : mist.makeSpawnData());
        var tMan = ChannelServer.Container.TimerManager;
        ScheduledFuture? poisonSchedule = null;
        Action? poisonTask = null;
        if (poison)
        {
            var playerMist = (mist as PlayerMist)!;
            poisonTask = () =>
            {
                List<IMapObject> affectedMonsters = getMapObjectsInBox(mist.getBox(), Collections.singletonList(MapObjectType.MONSTER));
                foreach (IMapObject mo in affectedMonsters)
                {
                    if (playerMist.makeChanceResult())
                    {
                        MonsterStatusEffect poisonEffect = new MonsterStatusEffect(Collections.singletonMap(MonsterStatus.POISON, 1), playerMist.getSourceSkill());
                        ((Monster)mo).applyStatus(playerMist.getOwner(), poisonEffect, true, duration);
                    }
                }
            };

        }
        else if (recovery)
        {
            var playerMist = (mist as PlayerMist)!;
            poisonTask = () =>
            {
                List<IMapObject> players = getMapObjectsInBox(mist.getBox(), Collections.singletonList(MapObjectType.PLAYER));
                foreach (IMapObject mo in players)
                {
                    if (playerMist.makeChanceResult())
                    {
                        IPlayer chr = (IPlayer)mo;
                        if (playerMist.getOwner().getId() == chr.getId() || playerMist.getOwner().getParty() != null && playerMist.getOwner().getParty()!.containsMembers(chr.Id))
                        {
                            chr.ChangeMP(playerMist.getSourceSkill().getEffect(chr.getSkillLevel(playerMist.getSourceSkill().getId())).getX() * chr.MP / 100);
                        }
                    }
                }
            };
        }

        if (poisonTask != null)
            poisonSchedule = tMan.register(new MapTaskBase(this, "PoisonTask", poisonTask), 2000, 2500);

        Action mistSchedule = () =>
        {
            removeMapObject(mist);
            if (poisonSchedule != null)
            {
                poisonSchedule.cancel(false);
            }
            broadcastMessage(mist.makeDestroyData());
        };

        MobMistService service = this.getChannelServer().MobMistService;
        service.registerMobMistCancelAction(mapid, mistSchedule, duration);
    }

    public void spawnKite(Kite kite)
    {
        addMapObject(kite);
        broadcastMessage(kite.makeSpawnData());

        var expireKite = () =>
        {
            removeMapObject(kite);
            broadcastMessage(kite.makeDestroyData());
        };

        ChannelServer.Container.MapObjectManager.RegisterTimedMapObject(expireKite, YamlConfig.config.server.KITE_EXPIRE_TIME);
    }

    public void spawnItemDrop(IMapObject dropper, IPlayer owner, Item item, Point pos, bool ffaDrop, bool playerDrop)
    {
        spawnItemDrop(dropper, owner, item, pos, (DropType)(ffaDrop ? 2 : 0), playerDrop);
    }

    private void spawnItemDrop(IMapObject dropper, IPlayer owner, Item item, Point pos, DropType dropType, bool playerDrop)
    {
        if (FieldLimit.DROP_LIMIT.check(this.getFieldLimit()))
        {
            // thanks Conrad for noticing some maps shouldn't have loots available
            this.disappearingItemDrop(dropper, owner, item, pos);
            return;
        }

        Point droppos = calcDropPos(pos, dropper.getPosition());
        MapItem mdrop = new MapItem(item, droppos, dropper, owner, dropType, playerDrop);

        spawnAndAddRangedMapObject(mdrop, c =>
        {
            mdrop.lockItem();
            try
            {
                c.sendPacket(PacketCreator.dropItemFromMapObject(c.OnlinedCharacter, mdrop, dropper.getPosition(), droppos, 1, 0));
            }
            finally
            {
                mdrop.unlockItem();
            }
        }, null);

        /// 不明，对比<see cref="spawnDrop"/> 多出来的一段 是否可以移除？
        mdrop.lockItem();
        try
        {
            broadcastItemDropMessage(mdrop, dropper.getPosition(), droppos, 0);
        }
        finally
        {
            mdrop.unlockItem();
        }

        instantiateItemDrop(mdrop);
        activateItemReactors(mdrop, owner.getClient());
    }

    private void registerMapSchedule(AbstractRunnable r, long delay)
    {
        OverallService service = this.getChannelServer().OverallService;
        service.registerOverallAction(mapid, r, delay);
    }

    private void registerMapSchedule(Action r, long delay) => registerMapSchedule(TempRunnable.Parse(r), delay);

    private void activateItemReactors(MapItem drop, IChannelClient c)
    {
        var item = drop.getItem();

        foreach (IMapObject o in getReactors())
        {
            Reactor react = (Reactor)o;

            if (react.getReactorType() == 100)
            {
                var reactItem = react.getReactItem(react.getEventState())!;
                if (reactItem.ItemId == item.getItemId() && reactItem.Quantity == item.getQuantity())
                {

                    if (react.getArea().Contains(drop.getPosition()))
                    {
                        registerMapSchedule(new ActivateItemReactor(this, drop, react, c), 5000);
                        break;
                    }
                }
            }
        }
    }

    public void searchItemReactors(Reactor react)
    {
        if (react.getReactorType() == 100)
        {
            var reactProp = react.getReactItem(react.getEventState())!;
            int reactItem = reactProp.ItemId, reactQty = reactProp.Quantity;
            Rectangle reactArea = react.getArea();

            List<MapItem> list = getDroppedItems();

            foreach (MapItem drop in list)
            {
                drop.lockItem();
                try
                {
                    if (!drop.isPickedUp())
                    {
                        var item = drop.getItem();

                        if (item != null && reactItem == item.getItemId() && reactQty == item.getQuantity())
                        {
                            if (reactArea.Contains(drop.getPosition()))
                            {
                                var owner = drop.getOwnerClient();
                                if (owner != null)
                                {
                                    registerMapSchedule(new ActivateItemReactor(this, drop, react, owner), 5000);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    drop.unlockItem();
                }
            }
        }
    }

    public void startMapEffect(string msg, int itemId, long time = 30000)
    {
        if (mapEffect != null)
        {
            return;
        }
        mapEffect = new MapEffect(msg, itemId);
        broadcastMessage(mapEffect.makeStartData());

        Action r = () =>
        {
            broadcastMessage(mapEffect.makeDestroyData());
            mapEffect = null;
        };

        registerMapSchedule(r, time);
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

    public void addPlayerPuppet(IPlayer player)
    {
        ProcessMonster(mm =>
        {
            mm.aggroAddPuppet(player);
        });
    }

    public void removePlayerPuppet(IPlayer player)
    {
        ProcessMonster(mm =>
        {
            mm.aggroRemovePuppet(player);
        });
    }

    public void broadcastMessage(Packet packet)
    {
        Broadcast(null, double.PositiveInfinity, null, e => e.sendPacket(packet));
    }

    public void broadcastGMMessage(Packet packet)
    {
        broadcastGMMessage(null, packet, double.PositiveInfinity, null);
    }

    /**
     * Ranged and repeat according to parameters.
     *
     * @param source
     * @param packet
     * @param repeatToSource
     * @param ranged
     */
    public void broadcastMessage(IPlayer source, Packet packet, bool repeatToSource, bool ranged = false)
    {
        broadcastMessage(repeatToSource ? null : source, packet, ranged ? getRangedDistance() : double.PositiveInfinity, source.getPosition());
    }

    /**
     * Always ranged from Point.
     *
     * @param packet
     * @param rangedFrom
     */
    public void broadcastMessage(Packet packet, Point rangedFrom)
    {
        broadcastMessage(null, packet, getRangedDistance(), rangedFrom);
    }

    /**
     * Always ranged from point. Does not repeat to source.
     *
     * @param source
     * @param packet
     * @param rangedFrom
     */
    public void broadcastMessage(IPlayer? source, Packet packet, Point rangedFrom)
    {
        broadcastMessage(source, packet, getRangedDistance(), rangedFrom);
    }

    private void broadcastMessage(IPlayer? source, Packet packet, double rangeSq, Point? rangedFrom)
    {
        Broadcast(source, rangeSq, rangedFrom, chr => chr.sendPacket(packet));
    }

    private void updateBossSpawn(Monster monster)
    {
        if (monster.hasBossHPBar())
        {
            broadcastBossHpMessage(monster, monster.GetHashCode(), monster.makeBossHPBarPacket(), monster.getPosition());
        }
        if (monster.isBoss())
        {
            if (unclaimOwnership() != null)
            {
                BroadcastAll(e => e.Pink(nameof(ClientMessage.Map_Ownership_Boss), e.Client.CurrentCulture.GetMobName(monster.getId())));
            }
        }
    }

    public void broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null)
    {
        broadcastBossHpMessage(mm, bossHash, null, packet, getRangedDistance(), rangedFrom);
    }

    private void broadcastBossHpMessage(Monster mm, int bossHash, IPlayer? source, Packet packet, double rangeSq, Point? rangedFrom)
    {
        Broadcast(source, rangeSq, rangedFrom, chr =>
        {
            chr.getClient().announceBossHpBar(mm, bossHash, packet);
        });
    }

    private void broadcastItemDropMessage(MapItem mdrop, Point dropperPos, Point dropPos, byte mod, double rangeSq = double.PositiveInfinity, Point? rangedFrom = null, short dropDelay = 0)
    {
        Broadcast(null, rangeSq, rangedFrom, chr =>
        {
            chr.sendPacket(PacketCreator.dropItemFromMapObject(chr, mdrop, dropperPos, dropPos, mod, dropDelay));
        });
    }

    public void broadcastSpawnPlayerMapObjectMessage(IPlayer source, IPlayer player, bool enteringField)
    {
        broadcastSpawnPlayerMapObjectMessage(source, player, enteringField, false);
    }

    public void broadcastGMSpawnPlayerMapObjectMessage(IPlayer source, IPlayer player, bool enteringField)
    {
        broadcastSpawnPlayerMapObjectMessage(source, player, enteringField, true);
    }

    private void broadcastSpawnPlayerMapObjectMessage(IPlayer source, IPlayer player, bool enteringField, bool gmBroadcast)
    {
        chrLock.EnterReadLock();
        try
        {
            if (gmBroadcast)
            {
                foreach (IPlayer chr in getAllPlayers())
                {
                    if (chr.isGM())
                    {
                        if (chr != source)
                        {
                            chr.sendPacket(PacketCreator.spawnPlayerMapObject(chr.Client, player, enteringField));
                        }
                    }
                }
            }
            else
            {
                foreach (IPlayer chr in getAllPlayers())
                {
                    if (chr != source)
                    {
                        chr.sendPacket(PacketCreator.spawnPlayerMapObject(chr.Client, player, enteringField));
                    }
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public void broadcastUpdateCharLookMessage(IPlayer source, IPlayer player)
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in getAllPlayers())
            {
                if (chr != source)
                {
                    chr.sendPacket(PacketCreator.updateCharLook(chr.Client, player));
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public void dropMessage(int type, string message)
    {
        broadcastMessage(PacketCreator.serverNotice(type, message));
    }

    private static bool isNonRangedType(MapObjectType type)
    {
        switch (type)
        {
            case MapObjectType.NPC:
            case MapObjectType.PLAYER:
            case MapObjectType.HIRED_MERCHANT:
            case MapObjectType.PLAYER_NPC:
            case MapObjectType.DRAGON:
            case MapObjectType.MIST:
            case MapObjectType.KITE:
                return true;
            default:
                return false;
        }
    }

    private void sendObjectPlacement(IChannelClient c)
    {
        var allMapObjects = getMapObjects();

        var chr = c.OnlinedCharacter;
        var chrPosition = chr.getPosition();
        var rangeDistance = getRangedDistance();

        List<int> removedSummonObjects = new List<int>();

        foreach (var o in allMapObjects)
        {
            if (o.getType() == MapObjectType.SUMMON && o is Summon summon)
            {
                if (summon.getOwner() == chr && (chr.isSummonsEmpty() || !chr.containsSummon(summon)))
                {
                    removedSummonObjects.Add(o.getObjectId());
                    continue;
                }
            }

            if (isNonRangedType(o.getType()))
            {
                o.sendSpawnData(c);
            }

            // rangedMapobjectTypes 和 NonRangedType都包含了NPC
            if (IsObjectInRange(o, chrPosition, rangeDistance, MapGlobalData.rangedMapobjectTypes))
            {
                if (o.getType() == MapObjectType.REACTOR && o is Reactor reactor && !reactor.isAlive())
                    continue;

                o.sendSpawnData(chr.Client);
                chr.addVisibleMapObject(o);

                if (o.getType() == MapObjectType.MONSTER && o is Monster monster)
                    monster.aggroUpdateController();
            }
        }

        if (removedSummonObjects.Count > 0)
        {
            RemoveMapObjects(removedSummonObjects);
        }
    }


    public List<IMapObject> getMapObjects()
    {
        objectLock.EnterReadLock();
        try
        {
            return new(mapobjects.Values);
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="codition"></param>
    /// <param name="action"></param>
    public void ProcessMapObject(Func<IMapObject, bool> codition, Action<IMapObject> action)
    {
        var list = getMapObjects();
        foreach (var item in list)
        {
            if (codition(item))
                action(item);
        }
    }

    public void ProcessMonster(Action<Monster> action)
    {
        ProcessMapObject(x => x.getType() == MapObjectType.MONSTER, o =>
        {
            action((Monster)o);
        });
    }


    public List<IMapObject> GetMapObjects(Func<IMapObject, bool> func)
    {
        objectLock.EnterReadLock();
        try
        {
            return mapobjects.Values.AsValueEnumerable().Where(func).ToList();
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }
    List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type) where TObject : IMapObject
    {
        objectLock.EnterReadLock();
        try
        {
            return mapobjects.Values.AsValueEnumerable()
                .Where(x => x.getType() == type)
                .Cast<TObject>()
                .ToList();
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }
    List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type, Func<TObject, bool> func) where TObject : IMapObject
    {
        objectLock.EnterReadLock();
        try
        {
            return mapobjects.Values.AsValueEnumerable()
                .Where(x => x.getType() == type)
                .Cast<TObject>()
                .Where(func)
                .ToList();
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    private static bool IsObjectInRange(IMapObject obj, Point from, double rangeSq, List<MapObjectType> types)
    {
        return types.Contains(obj.getType()) && from.distanceSq(obj.getPosition()) <= rangeSq;
    }

    public List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, List<MapObjectType> types)
    {
        return GetMapObjects(x => IsObjectInRange(x, from, rangeSq, types));
    }

    public List<IMapObject> getMapObjectsInBox(Rectangle box, List<MapObjectType> types)
    {
        List<IMapObject> ret = new();
        objectLock.EnterReadLock();
        try
        {
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
        finally
        {
            objectLock.ExitReadLock();
        }
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
    public void addMonsterSpawn(Monster monster, int mobTime, int team)
    {
        Point newpos = calcPointBelow(monster.getPosition())!.Value;
        newpos.Y -= 1;
        SpawnPoint sp = new SpawnPoint(this, monster, newpos, !monster.isMobile(), mobTime, SourceTemplate.CreateMobInterval, team);
        monsterSpawn.Add(sp);
        if (sp.shouldSpawn() || mobTime == -1)
        {
            // -1 does not respawn and should not either but force ONE spawn
            sp.SpawnMonster();
        }
    }

    /// <summary>
    /// 有mobTime > 0的野怪
    /// </summary>
    bool _hasLongLifeMob;
    public void addAllMonsterSpawn(Monster monster, int mobTime, int team)
    {
        Point newpos = calcPointBelow(monster.getPosition())!.Value;
        newpos.Y -= 1;
        SpawnPoint sp = new SpawnPoint(this, monster, newpos, !monster.isMobile(), mobTime, SourceTemplate.CreateMobInterval, team);
        allMonsterSpawn.Add(sp);

        if (!_hasLongLifeMob && mobTime > 0)
            _hasLongLifeMob = true;
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
            lock (monsterSpawn)
            {
                foreach (SpawnPoint sp in toRemove)
                {
                    monsterSpawn.Remove(sp);
                }
            }
        }
    }

    public void removeAllMonsterSpawn(int mobId, int x, int y)
    {
        // assumption: spawn points identifies by tuple (lifeid, x, y)

        Point checkpos = calcPointBelow(new Point(x, y))!.Value;
        checkpos.Y -= 1;

        List<SpawnPoint> toRemove = new();
        foreach (SpawnPoint sp in getAllMonsterSpawn())
        {
            Point pos = sp.getPosition();
            if (sp.getMonsterId() == mobId && checkpos.Equals(pos))
            {
                toRemove.Add(sp);
            }
        }

        if (toRemove.Count > 0)
        {
            lock (allMonsterSpawn)
            {
                foreach (SpawnPoint sp in toRemove)
                {
                    allMonsterSpawn.Remove(sp);
                }
            }
        }
    }

    public void reportMonsterSpawnPoints(IPlayer chr)
    {
        chr.dropMessage(6, "Mob spawnpoints on map " + getId() + ", with available Mob SPs " + monsterSpawn.Count() + ", used " + spawnedMonstersOnMap.get() + ":");
        foreach (SpawnPoint sp in getAllMonsterSpawn())
        {
            chr.dropMessage(6, "  id: " + sp.getMonsterId() + " canSpawn: " + !sp.getDenySpawn() + " numSpawned: " + sp.getSpawned() + " x: " + sp.getPosition().X + " y: " + sp.getPosition().Y + " time: " + sp.getMobTime() + " team: " + sp.getTeam());
        }
    }



    private static void updateMapObjectVisibility(IPlayer chr, IMapObject mo)
    {
        if (!chr.isMapObjectVisible(mo))
        { // object entered view range
            if (mo.getType() == MapObjectType.SUMMON || mo.getPosition().distanceSq(chr.getPosition()) <= getRangedDistance())
            {
                chr.addVisibleMapObject(mo);
                mo.sendSpawnData(chr.Client);
            }
        }
        else if (mo.getType() != MapObjectType.SUMMON && mo.getPosition().distanceSq(chr.getPosition()) > getRangedDistance())
        {
            chr.removeVisibleMapObject(mo);
            mo.sendDestroyData(chr.Client);
        }
    }

    public void moveMonster(Monster monster, Point reportedPos)
    {
        monster.setPosition(reportedPos);
        foreach (IPlayer chr in getAllPlayers())
        {
            updateMapObjectVisibility(chr, monster);
        }
    }

    public void movePlayer(IPlayer player, Point newPosition)
    {
        player.setPosition(newPosition);

        try
        {
            IMapObject[] visibleObjects = player.getVisibleMapObjects();

            var mapObjects = getCopyMapObjects();
            foreach (IMapObject mo in visibleObjects)
            {
                if (mo != null)
                {
                    if (mapObjects.GetValueOrDefault(mo.getObjectId()) == mo)
                    {
                        updateMapObjectVisibility(player, mo);
                    }
                    else
                    {
                        player.removeVisibleMapObject(mo);
                    }
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        foreach (var mo in getMapObjectsInRange(player.getPosition(), getRangedDistance(), MapGlobalData.rangedMapobjectTypes))
        {
            if (!player.isMapObjectVisible(mo))
            {
                mo.sendSpawnData(player.Client);
                player.addVisibleMapObject(mo);
            }
        }
    }

    public void toggleEnvironment(string ms)
    {
        var env = getEnvironment();

        if (env.TryGetValue(ms, out var value))
        {
            moveEnvironment(ms, value == 1 ? 2 : 1);
        }
        else
        {
            moveEnvironment(ms, 1);
        }
    }

    public void moveEnvironment(string ms, int type)
    {
        broadcastMessage(PacketCreator.environmentMove(ms, type));

        objectLock.EnterWriteLock();
        try
        {
            environment.AddOrUpdate(ms, type);
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    public IDictionary<string, int> getEnvironment()
    {
        objectLock.EnterReadLock();
        try
        {
            return new Dictionary<string, int>(environment);
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    public string getMapName()
    {
        return mapName;
    }

    public void setMapName(string mapName)
    {
        this.mapName = mapName;
    }

    public string getStreetName()
    {
        return streetName;
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

    public void setStreetName(string streetName)
    {
        this.streetName = streetName;
    }

    public bool getEverlast() => SourceTemplate.Everlast;

    public int getSpawnedMonstersOnMap()
    {
        return spawnedMonstersOnMap.get();
    }

    // not really costly to keep generating imo
    public void sendNightEffect(IPlayer chr)
    {
        foreach (var types in SourceTemplate.Backs)
        {
            if (types.Type >= 3)
            { // 3 is a special number
                chr.sendPacket(PacketCreator.changeBackgroundEffect(true, types.Index, 0));
            }
        }
    }

    public void broadcastNightEffect()
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in this.getAllPlayers())
            {
                sendNightEffect(chr);
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public IPlayer? getCharacterByName(string name)
    {
        chrLock.EnterReadLock();
        try
        {
            return getAllPlayers().FirstOrDefault(x => x.getName().Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public bool makeDisappearItemFromMap(IMapObject? mapobj)
    {
        if (mapobj is MapItem o)
        {
            return makeDisappearItemFromMap(o);
        }
        else
        {
            return mapobj == null;  // no drop to make disappear...
        }
    }

    public bool makeDisappearItemFromMap(MapItem mapitem)
    {
        if (mapitem != null && mapitem == getMapObject(mapitem.getObjectId()))
        {
            mapitem.lockItem();
            try
            {
                if (mapitem.isPickedUp())
                {
                    return true;
                }

                pickItemDrop(PacketCreator.removeItemFromMap(mapitem.getObjectId(), MapItemRemoveAnimation.Expired, 0), mapitem);
                return true;
            }
            finally
            {
                mapitem.unlockItem();
            }
        }

        return false;
    }

    //public class MobLootEntry : AbstractRunnable
    //{

    //    private byte droptype;
    //    private int mobpos;
    //    private int chRate;
    //    private Point pos;
    //    short delay;
    //    private List<MonsterDropEntry> dropEntry;
    //    private List<MonsterDropEntry> visibleQuestEntry;
    //    private List<MonsterDropEntry> otherQuestEntry;
    //    private List<MonsterGlobalDropEntry> globalEntry;
    //    private IPlayer chr;
    //    private Monster mob;
    //    IMap _map;
    //    public MobLootEntry(IMap map, byte droptype, int mobpos, int chRate, Point pos, short delay, List<MonsterDropEntry> dropEntry, List<MonsterDropEntry> visibleQuestEntry, List<MonsterDropEntry> otherQuestEntry, List<MonsterGlobalDropEntry> globalEntry, IPlayer chr, Monster mob)
    //    {
    //        _map = map;
    //        this.droptype = droptype;
    //        this.mobpos = mobpos;
    //        this.chRate = chRate;
    //        this.pos = pos;
    //        this.delay = delay;
    //        this.dropEntry = dropEntry;
    //        this.visibleQuestEntry = visibleQuestEntry;
    //        this.otherQuestEntry = otherQuestEntry;
    //        this.globalEntry = globalEntry;
    //        this.chr = chr;
    //        this.mob = mob;
    //    }

    //    public override void HandleRun()
    //    {
    //        byte d = 1;

    //        // Normal Drops
    //        d = _map.dropItemsFromMonsterOnMap(dropEntry, pos, d, chRate, droptype, mobpos, chr, mob, delay);

    //        // Global Drops
    //        d = _map.dropGlobalItemsFromMonsterOnMap(globalEntry, pos, d, droptype, mobpos, chr, mob, delay);

    //        // Quest Drops
    //        d = _map.dropItemsFromMonsterOnMap(visibleQuestEntry, pos, d, chRate, droptype, mobpos, chr, mob, delay);
    //        _map.dropItemsFromMonsterOnMap(otherQuestEntry, pos, d, chRate, droptype, mobpos, chr, mob, delay);
    //    }
    //}

    /// <summary>
    /// 丢掉道具触发hitReactor
    /// </summary>
    private class ActivateItemReactor : AbstractRunnable
    {

        private MapItem mapitem;
        private Reactor reactor;
        private IChannelClient c;
        private MapleMap _map;
        public ActivateItemReactor(MapleMap map, MapItem mapitem, Reactor reactor, IChannelClient c)
        {
            _map = map;
            this.mapitem = mapitem;
            this.reactor = reactor;
            this.c = c;
        }

        public override void HandleRun()
        {
            reactor.hitLockReactor();
            try
            {
                if (reactor.getReactorType() == 100)
                {
                    if (reactor.getShouldCollect() == true && mapitem != null && mapitem == _map.getMapObject(mapitem.getObjectId()))
                    {
                        mapitem.lockItem();
                        try
                        {
                            if (mapitem.isPickedUp())
                            {
                                return;
                            }
                            mapitem.setPickedUp(true);
                            _map.unregisterItemDrop(mapitem);

                            reactor.setShouldCollect(false);
                            _map.broadcastMessage(PacketCreator.removeItemFromMap(mapitem.getObjectId(), MapItemRemoveAnimation.Expired, 0), mapitem.getPosition());

                            reactor.hitReactor(c);

                            if (reactor.getDelay() > 0)
                            {
                                var reactorMap = reactor.getMap();

                                OverallService service = reactorMap.getChannelServer().OverallService;
                                service.registerOverallAction(reactorMap.getId(), () =>
                                {
                                    reactor.lockReactor();
                                    try
                                    {
                                        reactor.resetReactorActions(0);
                                        reactor.setAlive(true);
                                        _map.broadcastMessage(PacketCreator.triggerReactor(reactor, 0));
                                    }
                                    finally
                                    {
                                        reactor.unlockReactor();
                                    }
                                }, reactor.getDelay());
                            }
                        }
                        finally
                        {
                            mapitem.unlockItem();
                        }
                    }
                }
            }
            finally
            {
                reactor.hitUnlockReactor();
            }
        }
    }

    private void instanceMapFirstSpawn()
    {
        foreach (SpawnPoint spawnPoint in getAllMonsterSpawn())
        {
            if (spawnPoint.shouldSpawn() || spawnPoint.getMobTime() == -1)
            {
                //just those allowed to be spawned only once
                spawnPoint.SpawnMonster();
            }
        }
    }

    public void instanceMapRespawn()
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
                    spawnPoint.SpawnMonster();
                    spawned++;
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }
    }

    public void instanceMapForceRespawn()
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
                    spawnPoint.SpawnMonster();
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

    public void respawn()
    {
        if (!_allowSummons)
        {
            return;
        }

        int numPlayers;
        chrLock.EnterReadLock();
        try
        {
            numPlayers = characters.Count;

            if (numPlayers == 0)
            {
                return;
            }
        }
        finally
        {
            chrLock.ExitReadLock();
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
                    spawnPoint.SpawnMonster();
                    spawned++;

                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }
    }

    public void mobMpRecovery()
    {
        ProcessMonster(mob =>
        {
            if (mob.isAlive())
            {
                mob.heal(0, mob.getLevel());
            }
        });
    }



    //private interface DelayedPacketCreation
    //{
    //    public Action<IClient>? sendPackets { get; set; }
    //}

    //public class ActualDelayedPacketCreation : DelayedPacketCreation
    //{
    //    public Action<IClient>? sendPackets { get; set; }
    //}

    //private class SpawnCondition
    //{

    //    public Func<IPlayer, bool>? canSpawn;
    //}

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

    public bool getDocked()
    {
        return this.docked;
    }

    public int getSeats() => SourceTemplate.SeatCount;

    public void broadcastGMMessage(IPlayer source, Packet packet, bool repeatToSource)
    {
        broadcastGMMessage(repeatToSource ? null : source, packet, double.PositiveInfinity, source.getPosition());
    }

    private void broadcastGMMessage(IPlayer? source, Packet packet, double rangeSq, Point? rangedFrom)
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in getAllPlayers())
            {
                if (chr != source && chr.isGM())
                {
                    if (rangeSq < double.PositiveInfinity)
                    {
                        if (rangedFrom != null && rangedFrom.Value.distanceSq(chr.getPosition()) <= rangeSq)
                        {
                            chr.sendPacket(packet);
                        }
                    }
                    else
                    {
                        chr.sendPacket(packet);
                    }
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public void broadcastNONGMMessage(IPlayer source, Packet packet, bool repeatToSource)
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in getAllPlayers())
            {
                if (chr != source && !chr.isGM())
                {
                    chr.sendPacket(packet);
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

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

    public void clearDrops()
    {
        ProcessMapObject(x => x.getType() == MapObjectType.ITEM, i =>
        {
            this.broadcastMessage(PacketCreator.removeItemFromMap(i.getObjectId(), MapItemRemoveAnimation.Expired, 0));
            unregisterItemDrop((MapItem)i);
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

    public void warpEveryone(int to)
    {
        List<IPlayer> players = new(getAllPlayers());

        foreach (IPlayer chr in players)
        {
            chr.changeMap(to);
        }
    }

    public void warpEveryone(int to, int pto)
    {
        List<IPlayer> players = new(getAllPlayers());

        foreach (IPlayer chr in players)
        {
            chr.changeMap(to, pto);
        }
    }

    private bool specialEquip()
    {
        //Maybe I shouldn't use fieldType :\
        return SourceTemplate.FieldType == 4 || SourceTemplate.FieldType == 19;
    }

    public void warpOutByTeam(int team, int mapid)
    {
        List<IPlayer> chars = new(getAllPlayers());
        foreach (IPlayer chr in chars)
        {
            if (chr != null)
            {
                if (chr.getTeam() == team)
                {
                    chr.changeMap(mapid);
                }
            }
        }
    }

    public virtual void startEvent(IPlayer chr)
    {
        if (this.mapid == MapId.EVENT_PHYSICAL_FITNESS)
        {
            chr.Fitness = new Fitness(chr);
            chr.Fitness.startFitness();
        }
        else if (this.mapid == MapId.EVENT_OLA_OLA_1 || this.mapid == MapId.EVENT_OLA_OLA_2 ||
                this.mapid == MapId.EVENT_OLA_OLA_3 || this.mapid == MapId.EVENT_OLA_OLA_4)
        {
            chr.Ola = new Ola(chr);
            chr.Ola.startOla();
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

    public void clearMapObjects()
    {
        clearDrops();
        killAllMonsters();
        resetReactors();
    }

    public void resetFully()
    {
        resetMapObjects();
    }


    public void resetPQ(int difficulty = 1)
    {
        resetMapObjects(difficulty, true);
    }

    public void resetMapObjects()
    {
        resetMapObjects(1, false);
    }

    public void resetMapObjects(int difficulty, bool isPq)
    {
        clearMapObjects();

        restoreMapSpawnPoints();
        instanceMapFirstSpawn();
    }

    public void broadcastShip(bool state)
    {
        broadcastMessage(PacketCreator.boatPacket(state));
        this.setDocked(state);
    }

    public void broadcastEnemyShip(bool state)
    {
        broadcastMessage(PacketCreator.crogBoatPacket(state));
        pirateDocked = state;
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

    public void spawnHorntailOnGroundBelow(Point targetPoint)
    {   // ayy lmao
        var htIntro = LifeFactory.Instance.getMonster(MobId.SUMMON_HORNTAIL)!;
        spawnMonsterOnGroundBelow(htIntro, targetPoint);    // htintro spawn animation converting into horntail detected thanks to Arnah

        var ht = LifeFactory.Instance.getMonster(MobId.HORNTAIL)!;
        ht.setParentMobOid(htIntro.getObjectId());
        ht.addListener(new ActualMonsterListener()
        {
            monsterDamaged = (IPlayer from, int trueDmg) =>
            {
                ht.addHp(trueDmg);
            },

            monsterHealed = (int trueHeal) =>
            {
                ht.addHp(-trueHeal);
            }
        });
        spawnMonsterOnGroundBelow(ht, targetPoint);

        for (int mobId = MobId.HORNTAIL_HEAD_A; mobId <= MobId.HORNTAIL_TAIL; mobId++)
        {
            Monster m = LifeFactory.Instance.getMonster(mobId)!;
            m.setParentMobOid(htIntro.getObjectId());

            m.addListener(new ActualMonsterListener()
            {
                monsterDamaged = (IPlayer from, int trueDmg) =>
                {
                    // thanks Halcyon for noticing HT not dropping loots due to propagated damage not registering attacker
                    ht.applyFakeDamage(from, trueDmg, true);
                },

                monsterHealed = (int trueHeal) =>
                {
                    ht.addHp(trueHeal);
                }
            });

            spawnMonsterOnGroundBelow(m, targetPoint);
        }
    }

    public bool claimOwnership(IPlayer chr)
    {
        if (mapOwner == null)
        {
            this.mapOwner = chr;
            chr.setOwnedMap(this);

            mapOwnerLastActivityTime = ChannelServer.Container.getCurrentTime();

            getChannelServer().Container.MapOwnershipManager.RegisterOwnedMap(this);
            return true;
        }
        else
        {
            return chr == mapOwner;
        }
    }

    public IPlayer? unclaimOwnership()
    {
        var lastOwner = this.mapOwner;
        return unclaimOwnership(lastOwner) ? lastOwner : null;
    }

    public bool unclaimOwnership(IPlayer? chr)
    {
        if (chr != null && mapOwner == chr)
        {
            this.mapOwner = null;
            chr.setOwnedMap(null);

            mapOwnerLastActivityTime = long.MaxValue;

            getChannelServer().Container.MapOwnershipManager.UnregisterOwnedMap(this);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void refreshOwnership()
    {
        mapOwnerLastActivityTime = ChannelServer.Container.getCurrentTime();
    }

    public bool isOwnershipRestricted(IPlayer chr)
    {
        IPlayer? owner = mapOwner;

        if (owner != null)
        {
            if (owner != chr && !owner.isPartyMember(chr))
            {    // thanks Vcoc & BHB for suggesting the map ownership feature
                chr.showMapOwnershipInfo(owner);
                return true;
            }
            else
            {
                this.refreshOwnership();
            }
        }

        return false;
    }

    public void checkMapOwnerActivity()
    {
        long timeNow = ChannelServer.Container.getCurrentTime();
        if (timeNow - mapOwnerLastActivityTime > 60000)
        {
            if (unclaimOwnership() != null)
            {
                Pink(nameof(ClientMessage.Map_Ownership_Free));
            }
        }
    }

    protected virtual void SetMonsterInfo(Monster monster)
    {

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


    public void registerCharacterStatUpdate(Action r)
    {
        statUpdateRunnables.Writer.TryWrite(r);
    }
    public int _activeCounter;
    public bool IsActive()
    {
        if (_hasLongLifeMob)
            return true;

        var exceptTypes = new List<MapObjectType> { MapObjectType.MONSTER, MapObjectType.NPC, MapObjectType.PLAYER_NPC, MapObjectType.REACTOR };
        var isActive = GetMapObjects(x => !exceptTypes.Contains(x.getType())).Count > 0;
        if (isActive)
            _activeCounter = 5;
        else
            _activeCounter--;

        return _activeCounter >= 0;
    }

    bool disposed = false;
    public virtual void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        ProcessMonster(mm => mm.dispose());
        clearMapObjects();

        ChannelServer.OnWorldMobRateChanged -= UpdateMapActualMobRate;
        statUpdateRunnables.Writer.Complete();

        EventInstanceManager = null;
        portals.Clear();
        mapEffect = null;

        monsterSpawn.Clear();
        allMonsterSpawn.Clear();

        chrLock.EnterWriteLock();
        try
        {
            aggroMonitor.dispose();

            if (itemMonitor != null)
            {
                itemMonitor.cancel(false);
                itemMonitor = null;
            }

            if (expireItemsTask != null)
            {
                expireItemsTask.cancel(false);
                expireItemsTask = null;
            }

            if (characterStatUpdateTask != null)
            {
                characterStatUpdateTask.cancel(false);
                characterStatUpdateTask = null;
            }
        }
        finally
        {
            chrLock.ExitWriteLock();
            log.Debug("{MapInstance}已释放", InstanceName);
        }
    }

    #region Objects: Player
    private Dictionary<int, IPlayer> characters = new();
    public int countPlayers()
    {
        return getAllPlayers().Count;
    }

    public List<IPlayer> getAllPlayers()
    {
        chrLock.EnterReadLock();
        try
        {
            return characters.Values.ToList();
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public int getNumPlayersInArea(int index)
    {
        return getNumPlayersInRect(getArea(index));
    }

    public int getNumPlayersInRect(Rectangle rect)
    {
        chrLock.EnterReadLock();
        try
        {
            return getAllPlayers().Count(x => rect.Contains(x.getPosition()));
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    protected virtual void OnPlayerEnter(IPlayer chr)
    {
        if (pirateDocked)
        {
            chr.sendPacket(PacketCreator.musicChange("Bgm04/ArabPirate"));
            chr.sendPacket(PacketCreator.crogBoatPacket(true));
        }

        ChannelServer.Metrics.MapCountInc(Id);
    }

    public void addPlayer(IPlayer chr)
    {
        int chrSize;
        chrLock.EnterWriteLock();
        try
        {
            if (!characters.TryAdd(chr.Id, chr))
            {
                log.Error("MapleMap.AddPlayer {CharacterId}", chr.Id);
                return;
            }

            chrSize = characters.Count;

            itemMonitorTimeout = 1;
        }
        finally
        {
            chrLock.ExitWriteLock();
        }

        chr.setMap(this);
        chr.updateActiveEffects();

        if (this.getHPDec() > 0)
        {
            ChannelServer.Container.CharacterHpDecreaseManager.addPlayerHpDecrease(chr);
        }
        else
        {
            ChannelServer.Container.CharacterHpDecreaseManager.removePlayerHpDecrease(chr);
        }

        MapScriptManager msm = ChannelServer.MapScriptManager;
        if (chrSize == 1)
        {
            if (!hasItemMonitor())
            {
                startItemMonitor();
                aggroMonitor.startAggroCoordinator();
            }

            if (onFirstUserEnter.Length != 0)
            {
                msm.runMapScript(chr.getClient(), this, "onFirstUserEnter/" + onFirstUserEnter, true);
            }
        }
        if (onUserEnter.Length != 0)
        {
            if (onUserEnter.Equals("cygnusTest") && !MapId.isCygnusIntro(mapid))
            {
                chr.saveLocation("INTRO");
            }

            msm.runMapScript(chr.getClient(), this, "onUserEnter/" + onUserEnter, false);
        }
        if (FieldLimit.CANNOTUSEMOUNTS.check(SourceTemplate.FieldLimit) && chr.getBuffedValue(BuffStat.MONSTER_RIDING) != null)
        {
            chr.cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);
            chr.cancelBuffStats(BuffStat.MONSTER_RIDING);
        }

        if (mapid == MapId.FROM_ELLINIA_TO_EREVE)
        { 
            // To Ereve (SkyFerry)
            int travelTime = ChannelServer.getTransportationTime(TimeSpan.FromMinutes(2).TotalMilliseconds);
            chr.sendPacket(PacketCreator.getClock(travelTime / 1000));
            ChannelServer.Container.TimerManager.schedule(() =>
            {
                if (chr.getMapId() == MapId.FROM_ELLINIA_TO_EREVE)
                {
                    chr.changeMap(MapId.SKY_FERRY, 0);
                }
            }, travelTime);
        }
        else if (mapid == MapId.FROM_EREVE_TO_ELLINIA)
        { 
            // To Victoria Island (SkyFerry)
            int travelTime = ChannelServer.getTransportationTime(TimeSpan.FromMinutes(2).TotalMilliseconds);
            chr.sendPacket(PacketCreator.getClock(travelTime / 1000));
            ChannelServer.Container.TimerManager.schedule(() =>
            {
                if (chr.getMapId() == MapId.FROM_EREVE_TO_ELLINIA)
                {
                    chr.changeMap(MapId.ELLINIA_SKY_FERRY, 0);
                }
            }, travelTime);
        }
        else if (mapid == MapId.FROM_EREVE_TO_ORBIS)
        {
            // To Orbis (SkyFerry)
            int travelTime = ChannelServer.getTransportationTime(TimeSpan.FromMinutes(8).TotalMilliseconds);
            chr.sendPacket(PacketCreator.getClock(travelTime / 1000));
            ChannelServer.Container.TimerManager.schedule(() =>
            {
                if (chr.getMapId() == MapId.FROM_EREVE_TO_ORBIS)
                {
                    chr.changeMap(MapId.ORBIS_STATION, 0);
                }
            }, travelTime);
        }
        else if (mapid == MapId.FROM_ORBIS_TO_EREVE)
        { 
            // To Ereve From Orbis (SkyFerry)
            int travelTime = ChannelServer.getTransportationTime(TimeSpan.FromMinutes(8).TotalMilliseconds);
            chr.sendPacket(PacketCreator.getClock(travelTime / 1000));
            ChannelServer.Container.TimerManager.schedule(() =>
            {
                if (chr.getMapId() == MapId.FROM_ORBIS_TO_EREVE)
                {
                    chr.changeMap(MapId.SKY_FERRY, 0);
                }
            }, travelTime);
        }
        else if (MiniDungeonInfo.isDungeonMap(mapid))
        {
            var mmd = chr.getClient().getChannelServer().getMiniDungeon(mapid);
            if (mmd != null)
            {
                mmd.registerPlayer(chr);
            }
        }
        else if (GameConstants.isAriantColiseumArena(mapid))
        {
            int pqTimer = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            chr.sendPacket(PacketCreator.getClock(pqTimer / 1000));
        }

        Pet?[] pets = chr.getPets();
        foreach (Pet? pet in pets)
        {
            if (pet != null)
            {
                pet.setPos(getGroundBelow(chr.getPosition()));
                chr.sendPacket(PacketCreator.showPet(chr, pet, false, false));
            }
            else
            {
                break;
            }
        }
        chr.commitExcludedItems();  // thanks OishiiKawaiiDesu for noticing pet item ignore registry erasing upon changing maps

        chr.removeSandboxItems();

        if (chr.getChalkboard() != null)
        {
            if (!GameConstants.isFreeMarketRoom(mapid))
            {
                chr.sendPacket(PacketCreator.useChalkboard(chr, false)); // update player's chalkboard when changing maps found thanks to Vcoc
            }
            else
            {
                chr.setChalkboard(null);
            }
        }

        if (chr.isHidden())
        {
            broadcastGMSpawnPlayerMapObjectMessage(chr, chr, true);
            chr.sendPacket(PacketCreator.getGMEffect(0x10, 1));

            broadcastGMMessage(chr, PacketCreator.giveForeignBuff(chr.getId(), new BuffStatValue(BuffStat.DARKSIGHT, 0)), false);
        }
        else
        {
            broadcastSpawnPlayerMapObjectMessage(chr, chr, true);
        }

        sendObjectPlacement(chr.Client);

        if (isStartingEventMap() && !eventStarted())
        {
            chr.getMap().getPortal("join00")!.setPortalStatus(false);
        }
        if (hasForcedEquip())
        {
            chr.sendPacket(PacketCreator.showForcedEquip(-1));
        }
        if (specialEquip())
        {
            chr.sendPacket(PacketCreator.coconutScore(0, 0));
            chr.sendPacket(PacketCreator.showForcedEquip(chr.getTeam()));
        }
        objectLock.EnterWriteLock();
        try
        {
            this.mapobjects.AddOrUpdate(chr.getObjectId(), chr);
        }
        finally
        {
            objectLock.ExitWriteLock();
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
            this.addMapObject(dragon);
            if (chr.isHidden())
            {
                this.broadcastGMPacket(chr, PacketCreator.spawnDragon(dragon));
            }
            else
            {
                this.broadcastPacket(chr, PacketCreator.spawnDragon(dragon));
            }
        }

        StatEffect? summonStat = chr.getStatForBuff(BuffStat.SUMMON);
        if (summonStat != null)
        {
            var summon = chr.getSummonByKey(summonStat.getSourceId())!;
            summon.setPosition(chr.getPosition());
            chr.getMap().spawnSummon(summon);
            updateMapObjectVisibility(chr, summon);
        }
        if (mapEffect != null)
        {
            mapEffect.sendStartData(chr.getClient());
        }
        chr.sendPacket(PacketCreator.resetForcedStats());
        if (MapId.isGodlyStatMap(mapid))
        {
            chr.sendPacket(PacketCreator.aranGodlyStats());
        }
        if (chr.getEventInstance() != null && chr.getEventInstance()!.isTimerStarted())
        {
            chr.sendPacket(PacketCreator.getClock((int)(chr.getEventInstance()!.getTimeLeft() / 1000)));
        }
        if (chr.Fitness != null && chr.Fitness.isTimerStarted())
        {
            chr.sendPacket(PacketCreator.getClock((int)(chr.Fitness.getTimeLeft() / 1000)));
        }

        if (chr.Ola != null && chr.Ola.isTimerStarted())
        {
            chr.sendPacket(PacketCreator.getClock((int)(chr.Ola.getTimeLeft() / 1000)));
        }

        if (mapid == MapId.EVENT_SNOWBALL)
        {
            chr.sendPacket(PacketCreator.rollSnowBall());
        }

        if (hasClock())
        {
            var cal = ChannelServer.Container.GetCurrentTimeDateTimeOffSet();
            chr.sendPacket(PacketCreator.getClockTime(cal.Hour, cal.Minute, cal.Second));
        }
        if (hasBoat() > 0)
        {
            chr.sendPacket(PacketCreator.boatPacket(hasBoat() == 1));
        }

        chr.receivePartyMemberHP();
        ChannelServer.Container.CharacterDiseaseManager.registerAnnouncePlayerDiseases(chr.getClient());

        OnPlayerEnter(chr);
    }

    public void removePlayer(IPlayer chr)
    {
        var cserv = chr.getClient().getChannelServer();
        chr.unregisterChairBuff();

        var party = chr.getParty();
        chrLock.EnterWriteLock();
        try
        {
            characters.Remove(chr.Id);
            if (XiGuai?.Controller == chr)
                XiGuai = null;
        }
        finally
        {
            chrLock.ExitWriteLock();
        }

        if (MiniDungeonInfo.isDungeonMap(mapid))
        {
            var mmd = cserv.getMiniDungeon(mapid);
            if (mmd != null)
            {
                if (!mmd.unregisterPlayer(chr))
                {
                    cserv.removeMiniDungeon(mapid);
                }
            }
        }

        removeMapObject(chr.getObjectId());
        if (!chr.isHidden())
        {
            broadcastMessage(PacketCreator.removePlayerFromMap(chr.getId()));
        }
        else
        {
            broadcastGMMessage(PacketCreator.removePlayerFromMap(chr.getId()));
        }

        chr.leaveMap();

        foreach (Summon summon in chr.getSummonsValues())
        {
            if (summon.isStationary())
            {
                chr.cancelEffectFromBuffStat(BuffStat.PUPPET);
            }
            else
            {
                removeMapObject(summon);
            }
        }

        if (chr.getDragon() != null)
        {
            removeMapObject(chr.getDragon()!);
            if (chr.isHidden())
            {
                this.broadcastGMPacket(chr, PacketCreator.removeDragon(chr.getId()));
            }
            else
            {
                this.broadcastPacket(chr, PacketCreator.removeDragon(chr.getId()));
            }
        }

        ChannelServer.Metrics.MapCountDec(Id);
    }

    public Dictionary<int, IPlayer> getMapPlayers()
    {
        return characters;
    }

    public IPlayer? getCharacterById(int id)
    {
        chrLock.EnterReadLock();
        try
        {
            return characters.GetValueOrDefault(id);
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }

    public List<IPlayer> getPlayersInRange(Rectangle box)
    {
        List<IPlayer> character = new();
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in getAllPlayers())
            {
                if (box.Contains(chr.getPosition()))
                {
                    character.Add(chr);
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }

        return character;
    }

    public int countAlivePlayers()
    {
        return getAllPlayers().Count(x => x.isAlive());
    }
    #endregion

    #region Objects: MapItem
    private List<MapItem> droppedItems = new();

    private void instantiateItemDrop(MapItem mdrop)
    {
        if (droppedItems.Count >= YamlConfig.config.server.ITEM_LIMIT_ON_MAP)
        {
            IMapObject? mapobj;

            do
            {
                mapobj = null;

                objectLock.EnterWriteLock();
                try
                {
                    while (mapobj == null)
                    {
                        if (!registeredDrops.TryDequeue(out var item))
                            break;

                        if (item?.TryGetTarget(out var d) ?? false)
                            mapobj = d;
                    }
                }
                finally
                {
                    objectLock.ExitWriteLock();
                }
            } while (!makeDisappearItemFromMap(mapobj));
        }

        objectLock.EnterWriteLock();
        try
        {
            droppedItems.Add(mdrop);
            registeredDrops.Enqueue(new(mdrop));
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    public void unregisterItemDrop(MapItem mdrop)
    {
        objectLock.EnterWriteLock();
        try
        {
            removeMapObject(mdrop);
            droppedItems.Remove(mdrop);
        }
        finally
        {
            objectLock.ExitWriteLock();
        }
    }

    private List<MapItem> getDroppedItems()
    {
        objectLock.EnterReadLock();
        try
        {
            return new(droppedItems);
        }
        finally
        {
            objectLock.ExitReadLock();
        }
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

    public void pickItemDrop(Packet pickupPacket, MapItem mdrop)
    {
        // mdrop must be already locked and not-pickedup checked at this point
        broadcastMessage(pickupPacket, mdrop.getPosition());

        mdrop.setPickedUp(true);
        unregisterItemDrop(mdrop);
    }

    public int countItems()
    {
        objectLock.EnterReadLock();
        try
        {
            return droppedItems.Count;
        }
        finally
        {
            objectLock.ExitReadLock();
        }
    }

    public List<MapItem> getItems()
    {
        return getDroppedItems();
    }
    #endregion

    private void Broadcast(IPlayer? source, double rangeSq, Point? rangedFrom, Action<IPlayer> effectPlayer)
    {
        chrLock.EnterReadLock();
        try
        {
            foreach (IPlayer chr in getAllPlayers())
            {
                if (chr != source)
                {
                    if (rangeSq < double.PositiveInfinity)
                    {
                        if (rangedFrom != null && rangedFrom.Value.distanceSq(chr.getPosition()) <= rangeSq)
                        {
                            effectPlayer(chr);
                        }
                    }
                    else
                    {
                        effectPlayer(chr);
                    }
                }
            }
        }
        finally
        {
            chrLock.ExitReadLock();
        }
    }
    public void BroadcastAll(Action<IPlayer> effectPlayer)
    {
        Broadcast(null, double.PositiveInfinity, null, effectPlayer);
    }

    public void TypedMessage(int type, string messageKey, params string[] param)
    {
        BroadcastAll(e => e.TypedMessage(type, messageKey, param));
    }
    public void Notice(string text, params string[] param)
    {
        TypedMessage(0, text, param);
    }

    public void Popup(string text, params string[] param)
    {
        TypedMessage(1, text, param);
    }

    public void Dialog(string text, params string[] param)
    {
        BroadcastAll(e => e.Dialog(text, param));
    }

    public void Pink(string text, params string[] param)
    {
        TypedMessage(5, text, param);
    }

    public void LightBlue(string text, params string[] param)
    {
        TypedMessage(6, text, param);
    }

    public void LightBlue(Func<ClientCulture, string> action)
    {
        BroadcastAll(e => e.LightBlue(action));
    }

    public void TopScrolling(string text, params string[] param)
    {
        BroadcastAll(e => e.TopScrolling(text, param));
    }

    public void Yellow(string text, params string[] param)
    {
        BroadcastAll(e => e.Yellow(text, param));
    }
}
