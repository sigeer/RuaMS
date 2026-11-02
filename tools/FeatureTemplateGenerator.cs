// 功能模板生成器（.NET 10 file-based app，无需 csproj）：
//
//   dotnet run tools/FeatureTemplateGenerator.cs
//   dotnet run tools/FeatureTemplateGenerator.cs -- OrbisPQ --kinds Npc,Map,Portal
//
// 输入功能名并勾选脚本种类，在 src/Application.Plugin.Script/Features/<Name>/ 下
// 生成 <Name>Feature.cs 与各种类的处理器文件。
// 目录已存在时，逐个询问重复的种类是覆盖还是跳过（--force / --skip 可免交互）。

return FeatureGenerator.Scaffold.Run(args);

namespace FeatureGenerator
{
    /// <summary>
    /// 可生成的脚本种类。前九个与 <c>Application.Core.Gameplay.Plugins.ScriptKind</c> 一一对应，
    /// 后三个是事件模板——它不是脚本，却是功能目录里最常见的成员。
    /// </summary>
    internal enum ScriptKindOption
    {
        Npc,
        Quest,
        Item,
        Portal,
        Map,
        ReactorHit,
        ReactorAct,
        ReactorTouch,
        ReactorUntouch,
        PartyQuestEvent,
        SoloEvent,
        ExpeditionEvent
    }

    /// <summary>
    /// <see cref="ScriptKindOption"/> 的显示名与解析。
    /// 命令行与交互菜单共用同一套名字（大小写不敏感），也可用菜单编号。
    /// </summary>
    internal static class ScriptKinds
    {
        /// <summary>
        /// 菜单顺序即此顺序，编号从 1 开始。
        /// </summary>
        public static readonly ScriptKindOption[] All =
        [
            ScriptKindOption.Npc,
            ScriptKindOption.Quest,
            ScriptKindOption.Item,
            ScriptKindOption.Portal,
            ScriptKindOption.Map,
            ScriptKindOption.ReactorHit,
            ScriptKindOption.ReactorAct,
            ScriptKindOption.ReactorTouch,
            ScriptKindOption.ReactorUntouch,
            ScriptKindOption.PartyQuestEvent,
            ScriptKindOption.SoloEvent,
            ScriptKindOption.ExpeditionEvent
        ];

        public static string GetDisplayName(ScriptKindOption kind)
        {
            return kind switch
            {
                ScriptKindOption.Npc => "Npc",
                ScriptKindOption.Quest => "Quest",
                ScriptKindOption.Item => "Item",
                ScriptKindOption.Portal => "Portal",
                ScriptKindOption.Map => "Map",
                ScriptKindOption.ReactorHit => "ReactorHit",
                ScriptKindOption.ReactorAct => "ReactorAct",
                ScriptKindOption.ReactorTouch => "ReactorTouch",
                ScriptKindOption.ReactorUntouch => "ReactorUntouch",
                ScriptKindOption.PartyQuestEvent => "Event(PartyQuest)",
                ScriptKindOption.SoloEvent => "Event(Solo)",
                ScriptKindOption.ExpeditionEvent => "Event(Expedition)",
                _ => kind.ToString()
            };
        }

