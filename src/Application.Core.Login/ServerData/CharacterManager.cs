using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.Core.Managers.Constants;
using Application.EF;
using Application.Shared.Constants;
using Application.Shared.Items;
using Application.Shared.Team;
using Application.Utility;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using Dto;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using Serilog;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using tools;
using XmlWzReader;

namespace Application.Core.Login.Datas
{
    /// <summary>
    /// 不包含Account，Account可能会在登录时被单独修改
    /// </summary>
    public class CharacterManager : IDisposable
    {
        ConcurrentDictionary<int, CharacterLiveObject> _idDataSource = new();
        ConcurrentDictionary<string, CharacterLiveObject> _nameDataSource = new();

        ConcurrentDictionary<int, CharacterViewObject> _charcterViewCache = new();

        readonly IMapper _mapper;
        readonly ILogger<CharacterManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly DataStorage _dataStorage;
        readonly MasterServer _masterServer;

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory, DataStorage chrStorage, MasterServer masterServer)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _dataStorage = chrStorage;
            _masterServer = masterServer;
        }
        CharacterLiveObject _sysChr = new CharacterLiveObject() { Character = new CharacterModel { Id = ServerConstants.SystemCId, Name = "系统" } };
        public CharacterLiveObject? FindPlayerById(int id)
        {
            if (id == ServerConstants.SystemCId)
                return _sysChr;

            if (id <= 0)
                return null;

            if (_idDataSource.TryGetValue(id, out var data) && data != null)
                return data;

            data = GetCharacter(id);
            if (data == null)
                return null;
            _idDataSource[id] = data;
            return data;
        }
        public CharacterLiveObject? FindPlayerByName(string name)
        {
            if (_nameDataSource.TryGetValue(name, out var data) && data != null)
                return data;

            data = GetCharacter(null, name);
            if (data == null)
                return null;
            _nameDataSource[name] = data;
            return data;
        }

        public string GetPlayerName(int id)
        {
            return FindPlayerById(id)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }

        public void Update(Dto.PlayerSaveDto obj)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var origin))
            {
                var oldCharacterData = origin.Character;
                origin.Character = _mapper.Map<CharacterModel>(obj.Character);
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

                _masterServer.AccountManager.UpdateAccountGame(_mapper.Map<AccountGame>(obj.AccountGame));

                _logger.LogDebug("玩家{PlayerName}已缓存", obj.Character.Name);
                _dataStorage.SetCharacter(origin);

                if (oldCharacterData.Level != origin.Character.Level)
                {
                    // 等级变化通知
                    _masterServer.Transport.BroadcastPlayerLevelChanged(new Dto.PlayerLevelJobChange
                    {
                        Id = origin.Character.Id,
                        JobId = origin.Character.JobId,
                        Level = origin.Character.Level,
                        Name = origin.Character.Name,
                        GuildId = origin.Character.GuildId,
                        TeamId = origin.Character.Party,
                        FamilyId = origin.Character.FamilyId,
                        MedalItemId = origin.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? -1
                    });
                    _masterServer.TeamManager.UpdateParty(origin.Character.Party, PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);
                }

                if (oldCharacterData.JobId != origin.Character.JobId)
                {
                    // 转职通知
                    _masterServer.Transport.BroadcastPlayerJobChanged(new Dto.PlayerLevelJobChange
                    {
                        Id = origin.Character.Id,
                        JobId = origin.Character.JobId,
                        Level = origin.Character.Level,
                        Name = origin.Character.Name,
                        GuildId = origin.Character.GuildId,
                        TeamId = origin.Character.Party,
                        FamilyId = origin.Character.FamilyId,
                        MedalItemId = origin.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? -1
                    });
                    _masterServer.TeamManager.UpdateParty(origin.Character.Party, PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);
                }

                // 理论上这里只会被退出游戏（0），进入商城/拍卖（-1）触发
                if (origin.Channel != obj.Channel)
                {
                    origin.Channel = obj.Channel;
                    // 离线通知
                    if (obj.Channel == 0)
                    {
                        origin.Character.LastLogoutTime = DateTimeOffset.FromUnixTimeMilliseconds(_masterServer.getCurrentTime());
                        origin.ActualChannel = 0;
                        _masterServer.Transport.BroadcastPlayerLoginOff(new Dto.PlayerOnlineChange
                        {
                            Id = origin.Character.Id,
                            Name = origin.Character.Name,
                            GuildId = origin.Character.GuildId,
                            TeamId = origin.Character.Party,
                            FamilyId = origin.Character.FamilyId,
                            Channel = obj.Channel
                        });

                        foreach (var module in _masterServer.Modules)
                        {
                            module.OnPlayerLogoff(origin);
                        }

                        _masterServer.TeamManager.UpdateParty(origin.Character.Party, PartyOperation.LOG_ONOFF, origin.Character.Id, origin.Character.Id);
                    }
                    else
                    {
                        foreach (var module in _masterServer.Modules)
                        {
                            module.OnPlayerEnterCashShop(origin);
                        }
                    }

                    _masterServer.ChatRoomManager.LeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = origin.Character.Id });

                    _masterServer.BuddyManager.BroadcastNotify(origin);
                }
            }
        }

        public void BatchUpdate(List<Dto.PlayerSaveDto> list)
        {
            foreach (var item in list)
            {
                Update(item);
            }
        }

        internal void CompleteLogin(int playerId, int channel, out int accountId)
        {
            if (_idDataSource.TryGetValue(playerId, out var d))
            {
                d.Channel = channel;
                d.ActualChannel = channel;
                accountId = d.Character.AccountId;

                // 上线通知
                _masterServer.Transport.BroadcastPlayerLoginOff(new Dto.PlayerOnlineChange
                {
                    Id = d.Character.Id,
                    Name = d.Character.Name,
                    GuildId = d.Character.GuildId,
                    TeamId = d.Character.Party,
                    Channel = d.Channel,
                    FamilyId = d.Character.FamilyId,
                    IsNewComer = d.Channel == 0
                });

                _masterServer.TeamManager.UpdateParty(d.Character.Party, PartyOperation.LOG_ONOFF, d.Character.Id, d.Character.Id);

                _masterServer.BuddyManager.BroadcastNotify(d);

                foreach (var module in _masterServer.Modules)
                {
                    module.OnPlayerLogin(d);
                }

                _masterServer.NoteManager.SendNote(d);
            }
            else
            {
                throw new BusinessFatalException($"未验证的玩家Id {playerId}。{nameof(_idDataSource)} 中包含了所有登录过的玩家，而设置频道的玩家必然登录过。");
            }
        }

        public void UpdateMap(int characterId, int mapId)
        {
            var chr = FindPlayerById(characterId);
            if (chr != null)
            {
                chr.Character.Map = mapId;

                foreach (var module in _masterServer.Modules)
                {
                    module.OnPlayerMapChanged(chr);
                }
            }
        }

        public void BatchUpdateMap(List<SyncProto.MapSyncDto> data)
        {
            foreach (var item in data)
            {
                UpdateMap(item.MasterId, item.MapId);
            }
        }

        public void Dispose()
        {
            _idDataSource.Clear();
            _nameDataSource.Clear();
            _charcterViewCache.Clear();
        }

        public CharacterLiveObject? GetCharacter(int? characterId = null, string? characterName = null)
        {
            if (characterId == null && characterName == null)
                return null;

            CharacterLiveObject? d = null;
            if (characterId != null)
                _idDataSource.TryGetValue(characterId.Value, out d);
            if (d == null && characterName != null)
                _nameDataSource.TryGetValue(characterName, out d);

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

                foreach (var module in _masterServer.Modules)
                {
                    module.OnPlayerLoad(dbContext, chrModel);
                }

                var petIgnores = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                                  let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                                  select new PetIgnoreModel { PetId = a.Petid, ExcludedItems = excluded }).ToArray();

                var invItems = _masterServer.InventoryManager.LoadItems(dbContext, false, characterEntity.Id, ItemType.Inventory).ToArray();

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
                var fameRecords = dbContext.Famelogs.AsNoTracking().Where(x => x.Characterid == characterId && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();

                d = new CharacterLiveObject()
                {
                    Character = chrModel,
                    Channel = 0,
                    PetIgnores = petIgnores,
                    Areas = _mapper.Map<AreaModel[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    BuddyList = buddyData.ToDictionary(x => x.Id),
                    FameLogs = _mapper.Map<FameLogModel[]>(fameRecords),
                    Events = _mapper.Map<EventModel[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    InventoryItems = invItems,
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
                };

                _idDataSource[characterEntity.Id] = d;
                _nameDataSource[characterEntity.Name] = d;
                _charcterViewCache[characterEntity.Id] = d;
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
                if (_idDataSource.TryGetValue(item, out var e) && e != null)
                    list.Add(e);
                else if (_charcterViewCache.TryGetValue(item, out var d) && d != null)
                    list.Add(d);
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
                var obj = new CharacterViewObject()
                {
                    Character = _mapper.Map<CharacterModel>(character),
                    InventoryItems = _mapper.Map<ItemModel[]>(items.Where(x => x.Item.Characterid == character.Id))
                };
                _charcterViewCache[obj.Character.Id] = obj;
                list.Add(obj);
            }
            return list;

        }



        public bool DeleteChar(int cid, int senderAccId)
        {
            if (!_masterServer.AccountManager.ValidAccountCharacter(senderAccId, cid))
            {    // thanks zera (EpiphanyMS) for pointing a critical exploit with non-authed character deletion request
                return false;
            }

            int accId = senderAccId;
            int world = 0;
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                using var dbTrans = dbContext.Database.BeginTransaction();

                var characterModel = dbContext.Characters.Where(x => x.Id == cid).FirstOrDefault();
                if (characterModel == null)
                    return false;

                if (characterModel.AccountId != senderAccId)
                    return false;

                world = characterModel.World;

                dbContext.Buddies.Where(x => x.CharacterId == cid).ExecuteDelete();

                // TODO: 退出队伍

                // TODO: 退出家族

                var threadIdList = dbContext.BbsThreads.Where(x => x.Postercid == cid).Select(x => x.Threadid).ToList();
                dbContext.BbsReplies.Where(x => threadIdList.Contains(x.Threadid)).ExecuteDelete();
                dbContext.BbsThreads.Where(x => x.Postercid == cid).ExecuteDelete();


                dbContext.Wishlists.Where(x => x.CharId == cid).ExecuteDelete();
                dbContext.Cooldowns.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Playerdiseases.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.AreaInfos.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Monsterbooks.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Characters.Where(x => x.Id == cid).ExecuteDelete();
                dbContext.FamilyCharacters.Where(x => x.Cid == cid).ExecuteDelete();
                dbContext.Famelogs.Where(x => x.CharacteridTo == cid).ExecuteDelete();

                var inventoryItems = dbContext.Inventoryitems.Where(x => x.Characterid == cid).ToList();
                var inventoryItemIdList = inventoryItems.Select(x => x.Inventoryitemid).ToList();
                var inventoryEquipList = dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).ToList();
                inventoryItems.ForEach(rs =>
                {
                    var ringsList = inventoryEquipList.Where(x => x.Inventoryitemid == rs.Inventoryitemid).Select(x => x.RingId).ToList();
                    ringsList.ForEach(ringid =>
                    {
                        if (ringid > -1)
                        {
                            dbContext.Rings.Where(x => x.Id == ringid).ExecuteDelete();
                        }
                    });

                    dbContext.Pets.Where(x => x.Petid == rs.Petid).ExecuteDelete();
                });
                dbContext.Inventoryitems.RemoveRange(inventoryItems);
                dbContext.Inventoryequipments.RemoveRange(inventoryEquipList);

                dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();

                var mtsCartIdList = dbContext.MtsCarts.Where(x => x.Cid == cid).Select(x => x.Id).ToList();
                dbContext.MtsItems.Where(x => mtsCartIdList.Contains(x.Id)).ExecuteDelete();
                dbContext.MtsCarts.Where(x => x.Cid == cid).ExecuteDelete();

                string[] toDel = { "famelog", "inventoryitems", "keymap", "queststatus", "savedlocations", "trocklocations", "skillmacros", "skills", "eventstats", "server_queue" };
                foreach (string s in toDel)
                {
                    dbContext.Database.ExecuteSqlRaw("DELETE FROM `" + s + "` WHERE characterid = @cid", new MySqlParameter("cid", cid));
                }
                dbContext.SaveChanges();
                dbTrans.Commit();

                _masterServer.AccountManager.UpdateAccountCharacterCacheByRemove(accId, cid);

                _nameDataSource.TryRemove(characterModel.Name, out _);
                _charcterViewCache.TryRemove(cid, out _);
                _idDataSource.Remove(cid, out _);

                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return false;
            }
        }

        internal int GetOnlinedPlayerCount()
        {
            return _idDataSource.Values.Count(x => x.Channel != 0);
        }

        public int CreatePlayerCheck(int accountId, string name)
        {
            var accountInfo = _masterServer.AccountManager.GetAccountDto(accountId);
            if (accountInfo == null)
                return CreateCharResult.CharSlotLimited;

            if (accountInfo.Characterslots - _masterServer.AccountManager.GetAccountPlayerIds(accountId).Count <= 0)
                return CreateCharResult.CharSlotLimited;

            if (!CheckCharacterName(name))
                return CreateCharResult.NameInvalid;

            return CreateCharResult.Success;
        }

        public bool CheckCharacterName(string name)
        {
            // 禁用名
            if (StringConstants.BLOCKED_NAMES.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            var bLength = GlobalVariable.Encoding.GetBytes(name).Length;
            if (bLength < 3 || bLength > 12)
                return false;

            if (!Regex.IsMatch(name, "^[a-zA-Z0-9\\u4e00-\\u9fa5]+$"))
                return false;

            if (_nameDataSource.ContainsKey(name))
                return false;

            using var dbContext = _dbContextFactory.CreateDbContext();
            return !dbContext.Characters.Any(x => !_idDataSource.Keys.Contains(x.Id) && x.Name == name);
        }


        public int CreatePlayer(int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender, int type)
        {
            var checkResult = CreatePlayerCheck(accountId, name);
            if (checkResult != CreateCharResult.Success)
                return checkResult;

            return _masterServer.Transport.CreatePlayer(new Dto.CreateCharRequestDto
            {
                AccountId = accountId,
                Type = type,
                Name = name,
                Face = face,
                Hair = hair,
                SkinColor = skin,
                Top = top,
                Bottom = bottom,
                Shoes = shoes,
                Weapon = weapon,
                Gender = gender
            }).Code;
        }

        public int CreatePlayerDB(Dto.NewPlayerSaveDto data)
        {
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var characterId = _dataStorage.CommitNewPlayer(dbContext, data);
                dbTrans.Commit();

                _masterServer.AccountManager.UpdateAccountCharacterCacheByAdd(data.Character.AccountId, characterId);
                _masterServer.Transport.BroadcastWorldGMPacket(
                    PacketCreator.sendYellowTip("[New Char]: " + data.Character.AccountId + " has created a new character with IGN " + data.Character.Name));
                Log.Logger.Information("Account {AccountName} created chr with name {CharacterName}", data.Character.AccountId, data.Character.Name);
                return characterId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色保存到数据库");
                return CreateCharResult.Error;
            }
        }


        public IDictionary<int, int[]> GetPlayerChannelPair(IEnumerable<CharacterViewObject> players)
        {
            return players.Where(x => x != null).GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.Character.Id).ToArray());
        }

        internal float GetChannelPlayerCount(int channelId)
        {
            return _idDataSource.Values.Count(x => x.Channel == channelId);
        }

        internal List<int> GetOnlinedGMs()
        {
            var accIds = _masterServer.AccountManager.GetOnlinedGmAccId();
            return _idDataSource.Values.Where(x => x.Channel > 0 && accIds.Contains(x.Character.AccountId)).Select(x => x.Character.Id).ToList();
        }

        public List<int> GetOnlinedPlayerAccountId()
        {
            return _idDataSource.Values.Where(x => x.Channel > 0).Select(x => x.Character.AccountId).ToList();
        }

        public ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            var list = _idDataSource.Values.Where(x => x.Channel > 0).ToList();
            var res = new ShowOnlinePlayerResponse();
            res.List.AddRange(list.Select(x => new Dto.OnlinedPlayerInfoDto { Id = x.Character.Id, Channel = x.Channel, MapId = x.Character.Map, Name = x.Character.Name }));
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
    }
}
