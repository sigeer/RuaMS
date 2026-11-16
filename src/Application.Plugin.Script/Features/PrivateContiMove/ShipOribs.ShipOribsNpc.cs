using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class ShipOribsNpc : NpcScriptBase
    {
        public ShipOribsNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task contimoveEreOrb()
        {
            var contiMove = GetEventManager(ShipOribsFeature.EventName);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.ShipOribs_ToOrbis)))
            {
                if (getMeso() < 1000)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.ShipEllin_NoMeso, 1000));
                }
                else
                {
                    var r = await contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        await gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.ShipEllin_ChangedMind));
            }

        }

        public async Task contimoveOrbEre()
        {
            var contiMove = GetEventManager(ShipOribsFeature.EventName);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.ShipOribs_ToEreve)))
            {
                var meso = 1000;
                if (getMeso() < meso)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.ShipEllin_NoMeso, meso));
                }
                else
                {
                    var r = await contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        await gainMeso(-meso);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.ShipEllin_ChangedMind));
            }
        }
    }
}
