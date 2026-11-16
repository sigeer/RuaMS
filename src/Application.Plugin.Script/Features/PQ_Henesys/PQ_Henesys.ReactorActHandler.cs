using Application.Resources.Messages;
using Application.Core.Channel.Net.Packets;
using Application.Core.Client;
using Application.Core.Server.maps;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Mob;
using scripting.reactor;

namespace Application.Plugin.Script.Features.PQ_Henesys
{
    internal sealed class PQ_HenesysReactorActHandler : ReactorActScriptBase
    {
        public PQ_HenesysReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 9101000 
        public async Task moonMob0()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.StartStage(getMapId());
                await spawnMonster(MobId.MOON_BUNNY, 1, -183, -433); // (0, 0) is temp position
                await getMap().startMapEffect(this.GetTalkMessage(ScriptMessage.PQHenesysReactorActHandler_MoonBunny), ItemId.Weather_HenesysPQ, 7000);
                await getMap().broadcastMessage(MessagePacket.SystemMessage(getClient().CurrentCulture.GetTalkMessage(ScriptMessage.PQHenesys_ProtectMoonBunny))); // Protect the Moon Bunny!
                getMap().allowSummonState(true);
            }
        }

        // Reactor: 9102002
        [ScriptName("moonItem0", "moonItem1", "moonItem2", "moonItem3", "moonItem4", "moonItem5")]
        public async Task MoonItem0()
        {
            await dropItems();
        }
    }
}
