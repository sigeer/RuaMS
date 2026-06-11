using Application.Core.Plugins;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件包装器：包含插件实例、加载上下文、请求跟踪器
    /// </summary>
    public sealed class PluginContainer<TService> : IAsyncDisposable where TService : class, IPluginServiceBase
    {
        public List<TService> PluginServices { get; }
        public PluginLoadContext LoadContext { get; }
        public RequestTracker Tracker { get; } = new();
        public string ShadowCopyPath { get; }

        public PluginContainer(List<TService> pluginServices, PluginLoadContext context, string shadowCopyPath)
        {
            PluginServices = pluginServices;
            LoadContext = context;
            ShadowCopyPath = shadowCopyPath;
        }

        public async ValueTask DisposeAsync()
        {
            await Tracker.DisposeAsync();
            foreach (var item in PluginServices)
            {
                await item.DisposeAsync();
            }
            LoadContext.Unload();
            Log.Logger.Information("插件卸载完成");
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
