using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;
using System.Drawing;

namespace Application.Plugin.Script.Features.Horntail
{
    internal sealed class HorntailReactorActHandler : ReactorActScriptBase
    {
        public HorntailReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2401000 
        public async Task hontaleBoss()
        {
            await changeMusic("Bgm14/HonTale");
            if (getMap().getMonsterById(8810026) == null)
            {
                await getMap().spawnHorntailOnGroundBelow(new Point(71, 260));

                var eim = GetEventInstanceTrust();
                await eim.restartEventTimer(60 * 60000);
            }
            await mapMessage(6, "From the depths of his cave, here comes Horntail!");
        }

        // Reactor: 2402002 
        public async Task hontaleItem0()
        {
            // TODO
            // 2402002



        }


        // Reactor: 2402003 
        public async Task hontaleItem1()
        {
            // TODO
            // 2402003



        }


        // Reactor: 2402004 
        public async Task hontaleItem2()
        {
            // TODO
            // 2402004



        }


        // Reactor: 2402005 
        public async Task hontaleItem3()
        {
            // TODO
            // 2402005



        }


        // Reactor: 2402006 
        public async Task hontaleItem4()
        {
            await dropItems();


        }
    }
}
