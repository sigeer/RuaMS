using Acornima;
using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using Application.Shared.Team;
using AutoMapper;
using Dto;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class TeamManager
    {
        readonly ConcurrentDictionary<int, Team> TeamChannelStorage = new();
        readonly IMapper _mapper;
        readonly ILogger<TeamManager> _logger;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;

        public TeamManager(IMapper mapper, ILogger<TeamManager> logger, IChannelServerTransport transport, WorldChannelServer server)
        {
            _mapper = mapper;
            _logger = logger;
            _transport = transport;
            _server = server;
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

                var remoteData = _transport.CreateTeam(player.Id);
                if (remoteData == null)
                {
                    player.dropMessage(5, "创建队伍失败：发生了未知错误");
                    return false;
                }
                party = new Team(remoteData.Id, remoteData.LeaderId);
                foreach (var member in remoteData.Members)
                {
                    party.addMember(_mapper.Map<TeamMember>(member));
                }
                TeamChannelStorage[party.getId()] = party;
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

        public async Task LeaveParty(IPlayer player)
        {
            await UpdateTeam(player.Party, PartyOperation.LEAVE, player, player.Id);
            //MatchCheckerCoordinator mmce = world.getMatchCheckerCoordinator();
            //if (mmce.getMatchConfirmationLeaderid(player.getId()) == player.getId() && mmce.getMatchConfirmationType(player.getId()) == MatchCheckerType.GUILD_CREATION)
            //{
            //    mmce.dismissMatchConfirmation(player.getId());
            //}
        }

        public async Task JoinParty(IPlayer player, int partyid, bool silentCheck)
        {
            await UpdateTeam(partyid, PartyOperation.JOIN, player, player.Id);
        }

        public async Task ExpelFromParty(Team? party, IChannelClient c, int expelCid)
        {
            if (party != null)
            {
                await UpdateTeam(party.getId(), PartyOperation.EXPEL, c.OnlinedCharacter, expelCid);
            }
            return;
        }

        internal async Task ChangeLeader(IPlayer player, int newLeader)
        {
            await UpdateTeam(player.getPartyId(), PartyOperation.CHANGE_LEADER, player, newLeader);
        }

        public void ProcessTeamUpdate(SyncProto.PlayerFieldChange data)
        {
            if (data.TeamId > 0)
            {
                var team = ForcedGetTeam(data.TeamId);
                if (team != null)
                {
                    team.updateMember(new TeamMember
                    {
                        Id = data.Id,
                        JobId = data.JobId,
                        Level = data.Level,
                        MapId = data.MapId,
                        Name = data.Name,
                        Channel = data.Channel
                    });

                    var partyMembers = team.GetActiveMembers(_server);

                    foreach (var partychar in partyMembers)
                    {
                        partychar.setParty(team);
                        if (partychar.IsOnlined)
                        {
                            partychar.sendPacket(PacketCreator.updateParty(partychar.Channel, team, PartyOperation.SILENT_UPDATE, data.Id, data.Name));
                        }
                    }
                }
            }
        }

        public void ProcessUpdateResponse(TeamProto.UpdateTeamResponse res)
        {
            if (res.ErrorCode != 0)
            {
                var operatorPlayer = _server.FindPlayerById(res.OperatorId);
                if (operatorPlayer != null && !res.SilentCheck)
                {
                    var errorCode = (UpdateTeamCheckResult)res.ErrorCode;
                    // 人数已满
                    if (errorCode == UpdateTeamCheckResult.Join_TeamMemberFull)
                        operatorPlayer.sendPacket(PacketCreator.partyStatusMessage(17));
                    // 队伍已解散
                    if (errorCode == UpdateTeamCheckResult.TeamNotExsited)
                        operatorPlayer.Pink(nameof(ClientMessage.Team_TeamNotFound));
                    // 已有队伍
                    if (errorCode == UpdateTeamCheckResult.Join_HasTeam)
                        operatorPlayer.Pink(nameof(ClientMessage.Team_JoinFail_AlreadInTeam));

                    _logger.LogDebug("队伍操作失败 {ErrorCode}", errorCode);
                }
                return;
            }

            var partyId = res.TeamId;
            var party = ForcedGetTeam(partyId);
            if (party == null)
            {
                _logger.LogError("队伍{TeamId}不存在, Operation {Operation}, Target: {TargetId}", partyId, res.Operation, res.UpdatedMember.Id);
                return;
            }

            var targetMember = _mapper.Map<TeamMember>(res.UpdatedMember);
            var operation = (PartyOperation)res.Operation;
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
                    TeamChannelStorage.TryRemove(partyId, out _);
                    break;
                case PartyOperation.SILENT_UPDATE:
                case PartyOperation.LOG_ONOFF:
                    party.updateMember(targetMember);
                    break;
                default:
                    break;
            }
            var partyMembers = party.GetActiveMembers(_server);

            foreach (var partychar in partyMembers)
            {
                partychar.setParty(operation == PartyOperation.DISBAND ? null : party);
                if (partychar.IsOnlined)
                {
                    partychar.sendPacket(PacketCreator.updateParty(partychar.Channel, party, operation, targetMember.Id, targetMember.Name));
                }
            }

            var targetPlayer = _server.FindPlayerById(targetMember.Channel, targetMember.Id);
            if (operation == PartyOperation.JOIN)
            {
                if (targetPlayer != null)
                {
                    targetPlayer.receivePartyMemberHP();
                    targetPlayer.updatePartyMemberHP();

                    targetPlayer.partyOperationUpdate(party, null);
                }
            }
            else if (operation == PartyOperation.LEAVE)
            {
                if (targetPlayer != null)
                {
                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(targetPlayer);
                    }

                    targetPlayer.setParty(null);
                    targetPlayer.sendPacket(PacketCreator.updateParty(targetPlayer.Channel, party, operation, targetMember.Id, targetMember.Name));
                }
            }
            else if (operation == PartyOperation.DISBAND)
            {
                if (targetPlayer != null)
                {
                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.disbandParty();
                    }
                }
            }
            else if (operation == PartyOperation.EXPEL)
            {
                if (targetPlayer != null)
                {
                    var preData = targetPlayer.getPartyMembersOnline();

                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(targetPlayer);
                    }

                    targetPlayer.setParty(null);
                    targetPlayer.sendPacket(PacketCreator.updateParty(targetPlayer.Channel, party, operation, targetMember.Id, targetMember.Name));
                    targetPlayer.partyOperationUpdate(party, preData);
                }
            }
            else if (operation == PartyOperation.CHANGE_LEADER)
            {
                var oldLeader = party.GetTeamMember(party.getLeaderId());
                var mc = _server.FindPlayerById(oldLeader.Channel, oldLeader.Id);
                if (mc != null && targetPlayer != null && mc.Channel == targetPlayer.Channel)
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
                                var mmd = _server.GetChannel(mc.Channel)?.getMiniDungeon(oldLeaderMapid);
                                if (mmd != null)
                                {
                                    mmd.close();
                                }
                            }
                        }
                    }
                }
                party.SetLeaderId(targetMember.Id);
            }
        }


        public async Task UpdateTeam(int teamId, PartyOperation operation, IPlayer? player, int target)
        {
            await _transport.SendUpdateTeam(teamId, operation, player?.Id ?? -1, target);
        }

        /// <summary>
        /// 玩家数据绑定使用，意味着当前服务器需要这个队伍数据，不存在时请求master
        /// </summary>
        /// <param name="party"></param>
        /// <returns></returns>
        internal Team? ForcedGetTeam(int party)
        {
            if (TeamChannelStorage.TryGetValue(party, out var d) && d != null)
                return d;

            var dataRemote = _transport.GetTeam(party).Model;
            if (dataRemote == null)
                return null;

            d = new Team(party, dataRemote.LeaderId);
            foreach (var member in dataRemote.Members)
            {
                d.addMember(_mapper.Map<TeamMember>(member));
            }
            return TeamChannelStorage[party] = d;
        }

        public async Task CreateInvite(IPlayer fromChr, string toName)
        {
            var party = fromChr.getParty();
            if (party == null)
            {
                if (!CreateParty(fromChr, false))
                {
                    return;
                }

                party = fromChr.getParty()!;
            }
            if (party.GetMemberCount() >= 6)
            {
                fromChr.sendPacket(PacketCreator.partyStatusMessage(17));
                return;
            }
            await _transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = fromChr.Id, ToName = toName, Type = InviteTypes.Party });

        }
        public async Task AnswerInvite(IPlayer chr, int partyId, bool answer)
        {
            await _transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = chr.Id, Ok = answer, CheckKey = partyId, Type = InviteTypes.Party });
        }
    }
}
