using Application.Plugin.Script;
using Application.Resources.Messages;
using System.Globalization;
using System.Reflection;

namespace ServiceTest.Infrastructure.I18n
{
    /// <summary>
    /// 消息表加载测试。MessageStore 现在只读磁盘，测试直接用仓库里的 Messages 目录。
    /// </summary>
    [NonParallelizable]
    public class MessageStoreTests
    {
        private static readonly CultureInfo ZhCn = CultureInfo.GetCultureInfo("zh-CN");
        private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");

        /// <summary>仓库根目录（从测试输出目录往上找 RuaMS.slnx）。</summary>
        private static readonly string RepoRoot = FindRepoRoot();
        private static readonly string HostMessageDir = Path.Combine(RepoRoot, "src", "Application.Resources", "i18n");

        /// <summary>ScriptMessage 由 Application.Plugin.Script 提供，测试里当作插件目录注册。</summary>
        private static readonly string ScriptPluginMessageDir = Path.Combine(RepoRoot, "src", "Application.Plugin.Script", "i18n");
        private static readonly string PluginMessageDir = Path.Combine(RepoRoot, "src", "Application.Plugin.TreasureMap", "i18n");

        [SetUp]
        public void SetUp()
        {
            MessageStore.Reset();
            MessageStore.Initialize(HostMessageDir, hotReload: false);
            MessageStore.AddDirectory(ScriptPluginMessageDir);
        }

        [TearDown]
        public void TearDown() => MessageStore.Reset();

