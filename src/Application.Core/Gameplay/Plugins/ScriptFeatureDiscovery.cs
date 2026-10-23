using Application.Core.scripting.Events.Templates;
using Application.Core.scripting.item;
using Application.Core.scripting.npc;
using Application.Core.scripting.quest;
using scripting.map;
using scripting.portal;
using scripting.reactor;
using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件侧的类型扫描工具：扫描整个程序集，收集其中声明了脚本的类型。
    ///
    /// 脚本种类完全由基类判定：每个 <see cref="ScriptKind"/> 对应唯一的脚本上下文基类。
    /// 它只回答「这个程序集提供了哪些脚本」，不持有状态、不做跨插件裁决——
    /// 合并、优先级、重名处理全部由 <see cref="PluginManager"/> 负责。
    /// </summary>
    public static class ScriptFeatureDiscovery
    {
        /// <summary>
        /// 脚本上下文基类 → 脚本种类。每个种类对应唯一的基类，据此判定处理器种类。
        /// </summary>
        static readonly Dictionary<Type, ScriptKind> _kindByBaseType = new()
        {
            [typeof(NpcScriptBase)] = ScriptKind.Npc,
            [typeof(QuestScriptBase)] = ScriptKind.Quest,
            [typeof(ItemScriptBase)] = ScriptKind.Item,
            [typeof(PortalPlayerInteraction)] = ScriptKind.Portal,
            [typeof(MapScriptMethods)] = ScriptKind.Map,
            [typeof(ReactorHitScriptBase)] = ScriptKind.ReactorHit,
            [typeof(ReactorActScriptBase)] = ScriptKind.ReactorAct,
            [typeof(ReactorTouchScriptBase)] = ScriptKind.ReactorTouch,
            [typeof(ReactorUntouchScriptBase)] = ScriptKind.ReactorUntouch,
        };

        /// <summary>
        /// 已登记的全部脚本上下文基类。
        /// </summary>
        public static IReadOnlyCollection<Type> ScriptBaseTypes => [.. _kindByBaseType.Keys];

        /// <summary>
        /// 扫描整个程序集，返回其中声明了脚本的处理器。
        /// 抽象类跳过（它们只承载基类逻辑）。
        /// </summary>
        public static IReadOnlyCollection<ScriptHandlerDescriptor> CollectHandlers(Assembly assembly, PluginServiceBase source)
        {
            var result = new List<ScriptHandlerDescriptor>();

            foreach (var type in assembly.GetTypes())
            {
                result.AddRange(CollectHandlers(type, source));
            }

            return result;
        }

        /// <summary>
        /// 扫描单个类型的脚本处理器。
        /// 只取该类自己声明的方法——处理器都直接继承框架脚本上下文基类，
        /// 不依赖「基类持有方法、子类提供配置」的写法。
        /// </summary>
        public static IReadOnlyCollection<ScriptHandlerDescriptor> CollectHandlers(Type handlerType, PluginServiceBase source)
        {
            if (handlerType.IsAbstract || !TryResolveKind(handlerType, out var kind))
                return [];

            return [.. TypeUtils.LoadFromType(handlerType)
                .Select(x => new ScriptHandlerDescriptor(kind, x.Key, handlerType, x.Value.Method, source))];
        }

        /// <summary>
        /// 扫描整个程序集中的事件模板，返回其实例。
        /// 事件模板以类型为界，与命名空间、脚本种类都无关。
        /// </summary>
        public static IReadOnlyList<AbstractEventTemplate> CreateEventTemplates(Assembly assembly)
        {
            return CreateEventTemplates(assembly.GetTypes());
        }

        /// <summary>
        /// 从给定类型中挑出事件模板并实例化。
        /// </summary>
        public static IReadOnlyList<AbstractEventTemplate> CreateEventTemplates(IEnumerable<Type> types)
        {
            return [.. types
                .Where(x => !x.IsAbstract && typeof(AbstractEventTemplate).IsAssignableFrom(x))
                .Select(x => (AbstractEventTemplate)Activator.CreateInstance(x, nonPublic: true)!)];
        }

        /// <summary>
        /// 判断一个类型提供哪种脚本。
        ///
        /// 处理器一律**直接**继承对应种类的脚本上下文基类（本仓库 114 个处理器实测距离恒为 1），
        /// 因此只需看一层 <see cref="Type.BaseType"/>，不必沿基类链回溯。
        /// 中间基类（<see cref="MapScriptMethods"/> 等）的存在就是为了让每个种类有唯一的直接父类。
        /// </summary>
        public static bool TryResolveKind(Type type, out ScriptKind kind)
        {
            if (type.BaseType != null && _kindByBaseType.TryGetValue(type.BaseType, out kind))
                return true;

            kind = default;
            return false;
        }
    }
}
