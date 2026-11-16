using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Constants.Map;
using Application.Shared.MapObjects;
using Application.Utility;
using Application.Utility.Configs;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// 武陵道场（Mu Lung Dojo）：NPC 2091005 报名、2091006 公告板。
    /// 关卡推进与奖励结算在传送门脚本里。
    /// </summary>
    internal sealed class DojoNpcHandler : NpcScriptBase
    {
        public DojoNpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2091005
        public async Task dojang_enter()
        {
            var belts = new[] { 1132000, 1132001, 1132002, 1132003, 1132004 };
            var belt_level = new[] { 25, 35, 45, 60, 75 };
            var belt_points = YamlConfig.config.server.USE_FAST_DOJO_UPGRADE
                ? new[] { 10, 90, 200, 460, 850 }
                : new[] { 200, 1800, 4000, 9200, 17000 };

            int selectedMenu = -1;

            if (DojoFeature.IsRestingSpot(getMapId()))
            {
                var eim = GetEventInstanceTrust();
                string text = this.GetTalkMessage(ScriptMessage.Dojo_ImSurprised);
                if (eim.EventName == DojoFeature.SoloEventName)
                {
                    text += this.GetTalkMessage(ScriptMessage.Dojo_WantRecord);
                }
                var selection = await AskMenu(text);
                selectedMenu = selection;

                if (selectedMenu == 0)
                {
                    if (eim.EventName == DojoFeature.PartyEventName)
                    {
                        if (!isPartyLeader())
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Dojo_YoureParty));
                            return;
                        }
                    }
                    else
                    {
                        getPlayer().setDojoStage(0);
                    }

                    var nextStage = (getMapId() + 100);
                    await eim.warpEventTeam(nextStage);
                }
                else if (selectedMenu == 1)
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_YoureGivingYoure)))
                    {
                        await warp(DojoFeature.ExitMap, "st00");
                    }
                }
                else if (selectedMenu == 2)
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_RecordScore)))
                    {
                        var currentStg = DojoFeature.GetStage(getMapId());
                        if (getPlayer().getDojoStage() == currentStg)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Dojo_ScoreHave));
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_RecordedScore));
                            getPlayer().setDojoStage(currentStg);
                        }
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_ThinkGo));
                    }
                }
            }
            else if (getLevel() >= 25)
            {
                if (getMapId() == DojoFeature.RecruitMap)
                {
                    var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Dojo_MasterStrongestPerson));
                    selectedMenu = selection;

                    if (selectedMenu == 0)
                    {
                        if (!getPlayer().FinishedDojoTutorial)
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_FirstTime)))
                            {
                                var em = GetEventManager(DojoFeature.PreEventName);
                                var r = await em.StartInstance(getPlayer());
                                await SayOK(em.HandleCreateInstanceResult(r, c));
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_HahaTrying));
                            }
                        }
                        else
                        {
                            var storedStage = getPlayer().getDojoStage();
                            if (storedStage > 0)
                            {
                                var stageWarp = DojoFeature.GetMobStage(storedStage);

                                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_LastTime)))
                                {
                                    getPlayer().setDojoStage(0);
                                }
                            }
                            var em = GetEventManager(DojoFeature.SoloEventName);
                            var r = await em.StartInstance(getPlayer());
                            await SayOK(em.HandleCreateInstanceResult(r, c));
                        }
                    }
                    else if (selectedMenu == 1)
                    {
                        var em = GetEventManager(DojoFeature.PartyEventName);
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                    else if (selectedMenu == 2)
                    {
                        if (!canHold(belts[0]))
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_MakeRoom));
                            return;
                        }

                        var belt_on_inventory = new bool[belts.Length];
                        for (var i = 0; i < belts.Length; i++)
                        {
                            belt_on_inventory[i] = haveItemWithId(belts[i], true);
                        }

                        var selStr = this.GetTalkMessage(ScriptMessage.Dojo_BeltPointsIntro, getPlayer().getDojoPoints());
                        for (var i = 0; i < belts.Length; i++)
                        {
                            if (belt_on_inventory[i])
                            {
                                selStr += this.GetTalkMessage(ScriptMessage.Dojo_BeltEntryOwned, i, belts[i], belts[i]);
                            }
                            else
                            {
                                selStr += this.GetTalkMessage(ScriptMessage.Dojo_BeltEntry, i, belts[i], belts[i]);
                            }
                        }

                        var beltSelection = await AskMenu(selStr);
                        var belt = belts[beltSelection];
                        var level = belt_level[beltSelection];
                        var points = belt_points[beltSelection];
                        var oldbelt = beltSelection > 0 ? belts[beltSelection - 1] : -1;
                        var haveOldbelt = oldbelt == -1 || haveItemWithId(oldbelt, false);

                        if (beltSelection > 0 && !belt_on_inventory[beltSelection - 1])
                        {
                            await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                        }
                        else if (getPlayer().getDojoPoints() >= points)
                        {
                            if (beltSelection > 0 && !haveOldbelt)
                            {
                                await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                            }
                            else if (getLevel() > level)
                            {
                                if (beltSelection > 0)
                                {
                                    await gainItem(oldbelt, -1);
                                }
                                await gainItem(belt, 1);
                                getPlayer().setDojoPoints(getPlayer().getDojoPoints() - points);
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_HaveProven, belt, belt));
                            }
                            else
                            {
                                await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                            }
                        }
                        else
                        {
                            await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                        }
                    }
                    else if (selectedMenu == 3)
                    {
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_KnowReset)))
                        {
                            getPlayer().setDojoPoints(0);
                            await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_AllTraining));
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_NeedGather));
                        }
                    }
                    else if (selectedMenu == 4)
                    {
                        if (getPlayer().getVanquisherStage() <= 0)
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_HaventAttempted, 1142033 + getPlayer().getVanquisherStage())))
                            {
                                getPlayer().setVanquisherStage(1);
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_DontWant));
                            }
                        }
                        else
                        {
                            if (getPlayer().getDojoStage() > 37)
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_HaveCompleted));
                            }
                            else if (getPlayer().getVanquisherKills() < 100)
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_StillNeed, 100 - getPlayer().getVanquisherKills(), 1142032 + getPlayer().getVanquisherStage()));
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_HaveObtained, 1142032 + getPlayer().getVanquisherStage()));
                                await gainItem(1142033 + getPlayer().getVanquisherStage(), 1);
                                getPlayer().setVanquisherStage(getPlayer().getVanquisherStage() + 1);
                                getPlayer().setVanquisherKills(0);
                            }
                        }
                    }
                    else if (selectedMenu == 5)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_MasterStrongest));
                    }
                }
                else
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_YoureGiving)))
                    {
                        await warp(DojoFeature.ExitMap, 0);
                        await this.Pink(this.GetTalkMessage(ScriptMessage.Dojo_MakeMind));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_StopChanging));
                    }
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Dojo_MockingMaster));
            }
        }

        private async Task SendBeltRequirements(int belt, int oldbelt, bool haveOldbelt, int level, int points)
        {
            var beltReqStr = oldbelt != -1
                ? this.GetTalkMessage(ScriptMessage.Dojo_BeltNeedPrevious, oldbelt)
                : "";
            var pointsLeft = points - getPlayer().getDojoPoints();
            var pointsLeftStr = pointsLeft > 0
                ? this.GetTalkMessage(ScriptMessage.Dojo_BeltNeedPoints, pointsLeft)
                : "";
            var beltLeftStr = !haveOldbelt
                ? this.GetTalkMessage(ScriptMessage.Dojo_BeltUnequip)
                : "";
            var conjStr = pointsLeftStr.Length > 0 && beltLeftStr.Length > 0
                ? this.GetTalkMessage(ScriptMessage.Dojo_BeltAnd)
                : "";

            await SayNext(this.GetTalkMessage(ScriptMessage.Dojo_BeltRequirements,
                belt, belt, beltReqStr, level, points, beltLeftStr + conjStr + pointsLeftStr));
        }


        // Npc: 2091006
        public async Task dojang_move()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Dojo_NoticeAnyone));

            if (selection == 1)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Dojo_NoticeTake),
                    this.GetTalkMessage(ScriptMessage.Dojo_PsChallenge)
                    ]);
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Dojo_OnceHad)))
                {
                    getPlayer().SaveLocation(SavedLocationType.MIRROR);
                    await warp(925020000, 4);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Dojo_TookHand));
                }
            }
        }
    }
}
