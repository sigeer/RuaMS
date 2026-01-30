using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using Application.Shared.Team;
using AutoMapper;
using Google.Protobuf;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
        readonly IMemoryCache _cache;

        public TeamManager(IMapper mapper, ILogger<TeamManager> logger, IChannelServerTransport transport, WorldChannelServer server, IMemoryCache cache)
        {
            _mapper = mapper;
            _logger = logger;
            _transport = transport;
            _server = server;
            _cache = cache;
        }

        public void CreateTeam(Player leader, bool silentCheck = false)
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

            if (leader.Party > 0)
            {
                if (!silentCheck)
                {
                    leader.sendPacket(TeamPacketCreator.AlreadInTeam());
                }
                return;
            }

            _ = _transport.CreateTeam(new CreateTeamRequest { LeaderId = leader.Id, Method = 0 });
        }



        public void LeaveParty(Player player)
        {
            _ = UpdateTeam(player.Party, PartyOperation.LEAVE, player, player.Id);
            //MatchCheckerCoordinator mmce = world.getMatchCheckerCoordinator();
            //if (mmce.getMatchConfirmationLeaderid(player.getId()) == player.getId() && mmce.getMatchConfirmationType(player.getId()) == MatchCheckerType.GUILD_CREATION)
            //{
            //    mmce.dismissMatchConfirmation(player.getId());
            //}
        }

        public void LeaveStarterParty(Player player)
        {
            _ = UpdateTeam(player.Party, PartyOperation.LEAVE, player, player.Id, 1);
        }

        public void JoinParty(Player player, int partyid, bool silentCheck)
        {
            _ = UpdateTeam(partyid, PartyOperation.JOIN, player, player.Id);
        }

        public void ExpelFromParty(Player player, int expelCid)
        {
            _ = UpdateTeam(player.getPartyId(), PartyOperation.EXPEL, player, expelCid);
        }

        internal void ChangeLeader(Player player, int newLeader)
        {
            _ =  UpdateTeam(player.getPartyId(), PartyOperation.CHANGE_LEADER, player, newLeader);
        }

        /// <summary>
        /// 同频道内更新队员
        /// </summary>
        /// <param name="updatePlayer"></param>
        public void ChannelNotify(Player updatePlayer)
        {
            var team = GetTeamDto(updatePlayer.Party, false);
            if (team != null)
            {
                var partyMembers = GetChannelMembers(updatePlayer.getChannelServer(), team);

                foreach (var partychar in partyMembers)
                {
                    partychar.sendPacket(TeamPacketCreator.UpdateParty(updatePlayer.getChannelServer(), team, PartyOperation.SILENT_UPDATE, updatePlayer.Id, updatePlayer.Name));

                    if (partychar.Map == updatePlayer.Map)
                    {
                        partychar.sendPacket(TeamPacketCreator.updatePartyMemberHP(updatePlayer.Id, updatePlayer.HP, updatePlayer.ActualMaxHP));
                        updatePlayer.sendPacket(TeamPacketCreator.updatePartyMemberHP(partychar.Id, partychar.HP, partychar.ActualMaxHP));
                    }
                }
            }
        }

        async Task UpdateTeam(int teamId, PartyOperation operation, Player? player, int target, int reason = 0)
        {
            await _transport.SendUpdateTeam(teamId, operation, player?.Id ?? -1, target, reason);
        }

        static string GetTeamCacheKey(int teamId) => $"Team_{teamId}";
        public void SetTeam(TeamDto dto)
        {
            if (dto != null)
            {
                _cache.Set(GetTeamCacheKey(dto.Id), dto);
            }
        }

        public void ClearTeamCache(int teamId)
        {
            _cache.Remove(GetTeamCacheKey(teamId));
        }

        internal TeamDto? GetTeamDto(int party, bool useCache = true)
        {
            var cacheKey = GetTeamCacheKey(party);
            return _cache.GetOrCreate(cacheKey, e =>
            {
                return _transport.GetTeam(party).Model;
            });
        }

        internal Team? ForcedGetTeam(int party, bool useCache = true)
        {
            var res = GetTeamDto(party, useCache);
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

        List<Player> GetChannelMembers(WorldChannel channel, TeamDto team)
        {
            return team.Members.Select(x => channel.getPlayerStorage().getCharacterById(x.Id)).Where(x => x != null && x.isLoggedinWorld()).ToList();
        }

        public void CreateInvite(Player fromChr, string toName)
        {
            _ = _transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = fromChr.Id, ToName = toName, Type = InviteTypes.Party });

        }
        public void AnswerInvite(Player chr, int partyId, bool answer)
        {
            _ = _transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = chr.Id, Ok = answer, CheckKey = partyId, Type = InviteTypes.Party });
        }
    }
}
