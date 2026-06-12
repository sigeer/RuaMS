using Application.Core.scripting.Events.Abstraction;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2133000 
        public async Task party6_entry()
        {
            var em = GetEventManager(nameof(PQ_Ellin));
            var option = await AskMenu("#e#b<组队任务：毒雾森林>\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n你想要组建或加入一个队伍来解决#b毒雾森林#k的谜题吗？让你的#b队长#k和我交谈或者自己组建一个队伍。",
                ["我想参加组队任务。", "我想了解更多细节。", "我想要领取奖励。"]);

            switch (option)
            {
                case 0:
                    var r = em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                    break;
                default:
                    break;
            }
        }


        // Npc: 2133001 
        public async Task party6_elin()
        {
            var eim = GetEventInstanceTrust();

            var curStage = eim.ClearedMaps.GetValueOrDefault(getMapId(), StageStatus.NotStarted);
            if (getMapId() == 930000000)
            {
                await SayNext("Welcome to the Forest of Poison Haze. Proceed by entering the portal.");
                return;
            }
            else if (getMapId() == 930000100)
            {
                await SayNext("The #b#o9300172##k have taken the area. We have to eliminate all these contaminated monsters to proceed further.");
                return;
            }

            else if (getMapId() == 930000300)
            {
                if (curStage == StageStatus.NotStarted)
                {
                    eim.showClearEffect(getMapId());
                    eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                }

                await SayNext("哦，太好了，你找到我了。我们现在可以在森林里继续前进了。");
                eim.warpEventTeam(930000400);
            }
            else if (getMapId() == 930000400)
            {
                if (haveItem(4001169, 20))
                {
                    if (isEventLeader())
                    {
                        await SayNext("哦，你带来了它们！我们现在可以继续了，我们要继续吗？");

                        if (haveItem(4001169, 20) && isEventLeader())
                        {
                            gainItem(4001169, -20);
                            eim.warpEventTeam(930000500);
                        }
                    }
                    else
                    {
                        await SayOK("你已经带来了他们，但你不是队长！请让队长把#t4001169#给我……");
                        return;
                    }
                }
                else if (!haveItem(2270004))
                {
                    if (canHold(2270004, 10))
                    {
                        gainItem(2270004, 10);
                        await SayOK("拿10个#t2270004#。首先，#r削弱#o9300174#的力量，一旦它的生命值降低，使用我给你的物品来捕捉它们。");
                        return;

                    }
                    else
                    {
                        await SayOK("在领取#t2270004#之前，请确保你的使用物品栏有足够的空间！");
                        return;
                    }
                }
                else
                {

                }
            }
        }


        // Npc: 2133002 
        public async Task party6_giveUp()
        {
            if (await AskYesNo("你想要退出这个副本吗？你的队友可能也需要放弃，所以要考虑一下。"))
            {
                WarpOut();
            }
        }
        // Npc: 2133004 
        public async Task party6_spra()
        {
            if (!haveItem(4001163) || !isEventLeader())
            {
                if (await AskYesNo("让你的队长在这里给我看#t4001163#。\r\n\r\n或者你想要#r离开这片森林#k吗？现在离开意味着抛弃你的伙伴，记住这一点。"))
                {
                    WarpOut();
                }
            }
            else
            {
                await SayNext("太好了，你有了#t4001163#。我会带你们去通往石头祭坛的路。跟我来吧。");
                GetEventInstanceTrust().warpEventTeam(930000600);
            }
        }
    }
}
