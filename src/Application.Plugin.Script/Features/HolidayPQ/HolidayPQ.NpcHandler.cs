using Application.Core.Client;
using Application.Core.scripting.npc;

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
                await SayOK($"假期组队任务{pqType}遇到了一个错误。");
                return;
            }

            var em = GetEventManager(HolidayPQFeature.EventNameOf(pqType));

            // 在招募地图：显示报名菜单
            if (getMapId() % 10 == 0)
            {
                var sel = await AskMenu("#e#b<组队任务：假日>\r\n#k#n"
                    + (em?.Template?.GetRequirementDescription(c) ?? "")
                    + "\r\n\r\n你和你的队伍成员一起完成任务怎么样？在这里，你会遇到障碍和问题，如果没有出色的团队合作，你是无法完成的。如果你想尝试，请告诉你的队伍的#b队长#k来找我谈谈。#b\r\n#L0#我想参加组队任务。\r\n#L1#我想了解更多详情。");

                if (sel == 0)
                {
                    if (getParty() == null)
                    {
                        await SayOK("只有当你加入一个队伍时，你才能参加派对任务。");
                        return;
                    }

                    if (!isLeader())
                    {
                        await SayOK("你的队长必须与我交谈才能开始这个组队任务。");
                        return;
                    }

                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer(), pqType), c));
                }
                else if (sel == 1)
                {
                    await SayOK("#e#b<组队任务：假日>#k#n\r\n\r\n与您的团队一起参与，建造雪人，保护快乐村免受斯克鲁奇的恶行。在里面，与您的团队一起努力保护它，收集雪之力量，这将有助于建造雪人。");
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
                await SayNext("#b#h0##k…… 你终于来了。这是快乐村居民建造巨大雪人的地方。但斯克鲁奇的手下现在正在攻击它。快点！我们的任务是让你和你的队伍在限定时间内保护雪人免受斯克鲁奇手下的攻击。如果你消灭他们，他们会掉落一个叫做雪之力量的物品。收集它们并丢在雪人身上，你会看到它真的在变大。一旦它恢复到原来的大小，你的任务就完成了。只是要小心一件事。一些手下可能会掉落一个假的雪之力量。假的雪之力量实际上会让雪人比平常更快地融化。祝你好运。");

                if (!isEventLeader())
                {
                    await SayOK("请让你们的队长和我联系，以获取有关任务的进一步详情。");
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
                await eim.dropMessage(5, "雪人出现在了场地上！用一切手段保护它！");
                eim.setIntProperty("statusStg1", 0);
            }
            else if (stg == 0)
            {
                if (mapobj?.getMonsterById(9400321 + 5 * difficulty) == null)
                {
                    await SayNext("请打败斯克鲁奇的手下，让雪人变大，这样斯克鲁奇就无法再躲避露面了。");
                    return;
                }

                await SayNext("太棒了！正如我所预料的，你成功击败了斯克鲁奇的手下。非常感谢你！（沉默片刻……）不幸的是，斯克鲁奇似乎并不打算就此罢手。他的手下已经告诉他发生了什么，这意味着……他很快就会出现。请继续战斗，再次祝你好运。");

                if (!isEventLeader())
                {
                    await SayOK("请让你们的队长和我进一步讨论任务的细节。");
                    return;
                }

                // 雪人长成 → 斯克鲁奇登场
                await eim.dropMessage(5, "随着雪人长大成型，斯克鲁奇出现了！");
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
                    await SayNext("请击败斯克鲁奇，这样我们的枫之圣诞节就能免受伤害了！");
                    return;
                }

                await SayNext("哇！！你打败了Scrooge！非常感谢你！你成功地让这个枫之谷平安度过了Maplemas！谢谢！！");

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
            var sel = await AskMenu("哦，你带了一张#b#t4032092##k吗？太好了，稍等一下…… 这是你的冒险岛圣诞礼物。请选择你想要收到的礼物：\r\n\r\n"
                + "#L0#20个#t2000002##k + 1个#t1002850##l\r\n"
                + "#L1#20个#t2000006##k + 1个#t1012011##l");

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
                await SayOK("请确保在继续之前，您的装备和使用物品栏有足够的空间。");
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
                if (await AskYesNo("沙龙神殿与快乐村的其他地方都不一样，你想前往 #b沙龙神殿#k 吗？"))
                {
                    await warp(shalomTemple, 0);
                }
                else
                {
                    await SayNext("如果你改变主意了，告诉我！");
                }
            }
            else if (mapId == shalomTemple)
            {
                if (await AskYesNo("你想回到快乐村吗？"))
                {
                    await warp(happyville, 0);
                }
                else
                {
                    await SayNext("如果你改变主意了，告诉我！");
                }
            }
        }

        // Npc: 9105005 
        public async Task out_08Xmas()
        {
            var area = getMapId() % 10;
            if (await AskYesNo(area > 0 ? "你希望离开这个地方吗？" : "你想要回到 #b快乐村#k 吗？"))
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
