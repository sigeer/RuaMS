using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Utility;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Services
{
    public class DataStorage
    {
        ConcurrentDictionary<int, CharacterLiveObject> _chrUpdate = new();
        ConcurrentDictionary<int, AccountCtrl> _accUpdate = new();
        ConcurrentDictionary<int, AccountGame> _accGameUpdate = new();

        ConcurrentDictionary<int, AccountLoginStatus> _accLoginUpdate = new();


        ConcurrentDictionary<int, ItemModel[]> _merchantUpdate = new();
        /// <summary>
        /// 只会新增/删除 Key: Id（不是packageId）
        /// </summary>
        ConcurrentDictionary<int, UpdateField<DueyPackageModel>> _dueyUpdate = new();
        ConcurrentDictionary<int, ItemModel[]> _marriageUpdate = new();

        readonly IMapper _mapper;
        readonly ILogger<DataStorage> _logger;


        public DataStorage(IMapper mapper, ILogger<DataStorage> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public int CommitNewPlayer(DBContext dbContext, Dto.NewPlayerSaveDto newCharacter)
        {
            _logger.LogInformation("正在保存新角色数据...");

            try
            {
                var character = _mapper.Map<CharacterEntity>(newCharacter.Character);
                dbContext.Characters.Add(character);
                dbContext.SaveChanges();

                dbContext.Keymaps.AddRange(newCharacter.KeyMaps.Select(x => new KeyMapEntity(character.Id, x.Key, x.Type, x.Action)));


                InventoryManager.CommitInventoryByType(dbContext, character.Id, _mapper.Map<ItemModel[]>(newCharacter.InventoryItems), ItemFactory.INVENTORY);

                // Skill
                dbContext.Skills.AddRange(
                    newCharacter.Skills.Select(x => new SkillEntity(x.Skillid, character.Id, x.Skilllevel, x.Masterlevel, x.Expiration))
                    );

                dbContext.Eventstats.AddRange(newCharacter.Events.Select(x => new Eventstat(character.Id, x.Name, x.Info)));


                dbContext.SaveChanges();
                _logger.LogInformation("新角色创建成功Id={Id}, Name = {Name}, AccountId={AccountId}", character.Id, character.Name, character.AccountId);
                return character.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新角色保存用户{Status}", "失败");
                return -2;
            }

        }

        /// <summary>
        /// CharacterValueObject 保存到数据库
        /// </summary>
        public async Task CommitCharacterAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, CharacterLiveObject>();
            foreach (var key in _chrUpdate.Keys.ToList())
            {
                _chrUpdate.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            _logger.LogInformation("正在保存用户数据...");

            try
            {
                var updateCharacters = await dbContext.Characters.Where(x => updateData.Keys.Contains(x.Id)).ToListAsync();
                var accList = updateCharacters.Select(x => x.AccountId).ToArray();

                await dbContext.Monsterbooks.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                var petIdList = updateData.Values.SelectMany(x => x.PetIgnores.Select(x => x.PetId)).ToArray();
                await dbContext.Petignores.Where(x => petIdList.Contains(x.Petid)).ExecuteDeleteAsync();
                await dbContext.Keymaps.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skills.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Skillmacros.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Savedlocations.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Trocklocations.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Buddies.Where(x => updateData.Keys.Contains(x.CharacterId)).ExecuteDeleteAsync();
                await dbContext.AreaInfos.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
                await dbContext.Eventstats.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Cooldowns.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();

                await dbContext.Questprogresses.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Queststatuses.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();
                await dbContext.Medalmaps.Where(x => updateData.Keys.Contains(x.Characterid)).ExecuteDeleteAsync();

                foreach (var obj in updateData.Values)
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
                _logger.LogInformation("保存了{Count}个用户数据", updateCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存用户数据{Status}", "失败");
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

        internal async Task CommitAccountLoginRecord(DBContext dbContext)
        {
            if (_accLoginUpdate.Count == 0)
                return;

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
            var updateData = new Dictionary<int, AccountGame>();
            foreach (var key in _accGameUpdate.Keys.ToList())
            {
                _accGameUpdate.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            await dbContext.Quickslotkeymappeds.Where(x => updateData.Keys.Contains(x.Accountid)).ExecuteDeleteAsync();
            await dbContext.Storages.Where(x => updateData.Keys.Contains(x.Accountid)).ExecuteDeleteAsync();

            foreach (var acc in updateData)
            {
                if (acc.Value.QuickSlot != null)
                    dbContext.Quickslotkeymappeds.Add(new Quickslotkeymapped(acc.Key, acc.Value.QuickSlot.LongValue));

                dbContext.Storages.Add(new StorageEntity(acc.Key, acc.Value.Storage?.Slots ?? 4, acc.Value.Storage?.Meso ?? 0));
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.StorageItems, ItemFactory.STORAGE);

                await dbContext.Accounts.Where(x => x.Id == acc.Key).ExecuteUpdateAsync(
                    x => x.SetProperty(y => y.MaplePoint, acc.Value.MaplePoint).SetProperty(y => y.NxPrepaid, acc.Value.NxPrepaid).SetProperty(y => y.NxCredit, acc.Value.NxCredit)
                    );

                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashAranItems, ItemFactory.CASH_ARAN);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashCygnusItems, ItemFactory.CASH_CYGNUS);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashExplorerItems, ItemFactory.CASH_EXPLORER);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashOverallItems, ItemFactory.CASH_OVERALL);
            }
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// account的字段更新都是即时更新，不与character一同处理
        /// </summary>
        /// <param name="obj"></param>
        public async Task CommitAccountCtrlAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, AccountCtrl>();
            foreach (var key in _accUpdate.Keys.ToList())
            {
                _accUpdate.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var allAccounts = await dbContext.Accounts.Where(x => updateData.Keys.Contains(x.Id)).ToListAsync();
            foreach (var obj in updateData.Values)
            {
                var dbModel = allAccounts.FirstOrDefault(x => x.Id == obj.Id)!;

                dbModel.Macs = obj.Macs == null ? null : string.Join(",", obj.Macs.Split(",").Select(x => x.Trim()).ToHashSet());
                dbModel.Hwid = obj.Hwid;
                dbModel.Pic = obj.Pic;
                dbModel.Pin = obj.Pin;
                dbModel.Ip = obj.Ip;
                dbModel.Gender = obj.Gender;
                dbModel.Tos = obj.Tos;
                dbModel.GMLevel = obj.GMLevel;
                dbModel.Characterslots = obj.Characterslots;
            }
            await dbContext.SaveChangesAsync();
        }

        internal void SetDueyPackageAdded(DueyPackageModel dueyPackageModel)
        {
            _dueyUpdate[dueyPackageModel.Id] = new UpdateField<DueyPackageModel>(UpdateMethod.AddOrUpdate, dueyPackageModel);
        }

        internal void SetDueyPackageRemoved(DueyPackageModel dueyPackageModel)
        {
            dueyPackageModel.Item = null;
            _dueyUpdate[dueyPackageModel.Id] = new UpdateField<DueyPackageModel>(UpdateMethod.Remove, dueyPackageModel);
        }

        public async Task CommitDueyPackageAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, UpdateField<DueyPackageModel>>();
            foreach (var key in _dueyUpdate.Keys.ToList())
            {
                _dueyUpdate.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                int packageId = 0;
                if (item.Method == UpdateMethod.AddOrUpdate)
                {
                    var tempData = new DueyPackageEntity(obj.ReceiverId, obj.SenderName, obj.Mesos, obj.Message, obj.Checked, obj.Type, obj.TimeStamp);
                    dbContext.Dueypackages.Add(tempData);
                    await dbContext.SaveChangesAsync();
                    packageId = tempData.PackageId;
                    await InventoryManager.CommitInventoryByTypeAsync(dbContext, packageId, obj.Item == null ? [] : [obj.Item], ItemFactory.DUEY);
                    obj.PackageId = packageId;

                }
                if(item.Method == UpdateMethod.Remove && item.Data.PackageId > 0)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    packageId = item.Data.PackageId;
                    await dbContext.Dueypackages.Where(x => x.PackageId == packageId).ExecuteDeleteAsync();
                    await InventoryManager.CommitInventoryByTypeAsync(dbContext, packageId, [], ItemFactory.DUEY);
                }

            }
            await dbContext.SaveChangesAsync();
        }

        #region Guild
        ConcurrentDictionary<int, UpdateField<GuildModel>> _guild = new();
        public void SetGuildUpdate(GuildModel data)
        {
            _guild[data.GuildId] = new UpdateField<GuildModel>(UpdateMethod.AddOrUpdate, data);
        }
        public void SetGuildRemoved(GuildModel data)
        {
            _guild[data.GuildId] = new UpdateField<GuildModel>(UpdateMethod.Remove, data);
        }
        public async Task CommitGuildAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, UpdateField<GuildModel>>();
            foreach (var key in _guild.Keys.ToList())
            {
                _guild.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Guilds.Where(x => updateData.Keys.Contains(x.GuildId)).ToList();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Method == UpdateMethod.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Guilds.Where(x => x.GuildId == obj.GuildId).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.GuildId == obj.GuildId);
                    if (dbModel == null)
                    {
                        dbModel = new GuildEntity(obj.GuildId, obj.Name, obj.Leader);
                        dbContext.Add(dbModel);
                    }
                    dbModel.GP = obj.GP;
                    dbModel.Rank1Title = obj.Rank1Title;
                    dbModel.Rank2Title = obj.Rank2Title;
                    dbModel.Rank3Title = obj.Rank3Title;
                    dbModel.Rank4Title = obj.Rank4Title;
                    dbModel.Rank5Title = obj.Rank5Title;
                    dbModel.Logo = obj.Logo;
                    dbModel.LogoBg = obj.LogoBg;
                    dbModel.LogoBgColor = obj.LogoBgColor;
                    dbModel.LogoColor = obj.LogoColor;
                    dbModel.AllianceId = obj.AllianceId;
                    dbModel.Capacity = obj.Capacity;
                    dbModel.Name = obj.Name;
                    dbModel.Notice = obj.Notice;
                    dbModel.Signature = obj.Signature;
                    dbModel.Leader = obj.Leader;
                }

            }
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region Alliance
        ConcurrentDictionary<int, UpdateField<AllianceModel>> _alliance = new();
        public void SetAlliance(AllianceModel data)
        {
            _alliance[data.Id] = new UpdateField<AllianceModel>(UpdateMethod.AddOrUpdate, data);
        }
        public void SetAllianceRemoved(AllianceModel data)
        {
            _alliance[data.Id] = new UpdateField<AllianceModel>(UpdateMethod.Remove, data);
        }
        public async Task CommitAllianceAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, UpdateField<AllianceModel>>();
            foreach (var key in _alliance.Keys.ToList())
            {
                _alliance.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            var dbData = dbContext.Alliances.Where(x => updateData.Keys.Contains(x.Id)).ToList();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Method == UpdateMethod.Remove)
                {
                    // 已经保存过数据库，存在packageid 才需要从数据库移出
                    // 没保存过数据库的，从内存中移出就行，不需要执行这里的更新
                    await dbContext.Alliances.Where(x => x.Id == obj.Id).ExecuteDeleteAsync();
                }
                else
                {
                    var dbModel = dbData.FirstOrDefault(x => x.Id == obj.Id);
                    if (dbModel == null)
                    {
                        dbModel = new AllianceEntity(obj.Id, obj.Name);
                        dbContext.Add(dbModel);
                    }
                    dbModel.Capacity = obj.Capacity;
                    dbModel.Name = obj.Name;
                    dbModel.Notice = obj.Notice;
                    dbModel.Rank1 = obj.Rank1;
                    dbModel.Rank2 = obj.Rank2;
                    dbModel.Rank3 = obj.Rank3;
                    dbModel.Rank4 = obj.Rank4;
                    dbModel.Rank5 = obj.Rank5;
                }

            }
            await dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
