using Application.Core.Channel;
using Application.Core.Plugins;
using Application.Resources.Messages;
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

        /// <summary>本插件卷影副本的根目录（内含 <c>{插件名}/</c>），卸载时整体删除。</summary>
        public string ShadowCopyPath { get; }

        /// <summary>
        /// 插件的文案目录 <c>Plugins/{插件名}/Messages</c>。插件没有文案时为 <c>null</c>。
        /// 整个插件目录是一个整体，因此每个插件独占自己的文案目录，同名文件不会互相覆盖。
        /// </summary>
        public string? MessageDirectory { get; }
        public ILogger Logger { get; }
        public WorldChannelServer Node { get; }
        public PluginContainer(WorldChannelServer node, Assembly pluginAssembly, PluginLoadContext context, string shadowCopyPath, string sourceDirectory)
        {
            Node = node;
            LoadContext = context;
            PluginAssembly = pluginAssembly;
            ShadowCopyPath = shadowCopyPath;
            MessageDirectory = ResolveMessageDirectory(sourceDirectory);
            MessageStore.AddDirectory(MessageDirectory);
            Logger = LogFactory.GetLogger($"Plugin/{LoadContext.PluginName}");
        }

        /// <summary>
        /// 插件文案与 DLL 都在插件目录里：<c>Plugins/{插件名}/i18n/{Category}/{Culture}.json</c>。
        /// </summary>
        private static string? ResolveMessageDirectory(string sourceDirectory)
        {
            var directory = Path.Combine(sourceDirectory, "i18n");
            return Directory.Exists(directory) ? directory : null;
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

            MessageStore.RemoveDirectory(MessageDirectory);

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
