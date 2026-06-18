using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginMapObjectService : IPluginServiceBase
    {
        Task OnMapObjectEnterField(IMap map, IMapObject mapObject);
        Task OnMapObjectLeaveField(IMap map, IMapObject mapObject);
    }
}