        /// <summary>
        /// 接受显示名、枚举名或菜单编号。事件模板额外接受简写 <c>Event</c>（等同组队任务）。
        /// </summary>
        public static bool TryParse(string text, out ScriptKindOption kind)
        {
            var value = text.Trim();
            kind = default;

            if (value.Length == 0)
            {
                return false;
            }

            if (int.TryParse(value, out var index))
            {
                if (index < 1 || index > All.Length)
                {
                    return false;
                }

                kind = All[index - 1];
                return true;
            }

            var normalized = value.Replace("(", "").Replace(")", "").Replace("_", "").Replace("-", "").ToLowerInvariant();

            switch (normalized)
            {
                case "npc":
                    kind = ScriptKindOption.Npc;
                    return true;
                case "quest":
                    kind = ScriptKindOption.Quest;
                    return true;
                case "item":
                    kind = ScriptKindOption.Item;
                    return true;
                case "portal":
                    kind = ScriptKindOption.Portal;
                    return true;
                case "map":
                    kind = ScriptKindOption.Map;
                    return true;
                case "reactorhit":
                    kind = ScriptKindOption.ReactorHit;
                    return true;
                case "reactoract":
                    kind = ScriptKindOption.ReactorAct;
                    return true;
                case "reactortouch":
                    kind = ScriptKindOption.ReactorTouch;
                    return true;
                case "reactoruntouch":
                    kind = ScriptKindOption.ReactorUntouch;
                    return true;
                case "event":
                case "eventpartyquest":
                case "pq":
                    kind = ScriptKindOption.PartyQuestEvent;
                    return true;
                case "eventsolo":
                case "solo":
                    kind = ScriptKindOption.SoloEvent;
                    return true;
                case "eventexpedition":
                case "expedition":
                    kind = ScriptKindOption.ExpeditionEvent;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEvent(ScriptKindOption kind)
        {
            return kind is ScriptKindOption.PartyQuestEvent or ScriptKindOption.SoloEvent or ScriptKindOption.ExpeditionEvent;
        }
    }

    /// <summary>
    /// 生成 <c>src/Application.Plugin.Script/Features/&lt;Name&gt;/</c> 下的文件内容。
    ///
    /// 模板与仓库现有功能目录保持一致：目录名 = 命名空间末段，处理器类直接继承
    /// <c>ScriptFeatureDiscovery</c> 登记的脚本上下文基类（每种 <c>ScriptKind</c> 只有一个），
    /// 这样扫描时无需额外标注。文件只声明类与构造函数，地图、时限、奖励等参数按需补充。
    /// </summary>
    internal static class FeatureTemplates
    {
        /// <summary>
        /// 仓库风格：UTF-8 无 BOM、LF 换行。
        /// </summary>
        public static readonly System.Text.Encoding FileEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        const string Indent = "    ";

        /// <summary>
        /// 功能性文件 &lt;Name&gt;Feature.cs：功能内共用的常量与事件名。
        /// </summary>
        public static string FeatureFile(string name, IEnumerable<ScriptKindOption> kinds)
        {
            var eventKinds = kinds.Where(ScriptKinds.IsEvent).ToArray();

            var sb = new System.Text.StringBuilder();
            sb.Append("namespace Application.Plugin.Script.Features.").Append(name).Append('\n');
            sb.Append("{\n");
            sb.Append(Indent).Append("/// <summary>\n");
            sb.Append(Indent).Append("/// ").Append(name).Append(" 功能内共用的常量。\n");
            sb.Append(Indent).Append("/// </summary>\n");
            sb.Append(Indent).Append("internal static class ").Append(name).Append("Feature\n");
            sb.Append(Indent).Append("{\n");

            for (var i = 0; i < eventKinds.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(Indent).Append(Indent)
                    .Append("internal const string ").Append(EventNameConstant(eventKinds[i]))
                    .Append(" = \"").Append(EventConstantValue(name, eventKinds[i], eventKinds)).Append("\";\n");
            }

            sb.Append(Indent).Append("}\n");
            sb.Append("}\n");

            return sb.ToString();
        }

        /// <summary>
        /// 事件名的默认值：沿用功能名；同一功能内选中多种事件时加后缀，避免常量值重复
        /// （事件名跨插件重名会让 <c>PluginManager</c> 直接抛异常）。
        /// </summary>
        static string EventConstantValue(string name, ScriptKindOption kind, ScriptKindOption[] selected)
        {
            switch (kind)
            {
                case ScriptKindOption.SoloEvent:
                    return name + "_Solo";
                case ScriptKindOption.ExpeditionEvent:
                    return selected.Contains(ScriptKindOption.PartyQuestEvent) ? name + "_Expedition" : name;
                default:
                    return name;
            }
        }

        /// <summary>
        /// 脚本种类对应的处理器文件名，如 <c>OrbisPQ.MapHandler.cs</c>。
        /// </summary>
        public static string HandlerFileName(string name, ScriptKindOption kind)
        {
            return $"{name}.{GetHandlerClassSuffix(kind)}.cs";
        }

        /// <summary>
        /// 事件模板文件名。同一功能可以有多个事件模板，因此按种类区分文件名与类名：
        /// <c>X.Event.cs</c>（组队任务）、<c>X.SoloEvent.cs</c>、<c>X.ExpeditionEvent.cs</c>。
        /// </summary>
        public static string EventFileName(string name, ScriptKindOption kind)
        {
            return $"{name}.{EventClassName(kind)}.cs";
        }

        /// <summary>
        /// 事件模板类名。组队任务保持仓库惯例的 <c>Event</c>。
        /// </summary>
        public static string EventClassName(ScriptKindOption kind)
        {
            return kind switch
            {
                ScriptKindOption.PartyQuestEvent => "Event",
                ScriptKindOption.SoloEvent => "SoloEvent",
                ScriptKindOption.ExpeditionEvent => "ExpeditionEvent",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "不是事件模板种类")
            };
        }

        /// <summary>
        /// 事件模板在 <c>&lt;Name&gt;Feature</c> 中的事件名常量。
        /// </summary>
        static string EventNameConstant(ScriptKindOption kind)
        {
            return kind switch
            {
                ScriptKindOption.PartyQuestEvent => "EventName",
                ScriptKindOption.SoloEvent => "SoloEventName",
                ScriptKindOption.ExpeditionEvent => "ExpeditionEventName",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "不是事件模板种类")
            };
        }

