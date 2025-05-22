using Application.Core.Datas;
using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Constants;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Utility.Configs;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.EntityFrameworkCore.Extensions;

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
        readonly AccountManager _accManager;
        readonly DataStorage _dataStorage;

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory, AccountManager accountManager, DataStorage chrStorage)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _accManager = accountManager;
            _dataStorage = chrStorage;
        }

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
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="targetId"></param>
        /// <param name="isAccount"></param>
        /// <param name="itemType">需要满足IsAccount == isAccount</param>
        /// <returns></returns>
        private List<ItemEntityPair> LoadItems(DBContext dbContext, int characterId, params ItemFactory[] itemFactories)
        {
            var itemType = itemFactories.Select(x => x.getValue()).ToArray();
            var items = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => x.Characterid == characterId)
                .Where(x => itemType.Contains(x.Type))
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            return (from a in items
                    join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                    from bs in bss.DefaultIfEmpty()
                    select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();
        }

        private List<ItemEntityPair> LoadAccountItems(DBContext dbContext, int accountId, params ItemFactory[] itemFactories)
        {
            var itemType = itemFactories.Select(x => x.getValue()).ToArray();
            var items = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => x.Accountid == accountId)
                .Where(x => itemType.Contains(x.Type))
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            return (from a in items
                    join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                    from bs in bss.DefaultIfEmpty()
                    select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();
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

                var invItems = LoadItems(dbContext, characterId, ItemFactory.INVENTORY);

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

            var accountDto = _accManager.GetAccountDto(d.Character.AccountId);
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
            var allAccountItems = LoadAccountItems(dbContext, d.Character.AccountId, ItemFactory.STORAGE, cashFactory);

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

            using var dbContext = _dbContextFactory.CreateDbContext();
            var characters = dbContext.Characters.Where(x => needLoadFromDB.Contains(x.Id)).ToList();

            #region 所有道具加载
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
    }
}
