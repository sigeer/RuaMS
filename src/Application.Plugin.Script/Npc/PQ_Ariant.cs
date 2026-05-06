using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;
using Application.Shared.Constants.Map;
using Application.Shared.Constants.Skill;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 2101014 
        public async Task aMatchEnt()
        {
            if (getPlayer().getMapId() == MapId.ARPQ_LOBBY)
            {
                var allAriant = Enumerable.Range(0, 3).ToDictionary(x => x, x => GetEventManager<PQ_Ariant>(nameof(PQ_Ariant) + (x + 1).ToString()));
                var em = allAriant[0];

                if (getPlayer().getLevel() < em.MinLevel || getPlayer().getLevel() > em.MaxLevel)
                {
                    await SayOK("你已经超过了#r等级30#k，因此你不能再参与这个副本了。");
                    return;
                }
                var allAriantKeys = allAriant.Keys.ToArray();
                foreach (var k in allAriantKeys)
                {
                    var eim = allAriant[k].GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                    if (eim != null && eim.InstanceStatus != Core.scripting.Events.Abstraction.InstanceStatus.Recruitment)
                    {
                        allAriant.Remove(k);
                    }
                }

                if (allAriant.Count == 0)
                {
                    await SayOK("所有的战斗竞技场都已经被占用。我建议你稍后再回来，或者换个频道。");
                    return;
                }

                var option = await AskMenu("What would you like to do? \r\n\r\n\t#e#r(Choose a Battle Arena)#n#k",
                    allAriant.ToDictionary(x => x.Key, x =>
                    {
                        var eim = x.Value.GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                        if (eim == null)
                        {
                            return $"Battle Arena ({x.Key + 1}) (Empty)";
                        }
                        else if (eim.isRegistering())
                        {
                            return $"加入 Battle Arena ({x.Key + 1}) 房主（{eim.getLeader()!.Name}） 成员（TODO）";
                        }

                        return "";
                    }));

                em = allAriant[option];
                var selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();

                if (selectedExped == null)
                {
                    var inputNumber = await AskNumber($"这场比赛最多可以有多少人参加？ ({em.MinCount}~{em.MaxCount} 人)", em.MinCount, em.MinCount, em.MaxCount);
                    selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                    if (selectedExped != null)
                    {
                        await SayOK("房间里有人了");
                        return;
                    }

                    var r = em.StartInstance(getPlayer());
                    if (r == Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                    {
                        selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();

                        selectedExped.MaxCount = inputNumber;
                        getPlayer().dropMessage("房间创建成功，等待玩家加入。");
                    }
                    else
                    {
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                }
                else if (selectedExped.isRegistering())
                {
                    if (selectedExped.contains(getPlayer()))
                    {
                        await SayOK("抱歉，你已经在大厅里了。");
                        return;
                    }

                    var playerAdd = em.JoinMember(selectedExped, getPlayer());
                    await SayOK(em.HandleJoinInstanceResult(playerAdd, c));
                }
            }
        }



        // Npc: 2101015 
        public async Task aMatchScore()
        {
            switch (await AskMenu("你好，我能为你做些什么？", ["我想查看或用点数兑换#t3010018#", "我想了解更多关于战斗竞技场点数的相关信息"]))
            {
                case 0:
                    var apqpoints = getPlayer().AriantPoints;
                    if (apqpoints >= 100)
                    {
                        await SayNext("哇，看起来你已经准备好要交易的#b100#k点数了，我们来交易吧！");

                        getPlayer().AriantPoints -= 100;
                        gainItem(3010018, 1);
                    }
                    else
                    {
                        await SayOK("你的战斗竞技场分数：#b" + apqpoints + "#k 点。你需要超过#b100点#k，这样我才能给你#b棕榈树沙滩椅#k。当你有足够的分数时再和我交谈。");
                        return;
                    }
                    break;
                case 1:
                    await SayOK("主要目标是让玩家在战斗竞技场中积累点数，以便兑换最高奖品：#b椰树沙滩椅#k。在战斗中积累点数，当时机成熟时与我交谈以获取奖品。在每场战斗中，根据玩家最终拥有的珠宝数量来获得积分。但要小心！如果你的积分与其他玩家的#r差距太大#k，那一切都将化为乌有，你只能获得微薄的#r1 point#k。");
                    break;
                default:
                    break;
            }
        }


        // Npc: 2101016 
        public async Task aMatchRwd()
        {
            var eim = GetEventInstanceTrust<AriantEventInstanceManager>();
            var copns = eim.getAriantScore(getPlayer());
            if (copns < 1)
            {
                await SayOK("太糟糕了，你没有得到任何珠宝！");
                return;
            }
            var points = eim.getAriantRewardTier(getPlayer());
            await SayNext("好的，让我看看……你做得非常好，而且你带来了我喜欢的#b" + copns + "#k珠宝。由于你完成了比赛，我将奖励你#b" + points + "点#k战斗竞技场分数。如果你想了解更多关于战斗竞技场分数的信息，那就去找#b#p2101015##k谈谈吧。");

            var r = eim.GiveClearReward(getPlayer(), points);
            if (r == ClaimRewardResult.Success)
            {
                await SayOK("好的！下次再给我更多的宝石！啊哈哈哈哈哈！");
            }

        }


        // Npc: 2101017 
        public async Task aMatchPlay()
        {
            var eim = GetEventInstanceTrust<AriantEventInstanceManager>();
            if (eim.isRegistering())
            {
                if (eim.isEventLeader(getPlayer()))
                {
                    switch (await AskMenu("你想做什么？", ["查看当前成员", "禁止成员", "开始战斗", "离开竞技场"]))
                    {
                        case 0:
                            await SayOK(string.Join("\r\n", eim.getPlayers().Select(x => x.Name)));
                            break;
                        case 1:
                            var members = eim.GetPlayerSortList();
                            var selected = await AskMenu($"以下成员在房间里（点击成员名字可以将其踢出房间）：\r\n1. {members[0].Name}", members.Skip(1).Select((x, idx) => $"{idx + 2}. {x.Name}")) + 1;
                            eim.ban(members[selected].Id);
                            await SayOK($"你已经从房间中禁止了 {members[selected].Name}");
                            break;
                        case 2:
                            eim.StartBattle();
                            break;
                        case 4:
                            WarpReturn();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await SayOK("嗨，你听说过阿里安特角斗场战斗竞技场吗？这是一个供20级到30级玩家参与的竞争活动！");
                }
            }
            else
            {
                var gotTheBombs = eim.getProperty("gotBomb" + getChar().getId());
                if (gotTheBombs != null)
                {
                    await SayOK("我已经给了你炸弹，请立刻击败 #b天蝎座#k！");
                }
                else if (canHoldAll([2270002, 2100067], [50, 5]))
                {
                    eim.setProperty("gotBomb" + getChar().getId(), "1");
                    gainItem(2270002, 50);
                    gainItem(2100067, 5);

                    await SayOK("我已经给了你(5) #b#e#t2100067##k#n和(50) #b#e#t2270002##k#n。\r\n使用#t2270002#来捕捉蝎子，以获取#r#e灵魂宝石#k#n！");
                }
                else
                {
                    await SayOK("你的背包好像已经满了。");
                }
            }
        }


        // Npc: 2101018 
        public async Task aMatchMove()
        {
            await SaySpeech([
                "我已经在阿里安特为冒险岛的伟大战士们准备了一个盛大的节日。它被称为#b阿里安特角斗场挑战#k。",
                "阿里安特角斗场挑战是一项比赛，将怪物战斗技能与其他人进行比拼。在这项比赛中，你的目标不是猎杀怪物；相反，你需要从怪物身上消耗一定量的HP，然后用宝石吸收它。最终拥有最多宝石的战士将赢得比赛。",

                ]);
            await AskMenu("如果你是来自#b佩里安#k，在巴尔洛克舞者指导下训练的强大勇士，那么你对参加阿里安竞技场挑战感兴趣吗？！\r\n#b#L0# 我很想参加这场盛大的比赛。#l");
            await SayNext("好的，现在我会把你送到战斗竞技场。我希望看到你取得胜利！");
            getPlayer().SaveLocation(Shared.MapObjects.SavedLocationType.MIRROR);
            warp(MapId.ARPQ_LOBBY, 3);
        }
    }
}
