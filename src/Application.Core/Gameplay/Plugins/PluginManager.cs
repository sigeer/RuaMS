using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;
using server.maps;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 统一插件管理器：负责加载所有类型的插件，包括脚本插件和普通插件
    /// </summary>
    public sealed class PluginManager : IAsyncDisposable
    {
        readonly string _sourcePluginDir;      // 运维放置插件 DLL 的源目录
        readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）

        readonly ConcurrentDictionary<string, PluginContainer<IPluginServiceBase>> _pluginContainers = new();
        volatile bool _disposed = false;

        readonly WorldChannelServer _server;

        public PluginManager(WorldChannelServer server)
        {
            _server = server;
            _sourcePluginDir = AppDomain.CurrentDomain.BaseDirectory;
            _shadowCopyBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginShadowCopy");
        }

        public async Task<bool> LoadPlugin(string pluginDllName)
        {
            if (await LoadPluginInternal(pluginDllName, true))
            {
                OnPluginMounted(pluginDllName);
                return true;
            }
            return false;
        }

        void OnPluginMounted(string pluginDllName)
        {
            // 先发现监听器，确保 IPluginChannelLifeService 被注册
            OnPluginContainerChanged();

            // 然后初始化通道
            foreach (var channel in _server.Servers.Values.OfType<WorldChannel>().ToArray())
            {
                Log.Logger.Information("[{ServerName}] 初始化事件...", channel.InstanceName);
                OnChannelMounted(channel);
                Log.Logger.Information("[{ServerName}] 初始化事件（{EventCount}项）...完成", channel.InstanceName, channel.EventScriptManager.EventCount);
            }

            // 最后调用 IPluginLifeService 的 OnMounted
            foreach (var container in _pluginContainers.Values.ToList())
            {
                foreach (var listener in container.PluginServices.OfType<IPluginLifeService>())
                {
                    try
                    {
                        listener.OnMounted(_server);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "插件{PluginName}.OnMounted 失败");
                    }
                }
            }
        }

        void OnPluginUnmounted(string pluginDllName)
        {
            // 调用 IPluginLifeService 的 OnUnmounted
            foreach (var container in _pluginContainers.Values)
            {
                foreach (var listener in container.PluginServices.OfType<IPluginLifeService>())
                {
                    try
                    {
                        listener.OnUnmounted(_server);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "插件{PluginName}.OnUnmounted 初始化失败");
                    }
                }
            }

            OnPluginContainerChanged();
        }

        void OnPluginContainerChanged()
        {
            DiscoverListeners();
        }

        async Task<bool> LoadPluginInternal(string pluginDllName, bool allowMulti)
        {
            var newContainer = LoadPluginFromSource(pluginDllName);
            if (newContainer == null)
                return false;

            var pluginBaseKey = Path.GetFileNameWithoutExtension(pluginDllName);
            string pluginKey = pluginBaseKey;

            if (!allowMulti)
            {
                var exsitedKeys = _pluginContainers.Keys.ToArray();
                foreach (var item in exsitedKeys)
                {
                    await RemovePluginInternal(item);
                }
            }
            else
            {
                await RemovePluginInternal(pluginKey);
            }

            _pluginContainers[pluginKey] = newContainer;
            return true;
        }

        async Task<bool> RemovePluginInternal(string pluginName)
        {
            if (_pluginContainers.TryGetValue(pluginName, out var container))
            {
                try
                {
                    await container.DisposeAsync().ConfigureAwait(false);
                    _pluginContainers.TryRemove(pluginName, out _);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Failed to unload plugin {pluginName}: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> UnloadPlugin(string pluginName)
        {
            if (await RemovePluginInternal(pluginName))
            {
                OnPluginUnmounted(pluginName);
                return true;
            }
            return false;
        }

        public List<IPluginServiceBase> GetAllPlugins()
        {
            return _pluginContainers.Values.SelectMany(c => c.PluginServices).ToList();
        }

        public PluginContainer<IPluginServiceBase>? GetPluginContainer(string pluginName)
        {
            return _pluginContainers.GetValueOrDefault(pluginName);
        }

        public bool HasPlugin(string pluginName)
        {
            return _pluginContainers.ContainsKey(pluginName);
        }

        public int PluginCount => _pluginContainers.Count;

        private PluginContainer<IPluginServiceBase> LoadPluginFromSource(string pluginDllName)
        {
            string sourcePath = Path.Combine(_sourcePluginDir, pluginDllName);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Plugin not found: {sourcePath}");

            // 创建唯一的卷影副本目录
            string shadowDir = Path.Combine(_shadowCopyBaseDir, Guid.NewGuid().ToString());
            Directory.CreateDirectory(shadowDir);

            string shadowDllPath = Path.Combine(shadowDir, pluginDllName);
            File.Copy(sourcePath, shadowDllPath, overwrite: true);

            // 创建自定义加载上下文
            var loadContext = new PluginLoadContext(shadowDllPath);

            // 加载插件程序集
            Assembly pluginAssembly;
            try
            {
                pluginAssembly = loadContext.LoadFromAssemblyPath(shadowDllPath);
            }
            catch (Exception ex)
            {
                // 加载失败，清理目录
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException($"Failed to load plugin from {shadowDllPath}", ex);
            }

            var serviceType = typeof(IPluginServiceBase);

            var pluginServiceTypes = pluginAssembly.GetTypes()
                .Where(t => serviceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();

            if (pluginServiceTypes.Count == 0)
            {
                loadContext.Unload();
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException($"No type implementing {serviceType.Name} found in {pluginDllName}");
            }

            var services = pluginServiceTypes.Select(x => (IPluginServiceBase?)Activator.CreateInstance(x)).OfType<IPluginServiceBase>().ToList();
            return new PluginContainer<IPluginServiceBase>(services, loadContext, shadowDir);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            var containers = _pluginContainers.Values.ToList();
            _pluginContainers.Clear();

            foreach (var container in containers)
            {
                try
                {
                    await container.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Failed to dispose plugin container: {ex.Message}");
                }
            }
        }

        #region Channel Life Listeners
        private volatile ImmutableList<IPluginChannelLifeService> _channelLifeListeners = ImmutableList<IPluginChannelLifeService>.Empty;

        void DiscoverListeners()
        {
            var p1 = ImmutableList.CreateBuilder<IPluginChannelLifeService>();
            foreach (var plugin in GetAllPlugins())
            {
                if (plugin is IPluginChannelLifeService commandListener)
                    p1.Add(commandListener);
            }
            _channelLifeListeners = p1.ToImmutable();
        }

        public void OnChannelMounted(WorldChannel worldChannel)
        {
            var listeners = _channelLifeListeners;
            foreach (var listener in listeners)
            {
                try
                {
                    listener.OnChannelMounted(worldChannel);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "ChannelLifeListener.OnChannelMounted error");
                }
            }
        }

        public void OnChannelUnmounted(WorldChannel worldChannel)
        {
            var listeners = _channelLifeListeners;
            foreach (var listener in listeners)
            {
                try
                {
                    listener.OnChannelUnmounted(worldChannel);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "ChannelLifeListener.OnChannelUnmounted error");
                }
            }
        }
        #endregion

        internal void ReloadEventTemplate(WorldChannel channel)
        {
            OnChannelUnmounted(channel);
            OnChannelMounted(channel);
        }

        #region Service Invocation Helpers
        /// <summary>
        /// 遍历所有插件容器中的指定类型服务（返回快照，保证一致性）
        /// </summary>
        IEnumerable<TS> GetServices<TS>() where TS : class, IPluginServiceBase
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is TS typed)
                        yield return typed;
                }
            }
        }

        /// <summary>
        /// 同步调用服务（返回 bool）
        /// </summary>
        bool InvokeServices<TService>(Func<TService, bool> invoke, Action<Exception>? onError = null) 
            where TService : class, IPluginServiceBase
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not TService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            if (invoke(typed))
                                return true;
                        }
                    }
                    catch (InvalidOperationException) { } // Plugin is being disposed
                    catch (Exception ex) when (ex is not BusinessException)
                    {
                        onError?.Invoke(ex);
                    }
                    catch (BusinessException be)
                    {
                        // BusinessException 由调用方处理
                        throw;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 异步调用服务（返回 bool）
        /// </summary>
        async Task<bool> InvokeServicesAsync<TService>(
            Func<TService, Task<bool>> invoke, 
            IChannelClient? client = null,
            Action<Exception>? onError = null) 
            where TService : class, IPluginServiceBase
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not TService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            if (await invoke(typed))
                                return true;
                        }
                    }
                    catch (InvalidOperationException) { } // Plugin is being disposed
                    catch (BusinessException be)
                    {
                        client?.OnlinedCharacter.Pink(be.Message);
                        onError?.Invoke(be);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 异步调用服务（无返回值）
        /// </summary>
        async Task InvokeServicesAsync<TService>(
            Func<TService, Task> invoke,
            Action<Exception>? onError = null)
            where TService : class, IPluginServiceBase
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not TService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            await invoke(typed);
                            return;
                        }
                    }
                    catch (InvalidOperationException) { } // Plugin is being disposed
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 同步调用服务（无返回值）
        /// </summary>
        void InvokeServices<TService>(
            Action<TService> invoke,
            Action<Exception>? onError = null)
            where TService : class, IPluginServiceBase
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not TService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            invoke(typed);
                            return;
                        }
                    }
                    catch (InvalidOperationException) { } // Plugin is being disposed
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex);
                    }
                }
            }
        }
        #endregion

        #region Script Service Methods
        public Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            return InvokeServicesAsync<IScriptNpcService>(
                s => s.Start(c, npcId, npcObject, scriptName),
                c,
                e => Log.Logger.Error(e, "Npc script error in: {ScriptName}", scriptName));
        }

        public Task<bool> ProcessQuestConversation(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStart)
        {
            if (isStart)
            {
                return InvokeServicesAsync<IScriptQuestService>(
                    s => s.StartQuest(c, questObj, npcId),
                    c,
                    e => Log.Logger.Error(e, "Quest script error in: QuestId={QuestId}", questObj.getId()));
            }
            else
            {
                return InvokeServicesAsync<IScriptQuestService>(
                    s => s.CompleteQuest(c, questObj, npcId),
                    c,
                    e => Log.Logger.Error(e, "Quest script error in: QuestId={QuestId}", questObj.getId()));
            }
        }

        public Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            return InvokeServicesAsync<IScriptNpcService>(
                s => s.Action(c, mode, type, selection, inputText),
                e => Log.Logger.Error(e, "Npc script error more talk"));
        }

        public bool EnterPortal(IChannelClient c, Portal p)
        {
            return InvokeServices<IScriptPortalService>(
                s => s.Enter(c, p),
                e => Log.Logger.Error(e, "Portal script error in: {ScriptName}", p.getScriptName()));
        }

        public Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            return InvokeServicesAsync<IScriptItemService>(
                s => s.ItemScript(c, npcId, scriptName),
                e => Log.Logger.Error(e, "Item script error in: {ScriptName}", scriptName));
        }

        public void MapEnterScript(IChannelClient c, IMap map)
        {
            InvokeServices<IScriptMapService>(
                s => s.MapEnter(c, map),
                e => Log.Logger.Error(e, "Map script error in: {Map}(Enter)", map.Id));
        }

        public void MapFirstEnterScript(IChannelClient c, IMap map)
        {
            InvokeServices<IScriptMapService>(
                s => s.MapFirstEnter(c, map),
                e => Log.Logger.Error(e, "Map script error in: {Map}(FirstEnter)", map.Id));
        }

        internal void ReactorHit(IChannelClient c, Reactor reactor)
        {
            foreach (var container in _pluginContainers.Values)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not IScriptReactorService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            // TODO: 日后将所有handler改成async/await后再解决
                            typed.ReactorHit(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
                            return;
                        }
                    }
                    catch (InvalidOperationException) { }
                    catch (BusinessException be)
                    {
                        c.OnlinedCharacter.Pink(be.Message);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId());
                    }
                }
            }
        }

        internal void ReactorAct(IChannelClient c, Reactor reactor)
        {
            foreach (var container in _pluginContainers.Values)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not IScriptReactorService typed)
                        continue;
                    
                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            // TODO: 日后将所有handler改成async/await后再解决
                            typed.ReactorAct(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
                            return;
                        }
                    }
                    catch (InvalidOperationException) { }
                    catch (BusinessException be)
                    {
                        c.OnlinedCharacter.Pink(be.Message);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId());
                    }
                }
            }
        }
        #endregion

        #region Mob Listeners
        public void OnMobSpawned(Monster mob)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobSpawned(mob),
                e => Log.Logger.Error(e, "MobListener.OnMobSpawned error"));
        }

        public void OnMobHealed(Monster mob, int value)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobHealed(mob, value),
                e => Log.Logger.Error(e, "MobListener.OnMobHealed error"));
        }

        public void OnMobKilled(Monster mob, ICombatantObject? killer)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobKilled(mob, killer),
                e => Log.Logger.Error(e, "MobListener.OnMobKilled error"));
        }

        public void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobDamaged(mob, damage, attacker),
                e => Log.Logger.Error(e, "MobListener.OnMobDamaged error"));
        }
        #endregion

        #region Map Listeners
        public void OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            InvokeServices<IPluginMapService>(
                s => s.OnMapObjectEnterField(map, mapObject),
                e => Log.Logger.Error(e, "MapListener.OnMapObjectEnterField error"));
        }

        public void OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            InvokeServices<IPluginMapService>(
                s => s.OnMapObjectLeaveField(map, mapObject),
                e => Log.Logger.Error(e, "MapListener.OnMapObjectLeaveField error"));
        }
        #endregion
    }
}