using Application.Core.Channel;
using Microsoft.Extensions.DependencyInjection;
using Application.Core.Gameplay.Plugins;
using Application.Core.Plugins;
using Application.Plugin.Script;

namespace ServiceTest;

/// <summary>
/// 构造一个真实的 <see cref="ScriptService"/> 实例。
///
/// 服务现在从 <see cref="PluginContainer{TService}"/> 取依赖（节点、插件名、日志器），
/// 因此测试也走同一条路径，而不是绕过它。
/// </summary>
internal static class TestScriptService
{
    public static ScriptService Create()
    {
        var node = Games.GameTestGlobal.TestServer.ServiceProvider
            .GetRequiredService<WorldChannelServer>();

        var container = new PluginContainer<PluginServiceBase>(
            node,
            typeof(ScriptService).Assembly,
            new PluginLoadContext(typeof(ScriptService).Assembly.Location),
            Path.Combine(Path.GetTempPath(), "ServiceTest.Plugin"));

        var service = new ScriptService(container);
        container.LoadServices(service);
        return service;
    }
}
