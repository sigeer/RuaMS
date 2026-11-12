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

        /// <summary>
        /// 模拟运行期的 Plugins 目录：{插件根}/{插件名}/i18n/{Category}/{Culture}.json。
        /// MessageStore 每次查表实时枚举它，因此这里只要在磁盘上摆好目录即可。
        /// </summary>
        private static string PluginRoot = null!;

        [SetUp]
        public void SetUp()
        {
            PluginRoot = Path.Combine(Path.GetTempPath(), "ruams-i18n-plugins", Guid.NewGuid().ToString());
            Directory.CreateDirectory(PluginRoot);

            CopyPluginMessages("Application.Plugin.Script");
            CopyPluginMessages("Application.Plugin.TreasureMap");

            MessageStore.Reset();
            MessageStore.Initialize([HostMessageDir], PluginRoot, hotReload: false);
        }

        /// <summary>把仓库里某个插件的 i18n 目录原样拷到模拟的插件根下。</summary>
        private static void CopyPluginMessages(string pluginName)
        {
            var source = Path.Combine(RepoRoot, "src", pluginName, "i18n");
            var target = Path.Combine(PluginRoot, pluginName, "i18n");

            foreach (var file in Directory.GetFiles(source, "*.json", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(target, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }
        }

        [TearDown]
        public void TearDown()
        {
            MessageStore.Reset();
            TryDeleteDirectory(PluginRoot);
        }

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
        /// 插件文案目录由 MessageStore 实时枚举 {插件根}/{插件名}/i18n 得到，目录名即类别名。
        /// </summary>
        [Test]
        public void PluginDirectory_IsDiscoveredAsCategory()
        {
            var zh = MessageStore.Get("TreasureMapText", "LevelTooLow", ZhCn);

            Assert.That(zh, Is.Not.Null, "插件的消息类别没有被发现");
            Assert.That(zh, Is.EqualTo("30级之后再来吧"));
        }

        /// <summary>
        /// 「一直取最新的」：目录被移走/新增后，下一次查表立刻反映，不需要任何登记或失效调用。
        /// </summary>
        [Test]
        public void PluginDirectories_AreResolvedOnEachLookup()
        {
            var added = Path.Combine(PluginRoot, "Application.Plugin.Added", "i18n");
            WriteMessage(added, "AddedCategory", "zh-CN", "Added");

            Assert.That(MessageStore.Get("AddedCategory", "Key", ZhCn), Is.EqualTo("Added"));

            TryDeleteDirectory(Path.Combine(PluginRoot, "Application.Plugin.Added"));

            MessageStore.Reset();
            MessageStore.Initialize([HostMessageDir], PluginRoot, hotReload: false);

            Assert.That(MessageStore.Get("AddedCategory", "Key", ZhCn), Is.Null, "目录移除后不应再读到");
        }

        /// <summary>
        /// 两个插件各带一个同名类别时，宿主目录优先；插件之间按目录名排序，先者胜出且不互相污染。
        /// </summary>
        [Test]
        public void DuplicateCategories_AcrossPlugins_UseFirstByDirectoryName()
        {
            WriteMessage(Path.Combine(PluginRoot, "Application.Plugin.A", "i18n"), "SharedCategory", "zh-CN", "FromA");
            WriteMessage(Path.Combine(PluginRoot, "Application.Plugin.B", "i18n"), "SharedCategory", "zh-CN", "FromB");

            MessageStore.Reset();
            MessageStore.Initialize([HostMessageDir], PluginRoot, hotReload: false);

            Assert.That(MessageStore.Get("SharedCategory", "Key", ZhCn), Is.EqualTo("FromA"));
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
