using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Events
{
    internal partial class NpcScript
    {
        // Npc: 9201021 
        public async Task weddingParty()
        {
            if (getMapId() != 680000401)
            {
                var isMap400 = getMapId() == 680000400;
                var options = new List<string>();
                if (!isMap400)
                    options.Add("前往未开发的心脏狩猎场");
                if (isMap400)
                    options.Add("我有7把钥匙，带我去打破箱子");
                options.Add("请将我传送出去");

                var selection = await AskMenu("你好，你想去哪里？", options);

                if (!isMap400 && selection == 0)
                {
                    if (!haveItem(4000313, 1))
                    {
                        await SayOK("看起来你丢失了你的 #b#t4000313##k。很抱歉，但是没有那个物品我不能让你前往狩猎场地。");
                        return;
                    }
                    warp(680000400, 0);
                }
                else if (isMap400 && selection == 0)
                {
                    if (haveItem(4031217, 7))
                    {
                        gainItem(4031217, -7);
                        warp(680000401, 0);
                    }
                    else
                    {
                        await SayOK("看起来你没有7把钥匙。在未驯化之心狩猎地杀死蛋糕和蜡烛以获取钥匙。");
                    }
                }
                else
                {
                    warp(680000500, 0);
                    await SayOK("再见。希望你喜欢这场婚礼！");
                }
            }
            else
            {
                await AskMenu("你好，你现在想回去吗？再次返回这里将花费你 #r另外7把钥匙#k。", ["请将我传送回训练场"]);
                warp(680000400, 0);
            }
        }

        // Npc: 9201022 
        public async Task Thomas()
        {
            if (getMapId() == MapId.HENESYS)
            {
                if (await AskYesNo($"我可以带你去#m680000000#。你准备好了吗？"))
                {
                    warp(680000000, 0);
                }
                else
                {
                    await SayOK("好的，随时可以在这里等到你准备好走！");
                }
            }
            else
            {
                if (await AskYesNo($"我可以带你回到#m100000000#。你准备好了吗？"))
                {
                    warp(100000000, 5);
                }
                else
                {
                    await SayOK("好的，随时可以在这里等到你准备好走！");
                }
            }
        }



        async Task Proof(int idx)
        {
            if (!isQuestStarted(100400))
            {
                await SayOK($"你好 #b#h0##k，我是爱之仙子 #p{getNpc()}#。");
                return;
            }

            int[] questItems = [4000001, 4000037, 4000215, 4000026, 4000070, 4000128];
            int[] questExp = [2000, 5000, 10000, 17000, 22000, 30000];

            var rewardItem = 4031367 + idx;
            var questId = 100401 + idx;
            if (!haveItem(rewardItem, 1))
            {
                var processNanaQuest = async () =>
                {
                    if (haveItem(questItems[idx], 50))
                    {
                        if (canHold(rewardItem, 1))
                        {
                            gainItem(questItems[idx], -50);
                            gainItem(rewardItem, 1);

                            await SayOK("咿呀~ 非常感谢，这里拿着 #b#t4031367##k。");
                            return true;
                        }
                        else
                        {
                            sendOk("请确保有一个空余的ETC槽位来存放爱之令牌。");
                        }
                    }
                    else
                    {
                        sendOk("请带着 #b50 #t" + questItems[idx] + "##k到我这里。");
                    }

                    return false;
                };
                if (isQuestCompleted(questId))
                {
                    if (await SayAcceptDecline($"你是不是把我给你的#k#t{rewardItem}##k弄丢了？也罢，我可以再给你一个，不过你得重新完成我上次交代的差事，没问题吧？我需要你去给我找来#r50个#t{questItems[idx]}#。#k"))
                    {
                        await processNanaQuest();
                    }
                }
                else if (isQuestStarted(questId))
                {
                    if (await processNanaQuest())
                    {
                        gainExp((int)(questExp[idx] * getPlayer().getExpRate()));
                        completeQuest(questId);
                    }
                }
                else
                {
                    if (await SayAcceptDecline($"你是在找#k#t{rewardItem}##k吗？我可以给你一个，不过你得先帮我办件事，没问题吧？"))
                    {
                        startQuest(questId);
                        await SayOK($"我需要你去给我找来#r50个#t{questItems[idx]}#");
                    }
                }
            }
            else
            {
                await SayOK($"嘿，你好。你已经从其他娜娜那里得到了#t{rewardItem}#了吗？");
            }
        }


        // Npc: 9201023 
        public Task ProofKern()
        {
            return Proof(1);
        }


        // Npc: 9201024 
        public Task ProofElli()
        {
            return Proof(3);
        }


        // Npc: 9201025 
        public Task ProofOrbi()
        {
            return Proof(4);
        }


        // Npc: 9201026 
        public Task ProofLudi()
        {
            return Proof(5);
        }


        // Npc: 9201027 
        public Task ProofPeri()
        {
            return Proof(2);
        }


    }
}
