using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 插件服务优先级（数值越小优先级越高），用于 FirstMatch（首次命中）模式下同名脚本的裁决
    /// </summary>
    public enum PluginPriority: byte
    {
        /// <summary>最高优先级：优先于所有常规插件命中</summary>
        High = 0,

        /// <summary>常规优先级（默认）</summary>
        Normal = 1,

        /// <summary>最低优先级：兜底使用，默认脚本插件（Application.Plugin.Script）采用此档</summary>
        Low = 2
    }

    public abstract class PluginServiceBase : IPluginServiceBase
    {
        readonly ILogger _logger;
        protected WorldChannelServer _node;
        protected string _pluginName;
        public PluginServiceBase(WorldChannelServer node, string pluginName)
        {
            _node = node;
            _pluginName = pluginName;
            _logger = LogFactory.GetLogger($"Plugin/{pluginName}");
        }

        /// <summary>
        /// 插件服务优先级，默认 <see cref="PluginPriority.Normal"/>。
        /// 仅对 FirstMatch（首次命中）模式生效：同名脚本冲突时由优先级最高的插件处理。
        /// 默认脚本插件使用 <see cref="PluginPriority.Low"/>，第三方插件以默认 <see cref="PluginPriority.Normal"/> 即可覆盖默认脚本。
        /// </summary>
        public virtual PluginPriority Priority => PluginPriority.Normal;

        public abstract ValueTask DisposeAsync();
        public virtual Task OnMounted()
        {
            _logger.Information("{Plugin} 已加载", _pluginName);
            return Task.CompletedTask;
        }

        protected void LogInfo(string p, params object?[] t)
        {
            _logger.Information(p, t);
        }

        protected void LogError(string p, params object?[] t)
        {
            _logger.Error(p, t);
        }

        protected void LogError(Exception ex, string p, params object?[] t)
        {
            _logger.Error(ex, p, t);
        }

        protected void LogWarning(string p, params object?[] t)
        {
            _logger.Warning(p, t);
        }
    }
}
