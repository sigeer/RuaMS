using Application.Core.Gameplay.Plugins;
using Application.Plugin.Script;
using Application.Templates;
using Application.Templates.Item.Consume;
using Application.Templates.Map;
using Application.Templates.Npc;
using Application.Templates.Reader;
using System.Reflection;

namespace ServiceTest.Infrastructure.WZ;

/// <summary>
/// 校验 WZ 中声明的脚本名都能在插件注册表里解析。
/// 这类校验能提前抓到「WZ 指向一个不存在的脚本」——例如巴洛古简单模式
/// 曾经凭空拆出的 <c>easy_balog_summon</c>。
///
/// 仓库里仍存在大量 WZ 声明、两侧都没有实现的脚本名，因此采用基线快照：
/// 基线内的缺失视为已知债务，基线与实际不一致（新增缺失 / 已修复）都会失败，
/// 迫使调用方更新 <c>script-registration-baseline.txt</c>，避免缺口悄悄扩大。
/// </summary>
internal class ScriptRegistrationCheck : WzTestBase
{
    /// <summary>
    /// 本插件声明的全部脚本，按种类归类。等价于 PluginManager 对单个插件的扫描结果
    /// （测试只关心本插件，不涉及跨插件优先级）。
    /// </summary>
    static readonly Dictionary<ScriptKind, Dictionary<string, ScriptHandlerDescriptor>> _registries = LoadRegistries();

    static Dictionary<ScriptKind, Dictionary<string, ScriptHandlerDescriptor>> LoadRegistries()
    {
        // 这里只关心脚本名 → 实现的映射，不涉及插件容器与优先级，
        // 因此直接扫描（source 传 null）而不构造 ScriptService——后者需要服务器节点。
        var handlers = ScriptFeatureDiscovery.CollectHandlers(typeof(ScriptService).Assembly, null!);

        return Enum.GetValues<ScriptKind>()
            .ToDictionary(kind => kind, kind => ToRegistry(handlers.Where(x => x.Kind == kind)));
    }

    static Dictionary<string, ScriptHandlerDescriptor> ToRegistry(IEnumerable<ScriptHandlerDescriptor> handlers)
    {
        return handlers.ToDictionary(x => x.ScriptName);
    }

    public ScriptRegistrationCheck(string readerType) : base(readerType)
    {
    }

    /// <summary>
    /// 基线一次性生成：四个用例并行执行，若各自写文件会互相覆盖。
    /// </summary>
    [OneTimeSetUp]
    public void EnsureBaseline()
    {
        if (File.Exists(BaselinePath))
            return;

        var lines = new List<string>();
        foreach (var (kind, names) in DeclaredNames())
        {
            lines.AddRange(names
                .Where(x => !_registries[kind].ContainsKey(x))
                .Distinct()
                .OrderBy(x => x, StringComparer.Ordinal)
                .Select(x => $"{kind}|{x}"));
        }

        File.WriteAllLines(BaselinePath, lines);
    }

    IEnumerable<(ScriptKind Kind, string[] Names)> DeclaredNames()
    {
        // onUserEnter 与 onFirstUserEnter 共用 ScriptKind.Map
        yield return (ScriptKind.Map, _providerSource.GetProvider(ProviderType.Map)
            .LoadAll().OfType<MapTemplate>()
            .SelectMany(x => new[] { x.OnUserEnter, x.OnFirstUserEnter })
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!)
            .ToArray());

