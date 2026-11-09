using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.NineSpirit
{
    internal sealed class NineSpiritReactorActHandler : ReactorActScriptBase
    {
        public NineSpiritReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2406000 
        public async Task hontaleNPC0()
        {
            await spawnNpc(2081008);
            await startQuest(100203);
            await LightBlue("光芒闪烁间，龙蛋破壳而出，一只璀璨的幼龙降临世间！");
        }
    }
}
