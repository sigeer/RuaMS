using Application.Core.Login.Commands;
using Application.Core.Login.Mappers;
using Application.Core.Login.Models;
using Application.Core.Login.ServerData;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants;
using Application.Shared.Events;
using Application.Shared.Items;
using Application.Shared.Login;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Dto;
using JailProto;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ZLinq;

namespace Application.Core.Login.Datas
{
    /// <summary>
    /// 不包含Account，Account可能会在登录时被单独修改
    /// </summary>
    public class CharacterManager : IStorage, IDisposable
    {
        int _localId = 0;

        ConcurrentDictionary<int, IStoreUnit<CharacterLiveObject>> _idDataSource = new();
        ConcurrentDictionary<string, IStoreUnit<CharacterLiveObject>> _nameDataSource = new();


        readonly IMapper _mapper;
        readonly ILogger<CharacterManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _masterServer;

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer masterServer)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _masterServer = masterServer;
        }
        CharacterLiveObject _sysChr = new CharacterLiveObject(new Dto.CharacterDto { Id = ServerConstants.SystemCId, Name = "系统" });
        public CharacterLiveObject? FindPlayerById(int id)
        {
            if (id == ServerConstants.SystemCId)
                return _sysChr;

            if (id <= 0)
                return null;

            if (_idDataSource.TryGetValue(id, out var data))
            {
                if (data.Flag == StoreFlag.Remove)
                {
                    return null;
                }
                else
                {
                    return (data.Data as CharacterLiveObject) ?? GetCharacterFromDB(id);
                }
            }

            return GetCharacterFromDB(id);
        }
        public CharacterLiveObject? FindPlayerByName(string name)
        {
            if (_nameDataSource.TryGetValue(name, out var data))
            {
                if (data.Flag == StoreFlag.Remove)
                {
                    return null;
                }
                else
                {
                    return (data.Data as CharacterLiveObject) ?? GetCharacterFromDB(null, name);
                }
            }

            return GetCharacterFromDB(null, name);
        }

        public void SetState(CharacterLiveObject obj)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var o) && o.Flag != StoreFlag.Remove)
            {
                o.Update();
            }
        }

        public List<Dto.CharacterDto> GetAllCachedPlayers()
        {
            return _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Select(x => x.Data!.Character).ToList();
        }

        public string GetPlayerName(int id)
        {
            return FindPlayerById(id)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }

        public async Task Update(SyncProto.PlayerSaveDto obj, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var o) && o.Flag != StoreFlag.Remove && o.Data is CharacterLiveObject origin)
            {
                var oldMap = origin.Character.Map;
                var oldLevel = origin.Character.Level;
                var oldJob = origin.Character.JobId;

                origin.Character = obj.Character;

                _masterServer.AccountGameManager.UpdateAccountGame(obj.AccountGame);

                _logger.LogDebug("玩家{PlayerName}已缓存, 操作:{TriggerDetail}",
                    obj.Character.Name, GetTriggerDetail(trigger, origin.Channel, obj.Channel));
                if (trigger == SyncCharacterTrigger.Logoff)
                {
                    _masterServer.AccountManager.UpdateAccountState(obj.Character.AccountId, LoginStage.LOGIN_NOTLOGGEDIN);
                }
                else if (trigger == SyncCharacterTrigger.PreEnterChannel)
                {
                    _masterServer.AccountManager.UpdateAccountState(obj.Character.AccountId, LoginStage.PlayerServerTransition);
                    if (YamlConfig.config.server.USE_IP_VALIDATION)
                    {
                        var accInfo = _masterServer.AccountManager.GetAccountDto(obj.Character.AccountId)!;
                        _masterServer.SetCharacteridInTransition(accInfo.GetSessionRemoteHost(), obj.Character.Id);
                    }
                }
                SetState(origin);

                if (oldLevel != origin.Character.Level)
                {
                    // 等级变化通知
                    foreach (var module in _masterServer.Modules)
                    {
                        await module.OnPlayerLevelChanged(origin);
                    }
                }

                if (oldJob != origin.Character.JobId)
                {
                    // 转职通知
                    foreach (var module in _masterServer.Modules)
                    {
                        await module.OnPlayerJobChanged(origin);
                    }

                }

                if (oldMap != origin.Character.Map)
                {
                    // 地图切换
                    foreach (var module in _masterServer.Modules)
                    {
                        await module.OnPlayerMapChanged(origin);
                    }
                }

                // 理论上这里只会被退出游戏（0），进入商城/拍卖（-1）触发
                if (origin.Channel != obj.Channel)
                {
                    var lastChannel = origin.Channel;
                    origin.Channel = obj.Channel;
                    foreach (var module in _masterServer.Modules)
                    {
                        await module.OnPlayerServerChanged(origin, lastChannel);
                    }


                }
            }
        }

        static string GetTriggerDetail(SyncCharacterTrigger trigger, int oldChannel, int newChannel)
        {
            switch (trigger)
            {
                case SyncCharacterTrigger.Logoff:
                    return "离线";
                case SyncCharacterTrigger.EnterCashShop:
                    return $"进入商城（从频道{oldChannel}）";
                case SyncCharacterTrigger.PreEnterChannel:
                    return $"正在进入频道";
                case SyncCharacterTrigger.LevelChanged:
                    return "等级变化";
                case SyncCharacterTrigger.JobChanged:
                    return "职业变化";
                case SyncCharacterTrigger.Auto:
                    return "自动";
                case SyncCharacterTrigger.System:
                    return "系统";
                default:
                    return "未知";
            }
        }

        async Task BatchUpdateCore(List<SyncProto.PlayerSaveDto> list)
        {
            foreach (var item in list)
            {
                await Update(item, SyncCharacterTrigger.System);
            }
        }

        public async Task BatchUpdateOrSave(List<SyncProto.PlayerSaveDto> list, bool saveDB)
        {
            await BatchUpdateCore(list);
            if (saveDB)
            {
                await _masterServer.Send(new CommitDBCommand());
            }
        }

        public async Task UpdateOrSave(SyncProto.PlayerSaveDto data, SyncCharacterTrigger trigger, bool saveDB)
        {
            await Update(data, trigger);
            if (saveDB)
            {
                await _masterServer.Send(new CommitDBCommand());
            }
        }

        public void FlushCharacter(CharacterLiveObject o)
        {
            List<BuddyProto.BuddyDto> chrBuddies = [];
            var allMembers = o.Character.Data.BuddyList;
            foreach (var m in o.Character.Data.BuddyList)
            {
                var chr = _masterServer.CharacterManager.FindPlayerById(m.Id);
                if (chr != null)
                {
                    chrBuddies.Add(BuddyManager.GetChrBuddyDto(o.Character.Id, chr, m.Group));
                }
            }
            o.Character.Data.BuddyList.Clear();
            o.Character.Data.BuddyList.AddRange(chrBuddies);

            var day30 = _masterServer.GetCurrentTimeDateTimeOffset().AddDays(-30).ToUnixTimeMilliseconds();
            var fameDataIn30Days = o.Character.Data.FameLogs.Where(x => x.Time > day30);
            o.Character.Data.FameLogs.Clear();
            o.Character.Data.FameLogs.AddRange(fameDataIn30Days);
        }

        internal async Task<int> CompleteLogin(int playerId, int channel)
        {
            if (_idDataSource.TryGetValue(playerId, out var data) && data.Flag != StoreFlag.Remove && data.Data is CharacterLiveObject d)
            {
                var lastChannel = d.Channel;
                d.Channel = channel;
                d.ChannelNode = _masterServer.GetChannelServer(channel);

                if (lastChannel == 0)
                {
                    _logger.LogDebug("玩家{PlayerName} {TriggerDetail}", d.Character.Name, $"进入游戏（频道{channel}）");
                }
                else if (lastChannel == -1)
                {
                    _logger.LogDebug("玩家{PlayerName} {TriggerDetail}", d.Character.Name, $"离开商城（频道{channel}）");
                }
                else
                {
                    _logger.LogDebug("玩家{PlayerName} {TriggerDetail}", d.Character.Name, $"切换频道（从频道{lastChannel}到频道{channel}）");
                }


                foreach (var module in _masterServer.Modules)
                {
                    await module.OnPlayerServerChanged(d, lastChannel);
                }

                return d.Character.AccountId;
            }
            else
            {
                throw new BusinessFatalException($"未验证的玩家Id {playerId}。{nameof(_idDataSource)} 中包含了所有登录过的玩家，而设置频道的玩家必然登录过。");
            }
        }

        public async Task UpdateMap(int characterId, int mapId)
        {
            var chr = FindPlayerById(characterId);
            if (chr != null)
            {
                chr.Character.Map = mapId;

                foreach (var module in _masterServer.Modules)
                {
                    await module.OnPlayerMapChanged(chr);
                }
            }
        }

        public async Task BatchUpdateMap(List<SyncProto.MapSyncDto> data)
        {
            foreach (var item in data)
            {
                await UpdateMap(item.MasterId, item.MapId);
            }
        }

        public void Dispose()
        {
            _idDataSource.Clear();
            _nameDataSource.Clear();

        }

        CharacterLiveObject? GetCharacterFromDB(int? characterId = null, string? characterName = null)
        {
            if (characterId == null && characterName == null)
                return null;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var characterEntity = characterId != null
                ? dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Id == characterId)
                : dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Name == characterName);
            if (characterEntity == null)
                return null;

            characterId = characterEntity.Id;
            characterName = characterEntity.Name;

            var chrModel = _mapper.Map<Dto.CharacterDto>(characterEntity);
            var d = new CharacterLiveObject(chrModel)
            {
                Channel = 0,
            };

            var data = new StoreUnit<CharacterLiveObject>(StoreFlag.Cached, d);
            _idDataSource[characterEntity.Id] = data;
            _nameDataSource[characterEntity.Name] = data;

            return d;
        }

        /// <summary>
        /// 获取用于展示的角色object
        /// </summary>
        /// <param name="charIds"></param>
        /// <returns></returns>
        public List<CharacterLiveObject> GetCharactersView(IEnumerable<int> charIds)
        {
            List<CharacterLiveObject> list = new List<CharacterLiveObject>();

            List<int> needLoadFromDB = new();
            foreach (var item in charIds)
            {
                if (_idDataSource.TryGetValue(item, out var e) && e.Flag != StoreFlag.Remove)
                    list.Add(e.Data!);
                else
                    needLoadFromDB.Add(item);
            }

            if (needLoadFromDB.Count == 0)
                return list;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var characters = dbContext.Characters.Where(x => needLoadFromDB.Contains(x.Id)).ToList();

            foreach (var character in characters)
            {
                var chrDto = _mapper.Map<Dto.CharacterDto>(character);
                var obj = new CharacterLiveObject(chrDto);

                var data = new StoreUnit<CharacterLiveObject>(StoreFlag.Cached, obj);
                _idDataSource[obj.Character.Id] = data;
                _nameDataSource[obj.Character.Name] = data;
                list.Add(obj);
            }
            return list;

        }



        internal int GetOnlinedPlayerCount()
        {
            return _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Count(x => x.Data is CharacterLiveObject o && o.Channel != 0);
        }

        public bool CheckCharacterName(string name)
        {
            // 禁用名
            if (StringConstants.BLOCKED_NAMES.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            var bLength = GlobalVariable.Encoding.GetByteCount(name);
            if (bLength < 3 || bLength > 12)
                return false;

            if (!Regex.IsMatch(name, "^[a-zA-Z0-9\\u4e00-\\u9fa5]+$"))
                return false;

            if (_nameDataSource.ContainsKey(name))
                return false;

            using var dbContext = _dbContextFactory.CreateDbContext();
            return !dbContext.Characters.Any(x => !_idDataSource.Keys.Contains(x.Id) && x.Name == name);
        }


        //public IDictionary<int, int[]> GetPlayerChannelPair(IEnumerable<CharacterViewObject> players)
        //{
        //    return players.Where(x => x != null).GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.Character.Id).ToArray());
        //}

        internal float GetChannelPlayerCount(int channelId)
        {
            return _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Count(x => x.Data is CharacterLiveObject o && o.Channel == channelId);
        }

        internal int[] GetOnlinedGMs()
        {
            var accIds = _masterServer.AccountManager.GetOnlinedGmAccId();
            return _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Where(x => x.Data is CharacterLiveObject o && o.Channel > 0 && accIds.Contains(o.Character.AccountId))
                .Select(x => x.Data!.Character.Id).ToArray();
        }

        public List<int> GetOnlinedPlayerAccountId()
        {
            return _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Where(x => x.Data is CharacterLiveObject o && o.Channel > 0)
                .Select(x => x.Data!.Character.AccountId).ToList();
        }

        public SystemProto.ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            var list = _idDataSource.Values.AsValueEnumerable()
                .Where(x => x.Flag != StoreFlag.Remove)
                .Select(x => x.Data)
                .OfType<CharacterLiveObject>()
                .Where(x => x.Channel > 0).ToList();
            var res = new SystemProto.ShowOnlinePlayerResponse();
            res.List.AddRange(list.Select(x => new SystemProto.OnlinedPlayerInfoDto { Id = x.Character.Id, Channel = x.Channel, MapId = x.Character.Map, Name = x.Character.Name }));
            return res;
        }

        public Dto.NameChangeResponse ChangeName(Dto.NameChangeRequest request)
        {
            if (!_masterServer.CharacterManager.CheckCharacterName(request.NewName))
            {
                return new NameChangeResponse() { Code = (int)ChangeNameResponseCode.InvalidName };
            }

            var chr = FindPlayerById(request.MasterId);
            if (chr == null)
            {
                return new NameChangeResponse() { Code = (int)ChangeNameResponseCode.CharacterNotFound };
            }

            if (chr.Character.Level < 10)
            {
                return new NameChangeResponse() { Code = (int)ChangeNameResponseCode.Level };
            }

            if (chr != null)
            {
                chr.Character.Name = request.NewName;
            }

            return new NameChangeResponse();
        }

        public async Task JailPlayer(CreateJailRequest request)
        {
            var res = new CreateJailResponse { Request = request };
            var targetChr = FindPlayerByName(request.TargetName);
            if (targetChr == null)
            {
                res.Code = 1;
                await _masterServer.Transport.SendMessageN(Application.Shared.Message.ChannelRecvCode.Jail, res, [request.MasterId]);
                return;
            }

            if (targetChr.Character.Jailexpire < _masterServer.getCurrentTime())
            {
                targetChr.Character.Jailexpire = _masterServer.getCurrentTime() + request.Minutes * 60000;
            }
            else
            {
                targetChr.Character.Jailexpire += request.Minutes * 60000;
                res.IsExtend = true;
            }
            SetState(targetChr);

            res.TargetId = targetChr.Character.Id;
            await _masterServer.Transport.SendMessageN(Application.Shared.Message.ChannelRecvCode.Jail, res, [request.MasterId, res.TargetId]);
        }

        public async Task UnjailPlayer(CreateUnjailRequest request)
        {
            var res = new CreateUnjailResponse { Request = request };
            var targetChr = FindPlayerByName(request.TargetName);
            if (targetChr == null)
            {
                res.Code = 1;
                await _masterServer.Transport.SendMessageN(Application.Shared.Message.ChannelRecvCode.Unjail, res, [request.MasterId]);
                return;
            }

            if (targetChr.Character.Jailexpire < _masterServer.getCurrentTime())
            {
                res.Code = 2;
                await _masterServer.Transport.SendMessageN(Application.Shared.Message.ChannelRecvCode.Unjail, res, [request.MasterId]);
                return;
            }
            targetChr.Character.Jailexpire = 0;
            SetState(targetChr);

            res.TargetId = targetChr.Character.Id;
            await _masterServer.Transport.SendMessageN(Application.Shared.Message.ChannelRecvCode.Unjail, res, [request.MasterId, res.TargetId]);
        }

        public async Task InitializeAsync(DBContext dbContext)
        {
            _localId = (await dbContext.Characters.IgnoreQueryFilters().MaxAsync(x => (int?)x.Id) ?? 0);
        }

        public async Task Commit(DBContext dbContext)
        {
            var updateData = _idDataSource.Where(x => x.Value.Flag != StoreFlag.Cached).ToDictionary();
            if (updateData.Count == 0)
                return;

            var now = _masterServer.getCurrentTime();

            var monthDuration = (long)TimeSpan.FromDays(30).TotalMilliseconds;
            _logger.LogInformation("正在保存用户数据...");

            try
            {
                var updateCharacters = await dbContext.Characters.Where(x => updateData.Keys.Contains(x.Id)).ToListAsync();

                foreach (var item in updateData)
                {
                    var dbModel = updateCharacters.FirstOrDefault(x => x.Id == item.Key);

                    if (item.Value.Flag == StoreFlag.Remove)
                    {
                        _idDataSource.TryRemove(item.Key, out _);

                        if (dbModel != null)
                            dbModel.IsDeleted = true;
                        else

                        continue;
                    }

                    var obj = item.Value.Data;
                    if (obj == null)
                    {
                        _logger.LogWarning("发现了更新项，但是没有记录 CharacterId={CharacterId}", item.Key);
                        continue;
                    }
                    item.Value.Flag = StoreFlag.Cached;

                    if (dbModel == null)
                    {
                        dbModel = _mapper.Map<CharacterEntity>(obj.Character);
                        dbContext.Characters.Add(dbModel);
                    }
                    else
                    {
                        _mapper.Map(obj.Character, dbModel);
                    }

                    // family
                }
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("保存了{Count}个用户数据", updateData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存用户数据{Status}", "失败");
            }
        }

        public void InsertNewCharacter(NewCharacterPreview obj)
        {
            obj.Character.Id = Interlocked.Increment(ref _localId);
            var data = new StoreUnit<NewCharacterPreview>(StoreFlag.AddOrUpdate, obj);
            _idDataSource[obj.Character.Id] = data;
            _nameDataSource[obj.Character.Name] = data;

            _masterServer.AccountManager.UpdateAccountCharacterCacheByAdd(obj.Character.AccountId, obj.Character.Id);

            _ = _masterServer.DropYellowTip("[New Char]: " + obj.Account.Name + " has created a new character with IGN " + obj.Character.Name, true);
        }

        public bool RemoveCharacter(int chrId, int checkAccount)
        {
            if (_idDataSource.TryGetValue(chrId, out var model)
                && model.Flag != StoreFlag.Remove
                && model.Data!.Character.AccountId == checkAccount)
            {
                model.Remove();

                _masterServer.AccountManager.UpdateAccountCharacterCacheByRemove(checkAccount, chrId);
                return true;
            }
            return false;
        }
    }
}
