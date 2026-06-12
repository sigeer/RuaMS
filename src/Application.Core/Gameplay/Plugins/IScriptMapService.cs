using Application.Core.Channel;
using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 地图脚本服务
    /// </summary>
    public interface IScriptMapService : IPluginServiceBase
    {
        void MapFirstEnter(IChannelClient c, IMap map);
        void MapEnter(IChannelClient c, IMap map);
    }
}
