using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Utility.Exceptions;

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
            var option = await AskMenu("选择你的目的地。\r\n#L0##b废都广场#l\r\n#L1#进入建筑工地#l\r\n#L2#新叶城#l");
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
                        var text = "这是检票口。您将立即被带进去。您想使用哪张票？#b";
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
                        await SayOK("看起来你好像没有票！");
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
                await SayOK("无法与我对话");
                return;
            }
            if (subway.CanEnter)
            {
                if (await AskYesNo("列车已经进站,请出示你的票，这样我就可以让你进去了。乘坐时间不会很长，你会安全到达目的地的。你觉得怎么样？想要乘坐这趟列车吗？"))
                {
                    if (haveItem(p.Value.TicketItemId))
                    {
                        if (await subway.Enter(getPlayer()))
                        {
                            await gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
                        }
                    }
                    else
                    {
                        await SayOK("抱歉，您需要一张门票才能进入！");
                    }
                    return;
                }
            }
            else
            {
                await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
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
