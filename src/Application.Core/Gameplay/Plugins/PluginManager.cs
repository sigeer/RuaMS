using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins.Services;
using Application.Core.Plugins;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.item;
using Application.Core.scripting.npc;
using Application.Core.scripting.quest;
using client.inventory;
using scripting.map;
using scripting.portal;
using scripting.reactor;
using server.maps;
using System.Collections.Concurrent;
using System.Reflection;
using tools;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 统一插件管理器：负责加载所有类型的插件，包括脚本插件和普通插件
    /// </summary>
    public sealed class PluginManager : IAsyncDisposable
    {
        public string PluginDir { get; }      // 运维放置插件 DLL 的源目录
        readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）

        /// <summary>
        /// Key: 插件文件名（无后缀）
        /// </summary>
        readonly ConcurrentDictionary<string, PluginContainer<PluginServiceBase>> _pluginContainers = new();
        volatile bool _disposed = false;

        readonly WorldChannelServer _server;

        /// <summary>
        /// 已告警过的被压制脚本（插件名 × 脚本名），避免同名脚本每次命中都重复告警
        /// </summary>
        readonly HashSet<(string PluginName, string ScriptName)> _shadowedWarnings = [];

        /// <summary>
        /// 服务类型注册表：服务接口类型 → 按优先级排序的实现数组（插件加载/卸载时 copy-on-write 重建，运行时 O(1) 查找）
        /// </summary>
        volatile Dictionary<Type, (PluginContainer<PluginServiceBase> Container, PluginServiceBase Service)[]> _registrations = [];

        /// <summary>
        /// 脚本所有者缓存：(服务接口类型, 脚本名) → 按优先级排序的持有者数组，避免每次调用重复扫描注册表
        /// </summary>
        readonly ConcurrentDictionary<(Type ServiceType, string ScriptName), (PluginContainer<PluginServiceBase> Container, PluginServiceBase Service)[]> _scriptOwners = [];

        public PluginManager(WorldChannelServer server)
        {
            _server = server;
            PluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            Directory.CreateDirectory(PluginDir);
            _shadowCopyBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginShadowCopy");
        }

        public async Task<bool> LoadPlugin(string pluginDllName)
        {
            if (await LoadPluginInternal(pluginDllName))
            {
                return true;
            }
            return false;
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
            RebuildRegistrations();
            foreach (var listener in newContainer.PluginServices.ToArray())
            {
                try
                {
                    await listener.OnMounted();
                }
                catch (Exception ex)
                {
                    newContainer.Logger.Error(ex, "OnMounted 失败");
                }
            }
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
                    RebuildRegistrations();
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
            string sourcePath = Path.Combine(PluginDir, pluginDllName);
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

        #region Invocation Helpers

        /// <summary>
        /// 返回指定服务接口类型的全部实现（按优先级排序的缓存注册表，O(1) 查找，无分配）
        /// </summary>
        IReadOnlyList<(PluginContainer<PluginServiceBase> Container, PluginServiceBase Service)> GetServices<TService>()
            where TService : class, IPluginServiceBase
        {
            return _registrations.TryGetValue(typeof(TService), out var entries) ? entries : [];
        }

        /// <summary>
        /// 插件加载/卸载后重建服务注册表，并清空脚本所有者缓存。
        /// 先替换注册表引用再清空缓存，避免并发下新缓存读到旧注册表。
        /// </summary>
        void RebuildRegistrations()
        {
            var dict = new Dictionary<Type, List<(PluginContainer<PluginServiceBase> Container, PluginServiceBase Service)>>();
            foreach (var container in _pluginContainers.Values)
            {
                foreach (var service in container.PluginServices)
                {
                    foreach (var iface in service.GetType().GetInterfaces())
                    {
                        if (!PluginInvocationAttribute.HasMode(iface))
                            continue;
                        if (!dict.TryGetValue(iface, out var list))
                        {
                            list = [];
                            dict[iface] = list;
                        }
                        list.Add((container, service));
                    }
                }
            }

            var sorted = dict.ToDictionary(
                kv => kv.Key,
                kv => kv.Value
                    .OrderBy(x => x.Service.Priority)
                    .ThenBy(x => x.Container.LoadContext.PluginName, StringComparer.Ordinal)
                    .ToArray());
            _registrations = sorted;
            _scriptOwners.Clear();
        }

        /// <summary>
        /// 返回持有指定脚本的全部服务（按优先级排序），结果缓存避免每次调用重复扫描注册表
        /// </summary>
        (PluginContainer<PluginServiceBase> Container, PluginServiceBase Service)[] GetScriptOwners<TService>(
            string scriptName,
            Func<TService, Dictionary<string, (Type ObjType, MethodInfo Method)>> registrySelector)
            where TService : class, IPluginServiceBase
        {
            var key = (typeof(TService), scriptName);
            return _scriptOwners.GetOrAdd(key, _ => GetServices<TService>()
                .Where(x => registrySelector((TService)(object)x.Service).ContainsKey(scriptName))
                .ToArray());
        }

        /// <summary>
        /// 校验服务接口标注的调用模式与当前调用方式一致，防止误用
        /// </summary>
        static PluginInvocationMode RequireMode<TService>(PluginInvocationMode expected)
            where TService : class, IPluginServiceBase
        {
            var mode = PluginInvocationAttribute.GetMode(typeof(TService));
            if (mode == null)
            {
                throw new InvalidOperationException($"服务接口 {typeof(TService).Name} 未标注 [PluginInvocation]，无法确定调用模式");
            }
            if (mode != expected)
            {
                throw new InvalidOperationException($"服务接口 {typeof(TService).Name} 标注为 {mode} 模式，不能以 {expected} 模式调用");
            }
            return mode.Value;
        }

        /// <summary>
        /// 广播调用（同步）：遍历所有实现逐个调用（用于 IPlugin*Service 监听类接口）
        /// </summary>
        void Broadcast<TService>(
            Action<TService> invoke,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
            where TService : class, IPluginServiceBase
        {
            RequireMode<TService>(PluginInvocationMode.Broadcast);
            foreach (var (container, service) in GetServices<TService>())
            {
                try
                {
                    using (container.Tracker.EnterRequest())
                    {
                        invoke((TService)(object)service);
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(container, ex);
                }
            }
        }

        /// <summary>
        /// 广播调用（异步）：遍历所有实现逐个调用（用于 IPlugin*Service 监听类接口）
        /// </summary>
        async Task BroadcastAsync<TService>(
            Func<TService, Task> invoke,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
            where TService : class, IPluginServiceBase
        {
            RequireMode<TService>(PluginInvocationMode.Broadcast);
            foreach (var (container, service) in GetServices<TService>())
            {
                try
                {
                    using (container.Tracker.EnterRequest())
                    {
                        await invoke((TService)(object)service);
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(container, ex);
                }
            }
        }

        /// <summary>
        /// 首次命中调用（用于 IScript*Service 脚本注册类接口）：
        /// 预解析所有持有指定脚本的插件，按优先级（数字小者优先，同优先级按插件名）选出唯一胜者执行，保证只调用一次。
        /// 被优先级压制的插件用其自身 logger 输出告警；高优先级脚本抛异常不回退到低优先级实现。
        /// 返回 true 表示脚本已由某个插件处理；false 表示没有任何插件持有该脚本。
        /// </summary>
        async Task<bool> InvokeScriptAsync<TService>(
            string scriptName,
            Func<TService, Dictionary<string, (Type ObjType, MethodInfo Method)>> registrySelector,
            Func<TService, PluginContainer<PluginServiceBase>, Task<bool>> execute,
            IChannelClient? client = null,
            Action<PluginContainer<PluginServiceBase>, Exception>? onError = null)
            where TService : class, IPluginServiceBase
        {
            RequireMode<TService>(PluginInvocationMode.FirstMatch);

            if (string.IsNullOrEmpty(scriptName))
                return false;

            var owners = GetScriptOwners(scriptName, registrySelector);

            if (owners.Length == 0)
                return false;

            var winner = owners[0];
            for (int i = 1; i < owners.Length; i++)
            {
                if (_shadowedWarnings.Add((owners[i].Container.LoadContext.PluginName, scriptName)))
                {
                    owners[i].Container.Logger.Warning("脚本 {Script} 已由更高优先级的插件 {Winner} 处理，插件 {Shadowed} 的同名脚本因优先级不足未生效",
                        scriptName, winner.Container.LoadContext.PluginName, owners[i].Container.LoadContext.PluginName);
                }
            }

            try
            {
                using (winner.Container.Tracker.EnterRequest())
                {
                    return await execute((TService)(object)winner.Service, winner.Container);
                }
            }
            catch (BusinessException be)
            {
                if (client != null)
                    await client.OnlinedCharacter.Debug(5, be.Message);
                onError?.Invoke(winner.Container, be);
                return false;
            }
            catch (Exception ex)
            {
                onError?.Invoke(winner.Container, ex);
                return false;
            }
        }
        #endregion

        #region Script Service Methods

        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            if (c.NPCConversationManager != null)
            {
                await c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (string.IsNullOrEmpty(scriptName))
            {
                scriptName = $"n{npcId}";
            }

            if (!c.OnlinedCharacter.canClickNPC())
            {
                await c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            if (await InvokeScriptAsync<IScriptNpcService>(
                scriptName,
                s => s.NpcScripts,
                async (s, container) =>
                {
                    var p = s.NpcScripts[scriptName];
                    await using var talk = (NpcScriptBase)DynamicObjectFactory.Create<IChannelClient, int, NPC?>(p.ObjType, c, npcId, npcObject)!;
                    try
                    {
                        if (npcObject != null && c.OnlinedCharacter.getEventInstance() != npcObject.getMap().getEventInstance())
                        {
                            throw new ConversationDiffInstanceException();
                        }

                        c.OnlinedCharacter.setClickedNPC();
                        c.NPCConversationManager = talk;
                        await (Task)p.Method.Invoke(talk, null)!;
                        return true;
                    }
                    catch (ConversationInterruptException)
                    {
                        // 对话中断
                        return true;
                    }
                    catch (ConversationDiffInstanceException)
                    {
                        if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                        {
                            await talk.WarpOut();
                        }

                        container.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return true;
                    }
                    catch (ConversationDiffMapException)
                    {
                        await talk.SayOK(talk.GetDefault0());
                        container.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return true;
                    }
                    catch (NotImplementedException)
                    {
                        await talk.SayOK($"NPC {npcObject?.getName() ?? npcId.ToString()} 对话未实现。");
                        container.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return false;
                    }
                },
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

        public async Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            if (c.NPCConversationManager != null)
            {
                await c.NPCConversationManager.Response(mode, type, selection, inputText);
            }
        }

        public async Task<bool> ProcessQuestConversation(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStart)
        {
            if (c.NPCConversationManager != null)
            {
                await c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (!c.OnlinedCharacter.canClickNPC())
            {
                await c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            var scriptName = isStart ? questObj.GetStartScript() : questObj.GetEndScript();
            if (string.IsNullOrEmpty(scriptName))
            {
                throw new BusinessResException($"QuestId={questObj.getId()}客户端wz中包含了startScript/endScript节点，但是服务端没有");
            }

            var handled = await InvokeScriptAsync<IScriptQuestService>(
                scriptName,
                s => s.QuestScripts,
                async (s, container) =>
                {
                    var p = s.QuestScripts[scriptName];
                    await using var talk = (QuestScriptBase)DynamicObjectFactory.Create<IChannelClient, server.quest.Quest, int>(p.ObjType, c, questObj, npcId)!;
                    try
                    {
                        c.OnlinedCharacter.setClickedNPC();
                        c.NPCConversationManager = talk;
                        await (Task)p.Method.Invoke(talk, null)!;
                        return true;
                    }
                    catch (ConversationInterruptException)
                    {
                        // 对话中断
                        return true;
                    }
                    catch (ConversationDiffInstanceException)
                    {
                        if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                        {
                            await talk.WarpOut();
                        }

                        container.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return true;
                    }
                    catch (ConversationDiffMapException)
                    {
                        await talk.SayOK(talk.GetDefault0());
                        container.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return true;
                    }
                    catch (NotImplementedException)
                    {
                        await talk.SayOK($"任务 {c.CurrentCulture.GetQuestName(questObj.getId()) ?? questObj.getId().ToString()} 对话未实现。");
                        container.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                        return false;
                    }
                },
                c,
                (cd, e) =>
                {
                    cd.Logger.Error(e, "Quest endscript error in: QuestId={QuestId}", questObj.getId());
                });

            if (!handled)
            {
                await c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
            }
            return handled;
        }

        public async Task<bool> EnterPortal(IChannelClient c, Portal p)
        {
            return await InvokeScriptAsync<IScriptPortalService>(
                p.getScriptName()!,
                s => s.PortalScripts,
                async (s, container) =>
                {
                    var entry = s.PortalScripts[p.getScriptName()!];
                    var script = (PortalPlayerInteraction)DynamicObjectFactory.Create(entry.ObjType, c, p);
                    return await (Task<bool>)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Portal script error in: {ScriptName}", p.getScriptName()));
        }

        public Task ItemScript(IChannelClient c, Item item, int npcId, string scriptName)
        {
            return InvokeScriptAsync<IScriptItemService>(
                scriptName,
                s => s.ItemScripts,
                async (s, container) =>
                {
                    var p = s.ItemScripts[scriptName];
                    await using var talk = (ItemScriptBase)DynamicObjectFactory.Create(p.ObjType, c, item, npcId);
                    c.NPCConversationManager = talk;
                    await (Task)p.Method.Invoke(talk, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Item script error in: {ScriptName}", scriptName));
        }

        public async Task MapEnterScript(IChannelClient c, IMap map)
        {
            await InvokeScriptAsync<IScriptMapService>(
                map.SourceTemplate.OnUserEnter,
                s => s.MapEnterScripts,
                async (s, container) =>
                {
                    var entry = s.MapEnterScripts[map.SourceTemplate.OnUserEnter];
                    var script = (MapScriptMethods)DynamicObjectFactory.Create(entry.ObjType, c, map);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(Enter)", map.Id));
        }

        public async Task MapFirstEnterScript(IChannelClient c, IMap map)
        {
            await InvokeScriptAsync<IScriptMapService>(
                map.SourceTemplate.OnFirstUserEnter,
                s => s.MapFirstEnterScripts,
                async (s, container) =>
                {
                    var entry = s.MapFirstEnterScripts[map.SourceTemplate.OnFirstUserEnter];
                    var script = (MapScriptMethods)DynamicObjectFactory.Create(entry.ObjType, c, map);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(FirstEnter)", map.Id));
        }

        internal async Task ReactorHit(IChannelClient c, Reactor reactor)
        {
            await InvokeScriptAsync<IScriptReactorService>(
                reactor.getStats().Action,
                s => s.ReactorHitScripts,
                async (s, container) =>
                {
                    var entry = s.ReactorHitScripts[reactor.getStats().Action];
                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorHit error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorAct(IChannelClient c, Reactor reactor)
        {
            await InvokeScriptAsync<IScriptReactorService>(
                reactor.getStats().Action,
                s => s.ReactorActScripts,
                async (s, container) =>
                {
                    var entry = s.ReactorActScripts[reactor.getStats().Action];
                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorAct error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorTouch(IChannelClient c, Reactor reactor)
        {
            await InvokeScriptAsync<IScriptReactorService>(
                reactor.getStats()!.Action,
                s => s.ReactorTouchScripts,
                async (s, container) =>
                {
                    var entry = s.ReactorTouchScripts[reactor.getStats()!.Action];
                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorTouch error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorUntouch(IChannelClient c, Reactor reactor)
        {
            await InvokeScriptAsync<IScriptReactorService>(
                reactor.getStats().Action,
                s => s.ReactorUntouchScripts,
                async (s, container) =>
                {
                    var entry = s.ReactorUntouchScripts[reactor.getStats().Action];
                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                    return true;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorUntouch error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }
        #endregion

        #region Mob Listeners
        public void OnMobSpawned(Monster mob)
        {
            Broadcast<IPluginMobService>(
                s => s.OnMobSpawned(mob),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobSpawned error"));
        }

        public void OnMobHealed(Monster mob, int value)
        {
            Broadcast<IPluginMobService>(
                s => s.OnMobHealed(mob, value),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobHealed error"));
        }

        public void OnMobKilled(Monster mob, ICombatantObject? killer)
        {
            Broadcast<IPluginMobService>(
                s => s.OnMobKilled(mob, killer),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobKilled error"));
        }

        public void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker)
        {
            Broadcast<IPluginMobService>(
                s => s.OnMobDamaged(mob, damage, attacker),
                (cd, e) => cd.Logger.Error(e, "MobListener.OnMobDamaged error"));
        }
        #endregion

        #region Map Listeners
        public async Task OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            await BroadcastAsync<IPluginMapObjectService>(
                s => s.OnMapObjectEnterField(map, mapObject),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapObjectEnterField error"));
        }

        public async Task OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            await BroadcastAsync<IPluginMapObjectService>(
                s => s.OnMapObjectLeaveField(map, mapObject),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapObjectLeaveField error"));
        }

        public async Task OnMapLoad(IMap map)
        {
            await BroadcastAsync<IPluginMapService>(
                s => s.OnMapLoad(map),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapLoad error"));
        }

        public async Task OnMapUnload(IMap map)
        {
            await BroadcastAsync<IPluginMapService>(
                s => s.OnMapUnload(map),
                (cd, e) => cd.Logger.Error(e, "MapListener.OnMapUnload error"));
        }
        #endregion
    }
}
