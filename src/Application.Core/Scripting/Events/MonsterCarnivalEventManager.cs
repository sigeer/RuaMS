using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Resources.Messages;
using Application.Templates.Map;
using server.maps;
using tools;

namespace Application.Core.Scripting.Events
{
    public abstract class MonsterCarnivalEventManager : PartyQuestEventManager
    {
        public int RegistrationTime { get; init; }
        public int PrepareTime { get; init; }
        public MapMonsterCarnivalTemplate MapMonsterCarnivalTemplate { get; }

        public MonsterCarnivalEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {
            // 对整个FB而言，团队是后组建的，但是发起时仍要完整队伍
            PartyLeaderRequired = true;

            RegistrationTime = 180;

            MapMonsterCarnivalTemplate = MapFactory.Instance.GetMapTemplate(EntryMap).MonsterCarnival!;

            EventTime = MapMonsterCarnivalTemplate.TimeDefault - 10;
        }

        public TEim? GetOnlyEventInstanceManager<TEim>() where TEim : MonsterCarnivalEventInstanceManager => getInstance(Name) as TEim;
        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new MonsterCarnivalEventInstanceManager(ChannelServer, Name, instanceName);
        }


        //public bool Check2(MonsterCarnivalEventInstanceManager eim)
        //{
        //    try
        //    {
        //        var t0 = iv.CallFunction("getEligibleParty", eim.Team0.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];
        //        var t1 = iv.CallFunction("getEligibleParty", eim.Team1.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];

