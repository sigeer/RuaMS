using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Events.Templates;
using Application.Plugin.Script;
using Application.Plugin.Script.Features.Exped_Balrog;
using NUnit.Framework;
using System.Reflection;

namespace ServiceTest.Games.Gameplay
{
    /// <summary>
    /// 扫描层（<see cref="ScriptFeatureDiscovery"/>）的守卫测试。
    ///
    /// 脚本与事件模板都从整个插件程序集扫描得到，与命名空间无关——
    /// 旧式的按种类 partial 类与 <c>Features/</c> 下的功能目录一视同仁。
    /// </summary>
    [TestFixture]
    public class ScriptFeatureDiscoveryTests
    {
        static readonly ScriptService _service = TestScriptService.Create();

        static readonly Assembly _assembly = typeof(ScriptService).Assembly;

        static readonly IReadOnlyCollection<ScriptHandlerDescriptor> _handlers =
            ScriptFeatureDiscovery.CollectHandlers(_assembly, _service);

        static readonly IReadOnlyList<AbstractEventTemplate> _eventTemplates =
            ScriptFeatureDiscovery.CreateEventTemplates(_assembly);

        [OneTimeTearDown]
        public async Task DisposeService() => await _service.DisposeAsync();

        [Test]
        public void HandlersAreDiscovered()
        {
            Assert.That(_handlers, Is.Not.Empty);
            Assert.That(_handlers.Select(x => x.ScriptName), Does.Contain("easy_balog_summon"));
        }

        /// <summary>
        /// 巴洛古简单模式：召唤由地图脚本 <c>easy_balog_summon</c> 完成，
        /// 祭坛反应堆 <c>Easy_balogReactor</c> 的 hit 路径已移除，不再重复召唤。
        /// </summary>
        [Test]
        public void Exped_BalrogEasyEventWiring()
        {
            Assert.That(_handlers.Any(x => x.Kind == ScriptKind.Map && x.ScriptName == "easy_balog_summon"));
            Assert.That(_handlers.Any(x => x.Kind == ScriptKind.ReactorHit && x.ScriptName == "Easy_balogReactor"), Is.False);
            Assert.That(_eventTemplates.Any(x => x.Name == Exped_BalrogFeature.EasyEventName));
        }

        /// <summary>
        /// 处理器必须能由 <see cref="DynamicObjectFactory"/> 用脚本分发时的参数构造。
        /// </summary>
        [Test]
        public void HandlerConstructorsAreResolvable()
        {
            var handler = _handlers.Single(x => x.Kind == ScriptKind.Map && x.ScriptName == "easy_balog_summon");

            var ctor = handler.HandlerType.GetConstructor(
                [typeof(Application.Core.Client.IChannelClient), typeof(Application.Core.Game.Maps.IMap)]);

            Assert.That(ctor, Is.Not.Null);
        }

        /// <summary>
        /// 事件模板不允许重名——<see cref="PluginManager"/> 遇到重名会抛异常，这里先在本插件内部守一道。
        /// </summary>
        [Test]
        public void EventTemplateNamesAreUnique()
        {
            var duplicated = _eventTemplates
                .GroupBy(x => x.Name)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToArray();

            Assert.That(duplicated, Is.Empty, "事件重名：" + string.Join(", ", duplicated));
        }

        /// <summary>
        /// 守卫：事件模板总数不得减少。事件名不在 WZ 里，解析校验抓不到，只能靠名称快照兜底。
        /// </summary>
        [Test]
        public void EventTemplateCountsDoNotShrink()
        {
            var names = _eventTemplates
                .Select(x => x.Name)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            var snapshot = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..",
                "test", "ServiceTest", "Games", "Gameplay", "event-template-names.txt"));

            if (!File.Exists(snapshot))
            {
                File.WriteAllLines(snapshot, names);
            }

            var previous = File.ReadAllLines(snapshot).Where(x => x.Length > 0).ToHashSet(StringComparer.Ordinal);
            var lost = previous.Except(names).OrderBy(x => x, StringComparer.Ordinal).ToArray();

            Assert.That(lost, Is.Empty, "事件模板丢失：" + string.Join(", ", lost));
            Assert.That(names, Is.Not.Empty);
        }

        /// <summary>
        /// 守卫：每个登记的脚本基类都应至少派生出一个处理器。
        /// 某个种类失去全部实现时，注册表会出现缺口而解析校验未必抓得到；这里直接拦住。
        /// </summary>
        [Test]
        public void EveryScriptBaseHasHandlers()
        {
            var types = _assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsNested)
                .ToArray();

            var unused = ScriptFeatureDiscovery.ScriptBaseTypes
                .Where(b => !types.Any(t => t.BaseType == b))
                .Select(b => b.Name)
                .ToArray();

            Assert.That(unused, Is.Empty, "以下脚本基类没有任何直接子类：" + string.Join(", ", unused));
        }
    }
}
