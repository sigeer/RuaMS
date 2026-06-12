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


    /// <summary>
    /// 事件名
    /// </summary>
    protected string _name;

    public TickableStatus Status { get; private set; }
    public List<ITickable> SubTickables { get; private set; }

    public EventManager(WorldChannel cserv, string name)
    {
        this.cserv = cserv;
        _name = name;

        SubTickables = [];
    }

    public virtual void Initialize()
    {

    }
    /// <summary>
    /// 会被dispose的属性都继承
    /// </summary>
    /// <param name="em"></param>
    public virtual void Inherit(EventManager em)
    {
        _name = em._name;
        disposed = em.disposed;
        Status = em.Status;
        SubTickables = new List<ITickable>(em.SubTickables);
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

    public virtual void OnTick(long now)
    {
        this.ProcessSubTickables(now);
    }

}