using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.OrbisPQ
{
    internal sealed class OrbisPQReactorHitHandler : ReactorHitScriptBase
    {
        public OrbisPQReactorHitHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2008007 — 奥比斯 PQ 监狱机关：移动对应编号的陷阱
        public async Task r2008007()
        {
            // 机关名形如 lever1 / stone6，第 6 个字符即编号；
            // 抽奖地图里也有直接叫 "1"~"5" 的同类反应堆，取末位字符兜底。
            var name = getReactor().getName();
            var index = name.Length > 5 ? name[5] : name[^1];
            await getMap().moveEnvironment("trap" + index, 1);
        }
    }
}
