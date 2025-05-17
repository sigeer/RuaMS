using Application.Core.Datas;
using Application.Core.EF.Entities.Items;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using constants.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.EntityFrameworkCore.Extensions;
using Mysqlx.Crud;

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

        public CharacterManager(IMapper mapper, ILogger<CharacterManager> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public void Update(CharacterValueObject obj)
        {
            _idDataSource[obj.Character.Id] = obj;
            _nameDataSource[obj.Character.Name] = obj;

            Commit(obj);
        }

        public bool Remove(int characterId)
        {
            if (_idDataSource.Remove(characterId, out var d))
            {
                _nameDataSource.Remove(d.Character.Name);
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

            var items = (from a in dbContext.Inventoryitems.AsNoTracking().Where(x => x.Characterid == characterId || (x.Characterid == null && x.Accountid == characterEntity.AccountId))
                         join b in dbContext.Inventoryequipments.AsNoTracking() on a.Inventoryitemid equals b.Inventoryitemid into bss
                         from bs in bss.DefaultIfEmpty()
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new ItemEntityPair(a, bs, cs)).ToList();

            var buddyData = (from a in dbContext.Buddies
                             join b in dbContext.Characters on a.BuddyId equals b.Id
                             where a.CharacterId == characterId
                             select new BuddyDto { CharacterId = a.BuddyId, CharacterName = b.Name, Pending = a.Pending, Group = a.Group }).ToArray();

            var obj = new CharacterValueObject
            {
                Character = _mapper.Map<CharacterDto>(characterEntity),
                Link  = characterLink,
                PetIgnores = petIgnores,
                Areas = _mapper.Map<AreaDto[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                BuddyList = buddyData,
                CoolDowns = _mapper.Map<CoolDownDto[]>(dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                Events = _mapper.Map<EventDto[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                Items = _mapper.Map<ItemDto[]>(items),
                KeyMaps = _mapper.Map<KeyMapDto[]>(dbContext.Keymaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuickSlot = _mapper.Map<QuickSlotDto>(dbContext.Quickslotkeymappeds.AsNoTracking().Where(x => x.Accountid == characterEntity.AccountId).FirstOrDefault()),

                MonsterBooks = _mapper.Map<MonsterbookDto[]>(dbContext.Monsterbooks.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),

                MedalMaps = _mapper.Map<MedalMapDto[]>(dbContext.Medalmaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuestProgresses = _mapper.Map<QuestProgressDto[]>(dbContext.Questprogresses.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                QuestStatuses = _mapper.Map<QuestStatusDto[]>(dbContext.Queststatuses.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),

                SavedLocations = _mapper.Map<SavedLocationDto[]>(dbContext.Savedlocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                SkillMacros = _mapper.Map<SkillMacroDto[]>(dbContext.Skillmacros.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                Skills = _mapper.Map<SkillDto[]>(dbContext.Skills.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                StorageInfo = _mapper.Map<StorageDto>(dbContext.Storages.AsNoTracking().Where(x => x.Accountid == characterEntity.AccountId).FirstOrDefault()),
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
        public void Commit(CharacterValueObject obj)
        {

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

            var equipedType = InventoryType.EQUIPPED.getType();
            var dataList = (from a in dbContext.Inventoryitems.Where(x => x.Characterid != null && needLoadFromDB.Contains(x.Characterid.Value))
                            join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                            from bs in bss.DefaultIfEmpty()
                            where a.Inventorytype == equipedType
                            select new ItemEntityPair(a, bs, null)).ToList();

            foreach (var character in characters)
            {
                var obj = new CharacterViewObject()
                {
                    Character = _mapper.Map<CharacterDto>(character),
                    Items = _mapper.Map<ItemDto[]>(dataList.Where(x => x.Item.Characterid == character.Id))
                };
                _charcterViewCache[obj.Character.Id] = obj;
                list.Add(obj);
            }
            return list;

        }
    }
}
