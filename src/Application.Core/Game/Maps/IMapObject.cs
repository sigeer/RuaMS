namespace Application.Core.Game.Maps
{
    public interface IMapObject
    {
        public IMap MapModel { get; }
        string GetName();
        string GetReadableName(IChannelClient c);
        int GetSourceId();
        void setMap(IMap map);
        IMap getMap();
        Point getPosition();
        int getObjectId();
        void setObjectId(int id);
        MapObjectType getType();
        void setPosition(Point position);
        void sendSpawnData(IChannelClient client);
        void sendDestroyData(IChannelClient client);
        void nullifyPosition();
    }
}
