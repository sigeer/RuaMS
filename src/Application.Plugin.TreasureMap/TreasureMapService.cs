using Application.Core.Gameplay.Plugins;
using Application.Core.Gameplay.Plugins.Services;
using Application.Shared.Models;

namespace Application.Plugin.TreasureMap
{
    /// <summary>
    /// 藏宝图插件：只提供少数物品与 NPC 脚本，另外在指定频道挂一个可点击的 NPC。
    ///
    /// 脚本由 <see cref="PluginManager"/> 扫描本插件程序集自动发现，
    /// 这里只需声明自己实现了哪些脚本服务接口。
    /// </summary>
    public class TreasureMapService : PluginServiceBase, IScriptItemService, IScriptNpcService
    {
        public TreasureMapService(PluginContainer<PluginServiceBase> pluginContainer) : base(pluginContainer)
        {
        }

        public override async ValueTask DisposeAsync()
        {
            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    c.RemoveScriptableNpc(1052103);
                    await c.FlushScriptableNpc();
                });
            }
        }

        public override async Task OnMounted()
        {
            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    c.RegisterScriptableNpc(new ScriptableNpc(1052103, "getTreasureMap", "藏宝图任务"));
                    await c.FlushScriptableNpc();
                });
            }
            await base.OnMounted();
        }
    }
}
