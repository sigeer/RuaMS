using Application.Core.Datas;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace Application.Core.Login.Services
{
    public class CharacterManager
    {
        readonly IMapper _mapper;

        public CharacterManager(IMapper mapper)
        {
            _mapper = mapper;
        }
        /// <summary>
        /// 角色登录
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public async Task<CharacterValueObject?> GetCharacter(int characterId)
        {
            using var dbContext = new DBContext();
            var characterEntity = await dbContext.Characters.FirstOrDefaultAsync(x => x.Id == characterId);
            if (characterEntity == null)
                return null;

            var accountEntity = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == characterEntity.AccountId);
            if (accountEntity == null)
                return null;

            var now = DateTimeOffset.Now;
            var fameRecords = await dbContext.Famelogs.Where(x => x.Characterid == characterId && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToListAsync();

            var petIgnores = await (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                                    let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                                    select new PetIgnoreDto { PetId = a.Petid, ExcludedItems = excluded }).ToArrayAsync();

            return new CharacterValueObject
            {
                Character = _mapper.Map<CharacterDto>(characterEntity),
                Account = _mapper.Map<AccountDto>(accountEntity),
                PetIgnores = petIgnores,
                Areas = _mapper.Map<AreaDto[]>(await dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArrayAsync()),
                BuddyList = _mapper.Map<BuddyDto[]>(await dbContext.Buddies.AsNoTracking().Where(x => x.CharacterId == characterId).ToArrayAsync()),
                CoolDowns = _mapper.Map<CoolDownDto[]>(await dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArrayAsync()),
                Events = _mapper.Map<EventDto[]>(await dbContext.Eventstats.Where(x => x.Characterid == characterId).ToArrayAsync()),
                Items = _mapper.Map<ItemDto[]>(await dbContext.Inventoryitems.Where(x => x.Characterid == characterId).ToArrayAsync()),
                KeyMaps = _mapper.Map<KeyMapDto[]>(await dbContext.Keymaps.Where(x => x.Characterid == characterId).ToArrayAsync()),
                MedalMaps = _mapper.Map<MedalMapDto[]>(await dbContext.Medalmaps.Where(x => x.Characterid == characterId).ToArrayAsync()),
                MonsterBooks = _mapper.Map<MonsterbookDto[]>(await dbContext.Monsterbooks.Where(x => x.Charid == characterId).ToArrayAsync()),

                QuestProgresses = _mapper.Map<QuestProgressDto[]>(await dbContext.Questprogresses.Where(x => x.Characterid == characterId).ToArrayAsync()),
                QuestStatuses = _mapper.Map<QuestStatusDto[]>(await dbContext.Queststatuses.Where(x => x.Characterid == characterId).ToArrayAsync()),
                QuickSlot = _mapper.Map<QuickSlotDto>(await dbContext.Quickslotkeymappeds.Where(x => x.Accountid == characterEntity.AccountId).ToArrayAsync()),
                SavedLocations = _mapper.Map<SavedLocationDto[]>(await dbContext.Savedlocations.Where(x => x.Characterid == characterId).ToArrayAsync()),
                SkillMacros = _mapper.Map<SkillMacroDto[]>(await dbContext.Skillmacros.Where(x => x.Characterid == characterId).ToArrayAsync()),
                Skills = _mapper.Map<SkillDto[]>(await dbContext.Skills.Where(x => x.Characterid == characterId).ToArrayAsync()),
                StorageInfo = _mapper.Map<StorageDto>(await dbContext.Storages.Where(x => x.Accountid == characterEntity.AccountId).FirstOrDefaultAsync()),
                TrockLocations = _mapper.Map<TrockLocationDto[]>(await dbContext.Trocklocations.Where(x => x.Characterid == characterId).ToArrayAsync()),
                FameRecord = new RecentFameRecordDto { ChararacterIds = fameRecords.Select(x => x.Characterid).ToArray(), LastUpdateTime = fameRecords.Count == 0 ? 0 : fameRecords.Max(x => x.When).ToUnixTimeMilliseconds() }
            };
        }
    }
}
