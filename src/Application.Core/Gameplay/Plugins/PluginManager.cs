using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;
using OpenTelemetry.Trace;
using server.maps;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Application.Core.Plugins
{
    /// <summary>
    /// 插件管理器：负责加载、热更新、请求路由
    /// </summary>
    public sealed class PluginManager : IAsyncDisposable
    {
        private readonly string _sourcePluginDir;      // 运维放置插件 DLL 的源目录
        private readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）

        private volatile PluginContainer? _currentContainer;
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sourcePluginDir">源插件目录（运维可写）</param>
        /// <param name="shadowCopyBaseDir">卷影副本根目录</param>
        /// <param name="drainTimeout">排水超时</param>
        public PluginManager()
        {
            _sourcePluginDir = AppDomain.CurrentDomain.BaseDirectory;
            _shadowCopyBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginShadowCopy");
        }


        public async Task<bool> LoadPlugin(string pluginDllName)
        {
            // 1. 加载新插件（新上下文）
            var newContainer = LoadPluginFromSourceAsync(pluginDllName);
            if (newContainer == null)
                return false;

            PluginContainer? oldContainer = _currentContainer;
            _currentContainer = newContainer;

            // 2. 排水并卸载旧插件（如果存在）
            if (oldContainer != null)
            {
                try
                {
                    await oldContainer.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 记录日志，但新插件已经生效，旧插件可能泄漏
                    // 这里可以记录到 ILogger
                    Log.Logger.Error($"Failed to unload old plugin: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        #region Services
        public async Task<bool> StartNpcConversation(IChannelClient c, int npcId, NPC? npcObject, string scriptName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                return await container.Instance.Start(c, npcId, npcObject, scriptName);
            }
            catch (Exception e)
            {
                if (e is BusinessException)
                {
                    c.OnlinedCharacter.Pink(e.Message);
                }
                Log.Logger.Error(e, "Npc script error in: {ScriptName}", scriptName);
                return false;
            }
        }

        public void MoreNpcConversation(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance?.Action(c, mode, type, selection, inputText);
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

        public bool EnterPortal(IChannelClient c, Portal p)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                return container.Instance?.Enter(c, p)?.Result ?? false;
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance?.ItemScript(c, npcId, scriptName).ConfigureAwait(false).GetAwaiter().GetResult();
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance?.MapEnter(c, map).ConfigureAwait(false).GetAwaiter().GetResult();
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance?.MapFirstEnter(c, map).ConfigureAwait(false).GetAwaiter().GetResult();
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance.ReactorHit(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                container.Instance.ReactorAct(c, reactor).ConfigureAwait(false).GetAwaiter().GetResult();
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginManager));

            PluginContainer? container = _currentContainer;

            if (container == null)
                throw new InvalidOperationException("No plugin loaded");

            using var _ = container.Tracker.EnterRequest();
            try
            {
                return container.Instance?.RegisterEvents(channel) ?? 0;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
            return 0;
        }
        #endregion


        /// <summary>
        /// 从源目录复制 DLL 到卷影副本，并加载
        /// </summary>
        private PluginContainer LoadPluginFromSourceAsync(string pluginDllName)
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

            // 查找实现了 IGamePlugin 的类型
            var pluginType = pluginAssembly.GetTypes()
                .FirstOrDefault(t => typeof(IScriptService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (pluginType == null)
            {
                loadContext.Unload();
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException($"No type implementing {nameof(IScriptService)} found in {pluginDllName}");
            }

            // 创建插件实例（假设有无参构造函数）
            var pluginInstance = (IScriptService)Activator.CreateInstance(pluginType);

            // 可选：如果插件需要宿主服务，在这里注入（例如通过构造函数或属性）
            // 例如：InjectServices(pluginInstance);

            return new PluginContainer(pluginInstance, loadContext, shadowDir);
        }

        /// <summary>
        /// 释放管理器：卸载当前插件
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            var container = _currentContainer;
            _currentContainer = null;
            if (container != null)
            {
                await container.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
