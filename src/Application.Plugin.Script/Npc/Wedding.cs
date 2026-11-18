using Application.Shared.Constants.Map;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
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
                    options.Add(this.GetTalkMessage(ScriptMessage.Npc9201021_OptionHunting));
                if (isMap400)
                    options.Add(this.GetTalkMessage(ScriptMessage.Npc9201021_OptionSmashBox));
                options.Add(this.GetTalkMessage(ScriptMessage.Npc9201021_OptionWarpOut));

                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9201021_WhereToGo, options));

                if (!isMap400 && selection == 0)
                {
                    if (!haveItem(4000313, 1))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201021_NoInvitation));
                        return;
                    }
                    await warp(680000400, 0);
                }
                else if (isMap400 && selection == 0)
                {
                    if (haveItem(4031217, 7))
                    {
                        await gainItem(4031217, -7);
                        await warp(680000401, 0);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201021_NotEnoughKeys));
                    }
                }
                else
                {
                    await warp(680000500, 0);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201021_WeddingGoodbye));
                }
            }
            else
            {
                await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9201021_ReturnConfirm),
                    [this.GetTalkMessage(ScriptMessage.Npc9201021_OptionWarpBack)]);
                await warp(680000400, 0);
            }
        }

        // Npc: 9201022 
        public async Task Thomas()
        {
            if (getMapId() == MapId.HENESYS)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201022_WarpToAmoria)))
                {
                    await warp(680000000, 0);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201022_WaitHere));
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201022_WarpToHenesys)))
                {
                    await warp(100000000, 5);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201022_WaitHere));
                }
            }
        }



        async Task Proof(int idx)
        {
            if (!isQuestStarted(100400))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_IntroFairy));
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
                            await gainItem(questItems[idx], -50);
                            await gainItem(rewardItem, 1);

                            await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_ThanksGift));
                            return true;
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_NoEtcSlot));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_GatherItems, questItems[idx]));
                    }

                    return false;
                };
                if (isQuestCompleted(questId))
                {
                    if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Wedding_RedoFavor, rewardItem, questItems[idx])))
                    {
                        await processNanaQuest();
                    }
                }
                else if (isQuestStarted(questId))
                {
                    if (await processNanaQuest())
                    {
                        await gainExp((int)(questExp[idx] * getPlayer().getExpRate()));
                        await completeQuest(questId);
                    }
                }
                else
                {
                    if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Wedding_ShareOne)))
                    {
                        await startQuest(questId);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_CollectItems, questItems[idx]));
                    }
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Wedding_AlreadyHave, rewardItem));
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
