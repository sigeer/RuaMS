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
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using scripting.Event.scheduler;
using server;
using server.expeditions;
using server.life;
using server.quest;
using System.Collections.Concurrent;
using tools.exceptions;

//using jdk.nashorn.api.scripting;

namespace scripting.Event;

/**
 * @author Matze
 * @author Ronan
 */
public class EventManager
{
    ILogger log = LogFactory.GetLogger(LogType.EventManager);
    private IEngine iv;
    private WorldChannel cserv;
    private EventScriptScheduler ess;

    private ConcurrentDictionary<string, EventInstanceManager> instances = new();
    private Dictionary<int, bool> openedLobbys = new();

    private Queue<int> queuedGuilds = new();
    private Dictionary<int, int> queuedGuildLeaders = new();

    /// <summary>
    /// 预生成的 EventInstanceManager
    /// </summary>
    private Queue<EventInstanceManager> readyInstances = new();
    private int readyId = 0, onLoadInstances = 0;
    private Dictionary<string, string> props = new Dictionary<string, string>();
    /// <summary>
    /// 脚本名
    /// </summary>
    private string name;

    private object lobbyLock = new object();
    private object queueLock = new object();
    private object startLock = new object();

    private HashSet<int> playerPermit = new();
    private SemaphoreSlim startSemaphore = new SemaphoreSlim(7);


    public int MaxLobbys { get; set; }
    public const int DefaultMaxLobbys = 1;


    public EventManager(WorldChannel cserv, IEngine iv, string name)
    {
        this.iv = iv;
        this.cserv = cserv;
        this.name = name;

        this.ess = new EventScriptScheduler(cserv);
    }

    private bool isDisposed()
    {
        return onLoadInstances <= -1000;
    }

    public void cancel()
    {
        // make sure to only call this when there are NO PLAYERS ONLINE to mess around with the event manager!
        ess.dispose();

        try
        {
            iv.CallFunction("cancelSchedule");
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
        }

        var eimList = instances.Values.ToList();
        instances.Clear();

        foreach (EventInstanceManager eim in eimList)
        {
            eim.dispose(true);
        }

        Queue<EventInstanceManager> readyEims;
        Monitor.Enter(queueLock);
        try
        {
            readyEims = new(readyInstances);
            readyInstances.Clear();
            onLoadInstances = int.MinValue / 2;
        }
        finally
        {
            Monitor.Exit(queueLock);
        }

        foreach (EventInstanceManager eim in readyEims)
        {
            eim.dispose(true);
        }

        props.Clear();
        iv.Dispose();
    }

    public long getLobbyDelay()
    {
        return YamlConfig.config.server.EVENT_LOBBY_DELAY;
    }

    private int getMaxLobbies()
    {
        if (MaxLobbys > 0)
            return MaxLobbys;

        try
        {
            MaxLobbys = iv.CallFunction("getMaxLobbies").ToObject<int>();
        }
        catch (Exception ex)
        {
            // they didn't define a lobby range
            log.Error(ex, "Script: {Script}", name);
            MaxLobbys = DefaultMaxLobbys;
        }
        return MaxLobbys;
    }

    public EventScheduledFuture schedule(string methodName, long delay)
    {
        return schedule(methodName, null, delay);
    }

    public EventScheduledFuture schedule(string methodName, EventInstanceManager? eim, long delay)
    {
        var r = () =>
        {
            try
            {
                iv.CallFunction(methodName, eim);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Event script schedule, Script: {Script}, Method: {Method}", name, methodName);
            }
        };

        ess.registerEntry(r, delay);

        // hate to do that, but those schedules can still be cancelled, so well... Let GC do it's job
        return new EventScheduledFuture(r, ess);
    }

    /// <summary>
    /// js调用的代码已注释
    /// </summary>
    /// <returns></returns>
    //public EventScheduledFuture scheduleAtTimestamp(string methodName, long timestamp)
    //{
    //    var r = () =>
    //    {
    //        try
    //        {
    //            iv.CallFunction(methodName);
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error(ex, "Event script scheduleAtTimestamp, Script: {Script}", name);
    //        }
    //    };

    //    ess.registerEntry(r, timestamp - server.getCurrentTime());
    //    return new EventScheduledFuture(r, ess);
    //}

    public WorldChannel getChannelServer()
    {
        return cserv;
    }

    public IMap GetMap(int mapId)
    {
        var map = getChannelServer().getMapFactory().getMap(mapId);
        map.IsTrackedByEvent = true;
        return map;
    }

