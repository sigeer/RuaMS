using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PQ_WuGong
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9310004
        public async Task shanghai001()
        {
            var itemId = 4031289;
            var enterMap = 701010321;
            if (isQuestStarted(4103) || isQuestStarted(8512))
            {
                if (haveItem(itemId, 1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_LivestockHave, itemId, itemId, enterMap));
                }

                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_LivestockHave));
                }

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_EnterPrompt, enterMap)))
                {
                    await gainItem(itemId, -1);
                    await warp(enterMap);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_ComeBack, enterMap));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_SalutePatrolling, getNpc()));
            }
        }

        // Npc: 9310005
        public async Task shanghai002()
        {
            var questId = 4109;
            var lobbyMap = 701010321;
            var enterMap = lobbyMap + 1;
            var outMap = lobbyMap - 1;
            var itemID = 4000194;
            var collected = getItemQuantity(itemID);

            var chrQuestStatus = getQuestStatus(questId);
            if (chrQuestStatus == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_SaluteRoutine));
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_JustGot)))
                {
                    await startQuest(questId);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_WontCooperate));
                }
            }
            else
            {
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.WuGong_NeedCollect, itemID, itemID), [
                    this.GetTalkMessage(ScriptMessage.WuGong_EnterContinue, enterMap),
                    this.GetTalkMessage(ScriptMessage.WuGong_LeaveReturn, lobbyMap, outMap)
                ]);
                switch (option)
                {
                    case 0:
                        if (chrQuestStatus == 2)
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_EnterPrompt, enterMap)))
                            {
                                var quest = Application.Core.Server.quest.Quest.getInstance(questId);
                                await quest.reset(getPlayer());
                                await warp(enterMap, "h000");
                                return;
                            }
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_CantEven));
                        }

                        break;
                    case 1:
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_HaveDecided, enterMap)))
                        {
                            await warp(outMap);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // Npc: 9310006
        public async Task shanghai003()
        {
            var em = GetEventManager(PQ_WuGongFeature.EventName);
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.WuGong_SaluteSecret, getNpc()));
            switch (option)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_SaluteSecret));
                    var party = getParty();
                    if (party == null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_SecretMission));
                    }
                    else if (!isLeader())
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_PleaseHave));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.WuGong_MoveNext));
                        if (!isQuestCompleted(4103) && !isQuestCompleted(8512))
                        {
                            //完成了赤珠任务将传送到一个地图
                            var level = getLevel();
                            if (level >= em.MinLevel && level <= em.MaxLevel)
                            {
                                await warp(701010324);
                            }
                            else
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.WuGong_CannotUndertake, em.MinLevel, em.MaxLevel));
                            }
                        }
                        else
                        {
                            var r = await em.StartInstance(getPlayer());
                            await SayOK(em.HandleCreateInstanceResult(r, c));
                            return;
                        }
                    }
                    break;
                case 1:
                    var mapId = 701010320;
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_LeavePrompt, getMapId(), mapId)))
                    {
                        await warp(mapId);
                    }
                    break;
                default:
                    break;
            }
        }

        // Npc: 9310007
        public async Task shanghai004()
        {
            var mapId = 701010320;
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_LeavePrompt, getMapId(), mapId)))
            {
                await warp(mapId);
            }
        }
    }
}
