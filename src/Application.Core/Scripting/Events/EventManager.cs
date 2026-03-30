using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Utility.Tickables;
using scripting.Event;
using server.quest;
using System.Diagnostics;

namespace Application.Core.Scripting.Events;


public class EventManager : IDisposable, ITickableTree
{
    protected ILogger log = LogFactory.GetLogger(LogType.EventManager);
    protected IEngine iv;
    protected WorldChannel cserv;


    private Dictionary<string, string> props = new Dictionary<string, string>();

    /// <summary>
    /// 事件名
    /// </summary>
    protected string _name;
    protected ScriptFile _file;


    public bool AllowReconnect { get; set; }
    public TickableStatus Status { get; private set; }
    public List<ITickable> SubTickables { get; }

    public EventManager(WorldChannel cserv, IEngine iv, ScriptFile file)
    {
        this.iv = iv;
        this.cserv = cserv;
        this._file = file;
        _name = iv.GetValue("name").ToObject<string>() ?? file.Name;

        AllowReconnect = true;

        SubTickables = [];
    }


    bool disposed = false;
    protected bool isDisposed()
    {
        return disposed;
    }

    public virtual void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        Status = TickableStatus.Remove;
        try
        {
            iv.CallFunction("cancelSchedule");
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event 脚本：{ScriptFile}，方法：{Method}", _file.FileName, "cancelSchedule");
        }

        props.Clear();
        iv.Dispose();
    }

    public long getLobbyDelay()
    {
        return YamlConfig.config.server.EVENT_LOBBY_DELAY;
    }

    public void SetAllowReconnect()
    {
        AllowReconnect = true;
    }

    public void schedule(string methodName, long delay)
    {
        schedule(methodName, null, delay);
    }

    public void schedule(string methodName, EventInstanceManager? eim, long delay)
    {
        SubTickables.Add(new EventScheduleRequest(this, methodName, eim, cserv.Node.getCurrentTime() + delay));
    }

    public void InvokeMethod(string methodName, AbstractEventInstanceManager? eim)
    {
        try
        {
            iv.CallFunction(methodName, eim);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script schedule, Script: {Script}, Method: {Method}", _name, methodName);
        }
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


    public string GetRequirementDescription(IChannelClient client)
    {
        var countRange = props["CountMin"] == props["CountMax"] ? props["CountMin"] : props["CountMin"] + " ~ " + props["CountMax"];
        var levelRange = props["LevelMin"] == props["LevelMax"] ? props["LevelMin"] : props["LevelMin"] + " ~ " + props["LevelMax"];
        return client.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_Requirement),
            countRange,
            levelRange,
            props["EventTime"]);
    }

    public void SetRequirement(int minCount, int maxCount, int minLevel, int maxLevel, int eventTime)
    {
        props["CountMin"] = minCount.ToString();
        props["CountMax"] = maxCount.ToString();
        props["LevelMin"] = minLevel.ToString();
        props["LevelMax"] = maxLevel.ToString();
        props["EventTime"] = eventTime.ToString();
    }
    public void setProperty(string key, string value)
    {
        props[key] = value;
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


    public string getName()
    {
        return _name;
    }


    public virtual List<Player> getEligibleParty(Team party)
    {
        if (party == null)
        {
            return new();
        }
        try
        {
            var result = iv.CallFunction("getEligibleParty", party.GetChannelMembers(cserv));
            var eligibleParty = result.ToObject<List<Player>>() ?? [];
            return eligibleParty;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Script: {Script}", _name);
        }

        return new();
    }

    public virtual void clearPQ(EventInstanceManager eim)
    {
        try
        {
            iv.CallFunction("clearPQ", eim);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script clearPQ, Script: {Script}", _name);
        }
    }

    public virtual void clearPQ(EventInstanceManager eim, IMap toMap)
    {
        try
        {
            iv.CallFunction("clearPQ", eim, toMap);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Event script clearPQ, Script: {Script}", _name);
        }
    }



    public void startQuest(Player chr, int id, int npcid)
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

    public void completeQuest(Player chr, int id, int npcid)
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

    public void OnTick(long now)
    {
        this.ProcessSubTickables(now);
    }

    class EventScheduleRequest : DelayedTickable
    {
        EventManager _em;
        string _methodName;
        AbstractEventInstanceManager? _eim;
        public EventScheduleRequest(EventManager em, string methodName, AbstractEventInstanceManager? eim, long next) : base(next)
        {
            _em = em;
            _methodName = methodName;
            _eim = eim;
        }

        protected override void Handle(long now)
        {
            _em.InvokeMethod(_methodName, _eim);
        }
    }
}