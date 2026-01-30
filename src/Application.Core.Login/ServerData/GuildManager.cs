using AllianceProto;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Guild;
using Application.Shared.Message;
using Application.Shared.Team;
using Application.Utility;
using AutoMapper;
using Dto;
using Google.Protobuf;
using GuildProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlWzReader;

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

        public async Task CreateGuild(GuildProto.CreateGuildRequest request)
        {
            var res = new GuildProto.CreateGuildResponse { Request = request };
            if (_nameGuildDataSource.ContainsKey(request.Name))
            {
                res.Code = (int)GuildUpdateResult.Create_NameDumplicate;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCreated, res, [request.LeaderId]);
                return;
            }

            var header = _server.CharacterManager.FindPlayerById(request.LeaderId);
            if (header == null || header.Character.GuildId > 0)
            {
                res.Code = (int)GuildUpdateResult.Create_AlreadyInGuild;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCreated, res, [request.LeaderId]);
                return;
            }

            var memberList = request.Members.Select(_server.CharacterManager.FindPlayerById).Where(x => x != null).ToList();
            if (memberList.Any(x => x!.Character.GuildId != 0))
            {
                res.Code = (int)GuildUpdateResult.Create_AlreadyInGuild;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCreated, res, [request.LeaderId]);
                return;
            }

            var guildModel = new GuildModel() { 
                GuildId = Interlocked.Increment(ref _currentGuildId), 
                Name = request.Name, 
                Leader = request.LeaderId, 
                Signature = _server.getCurrentTime() 
            };

            _idGuildDataSource[guildModel.GuildId] = guildModel;
            _nameGuildDataSource[guildModel.Name] = guildModel;


            foreach (var member in memberList)
            {
                if (member!.Character.Id == request.LeaderId)
                {
                    header.Character.GuildId = guildModel.GuildId;
                    header.Character.GuildRank = 1;
                }
                else
                {
                    member.Character.GuildId = guildModel.GuildId;
                    member.Character.GuildRank = 2;
                    member.Character.AllianceRank = 5;
                }
            }

            SetGuildUpdate(guildModel);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCreated, res, request.Members);
        }

        public GuildModel? GetLocalGuild(int guildId)
        {
            return _idGuildDataSource.GetValueOrDefault(guildId);
        }

        GuildProto.GuildDto? MapGuildDto(GuildModel data)
        {
            var response = _mapper.Map<GuildProto.GuildDto>(data);
            response.Members.AddRange(_mapper.Map<GuildProto.GuildMemberDto[]>(GetGuildMembers(data)));
            return response;
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

        public IEnumerable<int> GetAllianceMembers(GuildModel guild)
        {
            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                return guild.Members;
            }

            return alliance.Guilds.Select(x => GetLocalGuild(x)).Where(x => x != null).SelectMany(x => x!.Members);
        }
        public IEnumerable<int> GetAllianceMembers(AllianceModel alliance)
        {
            return alliance.Guilds.Select(x => GetLocalGuild(x)).Where(x => x != null).SelectMany(x => x!.Members);
        }
        public IEnumerable<int> GetAllianceMembers(AllianceDto alliance)
        {
            return alliance.Guilds.SelectMany(x => x.Members.Select(y => y.Id));
        }
        public AllianceModel? GetLocalAlliance(int allianceId)
        {
            return _idAllianceDataSource.GetValueOrDefault(allianceId);
        }
        public AllianceProto.AllianceDto? GetAllianceDto(int allianceId)
        {
            var data = GetLocalAlliance(allianceId);
            if (data == null)
                return null;

            return MapAllianceDto(data);
        }

        AllianceProto.AllianceDto MapAllianceDto(AllianceModel data)
        {
            var response = _mapper.Map<AllianceProto.AllianceDto>(data);
            response.Guilds.AddRange(data.Guilds.Select(x => GetGuildFull(x)));
            return response;
        }

        public AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request)
        {
            return new AllianceProto.CreateAllianceCheckResponse() { IsValid = !_nameAllianceDataSource.Keys.Contains(request.Name) };
        }
        public async Task CreateAlliance(CreateAllianceRequest request)
        {
            var res = new CreateAllianceResponse { Request = request };
            if (_nameAllianceDataSource.Keys.Contains(request.Name))
            {
                res.Code = (int)AllianceUpdateResult.Create_NameInvalid;
                return;
            }

            if (request.Members.Count != 2)
            {
                res.Code = (int)AllianceUpdateResult.Create_Error;
                return;
            }

            var first = _server.CharacterManager.FindPlayerById(request.Members[0]);
            if (first == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                return;
            }

            var second = _server.CharacterManager.FindPlayerById(request.Members[1]);
            if (second == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                return;
            }

            if (first.Character.GuildRank != 1 || second.Character.GuildRank != 1)
            {
                res.Code = (int)AllianceUpdateResult.NotGuildLeader;
                return;
            }

            var guild1 = GetLocalGuild(first.Character.GuildId);
            var guild2 = GetLocalGuild(second.Character.GuildId);
            if (guild1 == null || guild2 == null || guild1.AllianceId != 0 || guild2.AllianceId != 0)
            {
                res.Code = (int)AllianceUpdateResult.AlreadyInAlliance;
                return;
            }

            var allianceModel = new AllianceModel()
            {
                Id = Interlocked.Increment(ref _currentAllianceId),
                Name = request.Name,
                Guilds = [guild1.GuildId, guild2.GuildId],
                Capacity = 2,
            };

            _idAllianceDataSource[allianceModel.Id] = allianceModel;
            _nameAllianceDataSource[request.Name] = allianceModel;
            SetAllianceUpdate(allianceModel);

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

            res.Model = _mapper.Map<AllianceProto.AllianceDto>(allianceModel);
            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceCreated, res, request.Members);
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

        public async Task SendGuildMessage(int guildId, int v, string callout)
        {
            if (_idGuildDataSource.TryGetValue(guildId, out var guild))
            {
                await _server.DropWorldMessage(v, callout, guild.Members.ToArray());
            }
        }

        public async Task SendGuildPacket(GuildProto.GuildPacketRequest data)
        {
            if (_idGuildDataSource.TryGetValue(data.GuildId, out var guild))
            {
                await _server.BroadcastPacket(new MessageProto.PacketRequest { Data = data.Data }, guild.Members.Where(x => x != data.ExceptChrId));
            }
        }

        public async Task UpdateGuildGPAsync(UpdateGuildGPRequest request)
        {
            var response = new UpdateGuildGPResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildGpUpdate, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildGpUpdate, response, [request.MasterId]);
                return;
            }

            guild.GP += request.Gp;

            response.GuildGP = guild.GP;

            response.GuildMembers.AddRange(guild.Members);
            response.GuildId = guild.GuildId;
            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildGpUpdate, response, response.GuildMembers);
        }

        public async Task UpdateGuildRankTitle(UpdateGuildRankTitleRequest request)
        {
            var response = new UpdateGuildRankTitleResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankTitleUpdate, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankTitleUpdate, response, [request.MasterId]);
                return;
            }

            guild.Rank1Title = request.RankTitles[0];
            guild.Rank2Title = request.RankTitles[1];
            guild.Rank3Title = request.RankTitles[2];
            guild.Rank4Title = request.RankTitles[3];
            guild.Rank5Title = request.RankTitles[4];
            SetGuildUpdate(guild);

            response.GuildId = guild.GuildId;
            response.GuildMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankTitleUpdate, response, response.GuildMembers);
        }

        public async Task UpdateGuildNotice(UpdateGuildNoticeRequest request)
        {
            var response = new UpdateGuildNoticeResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildNoticeUpdate, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildNoticeUpdate, response, [request.MasterId]);
                return;
            }

            guild.Notice = request.Notice;
            SetGuildUpdate(guild);

            response.GuildId = guild.GuildId;
            response.GuildMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildNoticeUpdate, response, response.GuildMembers);
        }

        public async Task IncreseGuildCapacity(UpdateGuildCapacityRequest request)
        {
            var response = new UpdateGuildCapacityResponse { Request = request };
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCapacityUpdate, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCapacityUpdate, response, [request.MasterId]);
                return;
            }

            if (guild.Capacity > 99)
            {
                response.Code = (int)GuildUpdateResult.GuildFull;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCapacityUpdate, response, [request.MasterId]);
                return;
            }

            guild.Capacity += 5;
            SetGuildUpdate(guild);

            response.GuildCapacity = guild.Capacity;
            response.GuildId = guild.GuildId;
            response.GuildMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildCapacityUpdate, response, response.GuildMembers);
        }

        public async Task UpdateGuildEmblem(UpdateGuildEmblemRequest request)
        {
            var response = new UpdateGuildEmblemResponse {  Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildEmblemUpdate, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildEmblemUpdate, response, [request.MasterId]);
                return;
            }

            guild.Logo = request.Logo;
            guild.LogoBg = request.LogoBg;
            guild.LogoColor = (short)request.LogoColor;
            guild.LogoBgColor = (short)request.LogoBgColor;
            SetGuildUpdate(guild);

            response.GuildId = guild.GuildId;
            response.AllianceId = guild.AllianceId;
            response.AllianceDto = GetAllianceDto(guild.AllianceId);
            if (response.AllianceDto != null)
            {
                response.AllMembers.AddRange(GetAllianceMembers(response.AllianceDto));
            }
            else
            {
                response.AllMembers.AddRange(guild.Members);
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildEmblemUpdate, response, response.AllMembers);
        }

        public async Task DisbandGuild(GuildDisbandRequest request)
        {
            var response = new GuildDisbandResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildDisband, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildDisband, response, [request.MasterId]);
                return;
            }

            List<int> notifyMembers = [];
            notifyMembers.AddRange(guild.Members);
            foreach (var member in GetGuildMembers(guild))
            {
                if (member != null)
                {
                    member.Character.GuildId = 0;
                    member.Character.GuildRank = 5;
                    member.Character.AllianceRank = 5;
                }
            }

            if (guild.AllianceId > 0)
            {
                var alliance = GetLocalAlliance(guild.AllianceId);
                if (alliance != null)
                {
                    alliance.TryRemoveGuild(guild.GuildId, out _);

                    response.AllianceId = guild.AllianceId;
                    response.AllianceDto = MapAllianceDto(alliance);
                    notifyMembers.AddRange(response.AllianceDto.Guilds.SelectMany(x => x.Members.Select(y => y.Id)));
                }
                guild.AllianceId = 0;
            }


            SetGuildRemoved(guild!);
            response.GuildId = guild.GuildId;

            response.AllMembers.AddRange(notifyMembers);
            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildDisband, response, notifyMembers);
        }


        public async Task ChangePlayerGuildRank(UpdateGuildMemberRankRequest request)
        {
            var response = new UpdateGuildMemberRankResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankChanged, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankChanged, response, [request.MasterId]);
                return;
            }

            var chrTo = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
            if (chrTo == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankChanged, response, [request.MasterId]);
                return;
            }

            if (master.Character.GuildId != chrTo.Character.GuildId || master.Character.GuildRank >= chrTo.Character.GuildRank)
            {
                response.Code = (int)GuildUpdateResult.MasterRankFail;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankChanged, response, [request.MasterId]);
                return;
            }

            chrTo.Character.GuildRank = request.NewRank;

            response.GuildId = guild.GuildId;
            response.GuildMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildRankChanged, response, response.GuildMembers);
        }

        public async Task GuildExpelMember(ExpelFromGuildRequest request)
        {
            var response = new ExpelFromGuildResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, [request.MasterId]);
                return;
            }

            var chrTo = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
            if (chrTo == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, [request.MasterId]);
                return;
            }

            if (master.Character.GuildRank > 2)
            {
                _logger.LogWarning("[Hack] Chr {CharacterName} is trying to expel without rank 1 or 2", master.Character.Name);
                response.Code = (int)GuildUpdateResult.MasterRankFail;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, [request.MasterId]);
                return;
            }

            if (master.Character.GuildId != chrTo.Character.GuildId || master.Character.GuildRank >= chrTo.Character.GuildRank)
            {
                response.Code = (int)GuildUpdateResult.MasterRankFail;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, [request.MasterId]);
                return;
            }

            List<int> notifyMembers = [];

            if (chrTo.Channel == 0)
            {
                await _server.NoteManager
                    .SendNormal("You have been expelled from the guild.", master.Character.Id, chrTo.Character.Name);
            }

            chrTo.Character.GuildId = 0;
            chrTo.Character.GuildRank = 5;
            chrTo.Character.AllianceRank = 5;

            guild.Members.Remove(chrTo.Character.Id);
            SetGuildUpdate(guild);

            response.GuildId = guild.GuildId;
            response.TargetName = chrTo.Character.Name;

            response.AllianceId = guild.AllianceId;
            response.AllianceDto = GetAllianceDto(guild.AllianceId);
            if (response.AllianceDto != null)
            {
                response.AllLeftMembers.AddRange(GetAllianceMembers(response.AllianceDto));
            }
            else
            {
                response.AllLeftMembers.AddRange(guild.Members);
            }


            notifyMembers.Add(request.MasterId); // 操作者
            notifyMembers.Add(request.TargetPlayerId); // 被操作者
            notifyMembers.AddRange(response.AllLeftMembers); // 家族/联盟剩余人员

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildExpelMember, response, notifyMembers);
        }

        public async Task PlayerLeaveGuild(LeaveGuildRequest request)
        {
            var response = new LeaveGuildResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.PlayerId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerLeaveGuild, response, [request.PlayerId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerLeaveGuild, response, [request.PlayerId]);
                return;
            }

            if (master.Character.Id == guild.Leader)
            {
                response.Code = (int)GuildUpdateResult.LeaderCannotLeave;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerLeaveGuild, response, [request.PlayerId]);
                return;
            }

            master.Character.GuildId = 0;
            master.Character.GuildRank = 5;
            master.Character.AllianceRank = 5;

            List<int> notifyMembers = [];

            guild.Members.Remove(master.Character.Id);
            SetGuildUpdate(guild);

            response.GuildId = guild.GuildId;
            response.MasterName = master.Character.Name;

            response.AllianceId = guild.AllianceId;
            response.AllianceDto = GetAllianceDto(guild.AllianceId);
            if (response.AllianceDto != null)
            {
                response.AllLeftMembers.AddRange(GetAllianceMembers(response.AllianceDto));
            }
            else
            {
                response.AllLeftMembers.AddRange(guild.Members);
            }

            notifyMembers.Add(request.PlayerId);
            notifyMembers.AddRange(response.AllLeftMembers);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerLeaveGuild, response, notifyMembers);
        }

        public async Task PlayerJoinGuild(JoinGuildRequest request)
        {
            var response = new JoinGuildResponse { Request = request };
            var master = _server.CharacterManager.FindPlayerById(request.PlayerId);
            if (master == null)
            {
                response.Code = (int)GuildUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerJoinGuild, response, [request.PlayerId]);
                return;
            }

            if (master.Character.GuildId > 0)
            {
                response.Code = (int)GuildUpdateResult.Join_AlreadyInGuild;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerJoinGuild, response, [request.PlayerId]);
                return;
            }

            var guild = GetLocalGuild(request.GuildId);
            if (guild == null)
            {
                response.Code = (int)GuildUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerJoinGuild, response, [request.PlayerId]);
                return;
            }

            if (guild.Members.Count >= guild.Members.Capacity)
            {
                response.Code = (int)GuildUpdateResult.GuildFull;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerJoinGuild, response, [request.PlayerId]);
                return;
            }

            guild.Members.Add(master.Character.Id);
            SetGuildUpdate(guild);

            master.Character.GuildId = guild.GuildId;
            master.Character.GuildRank = 5;
            master.Character.AllianceRank = 5;

            response.GuildDto = MapGuildDto(guild);

            response.AllianceId = guild.AllianceId;
            response.AllianceDto = GetAllianceDto(guild.AllianceId);
            if (response.AllianceDto != null)
            {
                response.AllMembers.AddRange(GetAllianceMembers(response.AllianceDto));
            }
            else
            {
                response.AllMembers.AddRange(guild.Members);
            }

            List<int> notifyMembers = [];
            notifyMembers.Add(request.PlayerId);
            notifyMembers.AddRange(response.AllMembers); 

            await _server.Transport.SendMessageN(ChannelRecvCode.OnPlayerJoinGuild, response, notifyMembers);
        }

        #endregion

        #region Alliance
        public async Task GuildJoinAlliance(AllianceProto.GuildJoinAllianceRequest request)
        {
            var response = new AllianceProto.GuildJoinAllianceResponse { Request = request };
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                response.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                response.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, [request.MasterId]);
                return;
            }

            if (guild.AllianceId > 0)
            {
                response.Code = (int)AllianceUpdateResult.AlreadyInAlliance;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(request.AllianceId);
            if (alliance == null)
            {
                response.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, [request.MasterId]);
                return;
            }

            if (!alliance.TryAddGuild(guild.GuildId, out var code))
            {
                response.Code = (int)code;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, [request.MasterId]);
                return;
            }
            SetAllianceUpdate(alliance);

            var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
            foreach (var member in guildMembers)
            {
                if (member != null)
                    member.Character.AllianceRank = 5;
            }
            guild.AllianceId = alliance.Id;
            SetGuildUpdate(guild);

            master.Character.AllianceRank = 2;

            response.GuildId = guild.GuildId;
            response.AllianceId = alliance.Id;
            response.AllianceDto = MapAllianceDto(alliance);

            List<int> notifyMembers = [];
            notifyMembers.AddRange(GetAllianceMembers(response.AllianceDto));
            notifyMembers.Add(request.MasterId);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildJoinAlliance, response, notifyMembers);
        }

        public async Task GuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest request)
        {
            var res = new AllianceProto.GuildLeaveAllianceResponse { Request = request };
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildLeaveAlliance, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildLeaveAlliance, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildLeaveAlliance, res, [request.MasterId]);
                return;
            }

            if (!alliance.TryRemoveGuild(master.Character.GuildId, out var code))
            {
                res.Code = (int)code;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildLeaveAlliance, res, [request.MasterId]);
                return;
            }
            SetAllianceUpdate(alliance);

            guild.AllianceId = 0;
            var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
            foreach (var member in guildMembers)
            {
                if (member != null)
                    member.Character.AllianceRank = 5;
            }
            SetGuildUpdate(guild);

            res.AllianceId = alliance.Id;
            res.AllianceDto = MapAllianceDto(alliance);
            res.GuildDto = MapGuildDto(guild);
            res.GuildId = guild.GuildId;

            List<int> notifyMembers = [];
            notifyMembers.AddRange(GetAllianceMembers(res.AllianceDto));
            notifyMembers.Add(request.MasterId);
            notifyMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnGuildLeaveAlliance, res, notifyMembers);
        }

        public async Task AllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest request)
        {
            var res = new AllianceProto.AllianceExpelGuildResponse { Request = request };
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            var guild = GetLocalGuild(masterChr.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceExpelGuild, res, [request.MasterId]);
                return;
            }

            if (!_idAllianceDataSource.TryGetValue(guild.AllianceId, out var alliance))
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceExpelGuild, res, [request.MasterId]);
                return;
            }

            if (masterChr.Character.AllianceRank != 1)
            {
                res.Code = (int)AllianceUpdateResult.NotAllianceLeader;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceExpelGuild, res, [request.MasterId]);
                return;
            }

            if (!alliance.TryRemoveGuild(guild.GuildId, out var code))
            {
                res.Code = (int)AllianceUpdateResult.GuildNotInAlliance;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceExpelGuild, res, [request.MasterId]);
                return;
            }
            SetAllianceUpdate(alliance);

            guild.AllianceId = 0;
            var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
            foreach (var member in guildMembers)
            {
                if (member != null)
                    member.Character.AllianceRank = 5;
            }
            SetGuildUpdate(guild);

            res.GuildId = guild.GuildId;
            res.GuildDto = MapGuildDto(guild);

            res.AllianceId = alliance.Id;
            res.AllianceDto = MapAllianceDto(alliance);

            List<int> notifyMembers = [];
            notifyMembers.AddRange(GetAllianceMembers(res.AllianceDto));
            notifyMembers.Add(request.MasterId);
            notifyMembers.AddRange(guild.Members);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceExpelGuild, res, notifyMembers);
        }

        public async Task IncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest request)
        {
            var res = new AllianceProto.IncreaseAllianceCapacityResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceCapacityUpdate, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceCapacityUpdate, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceCapacityUpdate, res, [request.MasterId]);
                return;
            }

            alliance.Capacity += 1;
            SetAllianceUpdate(alliance);

            res.AllianceId = alliance.Id;
            res.AllianceDto = MapAllianceDto(alliance);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceCapacityUpdate, res,  GetAllianceMembers(res.AllianceDto));
        }

        public async Task UpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request)
        {
            var res = new AllianceProto.UpdateAllianceRankTitleResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceRankTitleUpdate, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceRankTitleUpdate, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceRankTitleUpdate, res, [request.MasterId]);
                return;
            }

            alliance.Rank1 = request.RankTitles[0];
            alliance.Rank2 = request.RankTitles[1];
            alliance.Rank3 = request.RankTitles[2];
            alliance.Rank4 = request.RankTitles[3];
            alliance.Rank5 = request.RankTitles[4];
            SetAllianceUpdate(alliance);

            res.AllianceId = alliance.Id;
            res.AllMembers.AddRange(GetAllianceMembers(alliance));

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceRankTitleUpdate, res, res.AllMembers);
        }

        public async Task UpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest request)
        {
            var res = new AllianceProto.UpdateAllianceNoticeResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceNoticeUpdate, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceNoticeUpdate, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceNoticeUpdate, res, [request.MasterId]);
                return;
            }

            alliance.Notice = request.Notice;
            SetAllianceUpdate(alliance);

            res.AllianceId = alliance.Id;
            res.AllMembers.AddRange(GetAllianceMembers(alliance));

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceNoticeUpdate, res, res.AllMembers);
        }

        public async Task ChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest request)
        {
            var res = new AllianceProto.AllianceChangeLeaderResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            if (master.Character.AllianceRank != 1)
            {
                res.Code = (int)AllianceUpdateResult.NotAllianceLeader;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            var newLeader = _server.CharacterManager.FindPlayerById(request.PlayerId);
            if (newLeader == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            if (newLeader.Character.AllianceRank != 2)
            {
                res.Code = (int)AllianceUpdateResult.NotGuildLeader;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, [request.MasterId]);
                return;
            }

            master.Character.AllianceRank = 2;
            newLeader.Character.AllianceRank = 1;

            res.AllianceId = alliance.Id;
            res.OldLeaderName = master.Character.Name;
            res.NewLeaderName = newLeader.Character.Name;
            res.AllianceDto = MapAllianceDto(alliance);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceLeaderChanged, res, GetAllianceMembers(res.AllianceDto));
        }

        public async Task ChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest request)
        {
            var res = new AllianceProto.ChangePlayerAllianceRankResponse { Request = request };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, [request.MasterId]);
                return;
            }

            var guild = GetLocalGuild(master.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, [request.MasterId]);
                return;
            }

            var alliance = GetLocalAlliance(guild.AllianceId);
            if (alliance == null)
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, [request.MasterId]);
                return;
            }

            var targetPlayer = _server.CharacterManager.FindPlayerById(request.PlayerId);
            if (targetPlayer == null)
            {
                res.Code = (int)AllianceUpdateResult.PlayerNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, [request.MasterId]);
                return;
            }

            var newRank = targetPlayer.Character.AllianceRank + request.Delta;
            if (newRank < 3 || newRank > 5)
            {
                res.Code = (int)AllianceUpdateResult.RankLimitted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, [request.MasterId]);
                return;
            }

            targetPlayer.Character.AllianceRank = newRank;

            res.AllianceId = alliance.Id;
            res.NewRank = newRank;
            res.AllianceDto = MapAllianceDto(alliance);

            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceMemberRankChanged, res, GetAllianceMembers(res.AllianceDto));
        }

        public async Task DisbandAlliance(AllianceProto.DisbandAllianceRequest request)
        {
            var res = new AllianceProto.DisbandAllianceResponse { Request = request };
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            var guild = GetLocalGuild(masterChr.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceDisband, res, [request.MasterId]);
                return;
            }

            if (!_idAllianceDataSource.TryRemove(guild.AllianceId, out var alliance))
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceDisband, res, [request.MasterId]);
                return;
            }

            if (masterChr.Character.AllianceRank != 1)
            {
                res.Code = (int)AllianceUpdateResult.NotAllianceLeader;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceDisband, res, [request.MasterId]);
                return;
            }

            var allianceGuilds = alliance.Guilds.Select(GetLocalGuild).ToList();
            foreach (var item in allianceGuilds)
            {
                item!.AllianceId = 0;

                var guildMembers = item.Members.Select(_server.CharacterManager.FindPlayerById);
                foreach (var member in guildMembers)
                {
                    if (member != null)
                        member.Character.AllianceRank = 5;
                }
                res.AllMembers.AddRange(item.Members);
                SetGuildUpdate(item);
            }
            SetAllianceRemoved(alliance);

            res.AllianceId = alliance.Id;
            res.Guilds.AddRange(alliance.Guilds);
            await _server.Transport.SendMessageN(ChannelRecvCode.OnAllianceDisband, res, res.AllMembers);

        }

        public async Task AllianceBroadcastPlayerInfo(AllianceBroadcastPlayerInfoRequest request)
        {
            var res = new AllianceProto.AllianceBroadcastPlayerInfoResponse { Request = request };
            var masterChr = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            var guild = GetLocalGuild(masterChr.Character.GuildId);
            if (guild == null)
            {
                res.Code = (int)AllianceUpdateResult.GuildNotExisted;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAlliancePlayerInfoBroadcast, res, [request.MasterId]);
                return;
            }

            if (!_idAllianceDataSource.TryRemove(guild.AllianceId, out var alliance))
            {
                res.Code = (int)AllianceUpdateResult.AllianceNotFound;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnAlliancePlayerInfoBroadcast, res, [request.MasterId]);
                return;
            }

            res.AllianceId = guild.AllianceId;
            res.AllMembers.AddRange(GetAllianceMembers(alliance));
            await _server.Transport.SendMessageN(ChannelRecvCode.OnAlliancePlayerInfoBroadcast, res, res.AllMembers);
        }
        #endregion

        public QueryRankedGuildsResponse LoadRankedGuilds()
        {
            var list = _idGuildDataSource.Values.OrderByDescending(x => x.GP).Take(50);
            var res = new QueryRankedGuildsResponse();
            res.Guilds.AddRange(_mapper.Map<GuildDto[]>(list));
            return res;
        }

        #region Guild Storage
        ConcurrentDictionary<int, StoreUnit<GuildModel>> _guildStorage = new();
        public void SetGuildUpdate(GuildModel data)
        {
            _guildStorage[data.GuildId] = new StoreUnit<GuildModel>(StoreFlag.AddOrUpdate, data);
        }
        public void SetGuildRemoved(GuildModel data)
        {
            _idGuildDataSource.Remove(data.GuildId, out _);
            _nameGuildDataSource.Remove(data.Name, out _);

            _guildStorage[data.GuildId] = new StoreUnit<GuildModel>(StoreFlag.Remove, data);
        }
        public async Task CommitGuildAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, StoreUnit<GuildModel>>();
            foreach (var key in _guildStorage.Keys.ToList())
            {
                _guildStorage.TryRemove(key, out var d);
                updateData[key] = d!;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Guilds.Where(x => updateData.Keys.Contains(x.GuildId)).ToList();
            foreach (var item in updateData)
            {
                var obj = item.Value.Data;
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Guilds.Where(x => x.GuildId == item.Key).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.GuildId == item.Key);
                    if (dbModel == null)
                    {
                        dbModel = new GuildEntity(item.Key, obj!.Name, obj.Leader);
                        dbContext.Add(dbModel);
                    }
                    dbModel.GP = obj!.GP;
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

        #region Alliance Storage
        ConcurrentDictionary<int, StoreUnit<AllianceModel>> _alliance = new();
        public void SetAllianceUpdate(AllianceModel data)
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
                updateData[key] = d!;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Alliances.Where(x => updateData.Keys.Contains(x.Id)).ToList();
            foreach (var item in updateData)
            {
                var obj = item.Value.Data;
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Alliances.Where(x => x.Id == item.Key).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.Id == item.Key);
                    if (dbModel == null)
                    {
                        dbModel = new AllianceEntity(item.Key, obj!.Name);
                        dbContext.Add(dbModel);
                    }
                    dbModel.Capacity = obj!.Capacity;
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
