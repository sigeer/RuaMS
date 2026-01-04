using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using Application.Shared.Team;
using AutoMapper;
using Google.Protobuf;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TeamProto;

namespace Application.Core.Channel.ServerData
{
    public class TeamManager
    {
        readonly IMapper _mapper;
        readonly ILogger<TeamManager> _logger;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;
        readonly IDistributedCache _cache;

        public TeamManager(IMapper mapper, ILogger<TeamManager> logger, IChannelServerTransport transport, WorldChannelServer server, IDistributedCache cache)
        {
            _mapper = mapper;
            _logger = logger;
            _transport = transport;
            _server = server;
            _cache = cache;
        }

        public async Task CreateTeam(Player leader, bool silentCheck = false)
        {
            if (leader.Level < 10 && !YamlConfig.config.server.USE_PARTY_FOR_STARTERS)
            {
                leader.sendPacket(TeamPacketCreator.BeginnerCannotCreateTeam());
                return;
            }
            if (leader.getAriantColiseum() != null)
            {
                leader.dropMessage(5, "You cannot request a party creation while participating the Ariant Battle Arena.");
                return;
            }

            var party = leader.getParty();
            if (party != null)
            {
                if (!silentCheck)
                {
                    leader.sendPacket(TeamPacketCreator.AlreadInTeam());
                }
                return;
            }

            await _transport.CreateTeam(new CreateTeamRequest { LeaderId = leader.Id, Method = 0 });
        }



        public async Task LeaveParty(Player player)
        {
            await UpdateTeam(player.Party, PartyOperation.LEAVE, player, player.Id);
            //MatchCheckerCoordinator mmce = world.getMatchCheckerCoordinator();
            //if (mmce.getMatchConfirmationLeaderid(player.getId()) == player.getId() && mmce.getMatchConfirmationType(player.getId()) == MatchCheckerType.GUILD_CREATION)
            //{
            //    mmce.dismissMatchConfirmation(player.getId());
            //}
        }

        public async Task JoinParty(Player player, int partyid, bool silentCheck)
        {
            await UpdateTeam(partyid, PartyOperation.JOIN, player, player.Id);
        }

        public async Task ExpelFromParty(Player player, int expelCid)
        {
            await UpdateTeam(player.getPartyId(), PartyOperation.EXPEL, player, expelCid);
        }

        internal async Task ChangeLeader(Player player, int newLeader)
        {
            await UpdateTeam(player.getPartyId(), PartyOperation.CHANGE_LEADER, player, newLeader);
        }

        public void ProcessTeamUpdate(SyncProto.PlayerFieldChange data)
        {
            if (data.TeamId > 0)
            {
                var team = ForcedGetTeam(data.TeamId, false);
                if (team != null)
                {
                    var partyMembers = team.GetActiveMembers(_server);

                    foreach (var partychar in partyMembers)
                    {
                        partychar.Party = data.TeamId;
                        if (partychar.IsOnlined)
                        {
                            partychar.sendPacket(TeamPacketCreator.updateParty(partychar.getChannelServer(), team, PartyOperation.SILENT_UPDATE, data.Id, data.Name));
                        }
                    }
                }
            }
        }

        public void ProcessUpdateResponse(TeamProto.UpdateTeamResponse res)
        {
            if (res.Code != 0)
            {
                var operatorPlayer = _server.FindPlayerById(res.Request.FromId);
                if (operatorPlayer != null)
                {
                    var errorCode = (UpdateTeamCheckResult)res.Code;
                    // 人数已满
                    if (errorCode == UpdateTeamCheckResult.Join_TeamMemberFull)
                        operatorPlayer.sendPacket(TeamPacketCreator.partyStatusMessage(17));
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

            var party = MapTeam(res.Team);
            SetTeam(res.Team);

            var targetMember = party.GetTeamMember(res.Request.TargetId);
            var operation = (PartyOperation)res.Request.Operation;
            var partyMembers = party.GetActiveMembers(_server);

            foreach (var partychar in partyMembers)
            {
                partychar.Party = operation == PartyOperation.DISBAND ? -1 : party.getId();
                if (partychar.IsOnlined)
                {
                    partychar.sendPacket(TeamPacketCreator.updateParty(partychar.getChannelServer(), party, operation, targetMember.Id, targetMember.Name));
                }
            }

            var targetPlayer = _server.FindPlayerById(targetMember.Channel, targetMember.Id);
            if (operation == PartyOperation.JOIN)
            {
                if (targetPlayer != null)
                {
                    targetPlayer.receivePartyMemberHP();
                    targetPlayer.updatePartyMemberHP();

                    targetPlayer.HandleTeamMemberCountChanged(null);
                }
            }
            else if (operation == PartyOperation.LEAVE)
            {
                if (targetPlayer != null)
                {
                    var partymembers = targetPlayer.getPartyMembersOnline();

                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(targetPlayer);
                    }

                    targetPlayer.Party = -1;
                    targetPlayer.sendPacket(TeamPacketCreator.updateParty(targetPlayer.getChannelServer(), party, operation, targetMember.Id, targetMember.Name));
                    targetPlayer.HandleTeamMemberCountChanged(partymembers);
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

                    targetPlayer.Party = -1;
                    targetPlayer.sendPacket(TeamPacketCreator.updateParty(targetPlayer.getChannelServer(), party, operation, targetMember.Id, targetMember.Name));
                    targetPlayer.HandleTeamMemberCountChanged(preData);
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


        public async Task UpdateTeam(int teamId, PartyOperation operation, Player? player, int target)
        {
            await _transport.SendUpdateTeam(teamId, operation, player?.Id ?? -1, target);
        }

        static string GetTeamCacheKey(int teamId) => $"Team_{teamId}";
        void SetTeam(TeamDto dto)
        {
            _cache.Set(GetTeamCacheKey(dto.Id), dto.ToByteArray());
        }

        internal Team? ForcedGetTeam(int party, bool useCache = true)
        {
            var cacheKey = GetTeamCacheKey(party);
            TeamDto res;
            var data = _cache.Get(cacheKey);
            if (data == null || !useCache)
            {
                res = _transport.GetTeam(party).Model;
                SetTeam(res);
            }
            else
            {
                res = TeamDto.Parser.ParseFrom(data);
            }

            if (res == null)
                return null;

            return MapTeam(res);
        }

        Team MapTeam(TeamDto dto)
        {
            var d = new Team(dto.Id, dto.LeaderId);
            foreach (var member in dto.Members)
            {
                d.addMember(_mapper.Map<TeamMember>(member));
            }
            return d;
        }

        public async Task CreateInvite(Player fromChr, string toName)
        {
            await _transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = fromChr.Id, ToName = toName, Type = InviteTypes.Party });

        }
        public async Task AnswerInvite(Player chr, int partyId, bool answer)
        {
            await _transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = chr.Id, Ok = answer, CheckKey = partyId, Type = InviteTypes.Party });
        }
    }
}