        /// <summary>
        /// 处理器类名：功能名 + 种类，保证同一命名空间内不重名。
        /// </summary>
        public static string HandlerClassName(string name, ScriptKindOption kind)
        {
            return name + GetHandlerClassSuffix(kind);
        }

        static string GetHandlerClassSuffix(ScriptKindOption kind)
        {
            return kind switch
            {
                ScriptKindOption.Npc => "NpcHandler",
                ScriptKindOption.Quest => "QuestHandler",
                ScriptKindOption.Item => "ItemHandler",
                ScriptKindOption.Portal => "PortalHandler",
                ScriptKindOption.Map => "MapHandler",
                ScriptKindOption.ReactorHit => "ReactorHitHandler",
                ScriptKindOption.ReactorAct => "ReactorActHandler",
                ScriptKindOption.ReactorTouch => "ReactorTouchHandler",
                ScriptKindOption.ReactorUntouch => "ReactorUntouchHandler",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "事件模板没有处理器")
            };
        }

        /// <summary>
        /// 生成一个脚本处理器文件。所有种类都遵守同样的骨架：using + 命名空间 + sealed 类 + 一个转发构造函数。
        /// </summary>
        public static string HandlerFile(string name, ScriptKindOption kind)
        {
            var className = HandlerClassName(name, kind);

            if (kind == ScriptKindOption.Map)
            {
                return MapHandler(name, className);
            }

            var (usings, baseType, parameters, arguments) = kind switch
            {
                ScriptKindOption.Npc => (new[] { "Application.Core.Client", "Application.Core.Game.Life", "Application.Core.scripting.npc" },
                    "NpcScriptBase", "IChannelClient c, int npc, NPC? npcObj", "c, npc, npcObj"),
                ScriptKindOption.Quest => (new[] { "Application.Core.Client", "Application.Core.scripting.quest" },
                    "QuestScriptBase", "IChannelClient c, Application.Core.Server.quest.Quest quest, int npc", "c, quest, npc"),
                ScriptKindOption.Item => (new[] { "Application.Core.Client", "Application.Core.scripting.item", "client.inventory" },
                    "ItemScriptBase", "IChannelClient c, Item item, int npcId", "c, item, npcId"),
                ScriptKindOption.Portal => (new[] { "Application.Core.Client", "Application.Core.Server.maps", "scripting.portal" },
                    "PortalPlayerInteraction", "IChannelClient c, IPortal portal", "c, portal"),
                ScriptKindOption.ReactorHit => (new[] { "Application.Core.Client", "Application.Core.Server.maps", "scripting.reactor" },
                    "ReactorHitScriptBase", "IChannelClient c, Reactor reactor", "c, reactor"),
                ScriptKindOption.ReactorAct => (new[] { "Application.Core.Client", "Application.Core.Server.maps", "scripting.reactor" },
                    "ReactorActScriptBase", "IChannelClient c, Reactor reactor", "c, reactor"),
                ScriptKindOption.ReactorTouch => (new[] { "Application.Core.Client", "Application.Core.Server.maps", "scripting.reactor" },
                    "ReactorTouchScriptBase", "IChannelClient c, Reactor reactor", "c, reactor"),
                ScriptKindOption.ReactorUntouch => (new[] { "Application.Core.Client", "Application.Core.Server.maps", "scripting.reactor" },
                    "ReactorUntouchScriptBase", "IChannelClient c, Reactor reactor", "c, reactor"),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "事件模板请用 EventFile")
            };

