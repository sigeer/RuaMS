using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    internal sealed class Exped_BalrogReactorActHandler : ReactorActScriptBase
    {
        public Exped_BalrogReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 1052001, 1052002, 1052003 
        public async Task balogItem0()
        {
            await dropItems(true, 1, 500, 1000, 15);
        }


        // Reactor: 1058001, 1058003, 1058004, 1058011, 1058013, 1058014 
        public async Task minibalogSummon()
        {
            // TODO
            // 1058001

            // 1058003

            // 1058004

            // 1058011

            // 1058013

            // 1058014



        }


        // Reactor: 1058005 
        public async Task balogReactor()
        {
            // TODO
            // 1058005
        }

        // 1058015
        public async Task Easy_balogReactor()
        {
            //var eim = getEventInstance();
            //if (eim == null || eim.EventName != Exped_BalrogFeature.EasyEventName)
            //{
            //    return;
            //}

            //await Exped_BalrogFeature.SummonEasy(eim, getMap());
        }
    }
}
