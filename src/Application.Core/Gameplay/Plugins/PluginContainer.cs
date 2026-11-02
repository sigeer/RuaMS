using Application.Core.Channel;
using Application.Core.Plugins;
using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件包装器：包含插件实例、加载上下文、请求跟踪器
    /// </summary>
    public sealed class PluginContainer<TService> : IAsyncDisposable where TService : PluginServiceBase
    {
        public TService ServiceEntry { private set; get; } = null!;

        /// <summary>
        /// 插件主程序集。脚本扫描以它为范围，因此不会把依赖 DLL 里的类型算进来。
        /// </summary>
        public Assembly PluginAssembly { get; }

        public PluginLoadContext LoadContext { get; }
        public RequestTracker Tracker { get; } = new();
        public string ShadowCopyPath { get; }
        public ILogger Logger { get; }
        public WorldChannelServer Node { get; }
        public PluginContainer(WorldChannelServer node, Assembly pluginAssembly, PluginLoadContext context, string shadowCopyPath)
        {
            Node = node;
            LoadContext = context;
            PluginAssembly = pluginAssembly;
            ShadowCopyPath = shadowCopyPath;
            Logger = LogFactory.GetLogger($"Plugin/{LoadContext.PluginName}");
        }

        public void LoadServices(TService pluginServices)
        {
            ServiceEntry = pluginServices;
        }

        public async ValueTask DisposeAsync()
        {
            await Tracker.DisposeAsync();
            if (ServiceEntry != null)
            {
                await ServiceEntry.DisposeAsync();
            }
            ServiceEntry = null!;
            LoadContext.Unload();
            Logger.Information("插件卸载...完成");
            // 清理卷影副本目录
            try
            {
                if (Directory.Exists(ShadowCopyPath))
                    Directory.Delete(ShadowCopyPath, recursive: true);
            }
            catch { /* 忽略清理失败 */ }
        }
    }
}
