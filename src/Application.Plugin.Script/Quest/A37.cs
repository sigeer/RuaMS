using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;

namespace Application.Plugin.Script.Quest
{

    // 时间停止之湖
    internal partial class QuestScript
    {
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
        // Quest: 3414 
        public async Task q3414e()
        {
            await SayNext("呵呵呵…！就是这个！只要有这个样本，现在正在地球防卫总部进行的研究，就更加活跃了！不过，没想到还会有比我更出色的人啊…我还需要更努力呀！不管怎么说，这样帮了我大忙，应该给点酬劳才行。");

            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() < 1)
            {
                await Popup("USE inventory full.");
                return;
            }

            var talkStr = "来…请选择感兴趣的卷轴吧！成功的机率都是10%。\r\n\r\n#r选择卷轴\r\n#b";
            var stance = getPlayer().getJobStyle();
            int[] vecItem;
            if (stance == Job.WARRIOR || stance == Job.BEGINNER)
            {
                vecItem = [2043002, 2043102, 2043202, 2044002, 2044102, 2044202, 2044402, 2044302];
            }
            else if (stance == Job.MAGICIAN)
            {
                vecItem = [2043702, 2043802];
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                vecItem = [2044502, 2044602];
            }
            else if (stance == Job.THIEF)
            {
                vecItem = [2043302, 2044702];
            }
            else
            {
                vecItem = [2044802, 2044902];
            }

            for (var i = 0; i < vecItem.Length; i++)
            {
                talkStr += "\r\n#L" + i + "# #i" + vecItem[i] + "# #t" + vecItem[i] + "#";
            }
            var selection = await AskMenu(talkStr);
            var item = vecItem[selection];
            await gainItem(item, 1);
            await gainItem(4031103, -1);
            await gainItem(4031104, -1);
            await gainItem(4031105, -1);
            await gainItem(4031106, -1);
            await gainExp(12000);

            await completeQuest();
        }
        // Quest: 3437 
        public async Task q3437e()
        {
            await SayNext("什么？你是在告诉我你已经消灭了150只 #o4230120#？而且这些... 是的，这确实是120个 #t4000122#。我一直在想你是怎么一个人完成这个任务的，但你做得很好。好吧，这是一个对我来说非常重要的物品，但请收下它。");

            if (getPlayer().getInventory(InventoryType.EQUIP).getNumFreeSlot() < 1)
            {
                await SayOK("请在装备栏中腾出一个空位来领取奖励。");
                return;
            }

            var talkStr = "你喜欢这个手套吗？我已经保存了一段时间，本来打算有一天用它，但看起来你戴起来更好看。请好好利用它；此外，我从部门那里得到了很多东西，我不再需要它了。";
            var stance = getPlayer().getJobStyle();
            int item;
            if (stance == Job.WARRIOR)
            {
                item = 1082024;
            }
            else if (stance == Job.MAGICIAN)
            {
                item = 1082063;
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                item = 1082072;
            }
            else if (stance == Job.THIEF)
            {
                item = 1082076;
            }
            else if (stance == Job.BRAWLER || stance == Job.GUNSLINGER)
            {
                item = 1082195;
            }
            else
            {
                item = 1082149;
            }

            await SayNext(talkStr);
            await completeQuest();
            await gainItem(item, 1);
            await gainItem(4000122, -120);
            await gainExp(6100);
            await SayOK("非常感谢你作为Mesorangers之一完成任务。我已经告诉部门你成功的故事，部门似乎对你也很满意。希望你继续和我们合作。再见~");
        }
        // Quest: 3452 
        public async Task q3452e()
        {
            await SayNext("接受这些 #b#t2000011##k 作为我的感激之情。");
            if (canHold(2000011, 50))
            {
                await gainItem(4000099, -1);
                await gainItem(2000011, 50);
                await gainExp(8000);

                await forceCompleteQuest();
            }
            else
            {
                await SayNext("嗯？看起来你的背包已经满了。");
            }
        }
        // Quest: 3454 
        public async Task q3454e()
        {
            if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
            {
                await SayOK("请先在你的其他栏腾出空位.");
                return;
            }

            await SaySpeech([
                "嗯，所有的材料都搜集到了。请稍等一下。我马上为你制造超声波解读机。",
                "#b(...咣当咣当)#k",
                "#b(...当！当！当！)#k",
                "#b(...嘭！)#k"
                ]);

            await gainItem(4000125, -1);
            await gainItem(4031926, -10);
            await gainItem(4000119, -30);
            await gainItem(4000118, -30);

            await gainItem(4031928, 1);//
            // gainItem(4031927, 1);

            await forceCompleteQuest();
            await SayOK("来，解读机做好了。因为刚做好，所以没来得及测试性能，如果发生错误，请不要太吃惊。你可以再来找我，只要你能给我材料，我可以重新帮你制造解读机。");
        }
        // Quest: 7103 
        public async Task q7103s()
        {
            if (await AskYesNo("我们现在唯一要做的事情...就是让#o8500002#永远消失...你准备好了吗？"))
            {
                await SaySpeech([
                    "我会向你解释接下来需要做什么。\r\n要进入发电室，你需要通过#b遗忘之路#k或#b扭曲之路#k。一旦打败守卫通道的怪物，你就可以获得#b#t4031172:##k，这是进入发电室所需的物品。",
                    "然后通过中间的门进入房间。这里会比你想象的安静得多。时间球应该隐藏在我们眼中无法察觉的状态...但是如果你封住维度裂缝，#o8500002#会因为其出口被封住而惊慌失措，从那里出现。"
                    ]);

                if (await SayAcceptDecline("丢下我给你的#b#t4031179:##k 来封住#o8500002#可能用来进入这个维度的任何裂缝。然后它将从时间球中出来，展示其真正的外貌。请，杀死它然后回来。\r\n\r\n收集#r1 #t4031172:##k\r\n消灭#r#o8500001##k"))
                {
                    if (!haveItem(4031179, 1))
                    {
                        if (!canHold(4031179, 1))
                        {
                            await SayOK("请确保有一个#r其他栏位可用#k 来开始这个任务。");
                            return;
                        }

                        await gainItem(4031179, 1);
                    }
                    await forceStartQuest();
                }
            }
        }

    }
}