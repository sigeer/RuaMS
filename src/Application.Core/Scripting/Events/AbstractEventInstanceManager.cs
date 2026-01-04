using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.model;
using Application.Shared.Events;
using scripting.Event.scheduler;
using server;
using server.life;
using server.maps;
using System.IO;
using tools;

namespace Application.Core.Scripting.Events;

public abstract class AbstractEventInstanceManager : IClientMessenger, IDisposable
{
    protected ILogger log = LogFactory.GetLogger("EventInstanceManger");
    private Dictionary<int, Player> chars = new();
    /// <summary>
    /// 每关 已领取奖励的玩家
    /// </summary>
    protected Dictionary<int, HashSet<int>> rewardedChr = new();
    private int leaderId = -1;
    private List<Monster> mobs = new();
    private Dictionary<Player, int> killCount = new();
    public AbstractInstancedEventManager EventManager { get; }
    private EventScriptScheduler ess;
    protected MapManager mapManager;
    private string name;
    private Dictionary<string, object> props = new();
    private Dictionary<string, object> objectProps = new();
    private long timeStarted = 0;
    private long eventTime = 0;

    public int LobbyId { get; set; } = -1;

    private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    protected object propertyLock = new object();
    protected object scriptLock = new object();

    private ScheduledFuture? event_schedule = null;
    private bool disposed = false;
    protected bool eventCleared = false;
    protected bool eventStarted = false;

    // multi-leveled PQ rewards!
    Dictionary<int, EventRewardsPair> eventRewards = new Dictionary<int, EventRewardsPair>(YamlConfig.config.server.MAX_EVENT_LEVELS);

    // Exp/Meso rewards by CLEAR on a stage
    private List<int> onMapClearExp = new();
    private List<int> onMapClearMeso = new();

    // registers player status on an event (null on this Map structure equals to 0)
    private Dictionary<int, int> playerGrid = new();

    // registers all opened gates on the event. Will help late characters to encounter next stages gates already opened
    private Dictionary<int, KeyValuePair<string, int>?> openedGates = new();

    // forces deletion of items not supposed to be held outside of the event, dealt on a player's leaving moment.
    private HashSet<int> exclusiveItems = new();
    public EventInstanceType Type { get; set; }

