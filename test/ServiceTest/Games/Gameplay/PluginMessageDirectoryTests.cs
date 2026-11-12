using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using Application.Resources.Messages;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Globalization;

namespace ServiceTest.Games.Gameplay
{
    /// <summary>
    /// 校验「插件文案随插件目录出现即可读」这条链路。
    ///
    /// 文案目录不由 <see cref="PluginContainer{TService}"/> 登记：<see cref="MessageStore"/>
    /// 每次查表实时枚举 <c>Plugins/{插件名}/i18n</c>，所以插件装上/卸下不需要任何额外调用。
    /// </summary>
    [NonParallelizable]
    public class PluginMessageDirectoryTests
    {
        private static readonly CultureInfo ZhCn = CultureInfo.GetCultureInfo("zh-CN");

        private static WorldChannelServer Node =>
            GameTestGlobal.TestServer.ServiceProvider.GetRequiredService<WorldChannelServer>();

        /// <summary>
        /// 运行期的插件根目录 == <see cref="PluginManager.PluginDir"/>。
        /// 在它下面放一个带文案的插件目录，<see cref="MessageStore"/> 立刻能读到。
        /// </summary>
        [Test]
        public void PluginMessages_AreReadFromPluginDirectory()
        {
            var pluginRoot = Node.PluginManager.PluginDir;
            var pluginDirectory = Path.Combine(pluginRoot, "Application.Plugin.MessageTest");
            var categoryDirectory = Path.Combine(pluginDirectory, "i18n", "MessageTestText");

            MessageStore.Initialize([Path.Combine(AppContext.BaseDirectory, "i18n")], pluginRoot, hotReload: false);

            try
            {
                Directory.CreateDirectory(categoryDirectory);
                File.WriteAllText(Path.Combine(categoryDirectory, "zh-CN.json"),
                    """{ "Hello": "你好" }""");

                MessageStore.Reset();
                MessageStore.Initialize([Path.Combine(AppContext.BaseDirectory, "i18n")], pluginRoot, hotReload: false);

                Assert.That(MessageStore.Get("MessageTestText", "Hello", ZhCn), Is.EqualTo("你好"));
            }
            finally
            {
                TryDelete(pluginDirectory);
                MessageStore.Reset();
            }

            MessageStore.Initialize([Path.Combine(AppContext.BaseDirectory, "i18n")], pluginRoot, hotReload: false);

            Assert.That(MessageStore.Get("MessageTestText", "Hello", ZhCn), Is.Null, "目录移除后不应再读到");
        }

        private static void TryDelete(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // 清理失败不影响结论
            }
        }
    }
}
