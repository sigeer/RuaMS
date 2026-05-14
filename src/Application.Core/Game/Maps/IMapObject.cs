namespace Application.Core.Game.Maps
{
    public interface IMapObject
    {
        public IMap MapModel { get; }
        string GetName();
        string GetReadableName(IChannelClient c);
        int GetSourceId();
        IMap getMap();
        Point getPosition();
        int getObjectId();
        void setObjectId(int id);
        MapObjectType getType();
        void setPosition(Point position);
        void sendSpawnData(IChannelClient client);
        void sendDestroyData(IChannelClient client);

        /// <summary>
        /// 添加到地图时
        /// </summary>
        /// <param name="map"></param>
        void OnMounted(IMap map);
        /// <summary>
        /// 从地图移除时
        /// </summary>
        void OnUnmounted();
    }
}
