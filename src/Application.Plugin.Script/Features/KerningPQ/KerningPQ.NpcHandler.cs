using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Shared.Constants.Map;
using Application.Plugin.Script;

namespace Application.Plugin.Script.Features.KerningPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        /// <summary>
        /// Npc: 9020000 入口 NPC：创建或加入副本。
        /// </summary>
        public async Task party1_enter()
        {
            var em = GetEventManager(KerningPQFeature.EventName);
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.KerningPQ_Description, em.Template.GetRequirementDescription(getClient())),
                [this.GetTalkMessage(ScriptMessage.PartyQuest_Start), this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
            ]);

            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(this.GetTalkMessage(ScriptMessage.KerningPQ_Intro));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Npc: 9020001 关卡 NPC：第一阶段交通行证，第 2~4 阶段核对位置，第五阶段收尾。
        /// </summary>
        public async Task party1_play()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != KerningPQFeature.EventName || eim.EventManager.Template is not Event em)
            {
                return;
            }

            var curMap = getMapId();
            var stage = curMap - KerningPQFeature.Stage1Map + 1;

            if (eim.ClearedMaps.TryGetValue(curMap, out var s) && s == StageStatus.Completed)
            {
                if (stage < 5)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_PleaseHurry));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_IncredibleCleared));

                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(KerningPQFeature.BonusMap, "st00");
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_InventoryFull));
                    }
                }
                return;
            }

            if (curMap == KerningPQFeature.Stage1Map)
            {
                await PlayStage1(eim, em);
            }
            else if (curMap == KerningPQFeature.Stage2Map || curMap == KerningPQFeature.Stage3Map)
            {
                await CheckStage2_3_4(eim, em, curMap);
            }
            else if (curMap == KerningPQFeature.Stage4Map)
            {
                await PlayStage4(eim);
            }
        }

        async Task PlayStage1(AbstractEventInstanceManager eim, Event em)
        {
            var stage1 = em.GetStage1();
            if (isEventLeader())
            {
                var numpasses = eim.getPlayerCount() - 1;     // minus leader

                if (hasItem(KerningPQFeature.PassItem, numpasses))
                {
                    await ClearStage(eim, KerningPQFeature.Stage1Map);
                    eim.gridClear();
                    await gainItem(KerningPQFeature.PassItem, -numpasses);

                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_GatheredPasses));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_NotEnoughPasses, numpasses));
                }
                return;
            }

            var data = eim.gridCheck(getPlayer());
            if (data == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_ThanksBringing));
            }
            else if (data == -1)
            {
                data = Random.Shared.Next(stage1.Quest.Length) + 1;   // 从 1 开始计数
                eim.gridInsert(getPlayer(), data);
                await SayNext(this.GetTalkMessage(stage1.Quest[data - 1]));
            }
            else
            {
                var answer = stage1.Answer[data - 1];
                if (itemQuantity(KerningPQFeature.TicketItem) == answer)
                {
                    await gainItem(KerningPQFeature.TicketItem, -answer);
                    await gainItem(KerningPQFeature.PassItem, 1);
                    eim.gridInsert(getPlayer(), 0);

                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_ThatsRight));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_ImSorry) + this.GetTalkMessage(stage1.Quest[eim.gridCheck(getPlayer()) - 1]));
                }
            }
        }

        async Task PlayStage4(AbstractEventInstanceManager eim)
        {
            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_Stage5Leader));
                return;
            }

            if (!haveItem(KerningPQFeature.PassItem, KerningPQFeature.BonusPassCount))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_Stage5Member));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_HeresPortal));
            await gainItem(KerningPQFeature.PassItem, -KerningPQFeature.BonusPassCount);

            await ClearStage(eim, KerningPQFeature.Stage4Map);
            await eim.clearPQ();
        }

        async Task CheckStage2_3_4(AbstractEventInstanceManager eim, Event em, int mapId)
        {
            var stgAreas = getMap().getAreas().ToArray();

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.KerningPQ_FollowInstructions));
                return;
            }

            HashSet<int> passedIndex = [];
            var players = eim.getPlayers();
            for (int i = 0; i < stgAreas.Length; i++)
            {
                var area = stgAreas[i];
                if (players.Any(chr => area.Contains(chr.getPosition())))
                {
                    passedIndex.Add(i);
                }
            }

            if (passedIndex.Count != 3)
            {
                var msg = mapId switch
                {
                    KerningPQFeature.Stage2Map => this.GetTalkMessage(ScriptMessage.KerningPQ_Stage2),
                    KerningPQFeature.Stage3Map => this.GetTalkMessage(ScriptMessage.KerningPQ_Stage3),
                    _ => this.GetTalkMessage(ScriptMessage.KerningPQ_Stage4),
                };
                await SayNext(msg);
                return;
            }

            var stgCombos = em.GetStage(eim, getMap());
            if (passedIndex.SetEquals(stgCombos))
            {
                await ClearStage(eim, getMapId());
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_PleaseHurry));
            }
            else
            {
                await eim.showWrongEffect();
            }
        }

        /// <summary>
        /// Npc: 9020002 出口 NPC：在奖励地图直接返回城镇，其他地图询问后离开。
        /// </summary>
        public async Task party1_out()
        {
            if (getMapId() == KerningPQFeature.ExitMapId)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.KerningPQ_ReturnBack));
                await warp(MapId.KERNING_CITY);
                return;
            }

            var eim = getEventInstance();
            var outText = eim?.isEventCleared() ?? true
                ? this.GetTalkMessage(ScriptMessage.KerningPQ_ReadyLeave)
                : this.GetTalkMessage(ScriptMessage.KerningPQ_LeaveConfirm2);
            if (await AskYesNo(outText))
            {
                await WarpOut();
            }
        }

        static async Task ClearStage(AbstractEventInstanceManager eim, int curMap)
        {
            eim.ClearedMaps[curMap] = StageStatus.Completed;
            await eim.showClearEffect(true);

            await eim.GiveStageClearRewardAll(curMap);
        }
    }
}
