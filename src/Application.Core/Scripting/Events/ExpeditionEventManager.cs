using Application.Core.Channel;
using scripting.Event;
using server.expeditions;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public class ExpeditionEventManager : AbstractInstancedEventManager
    {
        public ExpeditionEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new ExpeditionEventInstanceManager(this, instanceName);
        }

        #region Expedition
        public bool StartInstance(Player leader)
        {
            return StartInstance(-1, leader);
        }

        public bool StartInstance(int lobbyId, Player leader, int difficult = 1)
        {
            return StartInstance(-1, leader);
        }

        public bool StartExpeditionInstance(Expedition exped)
        {
            return StartExpeditionInstance(-1, exped);
        }

        public bool StartExpeditionInstance(int lobbyId, Expedition exped)
        {
            return StartExpeditionInstance(lobbyId, exped, exped.getLeader());
        }

        //Expedition method: starts an expedition
        bool StartExpeditionInstance(int lobbyId, Expedition exped, Player leader)
        {
            if (this.isDisposed())
            {
                return false;
            }

            try
            {
                if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
                {
                    playerPermit.Add(leader.getId());

                    startLock.Enter();
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

                            ExpeditionEventInstanceManager eim;
                            try
                            {
                                eim = createInstance<ExpeditionEventInstanceManager>("setup", leader.getClient().getChannel());
                                registerEventInstance(eim, lobbyId);
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }

                            eim.setLeader(leader);

                            exped.start();
                            eim.registerExpedition(exped);

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
                        startLock.Exit();
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
