using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    internal sealed class MagatiaPQReactorActHandler : ReactorActScriptBase
    {
        public MagatiaPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2612001 
        public async Task rnjItem0()
        {
            await dropItems();
        }


        // Reactor: 2612002 
        public async Task rnjItem1()
        {
            await dropItems();


        }


        // Reactor: 2612003 
        public async Task rnjItem2()
        {
            await dropItems();
        }

        // Reactor: 2619001 
        public async Task rnjScript0()
        {
            // TODO
            // 2619001



        }


        // Reactor: 2619002 
        public async Task rnjScript1()
        {
            // TODO
            // 2619002



        }
    }
}
