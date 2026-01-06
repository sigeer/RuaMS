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
using System.Threading.Tasks;
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

            if (leader.Party <= 0)
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

        /// <summary>
        /// 同频道内更新队员
        /// </summary>
        /// <param name="updatePlayer"></param>
        public void ChannelNotify(Player updatePlayer)
        {
            var team = ForcedGetTeam(updatePlayer.Party, false);
            if (team != null)
            {
                var partyMembers = team.GetChannelMembers(updatePlayer.getChannelServer());

                foreach (var partychar in partyMembers)
                {
                    partychar.sendPacket(TeamPacketCreator.updateParty(updatePlayer.getChannelServer(), team, PartyOperation.SILENT_UPDATE, updatePlayer.Id, updatePlayer.Name));

                    if (partychar.Map == updatePlayer.Map)
                    {
                        partychar.sendPacket(TeamPacketCreator.updatePartyMemberHP(updatePlayer.Id, updatePlayer.HP, updatePlayer.ActualMaxHP));
                        updatePlayer.sendPacket(TeamPacketCreator.updatePartyMemberHP(partychar.Id, partychar.HP, partychar.ActualMaxHP));
                    }
                }
            }
        }
        public async Task ProcessUpdateResponse(TeamProto.UpdateTeamResponse res)
        {
            var operation = (PartyOperation)res.Request.Operation;
            var errorCode = (UpdateTeamCheckResult)res.Code;
            if (errorCode != UpdateTeamCheckResult.Success)
            {
                var operatorPlayer = _server.FindPlayerById(res.Request.FromId);
                if (operatorPlayer != null && operation != PartyOperation.SILENT_UPDATE && operation != PartyOperation.LOG_ONOFF)
                {
                    // 人数已满
                    if (errorCode == UpdateTeamCheckResult.Join_TeamMemberFull)
                        operatorPlayer.sendPacket(TeamPacketCreator.TeamFullCapacity());
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
                    foreach (var partychar in partyMembers)
                    {
                        if (partychar.Map == targetPlayer.Map && targetPlayer.Channel == partychar.Channel)
                        {
                            partychar.sendPacket(TeamPacketCreator.updatePartyMemberHP(targetPlayer.Id, targetPlayer.HP, targetPlayer.ActualMaxHP));
                            targetPlayer.sendPacket(TeamPacketCreator.updatePartyMemberHP(partychar.Id, partychar.HP, partychar.ActualMaxHP));
                        }
                    }

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
                await _cache.RemoveAsync(GetTeamCacheKey(party.getId()));
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
            if (dto != null)
            {
                _cache.Set(GetTeamCacheKey(dto.Id), dto.ToByteArray());
            }
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
