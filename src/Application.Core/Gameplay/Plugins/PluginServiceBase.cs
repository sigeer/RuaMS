using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
    public abstract class PluginServiceBase : IPluginServiceBase
    {
        readonly ILogger _logger;
        protected WorldChannelServer _node;

        public PluginServiceBase(WorldChannelServer node, string pluginName)
        {
            _node = node;
            _logger = LogFactory.GetLogger($"Plugin/{pluginName}");
        }
        public abstract ValueTask DisposeAsync();
        public abstract Task OnMounted();

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
