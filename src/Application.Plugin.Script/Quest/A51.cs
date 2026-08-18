using Application.Core.Channel.QuestRecordEx;
using Application.Shared.Quest;

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
                var questEx = chr.GetMedalQuestInfo((MedalQuestId)getQuest()) as MedalQuest29002Ex;
                if (questEx != null)
                {
                    questEx.PopG = 1000;
                    questEx.PopS = chr.Fame;
                    await questEx.Flush(chr);
                }
            }
        }
        // Quest: 29002 
        public async Task q29002e()
        {
            var chr = getPlayer();
            var questEx = (chr.GetMedalQuestInfo((MedalQuestId)getQuest()) as MedalQuest29002Ex)!;
            if (chr.Fame - questEx.PopS >= questEx.PopG)
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK("要想获得人气王称号，必须在限定时间内使人气度提高1000。如果你觉得太困难，可以放弃任务，挑战其他称号。");
            }
        }
        // Quest: 29400 
        public async Task q29400s()
        {
            if (await HandleMedalQuestStart("30天内击杀100000只符合等级条件的怪物"))
            {
                var chr = getPlayer();
                var questEx = chr.GetMedalQuestInfo(MedalQuestId.VeteranHunter) as MedalQuest29400Ex;
                if (questEx != null)
                {
                    questEx.Mg = 100000;
                    await questEx.Flush(chr);
                }
                await SayOK("挑战已经开始。请在限制时间内尽可能多地狩猎符合条件的怪物。");
            }
        }
        // Quest: 29400 
        public async Task q29400e()
        {
            var chr = getPlayer();
            var questEx = chr.GetMedalQuestInfo(MedalQuestId.VeteranHunter) as MedalQuest29400Ex;
            if (questEx != null && questEx.Mon >= questEx.Mg)
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK("挑战已经开始。请在限制时间内尽可能多地狩猎符合条件的怪物。");
            }

        }
        // Quest: 29500 
        public async Task q29500s()
        {
            if (await HandleMedalQuestStart("人气达到 1000"))
            {
                await SayOK("人气达到 1000 后，请再来找我接受审查。");
            }
        }
        // Quest: 29500 
        public async Task q29500e()
        {
            if (getPlayer().Fame < 1000)
            {
                await SayOK("你的人气还没有达到 1000。");
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
            if (await HandleMedalQuestStart("贡献 10000000 金币"))
            {
                await SayOK("准备好 10000000 金币后，请再来找我。");
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
                await SayOK("完成这次公益贡献需要 10000000 金币。");
            }

        }
        // Quest: 29505 
        public async Task q29505s()
        {
            if (await HandleMedalQuestStart("怪物嘉年华2胜利 100 场"))
            {
                await SayOK("在怪物嘉年华2中取得 100 场胜利后，请回来接受审查。");
            }
        }
        // Quest: 29505 
        public async Task q29505e()
        {
            var chr = getPlayer();

            var questEx = chr.GetPartyQuestRecord(QuestId.PQ_MC2) as ConfrontQuestEx;
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
            if (await HandleMedalQuestStart("怪物嘉年华2至少 50 场\n - 胜率至少 70%"))
            {
                await SayOK("保持稳定的胜率，才是真正的嘉年华天才。符合条件后，请回来接受审查。");
            }
        }
        // Quest: 29506 
        public async Task q29506e()
        {
            if (await AskYesNo("让我看看你是否达到了怪物嘉年华2的场次和胜率要求。"))
            {
                var chr = getPlayer();

                var questEx = chr.GetPartyQuestRecord(QuestId.PQ_MC2) as ConfrontQuestEx;
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
        public Task q29924s() => HandleMedalQuestStart("成为 10 级以上战神后", GetQuestMessage);
        public Task q29924e() => HandleMedalQuestComplete();
        // Quest: 29925 
        public Task q29925s() => HandleMedalQuestStart("成为 30 级以上战神后", GetQuestMessage);
        public Task q29925e() => HandleMedalQuestComplete();
        // Quest: 29926 
        public Task q29926s() => HandleMedalQuestStart("成为 70 级以上战神后", GetQuestMessage);
        public Task q29926e() => HandleMedalQuestComplete();
        // Quest: 29927 
        public Task q29927s() => HandleMedalQuestStart("成为 120 级以上战神后", GetQuestMessage);
        public Task q29927e() => HandleMedalQuestComplete();
        // Quest: 29928 
        public Task q29928s() => HandleMedalQuestStart("成为 200 级战神后", GetQuestMessage);
        public Task q29928e() => HandleMedalQuestComplete();
        // Quest: 29933 
        public Task q29933s() => HandleMedalQuestStart();

        async Task NotImplement()
        {
            await SayNext("尚未实现");
        }

        string GetChanllengeMessage(int medalItemId, string slot) => $"#v{medalItemId}# #e#b#t{medalItemId}##k\n\n - {slot}就能获得本勋章，不想挑战一下试试吗？";
        string GetQuestMessage(int medalItemId, string slot) => $"#v{medalItemId}# #e#b#t{medalItemId}##k\n\n - {slot}就能找#p{npc}#领取称号。";

        async Task<bool> HandleMedalQuestStart(string slot = "完成任务", Func<int, string , string>? slotFunc = null)
        {
            var questObj = server.quest.Quest.getInstance(getQuest());
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
                if (await SayAcceptDecline(slotFunc == null ? GetChanllengeMessage(questObj.ViewMedalItem, slot) : slotFunc(questObj.ViewMedalItem, slot)))
                {
                    await startQuest();
                    return true;
                }
            }
            return false;
        }

        async Task HandleMedalQuestComplete()
        {
            var questObj = server.quest.Quest.getInstance(getQuest());
            if (questObj?.ViewMedalItem > 0)
            {
                var medalname = c.CurrentCulture.GetItemName(questObj.ViewMedalItem);

                await SayNext($"恭喜你获得了 #b<{medalname}>#k 勋章！继续加油吧勇士.\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n #v{questObj.ViewMedalItem}:# #t{questObj.ViewMedalItem}# 1");
                if (canHold(questObj.ViewMedalItem))
                {
                    await gainItem(questObj.ViewMedalItem);
                    await earnTitle("<" + medalname + "> 奖励已获取.");

                    await forceCompleteQuest();
                }
                else
                {
                    await SayNext("背包空间不足或者已经领取过了");
                }
            }
        }

    }
}