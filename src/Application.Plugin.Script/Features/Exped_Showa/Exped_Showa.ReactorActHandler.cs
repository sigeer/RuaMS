using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Exped_Showa
{
    internal sealed class Exped_ShowaReactorActHandler : ReactorActScriptBase
    {
        public Exped_ShowaReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 8001000 
        public async Task shouwaBoss()
        {
            await spawnMonster(9400112, 1, 420, 160);
        }

    }
}
