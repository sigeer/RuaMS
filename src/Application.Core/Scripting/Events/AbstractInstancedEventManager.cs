using Application.Core.Channel;
using scripting.Event;
using server;
using System.Collections.Concurrent;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    /// <summary>
    /// 会生成副本的事件
    /// </summary>
    public abstract class AbstractInstancedEventManager : EventManager
    {
        protected ConcurrentDictionary<string, AbstractEventInstanceManager> instances = new();
        protected Dictionary<int, bool> openedLobbys = new();
        public int MaxLobbys { get; set; }
        public const int DefaultMaxLobbys = 1;


        /// <summary>
        /// 预生成的 EventInstanceManager
        /// </summary>
        private Queue<AbstractEventInstanceManager> readyInstances = new();
        private int readyId = 0;
        protected int onLoadInstances = 0;

        protected Lock lobbyLock = new ();
        protected Lock queueLock = new ();

        protected HashSet<int> playerPermit = new();
        protected SemaphoreSlim startSemaphore = new SemaphoreSlim(7);
        protected Lock startLock = new ();
        public AbstractInstancedEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
        }

        #region Instances
        protected virtual AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new EventInstanceManager(this, instanceName);
        }
        public AbstractEventInstanceManager? getInstance(string name)
        {
            return instances.GetValueOrDefault(name);
        }

        public List<AbstractEventInstanceManager> getInstances()
        {
            return instances.Values.ToList();
        }

        public virtual AbstractEventInstanceManager newInstance(string instanceName)
        {
            var ret = getReadyInstance() ?? CreateNewInstance(instanceName);

            ret.setName(instanceName);
            return ret;
        }


        protected virtual bool RegisterInstanceInternal(string instanceName, AbstractEventInstanceManager eim)
        {
            if (instances.TryAdd(instanceName, eim))
            {
                return true;
            }
            return false;
        }

        protected virtual void DisposeInstanceInternal(string name)
        {
            if (instances.TryRemove(name, out var eim))
            {
                if (eim != null)
                    UnregisterLobby(eim.LobbyId);
            }
        }

        public void disposeInstance(string name)
        {
            ess.registerEntry(() =>
            {
                DisposeInstanceInternal(name);
            }, YamlConfig.config.server.EVENT_LOBBY_DELAY * 1000);
        }

        /// <summary>
        /// C# -> js.setup -> C#.<see cref="newInstance"/> -> C#.<see cref="CreateNewInstance(string)"/>
        /// </summary>
        /// <typeparam name="TEIM"></typeparam>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="EventInstanceInProgressException"></exception>
        protected TEIM createInstance<TEIM>(string name, params object?[] args) where TEIM : AbstractEventInstanceManager
        {
            return iv.CallFunction(name, args).ToObject<TEIM>() ?? throw new EventInstanceInProgressException(name, this.getName());
        }


        protected void registerEventInstance(AbstractEventInstanceManager eim, int lobbyId)
        {
            if (!RegisterInstanceInternal(eim.getName(), eim))
                throw new EventInstanceInProgressException(eim.getName(), this.getName());
            eim.LobbyId = lobbyId;
        }

        private AbstractEventInstanceManager? getReadyInstance()
        {
            queueLock.Enter();
            try
            {
                if (readyInstances.TryDequeue(out var eim))
                    return eim;

                return null;
            }
            finally
            {
                fillEimQueue();
                queueLock.Exit();
            }
        }

        private void fillEimQueue()
        {
            ThreadManager.getInstance().newTask(instantiateQueuedInstance);
        }

        private void instantiateQueuedInstance()
        {
            int nextEventId;
            queueLock.Enter();
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
                queueLock.Exit();
            }

            var eim = CreateNewInstance("sampleName" + nextEventId);
            queueLock.Enter();
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
                queueLock.Exit();
            }

            instantiateQueuedInstance();    // keep filling the queue until reach threshold.
        }

        #endregion

        #region Lobby
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
                log.Error(ex, "Script: {Script}", _name);
                MaxLobbys = DefaultMaxLobbys;
            }
            return MaxLobbys;
        }
        protected void UnregisterLobby(int lobbyId)
        {
            lobbyLock.Enter();
            try
            {
                openedLobbys[lobbyId] = false;
            }
            finally
            {
                lobbyLock.Exit();
            }
        }

        protected bool TryRegisterLobby(int lobbyId)
        {
            lobbyLock.Enter();
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
                lobbyLock.Exit();
            }
        }

        protected int GetAvailableLobbyInstance()
        {
            int maxLobbies = getMaxLobbies();

            if (maxLobbies > 0)
            {
                for (int i = 0; i < maxLobbies; i++)
                {
                    if (TryRegisterLobby(i))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();

            var eimList = instances.Values.ToList();
            instances.Clear();

            foreach (var eim in eimList)
            {
                eim.Dispose();
            }

            openedLobbys.Clear();

            Queue<AbstractEventInstanceManager> readyEims;
            queueLock.Enter();
            try
            {
                readyEims = new(readyInstances);
                readyInstances.Clear();
            }
            finally
            {
                queueLock.Exit();
            }

            foreach (var eim in readyEims)
            {
                eim.Dispose();
            }
            onLoadInstances = 0;
        }
    }
}
