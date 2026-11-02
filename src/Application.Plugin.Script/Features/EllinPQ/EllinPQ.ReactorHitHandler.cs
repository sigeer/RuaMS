using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.EllinPQ
{
    internal sealed class EllinPQReactorHitHandler : ReactorHitScriptBase
    {
        public EllinPQReactorHitHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
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
