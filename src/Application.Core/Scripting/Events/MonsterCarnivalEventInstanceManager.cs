using Acornima;
using Application.Core.Game.GameEvents.CPQ;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.Game.Relation;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Scripting.Events
{
    public class MonsterCarnivalEventInstanceManager : AbstractEventInstanceManager
    {
        public MonsterCarnivalPreparationRoom Room { get; set; } = null!;
        public IMap LobbyMap { get; set; } = null!;
        public ICPQMap EventMap { get; set; } = null!;
        public int CurrentStage { get; set; }

        /// <summary>
        /// 红队
        /// </summary>
        public TeamRegistry? Team0 { get; set; }
        public MonsterCarnivalTeam? MCTeam0 { get; set; }
        /// <summary>
        /// 蓝队
        /// </summary>
        public TeamRegistry? Team1 { get; set; }
        public MonsterCarnivalTeam? MCTeam1 { get; set; }

        public MonsterCarnivalEventInstanceManager(AbstractInstancedEventManager em, string name) : base(em, name)
        {
        }

        public void Initialize(TeamRegistry team, MonsterCarnivalPreparationRoom room)
        {
            Team0 = team;
            Room = room;
            LobbyMap = getInstanceMap(Room.Map)!;
            EventMap = (getInstanceMap(Room.Map + (Room.RecruitMap == 980030000 ? 100 : 1)) as ICPQMap)!;
            CurrentStage = 0;
        }


        public override void unregisterPlayer(Player chr)
        {
            base.unregisterPlayer(chr);

            if (chr.Party == Team0?.Team)
            {
                Team0.EligibleMembers.Remove(chr);
            }
            if (chr.Party == Team1?.Team)
            {
                Team1.EligibleMembers.Remove(chr);
            }

            chr.ClearMC();
        }
        public void AcceptChallenge(bool accept)
        {
            if (accept)
            {
                if (Team1 != null)
                {
                    foreach (var item in Team1.EligibleMembers)
                    {
                        registerPlayer(item);
                    }
                    invokeScriptFunction("setStage", this, 1);
                }

            }
            else
            {
                if (Team1 != null)
                {
                    var requestLeader = getLeader();
                    if (requestLeader != null)
                    {
                        requestLeader.Pink(nameof(ClientMessage.CPQ_ChallengeRoomDenied));
                        return;
                    }
                }
                Team1 = null;
            }
        }


        public override void Dispose()
        {
            foreach (var mc in getPlayers())
            {
                if (mc.IsOnlined)
                {
                    mc.ClearMC();
                    mc.ForcedWarpOut();
                }
            }
            base.Dispose();

            Team0 = null;
            Team1 = null;
            MCTeam0?.Dispose();
            MCTeam1?.Dispose();
        }


        /// <summary>
        /// 先complete  过 10s finish
        /// </summary>
        /// <exception cref="Exception"></exception>
        public bool Complete()
        {
            if (MCTeam0 == null || MCTeam1 == null)
            {
                // 数据不正常
                Dispose();
                return false;
            }
            if (MCTeam0.TotalCP != MCTeam1.TotalCP)
            {
                EventMap.killAllMonsters();

                bool redWin = MCTeam0.TotalCP > MCTeam1.TotalCP;

                this.MCTeam0.Complete(redWin);
                this.MCTeam1.Complete(!redWin);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartEvent()
        {
            if (Team1 == null)
                return;

            MCTeam0 = new MonsterCarnivalTeam(this, Team0!, 0);
            MCTeam1 = new MonsterCarnivalTeam(this, Team1, 1);

            MCTeam0.Initialize(MCTeam1);
            MCTeam1.Initialize(MCTeam0);
        }

        public override void playerKilled(Player chr)
        {
            base.playerKilled(chr);

            int losing = EventMap.DeathCP;
            if (chr.AvailableCP < losing)
            {
                losing = chr.AvailableCP;
            }
            chr.gainCP(-losing);
            EventMap.broadcastMessage(PacketCreator.CPQ_PlayerDied(chr.Name, losing, chr.MCTeam!.TeamFlag));
        }

        public override void monsterKilled(Player chr, Monster mob)
        {
            base.monsterKilled(chr, mob);

            if (mob.getCP() > 0)
            {
                chr.gainCP(mob.getCP());
            }
        }

        public override void changedMap(Player chr, int mapId)
        {
            base.changedMap(chr, mapId);
        }

        public override void afterChangedMap(Player chr, int mapId)
        {
            base.afterChangedMap(chr, mapId);

            if (chr.MCTeam != null)
                chr.sendPacket(PacketCreator.startMonsterCarnival(chr));
        }

        public override void setEventCleared()
        {
            base.setEventCleared();

            MCTeam0?.MoveToReward();
            MCTeam1?.MoveToReward();
        }

        public int GetAveLevel()
        {
            return Team0.EligibleMembers.Sum(x => x.Level) / Team0.EligibleMembers.Count;
        }

        public int GetRoomSize()
        {
            return Team0.EligibleMembers.Count;
        }

        public bool IsWinner(Player chr)
        {
            return chr.MCTeam?.IsWinner ?? false;
        }
    }
}
