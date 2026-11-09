using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.NineSpirit
{
    internal sealed class NineSpiritReactorTouchHandler : ReactorTouchScriptBase
    {
        public NineSpiritReactorTouchHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2406000 — 龙蛋：持有 4001094 且处于初始状态时触发
        public async Task hontaleNPC0()
        {
            if (haveItem(4001094) && getReactor().getState() == 0)
            {
                await hitReactor();
                await gainItem(4001094, -1);
            }
        }
    }
}
