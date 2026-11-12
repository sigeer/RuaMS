using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Serilog;

namespace Application.Resources.Messages
{
    /// <summary>
    /// 消息文本加载器。只读磁盘，不使用内嵌资源。
    /// 文件布局：{Root}/{Category}/{CultureName}.json，值内的 "\r\n" 由 JSON 解析成真实 CRLF，
    /// 因此不受 git 换行归一化影响（客户端只认 0x0D，见 docs/ida/CTextAnalyzer_文本换行_分析.md）。
    /// <para>
    /// 类别是普通字符串：内置的见 <see cref="MessageCategories"/>；插件用自己的类别名，
    /// 把 <c>i18n/{Category}/{Culture}.json</c> 放在 <c>Plugins/{插件名}/i18n</c> 下即可。
    /// 插件目录每次查表时实时枚举，装/卸/改名都立刻生效，不需要任何登记。
    /// 每个插件独占一个目录，因此不同插件的同名文件不会互相覆盖。
    /// </para>
    /// 查找顺序：宿主的 <see cref="Initialize"/> 根目录在前，插件目录按注册顺序在后，首个命中的文件生效。
    /// 未调用 <see cref="Initialize"/> 时没有任何搜索目录，<see cref="Get"/> 一律返回 <c>null</c>。
    /// </summary>
    public static class MessageStore
    {
        private const string DefaultCulture = "zh-CN";

        private static readonly ConcurrentDictionary<(string Category, string Culture), Lazy<IReadOnlyDictionary<string, string>?>> _cache = new();

        /// <summary>宿主初始化时给定的根目录。</summary>
        private static readonly List<string> _roots = [];

        /// <summary>
        /// 插件根目录（默认 <c>BaseDirectory/Plugins</c>）。每次查表时实时展开成
        /// <c>{PluginRoot}/{插件名}/i18n</c>，不维护插件列表——插件热插拔、目录改名都能立刻反映。
        /// </summary>
        private static string _pluginRoot = Path.Combine(AppContext.BaseDirectory, "Plugins");

        private static readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _sync = new();

        private static bool _hotReload = true;

        /// <summary>
        /// 启用磁盘加载。目录默认为 <c>AppContext.BaseDirectory/i18n</c>。
        /// </summary>
        public static void Initialize(string? directory = null, bool hotReload = true)
        {
            Initialize(directory == null ? null : [directory], null, hotReload);
        }

        /// <summary>
        /// 启用磁盘加载，按顺序查找多个根目录（越靠前优先级越高）。
        /// 插件根目录默认为 <c>BaseDirectory/Plugins</c>。
        /// </summary>
        public static void Initialize(IReadOnlyList<string>? directories, bool hotReload = true)
        {
            Initialize(directories, null, hotReload);
        }

