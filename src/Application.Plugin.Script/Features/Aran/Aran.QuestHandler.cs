using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.quest;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.Constants.Skill;
// 本文件位于 ...Features.Aran 命名空间内，裸写 Aran 会绑定到该命名空间而非技能常量类。
using SkillAran = Application.Shared.Constants.Skill.Aran;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Aran
{
    // Area 10
    internal sealed class AranQuestHandler : QuestScriptBase
    {
        public AranQuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }

        // Quest: 21000 
        public async Task q21000s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ThinkTheres)))
            {
                await forceStartQuest();
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.AranQ_ChildDeep),
                    this.GetTalkMessage(ScriptMessage.AranQ_DontPanic),
                    this.GetTalkMessage(ScriptMessage.AranQ_PleaseAran)
                    ]);
                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AranCant));
            }
        }
        // Quest: 21001 
        public async Task q21001s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_SniffSniff)))
            {
                await gainItem(4001271, 1);
                await forceStartQuest();
                await warp(914000300, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_SobAran));
            }
        }
        // Quest: 21001 
        public async Task q21001e()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_MadeBack)))
            {
                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ReliefIm), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HurryBoard), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DontHave), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Leave), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AranPlease), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Text), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AthenaGo), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                    ]);

                await gainItem(4001271, -1);
                await removeEquipFromSlot(-11);
                await forceCompleteQuest();

                await warp(914090010, 0); // Initialize Aran Tutorial Scenes
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AboutChild));
            }

        }
        // Quest: 21010 
        public async Task q21010s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_HmWhats),
                "     #i4001170#",
                this.GetTalkMessage(ScriptMessage.AranQ_AhHero)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_SeemsHero)))
            {
                if (getPlayer().HP >= 50)
                {
                    await getPlayer().UpdateHP(25);
                }

                await gainItem(2000022, 1);
                await forceStartQuest();

                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DrinkFirst), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DrinkPotion), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);
                await guideHint(14);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_NeedDecline));
            }
        }
        // Quest: 21010 
        public async Task q21010e()
        {
            if (getPlayer().HP < 50)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HavetDrank));
                return;
            }

            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_WeveDigging),
                this.GetTalkMessage(ScriptMessage.AranQ_IveKept),
                this.GetTalkMessage(ScriptMessage.AranQ_AhHero)
                ]);

            await gainExp(16);
            await gainItem(2000022, 3);
            await gainItem(2000023, 3);
            await forceCompleteQuest();

            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_YouveLeveled, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_TheyreKind, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
            await guideHint(15);
        }
        // Quest: 21011 
        public async Task q21011e()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_WaitWay),
                "   #i4001171#",
                this.GetTalkMessage(ScriptMessage.AranQ_ImSorry),
                this.GetTalkMessage(ScriptMessage.AranQ_WaitMinute)
                ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_ItsMuch)))
            {
                if (canHold(1302000))
                {
                    await gainItem(1302000, 1);
                    await gainExp(35);
                    await forceCompleteQuest();

                    await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_LookingOwn, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                    await guideHint(16);
                }
                else
                {
                    await this.Popup(this.GetTalkMessage(ScriptMessage.A20_InventoryFull));
                }
            }

        }
        // Quest: 21012 
        public async Task q21012s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_WelcomeHero),
                this.GetTalkMessage(ScriptMessage.AranQ_HmDont)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_HmSince)))
            {
                await forceStartQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.AranQ_JustHappens),
                   this.GetTalkMessage(ScriptMessage.AranQ_AhYouve)
                ]);
                await guideHint(17);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_HmDontThink));
            }

        }
        // Quest: 21012 
        public async Task q21012e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_HmExpression));
            if (canHold(2000022) && canHold(2000023))
            {
                await forceCompleteQuest();
                await gainExp(57);
                await gainItem(2000022, 10);
                await gainItem(2000023, 10);

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_EvenReally, 3));
            }
            else
            {
                await this.Popup(this.GetTalkMessage(ScriptMessage.AranQ_InventoryFull));

            }
        }
        // Quest: 21013 
        public async Task q21013s()
        {
            await AskMenu(this.GetTalkMessage(ScriptMessage.AranQ_AhYoure));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_HaveSomething)))
            {
                await forceStartQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_PartsGift, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face));
                await guideHint(18);
            }
        }
        // Quest: 21013 
        public async Task q21013e()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_AhYouveBrought)))
            {
                await forceCompleteQuest();
                await gainExp(95);
                await gainItem(4032309, -1);
                await gainItem(4032310, -1);
                await gainItem(3010062, 1);

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_FullyAssembled, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face));
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HeroInvincible, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face));

                await guideHint(19);
            }
        }
        // Quest: 21015 
        public async Task q21015s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_AlrightIve),
                this.GetTalkMessage(ScriptMessage.AranQ_HaveHero)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_DontKnow)))
            {
                await forceStartQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_PopulationRien));
                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HesitantAbout));
            }
        }
        // Quest: 21016 
        public async Task q21016s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ContinueBasic)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AlrightTime));
                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_ReadyHunt));
            }

        }
        // Quest: 21017 
        public async Task q21017s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SeemsLike), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_GoDefeat), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_IsntNumber), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_CourseHappy), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TwentyPlenty), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_DontHaveModest)))
            {
                await forceStartQuest();
                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_GoesReally), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_PleaseGo), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                ]);

                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HeavyHeart, NpcTalkSpeaker.PlayerRight));
            }
        }
        // Quest: 21018 
        public async Task q21018s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_UndergoTest));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_CantHave)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_SDeeper));
                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_EnoughFeel));
            }
        }


        // Quest: 21100 
        public async Task q21100s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_IsntMuch), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DontRemember), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ExpectedCourse), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Weapon), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_FoundIncredible), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ComeThink), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ThatsAccording)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_ReactsWell, 8));
                await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/ClickPoleArm");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_WhatsStopping, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face));
            }
        }
        // Quest: 21101 
        public async Task q21101s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_RunningHand), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HeroWielded), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ThoughPossessing), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd)
                ]);

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_OwnMemory)))
            {
                await forceStartQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_OneOnce));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_PleaseThink));
            }
        }
        // Quest: 21101 
        public async Task q21101e()
        {
            if (getPlayer().getJob() == Job.LEGEND)
            {
                await changeJobById(JobId.ARAN1);
                await resetStats();

                await completeQuest();

                //getPlayer().changeSkillLevel(SkillFactory.getSkill(20009000), 0, -1);
                //getPlayer().changeSkillLevel(SkillFactory.getSkill(20009000), 1, 0);
                //showInfo("You have acquired the Pig's Weakness skill.");
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_StartingRemember, 3));
            }
        }
        // Quest: 21200 
        public async Task q21200s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_TrainingGoing)))
            {
                await startQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_ActingStrange));
            }
        }
        // Quest: 21200 
        public async Task q21200e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_VoomVoom), NpcTalkSpeaker.ExtraNpc, 1201001),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_IveNever), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_YoAran), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HuhWhose), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_UghOnly), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_WhoAreYou), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AranHear), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_PolearmTalk), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_WhatsSuspicious), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ImSorryCant), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_SorryDoesnt)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_KeepsCalling, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                await warp(MapId.ARAN_MAHA);
                await completeQuest();
            }

        }
        // Quest: 21201 
        public async Task q21201e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_FirstPromise), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TellMake), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AfterAll), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_NeverBegged), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_NeverBeggedHa), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_MaybeLittle), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AranSniff), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_EvenThough)))
            {
                if (!isQuestCompleted(21201))
                {
                    await changeJobById(JobId.ARAN2);

                    await completeQuest();

                    await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_LevelIsnt));
                }
            }
        }
        // Quest: 21202 
        public async Task q21202s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HmmWhats), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_IveCome), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_BestPolearm), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HearBest), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ImToo)))
            {
                await startQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_SayCant, 8));
            }
        }
        // Quest: 21202 
        public async Task q21202e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HaveBrought), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AfterLong), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd)
                ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_PolearmYouve)))
            {
                await completeQuest();
                await removeAll(4032311);
            }

        }
        // Quest: 21300 
        public async Task q21300s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HowsTraining));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_BeforeDoing)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_AnywayThought));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_KnowDont));
            }
        }

        // Quest: 21302 
        public async Task q21302e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_WaitIsnt)); //Giant Polearm
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_HaveRed)))
            {
                if (!isQuestCompleted(21302))
                {
                    if (haveItem(4032312, 1))
                    {
                        await gainItem(4032312, -1);
                    }

                    await changeJobById(JobId.ARAN3);

                    await completeQuest();
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_ComeKeep));
            }
        }
        // Quest: 21303 
        public async Task q21303s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AaaarghYetis), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_WhatsWrong), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_MadeGem), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_YoureSaying), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_GemRecovered)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_ThiefHeaded));
            }
        }
        // Quest: 21400 
        public async Task q21400s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_TrainingGoingKnow)))
            {
                await startQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_HaveBad));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_ImJoking));
            }
        }


        // Quest: 21600 
        public async Task q21600s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_AranSeem)))
            {
                await forceStartQuest();

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.AranQ_HappenedLong),
                    this.GetTalkMessage(ScriptMessage.AranQ_FirstThought),
                    this.GetTalkMessage(ScriptMessage.AranQ_KnowCant),
                    this.GetTalkMessage(ScriptMessage.AranQ_KnowLot),
                    this.GetTalkMessage(ScriptMessage.AranQ_OnceAgrees)
                    ]);
            }
        }
        // Quest: 21604 
        public async Task q21604s()
        {
            await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.AranQ_IsntWolf),
                    this.GetTalkMessage(ScriptMessage.AranQ_RidingMeans)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_WantLearn)))
            {
                await forceStartQuest();

                await SaySpeech([this.GetTalkMessage(ScriptMessage.AranQ_RideCant),
                this.GetTalkMessage(ScriptMessage.AranQ_MaterialAbout)]);

            }
        }

        // Quest: 21618 
        public async Task q21618s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_BefriendedWolf), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Wait), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_AstonishedHuh, 9)))
            {
                await forceStartQuest();
            }
        }
        // Quest: 21618 
        public async Task q21618e()
        {
            if (!haveItemWithId(1902017, false))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HaveUnequip));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_StepAside));

            await forceCompleteQuest();

            await gainItem(1902017, -1);
            await gainItem(1902018, 1);
        }
        // Quest: 21700 
        public async Task q21700s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SeemsLikeYouve), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_RecallFew), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ThatsLot), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_RecoverOld), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_OnlyOne)))
            {
                if (!isQuestStarted(21700) && !isQuestCompleted(21700))
                {
                    await gainItem(1442000, 1);
                    await forceStartQuest();
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_GavePolearm));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_SayingTrain));
            }

        }
        // Quest: 21703 
        public async Task q21703s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AbilitiesReally), 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HaventTrained), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AlrightHeres), 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_LittleBit), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_NaturalWarriors), 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_CantSerious), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_EnterTraining)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_GoTake));
            }
        }
        // Quest: 21703 
        public async Task q21703e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AhYouveCome), 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Joking), NpcTalkSpeaker.PlayerRight)
                ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_HaveNothing)))
            {
                if (isQuestStarted(21703))
                {
                    await forceCompleteQuest();
                    await teachSkill(21000000, (sbyte)getPlayer().GetPlayerSkillLevel(21000000), 10, -1);   // Combo Ability Skill
                    await gainExp(2800);
                }

                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_RememberedCombo), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ReportBack), 0)
                    ]);
            }
        }
        // Quest: 21704 
        public async Task q21704s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TrainingGo), 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TellHer), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ThatsGreat), 0)
            ]);

            await forceCompleteQuest();
            await gainExp(500);
        }
        // Quest: 21712 
        public async Task q21712s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AlwaysEmits));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_SHave)))
            {
                await forceStartQuest();
                await SaySpeech([
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_WonderTriggered), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_CauseAnomaly), NpcTalkSpeaker.PlayerRight)
                    ]);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_StillDont));
            }
        }
        // Quest: 21716 
        public async Task q21716s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Say), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ToldStrange), NpcTalkSpeaker.PlayerRight)
                    ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_KidPuppet)))
            {
                await forceStartQuest();
                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DareKid), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_StillVillagers), NpcTalkSpeaker.PlayerRight)
                    ]);
            }
        }
        // Quest: 21719 
        public async Task q21719s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ArentOne), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_WhoAreYou), NpcTalkSpeaker.PlayerRight)
                    ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_WantKnow)))
            {
                await forceCompleteQuest();
                await warp(910510200, 0);
            }
        }
        // Quest: 21720 
        public async Task q21720e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TruSent), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TellHerAbout), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SeeDidnt), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ThatsTrue), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_BookProphecy), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ArentScared), NpcTalkSpeaker.PlayerRight)
                    ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_ScaredPfft)))
            {
                if (getQuestStatus(21720) == 1)
                {
                    await forceCompleteQuest();
                    await teachSkill(SkillAran.POLEARM_BOOSTER, (sbyte)getPlayer().GetPlayerSkillLevel(SkillAran.POLEARM_BOOSTER), 20, -1);
                    await gainExp(3900);
                }

                await SaySpeech([
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_RememberedComboDrain), NpcTalkSpeaker.PlayerRight),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SkillFound), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SteadilyBecoming), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TheresOnly), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                    ]);
            }
        }
        // Quest: 21729 
        public async Task q21729s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_ReturnTru));

            await forceStartQuest();
        }
        // Quest: 21734 
        public async Task q21734s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_AranReceived),
                this.GetTalkMessage(ScriptMessage.AranQ_KnowEver),
                this.GetTalkMessage(ScriptMessage.AranQ_WithinSomeone)
                ]);

            await forceStartQuest();
        }
        // Quest: 21734 
        public async Task q21734e()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_MadeAran), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DidntFind), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AhaThats), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                    ]);
            await forceCompleteQuest();

            await gainExp(12500);
        }
        // Quest: 21735 
        public async Task q21735s()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_IveAlready), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Find), NpcTalkSpeaker.PlayerRight)
                    ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_AfterPuppeteers)))
            {
                if (!canHold(4032323, 1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_PleaseFree));

                    return;
                }

                if (!haveItem(4032323, 1))
                {
                    await gainItem(4032323, 1);
                }
                await forceStartQuest();
            }
        }
        // Quest: 21735 
        public async Task q21735e()
        {
            if (haveItem(4032323, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_IveHeard));
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AranQ_DontKnowGem)))
                {
                    await gainItem(4032323, -1);
                    await gainExp(6037);
                    await teachSkill(SkillAran.COMBO_DRAIN, 0, 20, -1); // combo drain
                    await forceCompleteQuest();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_HaventGone));
                }
            }
        }
        // Quest: 21736 
        public async Task q21736s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.AranQ_LongTime),
                this.GetTalkMessage(ScriptMessage.AranQ_AnywayEnough),
                this.GetTalkMessage(ScriptMessage.AranQ_SeemsLikeSomething)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ReadyAccept)))
            {
                await forceStartQuest();
            }
        }
        // Quest: 21738 
        public async Task q21738s()
        {
            await SaySpeech([
                 new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_UsuallyDont), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TellHerAboutTracker), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ItsDefinitely), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AtLocation), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_SomeoneTrying), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#m920030001#?", NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AskingSuch), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(".............", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(".........................", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_NothingLike), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                    ]);
            if (!IsQuestNotStarted(21738))
            {
                await forceStartQuest();
            }
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_WhoeverDoesnt, NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face));
        }

        // Quest: 21740 
        public async Task q21740s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_OrbisSeal));
            await forceStartQuest();
        }
        // Quest: 21740 
        public async Task q21740e()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_WontBelieve), 0),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_TellLilin), NpcTalkSpeaker.PlayerRight)
                ]);
            await forceCompleteQuest();
            await teachSkill(SkillAran.COMBO_SMASH, 0, 20, -1); // combo smash
        }
        // Quest: 21741 
        public async Task q21741s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HaveAdvancing));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ApparentlyMr)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_YoureReady));
                await forceStartQuest();
            }

        }
        // Quest: 21742 
        public async Task q21742s()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ImReally), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HeardMet), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_AhMean), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_Item), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_BigHanging), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_DeliverHanging), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                    ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AranQ_ThingSlight)))
            {
                await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_HappenedMaking), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.AranQ_ThoughtWhats), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face)
                    ]);
                if (!haveItem(4220151, 1))
                {
                    if (!canHold(4220151, 1))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.AranQ_NeedEtcSlot, 9));
                        return;
                    }

                    await gainItem(4220151, 1);
                }

                await forceStartQuest();
            }
        }
        // Quest: 21742 
        public async Task q21742e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HangingScroll));
            await gainItem(4032342, -8);
            await gainItem(4220151, -1);
            await gainExp(10000);

            await forceCompleteQuest();
        }



        // Quest: 21748 
        public async Task q21748e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AranCame));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_IveResearched));
            await gainExp(20000);
            await teachSkill(SkillAran.FINAL_CHARGE, 0, 30, -1); // final charge

            await forceCompleteQuest();
        }
        // Quest: 21749 
        public async Task q21749s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HaveLost));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AranNext));
            await forceCompleteQuest();
            await gainExp(500);
        }
        // Quest: 21750 
        public async Task q21750e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AranYoure));
            await forceCompleteQuest();
        }
        // Quest: 21753 
        public async Task q21753s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AranIve));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_AboutProblem));
            await forceStartQuest();
        }
        // Quest: 21754 
        public async Task q21754s()
        {
            if (!canHold(4032328, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HmNeed));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_TakeSend));

            await forceStartQuest();

            await gainItem(4032328, 1);
        }
        // Quest: 21757 
        public async Task q21757e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_LetterEmpress));
            await forceCompleteQuest();
            await gainExp(1000);
            await gainItem(4032330, -1);
        }
        // Quest: 21766 
        public async Task q21766s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_FavorSeems));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_UsedScowl));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HaveFeeling));
            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_KnowRight));
            await forceStartQuest();
        }
        // Quest: 21766 
        public async Task q21766e()
        {
            await forceCompleteQuest();
            await gainExp(200);
        }
        // Quest: 21767 
        public async Task q21767s()
        {
            if (haveItem(4032423, 1))
            {
                await forceStartQuest();
                return;
            }

            if (!canHold(4032423, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_PleaseOpen));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.AranQ_HmTheres));
            await gainItem(4032423, 1);
            await forceStartQuest();
        }
    }
}
