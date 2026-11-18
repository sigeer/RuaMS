using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Utility.Exceptions;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class KerningTrainNpc : NpcScriptBase
    {
        public KerningTrainNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1052007
        public async Task subway_in()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.KerningTrain_PickDestination));
            switch (option)
            {
                case 0:
                    var contiMove = GetEventManager(KerningTrainFeature.EventName);
                    var r = await contiMove.StartInstance(getPlayer());
                    await SayOK(contiMove.HandleCreateInstanceResult(r, c));
                    break;
                case 1:
                    if (haveItem(4031036) || haveItem(4031037) || haveItem(4031038))
                    {
                        var text = this.GetTalkMessage(ScriptMessage.KerningTrain_HeresTicket);
                        for (var i = 0; i < 3; i++)
                        {
                            if (haveItem(4031036 + i))
                            {
                                text += "\r\n#b#L" + (i + 1) + "##t" + (4031036 + i) + "#";
                            }
                        }
                        var selectTicket = await AskMenu(text);
                        await gainItem(4031035 + selectTicket, -1);
                        await warp(103000897 + (selectTicket * 3), "st00");
                        return;
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.KerningTrain_SeemsThough));
                    }
                    break;
                case 2:
                    await ContiMove_NLC();
                    break;
                default:
                    break;
            }
        }

        async Task ContiMove_NLC()
        {
            var subway = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(Subway)) ?? throw new BusinessNotsupportException(nameof(Subway));
            var p = subway.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.KerningTrain_CantTalk));
                return;
            }
            if (subway.CanEnter)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.KerningTrain_TrainHas)))
                {
                    if (haveItem(p.Value.TicketItemId))
                    {
                        if (await subway.Enter(getPlayer()))
                        {
                            await gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.KerningTrain_BeginBoarding));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.KerningTrain_SorryNeed));
                    }
                    return;
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningTrain_BeginBoarding));
                return;
            }
        }

        // Npc: 9201068 
        public Task NLC_Move()
        {
            return ContiMove_NLC();
        }
    }
}
