using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;
using server.maps;
using System.Collections.Concurrent;

namespace Application.Core.Plugins
{
    public class PluginManager
    {
        private PluginLoadContext? _currentContext;

        IScriptService? _scriptService;

        // 加载指定路径的DLL
        public void LoadPlugin(string dllPath)
        {
            var context = new PluginLoadContext(dllPath);
            var assembly = context.LoadFromAssemblyPath(dllPath);

            // 查找实现 INpcConversation 的类型
            var iScriptService = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IScriptService).IsAssignableFrom(t) && !t.IsInterface);

            if (iScriptService == null)
                throw new InvalidOperationException("No valid IScriptService found");

            var scriptService = (IScriptService?)Activator.CreateInstance(iScriptService);

            // 原子替换（注意：旧实例可能正在被使用，需要处理并发）
            var oldContext = _currentContext;
            var oldNpcConversation = _scriptService;

            _currentContext = context;
            _scriptService = scriptService;

            // 卸载旧的上下文（延迟到所有引用释放后）
            if (oldContext != null)
            {
                // 关键：先释放对旧的引用，再卸载上下文
                oldNpcConversation = null;

                oldContext.Unload();
            }
        }

        public void StartNpcConversation(IChannelClient c, int npcId, int npcObjectId, string scriptName)
        {
            try
            {
                _scriptService?.Start(c, npcId, npcObjectId, scriptName);
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

        public void MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            try
            {
                _scriptService?.Action(c, mode, type, selection, inputText);
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

        public bool EnterPortal(IChannelClient c, Portal p )
        {
            try
            {
                return _scriptService?.Enter(c, p)?.Result ?? false;
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
        public void ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            try
            {
                _scriptService?.ItemScript(c, npcId, scriptName).ConfigureAwait(false).GetAwaiter().GetResult();
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
            try
            {
                _scriptService?.MapEnter(c, map).ConfigureAwait(false).GetAwaiter().GetResult();
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
            try
            {
                _scriptService?.MapFirstEnter(c, map).ConfigureAwait(false).GetAwaiter().GetResult();
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

        public void Dispose()
        {
            _currentContext?.Unload();
            _currentContext = null;
        }

        internal void ReactorHit(IChannelClient c, Reactor reactor)
        {
            try
            {
                _scriptService?.ReactorHit(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}(FirstEnter)", reactor.getMap().Id, reactor.getId());
            }
        }

        internal void ReactorAct(IChannelClient c, Reactor reactor)
        {
            try
            {
                _scriptService?.ReactorAct(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Reactor script error in: Map={Map}.Reactor={Reactor}(FirstEnter)", reactor.getMap().Id, reactor.getId());
            }
        }
    }
}
