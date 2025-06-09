using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.Core.Managers.Constants;
using Application.EF;
using Application.Shared.Items;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using client.inventory.manipulator;
using client.processor.npc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using net.server;
using Serilog;
using System.Collections.Concurrent;
using System.Xml.Linq;
using tools;

namespace Application.Core.Login.Datas
{
    /// <summary>
    /// 不包含Account，Account可能会在登录时被单独修改
    /// </summary>
    public class CharacterManager : IDisposable
    {
        ConcurrentDictionary<int, CharacterLiveObject> _idDataSource = new();
        ConcurrentDictionary<string, CharacterLiveObject> _nameDataSource = new();

        ConcurrentDictionary<int, CharacterViewObject> _charcterViewCache = new();

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

        public CharacterLiveObject? FindPlayerById(int id)
        {
            if (_idDataSource.TryGetValue(id, out var data) && data != null)
                return data;

            data = GetCharacter(id);
            if (data == null)
                return null;
            _idDataSource[id] = data;
            return data;
        }
        public CharacterLiveObject? FindPlayerByName(string name)
        {
            if (_nameDataSource.TryGetValue(name, out var data) && data != null)
                return data;

            data = GetCharacter(null, name);
            if (data == null)
                return null;
            _nameDataSource[name] = data;
            return data;
        }

