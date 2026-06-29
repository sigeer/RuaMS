using AllianceProto;
using Application.Core.ServerTransports;
using Application.Shared.Invitations;
using AutoMapper;
using GuildProto;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using net.server.guild;

namespace Application.Core.Channel.ServerData
{
    public class GuildManager
    {
        readonly ILogger<GuildManager> _logger;
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _serverContainer;
        readonly IMemoryCache _cache;
        public GuildManager(ILogger<GuildManager> logger, IMapper mapper, IChannelServerTransport transport, WorldChannelServer serverContainer,
            IMemoryCache cache)
        {
            _logger = logger;
            _mapper = mapper;
            _transport = transport;
            _serverContainer = serverContainer;
            _cache = cache;
        }

        static string GetGuildCacheKey(int guildId) => $"Guild:{guildId}";
        static string GetAllianceCacheKey(int allianceId) => $"Alliance:{allianceId}";

        public void StoreGuild(GuildDto? guild)
        {
            if (guild == null)
                return;

            _cache.Set(GetGuildCacheKey(guild.GuildId), guild);
        }
        public GuildDto? GetGuild(int guildId)
        {
            var cacheKey = GetGuildCacheKey(guildId);
            return _cache.GetOrCreate<GuildDto>(cacheKey, e =>
            {
                return _transport.GetGuild(guildId).Model;
            });
        }

        public bool CheckGuildName(string name)
        {
            if (name.Length < 3 || name.Length > 12)
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (!char.IsLower(name.ElementAt(i)) && !char.IsUpper(name.ElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckAllianceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(" ") || name.Length > 12)
            {
                return false;
            }

            return _transport.CreateAllianceCheck(new AllianceProto.CreateAllianceCheckRequest { Name = name }).IsValid;
        }

        public Task SendInvitation(IChannelClient c, string targetName)
        {
            return _transport.SendInvitation(new InvitationProto.CreateInviteRequest
            {
                Type = InviteTypes.Guild,
                FromId = c.OnlinedCharacter.Id,
                ToName = targetName,
            });
        }

        public Task AnswerInvitation(Player answer, int guildId, bool operation)
        {
            return  _transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { Type = InviteTypes.Guild, MasterId = answer.Id, CheckKey = guildId, Ok = operation });
        }

        public async Task CreateGuild(Player leader, string name)
        {
            if (!CheckGuildName(name))
            {
                await leader.dropMessage(1, "The Guild name you have chosen is not accepted.");
                return;
            }

            if (leader.getGuildId() > 0)
            {
                await leader.Popup("You cannot create a new Guild while in one.");
                return;
            }

            var party = leader.getParty();
            if (party == null || !leader.isPartyLeader())
            {
                await leader.Popup("You cannot establish the creation of a new Guild without leading a party.");
                return;
            }
            if (leader.getMapId() != MapId.GUILD_HQ)
            {
                await leader.Popup("You cannot establish the creation of a new Guild outside of the Guild Headquarters.");
                return;
            }

            var members = party.GetTeamMembers();
            foreach (var member in members)
            {
                var mapChr = leader.getMap().getCharacterById(member.Id);
                if (mapChr == null)
                {
                    await leader.dropMessage(1, "You cannot establish the creation of a new Guild if one of the members is not present here.");
                    return;
                }

                if (mapChr.GuildId > 0)
                {
                    await leader.dropMessage(1, "Please make sure everyone you are trying to invite is neither on a guild.");
                    return;
                }
            }

            if (members.Count < YamlConfig.config.server.CREATE_GUILD_MIN_PARTNERS)
            {
                await leader.dropMessage(1, "Your Guild doesn't have enough cofounders present here and therefore cannot be created at this time.");
                return;
            }

            if (leader.getMeso() < YamlConfig.config.server.CREATE_GUILD_COST)
            {
                await leader.Pink("You do not have " + leader.Client.CurrentCulture.Number(YamlConfig.config.server.CREATE_GUILD_COST) + " mesos to create a Guild.");
                return;
            }

            await leader.GainMeso(-YamlConfig.config.server.CREATE_GUILD_COST, GainItemShow.ShowInChat);

            var req = new CreateGuildRequest { LeaderId = leader.Id, Name = name };
            req.Members.AddRange(members.Select(x => x.Id));
            await _serverContainer.Transport.CreateGuild(req);

        }


        public Task LeaveMember(Player fromChr)
        {
            return _transport.SendPlayerLeaveGuild(new LeaveGuildRequest { PlayerId = fromChr.Id });
        }

        public Task ExpelMember(Player fromChr, int toId)
        {
            return _transport.SendGuildExpelMember(new ExpelFromGuildRequest { MasterId = fromChr.Id, TargetPlayerId = toId });
        }

        public Task ChangeRank(Player fromChr, int toId, int toRank)
        {
            return _transport.SendChangePlayerGuildRank(new UpdateGuildMemberRankRequest { MasterId = fromChr.Id, TargetPlayerId = toId, NewRank = toRank });
        }

        public Task SetGuildEmblem(Player chr, short bg, byte bgcolor, short logo, byte logocolor)
        {
            return _transport.SendUpdateGuildEmblem(new GuildProto.UpdateGuildEmblemRequest
            {
                Logo = logo,
                LogoColor = logocolor,
                LogoBg = bg,
                LogoBgColor = bgcolor
            });
        }

        public Task SetGuildRankTitle(Player chr, string[] titles)
        {
            var request = new GuildProto.UpdateGuildRankTitleRequest { MasterId = chr.Id };
            request.RankTitles.AddRange(titles);
            return _transport.SendUpdateGuildRankTitle(request);
        }

