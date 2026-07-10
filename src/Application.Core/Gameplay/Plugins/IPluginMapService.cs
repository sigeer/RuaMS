using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginMapService : IPluginServiceBase
    {
        Task OnMapLoad(IMap map);
        Task OnMapUnload(IMap map);
    }
}
