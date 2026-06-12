using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginLifeService : IPluginServiceBase
    {
        void OnMounted(WorldChannelServer node);
        void OnUnmounted(WorldChannelServer node);
    }

    public interface IPluginChannelLifeService : IPluginServiceBase
    {
        void OnChannelMounted(WorldChannel channel);
        void OnChannelUnmounted(WorldChannel channel);
    }
}
