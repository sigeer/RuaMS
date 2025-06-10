using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Services;
using Application.EF;
using Application.Shared.Guild;
using Application.Shared.Team;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Application.Core.Login.ServerData
{
    public class GuildManager : IDisposable
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

        public void Initialize()
        {
            // 家族、联盟的数据应该不会太多，全部加载省事
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataList = dbContext.Guilds.AsNoTracking().ToList();
            foreach (var item in dataList)
            {
                var model = _mapper.Map<GuildModel>(item);
                _idGuildDataSource[model.GuildId] = model;
                _nameGuildDataSource[model.Name] = model;

                _currentGuildId = model.GuildId > _currentGuildId ? model.GuildId : _currentGuildId;
            }
            //_currentId += 1;
            _currentGuildId = dbContext.Guilds.Count() == 0 ? 0 : dbContext.Guilds.Max(x => x.GuildId);
            _currentAllianceId = dbContext.Alliances.Count() == 0 ? 0 : dbContext.Alliances.Max(x => x.Id);
        }

        public Dto.GuildDto? CreateGuild(string guildName, int leaderId, int[] members)
        {
            if (_nameGuildDataSource.Keys.Contains(guildName))
                return null;

            var header = _server.CharacterManager.FindPlayerById(leaderId);
            if (header == null || header.Character.GuildId > 0)
                return null;

            var guildModel = new GuildModel() { GuildId = Interlocked.Increment(ref _currentGuildId), Name = guildName, Leader = leaderId };

            _idGuildDataSource[guildModel.GuildId] = guildModel;
            _nameGuildDataSource[guildModel.Name] = guildModel;

            var memberList = members.Select(_server.CharacterManager.FindPlayerById).Where(x => x != null).ToList();
            foreach (var member in memberList)
            {
                if (member.Character.Id == leaderId)
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

            var response = new Dto.GuildDto();
            response.GuildId = guildModel.GuildId;
            response.Leader = guildModel.Leader;
            response.Members.AddRange(_mapper.Map<Dto.GuildMemberDto[]>(memberList));
            return response;
        }

        public GuildModel? GetLocalGuild(int guildId)
        {
            if (_idGuildDataSource.TryGetValue(guildId, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = dbContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guildId);
            d = _mapper.Map<GuildModel>(dbModel);
            d.Members = _server.CharacterManager.GetGuildMembers(dbContext, guildId).ToList();
            return d;
        }
        public Dto.GuildDto? GetGuildFull(int guildId)
        {
            var data = GetLocalGuild(guildId);
            if (data == null)
                return null;

            var response = _mapper.Map<Dto.GuildDto>(data);
            response.Members.AddRange(_mapper.Map<Dto.GuildMemberDto[]>(GetGuildMembers(data)));
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
                                _server.ServiceProvider.GetRequiredService<NoteService>()
                                    .sendNormal("You have been expelled from the guild.", chrFrom.Character.Name, chrTo.Character.Name, _server.getCurrentTime());
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


        public void BroadcastJobChanged(CharacterModel character)
        {
            var guildResponse = new UpdateGuildResponse()
            {
                GuildId = character.GuildId,
                UpdatedMember = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)GuildOperation.MemberJobChanged
            };
            _server.Transport.BroadcastGuildUpdate("", guildResponse);

            var allianceResponse = new UpdateAllianceResponse()
            {
                TargetPlayer = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)AllianceOperation.MemberUpdate,
            };
            _server.Transport.BroadcastAllianceUpdate("", allianceResponse);
        }

        public void BroadcastLevelChanged(CharacterModel character)
        {
            var response = new UpdateGuildResponse()
            {
                GuildId = character.GuildId,
                UpdatedMember = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)GuildOperation.MemberLevelChanged
            };
            _server.Transport.BroadcastGuildUpdate("", response);

            var allianceResponse = new UpdateAllianceResponse()
            {
                TargetPlayer = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)AllianceOperation.MemberUpdate,
            };
            _server.Transport.BroadcastAllianceUpdate("", allianceResponse);
        }

        public void BroadcastLogin(CharacterModel character)
        {
            var response = new UpdateGuildResponse()
            {
                GuildId = character.GuildId,
                UpdatedMember = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)GuildOperation.MemberLogin
            };
            _server.Transport.BroadcastGuildUpdate("", response);

            var allianceResponse = new UpdateAllianceResponse()
            {
                TargetPlayer = _mapper.Map<GuildMemberDto>(character),
                UpdateType = 1,
                Operation = (int)AllianceOperation.MemberLogin,
            };
            _server.Transport.BroadcastAllianceUpdate("", allianceResponse);
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
                        foreach (var member in GetGuildMembers(guild))
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

        public void SendGuildChat(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_idGuildDataSource.TryGetValue(sender.Character.GuildId, out var guild))
                {
                    var onlinedGuildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById)
                        .Where(x => x != null && x.Character.Id != sender.Character.Id && x.Channel > 0)
                        .Select(x => new PlayerChannelPair(x.Channel, x.Character.Id)).ToArray();
                    _server.Transport.SendMultiChat(2, nameFrom, onlinedGuildMembers, chatText);
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
        public Dto.AllianceDto? GetAllianceFull(int guildId)
        {
            var data = GetLocalAlliance(guildId);
            if (data == null)
                return null;

            var response = _mapper.Map<Dto.AllianceDto>(data);
            response.Guilds.AddRange(data.Guilds);
            return response;
        }


        public Dto.AllianceDto? CreateAlliance(int[] memberPlayers, string allianceName)
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
            if (guild1 == null || guild2 == null)
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

            var response = _mapper.Map<Dto.AllianceDto>(allianceModel);
            return response;
        }

        List<CharacterLiveObject> GetGuildMembers(GuildModel guild)
        {
            return guild.Members.Select(_server.CharacterManager.FindPlayerById).Where(x => x != null).ToList();
        }

        public UpdateAllianceResponse UpdateAllianceMember(Dto.UpdateAllianceRequest request)
        {
            var response = new Dto.UpdateAllianceResponse()
            {
                UpdateType = 1
            };

            AllianceUpdateResult code = 0;

            var master = _server.CharacterManager.FindPlayerById(request.OperatorPlayerId);
            if (master == null)
                code = AllianceUpdateResult.LeaderNotExisted;
            else
            {
                var guild = GetLocalGuild(master.Character.GuildId);
                if (guild == null)
                {
                    code = AllianceUpdateResult.GuildNotExsited;
                }
                else
                {
                    var alliance = GetLocalAlliance(request.AllianceId);

                    if (alliance == null)
                        code = AllianceUpdateResult.AllianceNotFound;
                    else
                    {
                        var operation = (AllianceOperation)request.Operation;
                        switch (operation)
                        {
                            case AllianceOperation.LeaveAlliance:
                                if (master.Character.GuildRank != 1)
                                {
                                    code = AllianceUpdateResult.NotGuildLeader;
                                    break;
                                }
                                if (alliance.TryRemoveGuild(master.Character.GuildId, out code))
                                {
                                    guild.AllianceId = 0;

                                    var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
                                    foreach (var member in guildMembers)
                                    {
                                        if (member != null)
                                            member.Character.AllianceRank = 5;
                                    }
                                }
                                break;
                            case AllianceOperation.ExpelGuild:
                                var targetGuild = GetLocalGuild(request.TargetGuildId);
                                if (targetGuild == null)
                                {
                                    code = AllianceUpdateResult.GuildNotExsited;
                                    break;
                                }
                                if (alliance.TryRemoveGuild(guild.GuildId, out code))
                                {
                                    targetGuild.AllianceId = 0;
                                    var guildMembers = targetGuild.Members.Select(_server.CharacterManager.FindPlayerById);
                                    foreach (var member in guildMembers)
                                    {
                                        if (member != null)
                                            member.Character.AllianceRank = 5;
                                    }
                                }

                                break;
                            case AllianceOperation.Join:
                                if (guild.AllianceId > 0)
                                {
                                    code = AllianceUpdateResult.GuildAlreadyInAlliance;
                                    break;
                                }
                                if (alliance.TryAddGuild(guild.GuildId, out code))
                                {
                                    var guildMembers = guild.Members.Select(_server.CharacterManager.FindPlayerById);
                                    foreach (var member in guildMembers)
                                    {
                                        if (member != null)
                                            member.Character.AllianceRank = 5;
                                    }
                                    master.Character.AllianceRank = 2;
                                }
                                break;
                            case AllianceOperation.MemberLogin:
                            case AllianceOperation.MemberUpdate:
                                response.TargetPlayer = _mapper.Map<Dto.GuildMemberDto>(_server.CharacterManager.FindPlayerById(request.TargetPlayerId));
                                break;
                            case AllianceOperation.ChangeAllianceLeader:
                                if (master.Character.AllianceRank != 1)
                                {
                                    code = AllianceUpdateResult.NotAllianceLeader;
                                    break;
                                }

                                var newLeader = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
                                if (newLeader == null)
                                {
                                    code = AllianceUpdateResult.LeaderNotExisted;
                                    break;
                                }

                                if (newLeader.Character.GuildRank != 2)
                                {
                                    code = AllianceUpdateResult.NotGuildLeader;
                                    break;
                                }

                                if (newLeader.Channel < 1)
                                {
                                    code = AllianceUpdateResult.PlayerNotOnlined;
                                    break;
                                }

                                master.Character.AllianceRank = 2;
                                newLeader.Character.AllianceRank = 1;
                                response.TargetPlayer = _mapper.Map<Dto.GuildMemberDto>(newLeader);
                                break;
                            case AllianceOperation.IncreasePlayerRank:
                            case AllianceOperation.DecreasePlayerRank:

                                var targetPlayer = _server.CharacterManager.FindPlayerById(request.TargetPlayerId);
                                if (targetPlayer == null)
                                {
                                    code = AllianceUpdateResult.PlayerNotExisted;
                                    break;
                                }
                                var newRank = targetPlayer.Character.AllianceRank + (operation == AllianceOperation.IncreasePlayerRank ? 1 : -1);
                                if (newRank < 3 || newRank > 5)
                                {
                                    break;
                                }
                                targetPlayer.Character.AllianceRank = newRank;
                                response.TargetPlayer = _mapper.Map<Dto.GuildMemberDto>(targetPlayer);
                                break;
                            case AllianceOperation.IncreaseCapacity:
                                alliance.Capacity += 1;
                                break;
                            case AllianceOperation.Disband:
                                if (master.Character.AllianceRank != 1)
                                {
                                    code = AllianceUpdateResult.NotAllianceLeader;
                                    break;
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
                                break;
                            case AllianceOperation.ChangeRankTitle:
                                alliance.Rank1 = request.UpdateFields.RankTitles[0];
                                alliance.Rank2 = request.UpdateFields.RankTitles[1];
                                alliance.Rank3 = request.UpdateFields.RankTitles[2];
                                alliance.Rank4 = request.UpdateFields.RankTitles[3];
                                alliance.Rank5 = request.UpdateFields.RankTitles[4];
                                break;
                            case AllianceOperation.ChangeNotice:
                                alliance.Notice = request.UpdateFields.Notice;
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
            if (code == 0)
                _server.Transport.BroadcastAllianceUpdate(request.FromChannel, response);

            return response;
        }

        public void SendAllianceChat(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_idGuildDataSource.TryGetValue(sender.Character.GuildId, out var guild))
                {
                    if (_idAllianceDataSource.TryGetValue(guild.AllianceId, out var alliance))
                    {
                        var allianceMembers = alliance.Guilds.Select(GetLocalGuild)
                            .SelectMany(x => GetGuildMembers(x).Where(y => y.Character.Id != sender.Character.Id && y.Channel > 0))
                            .Select(x => new PlayerChannelPair(x.Channel, x.Character.Id)).ToArray();
                        _server.Transport.SendMultiChat(3, nameFrom, allianceMembers, chatText);
                    }
                }

            }
        }

        #endregion
    }
}
