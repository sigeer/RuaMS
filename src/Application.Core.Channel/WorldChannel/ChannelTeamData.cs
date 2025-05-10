using Application.Core.Channel.LocalData;
using Application.Core.Game.Relation;
using Application.Shared.Relations;
using Application.Shared.Servers;
using server.maps;
using tools;

namespace Application.Core.Channel
{
    public partial class WorldChannel
    {
        Dictionary<int, Team> TeamChannelStorage { get; } = new();
        public ITeam CreateTeam(int playerId)
        {
            var teamId = Transport.CreateTeam(playerId);
            return TeamChannelStorage[teamId];
        }

        public ITeam? GetLocalTeam(int teamId)
        {
            return TeamChannelStorage.GetValueOrDefault(teamId);
        }
        public void SyncTeam(ITeamGlobal teamGlobal)
        {
            TeamChannelStorage[teamGlobal.Id] = new Team(this, teamGlobal.Id, teamGlobal.LeaderId);
        }
        public void UpdateTeamGlobalData(int partyId, PartyOperation operation, int targetId, string targetName)
        {
            Transport.SendUpdateTeamGlobalData(partyId, operation, targetId, targetName);
        }

        public void ProcessUpdateTeamChannelData(int partyId, PartyOperation operation, TeamMember targetMember)
        {
            if (TeamChannelStorage.TryGetValue(partyId, out var party))
            {
                switch (operation)
                {
                    case PartyOperation.JOIN:
                        party.addMember(targetMember);
                        break;
                    case PartyOperation.EXPEL:
                    case PartyOperation.LEAVE:
                        party.removeMember(targetMember.Id);
                        break;
                    case PartyOperation.DISBAND:
                        TeamChannelStorage.Remove(partyId);
                        break;
                    case PartyOperation.SILENT_UPDATE:
                    case PartyOperation.LOG_ONOFF:
                        party.updateMember(targetMember);
                        break;
                    default:
                        break;
                }
                var partyMembers = party.GetChannelMembers();

                foreach (var partychar in partyMembers)
                {
                    partychar.setParty(operation == PartyOperation.DISBAND ? null : party);
                    if (partychar.IsOnlined)
                    {
                        partychar.sendPacket(PacketCreator.updateParty(getId(), party, operation, targetMember.Id, targetMember.Name));
                    }
                }

                if (operation == PartyOperation.LEAVE || operation == PartyOperation.EXPEL || operation == PartyOperation.CHANGE_LEADER)
                {
                    var targetPlayer = Players.getCharacterById(targetMember.Id);
                    if (targetPlayer != null)
                    {
                        if (operation == PartyOperation.LEAVE || operation == PartyOperation.EXPEL)
                        {
                            targetPlayer.setParty(null);
                            if (targetPlayer.IsOnlined)
                            {
                                targetPlayer.sendPacket(PacketCreator.updateParty(channel, party, operation, targetMember.Id, targetMember.Name));
                            }
                        }
                        else
                        {
                            var mc = party.GetLeader();
                            if (mc != null)
                            {
                                var eim = mc.getEventInstance();

                                if (eim != null && eim.isEventLeader(mc))
                                {
                                    eim.changedLeader(targetPlayer);
                                }
                                else
                                {
                                    int oldLeaderMapid = mc.getMapId();

                                    if (MiniDungeonInfo.isDungeonMap(oldLeaderMapid))
                                    {
                                        if (oldLeaderMapid != targetPlayer.getMapId())
                                        {
                                            var mmd = mc.getClient().getChannelServer().getMiniDungeon(oldLeaderMapid);
                                            if (mmd != null)
                                            {
                                                mmd.close();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }


            }
        }
        /// <summary>
        /// 频道服务器调用，实际上是请求主服务器操作
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="expelCid"></param>
        public void ExpelFromParty(int partyId, int expelCid)
        {
            Transport.SendExpelFromParty(partyId, expelCid);
        }
        /// <summary>
        /// 被主服务器调用，实际的更新
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="expelCid"></param>
        public void ProcessExpelFromParty(int partyId, int expelCid)
        {
            var party = GetLocalTeam(partyId);
            if (party == null)
                return;

            var m = party.getMemberById(expelCid);
            if (m == null)
                return;

            var emc = Players.getCharacterById(m.Id);
            if (emc != null)
            {
                var partyMembers = party.GetChannelMembers();

                var mcpq = emc.getMonsterCarnival();
                if (mcpq != null)
                {
                    mcpq.leftParty(emc);
                }

                var eim = emc.getEventInstance();
                if (eim != null)
                {
                    eim.leftParty(emc);
                }

                emc.setParty(null);
                UpdateTeamGlobalData(party.getId(), PartyOperation.EXPEL, emc.Id, emc.Name);

                emc.updatePartySearchAvailability(true);
                emc.partyOperationUpdate(party, partyMembers);
            }
        }

        public void BroadcastTeamMessage(int teamId, string from, string message)
        {
            Transport.RequestTeamMessage(teamId, from, message);
        }
        public void ProcessBroadcastTeamMessage(int teamId, string from, string message)
        {
            if (TeamChannelStorage.TryGetValue(teamId, out var team))
            {
                team.BroadcastTeamMessage(from, message);
            }
        }

    }
}
