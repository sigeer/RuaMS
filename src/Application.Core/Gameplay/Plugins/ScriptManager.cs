using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;
using server.maps;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 脚本插件
    /// </summary>
    public sealed class ScriptManager : AbstractPluginManager<IScriptService>
    {
        public ScriptManager(WorldChannelServer server) : base(server)
        {
        }

        public override async Task<bool> LoadPlugin(string pluginDllName)
        {
            if (await LoadPluginInternal(pluginDllName, false))
            {
                foreach (var channel in _server.Servers.Values.OfType<WorldChannel>().ToArray())
                {
                    Log.Logger.Information("[{ServerName}] 初始化事件...", channel.InstanceName);
                    RegisterEvents(channel);
                    Log.Logger.Information("[{ServerName}] 初始化事件（{EventCount}项）...完成", channel.InstanceName, channel.EventScriptManager.EventCount);
                }

                return true;
            }
            return false;
        }

        #region Services
        private PluginContainer<IScriptService> EnsureNotDisposedAndGetContainer()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No script plugin loaded.");
            if (containers.Count > 1)
                Log.Logger.Warning("Multiple script plugins loaded, using the first one.");

            return containers[0];
        }

        IScriptService GetScriptService(PluginContainer<IScriptService> container)
        {
            if (container.PluginServices.Count == 0)
                throw new InvalidOperationException("No script service loaded.");
            if (container.PluginServices.Count > 1)
                Log.Logger.Warning("Multiple script service loaded, using the first one.");

            return container.PluginServices[0];
        }
        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                return await GetScriptService(container).Start(c, npcId, npcObject, scriptName);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Npc script error in: {ScriptName}", scriptName);
            }
            return false;
        }

        public async Task<bool> ProcessQuestConversation(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStart)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                if (isStart)
                {
                    return await GetScriptService(container).StartQuest(c, questObj, npcId);
                }
                else
                {
                    return await GetScriptService(container).CompleteQuest(c, questObj, npcId);
                }
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Quest script error in: QuestId={QuestId}", questObj.getId());
            }
            return false;
        }

        public async Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                await GetScriptService(container).Action(c, mode, type, selection, inputText);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Npc script error more talk");
            }
        }

        public bool EnterPortal(IChannelClient c, Portal p)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                return GetScriptService(container).Enter(c, p);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Portal script error in: {ScriptName}", p.getScriptName());
            }
            return false;
        }

        public async Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                await GetScriptService(container).ItemScript(c, npcId, scriptName);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Item script error in: {ScriptName}", scriptName);
            }
        }

        public void MapEnterScript(IChannelClient c, IMap map)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                GetScriptService(container).MapEnter(c, map);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Map script error in: {Map}(Enter)", map.Id);
            }
        }

        public void MapFirstEnterScript(IChannelClient c, IMap map)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                GetScriptService(container).MapFirstEnter(c, map);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Map script error in: {Map}(FirstEnter)", map.Id);
            }
        }

        internal void ReactorHit(IChannelClient c, Reactor reactor)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                // TODO: 日后将所有handler改成async/await后再解决
                GetScriptService(container).ReactorHit(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId());
            }
        }

        internal void ReactorAct(IChannelClient c, Reactor reactor)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                // TODO: 日后将所有handler改成async/await后再解决
                GetScriptService(container).ReactorAct(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}", reactor.getMap().Id, reactor.getId());
            }
        }

        internal int RegisterEvents(WorldChannel channel)
        {
            var container = EnsureNotDisposedAndGetContainer();
            using var _ = container.Tracker.EnterRequest();
            try
            {
                return GetScriptService(container).RegisterEvents(channel);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return 0;
            }
        }
        #endregion
    }
}
