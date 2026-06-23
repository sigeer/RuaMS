using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;
using server.maps;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Channels;
using tools;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 统一插件管理器：负责加载所有类型的插件，包括脚本插件和普通插件
    /// </summary>
    public sealed class PluginManager : IAsyncDisposable
    {
        readonly string _sourcePluginDir;      // 运维放置插件 DLL 的源目录
        readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）

        /// <summary>
        /// Key: 插件文件名（无后缀）
        /// </summary>
        readonly ConcurrentDictionary<string, PluginContainer<PluginServiceBase>> _pluginContainers = new();
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
            if (await LoadPluginInternal(pluginDllName))
            {
                await OnPluginMounted(pluginDllName);
                return true;
            }
            return false;
        }

        async Task OnPluginMounted(string pluginDllName)
        {
            // 先发现监听器，确保 IPluginChannelLifeService 被注册
            OnPluginContainerChanged();


            // 最后调用 IPluginLifeService 的 OnMounted
            foreach (var container in _pluginContainers.Values.ToList())
            {
                foreach (var listener in container.PluginServices.ToArray())
                {
                    try
                    {
                        await listener.OnMounted();
                    }
                    catch (Exception ex)
                    {
                        container.Logger.Error(ex, "OnMounted 失败");
                    }
                }
            }
        }

        void OnPluginContainerChanged()
        {
        }

        async Task<bool> LoadPluginInternal(string pluginDllName)
        {
            var newContainer = LoadPluginFromSource(pluginDllName);
            if (newContainer == null)
                return false;

            var pluginBaseKey = Path.GetFileNameWithoutExtension(pluginDllName);
            string pluginKey = pluginBaseKey;

            await RemovePluginInternal(pluginKey);

            _pluginContainers[pluginKey] = newContainer;
            return true;
        }

        async Task<bool> RemovePluginInternal(string pluginName)
        {
            if (_pluginContainers.TryGetValue(pluginName, out var container))
            {
                try
                {
                    await container.DisposeAsync();
                    _pluginContainers.TryRemove(pluginName, out _);
                    return true;
                }
                catch (Exception ex)
                {
                    container.Logger.Error(ex, "Unmounted 失败");
                }
            }
            return false;
        }

        public async Task<bool> UnloadPlugin(string pluginName)
        {
            if (await RemovePluginInternal(pluginName))
            {
                return true;
            }
            return false;
        }

        public List<PluginServiceBase> GetAllPlugins()
        {
            return _pluginContainers.Values.SelectMany(c => c.PluginServices).ToList();
        }

        public PluginContainer<PluginServiceBase>? GetPluginContainer(string pluginName)
        {
            return _pluginContainers.GetValueOrDefault(pluginName);
        }

        public bool HasPlugin(string pluginName)
        {
            return _pluginContainers.ContainsKey(pluginName);
        }

        public int PluginCount => _pluginContainers.Count;

        private PluginContainer<PluginServiceBase> LoadPluginFromSource(string pluginDllName)
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

            var serviceType = typeof(PluginServiceBase);

            var pluginServiceTypes = pluginAssembly.GetTypes()
                .Where(t => serviceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();

            if (pluginServiceTypes.Count == 0)
            {
                loadContext.Unload();
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException($"No type implementing {serviceType.Name} found in {pluginDllName}");
            }

            var services = pluginServiceTypes.Select(x => (PluginServiceBase?)Activator.CreateInstance(x, _server, pluginDllName)).OfType<PluginServiceBase>().ToList();
            return new PluginContainer<PluginServiceBase>(services, loadContext, shadowDir);
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
                    await container.DisposeAsync();
                }
                catch (Exception ex)
                {
                    container.Logger.Error(ex, "Dispose 失败");
                }
            }
        }

        #region Service Invocation Helpers
        /// <summary>
        /// 遍历所有插件容器中的指定类型服务（返回快照，保证一致性）
        /// </summary>
        IEnumerable<TS> GetServices<TS>() where TS : PluginServiceBase
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
        /// 异步调用服务（返回 bool）
        /// </summary>
        async Task<bool> InvokeBooleanServicesAsync<TService>(
            Func<TService, Task<bool>> invoke,
            IChannelClient client,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
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
                    catch (BusinessException be)
                    {
                        await client.OnlinedCharacter.Debug(5, be.Message);
                        onError?.Invoke(container, be);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(container, ex);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 异步调用服务（无返回值）
        /// </summary>
        async Task InvokePlayerServicesAsync<TService>(
            Func<TService, Task> invoke,
            IChannelClient client,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
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
                    catch (BusinessException be)
                    {
                        await client.OnlinedCharacter.Debug(5, be.Message);
                        onError?.Invoke(container, be);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(container, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 异步调用服务（无返回值）
        /// </summary>
        async Task InvokeServicesAsync<TService>(
            Func<TService, Task> invoke,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
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
                    catch (Exception ex)
                    {
                        onError?.Invoke(container, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 同步调用服务（无返回值）
        /// </summary>
        void InvokeServices<TService>(
            Action<TService> invoke,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
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
                    catch (Exception ex)
                    {
                        onError?.Invoke(container, ex);
                    }
                }
            }
        }
        #endregion

        #region Script Service Methods
        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            if (await InvokeBooleanServicesAsync<IScriptNpcService>(
                    s => s.Start(c, npcId, npcObject, scriptName),
                    c,
                    (cd, e) =>
                    {
                        cd.Logger.Error(e, "Npc script error in: {ScriptName}", scriptName);
                    }))
            {
                return true;
            }

            await c.SendPacket(PacketCreator.getNPCTalk(npcId, 0, c.CurrentCulture.GetNpcDefaultTalk(npcId, -1), "00 00", 0, 0));
            return false;

        }

        public Task<bool> ProcessQuestConversation(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStart)
        {
            if (isStart)
            {
                return InvokeBooleanServicesAsync<IScriptQuestService>(
                    s => s.StartQuest(c, questObj, npcId),
                    c,
                    (cd, e) => cd.Logger.Error(e, "Quest startscript error in: QuestId={QuestId}", questObj.getId()));
            }
            else
            {
                return InvokeBooleanServicesAsync<IScriptQuestService>(
                    s => s.CompleteQuest(c, questObj, npcId),
                    c,
                    (cd, e) => cd.Logger.Error(e, "Quest endscript error in: QuestId={QuestId}", questObj.getId()));
            }
        }

        public Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            return InvokePlayerServicesAsync<IScriptNpcService>(
                s => s.Action(c, mode, type, selection, inputText),
                c,
                (cd, e) => cd.Logger.Error(e, "Npc script error more talk"));
        }

        public async Task<bool> EnterPortal(IChannelClient c, Portal p)
        {
            var containers = _pluginContainers.Values.ToArray();
            foreach (var container in containers)
            {
                foreach (var service in container.PluginServices)
                {
                    if (service is not IScriptPortalService typed)
                        continue;

                    try
                    {
                        using (container.Tracker.EnterRequest())
                        {
                            return await typed.Enter(c, p);
                        }
                    }
                    catch (Exception ex)
                    {
                        container.Logger.Error(ex, "Portal script error in: {ScriptName}", p.getScriptName());
                    }
                }
            }
            return false;
        }

        public Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            return InvokePlayerServicesAsync<IScriptItemService>(
                s => s.ItemScript(c, npcId, scriptName),
                c,
                (cd, e) => cd.Logger.Error(e, "Item script error in: {ScriptName}", scriptName));
        }

        public async Task MapEnterScript(IChannelClient c, IMap map)
        {
            await InvokePlayerServicesAsync<IScriptMapService>(
                s => s.MapEnter(c, map),
                c,
                (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(Enter)", map.Id));
        }

        public async Task MapFirstEnterScript(IChannelClient c, IMap map)
        {
            await InvokePlayerServicesAsync<IScriptMapService>(
                 s => s.MapFirstEnter(c, map),
                 c,
                 (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(FirstEnter)", map.Id));
        }

        internal async Task ReactorHit(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                 s => s.ReactorHit(c, reactor),
                 c,
                 (cd, e) => cd.Logger.Error(e, "ReactorHit error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorAct(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                 s => s.ReactorAct(c, reactor),
                 c,
                 (cd, e) => cd.Logger.Error(e, "ReactorAct error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorTouch(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                 s => s.ReactorTouch(c, reactor),
                 c,
                 (cd, e) => cd.Logger.Error(e, "ReactorTouch error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorUntouch(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                 s => s.ReactorUntouch(c, reactor),
                 c,
                 (cd, e) => cd.Logger.Error(e, "ReactorUntouch error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }
        #endregion

        #region Mob Listeners
        public void OnMobSpawned(Monster mob)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobSpawned(mob),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobSpawned error"));
        }

        public void OnMobHealed(Monster mob, int value)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobHealed(mob, value),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobHealed error"));
        }

        public void OnMobKilled(Monster mob, ICombatantObject? killer)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobKilled(mob, killer),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobKilled error"));
        }

        public void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker)
        {
            InvokeServices<IPluginMobService>(
                s => s.OnMobDamaged(mob, damage, attacker),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobDamaged error"));
        }
        #endregion

        #region Map Listeners
        public async Task OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            await InvokeServicesAsync<IPluginMapObjectService>(
                s => s.OnMapObjectEnterField(map, mapObject),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapObjectEnterField error"));
        }

        public async Task OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            await InvokeServicesAsync<IPluginMapObjectService>(
                s => s.OnMapObjectLeaveField(map, mapObject),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapObjectLeaveField error"));
        }

        public async Task OnMapLoad(IMap map)
        {
            await InvokeServicesAsync<IPluginMapService>(
                s => s.OnMapLoad(map),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapLoad error"));
        }

        public async Task OnMapUnload(IMap map)
        {
            await InvokeServicesAsync<IPluginMapService>(
                s => s.OnMapUnload(map),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapUnload error"));
        }
        #endregion
    }
}