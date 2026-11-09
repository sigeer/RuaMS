using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    internal sealed class TreasurePQReactorActHandler : ReactorActScriptBase
    {
        public TreasurePQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 6741001 
        public async Task guyfawkesmob0()
        {
            await spawnMonster(9400589);


        }


        // Reactor: 6741015 
        public async Task guyfawkesmob1()
        {
            await dropItems();


        }


        // Reactor: 6741016 
        public async Task guyfawkesmob2()
        {
            // TODO
            // 6741016



        }


        // Reactor: 6742014 
        public async Task guyfawkesbox0()
        {
            await dropItems(true, 1, 5, 25, 15);


        }
    }
}
