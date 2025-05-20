using Application.Core.Datas;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Datas
{
    public class DataStorage
    {
        Dictionary<int, CharacterValueObject> _chrUpdate = new Dictionary<int, CharacterValueObject>();


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
            if (_chrUpdate.Count == 0)
                return;

            _logger.LogInformation("正在保存用户数据...");
            await _semaphore.WaitAsync();
            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

                var accList = dbContext.Characters.AsNoTracking().Where(x => _chrUpdate.Keys.Contains(x.Id)).Select(x => x.AccountId).ToArray();

                await dbContext.Monsterbooks.Where(x => _chrUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Petignores.Where(x => _chrUpdate.Values.SelectMany(x => x.PetIgnores.Select(x => x.PetId)).Contains(x.Petid)).ExecuteDeleteAsync();
                await dbContext.Keymaps.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skills.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skillmacros.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Savedlocations.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Trocklocations.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Buddies.Where(x => _chrUpdate.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();
                await dbContext.AreaInfos.Where(x => _chrUpdate.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Eventstats.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();

                await dbContext.Questprogresses.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Queststatuses.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Medalmaps.Where(x => _chrUpdate.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Wishlists.Where(x => _chrUpdate.Keys.Contains(x.CharId)).ExecuteDeleteAsync();

                await dbContext.Storages.Where(x => accList.Contains(x.Accountid)).ExecuteDeleteAsync();
                await dbContext.Quickslotkeymappeds.Where(x => accList.Contains(x.Accountid)).ExecuteDeleteAsync();
                foreach (var obj in _chrUpdate.Values)
                {
                    dbContext.Characters.Update(_mapper.Map<CharacterEntity>(obj.Character));
                    dbContext.Accounts.Update(_mapper.Map<AccountEntity>(obj.Account));

                    await dbContext.Monsterbooks.AddRangeAsync(obj.MonsterBooks.Select(x => new MonsterbookEntity(obj.Character.Id, x.Cardid, x.Level)));
                    dbContext.Pets.UpdateRange(obj.InventoryItems.Where(x => x.PetInfo != null)
                        .Select(x => new PetEntity(x.PetInfo!.Petid, x.PetInfo.Name, x.PetInfo.Level, x.PetInfo.Closeness, x.PetInfo.Fullness, x.PetInfo.Summoned, x.PetInfo.Flag)));
                    await dbContext.Petignores.AddRangeAsync(obj.PetIgnores.SelectMany(x => x.ExcludedItems.Select(y => new Petignore(x.PetId, y))));
                    await dbContext.Keymaps.AddRangeAsync(obj.KeyMaps.Select(x => new KeyMapEntity(obj.Character.Id, x.Key, x.Type, x.Action)));

                    await dbContext.Skillmacros.AddRangeAsync(
                        obj.SkillMacros.Select(x => new SkillMacroEntity(obj.Character.Id, (sbyte)x.Position, x.Skill1, x.Skill2, x.Skill3, x.Name, (sbyte)x.Shout)));

                    await CommitInventoryByType(dbContext, obj.Character.Id, obj.InventoryItems, ItemFactory.INVENTORY);

                    // Skill
                    await dbContext.Skills.AddRangeAsync(
                        obj.Skills.Select(x => new SkillEntity(obj.Character.Id, x.Skillid, x.Skilllevel, x.Masterlevel, x.Expiration))
                        );

                    if (obj.QuickSlot != null)
                    {
                        await dbContext.Quickslotkeymappeds.AddAsync(new Quickslotkeymapped(obj.Character.AccountId, obj.QuickSlot.LongValue));
                    }

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
                    await CommitInventoryByType(dbContext, obj.Account.Id, obj.CashShop.Items, ItemFactory.GetItemFactory(obj.CashShop.FactoryType));
                    await dbContext.Wishlists.AddRangeAsync(obj.CashShop.WishItems.Select(x => new WishlistEntity(obj.Character.Id, x)));

                    var m = obj.StorageInfo ?? new StorageDto(obj.Character.AccountId);
                    await dbContext.Storages.AddAsync(new StorageEntity(m.Accountid, m.Slots, m.Meso));
                    await CommitInventoryByType(dbContext, obj.Account.Id, m.Items, ItemFactory.STORAGE);
                }

                _chrUpdate.Clear();
                await dbTrans.CommitAsync();
                _logger.LogInformation("保存用户数据{Status}", "成功");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "保存用户数据{Status}", "失败");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal void Set(CharacterValueObject characterValueObject)
        {
            _chrUpdate[characterValueObject.Character.Id] = characterValueObject;
        }

        private async Task CommitInventoryByType(DBContext dbContext, int targetId, ItemDto[] items, ItemFactory type)
        {
            var allItems = dbContext.Inventoryitems.Where(x => (type.IsAccount ? x.Accountid == targetId : x.Characterid == targetId) && x.Type == type.getValue()).ToList();
            var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();

            var petIds = allItems.Select(x => x.Petid).ToArray();
            await dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
            await dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
            await dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDeleteAsync();

            foreach (var item in items)
            {
                var model = _mapper.Map<Inventoryitem>(item);
                await dbContext.Inventoryitems.AddAsync(model);
                await dbContext.SaveChangesAsync();

                if (item.EquipInfo != null)
                {
                    var temp = _mapper.Map<Inventoryequipment>(item.EquipInfo);
                    temp.Inventoryitemid = model.Inventoryitemid;
                    await dbContext.Inventoryequipments.AddAsync(temp);
                }
                if (item.PetInfo != null)
                {
                    await dbContext.Pets.AddAsync(_mapper.Map<PetEntity>(item.PetInfo));
                }
            }

            await dbContext.SaveChangesAsync();
        }

    }
}
