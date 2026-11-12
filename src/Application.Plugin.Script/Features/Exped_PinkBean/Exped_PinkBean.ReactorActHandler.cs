using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    internal sealed class Exped_PinkBeanReactorActHandler : ReactorActScriptBase
    {
        public Exped_PinkBeanReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // 调用方：2141000不存在
        // Reactor: 2709000 — 粉豆召唤：移除占位怪物，由事件模板生成真正的粉豆
        public async Task PinkBeenScript0()
        {
            await getMap().killMonster(8820000);
        }

    }
}
