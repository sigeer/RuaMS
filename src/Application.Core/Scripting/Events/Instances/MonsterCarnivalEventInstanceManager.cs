using Application.Core.Game.GameEvents.CPQ;
using Application.Core.Game.Maps.Specials;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using tools;

namespace Application.Core.scripting.Events.Instances
{
    public class MonsterCarnivalEventInstanceManager : AbstractEventInstanceManager
    {
        /// <summary>
        /// 0. 红队, 1. 蓝队
        /// </summary>
        public MonsterCarnivalTeamData[] Teams = new MonsterCarnivalTeamData[2];

        public sbyte WinnerTeamIndex = -1;
        public Dictionary<int, MonsterCarnivalData> PlayerData { get; } = new();
        public TeamRegistry? RequestTeam { get; set; }

        public override MonsterCarnivalEventManager EventManager { get; }
        public MonsterCarnivalEventInstanceManager(MonsterCarnivalEventManager em, string name) : base(em, name)
        {
            EventManager = em;
            Teams = new MonsterCarnivalTeamData[2] { new MonsterCarnivalTeamData(0, []), new MonsterCarnivalTeamData(1, []) };
        }

        public async Task<ICPQMap> GetEventMap()
        {
            return (await getMapInstance(EventManager.GetTemplate.EntryMap)) as ICPQMap;
        }
        /// <summary>
        /// 开始战斗
        /// </summary>
        public async Task StartBattle()
        {

            if (InstanceStatus == InstanceStatus.Recruitment && EventManager.GetTemplate.PrepareTime > 0)
            {
                InstanceStatus = InstanceStatus.Prepare;

                await restartEventTimer(EventManager.GetTemplate.PrepareTime * 1000);
                EventManager.GetTemplate.OnBattlePrepare(this);
                return;
            }

            if (InstanceStatus == InstanceStatus.Recruitment || InstanceStatus == InstanceStatus.Prepare)
            {
                InstanceStatus = Abstraction.InstanceStatus.InProgress;

                foreach (var chr in getPlayers())
                {
                    await EventManager.GetTemplate.OnPlayerEntry(this, chr);
                }
                await restartEventTimer(EventManager.EventTime * 1000);
                await EventManager.GetTemplate.OnBattleStarted(this);
                return;
            }
        }

        public void RegisterParty(List<Player> players)
        {
            Teams[0].EligibleMembers.Clear();
            Teams[0].EligibleMembers.AddRange(players);
            foreach (var chr in players)
            {
                PlayerData[chr.Id] = new MonsterCarnivalData(0);
            }
        }


        public async Task AcceptChallenge(bool accept)
        {
            if (accept)
            {
                if (RequestTeam != null)
                {
                    Teams[1].EligibleMembers.Clear();
                    Teams[0].EligibleMembers.AddRange(RequestTeam.EligibleMembers);

                    foreach (var chr in RequestTeam.EligibleMembers)
                    {
                        await registerPlayer(chr);
                        PlayerData[chr.Id] = new MonsterCarnivalData(1);
                    }

                    await StartBattle();
                }

            }
            else
            {
                if (RequestTeam != null)
                {
                    foreach (var item in RequestTeam.EligibleMembers)
                    {
                        await item.Pink(nameof(ClientMessage.CPQ_ChallengeRoomDenied));
                    }
                }
                RequestTeam = null;
            }
        }


        public int GetAveLevel()
        {
            if (Teams[0].EligibleMembers.Count == 0)
            {
                return 0;
            }

            return Teams[0].EligibleMembers.Sum(x => x.Level) / Teams[0].EligibleMembers.Count;
        }

        public int GetRoomSize()
        {
            return Teams[0].EligibleMembers.Count;
        }

        public bool IsWinner(Player chr)
        {
            return WinnerTeamIndex != -1 && GetPlayerTeam(chr.Id) == WinnerTeamIndex;
        }

        public sbyte GetPlayerTeam(int chrId) => GetPlayerData(chrId)?.TeamFlag ?? -1;
        public MonsterCarnivalTeamData? GetPlayerTeamData(int chrId)
        {
            var idx = GetPlayerTeam(chrId);
            if (idx == -1)
            {
                return null;
            }

            return Teams[idx];
        }

        public MonsterCarnivalTeamData? GetPlayerEnemyTeamData(int chrId)
        {
            var idx = GetPlayerTeam(chrId);
            if (idx == -1)
            {
                return null;
            }

            return Teams[1 - idx];
        }

        public List<Player> GetEnemyMembers(Player chr)
        {
            var chrTeam = GetPlayerTeam(chr.Id);
            if (chrTeam == -1)
            {
                return [];
            }
            return getPlayers().Where(x => GetPlayerTeam(x.Id) != chrTeam && x.MapModel == chr.MapModel).ToList();
        }

        public MonsterCarnivalData? GetPlayerData(int playerId) => PlayerData.GetValueOrDefault(playerId);
        public async Task GainCP(Player player, int gain)
        {
            var data = GetPlayerData(player.Id);
            if (data == null)
            {
                return;
            }

            var teamData = Teams[data.TeamFlag];
            if (teamData != null)
            {
                if (gain > 0)
                {
                    teamData.TotalCP += gain;
                    teamData.AvailableCP += gain;

                    data.TotalCP += gain;
                    data.AvailableCP += gain;
                }
                else
                {
                    teamData.AvailableCP += gain;
                    data.AvailableCP += gain;
                }

                await player.SendPacket(PacketCreator.CPUpdate(false, data.AvailableCP, data.TotalCP, data.TeamFlag));
                await player.MapModel.broadcastMessage(PacketCreator.CPUpdate(true, teamData.AvailableCP, teamData.TotalCP, teamData.TeamFlag));
            }
        }

        public void Summon(Player chr)
        {
            var teamIdx = GetPlayerTeam(chr.Id);
            if (teamIdx != -1)
            {
                Teams[teamIdx].SummonedMonster++;
            }

        }
        public bool CanSummon(Player chr)
        {
            var teamIdx = GetPlayerTeam(chr.Id);
            if (teamIdx != -1)
            {
                return Teams[teamIdx].SummonedMonster < EventManager.GetTemplate.MapMonsterCarnivalTemplate.MaxMobs;
            }
            return false;
        }

        public bool CanGuardian(Player chr, ICPQMap map)
        {
            var teamIdx = GetPlayerTeam(chr.Id);
            if (teamIdx != -1)
            {
                var strFlag = teamIdx.ToString();
                return map.getAllReactors().Count(x => x.getName().Substring(0, 1) == strFlag) < EventManager.GetTemplate.MapMonsterCarnivalTemplate.MaxReactors;
            }
            return false;
        }
    }
}
