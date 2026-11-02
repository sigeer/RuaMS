using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins.Services;
using Application.Core.Plugins;
using Application.Core.scripting.Events.Templates;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.item;
using Application.Core.scripting.npc;
using Application.Core.scripting.quest;
using Application.Core.Server.maps;
using Application.Core.Server.quest;
using client.inventory;
using scripting.map;
using scripting.portal;
using scripting.reactor;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 统一插件管理器：负责加载所有类型的插件，包括脚本插件和普通插件
    /// </summary>
    public sealed class PluginManager : IAsyncDisposable
    {
        public string PluginDir { get; }      // 运维放置插件 DLL 的源目录
        readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）


        volatile bool _disposed = false;

        readonly WorldChannelServer _server;

        /// <summary>
        /// 已告警过的被压制脚本（插件名 × 脚本名），避免同名脚本每次命中都重复告警
        /// </summary>
        readonly HashSet<(string PluginName, string ScriptName)> _shadowedWarnings = [];

        /// <summary>
        /// Key: 插件文件名（无后缀）
        /// </summary>
        readonly ConcurrentDictionary<string, PluginContainer<PluginServiceBase>> _pluginContainers = new();

        volatile List<IPluginServiceBase> _singletonServices = [];
        volatile List<IPluginServiceBase> _multipleServices = [];

        /// <summary>
        /// 生效脚本表：脚本种类 → 脚本名 → 生效实现（描述符自带来源插件）。
        /// 同名脚本按插件优先级裁决（数字小者优先，同优先级按插件名），被压制者输出日志。
        /// 插件加载/卸载时整体替换；正在执行的脚本持有的是旧快照里的描述符，不受影响。
        /// </summary>
        volatile Dictionary<ScriptKind, Dictionary<string, ScriptHandlerDescriptor>> _scripts = [];

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
            try
            {
                RebuildRegistrations();
                RebuildScripts();
                await SyncEventTemplatesAsync();
            }
            catch
            {
                // 事件重名等致命错误：把插件摘出去，避免留下一个半加载的容器
                _pluginContainers.TryRemove(pluginKey, out _);
                RebuildRegistrations();
                RebuildScripts();
                await newContainer.DisposeAsync();
                throw;
            }

            try
            {
                await newContainer.ServiceEntry.OnMounted();
            }
            catch (Exception ex)
            {
                newContainer.Logger.Error(ex, "OnMounted 失败");
            }
            return true;
        }

        async Task<bool> RemovePluginInternal(string pluginName)
        {
            if (_pluginContainers.TryGetValue(pluginName, out var container))
            {
                try
                {
                    // 先摘除注册并同步事件，再卸载 ALC：事件管理器持有插件类型，必须在程序集卸载前释放
                    _pluginContainers.TryRemove(pluginName, out _);
                    RebuildRegistrations();
                    RebuildScripts();
                    await SyncEventTemplatesAsync();

                    await container.DisposeAsync();
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

            if (pluginServiceTypes.Count != 1)
            {
                loadContext.Unload();
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException(
                    pluginServiceTypes.Count == 0
                    ? $"No type implementing {serviceType.Name} found in {pluginDllName}"
                    : $"每个接口仅允许一个入口服务");
            }

            var container = new PluginContainer<PluginServiceBase>(_server, pluginAssembly, loadContext, shadowDir);
            var pluginServiceImplement = (PluginServiceBase)Activator.CreateInstance(pluginServiceTypes[0], container)!;
            container.LoadServices(pluginServiceImplement);
            return container;
        }

        /// <summary>
        /// 收集所有插件的事件模板：扫描每个插件的主程序集。
        /// 事件名是全局键，**跨插件重名直接抛异常**——
        /// 静默覆盖会让另一个功能悄悄失效，而这类问题很难从日志里发现。
        /// </summary>
        public IReadOnlyList<AbstractEventTemplate> ResolveEventTemplates()
        {
            var winner = new Dictionary<string, AbstractEventTemplate>();
            var winnerPlugin = new Dictionary<string, string>();

            foreach (var container in _pluginContainers.Values
                .OrderBy(x => x.ServiceEntry.Priority)
                .ThenBy(x => x.LoadContext.PluginName))
            {
                var pluginName = container.LoadContext.PluginName;

                foreach (var template in ScriptFeatureDiscovery.CreateEventTemplates(container.PluginAssembly))
                {
                    if (winner.TryGetValue(template.Name, out _))
                    {
                        throw new BusinessFatalException(
                            $"事件重名：\"{template.Name}\" 同时由插件 {winnerPlugin[template.Name]} 与 {pluginName} 提供");
                    }

                    winner[template.Name] = template;
                    winnerPlugin[template.Name] = pluginName;
                }
            }

            return [.. winner.Values];
        }

        /// <summary>
        /// 重建生效脚本表：扫描每个插件的主程序集，收集其中声明了脚本的类型。
        /// 同名脚本按插件优先级（数字小者优先，同优先级按插件名）取唯一胜者，
        /// 被压制者输出一次日志，说明它被哪个插件覆盖。
        /// </summary>
        void RebuildScripts()
        {
            var merged = new Dictionary<ScriptKind, Dictionary<string, ScriptHandlerDescriptor>>();

            foreach (var kind in Enum.GetValues<ScriptKind>())
            {
                merged[kind] = [];
            }

            foreach (var container in _pluginContainers.Values
                .OrderBy(x => x.ServiceEntry.Priority)
                .ThenBy(x => x.LoadContext.PluginName))
            {
                var handlers = ScriptFeatureDiscovery.CollectHandlers(container.PluginAssembly, container.ServiceEntry);

                foreach (var handler in handlers)
                {
                    if (!merged.TryGetValue(handler.Kind, out var registry))
                    {
                        registry = [];
                        merged[handler.Kind] = registry;
                    }

                    if (registry.TryGetValue(handler.ScriptName, out var existing))
                    {
                        if (_shadowedWarnings.Add((container.LoadContext.PluginName, $"{handler.Kind}:{handler.ScriptName}")))
                        {
                            container.Logger.Warning(
                                "脚本 {Kind}:{Script} 已由更高优先级的插件 {Winner} 提供，本插件的同名实现未生效",
                                handler.Kind, handler.ScriptName, existing.Source.Container.LoadContext.PluginName);
                        }
                        continue;
                    }

                    registry[handler.ScriptName] = handler;
                }
            }

            _scripts = merged;
        }

        /// <summary>
        /// 取某个种类的生效脚本表。返回的是重建时替换下来的快照，调用期间不会被修改，
        /// 因此插件热更新时正在执行的脚本仍使用旧实现，新请求走新表。
        /// </summary>
        public IReadOnlyDictionary<string, ScriptHandlerDescriptor> GetScripts(ScriptKind kind)
        {
            return _scripts.TryGetValue(kind, out var registry) ? registry : EmptyScripts;
        }

        /// <summary>
        /// 取一条生效脚本；未注册返回 null。
        /// </summary>
        ScriptHandlerDescriptor? FindScript(ScriptKind kind, string? scriptName)
        {
            if (string.IsNullOrEmpty(scriptName))
                return null;

            return GetScripts(kind).GetValueOrDefault(scriptName);
        }

        static readonly Dictionary<string, ScriptHandlerDescriptor> EmptyScripts = [];

        /// <summary>
        /// 把合并后的事件模板下发到各频道。插件加载/卸载后调用。
        /// </summary>
        async Task SyncEventTemplatesAsync()
        {
            var templates = ResolveEventTemplates();
            foreach (var channel in _server.Servers.Values.OfType<WorldChannel>().ToArray())
            {
                try
                {
                    await channel.EventScriptManager.SyncEventTemplates(templates);
                }
                catch (Exception ex)
                {
                    LogFactory.GetLogger(nameof(PluginManager)).Error(ex, "同步事件模板失败：{Server}", channel.InstanceName);
                }
            }
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
        /// 返回指定服务接口类型的全部实现，按优先级排序。
        ///
        /// 返回**全部**实例而不是只取一个，由调用方决定如何使用——这很重要：
        /// <see cref="PluginInvocationMode.FirstMatch"/> 需要遍历整个列表，
        /// 才能输出「哪些插件被更高优先级的插件覆盖」的日志；在这里提前截断会让覆盖变得静默。
        /// </summary>
        IReadOnlyList<TService> GetServices<TService>()
            where TService : class, IPluginServiceBase
        {
            var mode = PluginInvocationAttribute.GetMode(typeof(TService));
            return mode switch
            {
                PluginInvocationMode.FirstMatch => _singletonServices.OfType<TService>().ToList(),
                PluginInvocationMode.Broadcast => _multipleServices.OfType<TService>().ToList(),
                _ => [],
            };
        }

        /// <summary>
        /// 插件加载/卸载后重建服务注册表。
        /// 按优先级（数字小者优先，同优先级按插件名）排序，供各调用模式按序取用。
        /// </summary>
        void RebuildRegistrations()
        {
            _singletonServices = [];
            _multipleServices = [];

            foreach (var container in _pluginContainers.Values
                .OrderBy(x => x.ServiceEntry.Priority)
                .ThenBy(x => x.LoadContext.PluginName))
            {
                // 一个容器（一个插件）在每个列表中只出现一次：
                // 服务实例可能同时实现多个同模式接口（如 ScriptService 同时是 NPC/Quest/Item 服务），
                // 若按接口逐个添加，按列表遍历的合并逻辑会把同一插件的脚本算两遍。
                var modes = container.ServiceEntry.GetType().GetInterfaces()
                    .Select(PluginInvocationAttribute.GetMode)
                    .ToHashSet();

                if (modes.Contains(PluginInvocationMode.FirstMatch))
                    _singletonServices.Add(container.ServiceEntry);

                if (modes.Contains(PluginInvocationMode.Broadcast))
                    _multipleServices.Add(container.ServiceEntry);
            }
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
            foreach (var service in GetServices<TService>())
            {
                try
                {
                    using (service.Container.Tracker.EnterRequest())
                    {
                        invoke(service);
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(service.Container, ex);
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
            foreach (var service in GetServices<TService>())
            {
                try
                {
                    using (service.Container.Tracker.EnterRequest())
                    {
                        await invoke(service);
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(service.Container, ex);
                }
            }
        }

        #endregion

        #region Script Service Methods

        /// <summary>
        /// 执行脚本方法，把非对话类异常收敛成日志。
        /// 执行期间持有 <see cref="RequestTracker"/> 的请求租约，插件卸载会等它结束，
        /// 因此正在跑的脚本不会被中途替换成新版本。
        /// </summary>
        async Task<bool> ExecuteScript(
            ScriptHandlerDescriptor script,
            object instance,
            string context,
            IChannelClient? client)
        {
            try
            {
                using (script.Source.Container.Tracker.EnterRequest())
                {
                    await (Task)script.Method.Invoke(instance, null)!;
                }
                return true;
            }
            catch (BusinessException be)
            {
                if (client != null)
                    await client.OnlinedCharacter.Debug(5, be.Message);
                LogScriptError(script.Kind, context, be);
                return false;
            }
            catch (Exception ex)
            {
                LogScriptError(script.Kind, context, ex);
                return false;
            }
        }

        static void LogScriptError(ScriptKind kind, string context, Exception ex)
        {
            LogFactory.GetLogger(nameof(PluginManager)).Error(ex, "{Kind} 脚本执行失败：{Context}", kind, context);
        }

        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            if (c.NPCConversationManager != null)
            {
                await c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            scriptName ??= $"n{npcId}";

            if (!c.OnlinedCharacter.canClickNPC())
            {
                await c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            var p = FindScript(ScriptKind.Npc, scriptName);
            if (p == null)
            {
                await c.OnlinedCharacter.Dialog(c.CurrentCulture.GetNpcDefaultTalk(npcId), npcId);
                return false;
            }

            await using var talk = (NpcScriptBase)DynamicObjectFactory.Create<IChannelClient, int, NPC?>(p.HandlerType, c, npcId, npcObject)!;
            try
            {
                if (npcObject != null && c.OnlinedCharacter.getEventInstance() != npcObject.getMap().getEventInstance())
                {
                    throw new ConversationDiffInstanceException();
                }

                c.OnlinedCharacter.setClickedNPC();
                c.NPCConversationManager = talk;
                using (p.Source.Container.Tracker.EnterRequest())
                {
                    await (Task)p.Method.Invoke(talk, null)!;
                }
                return true;
            }
            catch (ConversationInterruptException)
            {
                return true;
            }
            catch (ConversationDiffInstanceException)
            {
                if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                {
                    await talk.WarpOut();
                }

                Log.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (ConversationDiffMapException)
            {
                await talk.SayOK(talk.GetDefault0());
                Log.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (NotImplementedException)
            {
                await talk.SayOK($"NPC {npcId} 的脚本 {scriptName} 未实现。");
                Log.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return false;
            }
            catch (BusinessException be)
            {
                await c.OnlinedCharacter.Debug(5, be.Message);
                LogScriptError(ScriptKind.Npc, scriptName, be);
                return false;
            }
            catch (Exception ex)
            {
                LogScriptError(ScriptKind.Npc, scriptName, ex);
                return false;
            }
        }

        public async Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, long selection, string? inputText = null)
        {
            if (c.NPCConversationManager != null)
            {
                await c.NPCConversationManager.Response(mode, type, selection, inputText);
            }
        }

        public async Task<bool> ProcessQuestConversation(IChannelClient c, Quest questObj, int npcId, bool isStart)
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

            var p = FindScript(ScriptKind.Quest, scriptName);
            if (p == null)
            {
                await c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
                return false;
            }

            await using var talk = (QuestScriptBase)DynamicObjectFactory.Create<IChannelClient, Quest, int>(p.HandlerType, c, questObj, npcId)!;
            try
            {
                c.OnlinedCharacter.setClickedNPC();
                c.NPCConversationManager = talk;
                using (p.Source.Container.Tracker.EnterRequest())
                {
                    await (Task)p.Method.Invoke(talk, null)!;
                }
                return true;
            }
            catch (ConversationInterruptException)
            {
                return true;
            }
            catch (ConversationDiffInstanceException)
            {
                if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                {
                    await talk.WarpOut();
                }

                Log.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (ConversationDiffMapException)
            {
                await talk.SayOK(talk.GetDefault0());
                Log.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (NotImplementedException)
            {
                await talk.SayOK($"任务 {c.CurrentCulture.GetQuestName(questObj.getId()) ?? questObj.getId().ToString()} 对话未实现。");
                Log.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return false;
            }
            catch (BusinessException be)
            {
                await c.OnlinedCharacter.Debug(5, be.Message);
                LogScriptError(ScriptKind.Quest, scriptName, be);
                return false;
            }
            catch (Exception ex)
            {
                LogScriptError(ScriptKind.Quest, scriptName, ex);
                return false;
            }
        }

        public async Task<bool> EnterPortal(IChannelClient c, IPortal p)
        {
            var name = p.getScriptName();
            var script = FindScript(ScriptKind.Portal, name);
            if (script == null)
                return false;

            var interaction = (PortalPlayerInteraction)DynamicObjectFactory.Create(script.HandlerType, c, p);
            return await ExecuteScript(script, interaction, name!, c);
        }

        public async Task ItemScript(IChannelClient c, Item item, int npcId, string scriptName)
        {
            var script = FindScript(ScriptKind.Item, scriptName);
            if (script == null)
                return;

            await using var talk = (ItemScriptBase)DynamicObjectFactory.Create(script.HandlerType, c, item, npcId);
            c.NPCConversationManager = talk;
            await ExecuteScript(script, talk, scriptName, c);
        }

        public Task MapEnterScript(IChannelClient c, IMap map)
        {
            return RunMapScript(c, map, map.SourceTemplate.OnUserEnter);
        }

        public Task MapFirstEnterScript(IChannelClient c, IMap map)
        {
            return RunMapScript(c, map, map.SourceTemplate.OnFirstUserEnter);
        }

        /// <summary>
        /// 执行地图脚本。<c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用
        /// <see cref="ScriptKind.Map"/> 注册表，仅触发时机不同。
        /// </summary>
        async Task RunMapScript(IChannelClient c, IMap map, string? scriptName)
        {
            var script = FindScript(ScriptKind.Map, scriptName);
            if (script == null)
                return;

            var methods = (MapScriptMethods)DynamicObjectFactory.Create(script.HandlerType, c, map);
            await ExecuteScript(script, methods, scriptName!, c);
        }

        internal Task ReactorHit(IChannelClient c, Reactor reactor)
            => RunReactorScript(c, reactor, ScriptKind.ReactorHit, reactor.getStats().Action);

        internal Task ReactorAct(IChannelClient c, Reactor reactor)
            => RunReactorScript(c, reactor, ScriptKind.ReactorAct, reactor.getStats().Action);

        internal Task ReactorTouch(IChannelClient c, Reactor reactor)
            => RunReactorScript(c, reactor, ScriptKind.ReactorTouch, reactor.getStats()!.Action);

        internal Task ReactorUntouch(IChannelClient c, Reactor reactor)
            => RunReactorScript(c, reactor, ScriptKind.ReactorUntouch, reactor.getStats().Action);

        async Task RunReactorScript(IChannelClient c, Reactor reactor, ScriptKind kind, string? scriptName)
        {
            var script = FindScript(kind, scriptName);
            if (script == null)
                return;

            var manager = (ReactorActionManager)DynamicObjectFactory.Create(script.HandlerType, c, reactor);
            await ExecuteScript(script, manager, scriptName!, c);
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
