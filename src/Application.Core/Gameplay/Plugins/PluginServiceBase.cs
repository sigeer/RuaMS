using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
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
