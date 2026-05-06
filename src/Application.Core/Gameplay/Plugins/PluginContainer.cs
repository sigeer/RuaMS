using Application.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件包装器：包含插件实例、加载上下文、请求跟踪器
    /// </summary>
    internal sealed class PluginContainer : IAsyncDisposable
    {
        public IScriptService Instance { get; }
        public PluginLoadContext LoadContext { get; }
        public RequestTracker Tracker { get; } = new();
        public string ShadowCopyPath { get; }

        public PluginContainer(IScriptService instance, PluginLoadContext context, string shadowCopyPath)
        {
            Instance = instance;
            LoadContext = context;
            ShadowCopyPath = shadowCopyPath;
        }

        public async ValueTask DisposeAsync()
        {
            await Tracker.DisposeAsync();
            await Instance.DisposeAsync();
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
