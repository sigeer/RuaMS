using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Guild;
using Application.Shared.Team;
using Application.Utility;
using AutoMapper;
using GuildProto;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Application.Core.Login.ServerData
{
    public class GuildManager : IStorage, IDisposable
    {
        ConcurrentDictionary<int, GuildModel> _idGuildDataSource = new();
        ConcurrentDictionary<string, GuildModel> _nameGuildDataSource = new();
        int _currentGuildId = 1;

        int _currentAllianceId = 1;
        ConcurrentDictionary<int, AllianceModel> _idAllianceDataSource = new();
        ConcurrentDictionary<string, AllianceModel> _nameAllianceDataSource = new();

        readonly MasterServer _server;
        readonly ILogger<GuildManager> _logger;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        public GuildManager(MasterServer server, ILogger<GuildManager> logger, IMapper mapper, IDbContextFactory<DBContext> dbContext)
        {
            _server = server;
            _logger = logger;
            _mapper = mapper;
            _dbContextFactory = dbContext;
        }

        public async Task InitializeAsync(DBContext dbContext)
        {
            // 家族、联盟的数据应该不会太多，全部加载省事
            var allGuilds = await dbContext.Guilds.AsNoTracking().ToListAsync();
            foreach (var item in allGuilds)
            {
                var model = _mapper.Map<GuildModel>(item);
                model.Members = dbContext.Characters.Where(x => x.GuildId == model.GuildId).Select(x => x.Id).ToList();

                _idGuildDataSource[model.GuildId] = model;
                _nameGuildDataSource[model.Name] = model;

                _currentGuildId = model.GuildId > _currentGuildId ? model.GuildId : _currentGuildId;
            }

            var allAliance = await dbContext.Alliances.AsNoTracking().ToListAsync();
            foreach (var item in allAliance)
            {
                var model = _mapper.Map<AllianceModel>(item);
                model.Guilds = allGuilds.Where(x => x.AllianceId == model.Id).Select(x => x.GuildId).ToList();

                _idAllianceDataSource[model.Id] = model;
                _nameAllianceDataSource[model.Name] = model;

                _currentAllianceId = model.Id > _currentAllianceId ? model.Id : _currentAllianceId;
            }
            _logger.LogInformation("共加载了{GuildCount}个家族，{AllianceCount}个联盟", allGuilds.Count, allAliance.Count);
        }

        public GuildProto.GuildDto? CreateGuild(string guildName, int leaderId, int[] members)
        {
            if (_nameGuildDataSource.Keys.Contains(guildName))
                return null;

            var header = _server.CharacterManager.FindPlayerById(leaderId);
            if (header == null || header.Character.GuildId > 0)
                return null;

            var guildModel = new GuildModel() { GuildId = Interlocked.Increment(ref _currentGuildId), Name = guildName, Leader = leaderId, Signature = _server.getCurrentTime() };

            _idGuildDataSource[guildModel.GuildId] = guildModel;
            _nameGuildDataSource[guildModel.Name] = guildModel;

            var memberList = members.Select(_server.CharacterManager.FindPlayerById).Where(x => x != null).ToList();
            if (memberList.Any(x => x!.Character.GuildId != 0))
                return null;

            foreach (var member in memberList)
            {
                if (member!.Character.Id == leaderId)
                {
                    header.Character.GuildId = guildModel.GuildId;
                    header.Character.GuildRank = 1;
                }
                else
                {
                    var cofounder = member!.Character.Party == header.Character.Party;
                    member.Character.GuildId = guildModel.GuildId;
                    member.Character.GuildRank = cofounder ? 2 : 5;
                    member.Character.AllianceRank = 5;
                }
            }

            SetGuildUpdate(guildModel);
            var response = new GuildProto.GuildDto();
            response.GuildId = guildModel.GuildId;
            response.Leader = guildModel.Leader;
            response.Members.AddRange(_mapper.Map<GuildProto.GuildMemberDto[]>(memberList));
            return response;
        }

        public GuildModel? GetLocalGuild(int guildId)
        {
            if (_idGuildDataSource.TryGetValue(guildId, out var d) && d != null)
                return d;

            return null;
        }
        public GuildModel? FindGuildByName(string name) => _nameGuildDataSource.GetValueOrDefault(name);
        public GuildProto.GuildDto? GetGuildFull(int guildId)
        {
            var data = GetLocalGuild(guildId);
            if (data == null)
                return null;

            var response = _mapper.Map<GuildProto.GuildDto>(data);
            response.Members.AddRange(_mapper.Map<GuildProto.GuildMemberDto[]>(GetGuildMembers(data)));
            return response;
        }

        List<CharacterLiveObject> GetGuildMembers(GuildModel guild)
        {
            return guild.Members.Select(_server.CharacterManager.FindPlayerById).Where(x => x != null).ToList()!;
        }

        public void Dispose()
        {
            _idGuildDataSource.Clear();
            _nameGuildDataSource.Clear();
        }

        List<int> GetAllianceGuilds(int allianceId)
        {
            List<int> total = [];
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbData = dbContext.Guilds.Where(x => x.AllianceId == allianceId).ToList();
            foreach (var guild in dbData)
            {
                var localData = GetLocalGuild(guild.GuildId);
                if (localData != null && localData.AllianceId == allianceId)
                {
                    total.Add(localData.GuildId);
                }
            }

            total.AddRange(_idGuildDataSource.Values.Where(x => x.AllianceId == allianceId).Select(x => x.GuildId));
            return total.ToHashSet().ToList();
        }

        public async Task SendGuildChatAsync(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_idGuildDataSource.TryGetValue(sender.Character.GuildId, out var guild))
                {
                    var onlinedGuildMembers = guild.Members.Where(x => x != sender.Character.Id).Select(_server.CharacterManager.FindPlayerById)
                        .Where(x => x != null && x.Channel > 0);
                    await _server.Transport.SendMultiChatAsync(2, nameFrom, onlinedGuildMembers, chatText);
                }

            }
        }

        #region
        public AllianceModel? GetLocalAlliance(int allianceId)
        {
            if (_idAllianceDataSource.TryGetValue(allianceId, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Alliances.AsNoTracking().FirstOrDefault(x => x.Id == allianceId);
            d = _mapper.Map<AllianceModel>(dbModel);
            d.Guilds = GetAllianceGuilds(allianceId);
            return d;
        }
        public AllianceProto.AllianceDto? GetAllianceFull(int guildId)
        {
            var data = GetLocalAlliance(guildId);
            if (data == null)
                return null;

            var response = _mapper.Map<AllianceProto.AllianceDto>(data);
            response.Guilds.AddRange(data.Guilds);
            return response;
        }

        public AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request)
        {
            return new AllianceProto.CreateAllianceCheckResponse() { IsValid = !_nameAllianceDataSource.Keys.Contains(request.Name) };
        }
        public AllianceProto.AllianceDto? CreateAlliance(int[] memberPlayers, string allianceName)
        {
            if (_nameAllianceDataSource.Keys.Contains(allianceName))
                return null;

            if (memberPlayers.Length != 2)
                return null;

            var first = _server.CharacterManager.FindPlayerById(memberPlayers[0]);
            if (first == null)
                return null;

            var second = _server.CharacterManager.FindPlayerById(memberPlayers[1]);
            if (second == null)
                return null;

            if (first.Character.GuildRank != 1 || second.Character.GuildRank != 1)
                return null;

            var guild1 = GetLocalGuild(first.Character.GuildId);
            var guild2 = GetLocalGuild(second.Character.GuildId);
            if (guild1 == null || guild2 == null || guild1.AllianceId != 0 || guild2.AllianceId != 0)
                return null;

            var allianceModel = new AllianceModel()
            {
                Id = Interlocked.Increment(ref _currentAllianceId),
                Name = allianceName,
                Guilds = [guild1.GuildId, guild2.GuildId],
                Capacity = 2,
            };

            _idAllianceDataSource[allianceModel.Id] = allianceModel;
            _nameAllianceDataSource[allianceName] = allianceModel;
            SetAlliance(allianceModel);

            guild1.AllianceId = allianceModel.Id;
            guild2.AllianceId = allianceModel.Id;
            foreach (var guildMember in GetGuildMembers(guild1))
            {
                guildMember.Character.AllianceRank = 5;
            }
            foreach (var guildMember in GetGuildMembers(guild2))
            {
                guildMember.Character.AllianceRank = 5;
            }
            first.Character.AllianceRank = 1;
            second.Character.AllianceRank = 2;

            var response = _mapper.Map<AllianceProto.AllianceDto>(allianceModel);
            return response;
        }


        public async Task SendAllianceChatAsync(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_idGuildDataSource.TryGetValue(sender.Character.GuildId, out var guild))
                {
                    if (_idAllianceDataSource.TryGetValue(guild.AllianceId, out var alliance))
                    {
                        var allianceMembers = alliance.Guilds.Select(GetLocalGuild)
                            .SelectMany(x => GetGuildMembers(x).Where(y => y.Character.Id != sender.Character.Id && y.Channel > 0));
                        await _server.Transport.SendMultiChatAsync(3, nameFrom, allianceMembers, chatText);
                    }
                }

            }
        }

        public void SendGuildMessage(int guildId, int v, string callout)
        {
            if (_idGuildDataSource.TryGetValue(guildId, out var guild))
            {
                _server.DropWorldMessage(v, callout, guild.Members.ToArray());
            }
        }

        public void SendGuildPacket(GuildProto.GuildPacketRequest data)
        {
            if (_idGuildDataSource.TryGetValue(data.GuildId, out var guild))
            {
                _server.BroadcastPacket(new MessageProto.PacketRequest { Data = data.Data }, guild.Members.Except([data.ExceptChrId]));
            }
        }

        private int HandleGuildRequest(int masterId, Func<GuildModel, GuildUpdateResult> guildAction, out GuildUpdateResult code)
        {
            var master = _server.CharacterManager.FindPlayerById(masterId);
            if (master == null)
            {
                code = GuildUpdateResult.PlayerNotExisted;
            }
            else
            {
                var guild = GetLocalGuild(master.Character.GuildId);
                if (guild == null)
                {
                    code = GuildUpdateResult.GuildNotExisted;
                }
                else
                {
                    SetGuildUpdate(guild!);
                    code = guildAction(guild);
                    return guild.GuildId;
                }
            }
            return 0;
        }

        private int HandleGuildRequest(int masterId, Func<GuildModel, CharacterLiveObject, GuildUpdateResult> guildAction, out GuildUpdateResult code)
        {
            var master = _server.CharacterManager.FindPlayerById(masterId);
            if (master == null)
            {
                code = GuildUpdateResult.PlayerNotExisted;
            }
            else
            {
                var guild = GetLocalGuild(master.Character.GuildId);
                if (guild == null)
                {
                    code = GuildUpdateResult.GuildNotExisted;
                }
                else
                {
                    code = guildAction(guild, master);
                    return guild.GuildId;
                }
            }
            return 0;
        }

        public void UpdateGuildGP(UpdateGuildGPRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                guild.GP += request.Gp;
                return GuildUpdateResult.Success;
            }, out var code);
            var response = new UpdateGuildGPResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildGPUpdate(response);
        }

        public void UpdateGuildRankTitle(UpdateGuildRankTitleRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                guild.Rank1Title = request.RankTitles[0];
                guild.Rank2Title = request.RankTitles[1];
                guild.Rank3Title = request.RankTitles[2];
                guild.Rank4Title = request.RankTitles[3];
                guild.Rank5Title = request.RankTitles[4];

                return GuildUpdateResult.Success;
            }, out var code);
            var response = new UpdateGuildRankTitleResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildRankTitleUpdate(response);
        }

        public void UpdateGuildNotice(UpdateGuildNoticeRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                guild.Notice = request.Notice;
                return GuildUpdateResult.Success;
            }, out var code);
            var response = new UpdateGuildNoticeResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildNoticeUpdate(response);
        }

        public void IncreseGuildCapacity(UpdateGuildCapacityRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                if (guild.Capacity > 99)
                    return GuildUpdateResult.GuildFull;

                guild.Capacity += 5;
                return GuildUpdateResult.Success;
            }, out var code);

            var response = new UpdateGuildCapacityResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildCapacityUpdate(response);
        }

        public void UpdateGuildEmblem(UpdateGuildEmblemRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                guild.Logo = request.Logo;
                guild.LogoBg = request.LogoBg;
                guild.LogoColor = (short)request.LogoColor;
                guild.LogoBgColor = (short)request.LogoBgColor;

                return GuildUpdateResult.Success;
            }, out var code);

            var response = new UpdateGuildEmblemResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildEmblemUpdate(response);
        }

        public void DisbandGuild(GuildDisbandRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, guild =>
            {
                foreach (var member in GetGuildMembers(guild))
                {
                    if (member != null)
                    {
                        member.Character.GuildId = 0;
                        member.Character.GuildRank = 5;
                    }
                }
                if (guild.AllianceId > 0)
                {
                    var alliance = GetLocalAlliance(guild.AllianceId);
                    if (alliance != null)
                        alliance.TryRemoveGuild(guild.GuildId, out _);
                    guild.AllianceId = 0;
                }
                _idGuildDataSource.Remove(guild.GuildId, out _);
                _nameGuildDataSource.Remove(guild.Name, out _);
                SetGuildRemoved(guild!);

                return GuildUpdateResult.Success;
            }, out var code);

            var response = new GuildDisbandResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildDisband(response);
        }

        public void ChangePlayerGuildRank(UpdateGuildMemberRankRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, (guild, master) =>
            {
                var chrTo = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
                if (chrTo == null)
                    return GuildUpdateResult.PlayerNotExisted;

                if (master.Character.GuildId == chrTo.Character.GuildId && master.Character.GuildRank < chrTo.Character.GuildRank)
                {
                    chrTo.Character.GuildRank = request.NewRank;
                    return GuildUpdateResult.Success;
                }
                return GuildUpdateResult.MasterRankFail;
            }, out var code);

            var response = new UpdateGuildMemberRankResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildRankChanged(response);
        }

        public void GuildExpelMember(ExpelFromGuildRequest request)
        {
            var guildId = HandleGuildRequest(request.MasterId, (guild, master) =>
            {
                var chrTo = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
                if (chrTo == null)
                    return GuildUpdateResult.PlayerNotExisted;

                if (master.Character.GuildRank > 2)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} is trying to expel without rank 1 or 2", master.Character.Name);
                    return GuildUpdateResult.MasterRankFail;
                }

                if (master.Character.GuildId == chrTo.Character.GuildId && master.Character.GuildRank < chrTo.Character.GuildRank)
                {
                    if (chrTo.Channel == 0)
                    {
                        _server.NoteManager
                            .SendNormal("You have been expelled from the guild.", master.Character.Id, chrTo.Character.Name);
                    }

                    chrTo.Character.GuildId = 0;
                    chrTo.Character.GuildRank = 5;
                    guild.Members.Remove(chrTo.Character.Id);
                    return GuildUpdateResult.Success;
                }
                return GuildUpdateResult.MasterRankFail;
            }, out var code);

            var response = new ExpelFromGuildResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastGuildExpelMember(response);
        }

        public void PlayerLeaveGuild(LeaveGuildRequest request)
        {
            var guildId = HandleGuildRequest(request.PlayerId, (guild, master) =>
            {
                if (master.Character.Id == guild.Leader)
                {
                    // disband
                    DisbandGuild(new GuildDisbandRequest { MasterId = request.PlayerId });
                }
                else
                {
                    master.Character.GuildId = 0;
                    master.Character.GuildRank = 5;
                }
                return 0;
            }, out var code);

            var response = new LeaveGuildResponse { Code = (int)code, Request = request, GuildId = guildId };
            _server.Transport.BroadcastPlayerLeaveGuild(response);
        }

        public void PlayerJoinGuild(JoinGuildRequest request)
        {
            GuildUpdateResult code = GuildUpdateResult.Success;
            var master = _server.CharacterManager.FindPlayerById(request.PlayerId);
            if (master != null)
            {
                if (master.Character.GuildId > 0)
                    code = GuildUpdateResult.Join_AlreadyInGuild;
                else
                {
                    var guild = GetLocalGuild(request.GuildId);
                    if (guild == null)
                    {
                        code = GuildUpdateResult.GuildNotExisted;
                    }
                    else if (guild.Members.Count >= guild.Members.Capacity)
                    {
                        code = GuildUpdateResult.GuildFull;
                    }
                    else
                    {
                        guild.Members.Add(master.Character.Id);
                        master.Character.GuildId = guild.GuildId;
                        master.Character.GuildRank = 5;
                    }
                }
            }

            var response = new JoinGuildResponse { Code = (int)code, Request = request };
            _server.Transport.BroadcastPlayerJoinGuild(response);
        }

        #endregion

        #region Alliance
        private (int, int) HandleAllianceRequest(int masterId, Func<CharacterLiveObject, AllianceModel, GuildModel, AllianceUpdateResult> guildAction, out AllianceUpdateResult code)
        {
            var master = _server.CharacterManager.FindPlayerById(masterId);
            if (master == null)
            {
                code = AllianceUpdateResult.PlayerNotExisted;
            }
            else
            {
                var guild = GetLocalGuild(master.Character.GuildId);
                if (guild == null)
                {
                    code = AllianceUpdateResult.GuildNotExisted;
                }
                else
                {
                    var alliance = GetLocalAlliance(guild.AllianceId);
                    if (alliance == null)
                    {
                        code = AllianceUpdateResult.AllianceNotFound;
                    }
                    else
                    {
                        code = guildAction(master, alliance, guild);
                        SetGuildUpdate(guild!);
                        return (alliance.Id, guild.GuildId);
                    }
                }
            }
            return (0, 0);
        }
        public void GuildJoinAlliance(AllianceProto.GuildJoinAllianceRequest request)
        {
            var response = new AllianceProto.GuildJoinAllianceResponse { Request = request };
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)AllianceUpdateResult.PlayerNotExisted;
            }
            else
            {
                var guild = GetLocalGuild(master.Character.GuildId);
                if (guild == null)
                {
                    response.Code = (int)AllianceUpdateResult.GuildNotExisted;
                }
                else
                {
                    if (guild.AllianceId > 0)
                    {
                        response.Code = (int)AllianceUpdateResult.GuildAlreadyInAlliance;
                    }
                    else
                    {
                        var alliance = GetLocalAlliance(request.AllianceId);
                        if (alliance == null)
                        {
                            response.Code = (int)AllianceUpdateResult.AllianceNotFound;
                        }
                        else
                        {
                            if (alliance.TryAddGuild(guild.GuildId, out var code))
                            {
                                var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
                                foreach (var member in guildMembers)
                                {
                                    if (member != null)
                                        member.Character.AllianceRank = 5;
                                }
                                guild.AllianceId = alliance.Id;
                                master.Character.AllianceRank = 2;

                                response.GuildId = guild.GuildId;
                                response.AllianceId = alliance.Id;
                            }
                            else
                            {
                                response.Code = (int)code;
                            }
                        }
                    }
                }
            }

            _server.Transport.BroadcastGuildJoinAlliance(response);
        }

        public void GuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest request)
        {
            var (allianceId, guildId) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                if (master.Character.GuildRank != 1)
                {
                    return AllianceUpdateResult.NotGuildLeader;
                }
                if (alliance.TryRemoveGuild(master.Character.GuildId, out var code))
                {
                    guild.AllianceId = 0;

                    var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
                    foreach (var member in guildMembers)
                    {
                        if (member != null)
                            member.Character.AllianceRank = 5;
                    }
                    return AllianceUpdateResult.Success;
                }
                return code;
            }, out var code);

            _server.Transport.BroadcastGuildLeaveAlliance(new AllianceProto.GuildLeaveAllianceResponse { Code = (int)code, Request = request, AllianceId = allianceId, GuildId = guildId });
        }

        public void AllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest request)
        {
            var (allianceId, guildId) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                if (master.Character.AllianceRank != 1)
                {
                    return AllianceUpdateResult.NotAllianceLeader;
                }
                if (alliance.TryRemoveGuild(guild.GuildId, out var code))
                {
                    guild.AllianceId = 0;
                    var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
                    foreach (var member in guildMembers)
                    {
                        if (member != null)
                            member.Character.AllianceRank = 5;
                    }
                }
                return code;
            }, out var code);

            _server.Transport.BroadcastAllianceExpelGuild(new AllianceProto.AllianceExpelGuildResponse { Code = (int)code, Request = request, AllianceId = allianceId, GuildId = guildId });
        }

        public void IncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest request)
        {
            var (allianceId, _) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                alliance.Capacity += 1;
                SetAlliance(alliance);
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceCapacityIncreased(new AllianceProto.IncreaseAllianceCapacityResponse { Code = (int)code, Request = request, AllianceId = allianceId });
        }

        public void UpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request)
        {
            var (allianceId, _) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                alliance.Rank1 = request.RankTitles[0];
                alliance.Rank2 = request.RankTitles[1];
                alliance.Rank3 = request.RankTitles[2];
                alliance.Rank4 = request.RankTitles[3];
                alliance.Rank5 = request.RankTitles[4];
                SetAlliance(alliance);
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceRankTitleChanged(new AllianceProto.UpdateAllianceRankTitleResponse { Code = (int)code, Request = request, AllianceId = allianceId });
        }

        public void UpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest request)
        {
            var (allianceId, _) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                alliance.Notice = request.Notice;
                SetAlliance(alliance);
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceNoticeChanged(new AllianceProto.UpdateAllianceNoticeResponse { Code = (int)code, Request = request, AllianceId = allianceId });
        }

        public void ChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest request)
        {
            var (allianceId, guildId) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                if (master.Character.AllianceRank != 1)
                {
                    return AllianceUpdateResult.NotAllianceLeader;
                }

                var newLeader = _server.CharacterManager.FindPlayerById(request.PlayerId);
                if (newLeader == null)
                {
                    return AllianceUpdateResult.LeaderNotExisted;
                }

                if (newLeader.Character.GuildRank != 2)
                {
                    return AllianceUpdateResult.NotGuildLeader;
                }

                if (newLeader.Channel < 1)
                {
                    return AllianceUpdateResult.PlayerNotOnlined;
                }

                master.Character.AllianceRank = 2;
                newLeader.Character.AllianceRank = 1;
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceLeaderChanged(new AllianceProto.AllianceChangeLeaderResponse { Code = (int)code, Request = request, AllianceId = allianceId });
        }

        public void ChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest request)
        {
            int newRank = 0;
            var (allianceId, guildId) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                var targetPlayer = _server.CharacterManager.FindPlayerById(request.PlayerId);
                if (targetPlayer == null)
                {
                    return AllianceUpdateResult.PlayerNotExisted;
                }
                newRank = targetPlayer.Character.AllianceRank + request.Delta;
                if (newRank < 3 || newRank > 5)
                {
                    return AllianceUpdateResult.RankLimitted;
                }
                targetPlayer.Character.AllianceRank = newRank;
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceMemberRankChanged(new AllianceProto.ChangePlayerAllianceRankResponse { Code = (int)code, NewRank = newRank, Request = request, AllianceId = allianceId });
        }

        public void DisbandAlliance(AllianceProto.DisbandAllianceRequest request)
        {
            var (allianceId, guildId) = HandleAllianceRequest(request.MasterId, (master, alliance, guild) =>
            {
                if (master.Character.AllianceRank != 1)
                {
                    return AllianceUpdateResult.NotAllianceLeader;
                }
                var allianceGuilds = alliance.Guilds.Select(GetLocalGuild).ToList();
                foreach (var item in allianceGuilds)
                {
                    item.AllianceId = 0;
                    var guildMembers = item.Members.Select(_server.CharacterManager.FindPlayerById);
                    foreach (var member in guildMembers)
                    {
                        if (member != null)
                            member.Character.AllianceRank = 5;
                    }
                }
                SetAllianceRemoved(alliance);
                return 0;
            }, out var code);

            _server.Transport.BroadcastAllianceDisband(new AllianceProto.DisbandAllianceResponse { Code = (int)code, Request = request, AllianceId = allianceId });

        }

        #endregion

        public QueryRankedGuildsResponse LoadRankedGuilds()
        {
            var list = _idGuildDataSource.Values.OrderByDescending(x => x.GP).Take(50);
            var res = new QueryRankedGuildsResponse();
            res.Guilds.AddRange(_mapper.Map<GuildDto[]>(list));
            return res;
        }

        #region Guild
        ConcurrentDictionary<int, StoreUnit<GuildModel>> _guild = new();
        public void SetGuildUpdate(GuildModel data)
        {
            _guild[data.GuildId] = new StoreUnit<GuildModel>(StoreFlag.AddOrUpdate, data);
        }
        public void SetGuildRemoved(GuildModel data)
        {
            _guild[data.GuildId] = new StoreUnit<GuildModel>(StoreFlag.Remove, data);
        }
        public async Task CommitGuildAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, StoreUnit<GuildModel>>();
            foreach (var key in _guild.Keys.ToList())
            {
                _guild.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Guilds.Where(x => updateData.Keys.Contains(x.GuildId)).ToList();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Guilds.Where(x => x.GuildId == obj.GuildId).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.GuildId == obj.GuildId);
                    if (dbModel == null)
                    {
                        dbModel = new GuildEntity(obj.GuildId, obj.Name, obj.Leader);
                        dbContext.Add(dbModel);
                    }
                    dbModel.GP = obj.GP;
                    dbModel.Rank1Title = obj.Rank1Title;
                    dbModel.Rank2Title = obj.Rank2Title;
                    dbModel.Rank3Title = obj.Rank3Title;
                    dbModel.Rank4Title = obj.Rank4Title;
                    dbModel.Rank5Title = obj.Rank5Title;
                    dbModel.Logo = obj.Logo;
                    dbModel.LogoBg = obj.LogoBg;
                    dbModel.LogoBgColor = obj.LogoBgColor;
                    dbModel.LogoColor = obj.LogoColor;
                    dbModel.AllianceId = obj.AllianceId;
                    dbModel.Capacity = obj.Capacity;
                    dbModel.Name = obj.Name;
                    dbModel.Notice = obj.Notice;
                    dbModel.Signature = obj.Signature;
                    dbModel.Leader = obj.Leader;
                }

            }
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region Alliance
        ConcurrentDictionary<int, StoreUnit<AllianceModel>> _alliance = new();
        public void SetAlliance(AllianceModel data)
        {
            _alliance[data.Id] = new StoreUnit<AllianceModel>(StoreFlag.AddOrUpdate, data);
        }
        public void SetAllianceRemoved(AllianceModel data)
        {
            _alliance[data.Id] = new StoreUnit<AllianceModel>(StoreFlag.Remove, data);
        }
        public async Task CommitAllianceAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, StoreUnit<AllianceModel>>();
            foreach (var key in _alliance.Keys.ToList())
            {
                _alliance.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Alliances.Where(x => updateData.Keys.Contains(x.Id)).ToList();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Alliances.Where(x => x.Id == obj.Id).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.Id == obj.Id);
                    if (dbModel == null)
                    {
                        dbModel = new AllianceEntity(obj.Id, obj.Name);
                        dbContext.Add(dbModel);
                    }
                    dbModel.Capacity = obj.Capacity;
                    dbModel.Name = obj.Name;
                    dbModel.Notice = obj.Notice;
                    dbModel.Rank1 = obj.Rank1;
                    dbModel.Rank2 = obj.Rank2;
                    dbModel.Rank3 = obj.Rank3;
                    dbModel.Rank4 = obj.Rank4;
                    dbModel.Rank5 = obj.Rank5;
                }

            }
            await dbContext.SaveChangesAsync();
        }
        #endregion

        public async Task Commit(DBContext dbContext)
        {
            await CommitAllianceAsync(dbContext);
            await CommitGuildAsync(dbContext);
        }
    }
}
