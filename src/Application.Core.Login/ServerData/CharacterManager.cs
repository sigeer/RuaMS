using Application.Core.Login.Commands;
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
    public class CharacterManager : DataStorageBase<int, CharacterLiveObject, CharacterEntity>, IDisposable
    {
        ConcurrentDictionary<string, StoreUnit<CharacterLiveObject>> _nameDataSource = new();


        readonly MasterServer _masterServer;

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer masterServer)
            : base(StorageCategory.Character, dbContextFactory, mapper, logger)
        {
            _masterServer = masterServer;
        }

        protected override int GetKey(CharacterLiveObject model) => model.Character.Id;

        CharacterLiveObject _sysChr = new CharacterLiveObject(new Dto.CharacterDto { Id = ServerConstants.SystemCId, Name = "系统" });
        public CharacterLiveObject? FindPlayerById(int id)
        {
            if (id == ServerConstants.SystemCId)
                return _sysChr;

            if (id <= 0)
                return null;

            return Find(id);
        }


        public CharacterLiveObject? FindPlayerByName(string name)
        {
            if (_nameDataSource.TryGetValue(name, out var data))
            {
                if (data.Flag != StoreFlag.Remove)
                    return data.Data;
                else
                    return null;
            }

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbData = dbContext.Set<CharacterEntity>().Where(x => x.Name.Equals(name)).FirstOrDefault();
            if (dbData != null)
            {
                var localData = MapModel(dbData);
                SetCache(localData);
                return localData;
            }

            return null;
        }

        public void SetState(CharacterLiveObject obj)
        {
            SetDirty(obj);
        }

        protected override void SetDirty(CharacterLiveObject model)
        {
            base.SetDirty(model);
            _nameDataSource[model.Character.Name] = new StoreUnit<CharacterLiveObject>(StoreFlag.AddOrUpdate, model);
        }

        protected override void SetCache(CharacterLiveObject model)
        {
            base.SetCache(model);
            _nameDataSource[model.Character.Name] = new StoreUnit<CharacterLiveObject>(StoreFlag.Cached, model);
        }

        protected override void SetRemoved(CharacterLiveObject model)
        {
            base.SetRemoved(model);
            _nameDataSource[model.Character.Name] = new StoreUnit<CharacterLiveObject>(StoreFlag.Remove, model);
        }

        public List<Dto.CharacterDto> GetAllCachedPlayers()
        {
            return QueryLocal().Select(x => x.Character).ToList();
        }

        protected override CharacterLiveObject MapModel(CharacterEntity entity)
        {
            return new CharacterLiveObject(_mapper.Map<Dto.CharacterDto>(entity));
        }

        protected override CharacterEntity MapEntity(CharacterLiveObject localModel)
        {
            return _mapper.Map<CharacterEntity>(localModel.Character);
        }

        protected override CharacterEntity MapExsitedEntity(CharacterLiveObject localModel, CharacterEntity dbModel)
        {
            return _mapper.Map(localModel.Character, dbModel);
        }

        public string GetPlayerName(int id)
        {
            return FindPlayerById(id)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }

        public async Task Update(SyncProto.PlayerSaveDto obj, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown)
        {
            var origin = FindPlayerById(obj.Character.Id);
            if (origin != null)
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
            var d = FindPlayerById(playerId);
            if (d != null)
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
                throw new BusinessFatalException($"未验证的玩家Id {playerId}。");
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
            _nameDataSource.Clear();

        }


        /// <summary>
        /// 获取用于展示的角色object
        /// </summary>
        /// <param name="charIds"></param>
        /// <returns></returns>
        public List<CharacterLiveObject> GetCharactersView(IEnumerable<int> charIds)
        {
            return Query(x => charIds.Contains(x.Id), x => charIds.Contains(x.Character.Id));
        }

        internal int GetOnlinedPlayerCount()
        {
            return QueryLocal().Where(x => x.ChannelNode != null).Count();
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
            return !dbContext.Characters.Any(x => !_localData.Keys.Contains(x.Id) && x.Name == name);
        }


        //public IDictionary<int, int[]> GetPlayerChannelPair(IEnumerable<CharacterViewObject> players)
        //{
        //    return players.Where(x => x != null).GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.Character.Id).ToArray());
        //}

        internal float GetChannelPlayerCount(int channelId)
        {
            return QueryLocal().Where(x => x.Channel == channelId).Count();
        }

        internal int[] GetOnlinedGMs()
        {
            var accIds = _masterServer.AccountManager.GetOnlinedGmAccId();
            return QueryLocal(x => x.ChannelNode != null && accIds.Contains(x.Character.AccountId)).Select(x => x.Character.Id).ToArray();
        }

        public List<int> GetOnlinedPlayerAccountId()
        {
            return QueryLocal(x => x.ChannelNode != null).Select(x => x.Character.AccountId).ToList();
        }

        public SystemProto.ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            var res = new SystemProto.ShowOnlinePlayerResponse();
            res.List.AddRange(QueryLocal(x => x.ChannelNode != null).Select(x => new SystemProto.OnlinedPlayerInfoDto { Id = x.Character.Id, Channel = x.Channel, MapId = x.Character.Map, Name = x.Character.Name }));
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

        protected override void CommitRemove(DBContext dbContext, CharacterEntity? dbModel, CharacterLiveObject localModel)
        {
            if (dbModel != null)
            {
                dbModel.IsDeleted = true;
            }
        }

        public void InsertNewCharacter(NewCharacterPreview obj)
        {
            obj.Character.Id = Interlocked.Increment(ref _localId);

            SetDirty(obj);

            _masterServer.AccountManager.UpdateAccountCharacterCacheByAdd(obj.Character.AccountId, obj.Character.Id);

            _ = _masterServer.DropYellowTip("[New Char]: " + obj.Account.Name + " has created a new character with IGN " + obj.Character.Name, true);
        }

        public bool RemoveCharacter(int chrId, int checkAccount)
        {
            var chr = FindPlayerById(chrId);
            if (chr != null && chr.Character.AccountId == checkAccount)
            {
                SetRemoved(chr);

                _masterServer.AccountManager.UpdateAccountCharacterCacheByRemove(checkAccount, chrId);
                return true;
            }
            return false;
        }
    }
}
