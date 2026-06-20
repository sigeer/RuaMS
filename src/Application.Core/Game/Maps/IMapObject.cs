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
        Task sendSpawnData(IChannelClient client);
        Task sendDestroyData(IChannelClient client);

        /// <summary>
        /// 添加到地图时
        /// </summary>
        /// <param name="map"></param>
        Task OnMounted(IMap map);
        /// <summary>
        /// 从地图移除后
        /// </summary>
        Task OnUnmounted();
        /// <summary>
        /// 可以对 <paramref name="chr"/> 显示
        /// </summary>
        /// <param name="chr"></param>
        bool IsVisibleForPlayer(Player chr);
        Task BroadcastMap(Packet packet, int exceptCId = -1);
    }
}
