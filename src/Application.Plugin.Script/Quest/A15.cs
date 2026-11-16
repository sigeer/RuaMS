using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;
using Application.Shared.Items;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // 冒险骑士团
    internal partial class QuestScript
    {
        // Quest: 20000 
        public async Task q20000s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_AhYouve));
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_BattleAgainst));
            await SayOK(this.GetTalkMessage(ScriptMessage.A15_WontWorry));

            // gainItem(1142065, 1); // Noblesse Medal * 1
            await gainExp(20); //gain 20 exp!!
            await forceStartQuest();
            await forceCompleteQuest();
        }
        // Quest: 20001 
        public async Task q20001s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_FormallyWelcome),
                this.GetTalkMessage(ScriptMessage.A15_KnowDont),
                this.GetTalkMessage(ScriptMessage.A15_StandingIsland),
                this.GetTalkMessage(ScriptMessage.A15_YoungEmpress),
                this.GetTalkMessage(ScriptMessage.A15_TimeTime),
                this.GetTalkMessage(ScriptMessage.A15_ProblemNobody),
                this.GetTalkMessage(ScriptMessage.A15_YoungEmpressDecided),
                this.GetTalkMessage(ScriptMessage.A15_DutySimple)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_BasicOverview)))
            {
                if (isQuestCompleted(20001))
                {
                    await gainExp(40);
                    await gainItem(1052177, 1); // fancy noblesse robe
                }
                await forceStartQuest();
                await forceCompleteQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_ImGlad),
                    this.GetTalkMessage(ScriptMessage.A15_MemberCygnus),
                    this.GetTalkMessage(ScriptMessage.A15_AgainOne),
                    this.GetTalkMessage(ScriptMessage.A15_TakePortal)
                    ]);
            }
        }
        // Quest: 20002 
        public async Task q20002s()
        {
            await forceStartQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_HehReady));
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_KnightCommanders));
            await forceCompleteQuest();
        }
        // Quest: 20008 
        public async Task q20008s()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.A15_ReadyTake));
            if (selection == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_CallYourself));
            }
            else if (selection == 1)
            {
                await forceStartQuest();

                await AskMenu(this.GetTalkMessage(ScriptMessage.A15_ImGladDidnt));
                await forceCompleteQuest();
            }
        }
        // Quest: 20010 
        public async Task q20010s()
        {
            await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.A15_WelcomeEreve),
                        this.GetTalkMessage(ScriptMessage.A15_WantFormally),
                        this.GetTalkMessage(ScriptMessage.A15_LetRemind)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_LikeMeet)))
            {
                await forceStartQuest();
                await guideHint(2);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_WhoaWhoa));
            }
        }
        // Quest: 20010 
        public async Task q20010e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A15_NoblesseBrother, npc));
            if (canHold(2000022) && canHold(2000023))
            {
                if (!isQuestCompleted(21010))
                {
                    await gainItem(2000020, 5);
                    await gainItem(2000021, 5);

                    await gainExp(15);
                }
                await guideHint(3);
                await forceCompleteQuest();
            }
            else
            {
                await TypedMessage(NoticeType.Popup, this.GetTalkMessage(ScriptMessage.A15_EnoughInventory));
            }
        }
        // Quest: 20011 
        public async Task q20011s()
        {
            await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_NumberWays),
                        this.GetTalkMessage(ScriptMessage.A15_PressCtrl)
                    ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_YouveTried)))
            {
                await forceStartQuest();
                await guideHint(4);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_DontWant));
            }
        }
        // Quest: 20011 
        public async Task q20011e()
        {
            await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_PressCtrl),
                    this.GetTalkMessage(ScriptMessage.A15_EquipmentNoblesses)
                    ]);
            await gainItem(1002869, 1);
            await gainItem(1052177, 1);
            await forceCompleteQuest();
            await gainExp(30);
            await guideHint(6);
        }
        // Quest: 20012 
        public async Task q20012s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_IveWaiting),
                this.GetTalkMessage(ScriptMessage.A15_EarnSkill)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_TimeFlies)))
            {
                await forceStartQuest();
                await guideHint(8);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_IveWaiting));
            }
        }
        // Quest: 20012 
        public async Task q20012e()
        {
            await gainItem(4000483, -1);
            await forceCompleteQuest();
            await gainExp(40);

            await SayNext(this.GetTalkMessage(ScriptMessage.A15_YouveSuccessfully));
        }
        // Quest: 20013 
        public async Task q20013s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_ClangClang),
                this.GetTalkMessage(ScriptMessage.A15_StartledDidnt),
                this.GetTalkMessage(ScriptMessage.A15_HoldCant),
                this.GetTalkMessage(ScriptMessage.A15_KnowGet)
            ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_PleaseBring)))
            {
                await forceStartQuest();
                await guideHint(9);
            }
        }
        // Quest: 20013 
        public async Task q20013e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_BringBuilding));

            await gainItem(4032267, -1);
            await gainItem(4032268, -1);
            await gainItem(3010060, 1);
            await forceCompleteQuest();
            await forceCompleteQuest(20000);
            await forceCompleteQuest(20001);
            await forceCompleteQuest(20002);
            await forceCompleteQuest(20015);
            await gainExp(95);
            await guideHint(10);

            await SayNext(this.GetTalkMessage(ScriptMessage.A15_ThinkNifty));
        }
        // Quest: 20015 
        public async Task q20015s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_KnowMaple));
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_EnemiesGrow));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_WontWorryToo)))
            {
                await forceCompleteQuest(20015);
                await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.A15_HeheKnew),
                        this.GetTalkMessage(ScriptMessage.A15_NeinheartTactician)
                    ]);
            }
        }
        // Quest: 20016 
        public async Task q20016s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_WelcomeKnights),
                this.GetTalkMessage(ScriptMessage.A15_ImSure),
                this.GetTalkMessage(ScriptMessage.A15_IslandCalled),
                this.GetTalkMessage(ScriptMessage.A15_YoungEmpressRuler),
                this.GetTalkMessage(ScriptMessage.A15_ThatsCase),
                this.GetTalkMessage(ScriptMessage.A15_LongAgo),
                this.GetTalkMessage(ScriptMessage.A15_YoungEmpressDecidedTake),
                this.GetTalkMessage(ScriptMessage.A15_BecomeStronger)
            ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_ConcludesExplanation)))
            {
                if (!isQuestStarted(20016))
                {
                    await forceStartQuest();
                    await gainExp(380);
                }

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_ImGladYoure),
                    this.GetTalkMessage(ScriptMessage.A15_HaveAccepted),
                    this.GetTalkMessage(ScriptMessage.A15_AgainOneBorn),
                    this.GetTalkMessage(ScriptMessage.A15_TakePortalLeft)
                ]);

                await forceCompleteQuest();
            }
        }
        // Quest: 20017 
        public async Task q20017s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_HmmSent),
                this.GetTalkMessage(ScriptMessage.A15_BelongRace),
                this.GetTalkMessage(ScriptMessage.A15_AhKnow)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_SeemPrepared)))
            {
                await guideHint(12);
                await forceStartQuest(20020);
                await forceCompleteQuest(20100);
                await forceStartQuest();
            }
        }
        // Quest: 20020 
        public async Task q20020s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_TellYouve),
                this.GetTalkMessage(ScriptMessage.A15_TheresMore)
                ]);
            var o = await AskMenu(this.GetTalkMessage(ScriptMessage.A15_ThinkInterestedSeeing));
            if (o == 0)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A15_LikeSee)))
                {
                    await forceStartQuest();
                    await forceCompleteQuest();
                    await warp(913040100, 0);
                }
            }
        }
        // Quest: 20100 
        public async Task q20100s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_AhhhYoure)))
            {
                await forceStartQuest();
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A15_LookLeft));
            }
        }
        // Quest: 20101 
        public Task q20101e() => Cygnus1stJob(Job.DAWNWARRIOR1);
        // Quest: 20102 
        public Task q20102e() => Cygnus1stJob(Job.BLAZEWIZARD1);
        // Quest: 20103 
        public Task q20103e() => Cygnus1stJob(Job.WINDARCHER1);
        // Quest: 20104 
        public Task q20104e() => Cygnus1stJob(Job.NIGHTWALKER1);
        // Quest: 20105 
        public Task q20105e() => Cygnus1stJob(Job.THUNDERBREAKER1);
        async Task Cygnus1stJob(Job nextJob)
        {
            Dictionary<Job, ItemQuantity[]> initialItems = new()
            {
                { Job.DAWNWARRIOR1, [ new (1302077, 1)] },
                { Job.BLAZEWIZARD1, [ new (1372043, 1)] },
                { Job.WINDARCHER1, [ new (1452051, 1), new(2060000, 200)] },
                { Job.NIGHTWALKER1, [ new (1472061, 1), new(2070000, 200)] },
                { Job.THUNDERBREAKER1, [ new (2060000, 1)] },
            };

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A15_HaveDecided, c.CurrentCulture.GetJobName(nextJob))))
            {
                if (getJob() != Job.NOBLESSE)
                {
                    return;
                }

                var jobType = nextJob.GetJobNiche();
                if (!canGetFirstJob(jobType))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A15_PleaseRaise, getFirstJobStatRequirement(jobType)));
                    return;
                }

                var items = initialItems[nextJob];
                var itemDisplay = string.Join("\r\n", items.Select(x => $"#b#v{x.ItemId}##t{x.ItemId}##k * #r{x.Quantity}#k"));
                if (!CanHoldAll(items))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A15_PleaseMake, itemDisplay));
                    return;
                }

                await changeJob(nextJob);
                await getPlayer().resetStats();
                foreach (var item in items)
                {
                    await gainItem(item.ItemId, item.Quantity, true);
                }
                await forceCompleteQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_MomentEmpress, itemDisplay),
                    this.GetTalkMessage(ScriptMessage.A15_IveAlso),
                    this.GetTalkMessage(ScriptMessage.A15_OpenSkill),
                    this.GetTalkMessage(ScriptMessage.A15_YoullLose),
                    this.GetTalkMessage(ScriptMessage.A15_WantGo)
                    ]);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_ImportantDecision));
            }
        }

        // Quest: 20200 
        public async Task q20200s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_WowLevelHas)))
            {
                await startQuest();
                await completeQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A15_WishTakeKnighthood));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_HmmmFeelLike));
            }
        }
        // Quest: 20201 
        public Task q20201e() => Cygnus2ndJob(Job.DAWNWARRIOR2);
        // Quest: 20202 
        public Task q20202e() => Cygnus2ndJob(Job.BLAZEWIZARD2);
        // Quest: 20203 
        public Task q20203e() => Cygnus2ndJob(Job.WINDARCHER2);
        // Quest: 20204 
        public Task q20204e() => Cygnus2ndJob(Job.NIGHTWALKER2);
        // Quest: 20205 
        public Task q20205e() => Cygnus2ndJob(Job.THUNDERBREAKER2);

        public async Task Cygnus2ndJob(Job nextJob)
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A15_YouveBrought, c.CurrentCulture.GetJobName(nextJob))))
            {
                if (getPlayer().getRemainingSp() > ((getPlayer().getLevel() - getJob().MaxLevel) * 3))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A15_StillHave));
                    return;
                }

                await changeJob(nextJob);
                await CompleteQuestN();
            }
        }
        // Quest: 20311 
        public Task q20311s() => Cygnus3rdJob(Job.DAWNWARRIOR3);
        // Quest: 20312 
        public Task q20312s() => Cygnus3rdJob(Job.BLAZEWIZARD3);
        // Quest: 20313 
        public Task q20313s() => Cygnus3rdJob(Job.WINDARCHER3);
        // Quest: 20314 
        public Task q20314s() => Cygnus3rdJob(Job.NIGHTWALKER3);
        // Quest: 20315 
        public Task q20315s() => Cygnus3rdJob(Job.THUNDERBREAKER3);

        public async Task Cygnus3rdJob(Job nextJob)
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_JewelBroughtBack));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A15_EffortPreventingPotentially, c.CurrentCulture.GetJobName(nextJob))))
            {
                var nPSP = (getPlayer().getLevel() - getJob().MaxLevel) * 3;
                if (getPlayer().getRemainingSp() > nPSP)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A15_PleaseCheck));
                    return;
                }

                await changeJob(nextJob);  //更改职业
                await forceCompleteQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_MomentAdvancedKnight));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_GuessYoure));
            }
        }
        // Quest: 20400 
        public async Task q20400s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_LongAgoReceived));

            await forceCompleteQuest();
        }
        // Quest: 20401 
        public async Task q20401s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_LastTime));

            await forceCompleteQuest();
        }
        // Quest: 20405 
        public async Task q20405s()
        {
            await SaySpeech([
                new SpeechText (this.GetTalkMessage(ScriptMessage.A15_NotePosted), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                new SpeechText (this.GetTalkMessage(ScriptMessage.A15_MachineSent), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);
            await forceCompleteQuest();
        }
        // Quest: 20406 
        public async Task q20406s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_AllSaid));
            await forceCompleteQuest();
        }
        // Quest: 20408 
        public async Task q20408s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_FirstAllThank),
                this.GetTalkMessage(ScriptMessage.A15_NothingElse)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_CelebrateHardWork)))
            {
                if (getJobId() % 10 == 1)
                {
                    await changeJobById(getJobId() + 1);
                }

                await forceStartQuest();
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A15_CourageouslyBattlingBlack));
            }
        }
        //// Quest: 20500 
        //public Task q20500s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20502 
        //public Task q20502s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20502 
        //public Task q20502e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20506 
        //public Task q20506e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20507 
        //public Task q20507s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20509 
        //public Task q20509e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        // Quest: 20520 
        public async Task q20520s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_WowHave));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_SupposeItsDoing)))
            {
                await forceStartQuest();
                await forceCompleteQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_TheresSpecial));
            }
        }
        // Quest: 20522 
        public async Task q20522s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_RidingKnightsBit),
                this.GetTalkMessage(ScriptMessage.A15_DontThink)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_HeresMimianaEgg)))
            {
                if (!haveItem(4220137) && !canHold(4220137))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A15_MakeRoom));
                    return;
                }

                await forceStartQuest();
                if (!haveItem(4220137))
                {
                    await gainItem(4220137);
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_MimianasEgg));
            }
        }
        // Quest: 20522 
        public async Task q20522e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_HowsMimianasEgg));

            if (!haveItem(4220137))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_SeeLostEgg));
                return;
            }

            await forceCompleteQuest();
            await gainItem(4220137, -1);
            await gainExp(37600);
            await SayOK(this.GetTalkMessage(ScriptMessage.A15_LostMimiana));
        }
        // Quest: 20526 
        public async Task q20526s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A15_DontThink),
                this.GetTalkMessage(ScriptMessage.A15_HeresMimianaEgg)
                ]);

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_MakeRoomInventory)))
            {
                if (!haveItem(4220137) && !canHold(4220137))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A15_MimianasEggRaisedSharing));
                    return;
                }

                await forceStartQuest();

                if (!haveItem(4220137))
                {
                    await gainItem(4220137);
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_HowsMimianasEgg));
            }
        }
        // Quest: 20526 
        public async Task q20526e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_SeeLostEgg));

            if (!haveItem(4220137))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_PleaseMakeSpace));
                return;
            }
            if (!canHold(1902005))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_AbleAwaken));
                return;
            }

            await forceCompleteQuest();
            await gainItem(1902005, 1);
            await gainItem(4220137, -1);
            await gainMeso(-10000000);
            await SayOK(this.GetTalkMessage(ScriptMessage.A15_AlrightIll));
        }
        // Quest: 20527 
        public async Task q20527s()
        {
            var mount = getPlayer().getMount();

            if (mount != null && mount.getLevel() >= 3)
            {
                await forceCompleteQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_LooksLike));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A15_LooksLike));
            }
        }
        // Quest: 20600 
        public async Task q20600s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_AfterReaching)))
            {
                await forceStartQuest();
            }
        }
        // Quest: 20610 
        public async Task q20610s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_YouveLearned)))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_ItsLack));
            }
        }
        // Quest: 20700 
        public async Task q20700s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A15_YouveFinally));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_WhetherGo)))
            {
                await forceCompleteQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_TrainingInstructor),
                        this.GetTalkMessage(ScriptMessage.A15_KnowTalk)
                    ]);
            }
        }
        // Quest: 20710 
        public async Task q20710s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_NeverThought)))
            {
                await forceStartQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A15_AccordingIntelligence),
                    this.GetTalkMessage(ScriptMessage.A15_HollyHas)
                    ]);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A15_RefusedFine));
            }
        }
        // Quest: 20720 
        public async Task q20720s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A15_HasLeveling)))
            {
                await forceStartQuest();
            }
        }

    }
}