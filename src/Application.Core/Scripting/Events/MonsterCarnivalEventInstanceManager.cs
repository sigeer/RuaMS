using Application.Core.Game.GameEvents.CPQ;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.Game.Relation;
using Application.Resources.Messages;
using constants.String;
using server.events.gm;
using server.partyquest;
using tools;

namespace Application.Core.Scripting.Events
{
    public class MonsterCarnivalEventInstanceManager : AbstractEventInstanceManager
    {
        public MonsterCarnivalPreparationRoom Room { get; set; } = null!;
        public IMap LobbyMap { get; set; } = null!;
        public ICPQMap EventMap { get; set; } = null!;
        public IMap ExitMap { get; set; } = null!;
        public int CurrentStage { get; set; }

        /// <summary>
        /// 红队
        /// </summary>
        public Team Team0 { get; set; } = null!;
        /// <summary>
        /// 蓝队
        /// </summary>
        public Team? Team1 { get; set; }

        public MonsterCarnivalEventInstanceManager(AbstractInstancedEventManager em, string name) : base(em, name)
        {
        }

        public void Initialize(Team team, MonsterCarnivalPreparationRoom room)
        {
            Team0 = team;
            Room = room;
            LobbyMap = getInstanceMap(Room.Map)!;
            EventMap = (getInstanceMap(Room.Map + Room.RecruitMap == 980030000 ? 100 : 1) as ICPQMap)!;
            ExitMap = getInstanceMap(EventMap.getForcedReturnId())!;
            CurrentStage = 0;
        }

        public override void unregisterPlayer(IPlayer chr)
        {
            base.unregisterPlayer(chr);
            chr.resetCP();
            chr.setTeam(-1);
        }
        public void AcceptChallenge(bool accept)
        {
            if (accept)
            {
                if (Team1 != null)
                {
                    var requestLeader = Team1.GetChannelLeader(getEm().getChannelServer());
                    if (requestLeader != null)
                    {
                        registerParty(requestLeader);
                        invokeScriptFunction("setStage", this, 1);
                        return;
                    }
                }

            }
            else
            {
                if (Team1 != null)
                {
                    var requestLeader = Team1.GetChannelLeader(getEm().getChannelServer());
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
                    mc.resetCP();
                    mc.setTeam(-1);
                    mc.setMonsterCarnival(null);
                    if (mc.MapModel == EventMap)
                    {
                        mc.changeMap(ExitMap);
                    }
                    mc.setEventInstance(null);
                }
            }
            base.Dispose();

            Team0.MCTeam?.Dispose();
            Team1?.MCTeam?.Dispose();
        }


        /// <summary>
        /// 先complete  过 10s finish
        /// </summary>
        /// <exception cref="Exception"></exception>
        public bool Complete()
        {
            if (Team0.MCTeam == null || Team1?.MCTeam == null)
            {
                // 数据不正常
                Dispose();
                return false;
            }
            if (Team0.MCTeam.TotalCP != Team1.MCTeam.TotalCP)
            {
                EventMap.killAllMonsters();

                bool redWin = Team0.MCTeam.TotalCP > Team1.MCTeam.TotalCP;

                this.Team0.MCTeam.Complete(redWin);
                this.Team1.MCTeam.Complete(!redWin);
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

            Team0.MCTeam = new MonsterCarnivalTeam(this, Team0, 0);
            Team1.MCTeam = new MonsterCarnivalTeam(this, Team1, 1);

            Team0.MCTeam.SetEnemy(Team1.MCTeam);
            Team1.MCTeam.SetEnemy(Team0.MCTeam);

            foreach (var mc in Team0.MCTeam.EligibleMembers)
            {
                if (mc != null)
                {
                    mc.setTeam(0);
                    mc.setFestivalPoints(0);
                    mc.changeMap(EventMap, EventMap.GetInitPortal(0));
                    mc.sendPacket(PacketCreator.startMonsterCarnival(mc));
                    mc.LightBlue(nameof(ClientMessage.CPQ_Entry));
                }
            }

            foreach (var mc in Team1.MCTeam.EligibleMembers)
            {
                if (mc != null)
                {
                    mc.setTeam(1);
                    mc.setFestivalPoints(0);
                    mc.changeMap(EventMap, EventMap.GetInitPortal(1));
                    mc.sendPacket(PacketCreator.startMonsterCarnival(mc));
                    mc.LightBlue(nameof(ClientMessage.CPQ_Entry));
                }
            }


        }

        public override void playerKilled(IPlayer chr)
        {
            base.playerKilled(chr);

            int losing = EventMap.DeathCP;
            if (chr.AvailableCP < losing)
            {
                losing = chr.AvailableCP;
            }
            EventMap.broadcastMessage(PacketCreator.CPQ_PlayerDied(getName(), losing, chr.MCTeam!.TeamFlag));
            chr.gainCP(-losing);
        }

        public override void setEventCleared()
        {
            base.setEventCleared();

            Team0.MCTeam?.MoveToReward();
            Team1?.MCTeam?.MoveToReward();
        }

        public int GetAveLevel()
        {
            return Team0.getEligibleMembers().Sum(x => x.Level) / Team0.getEligibleMembers().Count;
        }

        public int GetRoomSize()
        {
            return Team0.getEligibleMembers().Count;
        }

        public bool IsWinner(IPlayer chr)
        {
            return chr.TeamModel?.MCTeam?.IsWinner ?? false;
        }

        public void EndInstance(IPlayer chr)
        {
            chr.setEventInstance(null);
        }
    }
}
