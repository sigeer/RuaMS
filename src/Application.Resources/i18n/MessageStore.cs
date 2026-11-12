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
    /// 把 <c>i18n/{Category}/{Culture}.json</c> 放在 <c>Plugins/{插件名}/i18n</c> 下，
    /// 由 <c>PluginManager</c> 通过 <see cref="AddDirectory"/> 注册，卸载时 <see cref="RemoveDirectory"/>。
    /// 每个插件独占一个目录，因此不同插件的同名文件不会互相覆盖。
    /// </para>
    /// 查找顺序：宿主的 <see cref="Initialize"/> 根目录在前，插件目录按注册顺序在后，首个命中的文件生效。
    /// 未调用 <see cref="Initialize"/> 时没有任何搜索目录，<see cref="Get"/> 一律返回 <c>null</c>。
    /// </summary>
    public static class MessageStore
    {
        private const string DefaultCulture = "zh-CN";

        private static readonly ConcurrentDictionary<(string Category, string Culture), Lazy<IReadOnlyDictionary<string, string>?>> _cache = new();

        /// <summary>宿主初始化时给定的根目录，不受插件装卸影响。</summary>
        private static readonly List<string> _roots = [];

        /// <summary>插件目录，按注册顺序排在根目录之后；同一目录可能被新旧两个容器共享，因此带引用计数。</summary>
        private static readonly List<(string Path, int RefCount)> _pluginDirectories = [];

        private static readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _sync = new();

        private static bool _hotReload = true;

        /// <summary>
        /// 启用磁盘加载。目录默认为 <c>AppContext.BaseDirectory/Messages</c>。
        /// </summary>
        public static void Initialize(string? directory = null, bool hotReload = true)
        {
            Initialize(directory == null ? null : [directory], hotReload);
        }

        /// <summary>
        /// 启用磁盘加载，按顺序查找多个根目录（越靠前优先级越高）。
        /// 会替换此前通过本方法设置的根目录，并清空所有插件目录。
        /// </summary>
        public static void Initialize(IReadOnlyList<string>? directories, bool hotReload = true)
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

                foreach (var path in _pluginDirectories.Select(x => x.Path).ToList())
                    StopWatching(path);

                _roots.Clear();
                _roots.AddRange(targets);
                _pluginDirectories.Clear();

                _hotReload = hotReload;
                _cache.Clear();

                foreach (var target in targets)
                    StartWatching(target);
            }
        }

        /// <summary>
        /// 追加一个搜索目录（插件文案用），优先级低于 <see cref="Initialize"/> 的根目录。
        /// 同一目录重复调用只增加引用计数。
        /// </summary>
        public static void AddDirectory(string? directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return;

            var path = Path.GetFullPath(directory);

            lock (_sync)
            {
                var index = IndexOfPluginDirectory(path);
                if (index >= 0)
                {
                    var entry = _pluginDirectories[index];
                    _pluginDirectories[index] = (entry.Path, entry.RefCount + 1);
                    return;
                }

                if (Directory.Exists(path))
                    WarnShadowedCategories(path);

                _pluginDirectories.Add((path, 1));
                _cache.Clear();
                StartWatching(path);
            }
        }

        /// <summary>
        /// 移除一个搜索目录。引用计数归零才真正移除并清空缓存。
        /// 插件热重载时新旧容器会短暂共享同一目录，因此必须按计数处理。
        /// </summary>
        public static void RemoveDirectory(string? directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return;

            var path = Path.GetFullPath(directory);

            lock (_sync)
            {
                var index = IndexOfPluginDirectory(path);
                if (index < 0)
                    return;

                var entry = _pluginDirectories[index];
                if (entry.RefCount > 1)
                {
                    _pluginDirectories[index] = (entry.Path, entry.RefCount - 1);
                    return;
                }

                _pluginDirectories.RemoveAt(index);
                StopWatching(path);
                _cache.Clear();
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
                _pluginDirectories.Clear();
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
            var directories = SnapshotDirectories();
            if (directories.Count == 0)
            {
                Log.Logger.Warning("i18n没有可用的消息目录，{Category}/{Culture}.json 无法加载", category, cultureName);
                return null;
            }

            var entry = _cache.GetOrAdd((category, cultureName), static (k, dirs) =>
                new Lazy<IReadOnlyDictionary<string, string>?>(() => ReadTable(k.Category, k.Culture, dirs)),
                directories);

            return entry.Value;
        }

        private static IReadOnlyList<string> SnapshotDirectories()
        {
            lock (_sync)
            {
                if (_roots.Count == 0 && _pluginDirectories.Count == 0)
                    return [];

                var result = new List<string>(_roots.Count + _pluginDirectories.Count);
                result.AddRange(_roots);
                result.AddRange(_pluginDirectories.Select(x => x.Path));
                return result;
            }
        }

        /// <summary>
        /// 新目录里的类别如果前面已经有目录提供，后者会被优先级更高的目录盖住——只记一条日志，便于巡检。
        /// </summary>
        private static void WarnShadowedCategories(string path)
        {
            foreach (var category in EnumerateCategories(path))
            {
                var owner = FindCategoryOwner(category);
                if (owner != null)
                    Log.Logger.Warning("i18n类别 {Category} 已由 {Owner} 提供，{Path} 中的同名类别不会生效", category, owner, path);
            }
        }

        private static IEnumerable<string> EnumerateCategories(string directory)
        {
            foreach (var sub in Directory.EnumerateDirectories(directory))
            {
                var category = Path.GetFileName(sub);
                if (!string.IsNullOrEmpty(category))
                    yield return category;
            }
        }

        private static string? FindCategoryOwner(string category)
        {
            foreach (var directory in _roots.Concat(_pluginDirectories.Select(x => x.Path)))
            {
                if (Directory.Exists(Path.Combine(directory, category)))
                    return directory;
            }

            return null;
        }

        private static int IndexOfPluginDirectory(string path)
        {
            for (var i = 0; i < _pluginDirectories.Count; i++)
            {
                if (IsSamePath(_pluginDirectories[i].Path, path))
                    return i;
            }

            return -1;
        }

        private static bool IsSamePath(string left, string right) =>
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

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
