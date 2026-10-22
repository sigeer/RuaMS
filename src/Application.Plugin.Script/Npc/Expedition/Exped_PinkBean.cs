using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 粉豆远征相关 NPC（时间神殿 - 遗忘的黄昏）。
    /// 对应 JS scripts/npc/2141000~2141002.js。
    /// </summary>
    internal partial class NpcScript
    {
        const int PINKBEAN_TEMPLE_MAP = 270050000;

        // Npc: 2141000 — Kirston：接受后召唤粉豆
        public async Task PinkBeen_Summon()
        {
            if (!await SayAcceptDecline("要是我有善良之镜，就能重新召唤黑魔法师了！\r\n等等！有点不对劲！为什么黑魔法师没有被召唤出来？等等，这股力量是什么？我感觉到某种……和黑魔法师完全不同的东西。啊啊啊啊啊！！！\r\n\r\n#b（把手放在克里斯顿的肩上。）"))
                return;

            var map = getMap();
            var npc = map.GetMapObjects(x => x.getType() == Shared.MapObjects.MapObjectType.NPC)
                .FirstOrDefault(x => x.GetSourceId() == 2141000);
            if (npc != null)
            {
                await map.RemoveMapObject(npc, null);
            }

            var reactor = map.getReactorById(2709000);
            if (reactor != null)
            {
                await reactor.hitReactor(c);
            }
        }

        // Npc: 2141001 — 遗忘神殿管理员：远征队管理
        public async Task PinkBeen_accept()
        {
            const string expedName = "Twilight of the Gods";
            var expedMap = "Twilight of Gods";

            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(nameof(Exped_PinkBean));
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK("您不符合与" + expedBoss + "战斗的条件！");
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu("#e#b<远征：" + expedName + ">\r\n#k#n"
                    + (em?.Template?.GetRequirementDescription(c) ?? "")
                    + "\r\n\r\n你想组建一个团队来挑战 #r" + expedBoss + "#k 吗？\r\n#b#L1#让我们开始吧！#l\r\n#L2#不，我想再等一会儿...#l");
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK("有人已经主动成为了远征队的领袖。试着加入他们吧！");
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK("当然，并非每个人都能挑战" + expedBoss + "。");
                }
            }
            else if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK("你的探险已经在进行中，对于那些仍在战斗中的人，让我们为那些勇敢的灵魂祈祷吧。");
                    return;
                }

                var list = "你想做什么？#b\r\n\r\n#L1#查看当前远征队成员#l\r\n#L2#开始战斗！#l\r\n#L3#退出远征队#l";
                var sel = await AskMenu(list);

                if (sel == 1)
                {
                    var expedMembers = expedition.GetPlayerSortList();
                    if (expedMembers.Count == 1)
                    {
                        await SayOK("你是探险队中唯一的成员。");
                        return;
                    }

                    var text = "以下成员组成了你的探险队（点击成员名字可以将其踢出探险队）：\r\n";
                    text += "\r\n\t\t1." + expedMembers[0].Name;
                    for (var i = 1; i < expedMembers.Count; i++)
                    {
                        text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                    }

                    var kickSelection = await AskMenu(text);
                    if (kickSelection > 0)
                    {
                        var banned = expedMembers[kickSelection - 1];
                        await expedition.ban(banned.Id);
                        await SayOK("你已经从远征中禁止了 " + banned.Name + "。");
                    }
                }
                else if (sel == 2)
                {
                    var size = expedition.getPlayers().Count;
                    if (size < expedition.EventManager.MinCount)
                    {
                        await SayOK("你的远征队至少需要有" + expedition.EventManager.MinCount + "名玩家注册。");
                        return;
                    }

                    await SayOK("探险队将开始，现在将由护送你前往 #b" + expedMap + "#k。");
                    await expedition.StartBattle();
                }
                else if (sel == 3)
                {
                    await player.getMap().LightBlue(expedition.getLeader().getName() + "探险结束了。");
                    await expedition.DisposeAsync();
                    await SayOK("这次探险已经结束。有时候最好的策略就是逃跑。");
                }
            }
            else if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK("你已经注册了这次远征。请等待 #r" + expedition.getLeader().getName() + "#k 开始。");
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, getPlayer()), c));
                }
            }
            else if (expedition.isInProgress())
            {
                if (expedition.contains(player))
                {
                    if (expedition.getIntProperty("canJoin") == 1)
                    {
                        await expedition.registerPlayer(player);
                    }
                    else
                    {
                        await SayOK("你的远征队已经开始对抗" + expedBoss + "的战斗。让我们为这些勇敢的灵魂祈祷。");
                    }
                }
                else
                {
                    await SayOK("另一支探险队正在挑战了" + expedBoss + "，让我们为这些勇敢的灵魂祈祷吧。");
                }
            }
        }

        // Npc: 2141002 — 战斗结束后离开
        public async Task PinkBeen_Out()
        {
            var eim = GetEventInstanceTrust();

            if (!eim.isEventCleared())
            {
                if (await AskYesNo("你现在想出去吗？"))
                {
                    await warp(PINKBEAN_TEMPLE_MAP);
                }
                return;
            }

            if (!await AskYesNo("粉豆已经被打败了！你们真是这片土地上真正的英雄！很快，时间神殿将再次闪耀如初，这都归功于你们的努力！向我们的英雄们欢呼！！你们准备好走了吗？"))
                return;

            if (await eim.GiveClearReward(getPlayer()) == Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
            {
                await warp(PINKBEAN_TEMPLE_MAP);
            }
            else
            {
                await SayOK("如果你的装备、使用、设置和其他物品栏中没有空位，你将无法获得实例奖励。");
            }
        }
    }
}
