using Application.Core.Game.Relation;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Services;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Job;
using Application.Shared.Guild;
using Application.Shared.Team;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.server.guild;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Xml.Linq;
using XmlWzReader;
using static Mysqlx.Notice.Warning.Types;

namespace Application.Core.Login.ServerData
{
    public class GuildManager: IDisposable
    {
        ConcurrentDictionary<int, GuildModel> _idGuildDataSource = new();
        ConcurrentDictionary<string, GuildModel> _nameGuildDataSource = new();
        int _currentGuildId = 1;

        ConcurrentDictionary<int, AllianceModel> _idAllianceDataSource = new();

        readonly MasterServer _server;
        readonly ILogger<GuildManager> _logger;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly NoteService _noteService;

        public GuildManager(MasterServer server, ILogger<GuildManager> logger, IMapper mapper, IDbContextFactory<DBContext> dbContext, NoteService noteService)
        {
            _server = server;
            _logger = logger;
            _mapper = mapper;
            _dbContextFactory = dbContext;
            _noteService = noteService;
        }

        public void Initialize()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            //var dataList = dbContext.Guilds.AsNoTracking().ToList();
            //foreach (var item in dataList)
            //{
            //    var model = _mapper.Map<GuildModel>(item);
            //    _idDataSource[model.GuildId] = model;
            //    _nameDataSource[model.Name] = model;

            //    _currentId = model.GuildId > _currentId ? model.GuildId : _currentId;
            //}
            //_currentId += 1;
            _currentGuildId = dbContext.Guilds.Max(x => x.GuildId) + 1;
        }

        public Dto.GuildDto? CreateGuild(string guildName, int leaderId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            if (_nameGuildDataSource.Keys.Contains(guildName))
                return null;

            var header = _server.CharacterManager.FindPlayerById(leaderId);
            if (header == null || header.Character.GuildId > 0)
                return null;

            var guildModel = new GuildModel() { GuildId = Interlocked.Increment(ref _currentGuildId), Name = guildName, Leader = leaderId };

            header.Character.GuildId = guildModel.GuildId;

            var response = new Dto.GuildDto();
            response.GuildId = guildModel.GuildId;
            response.Leader = guildModel.Leader;
            response.Members.AddRange(_mapper.Map<Dto.GuildMemberDto[]>(new CharacterLiveObject[] { header }));
            return response;
        }

