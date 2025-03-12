/*
This file is part of the OdinMS Maple Story NewServer
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


using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.ChannelEvents;
using net.netty;
using net.packet;
using net.server.services;
using net.server.services.type;
using scripting.Event;
using server.events.gm;
using server.expeditions;
using server.maps;
using System.Net;
using tools;

namespace net.server.channel;

public class WorldChannel : IWorldChannel
{
    private ILogger log;
    public bool IsRunning { get; private set; }

    public int Port { get; set; }
    private IPEndPoint ipEndPoint;
    private int world;
    private int channel;

    public ChannelPlayerStorage Players { get; }
    private ChannelServer channelServer;
    private string serverMessage = "";
    private MapManager mapManager;
    private EventScriptManager eventSM;
    private ServicesManager<ChannelServices> services;
    private Dictionary<int, HiredMerchant> hiredMerchants = new();
    private Dictionary<int, int> storedVars = new();
    private HashSet<int> playersAway = new();
    private Dictionary<ExpeditionType, Expedition> expeditions = new();
    private Dictionary<int, MiniDungeon> dungeons = new();
    private HashSet<IMap> ownedMaps = new();
    private Event? @event;
    private HashSet<int> usedMC = new();

    public DojoInstance DojoInstance { get; }

    public WeddingChannelInstance WeddingInstance { get; }


    private object lockObj = new object();
    private ReaderWriterLockSlim merchLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public IWorld WorldModel { get; set; }
    public WorldChannel(IWorld world, int channel, long startTime)
    {
        WorldModel = world;
        this.world = world.getId();
        this.channel = channel;
        Players = new ChannelPlayerStorage(this.world, channel);

        this.mapManager = new MapManager(null, this);
        this.Port = world.Configs.StartPort + (this.channel - 1);
        this.ipEndPoint = new IPEndPoint(IPAddress.Parse(YamlConfig.config.server.HOST), Port);

        channelServer = new ChannelServer(Port, this.world, this.channel);
        log = LogFactory.GetLogger($"World_{this.world}/Channel_{channel}");

        setServerMessage(WorldModel.ServerMessage);

        DojoInstance = new DojoInstance(this);
        WeddingInstance = new WeddingChannelInstance(this);

        services = new ServicesManager<ChannelServices>(ChannelServices.OVERALL);
        eventSM = new EventScriptManager(this, ScriptResFactory.GetEvents());
    }


    object loadEvetScriptLock = new object();
    public void reloadEventScriptManager()
    {
        lock (loadEvetScriptLock)
        {
            if (!IsRunning)
            {
                return;
            }

            eventSM.cancel();
            eventSM.dispose();
            eventSM = new EventScriptManager(this, ScriptResFactory.GetEvents());
        }
    }

    public async Task StartServer()
    {
        await channelServer.Start();

        IsRunning = true;

        log.Information("Channel {ChannelId}: Listening on port {Port}", getId(), Port);
    }

    bool isShuttingDown = false;
    public async Task Shutdown()
    {
        try
        {
            if (isShuttingDown || !IsRunning)
            {
                return;
            }

            isShuttingDown = true;
            log.Information("Shutting down channel {ChannelId} in world {WorldId}", channel, world);

            await channelServer.Stop();

            closeAllMerchants();
            disconnectAwayPlayers();
            Players.disconnectAll();

            eventSM.dispose();

            mapManager.dispose();

            await closeChannelSchedules();

            IsRunning = false;
            log.Information("Successfully shut down channel {ChannelId} in world {WorldId}", channel, world);
        }
        catch (Exception e)
        {
            log.Error(e, "Error while shutting down channel {ChannelId} in world {WorldId}", channel, world);
        }
        finally
        {
            isShuttingDown = false;
        }
    }

    private void closeChannelServices()
    {
        services.shutdown();
    }

    private async Task closeChannelSchedules()
    {
        await DojoInstance.DisposeAsync();
        await SchedulerManage.Scheduler.Clear();

        closeChannelServices();
    }

    private void closeAllMerchants()
    {
        try
        {
            List<HiredMerchant> merchs;

            merchLock.EnterWriteLock();
            try
            {
                merchs = new(hiredMerchants.Values);
                hiredMerchants.Clear();
            }
            finally
            {
                merchLock.ExitWriteLock();
            }

            foreach (HiredMerchant merch in merchs)
            {
                merch.forceClose();
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public MapManager getMapFactory()
    {
        return mapManager;
    }

    public BaseService getServiceAccess(ChannelServices sv)
    {
        return services.getAccess(sv).getService();
    }

    public int getWorld()
    {
        return world;
    }

    public IWorld getWorldServer()
    {
        return WorldModel;
    }

    public void addPlayer(IPlayer chr)
    {
        Players.AddPlayer(chr);
        chr.sendPacket(PacketCreator.serverMessage(serverMessage));
    }

    public string getServerMessage()
    {
        return serverMessage;
    }

    public ChannelPlayerStorage getPlayerStorage()
    {
        return Players;
    }

    public bool removePlayer(IPlayer chr)
    {
        return Players.RemovePlayer(chr.Id) != null;
    }

    public int getChannelCapacity()
    {
        return (int)(Math.Ceiling(((float)Players.getAllCharacters().Count / YamlConfig.config.server.CHANNEL_LOAD) * 800));
    }

    public void broadcastPacket(Packet packet)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.sendPacket(packet);
        }
    }

    public int getId()
    {
        return channel;
    }

    public IPEndPoint getIP()
    {
        return ipEndPoint;
    }

    public Event? getEvent()
    {
        return @event;
    }

    public void setEvent(Event? evt)
    {
        this.@event = evt;
    }

    public EventScriptManager getEventSM()
    {
        return eventSM;
    }

    public void broadcastGMPacket(Packet packet)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            if (chr.isGM())
            {
                chr.sendPacket(packet);
            }
        }
    }
    public void insertPlayerAway(int chrId)
    {   
        // either they in CS or MTS
        playersAway.Add(chrId);
    }

    public void removePlayerAway(int chrId)
    {
        playersAway.Remove(chrId);
    }

    public bool canUninstall()
    {
        return Players.Count() == 0 && playersAway.Count == 0;
    }

    private void disconnectAwayPlayers()
    {
        var wserv = getWorldServer();
        foreach (int cid in playersAway)
        {
            var chr = wserv.Players.getCharacterById(cid);
            if (chr != null && chr.IsOnlined)
            {
                chr.getClient().forceDisconnect();
            }
        }
    }

    public Dictionary<int, HiredMerchant> getHiredMerchants()
    {
        merchLock.EnterReadLock();
        try
        {
            return new Dictionary<int, HiredMerchant>(hiredMerchants);
        }
        finally
        {
            merchLock.ExitReadLock();
        }
    }

    public void addHiredMerchant(int chrid, HiredMerchant hm)
    {
        merchLock.EnterWriteLock();
        try
        {
            hiredMerchants.AddOrUpdate(chrid, hm);
        }
        finally
        {
            merchLock.ExitWriteLock();
        }
    }

    public void removeHiredMerchant(int chrid)
    {
        merchLock.EnterWriteLock();
        try
        {
            hiredMerchants.Remove(chrid);
        }
        finally
        {
            merchLock.ExitWriteLock();
        }
    }

    public int[] multiBuddyFind(int charIdFrom, int[] characterIds)
    {
        List<int> ret = new(characterIds.Length);
        var playerStorage = Players;
        foreach (int characterId in characterIds)
        {
            var chr = playerStorage.getCharacterById(characterId);
            if (chr != null)
            {
                if (chr.BuddyList.containsVisible(charIdFrom))
                {
                    ret.Add(characterId);
                }
            }
        }
        return ret.ToArray();
    }

    public bool addExpedition(Expedition exped)
    {
        lock (expeditions)
        {
            if (expeditions.ContainsKey(exped.getType()))
            {
                return false;
            }

            expeditions.Add(exped.getType(), exped);
            exped.beginRegistration();  // thanks Conrad for noticing leader still receiving packets on failure-to-register cases
            return true;
        }
    }

    public void removeExpedition(Expedition exped)
    {
        lock (expeditions)
        {
            expeditions.Remove(exped.getType());
        }
    }

    public Expedition? getExpedition(ExpeditionType type)
    {
        return expeditions.GetValueOrDefault(type);
    }

    public List<Expedition> getExpeditions()
    {
        lock (expeditions)
        {
            return new(expeditions.Values);
        }
    }

    public bool isConnected(string name)
    {
        return Players.getCharacterByName(name) != null;
    }

    public bool isActive()
    {
        EventScriptManager esm = this.getEventSM();
        return esm != null && esm.isActive();
    }

    public void setServerMessage(string message)
    {
        this.serverMessage = message;
        broadcastPacket(PacketCreator.serverMessage(message));
        WorldModel.resetDisabledServerMessages();
    }

    public int getStoredVar(int key)
    {
        return storedVars.GetValueOrDefault(key);
    }

    public void setStoredVar(int key, int val)
    {
        this.storedVars.AddOrUpdate(key, val);
    }

    #region dojo
    public int lookupPartyDojo(ITeam? party)
    {
        return DojoInstance.LookupPartyDojo(party);
    }

    public int ingressDojo(bool isPartyDojo, int fromStage)
    {
        return ingressDojo(isPartyDojo, null, fromStage);
    }

    public int ingressDojo(bool isPartyDojo, ITeam? party, int fromStage)
    {
        return DojoInstance.IngressDojo(isPartyDojo, party, fromStage);
    }

    public void resetDojoMap(int fromMapId)
    {
        DojoInstance.ResetDojoMap(fromMapId);
    }

    public void resetDojo(int dojoMapId)
    {
        DojoInstance.ResetDojo(dojoMapId, -1);
    }

    public void freeDojoSectionIfEmpty(int dojoMapId)
    {
        DojoInstance.FreeDojoSectionIfEmpty(dojoMapId);
    }

    public void dismissDojoSchedule(int dojoMapId, ITeam party)
    {
        DojoInstance.DismissDojoSchedule(dojoMapId, party);
    }

    public bool setDojoProgress(int dojoMapId)
    {
        return DojoInstance.SetDojoProgress(dojoMapId);
    }

    public long getDojoFinishTime(int dojoMapId)
    {
        return DojoInstance.GetDojoFinishTime(dojoMapId);
    }
    #endregion

    public bool addMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (dungeons.ContainsKey(dungeonid))
            {
                return false;
            }

            MiniDungeonInfo mmdi = MiniDungeonInfo.getDungeon(dungeonid);
            MiniDungeon mmd = new MiniDungeon(mmdi.getBase(), this.getMapFactory().getMap(mmdi.getDungeonId()).getTimeLimit());   // thanks Conrad for noticing hardcoded time limit for minidungeons

            dungeons.Add(dungeonid, mmd);
            return true;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public MiniDungeon? getMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            return dungeons.GetValueOrDefault(dungeonid);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            dungeons.Remove(dungeonid);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    #region wedding
    public bool isWeddingReserved(int weddingId)
    {
        return WeddingInstance.IsWeddingReserved(weddingId);
    }

    public int getWeddingReservationStatus(int? weddingId, bool cathedral)
    {
        return WeddingInstance.GetWeddingReservationStatus(weddingId, cathedral);
    }

    public int pushWeddingReservation(int? weddingId, bool cathedral, bool premium, int groomId, int brideId)
    {
        return WeddingInstance.PushWeddingReservation(weddingId, cathedral, premium, groomId, brideId);
    }

    public bool isOngoingWeddingGuest(bool cathedral, int playerId)
    {
        return WeddingInstance.IsOngoingWeddingGuest(cathedral, playerId);
    }

    public int getOngoingWedding(bool cathedral)
    {
        return WeddingInstance.GetOngoingWedding(cathedral);
    }

    public void closeOngoingWedding(bool cathedral)
    {
        WeddingInstance.CloseOngoingWedding(cathedral);
    }

    public bool acceptOngoingWedding(bool cathedral)
    {
        return WeddingInstance.AcceptOngoingWedding(cathedral);
    }

    public string? getWeddingReservationTimeLeft(int? weddingId)
    {
        return WeddingInstance.GetWeddingReservationTimeLeft(weddingId);
    }

    #endregion
    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().getAllCharacters())
        {
            player.dropMessage(type, message);
        }
    }

    public void registerOwnedMap(IMap map)
    {
        ownedMaps.Add(map);
    }

    public void unregisterOwnedMap(IMap map)
    {
        ownedMaps.Remove(map);
    }

    public void runCheckOwnedMapsSchedule()
    {
        if (ownedMaps.Count > 0)
        {
            List<IMap> ownedMapsList;

            lock (ownedMaps)
            {
                ownedMapsList = new(ownedMaps);
            }

            foreach (var map in ownedMapsList)
            {
                map.checkMapOwnerActivity();
            }
        }
    }

    private static int getMonsterCarnivalRoom(bool cpq1, int field)
    {
        return (cpq1 ? 0 : 100) + field;
    }

    public void initMonsterCarnival(bool cpq1, int field)
    {
        usedMC.Add(getMonsterCarnivalRoom(cpq1, field));
    }

    public void finishMonsterCarnival(bool cpq1, int field)
    {
        usedMC.Remove(getMonsterCarnivalRoom(cpq1, field));
    }

    public bool canInitMonsterCarnival(bool cpq1, int field)
    {
        return !usedMC.Contains(getMonsterCarnivalRoom(cpq1, field));
    }
}