    public IEngine getIv()
    {
        return iv;
    }

    public EventInstanceManager? getInstance(string name)
    {
        return instances.GetValueOrDefault(name);
    }

    public ICollection<EventInstanceManager> getInstances()
    {
        return instances.Values.ToList();
    }

    private EventInstanceManager? getReadyInstance()
    {
        Monitor.Enter(queueLock);
        try
        {
            if (readyInstances.TryDequeue(out var eim))
                return eim;

            return null;
        }
        finally
        {
            fillEimQueue();
            Monitor.Exit(queueLock);
        }
    }

    public EventInstanceManager newInstance(string instanceName)
    {
        var ret = getReadyInstance() ?? new EventInstanceManager(this, instanceName);

        ret.setName(instanceName);
        return ret;
    }


    public bool RegisterInstance(string instanceName, EventInstanceManager eim)
    {
        return instances.TryAdd(instanceName, eim);
    }

    void DisposeInstanceInternal(string name)
    {
        if (instances.TryRemove(name, out var eim) && eim != null && eim.LobbyId > -1)
            DisposeLobby(eim.LobbyId);
    }

    public void disposeInstance(string name)
    {
        ess.registerEntry(() =>
        {
            DisposeInstanceInternal(name);
        }, YamlConfig.config.server.EVENT_LOBBY_DELAY * 1000);
    }

    public void setProperty(string key, string value)
    {
        props.AddOrUpdate(key, value);
    }

    public void setIntProperty(string key, int value)
    {
        setProperty(key, value);
    }

    public void setProperty(string key, long value)
    {
        setProperty(key, value.ToString());
    }

    public string? getProperty(string key)
    {
        return props.GetValueOrDefault(key);
    }

    public int getIntProperty(string key)
    {
        return int.Parse(props.GetValueOrDefault(key) ?? "0");
    }


    void DisposeLobby(int lobbyId)
    {
        Monitor.Enter(lobbyLock);
        try
        {
            openedLobbys[lobbyId] = false;
        }
        finally
        {
            Monitor.Exit(lobbyLock);
        }
    }

