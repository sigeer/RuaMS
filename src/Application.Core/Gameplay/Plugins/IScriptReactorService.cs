using server.maps;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// Reactor 脚本服务
    /// </summary>
    public interface IScriptReactorService : IPluginServiceBase
    {
        Task ReactorHit(IChannelClient c, Reactor r);
        Task ReactorAct(IChannelClient c, Reactor r);
        Task ReactorTouch(IChannelClient c, Reactor r);
        Task ReactorUntouch(IChannelClient c, Reactor r);
    }
}
