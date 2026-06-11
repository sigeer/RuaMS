using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IMapListener : IPluginServiceBase
    {
        void OnMapObjectEnterField(IMap map, IMapObject mapObject);
        void OnMapObjectLeaveField(IMap map, IMapObject mapObject);
    }
}
