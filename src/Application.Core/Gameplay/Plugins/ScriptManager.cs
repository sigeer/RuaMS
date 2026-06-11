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
        protected override string GetPluginKey(string pluginDllName)
        {
            return "Plugin:Script";
        }

        #region Services
        private List<PluginContainer<IScriptService>> EnsureNotDisposedAndGetContainers()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");
            return containers;
        }
        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        if (await service.Start(c, npcId, npcObject, scriptName))
                            return true;
                    }
                }
                catch (Exception e)
                {
                    if (e is BusinessException)
                    {
                        c.OnlinedCharacter.Pink(e.Message);
                    }
                    Log.Logger.Error(e, "Npc script error in: {ScriptName}", scriptName);
                }
            }
            return false;
        }

        public async Task<bool> ProcessQuestConversation(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStart)
        {
            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        if (isStart)
                        {
                            if (await service.StartQuest(c, questObj, npcId))
                                return true;
                        }
                        else
                        {
                            if (await service.CompleteQuest(c, questObj, npcId))
                                return true;
                        }
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
            }
            return false;
        }

        public async Task MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        await service.Action(c, mode, type, selection, inputText);
                    }
                }
                catch (Exception e)
                {
                    if (e is BusinessException)
                    {
                        c.OnlinedCharacter.Pink(e.Message);
                    }
                    Log.Logger.Error(e, "Npc script error in: {ScriptName}");
                }
            }
        }

        public bool EnterPortal(IChannelClient c, Portal p)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");

            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        if (service.Enter(c, p))
                            return true;
                    }
                }
                catch (Exception e)
                {
                    if (e is BusinessException)
                    {
                        c.OnlinedCharacter.Pink(e.Message);
                    }
                    Log.Logger.Error(e, "Portal script error in: {ScriptName}", p.getScriptName());
                }
            }
            return false;
        }

        public async Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");

            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        await service.ItemScript(c, npcId, scriptName);
                    }
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
        }

        public void MapEnterScript(IChannelClient c, IMap map)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");

            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        service.MapEnter(c, map);
                    }
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
        }

        public void MapFirstEnterScript(IChannelClient c, IMap map)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");

            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        service.MapFirstEnter(c, map);
                    }
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
        }

        internal void ReactorHit(IChannelClient c, Reactor reactor)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptManager));

            var containers = _pluginContainers.Values.ToList();
            if (containers.Count == 0)
                throw new InvalidOperationException("No plugin loaded");

            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        service.ReactorHit(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
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
        }

        internal void ReactorAct(IChannelClient c, Reactor reactor)
        {
            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        service.ReactorAct(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
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
        }

        internal int RegisterEvents(WorldChannel channel)
        {
            int totalRegistered = 0;
            foreach (var container in EnsureNotDisposedAndGetContainers())
            {
                using var _ = container.Tracker.EnterRequest();
                try
                {
                    foreach (var service in container.PluginServices)
                    {
                        totalRegistered += service.RegisterEvents(channel);
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.ToString());
                }
            }
            return totalRegistered;
        }
        #endregion
    }
}