        public void Update(Dto.PlayerSaveDto obj)
        {
            if (_idDataSource.TryGetValue(obj.Character.Id, out var origin))
            {
                var oldCharacterData = origin.Character;
                origin.Character = _mapper.Map<CharacterModel>(obj.Character);
                origin.BuddyList = _mapper.Map<BuddyModel[]>(obj.BuddyList);
                origin.InventoryItems = _mapper.Map<ItemModel[]>(obj.InventoryItems);
                origin.KeyMaps = _mapper.Map<KeyMapModel[]>(obj.KeyMaps);
                origin.SkillMacros = _mapper.Map<SkillMacroModel[]>(obj.SkillMacros);
                origin.Skills = _mapper.Map<SkillModel[]>(obj.Skills);
                origin.Areas = _mapper.Map<AreaModel[]>(obj.Areas);
                origin.Events = _mapper.Map<EventModel[]>(obj.Events);
                origin.MonsterBooks = _mapper.Map<MonsterbookModel[]>(obj.MonsterBooks);
                origin.PetIgnores = _mapper.Map<PetIgnoreModel[]>(obj.PetIgnores);
                origin.QuestStatuses = _mapper.Map<QuestStatusModel[]>(obj.QuestStatuses);
                origin.SavedLocations = _mapper.Map<SavedLocationModel[]>(obj.SavedLocations);
                origin.TrockLocations = _mapper.Map<TrockLocationModel[]>(obj.TrockLocations);
                origin.CoolDowns = _mapper.Map<CoolDownModel[]>(obj.CoolDowns);

                origin.Channel = obj.Channel;

                _masterServer.AccountManager.UpdateAccountGame(_mapper.Map<AccountGame>(obj.AccountGame));

                _logger.LogDebug("玩家{PlayerName}已缓存", obj.Character.Name);
                _dataStorage.SetCharacter(origin);

                if (oldCharacterData.Level != origin.Character.Level)
                {
                    // 等级变化通知
                    _masterServer.GuildManager.BroadcastLevelChanged(origin.Character);
                    _masterServer.TeamManager.UpdateParty(obj.Channel, origin.Character.Party, Shared.Team.PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);
                }

                if (oldCharacterData.JobId != origin.Character.JobId)
                {
                    // 转职通知
                    _masterServer.GuildManager.BroadcastJobChanged(origin.Character);
                    _masterServer.TeamManager.UpdateParty(obj.Channel, origin.Character.Party, Shared.Team.PartyOperation.SILENT_UPDATE, origin.Character.Id, origin.Character.Id);
                }
            }

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

        public void Dispose()
        {
            _idDataSource.Clear();
            _nameDataSource.Clear();
            _charcterViewCache.Clear();
        }

        public CharacterLiveObject? GetCharacter(int? characterId = null, string? characterName = null)
        {
            if (characterId == null && characterName == null)
                return null;

            CharacterLiveObject? d = null;
            if (characterId != null)
                _idDataSource.TryGetValue(characterId.Value, out d);
            if (d == null && characterName != null)
                _nameDataSource.TryGetValue(characterName, out d);

            if (d == null)
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var characterEntity = characterId != null
                    ? dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Id == characterId)
                    : dbContext.Characters.AsNoTracking().FirstOrDefault(x => x.Name == characterName);
                if (characterEntity == null)
                    return null;

                var petIgnores = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == characterId && x.Petid > -1)
                                  let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).Select(x => x.Itemid).ToArray()
                                  select new PetIgnoreModel { PetId = a.Petid, ExcludedItems = excluded }).ToArray();

                var invItems = InventoryManager.LoadItems(dbContext, characterEntity.Id, ItemType.Inventory);

                var buddyData = (from a in dbContext.Buddies
                                 join b in dbContext.Characters on a.BuddyId equals b.Id
                                 where a.CharacterId == characterId
                                 select new BuddyModel { CharacterId = a.BuddyId, CharacterName = b.Name, Pending = a.Pending, Group = a.Group }).ToArray();

                #region quest
                var questStatusData = (from a in dbContext.Queststatuses.AsNoTracking().Where(x => x.Characterid == characterId)
                                       let bs = dbContext.Questprogresses.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       let cs = dbContext.Medalmaps.AsNoTracking().Where(x => x.Characterid == characterId && a.Queststatusid == x.Queststatusid).ToArray()
                                       select new QuestStatusEntityPair(a, bs, cs)).ToArray();
                #endregion

                d = new CharacterLiveObject()
                {
                    Character = _mapper.Map<CharacterModel>(characterEntity),
                    Channel = 0,
                    PetIgnores = petIgnores,
                    Areas = _mapper.Map<AreaModel[]>(dbContext.AreaInfos.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    BuddyList = buddyData,
                    Events = _mapper.Map<EventModel[]>(dbContext.Eventstats.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    InventoryItems = _mapper.Map<ItemModel[]>(invItems),
                    KeyMaps = _mapper.Map<KeyMapModel[]>(dbContext.Keymaps.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),

                    MonsterBooks = _mapper.Map<MonsterbookModel[]>(dbContext.Monsterbooks.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),

                    QuestStatuses = _mapper.Map<QuestStatusModel[]>(questStatusData),

                    SavedLocations = _mapper.Map<SavedLocationModel[]>(dbContext.Savedlocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    SkillMacros = _mapper.Map<SkillMacroModel[]>(dbContext.Skillmacros.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    Skills = _mapper.Map<SkillModel[]>(dbContext.Skills.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    TrockLocations = _mapper.Map<TrockLocationModel[]>(dbContext.Trocklocations.AsNoTracking().Where(x => x.Characterid == characterId).ToArray()),
                    CoolDowns = _mapper.Map<CoolDownModel[]>(dbContext.Cooldowns.AsNoTracking().Where(x => x.Charid == characterId).ToArray()),
                    WishItems = dbContext.Wishlists.Where(x => x.CharId == characterId).Select(x => x.Sn).ToArray()
                };

                _idDataSource[characterEntity.Id] = d;
                _nameDataSource[characterEntity.Name] = d;
                _charcterViewCache[characterEntity.Id] = d;
            }
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
                if (_idDataSource.TryGetValue(item, out var e) && e != null)
                    list.Add(e);
                else if (_charcterViewCache.TryGetValue(item, out var d) && d != null)
                    list.Add(d);
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
                    Character = _mapper.Map<CharacterModel>(character),
                    InventoryItems = _mapper.Map<ItemModel[]>(itemObj.Where(x => x.Item.Characterid == character.Id))
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

        public int CreatePlayerCheck(int accountId, string name)
        {
            var accountInfo = _masterServer.AccountManager.GetAccountDto(accountId);
            if (accountInfo == null)
                return CreateCharResult.CharSlotLimited;

            if (accountInfo.Characterslots - _masterServer.AccountManager.GetAccountPlayerIds(accountId).Count <= 0)
                return CreateCharResult.CharSlotLimited;

            if (!_masterServer.CheckCharacterName(name))
                return CreateCharResult.NameInvalid;

            return CreateCharResult.Success;
        }

        public int CreatePlayer(int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender, int type)
        {
            var checkResult = CreatePlayerCheck(accountId, name);
            if (checkResult != CreateCharResult.Success)
                return checkResult;

            return _masterServer.Transport.CreatePlayer(new Dto.CreateCharRequestDto
            {
                AccountId = accountId,
                Type = type,
                Name = name,
                Face = face,
                Hair = hair,
                SkinColor = skin,
                Top = top,
                Bottom = bottom,
                Shoes = shoes,
                Weapon = weapon,
                Gender = gender
            }).Code;
        }

        public int CreatePlayerDB(Dto.NewPlayerSaveDto data)
        {
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var characterId = _dataStorage.CommitNewPlayer(dbContext, data);
                dbTrans.Commit();

                _masterServer.AccountManager.UpdateAccountCharacterCacheByAdd(data.Character.AccountId, characterId);
                _masterServer.Transport.BroadcastWorldGMPacket(PacketCreator.sendYellowTip("[New Char]: " + data.Character.AccountId + " has created a new character with IGN " + data.Character.Name));
                Log.Logger.Information("Account {AccountName} created chr with name {CharacterName}", data.Character.AccountId, data.Character.Name);
                return characterId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色保存到数据库");
                return CreateCharResult.Error;
            }
        }

        public void SendPacket(int playerId, Packet packet)
        {
            var chr = FindPlayerById(playerId);
            if (chr != null && chr.Channel > 0)
            {
                _masterServer.Transport.SendChannelPlayerPacket(chr.Channel, chr.Character.Id, packet);
            }
        }

        public List<int> GetGuildMembers(DBContext dbContext, int guildId)
        {
            List<int> dataList = new List<int>();
            var chrTemp = GetCharactersView(dbContext.Characters.Where(x => x.GuildId == guildId).Select(x => x.Id).ToArray());
            dataList.AddRange(chrTemp.Where(x => x.Character.GuildId == guildId).Select(x => x.Character.Id));
            dataList.AddRange(_charcterViewCache.Values.Where(x => x.Character.GuildId == guildId).Select(x => x.Character.Id));
            return dataList.ToHashSet().ToList();
        }

        public IDictionary<int, int[]> GetPlayerChannelPair(IEnumerable<CharacterViewObject> players)
        {
            return players.Where(x => x != null).GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.Character.Id).ToArray());
        }
    }
}
