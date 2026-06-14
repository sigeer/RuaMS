using Application.Core.Channel;
using Application.Core.Game.Maps;
using server.maps;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 传送门脚本服务
    /// </summary>
    public interface IScriptPortalService : IPluginServiceBase
    {
        bool Enter(IChannelClient c, Portal p);
    }
}
