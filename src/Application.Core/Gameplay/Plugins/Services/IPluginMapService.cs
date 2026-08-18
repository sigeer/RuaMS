using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    [PluginInvocation(PluginInvocationMode.Broadcast)]
    public interface IPluginMapService : IPluginServiceBase
    {
        Task OnMapLoad(IMap map);
        Task OnMapUnload(IMap map);
    }
}
