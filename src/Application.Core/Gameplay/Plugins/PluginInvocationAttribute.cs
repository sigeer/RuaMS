using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件服务的调用模式
    /// </summary>
    public enum PluginInvocationMode
    {
        /// <summary>
        /// 首次命中：同名请求由优先级最高的实现处理，只调用一次
        /// </summary>
        FirstMatch,

        /// <summary>
        /// 广播：每个事件通知所有实现
        /// </summary>
        Broadcast
    }

    /// <summary>
    /// 标注服务接口的调用模式，PluginManager 按此分发，防止调用方式与语义不符
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class PluginInvocationAttribute(PluginInvocationMode mode) : Attribute
    {
        public PluginInvocationMode Mode { get; } = mode;

        static readonly ConcurrentDictionary<Type, PluginInvocationMode?> _cache = new();

        /// <summary>
        /// 读取服务接口标注的调用模式（按类型缓存）；未标注返回 null
        /// </summary>
        public static PluginInvocationMode? GetMode(Type serviceType)
        {
            return _cache.GetOrAdd(serviceType, t => t.GetCustomAttribute<PluginInvocationAttribute>()?.Mode);
        }

        /// <summary>
        /// 服务接口是否标注了调用模式
        /// </summary>
        public static bool HasMode(Type serviceType)
        {
            return GetMode(serviceType) != null;
        }
    }
}
