using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    internal partial class QuestScript
    {
        // Quest: 2001 
        public async Task q2001e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_DeedLand));
            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() < 1)
            {
                await this.Popup(this.GetTalkMessage(ScriptMessage.A30_InventoryFull));
                return;
            }

            var talkStr = this.GetTalkMessage(ScriptMessage.A30_PickScroll);
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
            await gainItem(4000022, -100);
            await gainItem(4003000, -30);
            await gainItem(4003001, -30);
            await gainItem(4001004, -1);
            await gainExp(20000);
            await gainMeso(15000);
            await gainFame(2);
            await completeQuest();
        }
        // Quest: 2034 
        public async Task q2034e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_KnewKnew));
            if (getPlayer().getInventory(InventoryType.EQUIP).getNumFreeSlot() < 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseFree));
                return;
            }

            var stance = getPlayer().getJobStyle();
            int item;
            if (stance == Job.WARRIOR)
            {
                item = 1072003;
            }
            else if (stance == Job.MAGICIAN)
            {
                item = 1072077;
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                item = 1072081;
            }
            else if (stance == Job.THIEF)
            {
                item = 1072035;
            }
            else if (stance == Job.BRAWLER || stance == Job.GUNSLINGER)
            {
                item = 1072294;
            }
            else
            {
                item = 1072018;
            }

            await gainItem(item, 1);
            await gainItem(4000007, -150);
            await gainExp(2200);
            await completeQuest();

            await SayOK(this.GetTalkMessage(ScriptMessage.A30_AlrightNeed));
        }
        // Quest: 2124 
        public async Task q2124e()
        {
            if (!haveItem(4031619, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseBring));
            }
            else
            {
                await gainItem(4031619, -1);
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A30_BroughtS));
            }
        }
        // Quest: 2126 
        public async Task q2126e()
        {
            if (!haveItem(4031619, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseBring));
            }
            else
            {
                await gainItem(4031619, -1);
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A30_BroughtS));
            }
        }
        // Quest: 2127 
        public async Task q2127e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A30_SeeYoure));
            await forceCompleteQuest();
        }
        // Quest: 2147 
        public Task q2147s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2147 
        public Task q2147e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2148 
        public async Task q2148s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_WhereverGhost));
            await forceCompleteQuest();
        }
        // Quest: 2149 
        public async Task q2149s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_SayWhenever));
            //最近聽說東部岩山時常發生奇怪的襲擊事件，也許你可以留意一下…
            await forceCompleteQuest();
        }
        // Quest: 2150 
        public async Task q2150s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_TellTheres));
            await forceCompleteQuest();
        }
        // Quest: 2151 
        public async Task q2151s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_TreeHas));
            await forceCompleteQuest();
        }
        // Quest: 2152 
        public async Task q2152s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_TreeIve));
            await forceCompleteQuest();
        }
        // Quest: 2186 
        public async Task q2186e()
        {
            if (!isQuestCompleted(2186))
            {
                if (haveItem(4031853))
                {
                    if (canHold(2030019))
                    {
                        await gainItem(4031853, -1);
                        await gainExp(1700);
                        await gainItem(2030019, 10);
                        await forceCompleteQuest();

                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_GeezFound));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_NeedHave));
                    }
                }
                else if (haveItem(4031854) || haveItem(4031855))
                {
                    //When I figure out how to make a completance with just a pickup xD
                    if (canHold(2030019))
                    {
                        if (haveItem(4031854))
                        {
                            await gainItem(4031854, -1);
                        }
                        else
                        {
                            await gainItem(4031855, -1);
                        }

                        await gainExp(1000);
                        await gainItem(2030019, 5);
                        await forceCompleteQuest();

                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_HmArent));

                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_AlreadyHave));
                    }
                }
            }
        }
        // Quest: 2197 
        public async Task q2197e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_HmmIm));
            await forceCompleteQuest();
        }
        // Quest: 2214 
        public async Task q2214e()
        {
            var hourDay = getHourOfDay();
            if (!(hourDay >= 17 && hourDay < 20))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_EhCant));
                return;
            }

            if (!canHold(4031894, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_AhCrumbled));
                return;
            }

            await gainItem(4031894, 1);
            await gainExp(20000);
            await forceCompleteQuest();

            await SayNext(this.GetTalkMessage(ScriptMessage.A30_HmmIm));

        }
        // Quest: 2215 
        public async Task q2215e()
        {
            var hourDay = getHourOfDay();
            if (!(hourDay >= 17 && hourDay < 20))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_DontHave));
                return;
            }

            if (getMeso() < 2000)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_EhCant));
                return;
            }

            if (!canHold(4031894, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_AlrightDeposit));
                return;
            }

            await gainMeso(-2000);
            await forceCompleteQuest();
            await gainItem(4031894, 1);

            await SayNext(this.GetTalkMessage(ScriptMessage.A30_IveJust));
        }

        bool isAllSubquestsDone()
        {
            for (var i = 2216; i <= 2219; i++)
            {
                if (!isQuestCompleted(i))
                {
                    return false;
                }
            }

            return true;
        }
        // Quest: 2216 
        public async Task q2216s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_NoticeAlready));
            await forceCompleteQuest();
            await gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                await gainItem(4031894, -1);
            }
        }
        // Quest: 2217 
        public async Task q2217s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_SeeStrange));
            await forceCompleteQuest();
            await gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                await gainItem(4031894, -1);
            }
        }
        // Quest: 2218 
        public async Task q2218s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_KnowSay));
            await forceCompleteQuest();
            await gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                await gainItem(4031894, -1);
            }
        }
        // Quest: 2219 
        public async Task q2219s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_ThankDefeating));
            await forceCompleteQuest();
            await gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                await gainItem(4031894, -1);
            }
        }
        // Quest: 2228 
        public async Task q2228s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_PutSmall));
            await forceCompleteQuest();
            await gainFame(8);
        }
        // Quest: 2230 
        public async Task q2230e()
        {
            await AskMenu(this.GetTalkMessage(ScriptMessage.A30_LookingAfter));
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A30_PutHand),
                    this.GetTalkMessage(ScriptMessage.A30_TravelerHave),
                    this.GetTalkMessage(ScriptMessage.A30_HaveFound),
                    this.GetTalkMessage(ScriptMessage.A30_RaisingPet)
                ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A30_WantInstill)))
            {
                if (canHold(5000054, 1))
                {
                    await gainItem(4032086, -1); // Mysterious Egg * -1
                    await forceCompleteQuest();
                    await gainItem(5000054, 1, false, true, 5 * 60 * 60 * 1000);  // rune snail (5hrs), missing expiration time detected thanks to cljnilsson
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A30_SnailPet));
                    return;
                }
            }
        }
        // Quest: 2232
        /// <summary>
        /// 家族任务：需要读取玩家的家族条目（<c>getFamilyEntry()</c>）来判断是否有下属。
        /// ⚠ 家族系统在 C# 中标注为「搁置」（见 <c>Character.cs</c> 中「替换Family，搁置」），
        /// <c>Player</c> 上没有 <c>getFamilyEntry()</c>，家族数据仅在
        /// <c>Application.Module.Family.Channel</c> 内维护且未与 <c>Player</c> 打通。
        /// 因此这里按约定跳过实现，保持空方法；待家族系统恢复后再补。
        /// </summary>
        public Task q2232e()
        {
            // TODO: 依赖家族系统（当前搁置），暂不实现。
            // 原 JS 逻辑：
            //   var familyEntry = qm.getPlayer().getFamilyEntry();
            //   if (familyEntry != null && familyEntry.getJuniorCount() > 0) {
            //       qm.forceCompleteQuest(); qm.gainExp(3000); qm.sendNext("做得很好！");
            //   } else { qm.sendNext("你还没有找到一个同学吗？"); }
            return Task.CompletedTask;
        }
        // Quest: 2233 
        public Task q2233e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2234 
        public Task q2234e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2238 
        public async Task q2238s()
        {
            await forceStartQuest();
        }

        // Quest: 2251 
        public async Task q2251e()
        {
            if (!haveItem(4032399, 20))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_UnderstandEvery));
            }
            else
            {
                await gainItem(4032399, -20);
                await gainExp(8000);
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseFreeSlot));
            }
        }

        // Quest: 2293 
        public async Task q2293s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A30_SnailOnly));
            await forceStartQuest();
        }
        // Quest: 2293 
        public async Task q2293e()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.A30_BringTwentyTickets));
            switch (option)
            {
                case 1:
                    await playSound("Party1/Failed");
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_BroughtThank));
                    break;
                case 2:
                    await playSound("Coconut/Failed");
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_RememberLast));
                    break;
                case 3:
                    await playSound("quest2288/6");
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_IllGive));
                    break;
                case 4:
                    var answer = await AskNumber(this.GetTalkMessage(ScriptMessage.A30_2293_AnswerPrompt), 1, 1, 3);
                    if (answer == 3)
                    {
                        await forceCompleteQuest();
                        await gainExp(32500);

                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_AwkwardlyFamiliar));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_Text));
                    }
                    break;
                default:
                    break;
            }

        }
        // Quest: 2300 
        public async Task q2300s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A30_Heard)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_SongPlaying));
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_ObviouslyDont));
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A30_HaveMade)))
                {
                    if (canHold(4032375, 1))
                    {
                        if (!haveItem(4032375, 1))
                        {
                            await gainItem(4032375, 1);
                        }

                        await warp(106020001, 0);
                        await forceStartQuest();
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseHaveSlot));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_HaveSomething));
                }
            }
            else
            {
                if (canHold(4032375, 1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A30_LookLike));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_WantHmmm));
                }
            }
        }
        // Quest: 2300 
        public async Task q2300e()
        {
            if (!haveItem(4032375, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_HmmmRecommendation));
                return;
            }
            else
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A30_AlrightSince),
                        this.GetTalkMessage(ScriptMessage.A30_OrderRescue)]);
                await gainItem(4032375, -1);
                await gainExp(6000);
                await forceCompleteQuest();
                await forceStartQuest(2312);
            }
        }
        // Quest: 2301 
        public Task q2301s() => q2300s();
        // Quest: 2301 
        public Task q2301e() => q2300e();
        // Quest: 2302 
        public Task q2302s() => q2300s();
        // Quest: 2302 
        public Task q2302e() => q2300e();
        // Quest: 2303 
        public Task q2303s() => q2300s();
        // Quest: 2303 
        public Task q2303e() => q2300e();
        // Quest: 2304 
        public Task q2304s() => q2300s();
        // Quest: 2304 
        public Task q2304e() => q2300e();
        // Quest: 2305 
        public Task q2305s() => q2300s();
        // Quest: 2305 
        public Task q2305e() => q2300e();
        // Quest: 2306 
        public Task q2306s() => q2300s();
        // Quest: 2306 
        public Task q2306e() => q2300e();
        // Quest: 2307 
        public Task q2307s() => q2300s();
        // Quest: 2307 
        public Task q2307e() => q2300e();
        // Quest: 2308 
        public Task q2308s() => q2300s();
        // Quest: 2308 
        public Task q2308e() => q2300e();
        // Quest: 2309 
        public Task q2309s() => q2300s();
        // Quest: 2309 
        public Task q2309e() => q2300e();
        // Quest: 2310 
        public Task q2310s() => q2300s();
        // Quest: 2310 
        public Task q2310e() => q2300e();
        // Quest: 2314 
        public async Task q2314s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A30_PleaseLose)))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_LikeTold));
            }
        }
        // Quest: 2322 
        public async Task q2322s()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A30_ScaredPlease)))
            {
                await forceStartQuest();
            }
        }
        // Quest: 2325 
        public async Task q2325e()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_BrotherSent), NpcTalkSpeaker.NpcLeft),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_AhhhSafe), NpcTalkSpeaker.PlayerRight)
            ]);


            await SayOK(this.GetTalkMessage(ScriptMessage.A30_ThankBringing));

            await forceCompleteQuest();
            await gainExp(6000);
        }
        // Quest: 2327 
        public async Task q2327s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A30_PlanPut),
                this.GetTalkMessage(ScriptMessage.A30_ThankMuch)
                ]);
            await forceStartQuest();
        }
        // Quest: 2332 
        public async Task q2332s()
        {
            if (hasItem(4032388) && !isQuestStarted(2332))
            {
                await forceStartQuest();
                await getPlayer().showHint(this.GetTalkMessage(ScriptMessage.A30_FindBioleta));
            }
        }
        // Quest: 2332 — 原 JS 的 end() 为空函数
        public Task q2332e()
        {
            return Task.CompletedTask;
        }
        // Quest: 2334 
        public async Task q2334s()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_ItsEmbarrassing), 0),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_SeeWow), 0),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_Mean), NpcTalkSpeaker.PlayerRight),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_EvenConsidered), NpcTalkSpeaker.PlayerRight),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_EvenConsideredPretty), NpcTalkSpeaker.PlayerRight),
                new SpeechText(this.GetTalkMessage(ScriptMessage.A30_ImShy), 0)
                ]);

            await forceStartQuest();
            await gainExp(1000);
            await forceCompleteQuest();

        }

        // Quest: 2338 
        public async Task q2338s()
        {
            if (haveItem(2430014, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_EasyMake));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A30_YouveUsed));

                if (!canHold(2430014, 1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A30_PleaseMake));
                }
                else
                {
                    await gainItem(2430014, 1);
                    await forceCompleteQuest();
                }
            }
        }
        // Quest: 2342 
        public async Task q2342s()
        {
            if (isQuestStarted(2331) && !isQuestCompleted(2331))
            {
                if (!hasItem(4001318))
                {
                    if (canHold(4001318))
                    {
                        await forceStartQuest();
                        await gainItem(4001318, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_LooksLike));
                        await forceCompleteQuest();
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseFreeOne));
                        return;
                    }
                }
                else
                {
                    await forceStartQuest();
                    await SayOK(this.GetTalkMessage(ScriptMessage.A30_PleaseDeliver));
                    await forceCompleteQuest();
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_NeedRecover));
                return;
            }
        }
        // Quest: 4647 
        public async Task q4647s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A30_WantLearn)))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_TheresSuch));
            }
        }
        // Quest: 4647 
        public async Task q4647e()
        {
            if (haveItem(5460000))
            {
                await completeQuest();
                await teachSkill(getPlayer().JobModel.Type.GetMultiPetSkillId(), 1, 1, -1);
                await gainItem(5460000, -1, false);

                await SayOK(this.GetTalkMessage(ScriptMessage.A30_GotPet));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A30_GetPet));
            }
        }

    }
}