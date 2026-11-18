using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.PapulatusBattle
{
    internal sealed class PapulatusBattleReactorActHandler : ReactorActScriptBase
    {
        public PapulatusBattleReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2201004 
        public async Task boss2()
        {
            await this.Pink(this.GetTalkMessage(ScriptMessage.PapulatusBattleReactorActHandler_DimensionalRift));
            await changeMusic("Bgm09/TimeAttack");
            await spawnMonster(8500000, -410, -400);
        }

    }
}
