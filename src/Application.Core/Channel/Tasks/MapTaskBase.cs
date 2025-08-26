using Application.Core.Game.Maps;

namespace Application.Core.Channel.Tasks
{
    internal class MapTaskBase : NamedRunnable
    {
        public MapTaskBase(IMap map, string taskName, Action action) : base($"{map.InstanceName}_{taskName}", action)
        {
        }
    }
}
