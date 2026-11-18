using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class ShipEllinNpc : NpcScriptBase
    {
        public ShipEllinNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task contimoveEreEli()
        {
            var contiMove = GetEventManager(ShipEllinFeature.EventName);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.ShipEllin_ToVictoria)))
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

        public async Task contimoveEliEre()
        {
            var contiMove = GetEventManager(ShipEllinFeature.EventName);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.ShipEllin_ToEreve)))
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
    }
}