            return Handler(name, usings, className, baseType, parameters, arguments);
        }

        /// <summary>
        /// 地图脚本：<c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用 <c>MapScriptMethods</c>，
        /// 因此不再区分种类。
        /// </summary>
        static string MapHandler(string name, string className)
        {
            var sb = Begin(name, ["Application.Core.Client", "Application.Core.Game.Maps", "scripting.map"]);
            sb.Append(Indent).Append("/// <summary>\n");
            sb.Append(Indent).Append("/// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，\n");
            sb.Append(Indent).Append("/// 两者只是触发时机不同，因此不再区分种类。\n");
            sb.Append(Indent).Append("/// </summary>\n");
            sb.Append(Indent).Append("internal sealed class ").Append(className).Append(" : MapScriptMethods\n");
            sb.Append(Indent).Append("{\n");
            sb.Append(Indent).Append(Indent).Append("public ").Append(className).Append("(IChannelClient c, IMap map) : base(c, map)\n");
            sb.Append(Indent).Append(Indent).Append("{\n");
            sb.Append(Indent).Append(Indent).Append("}\n");
            sb.Append(Indent).Append("}\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static string Handler(string name, string[] usings, string className, string baseType, string parameters, string arguments)
        {
            var sb = Begin(name, usings);
            sb.Append(Indent).Append("internal sealed class ").Append(className).Append(" : ").Append(baseType).Append('\n');
            sb.Append(Indent).Append("{\n");
            sb.Append(Indent).Append(Indent).Append("public ").Append(className).Append('(').Append(parameters).Append(") : base(").Append(arguments).Append(")\n");
            sb.Append(Indent).Append(Indent).Append("{\n");
            sb.Append(Indent).Append(Indent).Append("}\n");
            sb.Append(Indent).Append("}\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// 事件模板文件。三个模板基类分别对应组队任务、单人、远征。
        /// </summary>
        public static string EventFile(string name, ScriptKindOption kind)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("using Application.Core.scripting.Events.Templates;\n");

            if (kind == ScriptKindOption.ExpeditionEvent)
            {
                sb.Append("using Application.Shared.Battle;\n");
            }

            sb.Append('\n');
            sb.Append("namespace Application.Plugin.Script.Features.").Append(name).Append('\n');
            sb.Append("{\n");
            sb.Append(Indent).Append("internal sealed class ").Append(EventClassName(kind)).Append(" : ").Append(EventBaseType(kind)).Append('\n');
            sb.Append(Indent).Append("{\n");

            var eventName = name + "Feature." + EventNameConstant(kind);

            if (kind == ScriptKindOption.ExpeditionEvent)
            {
                sb.Append(Indent).Append(Indent).Append("public ").Append(EventClassName(kind)).Append("() : base(0, ").Append(eventName).Append(", ExpeditionEntryType.None.Name)\n");
            }
            else
            {
                sb.Append(Indent).Append(Indent).Append("public ").Append(EventClassName(kind)).Append("() : base(").Append(eventName).Append(")\n");
            }

            sb.Append(Indent).Append(Indent).Append("{\n");
            sb.Append(Indent).Append(Indent).Append("}\n");
            sb.Append(Indent).Append("}\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static string EventBaseType(ScriptKindOption kind)
        {
            return kind switch
            {
                ScriptKindOption.PartyQuestEvent => "AbstractPartyQuestEventTemplate",
                ScriptKindOption.SoloEvent => "AbstractSoloEventTemplate",
                ScriptKindOption.ExpeditionEvent => "AbstractExpeditionEventTemplate",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "不是事件模板种类")
            };
        }

        static System.Text.StringBuilder Begin(string name, string[] usings)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var item in usings)
            {
                sb.Append("using ").Append(item).Append(";\n");
            }

            sb.Append('\n');
            sb.Append("namespace Application.Plugin.Script.Features.").Append(name).Append('\n');
            sb.Append("{\n");
            return sb;
        }
    }

    internal static class Scaffold
    {
        /// <summary>
        /// 命令行给定的冲突处理方式；<see cref="Ask"/> 表示逐个询问。
        /// </summary>
        enum ConflictResolution
        {
            Ask,
            Overwrite,
            Skip
        }

        static readonly System.Text.RegularExpressions.Regex NamePattern =
            new("^[A-Za-z_][A-Za-z0-9_]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

        public static int Run(string[] args)
        {
            if (args.Any(x => x is "-h" or "--help" or "/?"))
            {
                PrintUsage();
                return 0;
            }

            var options = CommandLineOptions.Parse(args);
            if (options == null)
            {
                PrintUsage();
                return 1;
            }

            var name = options.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("类别（如 OrbisPQ）：");
                name = Console.ReadLine()?.Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.Error.WriteLine("未提供类别。");
                return 1;
            }

            if (!NamePattern.IsMatch(name))
            {
                Console.Error.WriteLine($"类别必须是合法的 C# 标识符：{name}");
                return 1;
            }

            var kinds = options.Kinds.Count == 0 ? SelectKindsInteractively() : options.Kinds;
            if (kinds.Count == 0)
            {
                Console.Error.WriteLine("至少选择一种脚本类型。");
                return 1;
            }

            var featureRoot = options.FeatureRoot ?? LocateFeatureRoot();
            if (featureRoot == null)
            {
                Console.Error.WriteLine("找不到 src/Application.Plugin.Script/Features 目录，请在仓库内运行，或用 --features-dir 指定。");
                return 1;
            }

            return Write(name, kinds, featureRoot, options.ConflictPolicy);
        }

        /// <summary>
        /// 已存在同名文件时如何处理。
        /// </summary>
        enum ConflictChoice
        {
            Overwrite,
            Skip
        }

        static int Write(string name, IReadOnlyCollection<ScriptKindOption> kinds, string featureRoot, ConflictResolution resolution)
        {
            var targetDir = Path.Combine(featureRoot, name);
            var existed = Directory.Exists(targetDir);

            Directory.CreateDirectory(targetDir);

            var files = new List<(string Label, string Path, string Content)>
            {
                ("功能常量", Path.Combine(targetDir, $"{name}Feature.cs"), FeatureTemplates.FeatureFile(name, kinds))
            };

            foreach (var kind in kinds)
            {
                if (ScriptKinds.IsEvent(kind))
                {
                    files.Add((ScriptKinds.GetDisplayName(kind),
                        Path.Combine(targetDir, FeatureTemplates.EventFileName(name, kind)),
                        FeatureTemplates.EventFile(name, kind)));
                }
                else
                {
                    files.Add((ScriptKinds.GetDisplayName(kind),
                        Path.Combine(targetDir, FeatureTemplates.HandlerFileName(name, kind)),
                        FeatureTemplates.HandlerFile(name, kind)));
                }
            }

            ConflictChoice? globalChoice = resolution switch
            {
                ConflictResolution.Overwrite => ConflictChoice.Overwrite,
                ConflictResolution.Skip => ConflictChoice.Skip,
                _ => null
            };

            var written = 0;
            var overwritten = new List<string>();
            var skipped = new List<string>();

            foreach (var (label, path, content) in files)
            {
                var fileName = Path.GetFileName(path);

                if (File.Exists(path))
                {
                    var choice = globalChoice ?? AskConflict(label, fileName, out globalChoice);

                    if (choice == ConflictChoice.Skip)
                    {
                        skipped.Add(fileName);
                        Console.WriteLine("  = " + fileName + "（已存在，跳过）");
                        continue;
                    }

                    overwritten.Add(fileName);
                }

                File.WriteAllText(path, content, FeatureTemplates.FileEncoding);
                written++;
                Console.WriteLine("  + " + fileName);
            }

            Console.WriteLine();
            Console.WriteLine(existed
                ? $"目录已存在：{targetDir}"
                : $"已创建：{targetDir}");

            Console.WriteLine($"写入 {written} 个文件。");

            if (overwritten.Count > 0)
            {
                Console.WriteLine("已覆盖：" + string.Join(", ", overwritten));
            }

            if (skipped.Count > 0)
            {
                Console.WriteLine("已跳过：" + string.Join(", ", skipped));
            }

            return 0;
        }

        /// <summary>
        /// 逐个询问重复种类：覆盖还是跳过。输入 <c>a</c> / <c>q</c> 时把选择记回
        /// <paramref name="globalChoice"/>，后续同类重复项不再追问。
        /// 输入流结束（非交互环境）按跳过处理。
        /// </summary>
        static ConflictChoice AskConflict(string label, string fileName, out ConflictChoice? globalChoice)
        {
            while (true)
            {
                Console.Write($"  {label}：{fileName} 已存在，覆盖？[y=覆盖 / n=跳过 / a=全部覆盖 / q=全部跳过]（回车=跳过）：");

                var input = Console.ReadLine()?.Trim().ToLowerInvariant();

                switch (input)
                {
                    case "y":
                    case "yes":
                        globalChoice = null;
                        return ConflictChoice.Overwrite;
                    case "a":
                    case "all":
                        globalChoice = ConflictChoice.Overwrite;
                        return ConflictChoice.Overwrite;
                    case "q":
                    case "quit":
                        globalChoice = ConflictChoice.Skip;
                        return ConflictChoice.Skip;
                    case "":
                    case null:
                    case "n":
                    case "no":
                        globalChoice = null;
                        return ConflictChoice.Skip;
                    default:
                        Console.WriteLine("  无法识别，请输入 y / n / a / q。");
                        break;
                }
            }
        }

        /// <summary>
        /// 交互选择：输入编号（可空格/逗号分隔，支持 1-5 区间），或直接输入名字。
        /// 回车表示不选，a 表示全选。
        /// </summary>
        static List<ScriptKindOption> SelectKindsInteractively()
        {
            Console.WriteLine("选择脚本类型（可多选）：");

            for (var i = 0; i < ScriptKinds.All.Length; i++)
            {
                Console.WriteLine($"  {i + 1,2}. {ScriptKinds.GetDisplayName(ScriptKinds.All[i])}");
            }

            Console.WriteLine();
            Console.Write("编号（空格/逗号分隔，如 1 5 9；a=全选；回车=只生成 Feature 文件）：");

            var input = Console.ReadLine();
            var result = new List<ScriptKindOption>();

            if (string.IsNullOrWhiteSpace(input))
            {
                return result;
            }

            if (input.Trim().Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                return [.. ScriptKinds.All];
            }

            foreach (var token in input.Split([',', ' ', ';', '、'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryParseRange(token, out var from, out var to))
                {
                    for (var index = Math.Max(from, 1); index <= Math.Min(to, ScriptKinds.All.Length); index++)
                    {
                        if (!result.Contains(ScriptKinds.All[index - 1]))
                        {
                            result.Add(ScriptKinds.All[index - 1]);
                        }
                    }
                }
                else if (ScriptKinds.TryParse(token, out var kind))
                {
                    if (!result.Contains(kind))
                    {
                        result.Add(kind);
                    }
                }
                else
                {
                    Console.Error.WriteLine($"无法识别的选项：{token}");
                }
            }

            return result;
        }

        /// <summary>
        /// 支持区间写法（如 1-5）。
        /// </summary>
        static bool TryParseRange(string token, out int from, out int to)
        {
            from = to = 0;

            var dash = token.IndexOf('-');
            if (dash <= 0 || dash == token.Length - 1)
            {
                return false;
            }

            return int.TryParse(token[..dash], out from) && int.TryParse(token[(dash + 1)..], out to);
        }

        /// <summary>
        /// 从当前目录（找不到再从程序目录）向上查找 <c>src/Application.Plugin.Script/Features</c>。
        /// </summary>
        static string? LocateFeatureRoot()
        {
            foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                var dir = new DirectoryInfo(start);

                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "src", "Application.Plugin.Script", "Features");
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }

                    dir = dir.Parent;
                }
            }

            return null;
        }

        static void PrintUsage()
        {
            Console.WriteLine("用法：dotnet run tools/FeatureTemplateGenerator.cs -- [功能名] [--kinds a,b,c] [--features-dir <path>] [--force|--skip]");
            Console.WriteLine();
            Console.WriteLine("  功能名          不填则交互输入。目录名与命名空间末段都用它。");
            Console.WriteLine("  --kinds, -k     逗号分隔的脚本类型；不填则交互选择。");
            Console.WriteLine("  --features-dir  直接指定 Features 目录；不填则从当前目录向上查找。");
            Console.WriteLine("  --force, -f     同名文件一律覆盖，不再询问。");
            Console.WriteLine("  --skip          同名文件一律跳过，不再询问。");
            Console.WriteLine();
            Console.WriteLine("未给 --force/--skip 时，每遇到已存在的文件都会询问覆盖还是跳过（a=全部覆盖，q=全部跳过）。");
            Console.WriteLine();
            Console.WriteLine("脚本类型：" + string.Join(", ", ScriptKinds.All.Select(ScriptKinds.GetDisplayName)));
        }

        sealed class CommandLineOptions
        {
            public string? Name { get; private set; }

            public List<ScriptKindOption> Kinds { get; } = [];

            public string? FeatureRoot { get; private set; }

            public ConflictResolution ConflictPolicy { get; private set; } = ConflictResolution.Ask;

            /// <summary>
            /// 解析失败（未知参数、种类名非法、--force 与 --skip 同时给出）返回 null。
            /// </summary>
            public static CommandLineOptions? Parse(string[] args)
            {
                var result = new CommandLineOptions();

                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--force":
                        case "-f":
                            if (result.ConflictPolicy == ConflictResolution.Skip)
                            {
                                Console.Error.WriteLine("--force 与 --skip 不能同时使用。");
                                return null;
                            }

                            result.ConflictPolicy = ConflictResolution.Overwrite;
                            break;
                        case "--skip":
                            if (result.ConflictPolicy == ConflictResolution.Overwrite)
                            {
                                Console.Error.WriteLine("--force 与 --skip 不能同时使用。");
                                return null;
                            }

                            result.ConflictPolicy = ConflictResolution.Skip;
                            break;
                        case "--kinds":
                        case "-k":
                            if (++i >= args.Length || !result.AddKinds(args[i]))
                            {
                                return null;
                            }

                            break;
                        case "--features-dir":
                            if (++i >= args.Length)
                            {
                                return null;
                            }

                            result.FeatureRoot = Path.GetFullPath(args[i]);
                            break;
                        default:
                            if (args[i].StartsWith('-') || result.Name != null)
                            {
                                return null;
                            }

                            result.Name = args[i];
                            break;
                    }
                }

                return result;
            }

            bool AddKinds(string text)
            {
                foreach (var token in text.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!ScriptKinds.TryParse(token, out var kind))
                    {
                        Console.Error.WriteLine($"无法识别的脚本类型：{token}");
                        return false;
                    }

                    if (!Kinds.Contains(kind))
                    {
                        Kinds.Add(kind);
                    }
                }

                return true;
            }
        }
    }
}
