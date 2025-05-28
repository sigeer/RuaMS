using Application.Core.Login.Datas;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Shared.Login;
using Application.Utility.Exceptions;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class DataStorage
    {
        Dictionary<int, PlayerSaveDto> _chrUpdate = new Dictionary<int, PlayerSaveDto>();
        Dictionary<int, AccountDto> _accUpdate = new Dictionary<int, AccountDto>();

        Dictionary<int, AccountLoginStatus> _accLoginUpdate = new();


        Dictionary<int, ItemDto[]> _merchantUpdate = new();
        Dictionary<int, ItemDto[]> _dueyUpdate = new();
        Dictionary<int, ItemDto[]> _marriageUpdate = new();

        readonly IMapper _mapper;
        readonly ILogger<DataStorage> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DataStorage(IMapper mapper, ILogger<DataStorage> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }


        /// <summary>
        /// CharacterValueObject 保存到数据库
        /// </summary>
        public async Task CommitCharacterAsync()
        {
            var updateCount = _chrUpdate.Count;
            if (updateCount == 0)
                return;

            _logger.LogInformation("正在保存用户数据...");

            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogInformation("失败：已经有一个保存操作正在进行中");
                return;
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

                
                var updateCharacters = await dbContext.Characters.Where(x => _chrUpdate.Keys.Contains(x.Id)).ToListAsync();
                var accList = updateCharacters.Select(x => x.AccountId).ToArray();

                await dbContext.Monsterbooks.Where(x => _chrUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                var petIdList = _chrUpdate.Values.SelectMany(x => x.PetIgnores.Select(x => x.PetId)).ToArray();
                await dbContext.Petignores.Where(x => petIdList.Contains(x.Petid)).ExecuteDeleteAsync();
                await dbContext.Keymaps.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skills.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skillmacros.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Savedlocations.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Trocklocations.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Buddies.Where(x => _chrUpdate.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();
                await dbContext.AreaInfos.Where(x => _chrUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Eventstats.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Cooldowns.Where(x => _chrUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();

                await dbContext.Questprogresses.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Queststatuses.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Medalmaps.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();

                foreach (var obj in _chrUpdate.Values)
                {
                    var character = updateCharacters.FirstOrDefault(x => x.Id == obj.Character.Id)!;
                    _mapper.Map(obj.Character, character);

                    await dbContext.Monsterbooks.AddRangeAsync(obj.MonsterBooks.Select(x => new MonsterbookEntity(obj.Character.Id, x.Cardid, x.Level)));

                    await dbContext.Petignores.AddRangeAsync(obj.PetIgnores.SelectMany(x => x.ExcludedItems.Select(y => new Petignore(x.PetId, y))));
                    await dbContext.Keymaps.AddRangeAsync(obj.KeyMaps.Select(x => new KeyMapEntity(obj.Character.Id, x.Key, x.Type, x.Action)));

                    await dbContext.Skillmacros.AddRangeAsync(
                        obj.SkillMacros.Where(x => x != null).Select(x => new SkillMacroEntity(obj.Character.Id, (sbyte)x.Position, x.Skill1, x.Skill2, x.Skill3, x.Name, (sbyte)x.Shout)));

                    await InventoryManager.CommitInventoryByTypeAsync(dbContext, obj.Character.Id, obj.InventoryItems, ItemFactory.INVENTORY);

                    await dbContext.Cooldowns.AddRangeAsync(
                        obj.CoolDowns.Select(x => new CooldownEntity(obj.Character.Id, x.SkillId, x.Length, x.StartTime)));

                    // Skill
                    await dbContext.Skills.AddRangeAsync(
                        obj.Skills.Select(x => new SkillEntity(x.Skillid, obj.Character.Id, x.Skilllevel, x.Masterlevel, x.Expiration))
                        );

                    await dbContext.Savedlocations.AddRangeAsync(obj.SavedLocations.Select(x => new SavedLocationEntity(x.Map, x.Portal, obj.Character.Id, x.Locationtype)));
                    await dbContext.Trocklocations.AddRangeAsync(obj.TrockLocations.Select(x => new Trocklocation(obj.Character.Id, x.Mapid, x.Vip)));
                    await dbContext.Buddies.AddRangeAsync(obj.BuddyList.Select(x => new BuddyEntity(obj.Character.Id, x.CharacterId, x.Pending, x.Group)));
                    await dbContext.AreaInfos.AddRangeAsync(obj.Areas.Select(x => new AreaInfo(obj.Character.Id, x.Area, x.Info)));
                    await dbContext.Eventstats.AddRangeAsync(obj.Events.Select(x => new Eventstat(obj.Character.Id, x.Name, x.Info)));

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

                    // family

                    // cashshop
                }
                await dbContext.SaveChangesAsync();
                _chrUpdate.Clear();
                await dbTrans.CommitAsync();
                _logger.LogInformation("保存了{Count}个用户数据", updateCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存用户数据{Status}", "失败");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void UpdateStorage(DBContext dbContext, int accId, StorageDto? storage)
        {
            var m = storage ?? new StorageDto(accId);
            var dbStorage = dbContext.Storages.Where(x => x.Accountid == accId).FirstOrDefault();
            if (dbStorage == null)
            {
                dbStorage = new StorageEntity(m.Accountid, m.Slots, m.Meso);
                dbContext.Storages.Add(dbStorage);
            }
            else
            {
                dbStorage.Slots = m.Slots;
                dbStorage.Meso = m.Meso;
            }
            InventoryManager.CommitInventoryByType(dbContext, accId, m.Items, ItemFactory.STORAGE);
        }

        public void SetCharacter(PlayerSaveDto obj)
        {
            _chrUpdate[obj.Character.Id] = obj;

            // Account相关的数据要即时更新
            using var dbContext = _dbContextFactory.CreateDbContext();
            using var dbTrans = dbContext.Database.BeginTransaction();

            dbContext.Quickslotkeymappeds.Where(x => x.Accountid == obj.Character.AccountId).ExecuteDelete();
            if (obj.QuickSlot != null)
            {
                dbContext.Quickslotkeymappeds.Add(new Quickslotkeymapped(obj.Character.AccountId, obj.QuickSlot.LongValue));
            }

            // storage
            UpdateStorage(dbContext, obj.Character.AccountId, obj.StorageInfo);

            dbContext.SaveChanges();

            dbTrans.Commit();
        }

        /// <summary>
        /// 每次进出商城都会重新加载数据 所以
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="BusinessException"></exception>
        public void UpdateCashShop(CharacterValueObject obj)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            using var dbTrans = dbContext.Database.BeginTransaction();

            // cash shop
            InventoryManager.CommitInventoryByType(dbContext, obj.Character.AccountId, obj.CashShop.Items, ItemFactory.GetItemFactory(obj.CashShop.FactoryType));
            dbContext.Wishlists.Where(x => obj.Character.Id == x.CharId).ExecuteDelete();
            dbContext.Wishlists.AddRange(obj.CashShop.WishItems.Select(x => new WishlistEntity(obj.Character.Id, x)));

            // account
            var dbAccount = dbContext.Accounts.Where(x => x.Id == obj.Character.AccountId).FirstOrDefault();
            if (dbAccount == null)
                throw new BusinessException("");

            dbAccount.NxCredit = obj.CashShop.NxCredit;
            dbAccount.MaplePoint = obj.CashShop.MaplePoint;
            dbAccount.NxPrepaid = obj.CashShop.NxPrepaid;

            dbContext.SaveChanges();
            dbTrans.Commit();
        }

        internal void SetAccountLoginRecord(KeyValuePair<int, AccountLoginStatus> item)
        {
            _accLoginUpdate[item.Key] = item.Value;
        }

        internal async Task CommitAccountLoginRecord()
        {
            if (_accLoginUpdate.Count == 0)
                return;

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

            var idsToUpdate = _accLoginUpdate.Keys.ToList();
            var accounts = await dbContext.Accounts.Where(x => idsToUpdate.Contains(x.Id)).ToListAsync();

            foreach (var acc in accounts)
            {
                acc.Lastlogin = _accLoginUpdate[acc.Id].DateTime;
            }

            await dbContext.SaveChangesAsync();
            _accLoginUpdate.Clear();
        }
    }
}