    private bool startLobbyInstance(int lobbyId)
    {
        Monitor.Enter(lobbyLock);
        try
        {
            if (lobbyId < 0)
            {
                lobbyId = 0;
            }

            if (!openedLobbys.TryGetValue(lobbyId, out var value) || !value)
            {
                openedLobbys[lobbyId] = true;
                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(lobbyLock);
        }
    }


    public string getName()
    {
        return name;
    }

    private int GetAvailableLobbyInstance()
    {
        int maxLobbies = getMaxLobbies();

        if (maxLobbies > 0)
        {
            for (int i = 0; i < maxLobbies; i++)
            {
                if (startLobbyInstance(i))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private string getInternalScriptExceptionMessage(Exception a)
    {
        return a.InnerException?.ToString();
    }

    private EventInstanceManager createInstance(string name, params object?[] args)
    {
        return iv.CallFunction(name, args).ToObject<EventInstanceManager>();
    }

    private void registerEventInstance(EventInstanceManager eim, int lobbyId)
    {
        if (!RegisterInstance(eim.getName(), eim))
            throw new EventInstanceInProgressException(eim.getName(), this.getName());
        eim.LobbyId = lobbyId;
    }

    public bool startInstance(Expedition exped)
    {
        return startInstance(-1, exped);
    }

    public bool startInstance(int lobbyId, Expedition exped)
    {
        return startInstance(lobbyId, exped, exped.getLeader());
    }

    //Expedition method: starts an expedition
    public bool startInstance(int lobbyId, Expedition exped, IPlayer leader)
    {
        if (this.isDisposed())
        {
            return false;
        }

        try
        {
            if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
            {
                playerPermit.Add(leader.getId());

                Monitor.Enter(startLock);
                try
                {
                    try
                    {
                        if (lobbyId == -1)
                        {
                            lobbyId = GetAvailableLobbyInstance();
                            if (lobbyId == -1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!startLobbyInstance(lobbyId))
                            {
                                return false;
                            }
                        }

                        EventInstanceManager eim;
                        try
                        {
                            eim = createInstance("setup", leader.getClient().getChannel());
                            registerEventInstance(eim, lobbyId);
                        }
                        catch (Exception e)
                        {
                            string message = getInternalScriptExceptionMessage(e);
                            if (message != null && !message.StartsWith(EventInstanceInProgressException.EIIP_KEY))
                            {
                                throw;
                            }

                            DisposeLobby(lobbyId);
                            return false;
                        }

                        eim.setLeader(leader);

                        exped.start();
                        eim.registerExpedition(exped);

                        eim.startEvent();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Event script startInstance");
                    }

                    return true;
                }
                finally
                {
                    Monitor.Exit(startLock);
                    playerPermit.Remove(leader.getId());
                    startSemaphore.Release();
                }
            }
        }
        catch (ThreadInterruptedException ie)
        {
            log.Error(ie.ToString());
            playerPermit.Remove(leader.getId());
        }

        return false;
    }

    //Regular method: player 
    public bool startInstance(IPlayer chr)
    {
        return startInstance(-1, chr);
    }

    public bool startInstance(int lobbyId, IPlayer leader)
    {
        return startInstance(lobbyId, leader, leader, 1);
    }

    public bool startInstance(int lobbyId, IPlayer chr, IPlayer leader, int difficulty)
    {
        if (this.isDisposed())
        {
            return false;
        }

        try
        {
            if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
            {
                playerPermit.Add(leader.getId());

                Monitor.Enter(startLock);
                try
                {
                    try
                    {
                        if (lobbyId == -1)
                        {
                            lobbyId = GetAvailableLobbyInstance();
                            if (lobbyId == -1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!startLobbyInstance(lobbyId))
                            {
                                return false;
                            }
                        }

                        EventInstanceManager eim;
                        try
                        {
                            eim = createInstance("setup", difficulty, (lobbyId > -1) ? lobbyId : leader.getId());
                            registerEventInstance(eim, lobbyId);
                        }
                        catch (Exception e)
                        {
                            string message = getInternalScriptExceptionMessage(e);
                            if (message != null && !message.StartsWith(EventInstanceInProgressException.EIIP_KEY))
                            {
                                throw;
                            }

                            DisposeLobby(lobbyId);
                            return false;
                        }
                        eim.setLeader(leader);

                        if (chr != null)
                        {
                            eim.registerPlayer(chr);
                        }

                        eim.startEvent();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Event script startInstance");
                    }

                    return true;
                }
                finally
                {
                    Monitor.Exit(startLock);
                    playerPermit.Remove(leader.getId());
                    startSemaphore.Release();
                }
            }
        }
        catch (ThreadInterruptedException ie)
        {
            log.Error(ie.ToString());
            playerPermit.Remove(leader.getId());
        }

        return false;
    }

    //PQ method: starts a PQ
    public bool startInstance(Team party, IMap map)
    {
        return startInstance(-1, party, map);
    }

    public bool startInstance(int lobbyId, Team party, IMap map)
    {
        return startInstance(lobbyId, party, map, party.GetChannelLeader(cserv));
    }

    public bool startInstance(int lobbyId, Team party, IMap map, IPlayer? leader)
    {
        return startInstance(lobbyId, party, map, 0, leader);
    }

    //PQ method: starts a PQ with a difficulty level, requires function setup(difficulty, leaderid) instead of setup()
    public bool startInstance(Team party, IMap map, int difficulty)
    {
        return startInstance(-1, party, map, difficulty);
    }

    public bool startInstance(int lobbyId, Team party, IMap map, int difficulty)
    {
        return startInstance(lobbyId, party, map, difficulty, party.GetChannelLeader(cserv));
    }

    public bool startInstance(int lobbyId, Team party, IMap map, int difficulty, IPlayer? leader)
    {
        if (leader == null)
        {
            log.Information("队长不在同一频道");
            return false;
        }
        if (this.isDisposed())
        {
            return false;
        }

        try
        {
            if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
            {
                playerPermit.Add(leader.getId());

                Monitor.Enter(startLock);
                try
                {
                    try
                    {
                        if (lobbyId == -1)
                        {
                            lobbyId = GetAvailableLobbyInstance();
                            if (lobbyId == -1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!startLobbyInstance(lobbyId))
                            {
                                return false;
                            }
                        }

                        EventInstanceManager? eim = null;
                        try
                        {
                            eim = createInstance("setup", difficulty, (lobbyId > -1) ? lobbyId : party.getLeaderId());
                            registerEventInstance(eim, lobbyId);
                        }
                        catch (Exception e)
                        {
                            string message = getInternalScriptExceptionMessage(e);
                            if (message != null && !message.StartsWith(EventInstanceInProgressException.EIIP_KEY))
                            {
                                throw;
                            }

                            DisposeLobby(lobbyId);
                            return false;
                        }


                        eim.setLeader(leader);

                        eim.registerParty(party, map);
                        party.setEligibleMembers([]);

                        eim.startEvent();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Event script startInstance");
                    }

                    return true;
                }
                finally
                {
                    Monitor.Exit(startLock);
                    playerPermit.Remove(leader.getId());
                    startSemaphore.Release();
                }
            }
        }
        catch (ThreadInterruptedException ie)
        {
            log.Error(ie.ToString());
            playerPermit.Remove(leader.getId());
        }

        return false;
    }

    //non-PQ method for starting instance
    //public bool startInstance(EventInstanceManager eim, string ldr)
    //{
    //    return startInstance(-1, eim, ldr);
    //}

    //public bool startInstance(EventInstanceManager eim, IPlayer ldr)
    //{
    //    return startInstance(-1, eim, ldr.getName(), ldr);
    //}

    //public bool startInstance(int lobbyId, EventInstanceManager eim, string ldr)
    //{
    //    return startInstance(lobbyId, eim, ldr, eim.getEm().getChannelServer().getPlayerStorage().getCharacterByName(ldr));  // things they make me do...
    //}

    //public bool startInstance(int lobbyId, EventInstanceManager eim, string ldr, IPlayer leader)
    //{
    //    if (this.isDisposed())
    //    {
    //        return false;
    //    }

    //    try
    //    {
    //        if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
    //        {
    //            playerPermit.Add(leader.getId());

    //            Monitor.Enter(startLock);
    //            try
    //            {
    //                try
    //                {
    //                    if (lobbyId == -1)
    //                    {
    //                        lobbyId = GetAvailableLobbyInstance();
    //                        if (lobbyId == -1)
    //                        {
    //                            return false;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (!startLobbyInstance(lobbyId))
    //                        {
    //                            return false;
    //                        }
    //                    }

    //                    if (eim == null)
    //                    {
    //                        DisposeLobby(lobbyId);
    //                        return false;
    //                    }
    //                    registerEventInstance(eim.getName(), lobbyId);
    //                    eim.setLeader(leader);

    //                    iv.CallFunction("setup", eim);
    //                    eim.setProperty("leader", ldr);

    //                    eim.startEvent();
    //                }
    //                catch (Exception ex)
    //                {
    //                    log.Error(ex, "Event script startInstance, Script: {Script}", name);
    //                }

    //                return true;
    //            }
    //            finally
    //            {
    //                Monitor.Exit(startLock);
    //                playerPermit.Remove(leader.getId());
    //                startSemaphore.Release();
    //            }
    //        }
    //    }
    //    catch (ThreadInterruptedException)
    //    {
    //        playerPermit.Remove(leader.getId());
    //    }

    //    return false;
    //}

    public List<IPlayer> getEligibleParty(Team party)
    {
        if (party == null)
        {
            return new();
        }
        try
        {
            var result = iv.CallFunction("getEligibleParty", party.GetChannelMembers(cserv));
            var eligibleParty = result.ToObject<List<IPlayer>>();
            party.setEligibleMembers(eligibleParty);
            return eligibleParty;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Script: {Script}", name);
        }

        return new();
    }

    public void clearPQ(EventInstanceManager eim)
    {
        try
        {
            iv.CallFunction("clearPQ", eim);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script clearPQ, Script: {Script}", name);
        }
    }

    public void clearPQ(EventInstanceManager eim, IMap toMap)
    {
        try
        {
            iv.CallFunction("clearPQ", eim, toMap);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script clearPQ, Script: {Script}", name);
        }
    }

    public Monster? getMonster(int mid)
    {
        return (LifeFactory.Instance.getMonster(mid));
    }

    private void exportReadyGuild(int guildId)
    {
        string callout = "[Guild Quest] Your guild has been registered to attend to the Sharenian Guild Quest at channel " + this.getChannelServer().getId()
            + " and HAS JUST STARTED THE STRATEGY PHASE. After 3 minutes, no more guild members will be allowed to join the effort."
            + " Check out Shuang at the excavation site in Perion for more info.";

        cserv.Container.GuildManager.DropGuildMessage(guildId, 6, callout);
    }

    private void exportMovedQueueToGuild(int guildId, int place)
    {
        string callout = "[Guild Quest] Your guild has been registered to attend to the Sharenian Guild Quest at channel " + this.getChannelServer().getId()
                + " and is currently on the " + GameConstants.ordinal(place) + " place on the waiting queue.";

        cserv.Container.GuildManager.DropGuildMessage(guildId, 6, callout);
    }

    private List<int>? getNextGuildQueue()
    {
        lock (queuedGuilds)
        {
            if (!queuedGuilds.TryDequeue(out var guildId))
                return null;

            cserv.Container.Transport.RemoveGuildQueued(guildId);
            var leaderId = queuedGuildLeaders.Remove(guildId, out var d) ? d : 0;

            int place = 1;
            foreach (int i in queuedGuilds)
            {
                exportMovedQueueToGuild(i, place);
                place++;
            }

            List<int> list = new(2);
            list.Add(guildId);
            list.Add(leaderId);
            return list;
        }
    }

    public bool isQueueFull()
    {
        lock (queuedGuilds)
        {
            return queuedGuilds.Count >= YamlConfig.config.server.EVENT_MAX_GUILD_QUEUE;
        }
    }

    public int getQueueSize()
    {
        lock (queuedGuilds)
        {
            return queuedGuilds.Count;
        }
    }

    public sbyte addGuildToQueue(int guildId, int leaderId)
    {
        if (cserv.Container.Transport.IsGuildQueued(guildId))
        {
            return -1;
        }

        if (!isQueueFull())
        {
            bool canStartAhead;
            lock (queuedGuilds)
            {
                canStartAhead = queuedGuilds.Count == 0;

                queuedGuilds.Enqueue(guildId);
                cserv.Container.Transport.PutGuildQueued(guildId);
                queuedGuildLeaders.AddOrUpdate(guildId, leaderId);

                int place = queuedGuilds.Count;
                exportMovedQueueToGuild(guildId, place);
            }

            if (canStartAhead)
            {
                if (!attemptStartGuildInstance())
                {
                    lock (queuedGuilds)
                    {
                        queuedGuilds.Enqueue(guildId);
                        cserv.Container.Transport.PutGuildQueued(guildId);
                        queuedGuildLeaders.AddOrUpdate(guildId, leaderId);
                    }
                }
                else
                {
                    return 2;
                }
            }

            return 1;
        }
        else
        {
            return 0;
        }
    }

    public bool attemptStartGuildInstance()
    {
        IPlayer? chr = null;
        List<int>? guildInstance = null;
        while (chr == null)
        {
            guildInstance = getNextGuildQueue();
            if (guildInstance == null)
            {
                return false;
            }

            chr = cserv.getPlayerStorage().getCharacterById(guildInstance.get(1));
        }

        if (startInstance(chr))
        {
            exportReadyGuild(guildInstance!.get(0));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void startQuest(IPlayer chr, int id, int npcid)
    {
        try
        {
            Quest.getInstance(id).forceStart(chr, npcid);
        }
        catch (NullReferenceException ex)
        {
            log.Error(ex.ToString());
        }
    }

    public void completeQuest(IPlayer chr, int id, int npcid)
    {
        try
        {
            Quest.getInstance(id).forceComplete(chr, npcid);
        }
        catch (NullReferenceException ex)
        {
            log.Error(ex.ToString());
        }
    }

    public int getTransportationTime(double travelTime)
    {
        return cserv.getTransportationTime(travelTime);
    }

    private void fillEimQueue()
    {
        ThreadManager.getInstance().newTask(instantiateQueuedInstance);
    }


    private void instantiateQueuedInstance()
    {
        int nextEventId;
        Monitor.Enter(queueLock);
        try
        {
            if (this.isDisposed() || readyInstances.Count + onLoadInstances >= Math.Ceiling(MaxLobbys / 3.0))
            {
                return;
            }

            onLoadInstances++;
            nextEventId = readyId;
            readyId++;
        }
        finally
        {
            Monitor.Exit(queueLock);
        }

        EventInstanceManager eim = new EventInstanceManager(this, "sampleName" + nextEventId);
        Monitor.Enter(queueLock);
        try
        {
            if (this.isDisposed())
            {  // EM already disposed
                return;
            }

            readyInstances.Enqueue(eim);
            onLoadInstances--;
        }
        finally
        {
            Monitor.Exit(queueLock);
        }

        instantiateQueuedInstance();    // keep filling the queue until reach threshold.
    }
}