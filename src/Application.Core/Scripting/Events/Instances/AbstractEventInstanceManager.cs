using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Skills;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using Application.Utility.Tickables;
using server;
using server.life;
using server.maps;
using tools;
using ZLinq;

namespace Application.Core.scripting.Events.Instances;

public abstract class AbstractEventInstanceManager : IClientMessenger, IDisposable, ITickableTree
{
    protected ILogger log = LogFactory.GetLogger("EventInstanceManger");
    protected Dictionary<int, Player> chars = new();
    /// <summary>
    /// 每关 已领取奖励的玩家
    /// </summary>
    protected Dictionary<int, HashSet<int>> rewardedChr = new();
    private int leaderId = -1;
    private List<Monster> mobs = new();
    private Dictionary<Player, int> killCount = new();


    protected MapManager mapManager;
    private string name;
    private Dictionary<string, object> props = new();

    private long timeStarted = 0;
    private long eventTime = 0;

    public int LobbyId { get; set; } = -1;
    public InstanceStatus InstanceStatus { get; set; }


    // forces deletion of items not supposed to be held outside of the event, dealt on a player's leaving moment.
    protected HashSet<int> exclusiveItems = new();

    // registers player status on an event (null on this Map structure equals to 0)
    public Dictionary<int, int> PlayerGrid { get; } = new();

    // registers all opened gates on the event. Will help late characters to encounter next stages gates already opened
    private Dictionary<int, KeyValuePair<string, int>?> openedGates = new();


