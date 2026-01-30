using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MapObjectRemoveCommand : IWorldChannelCommand
    {
        List<AbstractMapObject> toRemove;
        public MapObjectRemoveCommand(List<AbstractMapObject> mapObjects)
        {
            toRemove = mapObjects;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var obj in toRemove)
            {
                obj.MapRemove();
            }
        }
    }
}
