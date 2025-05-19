using Application.Core.Datas;
using Application.Core.EF.Entities.Items;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.EntityFrameworkCore.Extensions;
using System.Threading.Tasks;

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

        Dictionary<int, CharacterValueObject> _needUpdate = new Dictionary<int, CharacterValueObject>();

        readonly IMapper _mapper;
        readonly ILogger<CharacterManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly AccountManager _accManager;

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory, AccountManager accountManager)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _accManager = accountManager;
        }

        public void Update(CharacterValueObject obj)
        {
            _idDataSource[obj.Character.Id] = obj;
            _nameDataSource[obj.Character.Name] = obj;
            _charcterViewCache[obj.Character.Id] = obj;

            _idDataSource[obj.Character.Id] = obj;
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

        public CharacterValueObject? GetCharacter(int characterId, bool ignoreCache = false)
        {
            if (!ignoreCache && _idDataSource.TryGetValue(characterId, out var d))
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var characterEntity = dbContext.Characters.FirstOrDefault(x => x.Id == characterId);
            if (characterEntity == null)
                return null;

            var characterLink = dbContext.Characters.Where(x => x.AccountId == characterEntity.AccountId && x.Id != characterEntity.Id).OrderByDescending(x => x.Level)
                .Select(x => new CharacterLinkDto() { Level = x.Level, Name = x.Name }).FirstOrDefault();

            var now = DateTimeOffset.Now;
            var fameRecords = dbContext.Famelogs.AsNoTracking().Where(x => x.Characterid == characterId && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();

            var petIgnores = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                              let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                              select new PetIgnoreDto { PetId = a.Petid, ExcludedItems = excluded }).ToArray();


            #region 所有道具加载
            var items = (from a in dbContext.Inventoryitems.AsNoTracking().Where(x => x.Characterid == characterId || (x.Characterid == null && x.Accountid == characterEntity.AccountId))
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            var itemObj = (from a in items
                           join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                           from bs in bss.DefaultIfEmpty()
                           select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();

            List<ItemEntityPair> invItems = [];
            List<ItemEntityPair> storageItems = [];
            List<ItemEntityPair> merchantItems = [];
            List<ItemEntityPair> cashItems = [];
            List<ItemEntityPair> dueyItems = [];
            List<ItemEntityPair> marriageItems = [];
            foreach (var item in itemObj)
            {
                if (item.Item.Type == ItemFactory.INVENTORY.getValue())
                    invItems.Add(item);
                else if (item.Item.Type == ItemFactory.STORAGE.getValue())
                    storageItems.Add(item);
                else if (item.Item.Type == ItemFactory.MERCHANT.getValue())
                    merchantItems.Add(item);
                else if (item.Item.Type == ItemFactory.DUEY.getValue())
                    dueyItems.Add(item);
                else if (item.Item.Type == ItemFactory.MARRIAGE_GIFTS.getValue())
                    marriageItems.Add(item);
                else if (item.Item.Type == ItemFactory.CASH_OVERALL.getValue()
                    || item.Item.Type == ItemFactory.CASH_ARAN.getValue()
                    || item.Item.Type == ItemFactory.CASH_CYGNUS.getValue()
                    || item.Item.Type == ItemFactory.CASH_EXPLORER.getValue())
                    cashItems.Add(item);
            }

            var storageDto = _mapper.Map<StorageDto>(dbContext.Storages.AsNoTracking().Where(x => x.Accountid == characterEntity.AccountId).FirstOrDefault());
            if (storageDto == null)
                storageDto = new StorageDto(characterEntity.AccountId);
            storageDto.Items = _mapper.Map<ItemDto[]>(storageItems);
            #endregion

            var buddyData = (from a in dbContext.Buddies
                             join b in dbContext.Characters on a.BuddyId equals b.Id
                             where a.CharacterId == characterId
                             select new BuddyDto { CharacterId = a.BuddyId, CharacterName = b.Name, Pending = a.Pending, Group = a.Group }).ToArray();

            var obj = new CharacterValueObject
            {
                Character = _mapper.Map<CharacterDto>(characterEntity),
                Account = _accManager.GetAccountDto(characterEntity.AccountId)!,
                Link = characterLink,
                PetIgnores = petIgnores,
                Areas = _mapper.Map<AreaDto[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                BuddyList = buddyData,
                CoolDowns = _mapper.Map<CoolDownDto[]>(dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                Events = _mapper.Map<EventDto[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                InventoryItems = _mapper.Map<ItemDto[]>(invItems),
                CashShopItems = _mapper.Map<ItemDto[]>(cashItems),
                MarriageGiftItems = _mapper.Map<ItemDto[]>(marriageItems),
                DueyItems = _mapper.Map<ItemDto[]>(dueyItems),
                MerchantItems = _mapper.Map<ItemDto[]>(merchantItems),
                KeyMaps = _mapper.Map<KeyMapDto[]>(dbContext.Keymaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuickSlot = _mapper.Map<QuickSlotDto>(dbContext.Quickslotkeymappeds.AsNoTracking().Where(x => x.Accountid == characterEntity.AccountId).FirstOrDefault()),

                MonsterBooks = _mapper.Map<MonsterbookDto[]>(dbContext.Monsterbooks.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),

                MedalMaps = _mapper.Map<MedalMapDto[]>(dbContext.Medalmaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuestProgresses = _mapper.Map<QuestProgressDto[]>(dbContext.Questprogresses.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuestStatuses = _mapper.Map<QuestStatusDto[]>(dbContext.Queststatuses.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),

                SavedLocations = _mapper.Map<SavedLocationDto[]>(dbContext.Savedlocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                SkillMacros = _mapper.Map<SkillMacroDto[]>(dbContext.Skillmacros.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                Skills = _mapper.Map<SkillDto[]>(dbContext.Skills.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                StorageInfo = storageDto,
                TrockLocations = _mapper.Map<TrockLocationDto[]>(dbContext.Trocklocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                FameRecord = new RecentFameRecordDto
                {
                    ChararacterIds = fameRecords.Select(x => x.Characterid).ToArray(),
                    LastUpdateTime = fameRecords.Count == 0 ? 0 : fameRecords.Max(x => x.When).ToUnixTimeMilliseconds()
                }
            };

            _idDataSource[characterId] = obj;
            _nameDataSource[characterEntity.Name] = obj;
            _charcterViewCache[characterId] = obj;
            return obj;
        }

        /// <summary>
        /// CharacterValueObject 保存到数据库
        /// </summary>
        public async Task CommitAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

            dbContext.Monsterbooks.Where(x => _needUpdate.Keys.Contains(x.Charid)).ExecuteDelete();
            await dbContext.Petignores.Where(x => _needUpdate.Values.SelectMany(x => x.PetIgnores.Select(x => x.PetId)).Contains(x.Petid)).ExecuteDeleteAsync();
            await dbContext.Keymaps.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Skillmacros.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Savedlocations.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Trocklocations.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Buddies.Where(x => _needUpdate.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();
            await dbContext.AreaInfos.Where(x => _needUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
            await dbContext.Eventstats.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();

            await dbContext.Questprogresses.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Queststatuses.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            await dbContext.Medalmaps.Where(x => _needUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
            foreach (var obj in _needUpdate.Values)
            {
                dbContext.Characters.Update(_mapper.Map<CharacterEntity>(obj.Character));

                await dbContext.Monsterbooks.AddRangeAsync(_mapper.Map<Monsterbook[]>(obj.MonsterBooks));
                dbContext.Pets.UpdateRange(_mapper.Map<PetEntity[]>(obj.InventoryItems.Where(x => x.PetInfo != null).Select(x => x.PetInfo)));
                await dbContext.Petignores.AddRangeAsync(_mapper.Map<Petignore[]>(obj.PetIgnores));
                await dbContext.Keymaps.AddRangeAsync(_mapper.Map<KeyMapEntity[]>(obj.KeyMaps));

                await dbContext.Skillmacros.AddRangeAsync(_mapper.Map<SkillMacroEntity[]>(obj.SkillMacros));
                await dbContext.Savedlocations.AddRangeAsync(_mapper.Map<SavedLocationEntity[]>(obj.SavedLocations));
                await dbContext.Trocklocations.AddRangeAsync(_mapper.Map<Trocklocation[]>(obj.TrockLocations));
                await dbContext.Buddies.AddRangeAsync(_mapper.Map<BuddyEntity[]>(obj.BuddyList));
                await dbContext.AreaInfos.AddRangeAsync(_mapper.Map<AreaInfo[]>(obj.Areas));
                await dbContext.Eventstats.AddRangeAsync(_mapper.Map<Eventstat[]>(obj.Events));

                await dbContext.Questprogresses.AddRangeAsync(_mapper.Map<Questprogress[]>(obj.QuestProgresses));
                await dbContext.Queststatuses.AddRangeAsync(_mapper.Map<QuestStatusEntity[]>(obj.QuestStatuses));
                await dbContext.Medalmaps.AddRangeAsync(_mapper.Map<Medalmap[]>(obj.MedalMaps));

            }

            await dbTrans.CommitAsync();
            _needUpdate.Clear();
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
