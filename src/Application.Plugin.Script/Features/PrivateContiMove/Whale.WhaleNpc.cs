using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class WhaleNpc : NpcScriptBase
    {
        public WhaleNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task contimoveRieRit()
        {
            var contiMove = GetEventManager(WhaleFeature.EventName);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Whale_Offer)))
            {
                if (haveItem(4032338))
                {
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Whale_Offer),
                        this.GetTalkMessage(ScriptMessage.Whale_FreeRide)
                        ], current: 1);

                    var r = await contiMove.StartInstance(getPlayer());
                    await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Whale_Cheap));

                    if (getLevel() >= 8)
                    {
                        if (getMeso() < 80)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Whale_FreeShip));
                            return;
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Whale_Paid));


                            var r = await contiMove.StartInstance(getPlayer());
                            if (r == CreateInstanceResult.Success)
                            {
                                await gainMeso(-80);
                            }
                            else
                            {
                                await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                            }
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Whale_TooWeak));
                        return;
                    }
                }

            }
        }

        public async Task contimoveRitRie()
        {
            var contiMove = GetEventManager(WhaleFeature.EventName);

            await SayNext(this.GetTalkMessage(ScriptMessage.Whale_ReturnOffer));
            if (getMeso() < 800)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Whale_NoMeso));
            }
            else
            {
                var r = await contiMove.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await gainMeso(-800);
                }
                else
                {
                    await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                }
            }
        }
    }
}
