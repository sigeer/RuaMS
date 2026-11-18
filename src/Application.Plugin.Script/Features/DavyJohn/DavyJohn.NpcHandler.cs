using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.DavyJohn
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task davyJohn_enter()
        {
            var em = GetEventManager(DavyJohnFeature.EventName);

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.DavyJohn_EnterMenu, em.Template.GetRequirementDescription(c)),
                [this.GetTalkMessage(ScriptMessage.PartyQuest_Start), this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)]
            );
            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_PartyQuest));
                    break;
                default:
                    break;
            }
        }

        public async Task davy_clear()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return;
            }

            switch (getMapId())
            {
                case DavyJohnFeature.BossMap:
                    if (isEventLeader())
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HaveSaved));
                        await eim.clearPQ();
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HaveSavedThanks));
                    }
                    break;
                default:
                    await ShowExitMenu();
                    break;
            }
        }

        public async Task davyJohn_play()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return;
            }

            switch (getMapId())
            {
                case DavyJohnFeature.Stage1Map:
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HeadingIntoPirateShipGet));
                    break;
                case DavyJohnFeature.Stage2Map:
                    await TalkStage2(eim);
                    break;
                case DavyJohnFeature.Stage3Map:
                case DavyJohnFeature.Stage4Map:
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_AssaultPirate));
                    break;
                case DavyJohnFeature.Stage3Sub1:
                    await TalkSealedBossStage(eim, DavyJohnFeature.Stage2AProperty);
                    break;
                case DavyJohnFeature.Stage4Sub1:
                    await TalkSealedBossStage(eim, DavyJohnFeature.Stage3AProperty);
                    break;
                case DavyJohnFeature.Stage3Sub2:
                case DavyJohnFeature.Stage4Sub2:
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_CaptainsKrus));
                    break;
                case DavyJohnFeature.Stage5Map:
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_SourcesShips));
                    break;
                case DavyJohnFeature.BossMap:
                    if (getMap().countMonsters() == 0)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_ThanksSaving));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_DefeatAll));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 第二阶段：依次上缴 20 个 4001120 / 4001121 / 4001122。
        /// </summary>
        async Task TalkStage2(AbstractEventInstanceManager eim)
        {
            var stage = eim.getProperty(DavyJohnFeature.Stage2Property);
            if (stage == "0")
            {
                if (haveItem(4001120, 20))
                {
                    await gainItem(4001120, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "1");

                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_ExcellentHunt));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HeadingInto));
                }
            }
            else if (stage == "1")
            {
                if (haveItem(4001121, 20))
                {
                    await gainItem(4001121, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "2");

                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_ExcellentHuntVeteran));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HeadingIntoPirate));
                }
            }
            else if (stage == "2")
            {
                if (haveItem(4001122, 20))
                {
                    await gainItem(4001122, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "3");
                    eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                    await eim.showClearEffect(getMapId());

                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_ExcellentLet));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HeadingIntoPirateShip));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_NextStage));
            }
        }

        async Task TalkSealedBossStage(AbstractEventInstanceManager eim, string propertyKey)
        {
            if (getMap().countMonsters() == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_LordPirates));
                if (eim.getProperty(propertyKey) == "0")
                {
                    await getMap().setReactorState();
                    eim.setProperty(propertyKey, "1");
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_BellflowersHiding));
            }
        }

        async Task ShowExitMenu()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_ThankSaving));
            if (option == 0)
            {
                if (!canHold(DavyJohnFeature.ClearRewardItem, 1))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_PleaseMake));
                    return;
                }
                await gainItem(DavyJohnFeature.ClearRewardItem, 1);
                await warp(DavyJohnFeature.RecruitMapId, 0);
                return;
            }

            await ExchangeHat();
        }

        /// <summary>
        /// 海盗船长帽兑换链：1002572 → 1002573 → 1003267，每次消耗 20 个纪念道具。
        /// </summary>
        async Task ExchangeHat()
        {
            if (haveItem(1003267, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HaveBest));
                return;
            }

            int sourceItem;
            int targetItem;
            if (haveItem(1002573, 1))
            {
                sourceItem = 1002573;
                targetItem = 1003267;
            }
            else if (haveItem(1002572, 1))
            {
                sourceItem = 1002572;
                targetItem = 1002573;
            }
            else
            {
                sourceItem = 0;
                targetItem = 1002572;
            }

            if (!haveItem(DavyJohnFeature.ClearRewardItem, 20))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_NeedFeathers, DavyJohnFeature.ClearRewardItem));
                return;
            }

            if (!canHold(targetItem, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_PleaseMakeRoom));
                return;
            }

            if (sourceItem != 0)
            {
                await gainItem(sourceItem, -1);
            }
            await gainItem(DavyJohnFeature.ClearRewardItem, -20);
            await gainItem(targetItem, 1);
            await SayOK(this.GetTalkMessage(ScriptMessage.DavyJohn_Head_HaveGiven));
        }
    }
}
