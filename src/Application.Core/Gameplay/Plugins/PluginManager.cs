using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.item;
using Application.Core.scripting.npc;
using Application.Core.scripting.quest;
using scripting.map;
using scripting.portal;
using scripting.reactor;
using client.inventory;
using scripting.npc;
using scripting.quest;
using server.maps;
using server.quest;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
            Func<PluginContainer<PluginServiceBase>, TService, Task<bool>> invoke,
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
                            if (await invoke(container, typed))
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
                    async (container, s) =>
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

                        if (!s.NpcScripts.TryGetValue(scriptName, out var p))
                        {
                            await c.OnlinedCharacter.Debug(5, $"不支持的脚本 {scriptName}");
                            return false;
                        }

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
                        catch (Exception)
                        {

                            throw;
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
            return await InvokeBooleanServicesAsync<IScriptQuestService>(
                async (container, s) =>
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

                    if (!s.QuestScripts.TryGetValue(scriptName, out var p))
                    {
                        // 
                        await c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
                        return false;
                    }

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
                    catch (Exception)
                    {

                        throw;
                    }
                },
                c,
                (cd, e) =>
                {
                    cd.Logger.Error(e, "Quest endscript error in: QuestId={QuestId}", questObj.getId());
                });
        }

        public async Task<bool> EnterPortal(IChannelClient c, Portal p)
        {
            return await InvokeBooleanServicesAsync<IScriptPortalService>(
                async (container, s) =>
                {
                    if (!s.PortalScripts.TryGetValue(p.getScriptName()!, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"PortalScript: {p.getScriptName()}");
                    }

                    var script = (PortalPlayerInteraction)DynamicObjectFactory.Create(entry.ObjType, c, p);
                    return await (Task<bool>)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Portal script error in: {ScriptName}", p.getScriptName()));
        }

        public Task ItemScript(IChannelClient c, Item item, int npcId, string scriptName)
        {
            return InvokePlayerServicesAsync<IScriptItemService>(
                async s =>
                {
                    if (!s.ItemScripts.TryGetValue(scriptName, out var p))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {scriptName}");
                    }

                    await using var talk = (ItemScriptBase)DynamicObjectFactory.Create(p.ObjType, c, item, npcId);
                    c.NPCConversationManager = talk;
                    await(Task)p.Method.Invoke(talk, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Item script error in: {ScriptName}", scriptName));
        }

        public async Task MapEnterScript(IChannelClient c, IMap map)
        {
            await InvokePlayerServicesAsync<IScriptMapService>(
                async s =>
                {
                    if (!s.MapEnterScripts.TryGetValue(map.SourceTemplate.OnUserEnter, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {map.SourceTemplate.OnUserEnter}");
                    }

                    var script = (MapScriptMethods)DynamicObjectFactory.Create(entry.ObjType, c, map);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(Enter)", map.Id));
        }

        public async Task MapFirstEnterScript(IChannelClient c, IMap map)
        {
            await InvokePlayerServicesAsync<IScriptMapService>(
                async s =>
                {
                    if (!s.MapFirstEnterScripts.TryGetValue(map.SourceTemplate.OnFirstUserEnter, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {map.SourceTemplate.OnFirstUserEnter}");
                    }

                    var script = (MapScriptMethods)DynamicObjectFactory.Create(entry.ObjType, c, map);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "Map script error in: {Map}(FirstEnter)", map.Id));
        }

        internal async Task ReactorHit(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                async s =>
                {
                    if (!s.ReactorHitScripts.TryGetValue(reactor.getStats().Action, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {reactor.getStats().Action}");
                    }

                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorHit error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorAct(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                async s =>
                {
                    if (!s.ReactorActScripts.TryGetValue(reactor.getStats().Action, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {reactor.getStats().Action}");
                    }

                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorAct error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorTouch(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                async s =>
                {
                    if (!s.ReactorTouchScripts.TryGetValue(reactor.getStats()!.Action, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {reactor.getStats().Action}");
                    }

                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
                c,
                (cd, e) => cd.Logger.Error(e, "ReactorTouch error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId()));
        }

        internal async Task ReactorUntouch(IChannelClient c, Reactor reactor)
        {
            await InvokePlayerServicesAsync<IScriptReactorService>(
                async s =>
                {
                    if (!s.ReactorUntouchScripts.TryGetValue(reactor.getStats().Action, out var entry))
                    {
                        throw new BusinessScriptNotFoundException($"不支持的脚本 {reactor.getStats().Action}");
                    }

                    var script = (ReactorActionManager)DynamicObjectFactory.Create(entry.ObjType, c, reactor);
                    await (Task)entry.Method.Invoke(script, null)!;
                },
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