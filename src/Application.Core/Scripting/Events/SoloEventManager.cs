using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public class SoloEventManager : AbstractEventManager
    {
        public override AbstractSoloEventTemplate GetTemplate { get; }
        public SoloEventManager(WorldChannel cserv, AbstractSoloEventTemplate template) : base(cserv, template)
        {
            GetTemplate = template;
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





    }
}
