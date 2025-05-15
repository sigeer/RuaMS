using Application.Core.Datas;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using client.inventory.manipulator;
using client.processor.npc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using Serilog;

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


        public bool DeleteChar(int cid, int senderAccId)
        {
            if (!Server.getInstance().haveCharacterEntry(senderAccId, cid))
            {    // thanks zera (EpiphanyMS) for pointing a critical exploit with non-authed character deletion request
                return false;
            }

            int accId = senderAccId;
            int world = 0;
            try
            {
                using var dbContext = new DBContext();
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
                Server.getInstance().deleteCharacterEntry(accId, cid);
                AllPlayerStorage.DeleteCharacter(cid);
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return false;
            }
        }
    }
}
