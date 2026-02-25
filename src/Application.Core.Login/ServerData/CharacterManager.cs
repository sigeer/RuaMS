using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Login.Commands;
using Application.Core.Login.Models;
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
using AutoMapper;
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
    public class CharacterManager : IStorage, IDisposable
    {
        int _localId = 0;

        ConcurrentDictionary<int, IStoreUnit<CharacterViewObject>> _idDataSource = new();
        ConcurrentDictionary<string, IStoreUnit<CharacterViewObject>> _nameDataSource = new();


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
        CharacterLiveObject _sysChr = new CharacterLiveObject(new CharacterModel { Id = ServerConstants.SystemCId, Name = "系统" }, []);
        public CharacterLiveObject? FindPlayerById(int id)
        {
            if (id == ServerConstants.SystemCId)
                return _sysChr;

            if (id <= 0)
                return null;

            if (_idDataSource.TryGetValue(id, out var data) && data.Flag != StoreFlag.Remove)
                return (data.Data as CharacterLiveObject) ?? GetCharacter(id);

            return GetCharacter(id);
        }
        public CharacterLiveObject? FindPlayerByName(string name)
        {
            if (_nameDataSource.TryGetValue(name, out var data) && data.Flag != StoreFlag.Remove)
                return (data.Data as CharacterLiveObject) ?? GetCharacter(null, name);

            return GetCharacter(null, name);
        }

        public void SetState(CharacterLiveObject obj)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var o) && o.Flag != StoreFlag.Remove)
            {
                o.Update();
            }
        }

        public List<CharacterModel> GetAllCachedPlayers()
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

                _mapper.Map(obj.Character, origin.Character);
                origin.InventoryItems = _mapper.Map<ItemModel[]>(obj.InventoryItems);
                origin.KeyMaps = _mapper.Map<KeyMapModel[]>(obj.KeyMaps);
                origin.SkillMacros = _mapper.Map<SkillMacroModel[]>(obj.SkillMacros);
                origin.Skills = _mapper.Map<SkillModel[]>(obj.Skills);
                origin.Areas = _mapper.Map<AreaModel[]>(obj.Areas);
                origin.Events = _mapper.Map<EventModel[]>(obj.Events);
                origin.MonsterBooks = _mapper.Map<MonsterbookModel[]>(obj.MonsterBooks);
                origin.PetIgnores = _mapper.Map<PetIgnoreModel[]>(obj.PetIgnores);
                origin.QuestStatuses = _mapper.Map<QuestStatusModel[]>(obj.QuestStatuses);
                origin.SavedLocations = _mapper.Map<SavedLocationModel[]>(obj.SavedLocations);
                origin.TrockLocations = _mapper.Map<TrockLocationModel[]>(obj.TrockLocations);
                origin.CoolDowns = _mapper.Map<CoolDownModel[]>(obj.CoolDowns);
                origin.FameLogs = _mapper.Map<FameLogModel[]>(obj.FameLogs);
                origin.GachaponStorage = _mapper.Map<StorageModel>(obj.GachaponStorage);

                _masterServer.AccountGameManager.UpdateAccountGame(_mapper.Map<AccountGame>(obj.AccountGame));

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

        public void BatchUpdateOrSave(List<SyncProto.PlayerSaveDto> list, bool saveDB)
        {
            _ = BatchUpdateCore(list)
                    .ContinueWith(t =>
                    {
                        if (saveDB)
                        {
                            _masterServer.Post(new CommitDBCommand());
                        }
                    });
        }

        public void UpdateOrSave(SyncProto.PlayerSaveDto data, SyncCharacterTrigger trigger, bool saveDB)
        {
            _ = Update(data, trigger)
                    .ContinueWith(t =>
                    {
                        if (saveDB)
                        {
                            _masterServer.Post(new CommitDBCommand());
                        }
                    });
        }

        internal async Task<int> CompleteLogin(int playerId, int channel)
        {
            if (_idDataSource.TryGetValue(playerId, out var data) && data is CharacterLiveObject d)
            {
                var lastChannel = d.Channel;
                d.Channel = channel;
                d.ChannelNode = _masterServer.GetChannelServer(channel);

                if (lastChannel == 0)
                {
                    _logger.LogDebug("玩家{PlayerName}已缓存, 操作:{TriggerDetail}", d.Character.Name, $"进入游戏（频道{channel}）");
                }
                else if (lastChannel == -1)
                {
                    _logger.LogDebug("玩家{PlayerName}已缓存, 操作:{TriggerDetail}", d.Character.Name, $"离开商城（频道{channel}）");
                }
                else
                {
                    _logger.LogDebug("玩家{PlayerName}已缓存, 操作:{TriggerDetail}", d.Character.Name, $"切换频道（从频道{lastChannel}到频道{channel}）");
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

        CharacterLiveObject? GetCharacter(int? characterId = null, string? characterName = null)
        {
            if (characterId == null && characterName == null)
                return null;

            CharacterLiveObject? d = null;
            if (characterId != null && _idDataSource.TryGetValue(characterId.Value, out var p1) && p1.Flag != StoreFlag.Remove)
                d = p1.Data as CharacterLiveObject;
            if (d == null && characterName != null && _nameDataSource.TryGetValue(characterName, out var p2) && p2.Flag != StoreFlag.Remove)
                d = p2.Data as CharacterLiveObject;

            if (d == null)
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var characterEntity = characterId != null
                    ? dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Id == characterId)
                    : dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Name == characterName);
                if (characterEntity == null)
                    return null;

                characterId = characterEntity.Id;
                characterName = characterEntity.Name;

                var chrModel = _mapper.Map<CharacterModel>(characterEntity);

                var petIgnores = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                                  let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                                  select new PetIgnoreModel { PetId = a.Petid, ExcludedItems = excluded }).ToArray();

                var invItems = _masterServer.InventoryManager.LoadItems(dbContext, false, characterEntity.Id, ItemType.Inventory).ToArray();

                var gachponStore = _mapper.Map<StorageModel>(dbContext.Storages.FirstOrDefault(x => x.OwnerId == characterId && x.Type == (int)StorageType.GachaponRewardStorage))
                    ?? new StorageModel(characterId.Value, (int)StorageType.GachaponRewardStorage);
                gachponStore.Items = _masterServer.InventoryManager.LoadItems(dbContext, false, characterEntity.Id, ItemType.ExtraStorage_Gachapon).ToArray();

                var buddyData = (from a in dbContext.Buddies
                                 where a.CharacterId == characterId
                                 select new BuddyModel { Id = a.BuddyId, Group = a.Group, CharacterId = a.CharacterId }).ToArray();

                #region quest
                var questStatusData = (from a in dbContext.Queststatuses.AsNoTracking().Where(x => x.Characterid == characterId)
                                       let bs = dbContext.Questprogresses.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       let cs = dbContext.Medalmaps.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       select new QuestStatusEntityPair(a, bs, cs)).ToArray();
                #endregion

                var now = _masterServer.GetCurrentTimeDateTimeOffset();
                var before30Days = now.AddDays(-30);
                var fameRecords = dbContext.Famelogs.AsNoTracking().Where(x => x.Characterid == characterId && x.When >= before30Days).ToList();

                d = new CharacterLiveObject(chrModel, invItems)
                {
                    Channel = 0,
                    PetIgnores = petIgnores,
                    Areas = _mapper.Map<AreaModel[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    BuddyList = buddyData.ToDictionary(x => x.Id),
                    FameLogs = _mapper.Map<FameLogModel[]>(fameRecords),
                    Events = _mapper.Map<EventModel[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    KeyMaps = _mapper.Map<KeyMapModel[]>(dbContext.Keymaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),

                    MonsterBooks = _mapper.Map<MonsterbookModel[]>(dbContext.Monsterbooks.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),

                    QuestStatuses = _mapper.Map<QuestStatusModel[]>(questStatusData),

                    SavedLocations = _mapper.Map<SavedLocationModel[]>(dbContext.Savedlocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    SkillMacros = _mapper.Map<SkillMacroModel[]>(dbContext.Skillmacros.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    Skills = _mapper.Map<SkillModel[]>(dbContext.Skills.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    TrockLocations = _mapper.Map<TrockLocationModel[]>(dbContext.Trocklocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    CoolDowns = _mapper.Map<CoolDownModel[]>(dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    WishItems = dbContext.Wishlists.Where(x => x.CharId == characterId).Select(x => x.Sn).ToArray(),
                    NewYearCards = _masterServer.NewYearCardManager.LoadPlayerNewYearCard(characterId!.Value).ToArray(),
                    GachaponStorage = gachponStore
                };

                var data = new StoreUnit<CharacterLiveObject>(StoreFlag.Cached, d);
                _idDataSource[characterEntity.Id] = data;
                _nameDataSource[characterEntity.Name] = data;
            }
            return d;
        }


        /// <summary>
        /// 获取用于展示的角色object
        /// </summary>
        /// <param name="charIds"></param>
        /// <returns></returns>
        public List<CharacterViewObject> GetCharactersView(IEnumerable<int> charIds)
        {
            List<CharacterViewObject> list = new List<CharacterViewObject>();

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

            #region 仅需要加载装备栏
            var equipedType = InventoryType.EQUIPPED.getType();
            var items = (from a in dbContext.Inventoryitems.AsNoTracking().Where(x => x.Characterid != null && needLoadFromDB.Contains(x.Characterid.Value))
                         join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                         from bs in bss.DefaultIfEmpty()
                         where a.Inventorytype == equipedType
                         select new ItemEntityPair(a, bs, null)).ToList();
            #endregion

            foreach (var character in characters)
            {
                var obj = new CharacterViewObject(_mapper.Map<CharacterModel>(character), _mapper.Map<ItemModel[]>(items.Where(x => x.Item.Characterid == character.Id)));

                var data = new StoreUnit<CharacterViewObject>(StoreFlag.Cached, obj);
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

                await dbContext.Monsterbooks.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Keymaps.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skills.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skillmacros.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Savedlocations.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Trocklocations.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Buddies.Where(x => updateData.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();
                await dbContext.AreaInfos.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Eventstats.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Cooldowns.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();

                await dbContext.Questprogresses.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Queststatuses.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Medalmaps.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Famelogs.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Storages.Where(x => updateData.Keys.Contains(x.OwnerId) && x.Type == (int)StorageType.GachaponRewardStorage).ExecuteDeleteAsync();
                await dbContext.Petignores.Where(x => updateData.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();

                foreach (var item in updateData)
                {
                    if (item.Value.Flag == StoreFlag.Remove)
                    {
                        await dbContext.Characters.Where(x => x.Id == item.Key)
                            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
                        continue;
                    }

                    var data = item.Value.Data;
                    if (data == null)
                    {
                        _logger.LogWarning("发现了更新项，但是没有记录 CharacterId={CharacterId}", item.Key);
                        continue;
                    }

                    if (data is CharacterLiveObject obj)
                    {
                        item.Value.Flag = StoreFlag.Cached;

                        await InventoryManager.CommitInventoryByTypeAsync(dbContext, obj.Character.Id, obj.InventoryItems, ItemFactory.INVENTORY);

                        var character = updateCharacters.FirstOrDefault(x => x.Id == obj.Character.Id);
                        if (character == null)
                        {
                            character = _mapper.Map<CharacterEntity>(obj.Character);
                            dbContext.Characters.Add(character);
                        }
                        else
                        {
                            _mapper.Map(obj.Character, character);
                        }

                        await dbContext.Monsterbooks.AddRangeAsync(obj.MonsterBooks.Select(x => new MonsterbookEntity(obj.Character.Id, x.Cardid, x.Level)));

                        await dbContext.Petignores.AddRangeAsync(obj.PetIgnores.SelectMany(x => x.ExcludedItems.Select(y => new PetIgnoreEntity(x.PetId, y, obj.Character.Id))));

                        await dbContext.Keymaps.AddRangeAsync(obj.KeyMaps.Select(x => new KeyMapEntity(obj.Character.Id, x.Key, x.Type, x.Action)));

                        await dbContext.Skillmacros.AddRangeAsync(
                            obj.SkillMacros.Where(x => x != null).Select(x => new SkillMacroEntity(obj.Character.Id, (sbyte)x.Position, x.Skill1, x.Skill2, x.Skill3, x.Name, (sbyte)x.Shout)));

                        dbContext.Storages.Add(new StorageEntity(obj.Character.Id, (int)StorageType.GachaponRewardStorage, obj.GachaponStorage.Slots, obj.GachaponStorage.Meso));
                        await InventoryManager.CommitInventoryByTypeAsync(dbContext, obj.Character.Id, obj.GachaponStorage.Items, ItemFactory.ExtraStorage_Gachapon);

                        await dbContext.Cooldowns.AddRangeAsync(
                            obj.CoolDowns.Select(x => new CooldownEntity(obj.Character.Id, x.SkillId, x.Length, x.StartTime)));

                        // Skill
                        await dbContext.Skills.AddRangeAsync(
                            obj.Skills.Select(x => new SkillEntity(x.Skillid, obj.Character.Id, x.Skilllevel, x.Masterlevel, x.Expiration))
                            );

                        await dbContext.Savedlocations.AddRangeAsync(obj.SavedLocations.Select(x => new SavedLocationEntity(x.Map, x.Portal, obj.Character.Id, x.Locationtype)));
                        await dbContext.Trocklocations.AddRangeAsync(obj.TrockLocations.Select(x => new Trocklocation(obj.Character.Id, x.Mapid, x.Vip)));
                        await dbContext.Buddies.AddRangeAsync(obj.BuddyList.Values.Select(x => new BuddyEntity(obj.Character.Id, x.Id, 0, x.Group)));
                        await dbContext.AreaInfos.AddRangeAsync(obj.Areas.Select(x => new AreaInfo(obj.Character.Id, x.Area, x.Info)));
                        await dbContext.Eventstats.AddRangeAsync(obj.Events.Select(x => new Eventstat(obj.Character.Id, x.Name, x.Info)));

                        await dbContext.Famelogs.AddRangeAsync(obj.FameLogs.Where(x => now - x.Time < monthDuration)
                            .Select(x => new FamelogEntity(obj.Character.Id, x.ToId, DateTimeOffset.FromUnixTimeMilliseconds(x.Time))));

                        foreach (var q in obj.QuestStatuses)
                        {
                            var d = new QuestStatusEntity(obj.Character.Id, q.QuestId, q.Status, q.Time, q.Expires, q.Forfeited, q.Completed);
                            await dbContext.Queststatuses.AddAsync(d);
                            await dbContext.SaveChangesAsync();

                            foreach (var p in q.Progress)
                            {
                                await dbContext.Questprogresses.AddAsync(new Questprogress(obj.Character.Id, d.Queststatusid, p.ProgressId, p.Progress));
                            }
                            foreach (var medalMap in q.MedalMap)
                            {
                                await dbContext.Medalmaps.AddRangeAsync(new Medalmap(obj.Character.Id, d.Queststatusid, medalMap.MapId));
                            }
                        }
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

                _masterServer.AccountManager.UpdateAccountCharacterCacheByRemove(model.Data.Character.AccountId, chrId);
                return true;
            }
            return false;
        }
    }
}
