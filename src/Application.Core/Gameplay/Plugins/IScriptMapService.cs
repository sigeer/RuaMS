using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 地图脚本服务
    /// </summary>
    public interface IScriptMapService : IPluginServiceBase
    {
        Task MapFirstEnter(IChannelClient c, IMap map);
        Task MapEnter(IChannelClient c, IMap map);
    }
}
