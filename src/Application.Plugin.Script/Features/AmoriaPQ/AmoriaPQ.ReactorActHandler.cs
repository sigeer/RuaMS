using Application.Core.Client;
using Application.Core.Server.life;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.AmoriaPQ
{
    internal sealed class AmoriaPQReactorActHandler : ReactorActScriptBase
    {
        public AmoriaPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 6701000
        [ScriptName("amoriaboxMob0", "amoriaboxMob1", "amoriaboxMob2")]
        public async Task AmoriaboxMob()
        {
            var startId = 9400523;
            var mapObj = getMap();
            for (var i = 0; i < 7; i++)
            {
                var mobObj = LifeFactory.Instance.getMonster(startId + Random.Shared.Next(3));
                await mapObj.spawnMonsterOnGroundBelow(mobObj, getReactor().getPosition());
            }
        }

        // Reactor: 6702000 
        public async Task amoriaItem0()
        {
            await dropItems();


        }


        // Reactor: 6702001 
        public async Task amoriaItem1()
        {
            // TODO
            // 6702001



        }


        // Reactor: 6702002 
        public async Task amoriaItem2()
        {
            // TODO
            // 6702002



        }

        [ScriptName("amoriaItem3", "amoriaItem4", "amoriaItem5", "amoriaItem6", "amoriaItem7", "amoriaItem8",
            "amoriaItem9", "amoriaItem10", "amoriaItem11", "amoriaItem12")]
        public async Task AmoriaItem()
        {
            var count = Math.Max(1, Random.Shared.Next(4));
            //We'll make it drop a lot of crap :D
            for (var i = 0; i < count; i++)
            {
                await dropItems(true, 1, 30, 60, 15);
            }


        }

    }
}
