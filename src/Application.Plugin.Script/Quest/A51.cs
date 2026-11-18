using Application.Core.Channel.QuestRecordEx;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    // 勋章
    internal partial class QuestScript
    {
        // Quest: 19000 
        public Task q19000s() => HandleMedalQuestStart();
        // Quest: 19001 
        public Task q19001s() => HandleMedalQuestStart();
        // Quest: 19002 
        public Task q19002s() => HandleMedalQuestStart();
        // Quest: 19005 
        public Task q19005s() => HandleMedalQuestStart();
        // Quest: 19006 
        public Task q19006s() => HandleMedalQuestStart();
        // Quest: 29002 
        public async Task q29002s()
        {
            if (await HandleMedalQuestStart())
            {
                var chr = getPlayer();
                var questEx = chr.GetQuestRecordEx((ExQuestId)getQuest()) as MedalQuest29002Ex;
                if (questEx != null)
                {
                    questEx.PopG = 1000;
                    questEx.PopS = chr.Fame;
                    await chr.FlushQuestRecordEx(questEx);
                }
            }
        }
        // Quest: 29002 
        public async Task q29002e()
        {
            var chr = getPlayer();
            var questEx = (chr.GetQuestRecordEx((ExQuestId)getQuest()) as MedalQuest29002Ex)!;
            if (chr.Fame - questEx.PopS >= questEx.PopG)
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ObtainMost));
            }
        }
        // Quest: 29400 
        public async Task q29400s()
        {
            if (await HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_KillEligible)))
            {
                var chr = getPlayer();
                var questEx = chr.GetQuestRecordEx(ExQuestId.VeteranHunter) as MedalQuest29400Ex;
                if (questEx != null)
                {
                    questEx.Mg = 100000;
                    await chr.FlushQuestRecordEx(questEx);
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ChallengeHas));
            }
        }
        // Quest: 29400 
        public async Task q29400e()
        {
            var chr = getPlayer();
            var questEx = chr.GetQuestRecordEx(ExQuestId.VeteranHunter) as MedalQuest29400Ex;
            if (questEx != null && questEx.Mon >= questEx.Mg)
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ChallengeHas));
            }

        }
        // Quest: 29500 
        public async Task q29500s()
        {
            if (await HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_ReachFame)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_OnceFame));
            }
        }
        // Quest: 29500 
        public async Task q29500e()
        {
            if (getPlayer().Fame < 1000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_FameHas));
                return;
            }
            await HandleMedalQuestComplete();
        }
        // Quest: 29501 
        public Task q29501s() => HandleMedalQuestStart();
        // Quest: 29501 
        public Task q29501e() => HandleMedalQuestComplete();
        // Quest: 29502 
        public Task q29502s() => HandleMedalQuestStart();
        // Quest: 29502 
        public Task q29502e() => HandleMedalQuestComplete();
        // Quest: 29503 
        public async Task q29503s()
        {
            if (await HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_DonateMesos)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ComeBack));
            }
        }
        // Quest: 29503 
        public async Task q29503e()
        {
            var chr = getPlayer();

            if (await chr.TryGainMeso(-10000000))
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_PublicContribution));
            }

        }
        // Quest: 29505 
        public async Task q29505s()
        {
            if (await HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_WinMonster)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ComeBackReview));
            }
        }
        // Quest: 29505 
        public async Task q29505e()
        {
            var chr = getPlayer();

            var questEx = chr.GetQuestRecordEx(ExQuestId.PQ_MC2) as ConfrontQuestEx;
            if (questEx != null)
            {
                if (questEx.VicCount > 100)
                {
                    await HandleMedalQuestComplete();
                }
            }
        }
        // Quest: 29506 
        public async Task q29506s()
        {
            if (await HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_MonsterCarnival)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A51_ConsistentlyWinning));
            }
        }
        // Quest: 29506 
        public async Task q29506e()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A51_LetSee)))
            {
                var chr = getPlayer();

                var questEx = chr.GetQuestRecordEx(ExQuestId.PQ_MC2) as ConfrontQuestEx;
                if (questEx != null)
                {
                    if (questEx.Try > 50 && (float)questEx.VicCount / questEx.Try > 0.7)
                    {
                        await HandleMedalQuestComplete();
                    }
                }
            }
        }
        // Quest: 29508 
        public Task q29508e() => NotImplement();
        // Quest: 29900 
        public Task q29900s() => HandleMedalQuestStart();
        // Quest: 29900 
        public Task q29900e() => HandleMedalQuestComplete();
        // Quest: 29901 
        public Task q29901s() => HandleMedalQuestStart();
        // Quest: 29901 
        public Task q29901e() => HandleMedalQuestComplete();
        // Quest: 29902 
        public Task q29902s() => HandleMedalQuestStart();
        // Quest: 29902 
        public Task q29902e() => HandleMedalQuestComplete();

        // Quest: 29903 
        public Task q29903s() => HandleMedalQuestStart();
        // Quest: 29903 
        public Task q29903e() => HandleMedalQuestComplete();
        // Quest: 29904 
        public Task q29904s() => HandleMedalQuestStart();
        // Quest: 29905 
        public Task q29905s() => HandleMedalQuestStart();
        // Quest: 29906 
        public Task q29906s() => HandleMedalQuestStart();
        // Quest: 29907 
        public Task q29907s() => HandleMedalQuestStart();
        // Quest: 29908 
        public Task q29908s() => HandleMedalQuestStart();
        // Quest: 29909 
        public Task q29909s() => HandleMedalQuestStart();
        // Quest: 29910 
        public Task q29910s() => HandleMedalQuestStart();
        // Quest: 29911 
        public Task q29911s() => HandleMedalQuestStart();
        // Quest: 29912 
        public Task q29912s() => HandleMedalQuestStart();
        // Quest: 29913 
        public Task q29913s() => HandleMedalQuestStart();
        // Quest: 29914 
        public Task q29914s() => HandleMedalQuestStart();
        // Quest: 29915 
        public Task q29915s() => HandleMedalQuestStart();
        // Quest: 29916 
        public Task q29916s() => HandleMedalQuestStart();
        // Quest: 29917 
        public Task q29917s() => HandleMedalQuestStart();
        // Quest: 29918 
        public Task q29918s() => HandleMedalQuestStart();
        // Quest: 29919 
        public Task q29919s() => HandleMedalQuestStart();
        // Quest: 29920 
        public Task q29920s() => HandleMedalQuestStart();
        // Quest: 29921 
        public Task q29921s() => HandleMedalQuestStart();
        // Quest: 29922 
        public Task q29922s() => HandleMedalQuestStart();
        // Quest: 29923 
        public Task q29923s() => HandleMedalQuestStart();
        // Quest: 29924 
        public Task q29924s() => HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_AfterBecoming, GetQuestMessage));
        public Task q29924e() => HandleMedalQuestComplete();
        // Quest: 29925 
        public Task q29925s() => HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_AfterBecomingAran, GetQuestMessage));
        public Task q29925e() => HandleMedalQuestComplete();
        // Quest: 29926 
        public Task q29926s() => HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_AfterBecomingAranLevel, GetQuestMessage));
        public Task q29926e() => HandleMedalQuestComplete();
        // Quest: 29927 
        public Task q29927s() => HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_AfterBecomingAranLevelHigher, GetQuestMessage));
        public Task q29927e() => HandleMedalQuestComplete();
        // Quest: 29928 
        public Task q29928s() => HandleMedalQuestStart(this.GetTalkMessage(ScriptMessage.A51_AfterBecomingLevel, GetQuestMessage));
        public Task q29928e() => HandleMedalQuestComplete();
        // Quest: 29933 
        public Task q29933s() => HandleMedalQuestStart();

        async Task NotImplement()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A51_ImplementedYet));
        }

        string GetChanllengeMessage(int medalItemId, string slot) => this.GetTalkMessage(ScriptMessage.A51_ChallengePrompt, medalItemId, medalItemId, slot);
        string GetQuestMessage(int medalItemId, string slot) => this.GetTalkMessage(ScriptMessage.A51_QuestPrompt, medalItemId, medalItemId, slot, npc);

        async Task<bool> HandleMedalQuestStart(string? slot = null, Func<int, string, string>? slotFunc = null)
        {
            var questObj = Application.Core.Server.quest.Quest.getInstance(getQuest());
            if (questObj == null)
            {
                return false;
            }

            if (questObj.IsAutoAccept)
            {
                await startQuest();
                return true;
            }

            if (questObj?.ViewMedalItem > 0)
            {
                if (await SayAcceptDecline(slotFunc == null ? GetChanllengeMessage(questObj.ViewMedalItem, slot ?? this.GetTalkMessage(ScriptMessage.A51_CompleteQuest)) : slotFunc(questObj.ViewMedalItem, slot ?? this.GetTalkMessage(ScriptMessage.A51_CompleteQuest))))
                {
                    await startQuest();
                    return true;
                }
            }
            return false;
        }

        async Task HandleMedalQuestComplete()
        {
            var questObj = Application.Core.Server.quest.Quest.getInstance(getQuest());

            if (questObj?.ViewMedalItem > 0)
            {
                var medalname = c.CurrentCulture.GetItemName(questObj.ViewMedalItem);

                await SayNext(this.GetTalkMessage(ScriptMessage.A51_MedalEarned, medalname, questObj.ViewMedalItem, questObj.ViewMedalItem));
                if (canHold(questObj.ViewMedalItem))
                {
                    await gainItem(questObj.ViewMedalItem);
                    await this.EarnTitle("<" + medalname + this.GetTalkMessage(ScriptMessage.A51_RewardAlready));

                    await this.EarnTitle(this.GetTalkMessage(ScriptMessage.A51_MedalClaimed, medalname));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A51_NoSpace));
                }
            }
        }

    }
}