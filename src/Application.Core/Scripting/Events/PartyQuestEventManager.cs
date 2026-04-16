using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using scripting.npc;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public abstract class PartyQuestEventManager : AbstractInstancedEventManager
    {

        public int RecruitMap { get; init; }

        public PartyQuestEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {
        }

        public override List<Player> GetEligibleParty(Player leader)
        {
            var party = leader.getParty();
            if (party == null)
            {
                return [];
            }

            var members = party.GetChannelMembers(ChannelServer)
                .Where(x => x.MapModel == leader.MapModel && x.MapModel.Id == RecruitMap).ToList();

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

        public string GetRequirementDescription(IChannelClient client)
        {
            var countRange = MinCount == MaxCount ? MinCount.ToString() : MinCount + " ~ " + MaxCount;
            var levelRange = MinLevel == MaxLevel ? MinLevel.ToString() : MinLevel + " ~ " + MaxLevel;
            return client.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_Requirement),
                countRange,
                levelRange,
                (EventTime / 60).ToString());
        }


        #region StartInstance
        public override CreateInstanceResult StartInstance(Player leader, int difficulty = 1, int lobbyId = -1)
        {
            if (this.isDisposed())
            {
                return CreateInstanceResult.Disposed;
            }

            if (leader.getParty() == null)
            {
                return CreateInstanceResult.RequiredParty;
            }

            if (!leader.isLeader())
            {
                return CreateInstanceResult.RequiredLeader;
            }

            var members = GetEligibleParty(leader);
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
