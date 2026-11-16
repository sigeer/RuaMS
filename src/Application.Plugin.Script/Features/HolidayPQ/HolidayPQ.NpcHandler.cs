using Application.Core.Client;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.HolidayPQ
{
    /// <summary>
    /// 假日组队任务的场内 NPC（9105004，脚本名 <c>08_xmas</c>）：
    /// 招募图报名，副本内按阶段推进（召唤雪人、放斯克鲁奇、结算发奖）。
    /// </summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 9105004 
        [ScriptName("08_xmas")]
        public async Task s_08_xmas()
        {
            var pqType = (getMapId() / 10) % 10 + 1;
            if (pqType < 1 || pqType > 3)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.HolidayPQ_Error, pqType));
                return;
            }

            var em = GetEventManager(HolidayPQFeature.EventNameOf(pqType));

            // 在招募地图：显示报名菜单
            if (getMapId() % 10 == 0)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.HolidayPQ_EnterMenu, em?.Template?.GetRequirementDescription(c) ?? ""), [
                    this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                    this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
                ]);

                if (sel == 0)
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer(), pqType), c));
                }
                else if (sel == 1)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.HolidayPQ_PartyQuest));
                }
                return;
            }

            // 在任务地图内：按阶段推进
            var eim = GetEventInstanceTrust();
            var difficulty = eim.getIntProperty("level");
            var stg = eim.getIntProperty("statusStg1");
            var mapobj = await eim.getInstanceMap(889100001 + 10 * (difficulty - 1));

            if (stg == -1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_FinallyMade));

                if (!isEventLeader())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.HolidayPQ_PleaseLet));
                    return;
                }

                mapobj?.allowSummonState(true);
                if (mapobj != null)
                {
                    var snowmanId = 9400317 + 5 * difficulty;
                    var snowman = Application.Core.Server.life.LifeFactory.Instance.GetMonsterTrust(snowmanId);
                    await mapobj.spawnMonsterOnGroundBelow(snowman, new System.Drawing.Point(-180, 15));
                }
                eim.setIntProperty("snowmanLevel", 1);
                await eim.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.HolidayPQ_SnowmanAppeared);
                eim.setIntProperty("statusStg1", 0);
            }
            else if (stg == 0)
            {
                if (mapobj?.getMonsterById(9400321 + 5 * difficulty) == null)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_PleaseDefeatScrooges));
                    return;
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_AwesomeJust));

                if (!isEventLeader())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.HolidayPQ_PleaseLetParty));
                    return;
                }

                // 雪人长成 → 斯克鲁奇登场
                await eim.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.HolidayPQ_SnowmanGrows);
                if (mapobj != null)
                {
                    var boss = Application.Core.Server.life.LifeFactory.Instance.GetMonsterTrust(9400318 + difficulty);
                    await mapobj.spawnMonsterOnGroundBelow(boss, new System.Drawing.Point(-180, 15));
                }
                eim.setIntProperty("spawnedBoss", 1);
                eim.setIntProperty("statusStg1", 1);
            }
            else
            {
                if (!eim.isEventCleared())
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_PleaseDefeat));
                    return;
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_WowDefeated));

                // 结算：有圣诞礼物券（4032092）则让玩家挑选礼物，否则发放通关奖励
                if (haveItem(4032092))
                {
                    await GiveXmasPresent();
                }
                else if (eim.gridCheck(getPlayer()) == -1)
                {
                    await eim.GiveClearReward(getPlayer(), eim.getIntProperty("level"));
                    eim.gridInsert(getPlayer(), 1);
                }
            }
        }

        /// <summary>
        /// 圣诞礼物券（4032092）兑换：二选一。
        /// </summary>
        private async Task GiveXmasPresent()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.HolidayPQ_BroughtThats)
                + this.GetTalkMessage(ScriptMessage.HolidayPQ_Text)
                + this.GetTalkMessage(ScriptMessage.HolidayPQ_OptionEnergyAndCake));

            // 索引 1 额外随机 0~8 号帽子
            var (mainItem, subItem) = sel switch
            {
                0 => (2000002, 1002850),
                1 => (2000006, 1012011),
                _ => (0, 0),
            };
            if (mainItem == 0)
                return;

            if (!canHold(mainItem, 20) || !canHold(subItem, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.HolidayPQ_PleaseMake));
                return;
            }

            await gainItem(4032092, -1);
            await gainItem(mainItem, 20);
            await gainItem(sel == 1 ? subItem + Random.Shared.Next(9) : subItem, 1);
        }

        // Npc: 9201033 
        public async Task go_xmas06()
        {
            const int happyville = 209000000;
            const int shalomTemple = 681000000;

            var mapId = getMapId();
            if (mapId == happyville)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.HolidayPQ_ShalomTemple)))
                {
                    await warp(shalomTemple, 0);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_LetKnow));
                }
            }
            else if (mapId == shalomTemple)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.HolidayPQ_LikeHead)))
                {
                    await warp(happyville, 0);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.HolidayPQ_LetKnow));
                }
            }
        }

        // Npc: 9105005 
        public async Task out_08Xmas()
        {
            var area = getMapId() % 10;
            if (await AskYesNo(area > 0 ? this.GetTalkMessage(ScriptMessage.HolidayPQ_WishLeave) : this.GetTalkMessage(ScriptMessage.HolidayPQ_WishReturn)))
            {
                if (area > 0)
                {
                    await warp(getMapId() + 1, 0);
                }
                else
                {
                    await warp(209000000);
                }
            }
        }
    }
}