        public async Task IncreaseGuildCapacity(Player chr, int cost)
        {
            await chr.GainMeso(-cost, GainItemShow.ShowInChat);
            await _transport.SendUpdateGuildCapacity(new GuildProto.UpdateGuildCapacityRequest { MasterId = chr.Id, Cost = cost });
        }

        public Task SetGuildNotice(Player chr, string notice)
        {
            return _transport.SendUpdateGuildNotice(new UpdateGuildNoticeRequest { MasterId = chr.Id, Notice = notice });
        }

        public Task Disband(Player chr)
        {
            return _transport.SendGuildDisband(new GuildProto.GuildDisbandRequest { MasterId = chr.Id });
        }


        internal void DropGuildMessage(int guildId, int v, string callout)
        {
            _transport.BroadcastGuildMessage(guildId, v, callout);
        }

        public Task GainGP(Player chr, int gp)
        {
            return _transport.SendUpdateGuildGP(new UpdateGuildGPRequest { MasterId = chr.Id, Gp = gp });
        }

        public void ClearGuildCache(int guildId)
        {
            _cache.Remove(GetGuildCacheKey(guildId));
        }

        #region alliance

        public async Task CreateAlliance(Player leader, string name, int cost)
        {
            await leader.GainMeso(-cost, GainItemShow.ShowInChat);
            var guilds = leader.getPartyMembersOnSameMap().Select(x => x.Id).ToArray();

            var request = new CreateAllianceRequest { Name = name, Cost = cost };
            request.Members.AddRange(guilds);
            await _serverContainer.Transport.CreateAlliance(request);
        }
        public async Task SendAllianceInvitation(IChannelClient c, string targetGuildName)
        {
            await _transport.SendInvitation(new InvitationProto.CreateInviteRequest
            {
                Type = InviteTypes.Alliance,
                FromId = c.OnlinedCharacter.Id,
                ToName = targetGuildName
            });
        }

        public async Task AnswerAllianceInvitation(Player chr, int allianceId, bool answer)
        {
            await _transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { MasterId = chr.Id, Ok = answer, CheckKey = allianceId, Type = InviteTypes.Alliance });
        }


        #endregion

        #region Alliance
        public void StoreAlliance(AllianceDto? alliance)
        {
            if (alliance == null)
                return;
            _cache.Set(GetAllianceCacheKey(alliance.AllianceId), alliance);
            foreach (var guild in alliance.Guilds)
            {
                StoreGuild(guild);
            }
        }
        public AllianceDto? GetAlliance(int allianceId)
        {
            var cacheKey = GetGuildCacheKey(allianceId);
            return _cache.GetOrCreate<AllianceDto>(cacheKey, e =>
            {
                return _transport.GetAlliance(allianceId).Model;
            });
        }
        public async Task AllianceBroadcastPlayerInfo(Player chr)
        {
            await _transport.AllianceBroadcastPlayerInfo(new AllianceBroadcastPlayerInfoRequest { MasterId = chr.Id });
        }
        public async Task GuildLeaveAlliance(Player player, int guildId)
        {
            if (player.GuildRank != 1)
            {
                return;
            }
            await _transport.SendGuildLeaveAlliance(new AllianceProto.GuildLeaveAllianceRequest { MasterId = player.Id });
        }

        public async Task AllianceExpelGuild(Player player, int allianceId, int guildId)
        {
            await _transport.SendAllianceExpelGuild(new AllianceProto.AllianceExpelGuildRequest { MasterId = player.Id, GuildId = guildId });
        }

        public async Task ChageLeaderAllianceRank(Player player, int targetPlayerId)
        {
            if (player.GuildRank != 1)
            {
                return;
            }
            await _transport.SendChangeAllianceLeader(new AllianceProto.AllianceChangeLeaderRequest { MasterId = player.Id, PlayerId = targetPlayerId });
        }
        public async Task ChangePlayerAllianceRank(Player player, int targetPlayerId, bool isIncrease)
        {
            await _transport.SendChangePlayerAllianceRank(new AllianceProto.ChangePlayerAllianceRankRequest { MasterId = player.Id, PlayerId = targetPlayerId, Delta = isIncrease ? 1 : -1 });
        }
        public async Task HandleIncreaseAllianceCapacity(Player chr)
        {
            await _transport.SendIncreaseAllianceCapacity(new AllianceProto.IncreaseAllianceCapacityRequest { MasterId = chr.Id });
        }

        internal async Task UpdateAllianceRank(Player chr, string[] ranks)
        {
            var request = new AllianceProto.UpdateAllianceRankTitleRequest() { MasterId = chr.Id };
            request.RankTitles.AddRange(ranks);
            await _transport.SendUpdateAllianceRankTitle(request);
        }
        internal async Task UpdateAllianceNotice(Player chr, string notice)
        {
            await _transport.SendUpdateAllianceNotice(new AllianceProto.UpdateAllianceNoticeRequest { MasterId = chr.Id, Notice = notice });
        }
        internal async Task DisbandAlliance(Player player, int allianceId)
        {
            await _transport.SendAllianceDisband(new AllianceProto.DisbandAllianceRequest { MasterId = player.Id });
        }

        internal async Task ShowRankedGuilds(IChannelClient c, int npc)
        {
            var data = _transport.RequestRankedGuilds();
            await c.SendPacket(GuildPackets.showGuildRanks(npc, data.Guilds.ToList()));
        }

        public void ClearAllianceCache(int allianceId, bool deep = true)
        {
            if (!deep)
            {
                _cache.Remove(GetAllianceCacheKey(allianceId));
            }
            else
            {
                var data = GetAlliance(allianceId);
                if (data != null)
                {
                    _cache.Remove(GetAllianceCacheKey(allianceId));
                    foreach (var item in data.Guilds)
                    {
                        ClearGuildCache(item.GuildId);
                    }
                }
            }
        }
        #endregion
    }
}
