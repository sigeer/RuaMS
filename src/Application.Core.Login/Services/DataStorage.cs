using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Shared.Models;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class DataStorage
    {
        Dictionary<int, CharacterLiveObject> _chrUpdate = new();
        Dictionary<int, AccountCtrl> _accUpdate = new();
        Dictionary<int, AccountGame> _accGameUpdate = new();

        Dictionary<int, AccountLoginStatus> _accLoginUpdate = new();


        Dictionary<int, ItemModel[]> _merchantUpdate = new();
        Dictionary<int, ItemModel[]> _dueyUpdate = new();
        Dictionary<int, ItemModel[]> _marriageUpdate = new();

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
        public async Task CommitCharacterAsync(DBContext dbContext)
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
                }
                await dbContext.SaveChangesAsync();
                _chrUpdate.Clear();
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
        public void SetCharacter(CharacterLiveObject obj)
        {
            _chrUpdate[obj.Character.Id] = obj;
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

        internal void SetAccountGame(AccountGame accountGame)
        {
            _accGameUpdate[accountGame.Id] = accountGame;
        }

        internal void SetAccount(AccountCtrl obj)
        {
            _accUpdate[obj.Id] = obj;
        }

        public async Task CommitAccountGameAsync(DBContext dbContext)
        {
            var updateCount = _accGameUpdate.Count;
            if (updateCount == 0)
                return;

            await dbContext.Quickslotkeymappeds.Where(x => _accGameUpdate.Keys.Contains(x.Accountid)).ExecuteDeleteAsync();
            dbContext.Storages.Where(x => _accGameUpdate.Keys.Contains(x.Accountid)).ExecuteDelete();

            foreach (var acc in _accGameUpdate)
            {
                if (acc.Value.QuickSlot != null)
                    dbContext.Quickslotkeymappeds.Add(new Quickslotkeymapped(acc.Key, acc.Value.QuickSlot.LongValue));

                dbContext.Storages.Add(new StorageEntity(acc.Key, acc.Value.Storage?.Slots ?? 4, acc.Value.Storage?.Meso ?? 0));
                InventoryManager.CommitInventoryByType(dbContext, acc.Key, acc.Value.StorageItems, ItemFactory.STORAGE);

                dbContext.Accounts.Where(x => x.Id == acc.Key).ExecuteUpdate(
                    x => x.SetProperty(y => y.MaplePoint, acc.Value.MaplePoint).SetProperty(y => y.NxPrepaid, acc.Value.NxPrepaid).SetProperty(y => y.NxCredit, acc.Value.NxCredit)
                    );

                InventoryManager.CommitInventoryByType(dbContext, acc.Key, acc.Value.CashAranItems, ItemFactory.CASH_ARAN);
                InventoryManager.CommitInventoryByType(dbContext, acc.Key, acc.Value.CashCygnusItems, ItemFactory.CASH_CYGNUS);
                InventoryManager.CommitInventoryByType(dbContext, acc.Key, acc.Value.CashExplorerItems, ItemFactory.CASH_EXPLORER);
                InventoryManager.CommitInventoryByType(dbContext, acc.Key, acc.Value.CashOverallItems, ItemFactory.CASH_OVERALL);
            }
            await dbContext.SaveChangesAsync();
            _accGameUpdate.Clear();
        }

        /// <summary>
        /// account的字段更新都是即时更新，不与character一同处理
        /// <para>有3种更新：1.仅更新lastlogin，2.更新该方法以下属性，3.更新现金相关，随cashshop更新</para>
        /// </summary>
        /// <param name="obj"></param>
        public async Task CommitAccountCtrlAsync(DBContext dbContext)
        {
            var updateCount = _accUpdate.Count;
            if (updateCount == 0)
                return;

            var allAccounts = await dbContext.Accounts.AsNoTracking().Where(x => _accUpdate.Keys.Contains(x.Id)).ToListAsync();
            foreach (var obj in _accUpdate.Values)
            {
                var dbModel = allAccounts.FirstOrDefault(x => x.Id == obj.Id)!;

                dbModel.Macs = obj.Macs;
                dbModel.Hwid = obj.Hwid;
                dbModel.Pic = obj.Pic;
                dbModel.Pin = obj.Pin;
                dbModel.Ip = obj.Ip;
                dbModel.GMLevel = obj.GMLevel;
                dbModel.Characterslots = obj.Characterslots;
            }
            await dbContext.SaveChangesAsync();
            _accUpdate.Clear();
        }


    }
}
