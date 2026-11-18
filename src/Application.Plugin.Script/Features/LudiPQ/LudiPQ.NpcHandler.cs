using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.LudiPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2040034 
        public async Task party2_enter()
        {
            var em = GetEventManager(LudiPQFeature.EventName);

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.LudiPQ_EnterMenu, em.Template.GetRequirementDescription(c)),
                [this.GetTalkMessage(ScriptMessage.PartyQuest_Start), this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)]
            );
            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(this.GetTalkMessage(ScriptMessage.LudiPQ_PartyQuest));
                    break;
                default:
                    break;
            }

        }


        // Npc: 2040035, 2040036, 2040037, 2040038, 2040039, 2040040, 2040041, 2040042, 2040043, 2040044, 2040045 
        public async Task party2_play()
        {
            var eim = GetEventInstanceTrust();
            var stage = (int)Math.Floor((getMapId() - 922010100) / 100.0) + 1; ;
            var currentStatus = eim.ClearedMaps.GetValueOrDefault(getMapId(), StageStatus.NotStarted);
            switch (getNpc())
            {
                case 2040035:
                    await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_CongratulationsSealing));
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await WarpOut();
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_NoSpace));
                    }
                    break;
                case 2040036:
                case 2040037:
                case 2040038:
                case 2040039:
                case 2040040:
                case 2040042:
                    Dictionary<int, (int Count, string Description)> required = new()
                    {
                        {2040036, (25, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStageCollect, stage)) },
                        {2040037, (15, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStage, stage)) },
                        {2040038, (32, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStageCollectS, stage)) },
                        {2040039, (6, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStageStage, stage)) },
                        {2040040, (24, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStageNd, stage)) },
                        {2040042, (3, this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_WelcomeStageNeed, stage)) },
                    };
                    var data = required[getNpc()];
                    if (isEventLeader())
                    {
                        if (!eim.ClearedMaps.TryGetValue(getMapId(), out var s) || s == StageStatus.NotStarted)
                        {           // preamble
                            await SayOK(data.Description);
                            eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        }
                        else
                        {       // check stage completion
                            if (haveItem(4001022, data.Count))
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.LudiPQ_CollectedAll, data.Count));
                                await gainItem(4001022, -data.Count);

                                await ClearLudiPQStage(eim, getMapId());
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_NotEnough, data.Count));
                            }
                        }
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_TellLeader));
                    }
                    break;
                case 2040041:
                    await SayOK(this.GetTalkMessage(ScriptMessage.LudiPQ_NumberCombo));
                    break;
                case 2040043:
                    if (!eim.isEventLeader(getPlayer()))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.KerningPQ_FollowInstructions));
                        return;
                    }

                    if (currentStatus == StageStatus.NotStarted)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_Stage_Combo, stage));
                        eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        return;
                    }

                    var stgAreas = getMap().getAreas();

                    var em = eim.EventManager.Template as Event;

                    HashSet<int> passedIndex = [];
                    var players = eim.getPlayers();
                    for (int i = 0; i < stgAreas.Count; i++)
                    {
                        var area = stgAreas[i];
                        if (players.Any(chr => area.Contains(chr.getPosition())))
                        {
                            passedIndex.Add(i);
                        }
                    }

                    if (passedIndex.Count != 5)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_LooksLike));
                        return;
                    }

                    var stgCombos = em.GetStage(eim, getMap());
                    if (passedIndex.SetEquals(stgCombos))
                    {
                        await ClearLudiPQStage(eim, getMapId());
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_HurryGo));
                    }
                    else
                    {
                        await eim.showWrongEffect();
                    }
                    break;
                case 2040044:
                    if (!eim.isEventLeader(getPlayer()))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.KerningPQ_FollowInstructions));
                        return;
                    }

                    if (currentStatus == StageStatus.NotStarted)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_WelcomeBoss));
                        eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        return;
                    }

                    if (haveItem(4001023, 1))
                    {
                        await gainItem(4001023, -1);

                        await ClearLudiPQStage(eim, getMapId());

                        await eim.clearPQ();
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_PleaseDefeat));
                    }

                    break;
                case 2040045:
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.LudiPQ_WantLeave)))
                    {
                        // 等价于 JS 的 warp(922011100, "st00")：该地图 returnMap/forcedReturn 均为 922011100
                        await WarpOut();
                    }
                    break;
                default:
                    break;
            }
        }

        internal static async Task ClearLudiPQStage(AbstractEventInstanceManager eim, int curMap)
        {
            eim.ClearedMaps[curMap] = StageStatus.Completed;
            await eim.showClearEffect(true);

            await eim.GiveStageClearRewardAll(curMap);
        }

        // Npc: 2040047 
        public async Task party2_out()
        {
            if (getMapId() == 922010000)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_ReturnBack));
                await warp(221024500);
            }
            else
            {
                var eim = GetEventInstanceTrust();
                var outText = eim?.isEventCleared() ?? true
                    ? this.GetTalkMessage(ScriptMessage.KerningPQ_ReadyLeave)
                    : this.GetTalkMessage(ScriptMessage.LudiPQ_OnceLeave);
                if (await AskYesNo(outText))
                {
                    await WarpOut();
                }
            }
        }

    }
}
