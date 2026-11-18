using Application.Core.Channel.DataProviders;
using Application.Shared.Constants;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 战神/海盗/反抗者等新手与转职任务线的缺失实现。
    /// 对应 JS scripts/quest/{22500,22501,22502,22503,22504,22507,23011,2560,2561,2568,2570,2573,20514}.js。
    /// 这些任务在 Quest.wz 中未声明 startscript/endscript，当前运行期不会分发。
    /// </summary>
    internal partial class QuestScript
    {
        #region 伊班与小龙（22500 ~ 22507）

        // Quest: 22500
        public async Task q22500s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ImFinally));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_GoshTalks));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_MasterWeird));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HuhMean));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_MeanMean));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_DragonYoure));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_TalkingAbout));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_WaitWait));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_YeahWhats));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ItsIm));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_PetJust));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_StareDoubtfully));

            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_LookingLike)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_DontBelieve));
                return;
            }

            if (!isQuestStarted(22500))
            {
                await forceStartQuest();
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A25_CommandSlay));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HoldHave));
            await sendImage("UI/tutorial/evan/11/0");
        }

        // Quest: 22501
        public async Task q22501s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_YoMaster));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_UhStill));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_BornJust));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_GuessWere)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A25_BabyDragon));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_GaspRefuse));
            }
        }

        // Quest: 22502
        public async Task q22502s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_WouldntLizard)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_HmNever));
                return;
            }

            await forceStartQuest();
            await sendImage("UI/tutorial/evan/12/0");
        }

        // Quest: 22503
        public async Task q22503s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_IsntNeed));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HmmYoure));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_WhatsNever)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_TryGiving));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_StarveLike));
            }
        }

        // Quest: 22504
        public async Task q22504s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_UghIsnt));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_CantThink));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_SinceYoure)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_AlreadyAsked));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_UseTrying));
            }
        }

        // Quest: 22507
        public async Task q22507s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_KnewKnew));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_SeeExactly));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_DontKnowJust));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_WaitSounds));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_MasterConnected));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HavePay));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_DontRemember));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_Spirit));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_SpiritPact));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_DontKnow));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_CourseIts));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ItsPeaceful));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ThatsFun));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ThatsPart));

            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_BahLet)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_UhYoure));
                return;
            }

            if (!isQuestCompleted(22507))
            {
                await forceCompleteQuest();
                await gainExp(810);
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HeheheAlrighty));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_YoureStill));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_StillHave));
        }

        #endregion

        // Quest: 23011
        /// <summary>战斗法师转职。</summary>
        public async Task q23011e()
        {
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.A25_YouveFinally)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_ThinkCarefully));
                return;
            }

            if (!isQuestCompleted(23011))
            {
                await gainItem(1382100, 1);
                await gainItem(1142242, 1);
                await forceCompleteQuest();
                await changeJobById(3200);
                await showItemGain(1382100, 1);
                await showItemGain(1142242, 1);
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A25_WelcomeResistance));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_DontGo));
        }

        #region 诺特勒斯号 / 海盗转职（2560 ~ 2573）

        // Quest: 2560
        public async Task q2560s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_OoookOok));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_StomachFull));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_OookMonkey)))
            {
                await forceStartQuest();
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_OokOok));
            }
        }

        // Quest: 2561
        public async Task q2561s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_OokOokOook));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_JustWanted));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_OoookOokOok));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_TalkingCaptain));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_OoookOokOokLittle));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HuhHave));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_OokOokOm)))
            {
                if (!isQuestStarted(2561))
                {
                    await gainItem(2010000, 1, true);
                    await forceStartQuest();
                }
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_HaveReceived));
                await ShowEffect("UI/tutorial.img/28");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_ThingDont));
            }
        }

        // Quest: 2568
        public async Task q2568s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_YoureBack)))
                return;

            await forceStartQuest();
            await warp(912060200, 0);
        }

        // Quest: 2568
        public async Task q2568e()
        {
            await SayNext("");
        }

        // Quest: 2570
        /// <summary>炮手转职（凯琳）。</summary>
        public async Task q2570e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_GoodSee));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_CutterWants));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_FormedPirate));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_BecomePirate));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_StillKnow));

            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_ItsWish)))
            {
                if (isQuestCompleted(2570))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A25_SomethingElse));
                }
                return;
            }

            if (!isQuestCompleted(2570))
            {
                await gainItem(1532000, 1);
                await gainItem(1002610, 1);
                await gainItem(1052095, 1);
                await changeJobById(501);
                await forceCompleteQuest();
                await forceCompleteQuest(29900);
                await teachSkill(109, 1, 1, -1);
                await teachSkill(110, 0, -1, -1);
                await teachSkill(111, 1, 1, -1);
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A25_TrulyOne));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_SkillsAlone));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_HaveAnother));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_AhOne));
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_ThatsAll));
        }

        // Quest: 2573
        public async Task q2573s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A25_GreetingsIsnt, npc));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A25_WereQuite)))
            {
                await warp(3000000, 0);
                await forceCompleteQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_LooksLike));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A25_TakeEasy));
            }
        }

        #endregion

        // Quest: 20514
        /// <summary>
        /// 经验加成任务：按「上次记录到现在的经验差」写入任务进度。
        /// JS 用 <c>raiseOpen()</c> 作为入口（不是 start/end），由任务数据决定触发时机。
        /// </summary>
        public async Task q20514Open()
        {
            var chr = getPlayer();
            var questStatus = chr.getQuestStatus(getQuest());

            if (questStatus == 0)
            {
                await setQuestProgress(20515, 0, chr.getLevel());
                await setQuestProgress(20515, 1, (int)chr.getExp());
            }
            else if (questStatus == 1)
            {
                var diffExp = chr.getExp() - getQuestProgressInt(20515, 1);
                var initLevel = getQuestProgressInt(20515, 0);

                for (var i = initLevel; i < chr.getLevel(); i++)
                {
                    diffExp += ExpTable.getExpNeededForLevel(i);
                }

                if (diffExp > 0)
                {
                    var consItem = ItemInformationProvider.getInstance().getQuestConsumablesInfo(4220137);
                    if (consItem != null)
                    {
                        var exp = consItem.exp;
                        var grade = consItem.grade;
                        await setQuestProgress(20514, 0, (int)Math.Min(diffExp, (long)exp * grade));
                    }
                }
            }
        }
    }
}
