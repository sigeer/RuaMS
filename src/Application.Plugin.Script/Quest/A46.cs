using Application.Shared.GameProps;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // 时间神殿
    internal partial class QuestScript
    {
        // Quest: 3514 
        public async Task q3514s()
        {
            if (getPlayer().getMeso() >= 1000000)
            {
                if (canHold(2022337, 1))
                {
                    await gainItem(2022337, 1);
                    await gainMeso(-1000000);

                    //await SayOK(this.GetTalkMessage(ScriptMessage.A46_NiceDoing));
                    await startQuest(3514);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A46_CheckOut));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A46_OyDont));
            }
        }
        // Quest: 3514 
        public async Task q3514e()
        {
            if (getPlayer().getBuffSource(BuffStat.HPREC) != 2022337)
            {
                if (haveItem(2022337))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A46_ScaredDrink));
                }
                else
                {
                    if (canHold(2022337))
                    {
                        await gainItem(2022337, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.A46_LostLuckily));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A46_LostLuckilyManaged));
                    }
                }
                return;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A46_EmotionsLonger));

                await gainExp(891500);
                await completeQuest(3514);
            }

        }
        // Quest: 3523 
        public async Task q3523s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1); // thanks resinate for pointing out uncompletable quest due to non-updated progress
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_WarriorMemory));
        }
        // Quest: 3524 
        public async Task q3524s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1);
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_MageMemory));
        }
        // Quest: 3525 
        public async Task q3525s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1);
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_MageMemory));
        }
        // Quest: 3526 
        public async Task q3526s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1);
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_MageMemory));
        }
        // Quest: 3527 
        public async Task q3527s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1);
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_MageMemory));
        }
        // Quest: 3529 
        public async Task q3529s()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_MageMemory));
            await setQuestProgress(3507, 7081, 1);
            await forceCompleteQuest();
        }
        // Quest: 3539 
        public async Task q3539s()
        {
            await startQuest();
            await setQuestProgress(3507, 7081, 1);
            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A46_WarriorMemory));
        }

    }
}