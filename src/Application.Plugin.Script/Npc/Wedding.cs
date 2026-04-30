using Application.Shared.Constants.Map;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9201021 
        public Task weddingParty()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9201022 
        public async Task Thomas()
        {
            if (getMapId() == MapId.HENESYS)
            {
                if (await SayYesNo($"我可以带你去#m680000000#。你准备好了吗？"))
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
                if (await SayYesNo($"我可以带你回到#m100000000#。你准备好了吗？"))
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
