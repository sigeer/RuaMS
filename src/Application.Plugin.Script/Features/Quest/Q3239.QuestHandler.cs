using Application.Core.Client;
using Application.Shared.Constants.Inventory;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3239QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q3239QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 3239 
        public async Task q3239e()
        {
            if (haveItem(4031092, 10))
            {
                if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() >= 1)
                {
                    await gainItem(4031092, -10);

                    var rnd = Random.Shared.Next(4);
                    if (rnd == 0)
                    {
                        await gainItem(2040704, 1);
                    }
                    else if (rnd == 1)
                    {
                        await gainItem(2040705, 1);
                    }
                    else if (rnd == 2)
                    {
                        await gainItem(2040707, 1);
                    }
                    else
                    {
                        await gainItem(2040708, 1);
                    }

                    await gainExp(2700);
                    await forceCompleteQuest();

                    await SayOK("干得好！你带回了所有丢失的#t4031092#。在这里，拿着这卷轴作为我的谢意……");
                }
                else
                {
                    await SayOK("在领取奖品之前，请确保消耗栏有一个空位.");
                }
            }
            else
            {
                await SayOK("请归还我丢失在这个房间的10个#t4031092#.");
            }
        }
    }
}
