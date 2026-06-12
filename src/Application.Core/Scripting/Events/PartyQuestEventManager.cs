using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public class PartyQuestEventManager : AbstractEventManager
    {
        public bool PartyLeaderRequired => GetTemplate.PartyLeaderRequired;
        public int RecruitMap => GetTemplate.RecruitMap;
        public override AbstractPartyQuestEventTemplate GetTemplate => (Template as AbstractPartyQuestEventTemplate)!;
        public PartyQuestEventManager(WorldChannel cserv, AbstractPartyQuestEventTemplate template) : base(cserv, template)
        {
        }



        #region StartInstance
        public virtual void SetupInstance(AbstractEventInstanceManager eim, Player leader, List<Player> members) { }
        public override CreateInstanceResult StartInstance(Player leader, int difficulty = 1, int lobbyId = -1)
        {
            if (this.isDisposed())
            {
                return CreateInstanceResult.Disposed;
            }

            if (PartyLeaderRequired)
            {
                if (leader.getParty() == null)
                {
                    return CreateInstanceResult.RequiredParty;
                }

                if (!leader.isLeader())
                {
                    return CreateInstanceResult.RequiredLeader;
                }
            }

            var members = Template.GetEligibleParty(leader);
            if (members.Count == 0)
            {
                return CreateInstanceResult.Requirement;
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

                            AbstractEventInstanceManager? eim = null;
                            try
                            {
                                eim = CreateInstance(difficulty, (lobbyId > -1) ? lobbyId : leader.Id);
                                registerEventInstance(eim, lobbyId);
                                eim.Type = Application.Shared.Events.EventInstanceType.PartyQuest;
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }

                            eim.setLeader(leader);

                            foreach (var item in members)
                            {
                                eim.registerPlayer(item);
                            }

                            eim.startEvent();
                            SetupInstance(eim, leader, members);
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
            return CreateInstanceResult.Unknown;
        }
        #endregion

    }
}
