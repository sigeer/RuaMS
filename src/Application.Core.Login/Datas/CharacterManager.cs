using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Game.Players;
using Application.Core.Login.Services;
using Application.Core.Managers;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using Serilog;

namespace Application.Core.Login.Datas
{
    /// <summary>
    /// 不包含Account，Account可能会在登录时被单独修改
    /// </summary>
    public class CharacterManager
    {
        Dictionary<int, CharacterValueObject> _idDataSource = new Dictionary<int, CharacterValueObject>();
        Dictionary<string, CharacterValueObject> _nameDataSource = new Dictionary<string, CharacterValueObject>();

        Dictionary<int, CharacterViewObject> _charcterViewCache = new();

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

        public CharacterValueObject? FindPlayerById(int id) => _idDataSource.GetValueOrDefault(id);
        public CharacterValueObject? FindPlayerByName(string name) => _nameDataSource.GetValueOrDefault(name);

        public void Update(PlayerSaveDto obj)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var origin))
            {
                origin.Character = obj.Character;
                origin.CashShop = obj.CashShop;
                origin.BuddyList = obj.BuddyList;
                origin.InventoryItems = obj.InventoryItems;
                origin.KeyMaps = obj.KeyMaps;
                origin.SkillMacros = obj.SkillMacros;
                origin.Skills = obj.Skills;
                origin.Areas = obj.Areas;
                origin.Events = obj.Events;
                origin.MonsterBooks = obj.MonsterBooks;
                origin.PetIgnores = obj.PetIgnores;
                origin.QuestStatuses = obj.QuestStatuses;
                origin.QuickSlot = obj.QuickSlot;
                origin.SavedLocations = obj.SavedLocations;
                origin.StorageInfo = obj.StorageInfo;
                origin.TrockLocations = obj.TrockLocations;
                origin.CoolDowns = obj.CoolDowns;

                origin.Channel = obj.Channel;
            }
            _logger.LogDebug("玩家{PlayerName}已缓存", obj.Character.Name);
            _dataStorage.SetCharacter(obj);
        }

        public bool Remove(int characterId)
        {
            if (_idDataSource.Remove(characterId, out var d))
            {
                _nameDataSource.Remove(d.Character.Name);
                _charcterViewCache.Remove(characterId);
                return true;
            }
            return false;
        }

        private ItemFactory GetCashshopFactory(int jobId)
        {
            if (!YamlConfig.config.server.USE_JOINT_CASHSHOP_INVENTORY)
            {
                switch (JobFactory.GetJobTypeById(jobId))
                {
                    case JobType.Adventurer:
                        return ItemFactory.CASH_EXPLORER;
                    case JobType.Cygnus:
                        return ItemFactory.CASH_CYGNUS;
                    case JobType.Legend:
                        return ItemFactory.CASH_ARAN;
                    default:
                        return ItemFactory.CASH_OVERALL;
                }
            }
            else
            {
                return ItemFactory.CASH_OVERALL;
            }
        }

        public CharacterValueObject? GetCharacter(int characterId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            CharacterValueObject? d;
            if (!_idDataSource.TryGetValue(characterId, out d))
            {
                var characterEntity = dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Id == characterId);
                if (characterEntity == null)
                    return null;

                var petIgnores = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                                  let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                                  select new PetIgnoreDto { PetId = a.Petid, ExcludedItems = excluded }).ToArray();

                var invItems = InventoryManager.LoadItems(dbContext, characterId, ItemFactory.INVENTORY);

                var buddyData = (from a in dbContext.Buddies
                                 join b in dbContext.Characters on a.BuddyId equals b.Id
                                 where a.CharacterId == characterId
                                 select new BuddyDto { CharacterId = a.BuddyId, CharacterName = b.Name, Pending = a.Pending, Group = a.Group }).ToArray();

                #region quest
                var questStatusData = (from a in dbContext.Queststatuses.AsNoTracking().Where(x => x.Characterid == characterId)
                                       let bs = dbContext.Questprogresses.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       let cs = dbContext.Medalmaps.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       select new QuestStatusEntityPair(a, bs, cs)).ToArray();
                #endregion

                d = new CharacterValueObject()
                {
                    Character = _mapper.Map<CharacterDto>(characterEntity),
                    PetIgnores = petIgnores,
                    Areas = _mapper.Map<AreaDto[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    BuddyList = buddyData,
                    Events = _mapper.Map<EventDto[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    InventoryItems = _mapper.Map<ItemDto[]>(invItems),
                    KeyMaps = _mapper.Map<KeyMapDto[]>(dbContext.Keymaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),

                    MonsterBooks = _mapper.Map<MonsterbookDto[]>(dbContext.Monsterbooks.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),

                    QuestStatuses = _mapper.Map<QuestStatusDto[]>(questStatusData),

                    SavedLocations = _mapper.Map<SavedLocationDto[]>(dbContext.Savedlocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    SkillMacros = _mapper.Map<SkillMacroDto[]>(dbContext.Skillmacros.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    Skills = _mapper.Map<SkillDto[]>(dbContext.Skills.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    TrockLocations = _mapper.Map<TrockLocationDto[]>(dbContext.Trocklocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    CoolDowns = _mapper.Map<CoolDownDto[]>(dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArray())
                };

                _idDataSource[characterId] = d;
                _nameDataSource[characterEntity.Name] = d;
                _charcterViewCache[characterId] = d;
            }

            var accountDto = _mapper.Map<AccountDto>(dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == d.Character.AccountId));
            if (accountDto == null)
                return null;
            // 虽然传了所有字段，但是只有部分字段可能会发生修改
            d.Account = accountDto;

            d.Link = dbContext.Characters.Where(x => x.AccountId == d.Character.AccountId && x.Id != d.Character.Id).OrderByDescending(x => x.Level)
                    .Select(x => new CharacterLinkDto() { Level = x.Level, Name = x.Name }).FirstOrDefault();

            d.QuickSlot = _mapper.Map<QuickSlotDto>(dbContext.Quickslotkeymappeds.AsNoTracking().Where(x => x.Accountid == d.Character.AccountId).FirstOrDefault());

            var now = DateTimeOffset.UtcNow;
            var fameRecords = dbContext.Famelogs.AsNoTracking().Where(x => x.Characterid == characterId && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();

            d.FameRecord = new RecentFameRecordDto
            {
                ChararacterIds = fameRecords.Select(x => x.Characterid).ToArray(),
                LastUpdateTime = fameRecords.Count == 0 ? 0 : fameRecords.Max(x => x.When).ToUnixTimeMilliseconds()
            };


            var cashFactory = GetCashshopFactory(d.Character.JobId);
            var allAccountItems = InventoryManager.LoadAccountItems(dbContext, d.Character.AccountId, ItemFactory.STORAGE, cashFactory);

            var storageDto = _mapper.Map<StorageDto>(dbContext.Storages.AsNoTracking().Where(x => x.Accountid == d.Character.AccountId).FirstOrDefault());
            if (storageDto == null)
                storageDto = new StorageDto(d.Character.AccountId);
            storageDto.Items = _mapper.Map<ItemDto[]>(allAccountItems.Where(x => x.Item.Type == ItemFactory.STORAGE.getValue()));
            d.StorageInfo = storageDto;

            var cashShopDto = new CashShopDto
            {
                NxCredit = accountDto.NxCredit ?? 0,
                MaplePoint = accountDto.MaplePoint ?? 0,
                NxPrepaid = accountDto.NxPrepaid ?? 0,
                WishItems = dbContext.Wishlists.Where(x => x.CharId == characterId).Select(x => x.Sn).ToArray(),
                Items = _mapper.Map<ItemDto[]>(allAccountItems.Where(x => x.Item.Type == cashFactory.getValue()))
            };
            d.CashShop = cashShopDto;

            return d;
        }


        /// <summary>
        /// 获取用于展示的角色object
        /// </summary>
        /// <param name="charIds"></param>
        /// <returns></returns>
        public List<CharacterViewObject> GetCharactersView(int[] charIds)
        {
            List<CharacterViewObject> list = new List<CharacterViewObject>();

            List<int> needLoadFromDB = new();
            foreach (var item in charIds)
            {
                if (_charcterViewCache.TryGetValue(item, out var d) && d != null)
                    list.Add(d);
                else if (_idDataSource.TryGetValue(item, out var e) && e != null)
                    list.Add(e);
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
                         where a.Inventorytype == equipedType
                         select new { Item = a }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            var itemObj = (from a in items
                           join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                           from bs in bss.DefaultIfEmpty()
                           select new ItemEntityPair(a.Item, bs, null)).ToList();
            #endregion

            foreach (var character in characters)
            {
                var obj = new CharacterViewObject()
                {
                    Character = _mapper.Map<CharacterDto>(character),
                    InventoryItems = _mapper.Map<ItemDto[]>(itemObj.Where(x => x.Item.Characterid == character.Id))
                };
                _charcterViewCache[obj.Character.Id] = obj;
                list.Add(obj);
            }
            return list;

        }

        internal void SetPlayerChannel(int playerId, int channel, out int accountId)
        {
            if (_idDataSource.TryGetValue(playerId, out var d))
            {
                d.Channel = channel;
                accountId = d.Character.AccountId;
            }
            else
            {
                throw new BusinessFatalException($"未验证的玩家Id {playerId}。{nameof(_idDataSource)} 中包含了所有登录过的玩家，而设置频道的玩家必然登录过。");
            }
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

                var storage = Server.getInstance().getWorld(world).getPlayerStorage();
                var dbBuddyIdList = dbContext.Buddies.Where(x => x.CharacterId == cid).Select(x => x.BuddyId).ToList();
                dbBuddyIdList.ForEach(buddyid =>
                {
                    var buddy = storage.getCharacterById(buddyid);
                    if (buddy != null && buddy.IsOnlined)
                    {
                        buddy.deleteBuddy(cid);
                    }
                });
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
                            CashIdGenerator.freeCashId(ringid);
                        }
                    });

                    dbContext.Pets.Where(x => x.Petid == rs.Petid).ExecuteDelete();
                    CashIdGenerator.freeCashId(rs.Petid);
                });
                dbContext.Inventoryitems.RemoveRange(inventoryItems);
                dbContext.Inventoryequipments.RemoveRange(inventoryEquipList);

                dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();

                FredrickProcessor.removeFredrickLog(dbContext, cid);   // thanks maple006 for pointing out the player's Fredrick items are not being deleted at character deletion

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
                Remove(cid);
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
    }
}
