using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.NineSpirit
{
    internal sealed class NineSpiritReactorActHandler : ReactorActScriptBase
    {
        public NineSpiritReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2406000 
        public async Task hontaleNPC0()
        {
            await spawnNpc(2081008);
            await startQuest(100203);
            await this.LightBlue(this.GetTalkMessage(ScriptMessage.NineSpiritReactorActHandler_LightFlickers));
        }
    }
}
