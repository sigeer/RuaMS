using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using scripting.Event;
using Serilog;
using server;
using System.IO;
using System.Runtime.ConstrainedExecution;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public class PartyQuestEventManager : SoloEventManager
    {
        public PartyQuestEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
        }


        #region StartInstance
        public bool StartPQInstance(Player leader, List<Player> eligibleMembers, int difficulty = 1)
            => StartPQInstance(-1, leader, eligibleMembers, difficulty);
        public bool StartPQInstance(int lobbyId, Player leader, List<Player>? eligibleMembers = null, int difficulty = 1)
        {
            if (leader == null)
            {
                log.Information("队长不在同一频道");
                return false;
            }
            if (this.isDisposed())
            {
                return false;
            }

            eligibleMembers ??= leader.getPartyMembersOnSameMap();
            if (eligibleMembers.Count == 0 || !eligibleMembers.Contains(leader))
            {
                return false;
            }

            if (eligibleMembers.Any(x => x.MapModel != leader.MapModel))
            {
                return false;
            }

            try
            {
                if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
                {
                    playerPermit.Add(leader.getId());

                    try
                    {
                        try
                        {
                            if (lobbyId == -1)
                            {
                                lobbyId = GetAvailableLobbyInstance();
                                if (lobbyId == -1)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (!TryRegisterLobby(lobbyId))
                                {
                                    return false;
                                }
                            }

                            EventInstanceManager? eim = null;
                            try
                            {
                                eim = createInstance<EventInstanceManager>("setup", difficulty, (lobbyId > -1) ? lobbyId : leader.Id);
                                registerEventInstance(eim, lobbyId);
                                eim.Type = Application.Shared.Events.EventInstanceType.PartyQuest;
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }

                            eim.setLeader(leader);

                            foreach (var item in eligibleMembers)
                            {
                                eim.registerPlayer(item);
                            }

                            eim.startEvent();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Event script startInstance");
                            return false;
                        }

                        return true;
                    }
                    finally
                    {
                        playerPermit.Remove(leader.getId());
                        startSemaphore.Release();
                    }
                }
            }
            catch (ThreadInterruptedException ie)
            {
                log.Error(ie.ToString());
                playerPermit.Remove(leader.getId());
            }

            return false;
        }
        #endregion
    }
}