    public AbstractEventInstanceManager(AbstractInstancedEventManager em, string name)
    {
        EventManager = em;
        this.name = name;
        this.ess = new EventScriptScheduler(EventManager.getChannelServer());
        this.mapManager = new MapManager(this, EventManager.getChannelServer());
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public EventManager getEm() => EventManager;

    public int getEventPlayersJobs()
    {
        //Bits -> 0: BEGINNER 1: WARRIOR 2: MAGICIAN
        //        3: BOWMAN 4: THIEF 5: PIRATE

        int mask = 0;
        foreach (var chr in getPlayers())
        {
            mask |= (1 << chr.getJob().GetJobNiche());
        }

        return mask;
    }

    public void applyEventPlayersItemBuff(int itemId)
    {
        List<Player> players = getPlayerList();
        var mse = ItemInformationProvider.getInstance().getItemEffect(itemId);

        if (mse != null)
        {
            foreach (Player player in players)
            {
                mse.applyTo(player);
            }
        }
    }

    public void applyEventPlayersSkillBuff(int skillId)
    {
        applyEventPlayersSkillBuff(skillId, int.MaxValue);
    }

    public void applyEventPlayersSkillBuff(int skillId, int skillLv)
    {
        List<Player> players = getPlayerList();
        var skill = SkillFactory.getSkill(skillId);

        if (skill != null)
        {
            StatEffect mse = skill.getEffect(Math.Min(skillLv, skill.getMaxLevel()));
            if (mse != null)
            {
                foreach (Player player in players)
                {
                    mse.applyTo(player);
                }
            }
        }
    }

    public void giveEventPlayersExp(int gain, int mapId = -1)
    {
        if (gain <= 0)
            return;

        var bonus = Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PARTY_BONUS_EXP_RATE : 1;
        List<Player> players = getPlayerList();

        if (mapId == -1)
        {
            foreach (Player mc in players)
            {
                mc.gainExp((int)(gain * mc.getExpRate() * bonus), true, true);
            }
        }
        else
        {
            foreach (Player mc in players)
            {
                if (mc.getMapId() == mapId)
                {
                    mc.gainExp((int)(gain * mc.getExpRate() * bonus), true, true);
                }
            }
        }
    }


    public void giveEventPlayersMeso(int gain, int mapId = -1)
    {
        if (gain == 0)
        {
            return;
        }

        List<Player> players = getPlayerList();

        if (mapId == -1)
        {
            foreach (Player mc in players)
            {
                mc.gainMeso((int)(gain * mc.getMesoRate()), inChat: true);
            }
        }
        else
        {
            foreach (Player mc in players)
            {
                if (mc.getMapId() == mapId)
                {
                    mc.gainMeso((int)(gain * mc.getMesoRate()), inChat: true);
                }
            }
        }

    }



    #region 触发脚本事件
    public object? invokeScriptFunction(string name, params object[] args)
    {
        if (!disposed)
        {
            return EventManager.getIv().CallFunction(name, args).ToObject();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 离开地图
    /// </summary>
    /// <param name="chr"></param>
    public virtual void exitPlayer(Player chr)
    {
        if (chr == null || !chr.isLoggedin())
        {
            return;
        }

        unregisterPlayer(chr);

        try
        {
            invokeScriptFunction("playerExit", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerExit", EventManager.getName());
        }
    }

    public virtual void unregisterPlayer(Player chr)
    {
        try
        {
            invokeScriptFunction("playerUnregistered", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script {ScriptName} does not implement the playerUnregistered function", EventManager.getName());
        }

        lockObj.EnterWriteLock();
        try
        {
            chars.Remove(chr.getId());
            chr.setEventInstance(null);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }

        gridRemove(chr);
        dropExclusiveItems(chr);
    }

    public virtual void changedMap(Player chr, int mapId)
    {
        try
        {
            invokeScriptFunction("changedMap", this, chr, mapId);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "changedMap", EventManager.getName());
        } // optional
    }

    public virtual void afterChangedMap(Player chr, int mapId)
    {
        try
        {
            invokeScriptFunction("afterChangedMap", this, chr, mapId);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "afterChangedMap", EventManager.getName());
        } // optional
    }

    object changeLeaderLock = new object();
    public virtual void changedLeader(Player ldr)
    {
        lock (changeLeaderLock)
        {
            try
            {
                invokeScriptFunction("changedLeader", this, ldr);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "changedLeader", EventManager.getName());
            }

            leaderId = ldr.getId();
        }

    }

    public virtual void monsterKilled(Monster mob, bool hasKiller)
    {
        int scriptResult = 0;

        Monitor.Enter(scriptLock);
        try
        {
            mobs.Remove(mob);

            if (eventStarted)
            {
                scriptResult = 1;

                if (mobs.Count == 0)
                {
                    scriptResult = 2;
                }
            }
        }
        finally
        {
            Monitor.Exit(scriptLock);
        }

        if (scriptResult > 0)
        {
            try
            {
                invokeScriptFunction("monsterKilled", mob, this, hasKiller);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "monsterKilled", EventManager.getName());
            }

            if (scriptResult > 1)
            {
                try
                {
                    invokeScriptFunction("allMonstersDead", this, hasKiller);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "allMonstersDead", EventManager.getName());
                }
            }
        }
    }