        private static string FindRepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "RuaMS.slnx")))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("找不到仓库根目录（RuaMS.slnx）");
        }

        [Test]
        public void ScriptMessage_Multiline_UsesCrLf()
        {
            // 客户端只把 0x0D 当换行，见 docs/ida/CTextAnalyzer_文本换行_分析.md
            var text = ClientMessage.Get(ClientMessage.PartyQuest_Requirement, ZhCn);

            Assert.That(text, Is.Not.Null);
            Assert.That(text, Does.Contain("\r\n"));
            Assert.That(text, Does.Not.Contain("\n\n"));

            for (var i = 0; i < text!.Length; i++)
            {
                if (text[i] != '\n')
                    continue;

                Assert.That(i > 0 && text[i - 1] == '\r', Is.True, "存在裸 LF（0x0A），客户端不会换行");
            }
        }

        [Test]
        public void ScriptMessage_FormerLiteralEscapes_AreRealCrLf()
        {
            var text = ScriptMessage.Get(ScriptMessage.HenesysPQ_TutorialTalk0, ZhCn);

            Assert.That(text, Is.Not.Null);
            Assert.That(text, Does.Not.Contain("\\r"));
            Assert.That(text, Does.Not.Contain("\\n"));
            Assert.That(text, Does.Contain("\r\n"));
        }

        [Test]
        public void Constants_MatchKeyNames()
        {
            AssertConstsMatchKeys(typeof(ClientMessage));
            AssertConstsMatchKeys(typeof(ScriptMessage));
        }

        private static void AssertConstsMatchKeys(Type type)
        {
            var consts = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.IsLiteral && x.FieldType == typeof(string))
                .ToList();

            Assert.That(consts, Is.Not.Empty, $"{type.Name} 没有生成任何常量");

            var mismatched = consts.Where(x => (string?)x.GetRawConstantValue() != x.Name).Select(x => x.Name).ToList();
            Assert.That(mismatched, Is.Empty, $"{type.Name} 常量值与键名不一致: {string.Join(", ", mismatched)}");
        }

        [Test]
        public void AllLanguages_ShareSameKeys()
        {
            AssertLanguageKeysAligned(MessageCategories.ClientMessage);
            AssertLanguageKeysAligned(ScriptMessageCategories.ScriptMessage);
        }

        private static void AssertLanguageKeysAligned(string category)
        {
            var zh = MessageStore.GetTable(category, "zh-CN");
            var en = MessageStore.GetTable(category, "en-US");

            Assert.That(zh, Is.Not.Null, $"{category}/zh-CN.json 未加载");
            Assert.That(en, Is.Not.Null, $"{category}/en-US.json 未加载");
            Assert.That(en!.Keys.OrderBy(x => x), Is.EquivalentTo(zh!.Keys.OrderBy(x => x)));
        }

        [Test]
        public void EveryGeneratedConstant_ExistsInBothLanguages()
        {
            AssertConstantsResolvable(typeof(ClientMessage), MessageCategories.ClientMessage);
            AssertConstantsResolvable(typeof(ScriptMessage), ScriptMessageCategories.ScriptMessage);
        }

        private static void AssertConstantsResolvable(Type type, string category)
        {
            var missing = new List<string>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.IsLiteral))
            {
                var key = (string)field.GetRawConstantValue()!;
                if (string.IsNullOrEmpty(key))
                    continue;

                if (MessageStore.Get(category, key, ZhCn) == null)
                    missing.Add($"{type.Name}.{field.Name}(zh-CN)");
                if (MessageStore.Get(category, key, EnUs) == null)
                    missing.Add($"{type.Name}.{field.Name}(en-US)");
            }

            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test]
        public void UnknownKey_ReturnsNull()
        {
            Assert.That(MessageStore.Get(MessageCategories.ClientMessage, "__not_a_key__", ZhCn), Is.Null);
            Assert.That(MessageStore.Format(MessageCategories.ClientMessage, "__not_a_key__", ZhCn), Is.EqualTo("__not_a_key__"));
        }

        [Test]
        public void UnknownCulture_FallsBackToDefault()
        {
            var fromFallback = MessageStore.Get(MessageCategories.ClientMessage, ClientMessage.BanReason, CultureInfo.GetCultureInfo("ja-JP"));
            var fromDefault = MessageStore.Get(MessageCategories.ClientMessage, ClientMessage.BanReason, ZhCn);

            Assert.That(fromFallback, Is.EqualTo(fromDefault));
        }

        [Test]
        public void Format_ReplacesPlaceholders()
        {
            var text = MessageStore.Format(MessageCategories.ClientMessage, ClientMessage.Ban_NoticePlayer, ZhCn, "测试GM");

            Assert.That(text, Is.Not.Null);
            Assert.That(text, Does.Contain("测试GM"));
            Assert.That(text, Does.Not.Contain("{0}"));
        }

        [Test]
        public void WithoutInitialize_NothingLoads()
        {
            MessageStore.Reset();

            Assert.That(MessageStore.Get(MessageCategories.ClientMessage, ClientMessage.BanReason, ZhCn), Is.Null);
        }

        /// <summary>
        /// 插件文案目录由 PluginContainer 注册，优先级低于宿主根目录；目录名即类别名。
        /// 每个插件一个 Messages 目录，同名文件不会互相覆盖。
        /// </summary>
        [Test]
        public void PluginDirectory_IsDiscoveredAsCategory()
        {
            MessageStore.AddDirectory(PluginMessageDir);

            var zh = MessageStore.Get("TreasureMapText", "LevelTooLow", ZhCn);

            Assert.That(zh, Is.Not.Null, "插件的消息类别没有被发现");
            Assert.That(zh, Is.EqualTo("30级之后再来吧"));

            MessageStore.RemoveDirectory(PluginMessageDir);

            Assert.That(MessageStore.Get("TreasureMapText", "LevelTooLow", ZhCn), Is.Null, "移除目录后不应再读到插件文案");
        }

        /// <summary>
        /// 两个插件各带一个同名类别时，宿主目录优先、先注册的插件优先，且不会互相污染。
        /// </summary>
        [Test]
        public void DuplicateCategories_AcrossDirectories_UseFirstFound()
        {
            var first = Path.Combine(Path.GetTempPath(), "ruams-i18n-test-a", Guid.NewGuid().ToString());
            var second = Path.Combine(Path.GetTempPath(), "ruams-i18n-test-b", Guid.NewGuid().ToString());

            try
            {
                WriteMessage(first, "SharedCategory", "zh-CN", "FromFirst");
                WriteMessage(second, "SharedCategory", "zh-CN", "FromSecond");

                MessageStore.AddDirectory(first);
                MessageStore.AddDirectory(second);

                Assert.That(MessageStore.Get("SharedCategory", "Key", ZhCn), Is.EqualTo("FromFirst"));
            }
            finally
            {
                MessageStore.RemoveDirectory(first);
                MessageStore.RemoveDirectory(second);
                TryDeleteDirectory(first);
                TryDeleteDirectory(second);
            }
        }

        /// <summary>
        /// 同一目录新旧两个插件容器共享时，旧容器卸载不能把新容器还在用的文案目录摘掉。
        /// </summary>
        [Test]
        public void RemoveDirectory_IsReferenceCounted()
        {
            MessageStore.AddDirectory(PluginMessageDir);
            MessageStore.AddDirectory(PluginMessageDir);

            MessageStore.RemoveDirectory(PluginMessageDir);
            Assert.That(MessageStore.Get("TreasureMapText", "LevelTooLow", ZhCn), Is.Not.Null, "引用计数未归零时不应移除");

            MessageStore.RemoveDirectory(PluginMessageDir);
            Assert.That(MessageStore.Get("TreasureMapText", "LevelTooLow", ZhCn), Is.Null);
        }

        private static void WriteMessage(string root, string category, string culture, string value)
        {
            var directory = Path.Combine(root, category);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, $"{culture}.json"), $"{{ \"Key\": \"{value}\" }}");
        }

        private static void TryDeleteDirectory(string directory)
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
