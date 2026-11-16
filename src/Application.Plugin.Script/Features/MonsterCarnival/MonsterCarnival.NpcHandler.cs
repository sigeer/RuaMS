using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.MapObjects;

namespace Application.Plugin.Script.Features.MonsterCarnival
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2042000
        public async Task mc_enter()
        {
            var talkMap = getMapId();
            if (talkMap == 980000000)
            {
                var msg = this.GetTalkMessage(ScriptMessage.CPQ_PickRoom) + "#b";
                var o = msg.Length;
                var roomName = this.GetTalkMessage(ScriptMessage.CPQ_Room);
                var levelName = this.GetTalkMessage(ScriptMessage.Level);

                var allCPQ1 = Enumerable.Range(0, 3).ToDictionary(x => x, x => GetEventManager<MonsterCarnivalEventManager>(MonsterCarnivalFeature.Cpq1EventNames[x]));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.MC_AllBattle));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.CPQ_NoEmptyRoom));
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
                        await this.Pink(em.HandleCreateInstanceResult(r, c) ?? "");
                    }
                }
                else if (eim.InstanceStatus == InstanceStatus.Recruitment)
                {
                    await SayOK(em.GetTemplate.HandleJoinRequestResult(await em.SendJoinRequest(getPlayer()), c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CPQ_Error));
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
                await this.Pink(this.GetTalkMessage(ScriptMessage.CPQ_EntryLobby));
                return;
            }

            var teamMembers = eim.RequestTeam.EligibleMembers;
            var snd = "";
            for (var i = 0; i < teamMembers.Count; i++)
            {
                snd += $"#b{this.GetTalkMessage(ScriptMessage.Name)}: {teamMembers[i].Name} / ({this.GetTalkMessage(ScriptMessage.Level)}: {teamMembers[i].Level}) / {GetJobName(teamMembers[i].JobModel)}#k\r\n\r\n";
            }

            await eim.AcceptChallenge(await SayAcceptDecline(snd + this.GetTalkMessage(ScriptMessage.MC_WantFight)));
        }

        // Npc: 2042002
        public async Task mc_move()
        {
            var talkMap = getMapId();
            if (talkMap == 980000010)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.MC_HopeHad));
                await warp(980000000, 0);
                return;
            }
            else if (talkMap == 980030010)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.MC_HopeHad));
                await warp(980030000, 0);
                return;
            }

            var pEim = getEventInstance();
            if (pEim is MonsterCarnivalEventInstanceManager eim)
            {
                var idx = Array.FindIndex([300, 100, 50, 0], x => eim.GetPlayerData(getPlayer().Id)?.TotalCP >= x) + (eim.IsWinner(getPlayer()) ? 0 : 4);
                List<string?> messageList = [
                    this.GetTalkMessage(ScriptMessage.MC_CongratulationsVictory),
                    this.GetTalkMessage(ScriptMessage.MC_CongratulationsVictoryAwesome),
                    this.GetTalkMessage(ScriptMessage.MC_CongratulationsVictorySome),
                    this.GetTalkMessage(ScriptMessage.MC_CongratulationsVictoryThough),

                    this.GetTalkMessage(ScriptMessage.MC_UnfortunatelyEither),
                    this.GetTalkMessage(ScriptMessage.MC_UnfortunatelyEitherDrew),
                    this.GetTalkMessage(ScriptMessage.MC_UnfortunatelyEitherDrewLost),
                    this.GetTalkMessage(ScriptMessage.MC_UnfortunatelyEitherEqualized)
                ];

                await SayNext(messageList[idx]);

                await eim.GiveClearReward(getPlayer(), idx);
                await warp(980000000);
            }
            else
            {
                var talk = this.GetTalkMessage(ScriptMessage.MC_MainMenu);

                var option = await AskMenu(talk);

                switch (option)
                {
                    case 0:
                        var targetEm = GetEventManager<MonsterCarnivalEventManager>(MonsterCarnivalFeature.Cpq1EventNames[0]);
                        if (getLevel() < targetEm.MinLevel)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.MC_NeedLevel, targetEm.MinLevel));
                        }
                        else if (getLevel() > targetEm.MaxLevel)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.MC_LevelRange, targetEm.MinLevel, targetEm.MaxLevel));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_NeedCreate));
                return;
            }

            if (!isLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_WantStart));
                return;
            }

            if (!CheckTeamMemberCount(2, 6))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_HaveEnough));
                return;
            }

            if (!CheckTeamMemberLevel(51, 70))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_MakeSure));
                return;
            }

            if (!CheckTeamMemberMap())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_SomeParty));
                return;
            }

            // 选择空闲的嘉年华 2 场地
            var allCPQ2 = Enumerable.Range(0, 3)
            .ToDictionary(x => x, x => GetEventManager<MonsterCarnivalEventManager>(MonsterCarnivalFeature.Cpq2EventNames[x]));

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
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_AllMonster));
                return;
            }

            var msg = this.GetTalkMessage(ScriptMessage.MC_MonsterCarnival);
            foreach (var idx in available)
            {
                msg += this.GetTalkMessage(ScriptMessage.MC_RoomEntry, idx, idx + 1);
            }

            var option = await AskMenu(msg);
            if (!available.Contains(option))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MC_RoomCurrently));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.CPQ_Error));
            }
        }

        // Npc: 2042007 — 怪物嘉年华 2 结束结算
        public async Task mc2_move()
        {
            var talkMap = getMapId();
            if (talkMap == 980030010)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.MC_HopeHad));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.MC_ResultVictory, "ABCD"[idx]));
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
