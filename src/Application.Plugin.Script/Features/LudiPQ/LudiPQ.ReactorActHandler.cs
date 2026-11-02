using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.LudiPQ
{
    internal sealed class LudiPQReactorActHandler : ReactorActScriptBase
    {
        public LudiPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }


        // Reactor: 2201000 
        public async Task ludiMob0()
        {
            await spawnMonster(9300011, 10);
        }


        // Reactor: 2201001 
        public async Task ludiMob1()
        {
            for (var i = 0; i < 3; i++)
            {
                await spawnMonster(9300007);
            }
        }


        // Reactor: 2201002 
        public async Task ludiMob2()
        {
            await mapMessage(5, "Rombard has been summoned somewhere in the map.");
            await spawnMonster(9300010, 1, -211);


        }

        // Reactor: 2201003 
        public async Task ludiBoss0()
        {
            if (getMapId() == 922010900)
            {
                await Pink("Alishar has been summoned.");
                await spawnMonster(9300012, 941, 184);
            }
            else if (getMapId() == 922010700)
            {
                await Pink("Rombard has been summoned somewhere in the map.");
                await spawnMonster(9300010, 1, -211);
            }

        }
    }
}