    public virtual void friendlyKilled(Monster mob, bool hasKiller)
    {
        try
        {
            invokeScriptFunction("friendlyKilled", mob, this, hasKiller);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "friendlyKilled", EventManager.getName());
        } //optional
    }

    public virtual void friendlyDamaged(Monster mob)
    {
        try
        {
            invokeScriptFunction("friendlyDamaged", this, mob);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "friendlyDamaged", EventManager.getName());
        } // optional
    }

    public virtual void friendlyItemDrop(Monster mob)
    {
        try
        {
            invokeScriptFunction("friendlyItemDrop", this, mob);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "friendlyItemDrop", EventManager.getName());
        } // optional
    }

    public virtual void playerKilled(Player chr)
    {
        try
        {
            invokeScriptFunction("playerDead", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerDead", EventManager.getName());
        } // optional
    }

    public virtual void reviveMonster(Monster mob)
    {
        try
        {
            invokeScriptFunction("monsterRevive", this, mob);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "monsterRevive", EventManager.getName());
        } // optional
    }

    public virtual bool revivePlayer(Player chr)
    {
        try
        {
            return Convert.ToBoolean(invokeScriptFunction("playerRevive", this, chr) ?? true);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerRevive", EventManager.getName());
        } // optional

        return true;
    }

    public virtual void playerDisconnected(Player chr)
    {
        try
        {
            invokeScriptFunction("playerDisconnected", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerDisconnected", EventManager.getName());
        }

        if (getEm().AllowReconnect)
        {
            chr.Client.CurrentServer.EventRecallManager?.storeEventInstance(chr.Id, this);
        }
    }

    public virtual void monsterKilled(Player chr, Monster mob)
    {
        try
        {
            int inc = Convert.ToInt32(invokeScriptFunction("monsterValue", this, mob.getId()));

            if (inc != 0)
            {
                OnMonsterValueChanged(chr, mob, inc);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "monsterValue", EventManager.getName());
        }
    }

    protected virtual void OnMonsterValueChanged(Player chr, Monster mob, int val)
    {
        killCount[chr] = killCount.GetValueOrDefault(chr) + val;
    }

    public virtual void leftParty(Player chr)
    {
        try
        {
            invokeScriptFunction("leftParty", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "leftParty", EventManager.getName());
        }
    }

    public virtual void disbandParty()
    {
        try
        {
            invokeScriptFunction("disbandParty", this);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "disbandParty", EventManager.getName());
        }
    }

    public virtual void clearPQ()
    {
        try
        {
            invokeScriptFunction("clearPQ", this);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "clearPQ", EventManager.getName());
        }
    }

    /// <summary>
    /// playerExit
    /// </summary>
    /// <param name="chr"></param>
    public virtual void removePlayer(Player chr)
    {
        try
        {
            invokeScriptFunction("playerExit", this, chr);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerExit", EventManager.getName());
        }
    }

    public virtual void startEvent()
    {
        try
        {
            invokeScriptFunction("afterSetup", this);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "afterSetup", EventManager.getName());
        }
    }

    public virtual void setEventCleared()
    {
        eventCleared = true;

        foreach (Player chr in getPlayers())
        {
            chr.awardQuestPoint(YamlConfig.config.server.QUEST_POINT_PER_EVENT_CLEAR);
        }

        Monitor.Enter(scriptLock);
        try
        {
            EventManager.disposeInstance(name);
        }
        finally
        {
            Monitor.Exit(scriptLock);
        }
    }
    #endregion

    object registerLock = new object();

    public void registerPlayer(Player chr, bool runEntryScript = true)
    {

        lock (registerLock)
        {
            if (chr == null || !chr.isLoggedinWorld() || disposed)
            {
                return;
            }

            lockObj.EnterWriteLock();
            try
            {
                if (chars.ContainsKey(chr.getId()))
                {
                    return;
                }

                chars.AddOrUpdate(chr.getId(), chr);
                chr.setEventInstance(this);
            }
            finally
            {
                lockObj.ExitWriteLock();
            }

            if (runEntryScript)
            {
                try
                {
                    invokeScriptFunction("playerEntry", this, chr);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "playerEntry", EventManager.getName());
                }
            }
        }

    }



    public void dropMessage(int type, string message)
    {
        foreach (Player chr in getPlayers())
        {
            chr.TypedMessage(type, message);
        }
    }

    public void restartEventTimer(long time)
    {
        stopEventTimer();
        startEventTimer(time);
    }

    public void startEventTimer(long time)
    {
        timeStarted = EventManager.getChannelServer().Container.getCurrentTime();
        eventTime = time;

        foreach (Player chr in getPlayers())
        {
            chr.sendPacket(PacketCreator.getClock((int)(time / 1000)));
        }

        event_schedule = EventManager.getChannelServer().Container.TimerManager.schedule(() =>
        {
            dismissEventTimer();

            try
            {
                invokeScriptFunction("scheduledTimeout", this);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Event script {ScriptName} does not implement the scheduledTimeout function", EventManager.getName());
            }
        }, time);
    }

    private void dismissEventTimer()
    {
        foreach (Player chr in getPlayers())
        {
            chr.sendPacket(PacketCreator.removeClock());
        }

        event_schedule = null;
        eventTime = 0;
        timeStarted = 0;
    }

    public void stopEventTimer()
    {
        if (event_schedule != null)
        {
            event_schedule.cancel(false);
            event_schedule = null;
        }

        dismissEventTimer();
    }

    public bool isTimerStarted()
    {
        return eventTime > 0 && timeStarted > 0;
    }

    public long getTimeLeft()
    {
        return eventTime - (EventManager.getChannelServer().Container.getCurrentTime() - timeStarted);
    }

    public virtual void registerParty(List<Player> eligibleMembers)
    {
        foreach (var mpc in eligibleMembers)
        {
            if (mpc.IsOnlined)
            {
                registerPlayer(mpc);
            }
        }
    }

    public int getPlayerCount()
    {
        lockObj.EnterReadLock();
        try
        {
            return chars.Count;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public Player? getPlayerById(int id)
    {
        lockObj.EnterReadLock();
        try
        {
            return chars.GetValueOrDefault(id);
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public List<Player> getPlayers()
    {
        lockObj.EnterReadLock();
        try
        {
            return new(chars.Values);
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    private List<Player> getPlayerList()
    {
        lockObj.EnterReadLock();
        try
        {
            return chars.Values.ToList();
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public void registerMonster(Monster mob)
    {
        if (!mob.getStats().isFriendly())
        {
            //We cannot register moon bunny
            mobs.Add(mob);
        }
    }


    public int getKillCount(Player chr)
    {
        return killCount.GetValueOrDefault(chr, 0);
    }


    Lock disposeLock = new();
    public virtual void Dispose()
    {
        lock (disposeLock)
        {
            // should not trigger any event script method after disposed
            if (disposed)
            {
                return;
            }

            try
            {
                invokeScriptFunction("dispose", this);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "dispose", EventManager.getName());
            }
            disposed = true;

            ess.dispose();

            lockObj.EnterWriteLock();
            try
            {
                foreach (Player chr in chars.Values)
                {
                    chr.setEventInstance(null);
                }
                chars.Clear();
                mobs.Clear();
                // ess = null;
            }
            finally
            {
                lockObj.ExitWriteLock();
            }

            if (event_schedule != null)
            {
                event_schedule.cancel(false);
                event_schedule = null;
            }

            killCount.Clear();
            props.Clear();
            objectProps.Clear();

            Monitor.Enter(scriptLock);
            try
            {
                if (!eventCleared)
                {
                    EventManager.disposeInstance(name);
                }
            }
            finally
            {
                Monitor.Exit(scriptLock);
            }

            EventManager.getChannelServer().Container.TimerManager.schedule(() =>
            {
                mapManager.Dispose();   // issues from instantly disposing some event objects found thanks to MedicOP
                //lockObj.EnterWriteLock();
                //try
                //{
                //    mapManager = null;
                //    EventManager = null;
                //}
                //finally
                //{
                //    lockObj.ExitWriteLock();
                //}
            }, TimeSpan.FromMinutes(1));
        }
    }

    public MapManager getMapFactory()
    {
        return mapManager;
    }

    public void schedule(string methodName, long delay)
    {
        lockObj.EnterReadLock();
        try
        {
            if (ess != null)
            {
                var r = () =>
                {
                    try
                    {
                        invokeScriptFunction(methodName, this);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Invoke {JsFunction} from {ScriptName}", methodName, EventManager.getName());
                    }
                };

                ess.registerEntry(r, delay);
            }
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public string getName()
    {
        return name;
    }
    /// <summary>
    /// 和 getMapInstance 有什么区别？
    /// 没有打乱箱子？
    /// </summary>
    /// <param name="mapid"></param>
    /// <returns></returns>
    public IMap? getInstanceMap(int mapid)
    {
        if (disposed)
        {
            return null;
        }
        return mapManager.getMap(mapid);
    }
    public IMap getMapInstance(int mapId)
    {
        return mapManager.getMap(mapId);
    }

    public void setIntProperty(string key, int value)
    {
        setProperty(key, value);
    }

    public void setProperty(string key, int value)
    {
        setProperty(key, value.ToString());
    }

    public void setProperty(string key, string value)
    {
        Monitor.Enter(propertyLock);
        try
        {
            props.AddOrUpdate(key, value);
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }

    public object? setProperty(string key, string value, bool prev)
    {
        Monitor.Enter(propertyLock);
        try
        {
            return props.AddOrUpdateReturnOldValue(key, value);
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }

    public void setObjectProperty(string key, object obj)
    {
        Monitor.Enter(propertyLock);
        try
        {
            objectProps.AddOrUpdate(key, obj);
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }

    public string? getProperty(string key)
    {
        Monitor.Enter(propertyLock);
        try
        {
            var d = props.GetValueOrDefault(key);
            return d?.ToString();
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }

    public int getIntProperty(string key)
    {
        Monitor.Enter(propertyLock);
        try
        {
            var d = props.GetValueOrDefault(key);
            return Convert.ToInt32(d);
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }

    public object? getObjectProperty(string key)
    {
        Monitor.Enter(propertyLock);
        try
        {
            return objectProps.GetValueOrDefault(key);
        }
        finally
        {
            Monitor.Exit(propertyLock);
        }
    }



    public bool isLeader(Player chr)
    {
        return chr.isPartyLeader();
    }

    public bool isEventLeader(Player chr)
    {
        return (chr.getId() == getLeaderId());
    }


    public bool disposeIfPlayerBelow(byte size, int towarp)
    {
        if (disposed)
        {
            return true;
        }
        if (chars == null)
        {
            return false;
        }

        IMap? map = null;
        if (towarp > 0)
        {
            map = this.getMapFactory().getMap(towarp);
        }

        List<Player> players = getPlayerList();

        try
        {
            if (players.Count < size)
            {
                foreach (Player chr in players)
                {
                    if (chr == null)
                    {
                        continue;
                    }

                    unregisterPlayer(chr);
                    if (towarp > 0)
                    {
                        chr.changeMap(map!, map!.getPortal(0));
                    }
                }

                Dispose();
                return true;
            }
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
        }

        return false;
    }

    public void spawnNpc(int npcId, Point pos, IMap map)
    {
        var npc = LifeFactory.Instance.getNPC(npcId);
        if (npc != null)
        {
            npc.setPosition(pos);
            npc.setCy(pos.Y);
            npc.setRx0(pos.X + 50);
            npc.setRx1(pos.X - 50);
            npc.setFh(map.Footholds.FindBelowFoothold(pos).getId());
            map.addMapObject(npc);
            map.broadcastMessage(PacketCreator.spawnNPC(npc));
        }
    }

    public void dispatchRaiseQuestMobCount(int mobid, int mapid)
    {
        var mapChars = getInstanceMap(mapid)?.getMapPlayers() ?? [];
        if (mapChars.Count > 0)
        {
            List<Player> eventMembers = getPlayers();

            foreach (Player evChr in eventMembers)
            {
                var chr = mapChars.GetValueOrDefault(evChr.getId());

                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.raiseQuestMobCount(mobid);
                }
            }
        }
    }

    public Monster getMonster(int mid)
    {
        return (LifeFactory.Instance.GetMonsterTrust(mid));
    }

    private List<int> convertToIntegerList(List<object> objects)
    {
        return objects.Select(x => Convert.ToInt32(x)).ToList();
    }

    public void setEventClearStageExp(List<object> gain)
    {
        onMapClearExp.Clear();
        onMapClearExp.AddRange(convertToIntegerList(gain));
    }

    public void setEventClearStageMeso(List<object> gain)
    {
        onMapClearMeso.Clear();
        onMapClearMeso.AddRange(convertToIntegerList(gain));
    }

    int getClearStageExp(int stage)
    {
        //stage counts from ONE.
        if (stage > onMapClearExp.Count)
        {
            return 0;
        }
        return onMapClearExp.ElementAt(stage - 1);
    }

    int getClearStageMeso(int stage)
    {
        //stage counts from ONE.
        if (stage > onMapClearMeso.Count)
        {
            return 0;
        }
        return onMapClearMeso.ElementAt(stage - 1);
    }

    List<int> getClearStageBonus(int stage)
    {
        List<int> list = new();
        list.Add(getClearStageExp(stage));
        list.Add(getClearStageMeso(stage));

        return list;
    }

    private void dropExclusiveItems(Player chr)
    {
        chr.Bag.ClearPartyQuestItems();
        //AbstractPlayerInteraction api = chr.getAbstractPlayerInteraction();

        //foreach (int item in exclusiveItems)
        //{
        //    api.removeAll(item);
        //}
    }

    public void dropAllExclusiveItems()
    {
        getPlayers().ForEach(dropExclusiveItems);
    }

    public void setExclusiveItems(List<object> items)
    {
        List<int> exclusive = convertToIntegerList(items);

        lockObj.EnterWriteLock();
        try
        {
            exclusiveItems.UnionWith(exclusive);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    public void setEventRewards(List<object> rwds, List<object> qtys, int expGiven = 0)
    {
        setEventRewards(1, rwds, qtys, expGiven);
    }

    public void setEventRewards(int eventLevel, List<object> rwds, List<object> qtys, int expGiven = 0)
    {
        // fixed EXP will be rewarded at the same time the random item is given

        if (eventLevel <= 0 || eventLevel > YamlConfig.config.server.MAX_EVENT_LEVELS)
        {
            return;
        }
        eventLevel--;    //event level starts from 1

        List<int> rewardIds = convertToIntegerList(rwds);
        List<int> rewardQtys = convertToIntegerList(qtys);

        //rewardsSet and rewardsQty hold temporary values
        lockObj.EnterWriteLock();
        try
        {
            eventRewards.AddOrUpdate(eventLevel, new EventRewardsPair(rewardIds, rewardQtys, expGiven));
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    private byte getRewardListRequirements(int level)
    {
        if (level >= eventRewards.Count)
        {
            return 0;
        }

        byte rewardTypes = 0;
        var list = eventRewards.GetValueOrDefault(level)!;

        foreach (int itemId in list.Rewards)
        {
            rewardTypes |= (byte)(1 << (int)ItemConstants.getInventoryType(itemId));
        }

        return rewardTypes;
    }

    private bool hasRewardSlot(Player player, int eventLevel)
    {
        byte listReq = getRewardListRequirements(eventLevel);   //gets all types of items present in the event reward list

        //iterating over all valid inventory types
        for (sbyte type = 1; type <= 5; type++)
        {
            if ((listReq >> type) % 2 == 1 && !player.hasEmptySlot(type))
            {
                return false;
            }
        }

        return true;
    }


    //gives out EXP & a random item in a similar fashion of when clearing KPQ, LPQ, etc.
    public bool giveEventReward(Player player, int eventLevel = 1)
    {
        List<int>? rewardsSet, rewardsQty;
        int rewardExp;
        var rewardIndex = eventLevel - 1;

        lockObj.EnterReadLock();
        try
        {
            if (!CanGiveReward(player, -eventLevel))
            {
                return true;
            }


            if (rewardIndex >= eventRewards.Count)
            {
                return true;
            }

            var item = eventRewards.GetValueOrDefault(rewardIndex)!;
            rewardsSet = item.Rewards;
            rewardsQty = item.Quantity;

            rewardExp = item.Exp;
        }
        finally
        {
            lockObj.ExitReadLock();
        }

        if (rewardsSet.Count > 0)
        {
            if (!hasRewardSlot(player, rewardIndex))
            {
                return false;
            }

            var api = player.getAbstractPlayerInteraction();
            int rnd = (int)Math.Floor(Randomizer.nextDouble() * rewardsSet.Count);

            api.gainItem(rewardsSet.get(rnd), (short)rewardsQty.ElementAtOrDefault(rnd));
        }
        if (rewardExp > 0)
        {
            player.gainExp(rewardExp);
        }
        SetRewardClaimed(player, -eventLevel);
        return true;
    }


    public bool isEventCleared()
    {
        return eventCleared;
    }

    public bool isEventDisposed()
    {
        return disposed;
    }

    private bool isEventTeamLeaderOn()
    {
        return getPlayers().Any(x => x.getId() == getLeaderId());
    }

    public bool checkEventTeamLacking(bool leavingEventMap, int minPlayers)
    {
        if (eventCleared && getPlayerCount() > 1)
        {
            return false;
        }

        if (!eventCleared && leavingEventMap && !isEventTeamLeaderOn())
        {
            return true;
        }
        return getPlayerCount() < minPlayers;
    }

    public bool isExpeditionTeamLackingNow(bool leavingEventMap, int minPlayers, Player quitter)
    {
        if (eventCleared)
        {
            return leavingEventMap && getPlayerCount() <= 1;
        }
        else
        {
            // thanks Conrad for noticing expeditions don't need to have neither the leader nor meet the minimum requirement inside the event
            return getPlayerCount() <= 1;
        }
    }

    /// <summary>
    /// 有成员退出时（离开队伍，离线，死亡等），判断队伍是否仍然可以继续任务
    /// </summary>
    /// <param name="leavingEventMap">正在离开任务地图</param>
    /// <param name="minPlayers">任务要求的最少人数</param>
    /// <param name="quitter">离开者</param>
    /// <returns>true：不能继续任务</returns>
    public bool isEventTeamLackingNow(bool leavingEventMap, int minPlayers, Player quitter)
    {
        if (eventCleared)
        {
            return leavingEventMap && getPlayerCount() <= 1;
        }
        else
        {
            if (leavingEventMap && getLeaderId() == quitter.getId())
            {
                return true;
            }
            return getPlayerCount() <= minPlayers;
        }
    }

    public bool isEventTeamTogether()
    {
        lockObj.EnterReadLock();
        try
        {
            if (chars.Count <= 1)
            {
                return true;
            }

            if (chars.Values.Zip(chars.Values.Skip(1), (a, b) => a.getMapId() == b.getMapId()).Any(x => x))
                return false;
            return true;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public void warpEventTeam(int warpFrom, int warpTo)
    {
        List<Player> players = getPlayerList();

        foreach (Player chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo);
            }
        }
    }

    public void warpEventTeam(int warpTo)
    {
        List<Player> players = getPlayerList();

        foreach (Player chr in players)
        {
            chr.changeMap(warpTo);
        }
    }

    public void warpEventTeamToMapSpawnPoint(int warpFrom, int warpTo, int toSp)
    {
        List<Player> players = getPlayerList();

        foreach (Player chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo, toSp);
            }
        }
    }

    public void warpEventTeamToMapSpawnPoint(int warpTo, int toSp)
    {
        List<Player> players = getPlayerList();

        foreach (Player chr in players)
        {
            chr.changeMap(warpTo, toSp);
        }
    }

    public int getLeaderId()
    {
        lockObj.EnterReadLock();
        try
        {
            return leaderId;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public Player? getLeader()
    {
        lockObj.EnterReadLock();
        try
        {
            return chars.GetValueOrDefault(leaderId);
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public void setLeader(Player chr)
    {
        lockObj.EnterWriteLock();
        try
        {
            leaderId = chr.getId();
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    public void showWrongEffect()
    {
        var leader = getLeader();
        if (leader != null)
        {
            showWrongEffect(leader.getMapId());
        }

    }

    public void showWrongEffect(int mapId)
    {
        IMap map = getMapInstance(mapId);
        map.broadcastMessage(PacketCreator.showEffect("quest/party/wrong_kor"));
        map.broadcastMessage(PacketCreator.playSound("Party1/Failed"));
    }

    public void showClearEffect()
    {
        showClearEffect(false);
    }

    public void showClearEffect(bool hasGate)
    {
        var leader = getLeader();
        if (leader != null)
        {
            showClearEffect(hasGate, leader.getMapId());
        }
    }

    public void showClearEffect(int mapId)
    {
        showClearEffect(false, mapId);
    }

    public void showClearEffect(bool hasGate, int mapId)
    {
        showClearEffect(hasGate, mapId, "gate", 2);
    }

    public void showClearEffect(int mapId, string mapObj, int newState)
    {
        showClearEffect(true, mapId, mapObj, newState);
    }

    public void showClearEffect(bool hasGate, int mapId, string mapObj, int newState)
    {
        IMap map = getMapInstance(mapId);
        map.broadcastMessage(PacketCreator.showEffect("quest/party/clear"));
        map.broadcastMessage(PacketCreator.playSound("Party1/Clear"));
        if (hasGate)
        {
            map.broadcastMessage(PacketCreator.environmentChange(mapObj, newState));
            lockObj.EnterWriteLock();
            try
            {
                openedGates.AddOrUpdate(map.getId(), new(mapObj, newState));
            }
            finally
            {
                lockObj.ExitWriteLock();
            }
        }
    }

    public void recoverOpenedGate(Player chr, int thisMapId)
    {
        KeyValuePair<string, int>? gateData = null;

        lockObj.EnterReadLock();
        try
        {
            gateData = openedGates.GetValueOrDefault(thisMapId);
        }
        finally
        {
            lockObj.ExitReadLock();
        }

        if (gateData != null)
        {
            chr.sendPacket(PacketCreator.environmentChange(gateData.Value.Key, gateData.Value.Value));
        }
    }

    public void giveEventPlayersStageReward(int thisStage)
    {
        List<int> list = getClearStageBonus(thisStage);     // will give bonus exp & mesos to everyone in the event

        var expExtraBonus = Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PARTY_BONUS_EXP_RATE : 1;
        var players = getPlayerList();
        foreach (Player mc in players)
        {
            if (CanGiveReward(mc, thisStage))
            {
                SetRewardClaimed(mc, thisStage);
                mc.gainExp((int)(list[0] * mc.getExpRate() * expExtraBonus), true, true);
                mc.GainMeso((int)(list[1] * mc.getMesoRate()), inChat: true);
            }
        }
    }
    /// <summary>
    /// 对单个玩家发放通关奖励
    /// </summary>
    /// <param name="mc"></param>
    /// <param name="thisStage"></param>
    public void GiveEventClearReward(Player mc, int thisStage)
    {
        List<int> list = getClearStageBonus(thisStage);     // will give bonus exp & mesos to everyone in the event

        var expExtraBonus = Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PARTY_BONUS_EXP_RATE : 1;

        if (CanGiveReward(mc, thisStage))
        {
            SetRewardClaimed(mc, thisStage);
            mc.gainExp((int)(list[0] * mc.getExpRate() * expExtraBonus), true, true);
            mc.GainMeso((int)(list[1] * mc.getMesoRate()), inChat: true);
        }
    }

    public void linkToNextStage(int thisStage, string eventFamily, int thisMapId)
    {
        giveEventPlayersStageReward(thisStage);
        thisStage--;    //stages counts from ONE, scripts from ZERO
        IMap nextStage = getMapInstance(thisMapId);
        var portal = nextStage.getPortal("next00");
        portal?.setScriptName(eventFamily + thisStage);
    }

    public void linkPortalToScript(int thisStage, string portalName, string scriptName, int thisMapId)
    {
        giveEventPlayersStageReward(thisStage);
        // thisStage--;    //stages counts from ONE, scripts from ZERO
        IMap nextStage = getMapInstance(thisMapId);
        var portal = nextStage.getPortal(portalName);
        portal?.setScriptName(scriptName);
    }

    // registers a player status in an event
    public void gridInsert(Player chr, int newStatus)
    {
        lockObj.EnterWriteLock();
        try
        {
            playerGrid.AddOrUpdate(chr.getId(), newStatus);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    // unregisters a player status in an event
    public void gridRemove(Player chr)
    {
        lockObj.EnterWriteLock();
        try
        {
            playerGrid.Remove(chr.getId());
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    // checks a player status
    public int gridCheck(Player chr)
    {
        lockObj.EnterReadLock();
        try
        {
            return playerGrid.GetValueOrDefault(chr.getId(), -1);
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public int gridSize()
    {
        lockObj.EnterReadLock();
        try
        {
            return playerGrid.Count;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public void gridClear()
    {
        lockObj.EnterWriteLock();
        try
        {
            playerGrid.Clear();
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    public bool activatedAllReactorsOnMap(int mapId, int minReactorId, int maxReactorId)
    {
        return activatedAllReactorsOnMap(this.getMapInstance(mapId), minReactorId, maxReactorId);
    }

    public bool activatedAllReactorsOnMap(IMap map, int minReactorId, int maxReactorId)
    {
        if (map == null)
        {
            return true;
        }

        foreach (Reactor mr in map.getReactorsByIdRange(minReactorId, maxReactorId))
        {
            if (mr.getReactorType() != -1)
            {
                return false;
            }
        }

        return true;
    }

    public void TypedMessage(int type, string messageKey, params string[] param)
    {
        foreach (Player chr in getPlayers())
        {
            chr.TypedMessage(type, messageKey, param);
        }
    }

    public void Dialog(string key, params string[] param)
    {
        foreach (Player chr in getPlayers())
        {
            chr.Dialog(key, param);
        }
    }

    public void Yellow(string key, params string[] param)
    {
        foreach (Player chr in getPlayers())
        {
            chr.Yellow(key, param);
        }
    }

    public void Notice(string key, params string[] param)
    {
        TypedMessage(0, key, param);
    }

    public void Popup(string key, params string[] param)
    {
        TypedMessage(1, key, param);
    }

    public void Pink(string key, params string[] param)
    {
        TypedMessage(5, key, param);
    }

    public void LightBlue(string key, params string[] param)
    {
        TypedMessage(6, key, param);
    }

    public void LightBlue(Func<ClientCulture, string> action)
    {
        foreach (Player chr in getPlayers())
        {
            chr.LightBlue(action);
        }
    }

    public void TopScrolling(string key, params string[] param)
    {
        foreach (Player chr in getPlayers())
        {
            chr.TopScrolling(key, param);
        }
    }

    bool CanGiveReward(Player chr, int stage = 1)
    {
        return chars.ContainsKey(chr.Id) && !rewardedChr.GetValueOrDefault(stage, []).Contains(chr.Id);
    }

    void SetRewardClaimed(Player chr, int stage = 1)
    {
        if (rewardedChr.TryGetValue(stage, out var arr))
            arr.Add(chr.Id);
        else
            rewardedChr[stage] = new HashSet<int>() { chr.Id };
    }
}

