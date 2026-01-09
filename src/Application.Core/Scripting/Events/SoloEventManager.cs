using Application.Core.Channel;
using scripting.Event;
using System;
using System.Collections.Generic;
using System.Text;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public class SoloEventManager : AbstractInstancedEventManager
    {
        public SoloEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
        }

        #region Start Instance
        public bool startInstance(Player chr)
        {
            return startInstance(-1, chr);
        }

        public bool startInstance(int lobbyId, Player leader, int difficult = 1)
        {
            return startInstance(lobbyId, leader, leader, difficult);
        }

        bool startInstance(int lobbyId, Player chr, Player leader, int difficulty)
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

                            EventInstanceManager eim;
                            try
                            {
                                eim = createInstance<EventInstanceManager>("setup", difficulty, (lobbyId > -1) ? lobbyId : leader.getId());
                                registerEventInstance(eim, lobbyId);
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }
                            eim.setLeader(leader);

                            if (chr != null)
                            {
                                eim.registerPlayer(chr);
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
