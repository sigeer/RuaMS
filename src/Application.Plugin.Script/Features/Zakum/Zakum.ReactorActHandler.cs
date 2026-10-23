using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class ZakumReactorActHandler : ReactorActScriptBase
    {
        public ZakumReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2111001 
        public async Task boss()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.setProperty("summoned", "true");
                eim.setProperty("canEnter", "false");
            }
            await changeMusic("Bgm06/FinalFight");
            await SpawnZakum();

            await Pink("Zakum is summoned by the force of Eye of Fire.");
        }
    }
}