        public GuildModel? GetLocalGuild(int guildId)
        {
            if (_idGuildDataSource.TryGetValue(guildId, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guildId);
            d = _mapper.Map<GuildModel>(dbModel);
            d.Members = _server.CharacterManager.GetGuildMembers(guildId).Select(x => x.Character.Id).ToList();
            return d;
        }
        public Dto.GuildDto? GetGuildFull(int teamId)
        {
            var data = GetLocalGuild(teamId);
            if (data == null)
                return null;

            var response = new Dto.GuildDto();
            response.GuildId = teamId;
            response.Leader = data.Leader;
            response.Members.AddRange(_mapper.Map<Dto.GuildMemberDto[]>(data.Members.Select(_server.CharacterManager.FindPlayerById)));
            return response;
        }


        public void Dispose()
        {
            _idGuildDataSource.Clear();
            _nameGuildDataSource.Clear();
        }

        public UpdateGuildResponse UpdateGuildMember(UpdateGuildMemberRequest request)
        {
            var response = new Dto.UpdateGuildResponse()
            {
                UpdateType = 1
            };
            var code = 0;

            var chrFrom = _server.CharacterManager.FindPlayerById(request.FromId)!;

            var chrTo = request.FromId == request.ToId ? chrFrom : _server.CharacterManager.FindPlayerById(request.ToId)!;

            var guild = GetLocalGuild(request.GuildId);
            if (guild == null)
                code = 1;
            else
            {
                var operation = (GuildOperation)request.Operation;
                switch (operation)
                {
                    case GuildOperation.AddMember:
                        if (chrFrom.Character.GuildId > 0)
                            code = 2;
                        else
                        {
                            guild.Members.Add(chrFrom.Character.Id);
                            chrFrom.Character.GuildId = guild.GuildId;
                            chrFrom.Character.GuildRank = 5;
                        }        
                        break;
                    case GuildOperation.ChangeRank:
                        if (chrFrom.Character.GuildId == chrTo.Character.GuildId && chrFrom.Character.GuildRank < chrTo.Character.GuildRank)
                        {
                            chrTo.Character.GuildRank = request.ToRank;
                        }
                        break;
                    case GuildOperation.ExpelMember:
                        if (chrFrom.Character.GuildRank > 2)
                        {
                            _logger.LogWarning("[Hack] Chr {CharacterName} is trying to expel without rank 1 or 2", chrFrom.Character.Name);
                        }
                        else if (chrFrom.Character.GuildId == chrTo.Character.GuildId && chrFrom.Character.GuildRank < chrTo.Character.GuildRank)
                        {
                            if (chrTo.Channel == 0)
                            {
                                _noteService.sendNormal("You have been expelled from the guild.", chrFrom.Character.Name, chrTo.Character.Name, _server.getCurrentTime());
                            }

                            chrTo.Character.GuildId = 0;
                            chrTo.Character.GuildRank = 5;
                            guild.Members.Remove(chrTo.Character.Id);
                        }
                        break;
                    case GuildOperation.Leave:
                        if (chrFrom.Character.Id == guild.Leader)
                        {
                            // disband
                        }
                        chrFrom.Character.GuildId = 0;
                        chrFrom.Character.GuildRank = 5;
                        break;
                    default:
                        break;
                }
            }
            if (code == 0)
            {
                _server.Transport.BroadcastGuildUpdate(request.FromChannel, response);
            }
            return response;
        }


        public void BroadcastJobChanged(string name, int jobId, int id)
        {
            var chr = _server.CharacterManager.FindPlayerById(id);
            if (chr != null)
            {
                var guild = GetLocalGuild(chr.Character.GuildId);
                if (guild != null)
                {
                    var targets = _server.CharacterManager.GetPlayerChannelPair(guild.Members);
                    targets.Remove(id);
                    _server.Transport.BroadcastJobChanged(0, targets, name, jobId);
                }

            }
        }

        public void BroadcastLevelChanged(string name, int level, int id)
        {
            var chr = _server.CharacterManager.FindPlayerById(id);
            if (chr != null)
            {
                var guild = GetLocalGuild(chr.Character.GuildId);
                if (guild != null)
                {
                    var targets = _server.CharacterManager.GetPlayerChannelPair(guild.Members);
                    targets.Remove(id);
                    _server.Transport.BroadcastLevelChanged(0, targets, name, level);
                }

            }
        }

        public UpdateGuildResponse UpdateGuild(Dto.UpdateGuildRequest request)
        {
            var response = new Dto.UpdateGuildResponse()
            {
                UpdateType = 1
            };

            var code = 0;

            var chrFrom = _server.CharacterManager.FindPlayerById(request.OperatorId)!;

            var guild = GetLocalGuild(request.GuildId);
            if (guild == null)
                code = 1;
            else
            {
                var operation = (GuildInfoOperation)request.Operation;
                switch (operation)
                {
                    case GuildInfoOperation.ChangeName:
                        guild.Name = request.UpdateFields.Name;
                        break;
                    case GuildInfoOperation.ChangeEmblem:
                        guild.Logo = request.UpdateFields.Logo;
                        guild.LogoBg = request.UpdateFields.LogoBg;
                        guild.LogoColor = (short)request.UpdateFields.LogoColor;
                        guild.LogoBgColor = (short)request.UpdateFields.LogoBgColor;
                        break;
                    case GuildInfoOperation.ChangeRankTitle:
                        guild.Rank1Title = request.UpdateFields.Rank1Title;
                        guild.Rank2Title = request.UpdateFields.Rank2Title;
                        guild.Rank3Title = request.UpdateFields.Rank3Title;
                        guild.Rank4Title = request.UpdateFields.Rank4Title;
                        guild.Rank5Title = request.UpdateFields.Rank5Title;
                        break;
                    case GuildInfoOperation.ChangeNotice:
                        guild.Notice = request.UpdateFields.Notice;
                        break;
                    case GuildInfoOperation.IncreaseCapacity:
                        guild.Capacity += 5;
                        break;
                    case GuildInfoOperation.Disband:
                        var allMember = guild.Members.Select(_server.CharacterManager.FindPlayerById).ToArray();
                        foreach (var member in allMember)
                        {
                            if (member != null)
                            {
                                member.Character.GuildId = 0;
                                member.Character.GuildRank = 5;
                            }
                        }
                        _idGuildDataSource.Remove(guild.GuildId, out _);
                        _nameGuildDataSource.Remove(guild.Name, out _);
                        break;
                    default:
                        break;
                }

            }
            response.UpdatedGuild = _mapper.Map<Dto.GuildDto>(guild);
            if (code == 0)
                _server.Transport.BroadcastGuildUpdate(request.FromChannel, response);

            return response;
        }


        public AllianceModel? GetLocalAlliance(int allianceId)
        {
            if (_idAllianceDataSource.TryGetValue(allianceId, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Alliances.AsNoTracking().FirstOrDefault(x => x.Id == allianceId);
            d = _mapper.Map<AllianceModel>(dbModel);
            d.Guilds = dbContext.Guilds.Where(x => x.AllianceId == allianceId).Select(x => x.GuildId).ToList();
            return d;
        }


        public UpdateAllianceResponse UpdateAllianceMember(Dto.UpdateAllianceMemberRequest request)
        {
            var response = new Dto.UpdateAllianceResponse()
            {
                UpdateType = 1
            };

            var code = 0;


            var alliance = GetLocalAlliance(request.GuildId);
            if (alliance == null)
                code = 1;
            else
            {
                var operation = (AllianceOperation)request.Operation;
                switch (operation)
                {
                    case AllianceOperation.LeaveAlliance:
                        alliance.Guilds.Remove(request.GuildId);
                        break;
                    case AllianceOperation.ExpelGuild:
                        alliance.Guilds.Remove(request.GuildId);
                        break;
                    case AllianceOperation.AddGuild:
                        alliance.Guilds.Add(request.GuildId);
                        break;
                    default:
                        break;
                }

            }
            if (code == 0)
                _server.Transport.BroadcastAllianceUpdate(request.FromChannel, response);

            return response;
        }
    }
}
