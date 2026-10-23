using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ServiceTest.Games.Gameplay
{
    /// <summary>
    /// 校验「插件加载 → 服务注册 → 脚本合并」这条链路本身。
    ///
    /// 其余脚本测试都是直接 new 一个 ScriptService 来验证扫描结果，覆盖不到
    /// <see cref="PluginManager"/> 的注册与合并。这里补上：服务接口的
    /// <c>[PluginInvocation]</c> 若读不到，<c>GetServices</c> 会返回空，最终脚本表整体为空
    /// ——而单看扫描结果完全看不出问题。
    /// </summary>
    [TestFixture]
    public class PluginManagerScriptTests
    {
        [OneTimeSetUp]
        public async Task EnsurePluginLoaded()
        {
            var manager = Manager;

            // 服务器启动时加载的是 Plugins/ 目录下的副本，构建后可能是旧的。
            // 先刷新副本，保证后续（重新）加载的是刚编译出来的代码。
            Directory.CreateDirectory(manager.PluginDir);
            var target = Path.Combine(manager.PluginDir, "Application.Plugin.Script.dll");
            File.Copy(typeof(Application.Plugin.Script.ScriptService).Assembly.Location, target, overwrite: true);

            // 已加载就不再重载：强行重载会卸载其他测试正在使用的插件。
            if (manager.PluginCount == 0)
            {
                await manager.LoadPlugin("Application.Plugin.Script.dll");
            }
        }

        static PluginManager Manager =>
            GameTestGlobal.TestServer.ServiceProvider
                .GetRequiredService<WorldChannelServer>()
                .PluginManager;

        [Test]
        public void PluginIsLoaded()
        {
            Assert.That(Manager.PluginCount, Is.GreaterThan(0));
        }

        /// <summary>
        /// 每个脚本种类都应有实现。专门守住「服务注册表为空」这类静默失败。
        /// </summary>
        [Test]
        public void EveryScriptKindIsPopulated()
        {
            var empty = Enum.GetValues<ScriptKind>()
                .Where(kind => Manager.GetScripts(kind).Count == 0)
                .ToArray();

            Assert.That(empty, Is.Empty, "以下脚本种类的生效表为空：" + string.Join(", ", empty));
        }

        /// <summary>
        /// 生效脚本表应与「扫描插件程序集得到的脚本」一致。
        /// 本仓库只有一个默认脚本插件，因此不会发生跨插件覆盖。
        /// </summary>
        [Test]
        public void EffectiveScriptsMatchAssemblyScan()
        {
            var effective = Enum.GetValues<ScriptKind>().Sum(kind => Manager.GetScripts(kind).Count);

            var scanned = ScriptFeatureDiscovery
                .CollectHandlers(typeof(Application.Plugin.Script.ScriptService).Assembly, null!)
                .Select(x => (x.Kind, x.ScriptName))
                .Distinct()
                .Count();

            Assert.That(effective, Is.EqualTo(scanned));
        }

        /// <summary>
        /// 生效表里的每条描述符都要带来源插件，否则执行时拿不到 RequestTracker 租约。
        /// </summary>
        [Test]
        public void EveryEffectiveScriptHasSource()
        {
            var missing = Enum.GetValues<ScriptKind>()
                .SelectMany(kind => Manager.GetScripts(kind).Values)
                .Where(x => x.Source == null)
                .Select(x => x.ToString())
                .ToArray();

            Assert.That(missing, Is.Empty, "缺少来源插件：" + string.Join(", ", missing));
        }
    }
}
