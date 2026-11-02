using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.EllinPQ
{
    internal sealed class EllinPQReactorActHandler : ReactorActScriptBase
    {
        public EllinPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 3001000 
        public async Task pFBoss()
        {
            await Pink("Poison Golem has been spawned.");
            await spawnMonster(9300180, 1);


        }


        // Reactor: 3002000 
        public async Task pFItem0()
        {
            await dropItems();


        }


        // Reactor: 3002001 
        public async Task pFItem1()
        {
            await GetEventInstanceTrust().showClearEffect(getMap().getId());
            await dropItems();


        }


        // Reactor: 3008000 
        public async Task pFBack0()
        {
            var eim = getEventInstance();
            if (eim != null)
                await eim.giveEventPlayersExp(52000, getMapId());
        }
    }
}
