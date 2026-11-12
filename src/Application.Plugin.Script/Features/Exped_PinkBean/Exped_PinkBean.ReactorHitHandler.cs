using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    internal sealed class Exped_PinkBeanReactorHitHandler : ReactorHitScriptBase
    {
        public Exped_PinkBeanReactorHitHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // 在Reactor中的name是 用于背景转换的反应器
        // Reactor: 2708000
        public async Task PinkBeenBack0()
        {
            var mapObj = getMap();
            await mapObj.killMonster(8820000);
        }
    }
}
