using Application.Core.Game.Maps;

namespace Application.Core.Channel.ServerData
{
    public class MapObjectManager
    {
        readonly WorldChannel _server;

        PriorityQueue<AbstractMapObject, long> _objects = new();
        public MapObjectManager(WorldChannel server)
        {
            _server = server;
        }

        public void RegisterTimedMapObject(AbstractMapObject mapObj, long duration)
        {
            _objects.Enqueue(mapObj, _server.Node.getCurrentTime() + duration);
        }

        public void HandleRun()
        {
            var timeNow = _server.Node.getCurrentTime();
            List<AbstractMapObject> items = [];
            while (_objects.TryPeek(out var mapObj, out var expiredAt) && expiredAt <= timeNow)
            {
                _objects.TryDequeue(out mapObj, out expiredAt);
                items.Add(mapObj!);
            }

            if (items.Count > 0)
            {
                foreach (var obj in items)
                {
                    obj.MapRemove();
                }
            }
        }
    }
}