        //        return t0.Count == t1.Count && t0.Count >= eim.Room.MinCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex, "Script: {Script}", _name);
        //        return false;
        //    }
        //}
        public override bool OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            return true;
        }

        public override List<Player> GetEligibleParty(Player leader)
        {
            var party = leader.getParty();
            if (party == null)
            {
                return [];
            }

            var members = party.GetChannelMembers(ChannelServer)
                .Where(x => x.MapModel == leader.MapModel && x.MapModel.Id == RecruitMap).ToList();

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

        public override void SetupInstance(AbstractEventInstanceManager eim, Player leader, List<Player> members)
        {
            var pEim = (eim as MonsterCarnivalEventInstanceManager)!;
            pEim.RegisterParty(members);
            base.SetupInstance(eim, leader, members);
        }

        public async Task<MCJoinInstanceResult> SendJoinRequest(Player chr)
        {
            var eim = getInstance(Name) as MonsterCarnivalEventInstanceManager;
            if (eim == null)
                return MCJoinInstanceResult.Unknown;

            if (chr.Party == 0)
                return MCJoinInstanceResult.RequiredParty;

            if (!chr.isLeader())
                return MCJoinInstanceResult.RequiredLeader;

            var members = GetEligibleParty(chr);
            if (members.Count == 0)
                return MCJoinInstanceResult.Requirement;

            if (eim.InstanceStatus != InstanceStatus.Recruitment)
                return MCJoinInstanceResult.NotInWaiting;

            if (eim.RequestTeam != null)
                return MCJoinInstanceResult.AnthorRequest;

            eim.RequestTeam = new TeamRegistry(members);
            // send challenge
            if (await ChannelServer.NodeService.PluginManager.StartNpcConversation(
                eim.getLeader()!.Client,
                2042001,
                null,
                "mc_enter1"))
            {
                return MCJoinInstanceResult.Success;
            }
            return MCJoinInstanceResult.AnthorRequest;
        }


        public override void OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            base.OnPlayerUnregister(eim, chr);

            chr.setTeam(-1);

            switch (eim.InstanceStatus)
            {
                case InstanceStatus.Recruitment:
                case InstanceStatus.Prepare:
                case InstanceStatus.InProgress:
                    var pEim = eim as MonsterCarnivalEventInstanceManager;
                    eim.Pink(nameof(ClientMessage.CPQ_PlayerExit), pEim.GetPlayerTeam(chr.Id) == 0 ? "TeamRed" : "TeamBlue");
                    if (eim.InstanceStatus != InstanceStatus.InProgress)
                    {
                        eim.Dispose();
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            base.OnPlayerDied(eim, chr);

            var pEim = eim as MonsterCarnivalEventInstanceManager;
            var playerData = pEim.GetPlayerData(chr.Id)!;
            var losing = Math.Min(playerData.AvailableCP, MapMonsterCarnivalTemplate.DeathCP);
            pEim.GainCP(chr, -losing);
            foreach (var item in eim.getPlayers())
            {
                item.sendPacket(PacketCreator.CPQ_PlayerDied(chr.Name, losing, playerData.TeamFlag));
            }
        }

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            base.OnMobKilled(eim, mob, killer);

            if (mob.getCP() > 0 && killer is Player chr)
            {
                var pEim = eim as MonsterCarnivalEventInstanceManager;
                pEim.GainCP(chr, mob.getCP());
            }
        }

        public override void ClearPQ(AbstractEventInstanceManager eim)
        {
            base.ClearPQ(eim);

            var pEim = eim as MonsterCarnivalEventInstanceManager;

            foreach (var chr in eim.getPlayers())
            {
                var rewardMap = eim.getInstanceMap(pEim.IsWinner(chr) ? MapMonsterCarnivalTemplate.RewardMapWin : MapMonsterCarnivalTemplate.RewardMapLose);
                if (rewardMap != null)
                {
                    chr.changeMap(rewardMap);
                    chr.setTeam(-1);
                    chr.dispelDebuffs();
                }
            }
        }

        public virtual void OnBattlePrepare(AbstractEventInstanceManager eim) { }
        public virtual void OnBattleStarted(AbstractEventInstanceManager eim)
        {
            var pEim = (eim as MonsterCarnivalEventInstanceManager)!;

            pEim.EventMap.allowSummonState(true);
            foreach (var mc in eim.getPlayers())
            {
                var playerData = pEim.GetPlayerData(mc.Id)!;

                mc.setTeam(playerData.TeamFlag);
                mc.changeMap(pEim.EventMap, pEim.EventMap.GetInitPortal(playerData.TeamFlag));
                mc.sendPacket(PacketCreator.startMonsterCarnival(playerData, pEim.GetPlayerTeamData(mc.Id)!, pEim.GetPlayerEnemyTeamData(mc.Id)!));
                mc.LightBlue(nameof(ClientMessage.CPQ_Entry));
            }


        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            var e = (eim as MonsterCarnivalEventInstanceManager)!;
            switch (eim.InstanceStatus)
            {
                case InstanceStatus.Recruitment:
                    eim.Dispose();
                    break;
                case InstanceStatus.Prepare:
                    e.StartBattle();
                    break;
                case InstanceStatus.InProgress:
                    var team0 = e.Teams[0];
                    var team1 = e.Teams[1];

                    if (team0 == null || team1 == null)
                    {
                        // 数据不正常
                        eim.Dispose();
                        return;
                    }

                    if (team0.TotalCP != team1.TotalCP)
                    {
                        var map = eim.getInstanceMap(EntryMap);
                        map?.killAllMonsters();
                        map?.allowSummonState(false);

                        bool redWin = team0.TotalCP > team1.TotalCP;
                        e.WinnerTeamIndex = (sbyte)(team0.TotalCP > team1.TotalCP ? 0 : 1);


                        foreach (var chr in eim.getPlayers())
                        {
                            var effect = e.IsWinner(chr) ? MapMonsterCarnivalTemplate.EffectWin : MapMonsterCarnivalTemplate.EffectLose;
                            if (!string.IsNullOrEmpty(effect))
                            {
                                chr.sendPacket(PacketCreator.showEffect(effect));
                            }

                            var sound = e.IsWinner(chr) ? MapMonsterCarnivalTemplate.SoundWin : MapMonsterCarnivalTemplate.SoundLose;
                            if (!string.IsNullOrEmpty(sound))
                            {
                                chr.sendPacket(PacketCreator.playSound(sound));
                            }

                            chr.dispelDebuffs();
                        }

                        eim.Schedule(ClearPQ, MapMonsterCarnivalTemplate.TimeFinish * 1000);
                    }
                    else
                    {
                        eim.Pink(nameof(ClientMessage.CPQ_ExtendTime));
                        eim.restartEventTimer(MapMonsterCarnivalTemplate.TimeExpand * 1000);
                    }
                    break;
                default:
                    break;
            }
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_EntryLobby));
                case CreateInstanceResult.RequiredParty:
                    return "在你加入战斗之前，你需要先创建一个队伍！";
                case CreateInstanceResult.RequiredLeader:
                    return "如果你想开始战斗，让#b队长#k和我对话。";
                case CreateInstanceResult.Requirement:
                    return "队伍不满足条件。";
                case CreateInstanceResult.LobbyLimited:
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_Error));
            }
        }
        public string? HandleJoinRequestResult(MCJoinInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case MCJoinInstanceResult.Success:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_ChallengeRoomSent));

                case MCJoinInstanceResult.RequiredParty:
                    return "在你加入战斗之前，你需要先创建一个队伍！";

                case MCJoinInstanceResult.RequiredLeader:
                    return "如果你想开始战斗，让#b队长#k和我对话。";

                case MCJoinInstanceResult.Requirement:
                    return "队伍不满足条件。需要与被挑战的队伍人数一致！";

                case MCJoinInstanceResult.NotInWaiting:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_FindError));

                case MCJoinInstanceResult.AnthorRequest:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_ChallengeRoomAnswer));
                case MCJoinInstanceResult.Unknown:

                default:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_Error));
            }
        }
    }
}