        /// <summary>
        /// 启用磁盘加载，并指定插件根目录（<c>{pluginRoot}/{插件名}/i18n</c>）。
        /// </summary>
        public static void Initialize(IReadOnlyList<string>? directories, string? pluginRoot, bool hotReload)
        {
            lock (_sync)
            {
                var targets = (directories ?? [Path.Combine(AppContext.BaseDirectory, "i18n")])
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(Path.GetFullPath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (targets.Count == 0)
                    targets.Add(Path.Combine(AppContext.BaseDirectory, "i18n"));

                foreach (var path in _watchers.Keys.ToList())
                    StopWatching(path);

                _roots.Clear();
                _roots.AddRange(targets);
                _pluginRoot = Path.GetFullPath(pluginRoot ?? Path.Combine(AppContext.BaseDirectory, "Plugins"));

                _hotReload = hotReload;
                _cache.Clear();

                foreach (var target in targets)
                    StartWatching(target);

                StartWatching(_pluginRoot);
            }
        }

        /// <summary>
        /// 清空所有目录与缓存（主要给测试用）。
        /// </summary>
        public static void Reset()
        {
            lock (_sync)
            {
                foreach (var path in _watchers.Keys.ToList())
                    StopWatching(path);

                _roots.Clear();
                _pluginRoot = Path.Combine(AppContext.BaseDirectory, "Plugins");
                _cache.Clear();
            }
        }

        /// <summary>
        /// 取一条消息原文。找不到返回 <c>null</c>。
        /// </summary>
        public static string? Get(string category, string key, CultureInfo? culture = null)
        {
            var cultureName = culture?.Name ?? DefaultCulture;

            var table = GetTable(category, cultureName);
            if (table != null && table.TryGetValue(key, out var value))
                return value;

            if (!string.Equals(cultureName, DefaultCulture, StringComparison.OrdinalIgnoreCase))
            {
                table = GetTable(category, DefaultCulture);
                if (table != null && table.TryGetValue(key, out value))
                    return value;
            }

            Log.Logger.Verbose("i18n未找到{Key}", key);
            return null;
        }

        /// <summary>
        /// 取一条消息并按 <see cref="string.Format(string, object[])"/> 填充占位符。找不到返回 <paramref name="key"/>。
        /// </summary>
        public static string Format(string category, string key, CultureInfo? culture, params object[] args)
        {
            var message = Get(category, key, culture);
            if (string.IsNullOrEmpty(message))
                return key;

            return args.Length == 0 ? message : string.Format(message, args);
        }

        /// <summary>
        /// 取默认语言（zh-CN）的消息，不做格式化。服务端日志等与玩家语言无关的场景使用。
        /// </summary>
        public static string? GetDefault(string category, string key) => Get(category, key, null);

        /// <summary>
        /// 取默认语言（zh-CN）的消息并格式化。
        /// </summary>
        public static string FormatDefault(string category, string key, params object[] args) =>
            Format(category, key, null, args);

        /// <summary>
        /// 某类别某语言已加载的全部消息（测试与诊断用）。
        /// </summary>
        public static IReadOnlyDictionary<string, string>? GetTable(string category, string cultureName)
        {
            var key = (category, cultureName);

            // 快路径：命中后不碰 _sync。Get 是热路径，而 Initialize
            // 会在持有 _sync 期间创建/销毁 FileSystemWatcher（毫秒级），不能让它挡在查表前面。
            if (_cache.TryGetValue(key, out var cached))
                return cached.Value;

            var directories = SnapshotDirectories();
            if (directories.Count == 0)
            {
                Log.Logger.Warning("i18n没有可用的消息目录，{Category}/{Culture}.json 无法加载", category, cultureName);

                // 连空结果一起缓存：否则每次查表都会重复打这条日志。
                // Initialize 会清缓存；热重载由文件监听单独失效，不会留下过期状态。
                return _cache.GetOrAdd(key, static _ =>
                    new Lazy<IReadOnlyDictionary<string, string>?>(static () => null)).Value;
            }

            return _cache.GetOrAdd(key, static (k, dirs) =>
                    new Lazy<IReadOnlyDictionary<string, string>?>(() => ReadTable(k.Category, k.Culture, dirs)),
                    directories)
                .Value;
        }

        /// <summary>
        /// 查表顺序：宿主根目录在前，插件文案目录（<c>{PluginRoot}/{插件名}/i18n</c>）按目录名在后。
        /// 每次实时枚举插件目录，插件装了/卸了/改名都立刻反映，不需要登记。
        /// </summary>
        private static IReadOnlyList<string> SnapshotDirectories()
        {
            List<string> roots;
            string pluginRoot;

            lock (_sync)
            {
                roots = _roots;
                pluginRoot = _pluginRoot;
            }

            if (roots.Count == 0 && !Directory.Exists(pluginRoot))
                return [];

            var result = new List<string>(roots.Count + 4);
            result.AddRange(roots);

            if (Directory.Exists(pluginRoot))
            {
                foreach (var plugin in Directory.EnumerateDirectories(pluginRoot).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    var directory = Path.Combine(plugin, "i18n");
                    if (Directory.Exists(directory))
                        result.Add(directory);
                }
            }

            return result;
        }

        private static void StartWatching(string directory)
        {
            if (!_hotReload || _watchers.ContainsKey(directory) || !Directory.Exists(directory))
                return;

            var watcher = new FileSystemWatcher(directory, "*.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileRenamed;

            _watchers[directory] = watcher;
        }

        private static void StopWatching(string directory)
        {
            if (!_watchers.Remove(directory, out var watcher))
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFileChanged;
            watcher.Created -= OnFileChanged;
            watcher.Deleted -= OnFileChanged;
            watcher.Renamed -= OnFileRenamed;
            watcher.Dispose();
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e) => InvalidateByPath(e.FullPath);

        private static void OnFileRenamed(object sender, RenamedEventArgs e) => InvalidateByPath(e.FullPath);

        private static void InvalidateByPath(string fullPath)
        {
            // .../<Category>/<culture>.json
            var culture = Path.GetFileNameWithoutExtension(fullPath);
            var category = Path.GetFileName(Path.GetDirectoryName(fullPath));
            if (string.IsNullOrEmpty(culture) || string.IsNullOrEmpty(category))
                return;

            _cache.TryRemove((category, culture), out _);

            // 也失效 CultureInfo 规范化后的名字
            try
            {
                var normalized = CultureInfo.GetCultureInfo(culture).Name;
                if (!string.Equals(normalized, culture, StringComparison.Ordinal))
                    _cache.TryRemove((category, normalized), out _);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        private static IReadOnlyDictionary<string, string>? ReadTable(string category, string cultureName, IReadOnlyList<string> directories)
        {
            foreach (var directory in directories)
            {
                var table = ReadJsonFile(Path.Combine(directory, category, $"{cultureName}.json"));
                if (table != null)
                    return table;
            }

            Log.Logger.Warning("i18n缺少消息文件 {Category}/{Culture}.json（已搜索 {Count} 个目录）", category, cultureName, directories.Count);
            return null;
        }

        private static Dictionary<string, string>? ReadJsonFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                using var stream = File.OpenRead(path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
            }
            catch (Exception ex)
            {
                Log.Logger.Warning(ex, "i18n读取失败 {Path}", path);
                return null;
            }
        }
    }
}
