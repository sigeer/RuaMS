using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Core.Plugins
{
    /// <summary>
    /// 插件管理器：负责加载、热更新、请求路由，支持多插件注册
    /// </summary>
    public abstract class AbstractPluginManager<TService> : IAsyncDisposable where TService : class, IPluginServiceBase
    {
        protected readonly string _sourcePluginDir;      // 运维放置插件 DLL 的源目录
        protected readonly string _shadowCopyBaseDir;    // 卷影副本根目录（例如 "ShadowCopy"）

        protected readonly ConcurrentDictionary<string, PluginContainer<TService>> _pluginContainers = new();
        protected bool _disposed = false;

        protected WorldChannelServer _server;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sourcePluginDir">源插件目录（运维可写）</param>
        /// <param name="shadowCopyBaseDir">卷影副本根目录</param>
        /// <param name="drainTimeout">排水超时</param>
        public AbstractPluginManager(WorldChannelServer server)
        {
            _server = server;

            _sourcePluginDir = AppDomain.CurrentDomain.BaseDirectory;
            _shadowCopyBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginShadowCopy");
        }


        public virtual async Task<bool> LoadPlugin(string pluginDllName)
        {
            if (await LoadPluginInternal(pluginDllName, true))
            {
                OnPluginMounted(pluginDllName);
                return true;
            }
            return false;
        }

        protected virtual void OnPluginMounted(string pluginDllName)
        {
            OnPluginContainerChanged();
        }
        protected virtual void OnPluginUnmounted(string pluginDllName)
        {
            OnPluginContainerChanged();
        }

        protected virtual void OnPluginContainerChanged()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginDllName"></param>
        /// <param name="allowMulti">是否允许注册多个dll</param>
        /// <returns></returns>
        protected async Task<bool> LoadPluginInternal(string pluginDllName, bool allowMulti)
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

        public virtual async Task<bool> UnloadPlugin(string pluginName)
        {
            if (await RemovePluginInternal(pluginName))
            {
                OnPluginUnmounted(pluginName);
                return true;
            }
            return false;
        }

        public List<TService> GetAllPlugins()
        {
            return _pluginContainers.Values.SelectMany(c => c.PluginServices).ToList();
        }

        public PluginContainer<TService>? GetPluginContainer(string pluginName)
        {
            _pluginContainers.TryGetValue(pluginName, out var container);
            return container;
        }

        public bool HasPlugin(string pluginName)
        {
            return _pluginContainers.ContainsKey(pluginName);
        }

        public int PluginCount => _pluginContainers.Count;

        /// <summary>
        /// 从源目录复制 DLL 到卷影副本，并加载
        /// </summary>
        private PluginContainer<TService> LoadPluginFromSource(string pluginDllName)
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

            var serviceType = typeof(TService);

            var pluginServiceTypes = pluginAssembly.GetTypes()
                .Where(t => serviceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();

            if (pluginServiceTypes.Count == 0)
            {
                loadContext.Unload();
                Directory.Delete(shadowDir, recursive: true);
                throw new InvalidOperationException($"No type implementing {serviceType.Name} found in {pluginDllName}");
            }

            var services = pluginServiceTypes.Select(x => (TService?)Activator.CreateInstance(x)).OfType<TService>().ToList();
            return new PluginContainer<TService>(services, loadContext, shadowDir);
        }

        /// <summary>
        /// 释放管理器：卸载所有已加载的插件
        /// </summary>
        public virtual async ValueTask DisposeAsync()
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
    }
}
