using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using scripting.Event;
using Serilog;
using server;
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
        //PQ method: starts a PQ
        public bool StartPQInstance(Team party, IMap map)
        {
            return StartPQInstance(party, map, 0);
        }

        public bool StartPQInstance(int lobbyId, Team party, IMap map)
        {
            return StartPQInstance(lobbyId, party, map, 0, party.GetChannelLeader(cserv));
        }

        //PQ method: starts a PQ with a difficulty level, requires function setup(difficulty, leaderid) instead of setup()
        public bool StartPQInstance(Team party, IMap map, int difficulty)
        {
            return StartPQInstance(-1, party, map, difficulty, party.GetChannelLeader(cserv));
        }

        bool StartPQInstance(int lobbyId, Team party, IMap map, int difficulty, IPlayer? leader)
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

            try
            {
                if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
                {
                    playerPermit.Add(leader.getId());

                    Monitor.Enter(startLock);
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
                                eim = createInstance<EventInstanceManager>("setup", difficulty, (lobbyId > -1) ? lobbyId : party.getLeaderId());
                                registerEventInstance(eim, lobbyId);
                                eim.Type = Application.Shared.Events.EventInstanceType.PartyQuest;
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }

                            eim.setLeader(leader);

                            eim.registerParty(party, map);
                            party.setEligibleMembers([]);

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
                        Monitor.Exit(startLock);
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
