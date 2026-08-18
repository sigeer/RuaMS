using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    [PluginInvocation(PluginInvocationMode.Broadcast)]
    public interface IPluginMapObjectService : IPluginServiceBase
    {
        Task OnMapObjectEnterField(IMap map, IMapObject mapObject);
        Task OnMapObjectLeaveField(IMap map, IMapObject mapObject);
    }
}
