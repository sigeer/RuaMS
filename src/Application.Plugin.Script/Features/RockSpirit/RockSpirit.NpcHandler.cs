using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.RockSpirit
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1052125 
        public async Task Depart_topFloorIn()
        {

            var option = await AskMenu("等一下！由于装修，该区域的进入受到限制。我只能允许符合特定条件的人进入这里。#b\n\r\n#L0#我现在正在帮助#eBlake#n。#l\r\n#L1#我是这家购物中心的#rVIP#b！#l");
            switch (option)
            {
                case 0:
                    if (isQuestStarted(2286) || isQuestStarted(2287) || isQuestStarted(2288))
                    {
                        var em = GetEventManager(RockSpiritFeature.EventName);
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        return;
                    }
                    else
                    {
                        await SayOK("我没有听到布莱克说你在帮助他。");
                    }
                    break;
                case 1:
                    if (isQuestCompleted(2290))
                    {
                        if (getPlayer().getLevel() > 50)
                        {
                            await SayOK("VIP区域仅供50级或以下的玩家使用。");
                        }
                        else
                        {
                            await SayOK("VIP区域只有在完成“进入VIP区域”的任务并交出#r#t4032521#s#k后才能进入。");
                        }
                    }
                    else
                    {
                        await SayOK("#rVIP#k？是的，这很有趣 #rVIP先生#k，现在赶紧滚开，否则我就叫保安了。");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
