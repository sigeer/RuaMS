using Application.Core.Channel;
using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.QuestRecordEx;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Core.Server.maps;
using Application.Resources.Messages;
using Application.Templates.Map;
using tools;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractMonsterCarnivalEventTemplate : AbstractPartyQuestEventTemplate
    {
        public int RegistrationTime { get; init; }
        public int PrepareTime { get; init; }
        public MapMonsterCarnivalTemplate MapMonsterCarnivalTemplate { get; }

        public AbstractMonsterCarnivalEventTemplate(string name, int eventMap) : base(name)
        {
            PartyLeaderRequired = true;

            MapMonsterCarnivalTemplate = MapFactory.Instance.GetMapTemplate(eventMap).MonsterCarnival!;

            RegistrationTime = 180;
            EventTime = MapMonsterCarnivalTemplate.TimeDefault - 10;
            PrepareTime = 10;
        }


        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new MonsterCarnivalEventManager(worldChannel, this);
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            var pEim = eim as MonsterCarnivalEventInstanceManager;
            var map = await pEim.GetEventMap();

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            await base.OnPlayerMapChanged(eim, player, mapid);

            var pEim = (eim as MonsterCarnivalEventInstanceManager)!;

            var playerData = pEim.GetPlayerData(player.Id)!;
            await player.SendPacket(PacketCreator.startMonsterCarnival(playerData, pEim.GetPlayerTeamData(player.Id)!, pEim.GetPlayerEnemyTeamData(player.Id)!));
        }
        public override List<Player> GetEligibleParty(Player leader)
        {
            var party = leader.getParty();
            if (party == null)
            {
                return [];
            }

            var members = party.GetChannelMembers(leader.Client.CurrentServer)
                .Where(x => x.MapModel == leader.MapModel && x.MapModel.Id == RecruitMap).ToList();

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }
        /// <summary>
        /// 挑战另一支队伍（由被挑战方确认）的结果文案。
        /// 怪物嘉年华的文案属于脚本插件，因此只在这里声明契约，具体文案由 <c>MonsterCarnivalRoomTemplate</c> 提供。
        /// </summary>
        public abstract string? HandleJoinRequestResult(MCJoinInstanceResult r, IChannelClient c);
        public virtual async Task OnBattleStarted(MonsterCarnivalEventInstanceManager pEim)
        {
            var map = await pEim.GetEventMap();
            foreach (var mc in pEim.getPlayers())
            {
                var playerData = pEim.GetPlayerData(mc.Id)!;

                mc.setTeam(playerData.TeamFlag);
                await mc.changeMap(map, map.GetInitPortal(playerData.TeamFlag));
                await mc.SendPacket(PacketCreator.startMonsterCarnival(playerData, pEim.GetPlayerTeamData(mc.Id)!, pEim.GetPlayerEnemyTeamData(mc.Id)!));
                await mc.LightBlue(ClientMessage.CPQ_Entry);
            }
        }
        public override async Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            await base.OnPlayerUnregister(eim, chr);

            chr.setTeam(-1);

            switch (eim.InstanceStatus)
            {
                case InstanceStatus.Recruitment:
                case InstanceStatus.Prepare:
                case InstanceStatus.InProgress:
                    var pEim = eim as MonsterCarnivalEventInstanceManager;
                    await eim.Pink(ClientMessage.CPQ_PlayerExit, pEim.GetPlayerTeam(chr.Id) == 0 ? "TeamRed" : "TeamBlue");
                    if (eim.InstanceStatus != InstanceStatus.InProgress)
                    {
                        await eim.DisposeAsync();
                    }
                    break;
                default:
                    break;
            }
        }
        public override async Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            await base.OnPlayerDied(eim, chr);

            var pEim = eim as MonsterCarnivalEventInstanceManager;
            var playerData = pEim.GetPlayerData(chr.Id)!;
            var losing = Math.Min(playerData.AvailableCP, MapMonsterCarnivalTemplate.DeathCP);
            await pEim.GainCP(chr, -losing);
            foreach (var item in eim.getPlayers())
            {
                await item.SendPacket(PacketCreator.CPQ_PlayerDied(chr.Name, losing, playerData.TeamFlag));
            }
        }
        public override Task<bool> OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            return Task.FromResult(true);
        }
        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            await base.OnMobKilled(eim, mob, killer);

            if (mob.getCP() > 0 && killer is Player chr)
            {
                var pEim = eim as MonsterCarnivalEventInstanceManager;
                await pEim.GainCP(chr, mob.getCP());
            }
        }
        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            var e = (eim as MonsterCarnivalEventInstanceManager)!;
            switch (eim.InstanceStatus)
            {
                case InstanceStatus.Recruitment:
                    await eim.DisposeAsync();
                    break;
                case InstanceStatus.Prepare:
                    await e.StartBattle();
                    break;
                case InstanceStatus.InProgress:
                    var team0 = e.Teams[0];
                    var team1 = e.Teams[1];

                    if (team0 == null || team1 == null)
                    {
                        // 数据不正常
                        await eim.DisposeAsync();
                        return;
                    }

                    if (team0.TotalCP != team1.TotalCP)
                    {
                        var map = await eim.getInstanceMap(EntryMap);
                        map?.killAllMonsters();
                        map?.allowSummonState(false);

                        bool redWin = team0.TotalCP > team1.TotalCP;
                        e.WinnerTeamIndex = (sbyte)(team0.TotalCP > team1.TotalCP ? 0 : 1);


                        foreach (var chr in eim.getPlayers())
                        {
                            var effect = e.IsWinner(chr) ? MapMonsterCarnivalTemplate.EffectWin : MapMonsterCarnivalTemplate.EffectLose;
                            if (!string.IsNullOrEmpty(effect))
                            {
                                await chr.SendPacket(FieldEffectPacket.Screen(effect));
                            }

                            var sound = e.IsWinner(chr) ? MapMonsterCarnivalTemplate.SoundWin : MapMonsterCarnivalTemplate.SoundLose;
                            if (!string.IsNullOrEmpty(sound))
                            {
                                await chr.SendPacket(FieldEffectPacket.Sound(sound));
                            }

                            await chr.dispelDebuffs();
                        }

                        eim.Schedule(ClearPQ, MapMonsterCarnivalTemplate.TimeFinish * 1000);
                    }
                    else
                    {
                        await eim.Pink(ClientMessage.CPQ_ExtendTime);
                        await eim.restartEventTimer(MapMonsterCarnivalTemplate.TimeExpand * 1000);
                    }
                    break;
                default:
                    break;
            }
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            var pEim = eim as MonsterCarnivalEventInstanceManager;

            foreach (var chr in eim.getPlayers())
            {
                var rewardMap = await eim.getInstanceMap(pEim.IsWinner(chr) ? MapMonsterCarnivalTemplate.RewardMapWin : MapMonsterCarnivalTemplate.RewardMapLose);
                if (rewardMap != null)
                {
                    await chr.changeMap(rewardMap);
                    chr.setTeam(-1);
                    await chr.dispelDebuffs();
                }
            }
        }

        protected override async Task StartPartyQuest(Player chr)
        {
            if (QuestId <= 0)
                return;

            if (chr.GetQuestRecordEx(QuestId) is ConfrontQuestEx model)
            {
                model.Try++;

                await chr.FlushQuestRecordEx(model);
            }
        }

        protected override async Task AbortPartyQuest(Player chr)
        {
            await base.AbortPartyQuest(chr);

            if (chr.GetQuestRecordEx(QuestId) is ConfrontQuestEx model)
            {
                model.GiveUpCount++;
                await chr.FlushQuestRecordEx(model);
            }
        }

        protected override async Task CompletePartyQuest(AbstractEventInstanceManager eim, Player chr, long now)
        {
            if (QuestId <= 0)
                return;

            if (chr.GetQuestRecordEx(QuestId) is ConfrontQuestEx model)
            {
                var pEim = eim as MonsterCarnivalEventInstanceManager;
                if (pEim.IsWinner(chr))
                {
                    model.VicCount++;
                }
                else
                {
                    model.LoseCount++;
                }

                await chr.FlushQuestRecordEx(model);
            }
        }
    }
}