    public EventInstanceType Type { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// 已通过关卡
    /// </summary>
    public Dictionary<int, StageStatus> ClearedMaps { get; set; } = [];
    public int Level { get; set; } = 1;
    public virtual AbstractEventManager EventManager { get; }
    public string EventName => EventManager.Name;
    public WorldChannel ChannelServer => EventManager.ChannelServer;
    public AbstractEventInstanceManager(AbstractEventManager em, string instanceName)
    {
        EventManager = em;
        this.name = instanceName;

        this.mapManager = new MapManager(this, ChannelServer);

        SubTickables = [mapManager];
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public AbstractEventManager getEm() => EventManager;

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

        var bonus = Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PQ_BONUS_EXP_RATE : 1;
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
                mc.GainMeso((int)(gain * mc.getMesoRate()), GainItemShow.ShowInChat);
            }
        }
        else
        {
            foreach (Player mc in players)
            {
                if (mc.getMapId() == mapId)
                {
                    mc.GainMeso((int)(gain * mc.getMesoRate()), GainItemShow.ShowInChat);
                }
            }
        }

    }



    #region 触发事件
    /// <summary>
    /// 退出副本
    /// </summary>
    /// <param name="chr"></param>
    public void exitPlayer(Player chr)
    {
        EventManager.OnPlayerExit(this, chr);
    }


    public void unregisterPlayer(Player chr)
    {
        chars.Remove(chr.getId());
        gridRemove(chr);

        if (chr.isLoggedin())
        {
            EventManager.OnPlayerUnregister(this, chr);

            chr.setEventInstance(null);
            dropExclusiveItems(chr);
        }

        if (chars.Count == 0)
        {
            Dispose();
        }
    }


    public void changedMap(Player chr, int mapId)
    {
        EventManager.OnPlayerMapChanging(this, chr, mapId);
    }

    public void afterChangedMap(Player chr, int mapId)
    {
        EventManager.OnPlayerMapChanged(this, chr, mapId);
    }

    public void changedLeader(Player ldr)
    {
        EventManager.OnLeaderChanged(this, ldr);

        leaderId = ldr.getId();
    }

    public void monsterKilled(Monster mob, ICombatantObject? killer)
    {
        int scriptResult = 0;

        mobs.Remove(mob);

        if (InstanceStatus == InstanceStatus.InProgress)
        {
            scriptResult = 1;

            if (mobs.Count == 0)
            {
                scriptResult = 2;
            }
        }

        if (scriptResult > 0)
        {
            EventManager.OnMobKilled(this, mob, killer);

            if (scriptResult > 1)
            {
                EventManager.OnMobClear(this, mob.getMap());
            }
        }
    }

    public void friendlyKilled(Monster mob, ICombatantObject? killer)
    {
        EventManager.OnFriendlyMobKilled(this, mob, killer);
    }

    public void friendlyDamaged(Monster mob, ICombatantObject? attacker, int damage)
    {
        EventManager.OnFriendlyMobDamaged(this, mob, attacker, damage);
    }

    public void friendlyItemDrop(Monster mob)
    {
        EventManager.OnFriendlyMobDrop(this, mob);
    }

    public void playerKilled(Player chr)
    {
        EventManager.OnPlayerDied(this, chr);
    }

    public void reviveMonster(Monster mob)
    {
        EventManager.OnMobRevive(this, mob);
    }

    public bool revivePlayer(Player player)
    {
        return EventManager.OnPlayerRevive(this, player);
    }

    public void playerDisconnected(Player chr)
    {
        EventManager.OnPlayerDisconnected(this, chr);

        if (EventManager.AllowReconnect)
        {
            chr.Client.CurrentServer.EventRecallManager?.storeEventInstance(chr.Id, this);
        }
    }

    public void monsterKilled(Player chr, Monster mob)
    {
        try
        {
            //int inc = Convert.ToInt32(invokeScriptFunction("monsterValue", this, mob.getId()));

            OnMonsterValueChanged(chr, mob, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "monsterValue", EventManager.Name);
        }
    }

    protected void OnMonsterValueChanged(Player chr, Monster mob, int val)
    {
        killCount[chr] = killCount.GetValueOrDefault(chr) + val;
    }

    public void leftParty(Player chr)
    {
        EventManager.OnPlayerLeftParty(this, chr);
    }

    public void disbandParty()
    {
        EventManager.OnPartyDisband(this);
    }

    public void clearPQ()
    {
        EventManager.ClearPQ(this);
    }

    public virtual void startEvent()
    {
        InstanceStatus = InstanceStatus.InProgress;

        EventManager.AfterSeup(this);
    }

    public void setEventCleared()
    {
        InstanceStatus = InstanceStatus.Cleared;

        foreach (Player chr in getPlayers())
        {
            chr.awardQuestPoint(YamlConfig.config.server.QUEST_POINT_PER_EVENT_CLEAR);
        }

        EventManager.DisposeInstance(name);
    }
    #endregion


    public bool registerPlayer(Player chr)
    {

        if (chr == null || !chr.isLoggedinWorld() || InstanceStatus == InstanceStatus.Disposed)
        {
            return false;
        }

        if (chars.TryAdd(chr.Id, chr))
        {
            chr.setEventInstance(this);

            EventManager.OnPlayerRegister(this, chr);
            return true;
        }

        return false;
    }

    public void dropMessage(int type, string message)
    {
        foreach (Player chr in getPlayers())
        {
            chr.TypedMessage(type, message);
        }
    }

    EventInstanceTimperDismissRequest? _dismissRequest;
    public void restartEventTimer(long time)
    {
        stopEventTimer();
        startEventTimer(time);
    }

    public void startEventTimer(long time)
    {
        timeStarted = EventManager.ChannelServer.Node.getCurrentTime();

        if (time >= 0)
        {
            eventTime = time;

            foreach (Player chr in getPlayers())
            {
                chr.sendPacket(PacketCreator.getClock((int)(time / 1000)));
            }

            _dismissRequest = new EventInstanceTimperDismissRequest(this, timeStarted + time);
            SubTickables.Add(_dismissRequest);
        }
    }

    public void DismissEventTimer()
    {
        dismissEventTimer();

        EventManager.OnTimeOut(this);
    }

    private void dismissEventTimer()
    {
        foreach (Player chr in getPlayers())
        {
            chr.sendPacket(PacketCreator.removeClock());
        }


        eventTime = 0;
        timeStarted = 0;
    }

    public void stopEventTimer()
    {
        _dismissRequest?.Status = TickableStatus.Remove;

        dismissEventTimer();
    }

    public bool isTimerStarted()
    {
        return eventTime > 0 && timeStarted > 0;
    }

    public long getTimeLeft()
    {
        return eventTime - (EventManager.ChannelServer.Node.getCurrentTime() - timeStarted);
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
        return chars.Count;
    }

    public Player? getPlayerById(int id)
    {
        return chars.GetValueOrDefault(id);
    }

    public List<Player> getPlayers()
    {
        return new(chars.Values);
    }

    public List<Player> GetPlayerSortList()
    {
        return chars.Values.OrderBy(x => x.Id == leaderId ? 0 : 1).ToList();
    }

    private List<Player> getPlayerList()
    {
        return chars.Values.ToList();
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


    public virtual void Dispose()
    {
        // should not trigger any event script method after disposed
        if (InstanceStatus == InstanceStatus.Disposed)
        {
            return;
        }

        Status = TickableStatus.Remove;
        //try
        //{
        //    invokeScriptFunction("dispose", this);
        //}
        //catch (Exception ex)
        //{
        //    log.Error(ex, "Invoke {JsFunction} from {ScriptName}", "dispose", EventManager.Name);
        //}
        InstanceStatus = InstanceStatus.Disposed;
        stopEventTimer();

        foreach (Player chr in getPlayers())
        {
            exitPlayer(chr);
        }
        chars.Clear();
        mobs.Clear();
        // ess = null;

        killCount.Clear();
        props.Clear();

        EventManager.DisposeInstance(name);

        EventManager.ChannelServer.TimerManager.schedule(() =>
        {
            EventManager.ChannelServer.Send(w =>
            {
                mapManager.Dispose();
            });
        }, TimeSpan.FromMinutes(1));

    }

    public MapManager getMapFactory()
    {
        return mapManager;
    }

    public void Schedule(Action<AbstractEventInstanceManager> nextAction, long delay)
    {
        SubTickables.Add(new EventInstanceScheduleRequest(nextAction, this, EventManager.ChannelServer.Node.getCurrentTime() + delay));
    }


    public string getName()
    {
        return name;
    }
    /// <summary>
    /// 和 <see cref="getMapInstance"/> 有什么区别？
    /// 没有打乱箱子？
    /// </summary>
    /// <param name="mapid"></param>
    /// <returns></returns>
    public IMap? getInstanceMap(int mapid)
    {
        if (InstanceStatus.Disposed == InstanceStatus)
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

        props.AddOrUpdate(key, value);
    }

    public object? setProperty(string key, string value, bool prev)
    {

        return props.AddOrUpdateReturnOldValue(key, value);
    }


    public string? getProperty(string key)
    {
        var d = props.GetValueOrDefault(key);
        return d?.ToString();
    }

    public int getIntProperty(string key)
    {

        var d = props.GetValueOrDefault(key);
        return Convert.ToInt32(d);
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
        if (InstanceStatus == InstanceStatus.Disposed)
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
        map.SpawnNpc(npcId, pos);
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

    public void dropExclusiveItems(Player chr)
    {
        chr.Bag.ClearPartyQuestItems();

        foreach (var item in exclusiveItems)
        {
            chr.GainItem(item, short.MinValue);
        }
    }

    public void dropAllExclusiveItems()
    {
        getPlayers().ForEach(dropExclusiveItems);
    }


    private byte getRewardListRequirements(int level)
    {
        var rewards = EventManager.AllClearRewards;
        if (level >= rewards.Count)
        {
            return 0;
        }

        byte rewardTypes = 0;
        var list = rewards.GetValueOrDefault(level)!;

        foreach (var item in list.ItemPool)
        {
            rewardTypes |= (byte)(1 << (int)ItemConstants.getInventoryType(item.ItemId));
        }

        return rewardTypes;
    }

    public bool hasRewardSlot(Player player, int eventLevel)
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

    public bool IsEventInProgress()
    {
        return InstanceStatus == InstanceStatus.InProgress;
    }

    public bool isEventCleared()
    {
        return InstanceStatus == InstanceStatus.Cleared;
    }

    public bool isEventDisposed()
    {
        return InstanceStatus == InstanceStatus.Disposed;
    }

    private bool isEventTeamLeaderOn()
    {
        return getPlayers().Any(x => x.getId() == getLeaderId());
    }

    public bool checkEventTeamLacking(bool leavingEventMap, int minPlayers)
    {
        if (InstanceStatus == InstanceStatus.Cleared && getPlayerCount() > 1)
        {
            return false;
        }

        if (InstanceStatus != InstanceStatus.Cleared && leavingEventMap && !isEventTeamLeaderOn())
        {
            return true;
        }
        return getPlayerCount() < minPlayers;
    }


    public bool isEventTeamTogether()
    {
        if (chars.Count <= 1)
        {
            return true;
        }

        if (chars.Values.Zip(chars.Values.Skip(1), (a, b) => a.getMapId() == b.getMapId()).Any(x => x))
            return false;
        return true;
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
        return leaderId;
    }

    public Player? getLeader()
    {
        return chars.GetValueOrDefault(leaderId);
    }

    public void setLeader(Player chr)
    {
        leaderId = chr.getId();
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

            openedGates.AddOrUpdate(map.getId(), new(mapObj, newState));
        }
    }

    public void recoverOpenedGate(Player chr, int thisMapId)
    {
        if (openedGates.TryGetValue(thisMapId, out var gateData) && gateData != null)
        {
            chr.sendPacket(PacketCreator.environmentChange(gateData.Value.Key, gateData.Value.Value));
        }
    }

    ///// <summary>
    ///// 对单个玩家发放关卡奖励
    ///// </summary>
    ///// <param name="mc"></param>
    ///// <param name="thisStage"></param>
    //public void GiveStageClearReward(Player mc, int thisStage)
    //{
    //    var rewardExp = getClearStageExp(thisStage);
    //    var rewardMeso = getClearStageMeso(thisStage);

    //    var expExtraBonus = Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PARTY_BONUS_EXP_RATE : 1;

    //    if (CanGiveReward(mc, thisStage))
    //    {
    //        SetRewardClaimed(mc, thisStage);
    //        mc.gainExp((int)(rewardExp * mc.getExpRate() * expExtraBonus), true, true);
    //        mc.GainMeso((int)(rewardMeso * mc.getMesoRate()), GainItemShow.ShowInChat);
    //    }
    //}

    public ClaimRewardResult GiveClearReward(Player player, int point = 1)
    {
        return EventManager.GiveClearReward(this, player, point);
    }

    public ClaimRewardResult GiveStageClearReward(Player player, int stageMap)
    {
        return EventManager.GiveStageClearReward(this, player, stageMap);
    }

    public void GiveStageClearRewardAll(int stageMap)
    {
        EventManager.GiveStageClearRewardAll(this, stageMap);
    }

    //public void linkPortalToScript(int thisStage, string portalName, string scriptName, int thisMapId)
    //{
    //    giveEventPlayersStageReward(thisStage);
    //    // thisStage--;    //stages counts from ONE, scripts from ZERO
    //    IMap nextStage = getMapInstance(thisMapId);
    //    var portal = nextStage.getPortal(portalName);
    //    portal?.setScriptName(scriptName);
    //}

    // registers a player status in an event
    public void gridInsert(Player chr, int newStatus)
    {
        PlayerGrid.AddOrUpdate(chr.getId(), newStatus);
    }

    // unregisters a player status in an event
    public void gridRemove(Player chr)
    {
        PlayerGrid.Remove(chr.getId());
    }

    // checks a player status
    public int gridCheck(Player chr)
    {
        return PlayerGrid.GetValueOrDefault(chr.getId(), -1);
    }

    public int gridSize()
    {
        return PlayerGrid.Count;
    }

    public void gridClear()
    {
        PlayerGrid.Clear();
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
    public void Notice(string key, params string[] param) => TypedMessage(0, key, param);

    public void Popup(string key, params string[] param) => TypedMessage(1, key, param);

    public void TopScrolling(string key, params string[] param) => TypedMessage(4, key, param);

    public void Pink(string key, params string[] param) => TypedMessage(5, key, param);

    public void LightBlue(string key, params string[] param) => TypedMessage(6, key, param);

    public void Yellow(string key, params string[] param) => TypedMessage(-1, key, param);
    public void EarnTitle(string key, params string[] param) => TypedMessage(-2, key, param);
    public void Dialog(string key, params string[] param) => TypedMessage(-3, key, param);

    public void LightBlue(Func<ClientCulture, string> action)
    {
        foreach (Player chr in getPlayers())
        {
            chr.LightBlue(action);
        }
    }


    public bool CanGiveReward(Player chr, int stage = 1)
    {
        return chars.ContainsKey(chr.Id) && !rewardedChr.GetValueOrDefault(stage, []).Contains(chr.Id);
    }

    public void SetRewardClaimed(Player chr, int stage = 1)
    {
        if (rewardedChr.TryGetValue(stage, out var arr))
            arr.Add(chr.Id);
        else
            rewardedChr[stage] = new HashSet<int>() { chr.Id };
    }

    public TickableStatus Status { get; set; }
    public List<ITickable> SubTickables { get; }
    public virtual void OnTick(long now)
    {
        this.ProcessSubTickables(now);
    }


    class EventInstanceScheduleRequest : DelayedTickable
    {
        Action<AbstractEventInstanceManager> _action;
        AbstractEventInstanceManager _eim;
        public EventInstanceScheduleRequest(Action<AbstractEventInstanceManager> action, AbstractEventInstanceManager eim, long next) : base(next)
        {
            _action = action;
            _eim = eim;
        }

        protected override void Handle(long now)
        {
            _action(_eim);
        }
    }

    class EventInstanceTimperDismissRequest : DelayedTickable
    {
        AbstractEventInstanceManager _eim;
        public EventInstanceTimperDismissRequest(AbstractEventInstanceManager eim, long next) : base(next)
        {
            _eim = eim;
        }

        protected override void Handle(long now)
        {
            _eim.DismissEventTimer();
        }
    }
}

