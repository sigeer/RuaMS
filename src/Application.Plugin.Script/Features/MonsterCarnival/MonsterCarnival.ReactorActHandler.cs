using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.MonsterCarnival
{
    internal sealed class MonsterCarnivalReactorActHandler : ReactorActScriptBase
    {
        public MonsterCarnivalReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }


        // Reactor: 9980000, 9980001 
        public async Task mcGuardian0()
        {
            await dispelAllMonsters(int.Parse(getReactor().getName().Substring(1, 2)), int.Parse(getReactor().getName().Substring(0, 1)));
        }

    }
}
