using Application.Core.Game.GameEvents.CPQ;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using Application.Shared.Events;
using tools;

namespace Application.Core.Scripting.Events
{
    public enum MonsterCarnivalStage
    {
        /// <summary>
        /// 房间无人
        /// </summary>
        None,
        /// <summary>
        /// 等待室匹配
        /// </summary>
        Waiting,
        /// <summary>
        /// 完成匹配，等待开始
        /// </summary>
        Matched,

        Battle,
        /// <summary>
        /// 结束
        /// </summary>
        Completed
    }
    public class MonsterCarnivalEventInstanceManager : AbstractEventInstanceManager
    {
        public int MinCount { get; }
        int _lobbyMapId;
        int _eventMapId;
        public IMap LobbyMap { get; set; } = null!;
        public ICPQMap EventMap { get; set; } = null!;

        /// <summary>
        /// 红队
        /// </summary>
        public TeamRegistry? Team0 { get; set; }
        /// <summary>
        /// 蓝队
        /// </summary>
        public TeamRegistry? Team1 { get; set; }
        public MonsterCarnivalStage CurrentStage { get; private set; }

        public override MonsterCarnivalEventManager EventManager { get; }
        public MonsterCarnivalEventInstanceManager(MonsterCarnivalEventManager em, string name, int minCount, int lobbyMapId, int eventMapId) : base(em, name)
        {
            EventManager = em;
            MinCount = minCount;
            _lobbyMapId = lobbyMapId;
            _eventMapId = eventMapId;
        }

        public void EnterLobby(TeamRegistry team)
        {
            Team0 = team;
            foreach (var pChr in team.EligibleMembers)
            {
                registerPlayer(pChr);
            }

            LobbyMap = getInstanceMap(_lobbyMapId)!;
            EventMap = (getInstanceMap(_eventMapId) as ICPQMap)!;
            CurrentStage = MonsterCarnivalStage.Waiting;

            startEventTimer(EventManager.EventTime * 1000);
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

                    restartEventTimer(10_000);
                    CurrentStage = MonsterCarnivalStage.Matched;
                }

            }
            else
            {
                if (Team1 != null)
                {
                    foreach (var item in Team1.EligibleMembers)
                    {
                        item.Pink(nameof(ClientMessage.CPQ_ChallengeRoomDenied));
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

            Team0?.MCTeam?.Dispose();
            Team0 = null;

            Team1?.MCTeam?.Dispose();
            Team1 = null;


            CurrentStage = MonsterCarnivalStage.None;
            disposed = false;
        }


        /// <summary>
        /// 先complete  过 10s finish
        /// </summary>
        /// <exception cref="Exception"></exception>
        public bool Complete()
        {
            if (Team0?.MCTeam == null || Team1?.MCTeam == null)
            {
                // 数据不正常
                Dispose();
                return false;
            }
            if (Team0.MCTeam.TotalCP != Team1.MCTeam.TotalCP)
            {
                EventMap.killAllMonsters();

                bool redWin = Team0.MCTeam.TotalCP > Team1.MCTeam.TotalCP;

                Team0.MCTeam.Complete(redWin);
                Team1.MCTeam.Complete(!redWin);

                CurrentStage = MonsterCarnivalStage.Completed;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void startEvent()
        {
            base.startEvent();

            if (Team1 == null)
                return;

            Team0.MCTeam = new MonsterCarnivalTeam(this, Team0!, 0);
            Team1.MCTeam = new MonsterCarnivalTeam(this, Team1!, 0);

            Team0.MCTeam.Initialize(Team1.MCTeam);
            Team1.MCTeam.Initialize(Team0.MCTeam);

            CurrentStage = MonsterCarnivalStage.Battle;
        }

        public override void setEventCleared()
        {
            eventCleared = true;

            foreach (Player chr in getPlayers())
            {
                chr.awardQuestPoint(YamlConfig.config.server.QUEST_POINT_PER_EVENT_CLEAR);
            }

            Team0?.MCTeam?.MoveToReward();
            Team1?.MCTeam?.MoveToReward();
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
