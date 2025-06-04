using Application.Core.Game.Relation;
using Application.Shared.Team;
using AutoMapper;
using System.Collections.Concurrent;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class TeamManager
    {
        readonly ConcurrentDictionary<int, Team> TeamChannelStorage = new();
        readonly WorldChannel _server;
        readonly IMapper _mapper;

        public TeamManager(WorldChannel server, IMapper mapper)
        {
            _server = server;
            _mapper = mapper;
        }

        public bool CreateParty(IPlayer player, bool silentCheck)
        {
            var party = player.getParty();
            if (party == null)
            {
                if (player.Level < 10 && !YamlConfig.config.server.USE_PARTY_FOR_STARTERS)
                {
                    player.sendPacket(PacketCreator.partyStatusMessage(10));
                    return false;
                }
                else if (player.getAriantColiseum() != null)
                {
                    player.dropMessage(5, "You cannot request a party creation while participating the Ariant Battle Arena.");
                    return false;
                }

                party = _server.Service.CreateParty(player.Id);
                player.setParty(party);
                // player.setMPC(partyplayer);
                player.silentPartyUpdate();

                player.partyOperationUpdate(party, null);

                player.sendPacket(PacketCreator.partyCreated(party, player.Id));

                return true;
            }
            else
            {
                if (!silentCheck)
                {
                    player.sendPacket(PacketCreator.partyStatusMessage(16));
                }

                return false;
            }
        }

        public bool LeaveParty(IPlayer player)
        {
            return UpdateTeam(player.Party, PartyOperation.LEAVE, player, player.Id);
            //MatchCheckerCoordinator mmce = world.getMatchCheckerCoordinator();
            //if (mmce.getMatchConfirmationLeaderid(player.getId()) == player.getId() && mmce.getMatchConfirmationType(player.getId()) == MatchCheckerType.GUILD_CREATION)
            //{
            //    mmce.dismissMatchConfirmation(player.getId());
            //}
        }

        public bool JoinParty(IPlayer player, int partyid, bool silentCheck)
        {
            return UpdateTeam(partyid, PartyOperation.JOIN, player, player.Id);
        }

        public bool ExpelFromParty(Team? party, IChannelClient c, int expelCid)
        {
            if (party != null)
            {
                return UpdateTeam(party.getId(), PartyOperation.EXPEL, c.OnlinedCharacter, expelCid);
            }
            return false;
        }

        internal bool ChangeLeader(IPlayer player, int newLeader)
        {
            return UpdateTeam(player.getPartyId(), PartyOperation.CHANGE_LEADER, player, newLeader);
        }

        public IPlayer? GetTeamLeader(Team team)
        {
            return _server.Players.getCharacterById(team.getLeaderId());
        }

        private bool ProcessUpdateResponse(int partyId, PartyOperation operation, TeamMember targetMember)
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
                var partyMembers = party.GetChannelMembers(_server);

                foreach (var partychar in partyMembers)
                {
                    partychar.setParty(operation == PartyOperation.DISBAND ? null : party);
                    if (partychar.IsOnlined)
                    {
                        partychar.sendPacket(PacketCreator.updateParty(_server, party, operation, targetMember.Id, targetMember.Name));
                    }
                }

                var targetPlayer = _server.Players.getCharacterById(targetMember.Id);
                if (targetPlayer != null)
                {
                    if (operation == PartyOperation.JOIN)
                    {
                        targetPlayer.receivePartyMemberHP();
                        targetPlayer.updatePartyMemberHP();

                        targetPlayer.partyOperationUpdate(party, null);
                    }
                    else if (operation == PartyOperation.LEAVE)
                    {
                        var mcpq = targetPlayer.getMonsterCarnival();
                        if (mcpq != null)
                        {
                            mcpq.leftParty(targetPlayer);
                        }

                        var eim = targetPlayer.getEventInstance();
                        if (eim != null)
                        {
                            eim.leftParty(targetPlayer);
                        }

                        targetPlayer.setParty(null);
                        if (targetPlayer.IsOnlined)
                        {
                            targetPlayer.sendPacket(PacketCreator.updateParty(_server, party, operation, targetMember.Id, targetMember.Name));
                        }
                    }
                    else if (operation == PartyOperation.DISBAND)
                    {
                        var mcpq = targetPlayer.getMonsterCarnival();
                        if (mcpq != null)
                        {
                            mcpq.leftParty(targetPlayer);
                        }

                        var eim = targetPlayer.getEventInstance();
                        if (eim != null)
                        {
                            eim.disbandParty();
                        }
                    }
                    else if (operation == PartyOperation.EXPEL)
                    {
                        var preData = targetPlayer.getPartyMembersOnline();

                        var mcpq = targetPlayer.getMonsterCarnival();
                        if (mcpq != null)
                        {
                            mcpq.leftParty(targetPlayer);
                        }

                        var eim = targetPlayer.getEventInstance();
                        if (eim != null)
                        {
                            eim.leftParty(targetPlayer);
                        }

                        targetPlayer.setParty(null);
                        targetPlayer.partyOperationUpdate(party, preData);
                    }
                    else if (operation == PartyOperation.CHANGE_LEADER)
                    {
                        var mc = GetTeamLeader(party);
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

                return true;

            }
            return false;
        }


        public bool UpdateTeam(int teamId, PartyOperation operation, IPlayer? player, int target)
        {
            var result = _server.Transport.SendUpdateTeam(teamId, operation, player?.Id ?? -1, target);
            if (result.ErrorCode == 0)
                return ProcessUpdateResponse(result.TeamId, (PartyOperation)result.Operation, _mapper.Map<TeamMember>(result.UpdatedMember));

            if (player != null && !result.SilentCheck)
            {
                var errorCode = (UpdateTeamCheckResult)result.ErrorCode;
                // 人数已满
                if (errorCode == UpdateTeamCheckResult.Join_TeamMemberFull)
                    player.sendPacket(PacketCreator.partyStatusMessage(17));
                // 队伍已解散
                if (errorCode == UpdateTeamCheckResult.TeamNotExsited)
                    player.sendPacket(PacketCreator.serverNotice(5, "You couldn't join the party since it had already been disbanded."));
                // 已有队伍
                if (errorCode == UpdateTeamCheckResult.Join_HasTeam)
                    player.sendPacket(PacketCreator.serverNotice(5, "You can't join the party as you are already in one."));
            }
            return false;
        }

        internal Team? GetParty(int party)
        {
            if (TeamChannelStorage.TryGetValue(party, out var d))
                return d;

            var dataRemote = _mapper.Map<Team>(_server.Transport.GetTeam(party));
            TeamChannelStorage[party] = dataRemote;
            return dataRemote;
        }
    }
}
