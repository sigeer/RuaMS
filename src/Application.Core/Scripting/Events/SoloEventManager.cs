using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Shared.Events;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public abstract class SoloEventManager : AbstractInstancedEventManager
    {
        public SoloEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {
        }


        #region Start Instance
        public override CreateInstanceResult StartInstance(Player chr, int difficulty = 1, int lobbyId = -1)
        {
            if (this.isDisposed())
            {
                return CreateInstanceResult.Disposed;
            }

            try
            {
                if (!playerPermit.Contains(chr.getId()) && startSemaphore.Wait(7777))
                {
                    playerPermit.Add(chr.getId());

                    try
                    {
                        try
                        {
                            if (lobbyId == -1)
                            {
                                lobbyId = GetAvailableLobbyInstance();
                                if (lobbyId == -1)
                                {
                                    return CreateInstanceResult.LobbyLimited;
                                }
                            }
                            else
                            {
                                if (!TryRegisterLobby(lobbyId))
                                {
                                    return CreateInstanceResult.LobbyLimited;
                                }
                            }

                            AbstractEventInstanceManager eim;
                            try
                            {
                                eim = CreateInstance(difficulty, (lobbyId > -1) ? lobbyId : chr.getId());
                                registerEventInstance(eim, lobbyId);
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }
                            eim.setLeader(chr);

                            if (chr != null)
                            {
                                eim.registerPlayer(chr);
                            }

                            eim.startEvent();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Event script startInstance");
                            return CreateInstanceResult.Unknown;
                        }

                        return CreateInstanceResult.Success;
                    }
                    finally
                    {
                        playerPermit.Remove(chr.getId());
                        startSemaphore.Release();
                    }
                }
            }
            catch (ThreadInterruptedException ie)
            {
                log.Error(ie.ToString());
                playerPermit.Remove(chr.getId());
            }

            return CreateInstanceResult.Unknown;
        }
        #endregion


        public override List<Player> GetEligibleParty(Player leader)
        {
            List<Player> members = [leader];

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }
    }
}
