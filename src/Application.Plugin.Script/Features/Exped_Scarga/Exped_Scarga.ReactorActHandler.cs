using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Exped_Scarga
{
    internal sealed class Exped_ScargaReactorActHandler : ReactorActScriptBase
    {
        public Exped_ScargaReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 5511000 
        public async Task myboss0()
        {
            var targaMobId = 9420542;
            if (getMap().getMonsterById(targaMobId) == null)
            {
                await summonBossDelayed(targaMobId, 3200, -527, 637, "Bgm09/TimeAttack", "Beware! The furious Targa has shown himself!");
            }
        }


        // Reactor: 5511001 
        public async Task myboss1()
        {
            var scarlionMobId = 9420547;
            if (getMap().getMonsterById(scarlionMobId) == null)
            {
                await summonBossDelayed(scarlionMobId, 3200, -238, 636, "Bgm09/TimeAttack", "Beware! The furious Scarlion has shown himself!");
            }
        }
    }
}