        yield return (ScriptKind.Portal, _providerSource.GetProvider(ProviderType.Map)
            .LoadAll().OfType<MapTemplate>()
            .SelectMany(x => x.Portals)
            .Select(x => x.Script)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!)
            .ToArray());

        yield return (ScriptKind.Npc, _providerSource.GetProvider(ProviderType.Npc)
            .LoadAll().OfType<NpcTemplate>()
            .Select(x => string.IsNullOrEmpty(x.Script) ? $"n{x.TemplateId}" : x.Script!)
            .ToArray());

        yield return (ScriptKind.Item, _providerSource.GetProvider(ProviderType.Item)
            .LoadAll().OfType<ScriptItemTemplate>()
            .Select(x => x.Script)
            .ToArray());
    }

    /// <summary>
    /// 地图脚本来自 Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c>，两者共用 <see cref="ScriptKind.Map"/>。
    /// </summary>
    [Test]
    public void MapHooksResolve()
    {
        AssertHooksResolve(ScriptKind.Map, x => x.OnUserEnter);
        AssertHooksResolve(ScriptKind.Map, x => x.OnFirstUserEnter);
    }

    /// <summary>
    /// 传送门脚本来自 Map.wz 中 portal 节点的 <c>script</c>。
    /// </summary>
    [Test]
    public void PortalScriptsResolve()
    {
        var names = _providerSource.GetProvider(ProviderType.Map)
            .LoadAll().OfType<MapTemplate>()
            .SelectMany(x => x.Portals)
            .Select(x => x.Script)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!);

        AssertAgainstBaseline(ScriptKind.Portal, names, _registries[ScriptKind.Portal]);
    }

    /// <summary>
    /// 守卫：注册总数不得少于快照。
    ///
    /// 迁移脚本时最容易犯的错是「把实现从旧文件删了，却没真正写进新文件」——
    /// 若该脚本名恰好不在 WZ 声明里，上面的解析校验就抓不到，脚本会静默消失。
    /// 这里对每个种类记录注册总数，减少即失败并指出缺失的键。
    /// </summary>
    [Test]
    public void RegistryCountsDoNotShrink()
    {
        var previous = LoadCounts();
        var current = _registries.ToDictionary(x => x.Key, x => x.Value.Keys.ToHashSet());
        var problems = new List<string>();

        foreach (var (kind, names) in current)
        {
            if (!previous.TryGetValue(kind, out var oldNames))
                continue;

            var lost = oldNames.Except(names).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            if (lost.Length > 0)
            {
                problems.Add($"{kind}：丢失 {lost.Length} 个已注册脚本：{string.Join(", ", lost.Take(20))}");
            }
        }

        Assert.That(problems, Is.Empty, string.Join(Environment.NewLine, problems));
    }

    /// <summary>
    /// 更新注册数快照。仅在有意删除脚本后手动调用。
    /// </summary>
    static void SaveCounts(Dictionary<ScriptKind, HashSet<string>> current)
    {
        File.WriteAllLines(CountsPath, current
            .SelectMany(kv => kv.Value.Select(x => $"{kv.Key}|{x}"))
            .OrderBy(x => x, StringComparer.Ordinal));
    }

    static string CountsPath => Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..",
        "test", "ServiceTest", "Infrastructure", "WZ", "script-registration-counts.txt"));

    static Dictionary<ScriptKind, HashSet<string>> LoadCounts()
    {
        var result = new Dictionary<ScriptKind, HashSet<string>>();
        if (!File.Exists(CountsPath))
            return result;

        foreach (var line in File.ReadAllLines(CountsPath).Where(x => x.Contains('|')))
        {
            var sep = line.IndexOf('|');
            if (!Enum.TryParse<ScriptKind>(line[..sep], out var kind))
                continue;

            if (!result.TryGetValue(kind, out var set))
            {
                set = [];
                result[kind] = set;
            }
            set.Add(line[(sep + 1)..]);
        }

        return result;
    }

    /// <summary>
    /// 首次运行生成快照，之后作为回归基线。
    /// </summary>
    [OneTimeSetUp]
    public void EnsureCountsSnapshot()
    {
        if (!File.Exists(CountsPath))
        {
            SaveCounts(_registries.ToDictionary(x => x.Key, x => x.Value.Keys.ToHashSet()));
        }
    }

    /// <summary>
    /// NPC 的 <c>info/script</c> 为空时运行期回退到 <c>n{id}</c>。
    /// </summary>
    [Test]
    public void NpcScriptsResolve()
    {
        var registry = _registries[ScriptKind.Npc];
        var names = _providerSource.GetProvider(ProviderType.Npc)
            .LoadAll().OfType<NpcTemplate>()
            .Select(x => string.IsNullOrEmpty(x.Script) ? $"n{x.TemplateId}" : x.Script!);

        AssertAgainstBaseline(ScriptKind.Npc, names, registry);
    }

    /// <summary>
    /// 道具脚本的键来自 <c>spec/script</c>，为空时模板构造器已回退为 <c>item{id}</c>。
    /// </summary>
    [Test]
    public void ItemScriptsResolve()
    {
        var registry = _registries[ScriptKind.Item];
        var names = _providerSource.GetProvider(ProviderType.Item)
            .LoadAll().OfType<ScriptItemTemplate>()
            .Select(x => x.Script);

        AssertAgainstBaseline(ScriptKind.Item, names, registry);
    }

    void AssertHooksResolve(ScriptKind kind, Func<MapTemplate, string?> selector)
    {
        var names = _providerSource.GetProvider(ProviderType.Map)
            .LoadAll().OfType<MapTemplate>()
            .Select(selector)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!);

        AssertAgainstBaseline(kind, names, _registries[kind]);
    }

    static void AssertAgainstBaseline(
        ScriptKind kind,
        IEnumerable<string> declaredNames,
        Dictionary<string, ScriptHandlerDescriptor> registry)
    {
        var missing = declaredNames.Where(x => !registry.ContainsKey(x)).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();

        // 基线为空的种类表示「已全部解析」，这是合法状态，不需要额外断言。
        var baseline = LoadBaseline(kind);

        var newlyMissing = missing.Except(baseline).ToArray();
        var fixedNames = baseline.Except(missing).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(newlyMissing, Is.Empty,
                $"{kind}：WZ 声明但未注册的脚本（新增缺口）：{string.Join(", ", newlyMissing.Take(20))}");
            Assert.That(fixedNames, Is.Empty,
                $"{kind}：基线中的缺失已修复或消失，请更新基线：{string.Join(", ", fixedNames.Take(20))}");
        });
    }

    /// <summary>
    /// 基线文件放在测试工程源码目录下，随仓库提交，便于评审基线变化。
    /// </summary>
    static string BaselinePath => Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..",
        "test", "ServiceTest", "Infrastructure", "WZ", "script-registration-baseline.txt"));

    static HashSet<string> LoadBaseline(ScriptKind kind)
    {
        if (!File.Exists(BaselinePath))
            return [];

        var prefix = kind + "|";
        return File.ReadAllLines(BaselinePath)
            .Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
            .Select(x => x[prefix.Length..])
            .ToHashSet(StringComparer.Ordinal);
    }
}
