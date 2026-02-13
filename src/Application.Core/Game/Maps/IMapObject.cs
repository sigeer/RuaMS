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

        /// <summary>
        /// 地图对象加入到地图
        /// </summary>
        /// <param name="map"></param>
        /// <param name="chrAction"></param>
        void Enter(IMap map, Action<Player> chrAction);
        /// <summary>
        /// 地图对象从地图上移除
        /// </summary>
        /// <param name="chrAction"></param>
        void Leave(Action<Player> chrAction);
    }
}
