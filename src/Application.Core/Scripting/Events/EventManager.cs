using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Utility.Tickables;
using server.quest;

namespace Application.Core.Scripting.Events;


public class EventManager : IDisposable, ITickableTree
{
    protected ILogger log = LogFactory.GetLogger(LogType.EventManager);

    protected WorldChannel cserv;
    public WorldChannel ChannelServer => cserv;
    public string Name => _name;


    private Dictionary<string, string> props = new Dictionary<string, string>();

    /// <summary>
    /// 事件名
    /// </summary>
    protected string _name;

    public TickableStatus Status { get; private set; }
    public List<ITickable> SubTickables { get; }

    public EventManager(WorldChannel cserv, string name)
    {
        this.cserv = cserv;
        _name = name;

        SubTickables = [];
    }

    public virtual void Initialize()
    {

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
        props.Clear();
    }

    public long getLobbyDelay()
    {
        return YamlConfig.config.server.EVENT_LOBBY_DELAY;
    }

    public WorldChannel getChannelServer()
    {
        return cserv;
    }

    public IMap GetMap(int mapId)
    {
        var map = getChannelServer().getMapFactory().getMap(mapId);
        return map;
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

    public virtual void OnTick(long now)
    {
        this.ProcessSubTickables(now);
    }

}