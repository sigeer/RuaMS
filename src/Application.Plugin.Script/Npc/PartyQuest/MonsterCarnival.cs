using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;
using Application.Shared.MapObjects;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 2042000 
        public async Task mc_enter()
        {
            var talkMap = getMapId();
            if (talkMap == 980000000)
            {
                var msg = GetClientMessage(nameof(ClientMessage.CPQ_PickRoom)) + "#b";
                var o = msg.Length;
                var roomName = GetClientMessage(nameof(ClientMessage.CPQ_Room));
                var levelName = GetClientMessage(nameof(ClientMessage.Level));

                var allCPQ1 = Enumerable.Range(0, 3).ToDictionary(x => x, x => GetEventManager<MonsterCarnivalEventManager>(nameof(PQ_CPQ1) + (x + 1).ToString()));
                var allCPQ1Keys = allCPQ1.Keys.ToArray();
                foreach (var k in allCPQ1Keys)
                {
                    var dEim = allCPQ1[k].GetOnlyEventInstanceManager<MonsterCarnivalEventInstanceManager>();
                    if (dEim != null && dEim.InstanceStatus != InstanceStatus.Recruitment)
                    {
                        allCPQ1.Remove(k);
                    }
                }

                if (allCPQ1.Count == 0)
                {
                    await SayOK("所有的战斗竞技场都已经被占用。我建议你稍后再回来，或者换个频道。");
                    return;
                }

                foreach (var item in allCPQ1)
                {
                    var tEm = item.Value;
                    var tEim = item.Value.GetOnlyEventInstanceManager<MonsterCarnivalEventInstanceManager>();
                    if (tEim == null)
                    {
                        msg += $"#L{item.Key}# {roomName}{item.Key + 1} （{tEm.MinCount}x{tEm.MinCount}）#l\r\n";
                    }
                    else if (tEim.InstanceStatus == InstanceStatus.Recruitment)
                    {
                        msg += $"#L{item.Key}# {roomName}{item.Key + 1} （{levelName}: {tEim.GetAveLevel()} / {tEim.GetRoomSize()}x{tEim.GetRoomSize()}）#l\r\n";
                    }
                }

                if (msg.Length == o)
                {
                    await SayOK(GetClientMessage(nameof(ClientMessage.CPQ_NoEmptyRoom)));
                    return;
                }

                var option = await AskMenu(msg);
                var em = allCPQ1[option];
                var eim = em.GetOnlyEventInstanceManager<MonsterCarnivalEventInstanceManager>();
                if (eim == null)
                {
                    var r = await em.StartInstance(getPlayer());
                    if (r != CreateInstanceResult.Success)
                    {
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                    else
                    {
                        await Pink(em.HandleCreateInstanceResult(r, c) ?? "");
                    }
                }
                else if (eim.InstanceStatus == InstanceStatus.Recruitment)
                {
                    await SayOK(em.GetTemplate.HandleJoinRequestResult(await em.SendJoinRequest(getPlayer()), c));
                }
                else
                {
                    await SayOK(GetClientMessage(nameof(ClientMessage.CPQ_Error)));
                }
            }
            else
            {
                await mc_move();
            }

        }


        // Npc: 2042001, 2042006 
        public async Task mc_enter1()
        {
            var eim = GetEventInstanceTrust<MonsterCarnivalEventInstanceManager>();
            if (eim.RequestTeam == null)
            {
                await Pink(GetClientMessage(nameof(ClientMessage.CPQ_EntryLobby)));
                return;
            }

            var teamMembers = eim.RequestTeam.EligibleMembers;
            var snd = "";
            for (var i = 0; i < teamMembers.Count; i++)
            {
                snd += $"#b{GetClientMessage(nameof(ClientMessage.Name))}: {teamMembers[i].Name} / ({GetClientMessage(nameof(ClientMessage.Level))}: {teamMembers[i].Level}) / {GetJobName(teamMembers[i].JobModel)}#k\r\n\r\n";
            }

            await eim.AcceptChallenge(await SayAcceptDecline(snd + "你想在怪物嘉年华上和这个队伍战斗吗？"));
        }


        // Npc: 2042002 
        public async Task mc_move()
        {
            var talkMap = getMapId();
            if (talkMap == 980000010)
            {
                await SayNext("希望你在怪物嘉年华玩得开心！");
                await warp(980000000, 0);
            }
            else if (talkMap == 980030010)
            {
                await SayNext("希望你在怪物嘉年华玩得开心！");
                await warp(980030000, 0);
            }
            else if (GetEventInstanceTrust() != null)
            {
                // 奖励发放
                var eim = GetEventInstanceTrust<MonsterCarnivalEventInstanceManager>();

                var idx = Array.FindIndex([300, 100, 50, 0], x => eim.GetPlayerData(getPlayer().Id)?.TotalCP >= x) + (eim.IsWinner(getPlayer()) ? 0 : 4);
                List<string> messageList = [
                    "恭喜你的胜利！表现太棒了！对方队伍毫无还手之力！希望下次也能有同样出色的表现！\r\n\r\n#b你的成绩：#rA#k",
                    "恭喜你的胜利！太棒了！你对抗对方团队做得很好！再坚持一会儿，下次你肯定能拿到A！\r\n\r\n#b你的成绩：#rB#k",
                    "恭喜你的胜利。你做了一些事情，但这不能算是一个好的胜利。我期待你下次能做得更好。\r\n\r\n#b你的成绩：#rC#k",
                    "恭喜你的胜利，尽管你的表现并没有完全体现出来。在下一次怪物嘉年华中更加积极参与吧！\r\n\r\n#b你的成绩：#rD#k",

                    "很遗憾，尽管你表现出色，但你要么平局要么输掉了这场战斗。下次胜利就属于你了！\r\n\r\n#b你的成绩：#rA#k",
                    "很遗憾，即使你表现出色，你要么平局要么失败了这场战斗。只差一点点，胜利就可能属于你了！\r\n\r\n#b你的成绩：#rB#k",
                    "很遗憾，你要么平局要么失败了。胜利属于那些努力奋斗的人。我看到了你的努力，所以胜利离你并不遥远。继续努力吧！\r\n##b你的成绩：#rC#k",
                    "很遗憾，你要么打成了平局，要么输掉了战斗，你的表现清楚地反映了这一点。我希望你下次能做得更好。\r\n\r\n#b你的成绩：#rD#k",
                    ];

                await SayNext(messageList[idx]);

                await eim.GiveClearReward(getPlayer(), idx);
                await warp(980000000);
            }
            else
            {
                var talk = $"你想做什么呢？ 如果你没有参加过怪物嘉年华, 在参加之前，你需要知道一些事情! \r\n#b" +
                    $"#L0# 前往怪物嘉年华地图 1.#l \r\n" +
                    $"#L2# 了解怪物嘉年华.#l";

                var option = await AskMenu(talk);

                switch (option)
                {
                    case 0:
                        var targetEm = GetEventManager<MonsterCarnivalEventManager>(nameof(PQ_CPQ1) + "1");
                        if (getLevel() < targetEm.MinLevel)
                        {
                            await SayOK($"你必须至少达到{targetEm.MinLevel}级才能参加怪物嘉年华。当你足够强大时，和我交谈。");
                        }
                        else if (getLevel() > targetEm.MaxLevel)
                        {
                            await SayOK($"很抱歉，只有等级在{targetEm.MinLevel}到{targetEm.MaxLevel}级之间的玩家才能参加怪物嘉年华活动。");
                        }
                        else
                        {
                            getPlayer().SaveLocation(SavedLocationType.MONSTER_CARNIVAL);
                            await warp(980000000, 0);
                        }
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }

            }
        }


        // Npc: 2042003, 2042004 
        public async Task mc_roomout()
        {
            var eim = GetEventInstanceTrust();
            if (eim == null)
            {
                await WarpOut();
            }
            else
            {
                await eim.DisposeAsync();
            }
        }


        // Npc: 2042005 — 怪物嘉年华 2 报名
        public async Task mc2_enter()
        {
            if (getParty() == null)
            {
                await SayOK("在参加怪物嘉年华之前，你需要创建一个队伍！");
                return;
            }

            if (!isLeader())
            {
                await SayOK("如果你想开始战斗，让#b组队队长#k来找我谈谈。");
                return;
            }

            if (!CheckTeamMemberCount(2, 6))
            {
                await SayOK("你的队伍人数不够。你需要一个有 #b2#k - #r6#k 名成员的队伍，并且他们应该和你在同一地图上。");
                return;
            }

            if (!CheckTeamMemberLevel(51, 70))
            {
                await SayOK("确保你的队伍中的每个人都处于正确的等级范围内（51~70）！");
                return;
            }

            if (!CheckTeamMemberMap())
            {
                await SayOK("有一些队员不在地图上！");
                return;
            }

            // 选择空闲的嘉年华 2 场地
            var allCPQ2 = Enumerable.Range(0, 3)
                .ToDictionary(x => x, x => GetEventManager<MonsterCarnivalEventManager>(nameof(PQ_CPQ2) + (x + 1).ToString()));

            var available = new List<int>();
            foreach (var (idx, em) in allCPQ2)
            {
                var dEim = em.GetOnlyEventInstanceManager<MonsterCarnivalEventInstanceManager>();
                if (dEim == null || dEim.InstanceStatus == InstanceStatus.Recruitment)
                {
                    available.Add(idx);
                }
            }

            if (available.Count == 0)
            {
                await SayOK("所有的怪物嘉年华场地目前都在使用中！请稍后再试。");
                return;
            }

            var msg = "你想进入哪一间怪物嘉年华 2 场地？#b\r\n";
            foreach (var idx in available)
            {
                msg += $"#L{idx}# 场地 {idx + 1}#l\r\n";
            }

            var option = await AskMenu(msg);
            if (!available.Contains(option))
            {
                await SayOK("房间目前已满。");
                return;
            }

            var em2 = allCPQ2[option];
            var eim = em2.GetOnlyEventInstanceManager<MonsterCarnivalEventInstanceManager>();
            if (eim == null)
            {
                var r = await em2.StartInstance(getPlayer());
                await SayOK(em2.HandleCreateInstanceResult(r, c));
            }
            else if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                await SayOK(em2.GetTemplate.HandleJoinRequestResult(await em2.SendJoinRequest(getPlayer()), c));
            }
            else
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.CPQ_Error)));
            }
        }


        // Npc: 2042007 — 怪物嘉年华 2 结束结算
        public async Task mc2_move()
        {
            var talkMap = getMapId();
            if (talkMap == 980030010)
            {
                await SayNext("希望你在怪物嘉年华上玩得开心！");
                await warp(980030000, 0);
                return;
            }

            var eim = GetEventInstanceTrust<MonsterCarnivalEventInstanceManager>();
            if (getPlayer().getParty() == null)
            {
                await warp(980030000, 0);
                return;
            }

            var cp = eim.GetPlayerData(getPlayer().Id)?.TotalCP ?? 0;
            var (idx, exp) = cp switch
            {
                >= 300 => (0, 35000),
                >= 100 => (1, 25000),
                >= 50 => (2, 15000),
                _ => (3, 5000),
            };

            var result = eim.IsWinner(getPlayer()) ? idx : idx + 4;
            await SayNext("恭喜你的胜利！希望下次也能有同样出色的表现！\r\n\r\n#b你的成绩：#r" + "ABCD"[idx] + "#k");
            await gainExp(exp);
            await eim.GiveClearReward(getPlayer(), result);
            await warp(980030000, 0);
        }


        // Npc: 2042008, 2042009 — 离开嘉年华 2 大厅
        public async Task mc2_roomout()
        {
            await warpParty(980030000, 4);
            var eim = getEventInstance();
            if (eim != null)
            {
                await eim.DisposeAsync();
            }
        }

    }
}
