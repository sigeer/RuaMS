using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Thief4th
{
    internal sealed class Thief4thReactorActHandler : ReactorActScriptBase
    {
        public Thief4thReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 1020000 
        public async Task s4hitmanMap0()
        {
            await warp(910200000, "pt00");
        }


        // Reactor: 1020001 
        public async Task s4hitmanMap1()
        {
            await warp(910200000, "pt01");
        }


        // Reactor: 1020002 
        public async Task s4hitmanMap2()
        {
            await warp(910200000, "pt02");
        }


        // Reactor: 1020003 
        public async Task s4hitmanMap3()
        {
            // TODO
            // 1020003



        }


        // Reactor: 1020004 
        public async Task s4hitmanMap4()
        {
            // TODO
            // 1020004



        }


        // Reactor: 1020005 
        public async Task s4hitmanMap5()
        {
            // TODO
            // 1020005



        }


        // Reactor: 1020006 
        public async Task s4hitmanMap6()
        {
            // TODO
            // 1020006



        }


        // Reactor: 1020007 
        public async Task s4hitmanMap7()
        {
            // TODO
            // 1020007



        }


        // Reactor: 1020008 
        public async Task s4hitmanMap8()
        {
            // TODO
            // 1020008



        }


        // Reactor: 1021000 
        public async Task s4hitmanMob0()
        {
            await spawnMonster(9300091);


        }


        // Reactor: 1021001 
        public async Task s4hitmanMob1()
        {
            await spawnMonster(9300091);


        }


        // Reactor: 1022000 
        public async Task s4hitmanItem0()
        {
            await dropItems();


        }
    }
}
