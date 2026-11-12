using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using Application.Core.Plugins;
using Application.Plugin.TreasureMap;
using Application.Resources.Messages;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Globalization;

namespace ServiceTest.Games.Gameplay
{
    /// <summary>
    /// 校验「插件目录自带的文案会随插件加载/卸载自动注册」这条链路。
    ///
    /// 文案目录不是硬编码的类别枚举，而是 <see cref="PluginContainer{TService}"/> 按
    /// <c>Plugins/{插件名}/Messages</c> 注册到 <see cref="MessageStore"/>。
    /// 这里直接构造容器（不注册脚本服务），避免影响其它插件测试的脚本表。
    /// </summary>
    [NonParallelizable]
    public class PluginMessageDirectoryTests
    {
        private static string PluginName => nameof(TreasureMapService).Replace("Service", "");

        private static WorldChannelServer Node =>
            GameTestGlobal.TestServer.ServiceProvider.GetRequiredService<WorldChannelServer>();

        [Test]
        public void Container_RegistersAndRemovesPluginMessages()
        {
            var sourceDirectory = CreatePluginSourceDirectory();
            var category = Path.GetFileName(Directory.GetDirectories(Path.Combine(sourceDirectory, "i18n"))[0]);

            PluginContainer<PluginServiceBase>? container = null;
            try
            {
                container = new PluginContainer<PluginServiceBase>(
                    Node,
                    typeof(TreasureMapService).Assembly,
                    new PluginLoadContext(typeof(TreasureMapService).Assembly.Location),
                    Path.Combine(Path.GetTempPath(), "ServiceTest.PluginShadow"),
                    sourceDirectory);

                Assert.That(container.MessageDirectory, Is.Not.Null, "插件目录里的 Messages 没有被识别");

                var text = MessageStore.Get(category, "LevelTooLow", CultureInfo.GetCultureInfo("zh-CN"));
                Assert.That(text, Is.EqualTo("30级之后再来吧"));
            }
            finally
            {
                if (container != null)
                    container.DisposeAsync().AsTask().GetAwaiter().GetResult();

                TryDelete(sourceDirectory);
            }

            Assert.That(
                MessageStore.Get(category, "LevelTooLow", CultureInfo.GetCultureInfo("zh-CN")),
                Is.Null,
                "插件卸载后不应再读到它的文案");
        }

        /// <summary>
        /// 在临时目录里复刻 <c>Plugins/{插件名}/</c> 的布局，内容取自仓库里的插件源码目录。
        /// </summary>
        private static string CreatePluginSourceDirectory()
        {
            var root = Path.Combine(Path.GetTempPath(), "ruams-plugin-messages", Guid.NewGuid().ToString());
            var directory = Path.Combine(root, PluginName);
            Directory.CreateDirectory(directory);

            foreach (var file in Directory.GetFiles(FindPluginSource("i18n"), "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(directory, "i18n", Path.GetRelativePath(FindPluginSource("i18n"), file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }

            return directory;
        }

        private static string FindPluginSource(string child)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "RuaMS.slnx")))
                    return Path.Combine(directory.FullName, "src", "Application.Plugin.TreasureMap", child);

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("找不到仓库根目录（RuaMS.slnx）");
        }

        private static void TryDelete(string directory)
        {
            try
            {
                var root = Directory.GetParent(directory)?.FullName;
                if (root != null && Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // 清理失败不影响结论
            }
        }
    }
}
