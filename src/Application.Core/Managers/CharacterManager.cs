using Application.Core.Game.Items;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using AutoMapper;
using client;
using client.inventory;
using client.inventory.manipulator;
using client.keybind;
using client.newyear;
using client.processor.npc;
using constants.id;
using constants.inventory;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using server.events;
using server.life;
using server.maps;
using server.quest;
using System.Text.RegularExpressions;
using tools;

namespace Application.Core.Managers
{
    public class CharacterManager
    {
        static IMapper Mapper = GlobalTools.Mapper;
        public static string makeMapleReadable(string input)
        {
            string i = input.Replace('I', 'i');
            i = i.Replace('l', 'L');
            i = i.Replace("rn", "Rn");
            i = i.Replace("vv", "Vv");
            i = i.Replace("VV", "Vv");

            return i;
        }

        private static string[] BLOCKED_NAMES = {
            "admin", "owner", "moderator", "intern", "donor", "administrator", "FREDRICK", "help", "helper", "alert", "notice", "maplestory", "fuck", "wizet", "fucking",
            "negro", "fuk", "fuc", "penis", "pussy", "asshole", "gay", "nigger", "homo", "suck", "cum", "shit", "shitty", "condom", "security", "official", "rape", "nigga",
            "sex", "tit", "boner", "orgy", "clit", "asshole", "fatass", "bitch", "support", "gamemaster", "cock", "gaay", "gm", "operate", "master",
            "sysop", "party", "GameMaster", "community", "message", "event", "test", "meso", "Scania", "yata", "AsiaSoft", "henesys"};
        public static bool CheckCharacterName(string name)
        {
            // 禁用名
            if (BLOCKED_NAMES.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (!Regex.IsMatch(name, "[a-zA-Z0-9]{3,12}"))
                return false;

            using DBContext dbContext = new DBContext();
            if (dbContext.Characters.Any(x => x.Name == name))
                return false;

            return true;
        }

        public static bool DeleteCharacterFromDB(int cid)
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var charModel = dbContext.Characters.FirstOrDefault(x => x.Id == cid);
                if (charModel == null)
                    return false;

                if (charModel.GuildId > 0 && charModel.GuildRank < 1)
                {
                    var guildInfo = dbContext.Guilds.FirstOrDefault(x => x.GuildId == charModel.GuildId);
                    if (guildInfo != null)
                    {
                        if (guildInfo.AllianceId > 0)
                        {
                            var allianceInfo = dbContext.Alliances.FirstOrDefault(x => x.Id == guildInfo.AllianceId);
                            if (allianceInfo != null)
                            {
                                if (charModel.AllianceRank == 1)
                                {
                                    dbContext.AllianceGuilds.Where(x => x.AllianceId == allianceInfo.Id).ExecuteDelete();
                                    dbContext.Alliances.Where(x => x.Id == allianceInfo.Id).ExecuteDelete();
                                }
                                else
                                {
                                    dbContext.AllianceGuilds.Where(x => x.GuildId == guildInfo.GuildId).ExecuteDelete();
                                }
                            }
                        }
                        dbContext.Guilds.Remove(guildInfo);
                    }
                }
                dbContext.Buddies.Where(x => x.CharacterId == cid || x.BuddyId == cid).ExecuteDelete();
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
                var ringIdList = dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).Select(x => x.RingId).ToList();
                dbContext.Rings.Where(x => ringIdList.Contains(x.Id)).ExecuteDelete();

                var inventoryPetIdList = inventoryItems.Select(x => x.Petid).ToList();
                dbContext.Pets.Where(x => inventoryPetIdList.Contains(x.Petid)).ExecuteDelete();

                dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Inventoryitems.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();

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
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return false;
            }
        }

        public static bool deleteCharFromDB(IPlayer player, int senderAccId)
        {
            int cid = player.getId();
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
                world = dbContext.Characters.Where(x => x.Id == cid).Select(x => x.World).FirstOrDefault();

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

                var threadIdList = dbContext.BbsThreads.Where(x => x.Postercid == cid).Select(x => x.Threadid).ToList();
                dbContext.BbsReplies.Where(x => threadIdList.Contains(x.Threadid)).ExecuteDelete();
                dbContext.BbsThreads.Where(x => x.Postercid == cid).ExecuteDelete();


                var rs = dbContext.Characters.FirstOrDefault(x => x.Id == cid && x.AccountId == accId);
                if (rs == null)
                    throw new BusinessResException();

                Server.getInstance().deleteGuildCharacter(player);

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

                deleteQuestProgressWhereCharacterId(dbContext, cid);
                FredrickProcessor.removeFredrickLog(cid);   // thanks maple006 for pointing out the player's Fredrick items are not being deleted at character deletion

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

        public static void deleteQuestProgressWhereCharacterId(DBContext dbContext, int cid)
        {
            dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();

            dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();

            dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();
        }


        public static IPlayer NewPlayer(int world, int accountId)
        {
            var m = new Player(
                world: world,
                accountId: accountId,
                hp: 50,
                mp: 5,
                str: 12,
                dex: 5,
                @int: 4,
                luk: 4,
                job: Job.BEGINNER,
                level: 1
             );

            return m;
        }


        public static bool Ban(string id, string reason, bool isAccountId)
        {
            using var dbContext = new DBContext();

            if (Regex.IsMatch(id, "/[0-9]{1,3}\\..*"))
            {
                dbContext.Ipbans.Add(new Ipban { Ip = id });
                dbContext.SaveChanges();
                return true;
            }
            int actualAccId = 0;
            if (isAccountId)
            {
                actualAccId = dbContext.Accounts.Where(x => x.Name == id).Select(x => x.Id).FirstOrDefault();

            }
            else
            {
                actualAccId = dbContext.Characters.Where(x => x.Name == id).Select(x => x.AccountId).FirstOrDefault();
            }
            dbContext.Accounts.Where(x => x.Id == actualAccId).ExecuteUpdate(x => x.SetProperty(y => y.Banned, 1).SetProperty(y => y.Banreason, reason));

            return true;
        }


        public static int getAccountIdByName(string name)
        {
            using DBContext dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.AccountId }).FirstOrDefault()?.AccountId ?? -1;
        }

        public static int getIdByName(string name)
        {
            using DBContext dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.Id }).FirstOrDefault()?.Id ?? -1;
        }

        public static string getNameById(int id)
        {
            using DBContext dbContext = new DBContext();
            return dbContext.Characters.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault()!;
        }

        /// <summary>
        /// 角色
        /// </summary>
        /// <param name="charid"></param>
        /// <param name="client"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public static IPlayer? LoadPlayerFromDB(int charid, IClient client, bool login)
        {
            try
            {
                var ret = new Player(client);
                using var dbContext = new DBContext();
                var dbModel = dbContext.Characters.FirstOrDefault(x => x.Id == charid);
                if (dbModel == null)
                {
                    throw new Exception("Loading char failed (not found)");
                }

                Mapper.Map(dbModel, ret);

                ret.Monsterbook.loadCards(dbContext, ret.Id);

                var wserv = Server.getInstance().getWorld(ret.World);

                short sandboxCheck = 0x0;
                foreach (var item in ItemFactory.INVENTORY.loadItems(ret.Id, !login))
                {
                    sandboxCheck |= item.Item.getFlag();

                    ret.Bag[item.Type].addItemFromDB(item.Item);
                    Item itemz = item.Item;
                    if (itemz.getPetId() > -1)
                    {
                        var pet = itemz.getPet();
                        if (pet != null && pet.isSummoned())
                        {
                            ret.addPet(pet);
                        }
                        continue;
                    }

                    InventoryType mit = item.Type;
                    if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                    {
                        Equip equip = (Equip)item.Item;
                        if (equip.getRingId() > -1)
                        {
                            var ring = Ring.loadFromDb(equip.getRingId())!;
                            if (ring != null)
                            {
                                if (item.Type.Equals(InventoryType.EQUIPPED))
                                {
                                    ring.equip();
                                }

                                ret.addPlayerRing(ring);
                            }
                        }
                    }
                }

                if ((sandboxCheck & ItemConstants.SANDBOX) == ItemConstants.SANDBOX)
                {
                    ret.setHasSandboxItem();
                }

                ret.CheckMarriageData();

                NewYearCardRecord.loadPlayerNewYearCards(ret);

                //PreparedStatement ps2, ps3;
                //ResultSet rs2, rs3;

                // Items excluded from pet loot
                var petDataFromDB = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == charid && x.Petid > -1)
                                     let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).ToList()
                                     select new { a.Petid, excluded }).ToList();
                foreach (var item in petDataFromDB)
                {
                    int petId = item.Petid;
                    ret.resetExcluded(petId);

                    foreach (var ex in item.excluded)
                    {
                        ret.addExcluded(petId, ex.Itemid);
                    }
                }

                ret.commitExcludedItems();


                if (login)
                {
                    client.setPlayer(ret);

                    var mapManager = client.getChannelServer().getMapFactory();
                    ret.setMap(mapManager.getMap(ret.Map) ?? mapManager.getMap(MapId.HENESYS));

                    var portal = ret.MapModel.getPortal(ret.InitialSpawnPoint);
                    if (portal == null)
                    {
                        portal = ret.MapModel.getPortal(0)!;
                        ret.InitialSpawnPoint = 0;
                    }
                    ret.setPosition(portal.getPosition());

                    ret.setParty(wserv.getParty(dbModel.Party));

                    int messengerid = dbModel.MessengerId;
                    int position = dbModel.MessengerPosition;
                    if (messengerid > 0 && position < 4 && position > -1)
                    {
                        var messenger = wserv.getMessenger(messengerid);
                        if (messenger != null)
                        {
                            ret.Messenger = messenger;
                        }
                    }
                }


                var trockLocList = dbContext.Trocklocations.Where(x => x.Characterid == ret.Id).Select(x => new { x.Vip, x.Mapid }).Take(15).ToList();

                byte vip = 0;
                byte reg = 0;
                foreach (var item in trockLocList)
                {
                    if (item.Vip == 1)
                    {
                        ret.VipTrockMaps.Add(item.Mapid);
                        vip++;
                    }
                    else
                    {
                        ret.TrockMaps.Add(item.Mapid);
                        reg++;
                    }
                }

                while (vip < 10)
                {
                    ret.VipTrockMaps.Add(MapId.NONE);
                    vip++;
                }
                while (reg < 5)
                {
                    ret.TrockMaps.Add(MapId.NONE);
                    reg++;
                }

                var accountFromDB = dbContext.Accounts.Where(x => x.Id == ret.AccountId).Select(x => new { x.Name, x.Characterslots, x.Language }).FirstOrDefault();
                if (accountFromDB != null)
                {
                    client.setAccountName(accountFromDB.Name);
                    client.setCharacterSlots(accountFromDB.Characterslots);
                    client.setLanguage(accountFromDB.Language);   // thanks Zein for noticing user language not overriding default once player is in-game
                }

                var areaInfoFromDB = dbContext.AreaInfos.Where(x => x.Charid == ret.Id).Select(x => new { x.Area, x.Info }).ToList();
                foreach (var item in areaInfoFromDB)
                {
                    ret.AreaInfo.AddOrUpdate((short)item.Area, item.Info);
                }

                var eventStatsFromDB = dbContext.Eventstats.Where(x => x.Characterid == ret.Id).Select(x => new { x.Name, x.Info }).ToList();
                foreach (var item in eventStatsFromDB)
                {
                    if (item.Name == "rescueGaga")
                    {
                        ret.Events.AddOrUpdate(item.Name, new RescueGaga(item.Info));
                    }
                }

                // Blessing of the Fairy
                var otherCharFromDB = dbContext.Characters.Where(x => x.AccountId == ret.AccountId && x.Id != charid)
                    .OrderByDescending(x => x.Level).Select(x => new { x.Name, x.Level }).FirstOrDefault();
                if (otherCharFromDB != null)
                {
                    ret.Link = new CharacterLink(otherCharFromDB.Name, otherCharFromDB.Level);
                }

                if (login)
                {
                    Dictionary<int, QuestStatus> loadedQuestStatus = new();

                    var statusFromDB = dbContext.Queststatuses.Where(x => x.Characterid == charid).ToList();
                    foreach (var item in statusFromDB)
                    {
                        var q = Quest.getInstance(item.Quest);
                        QuestStatus status = new QuestStatus(q, (QuestStatus.Status)item.Status);
                        long cTime = item.Time;
                        if (cTime > -1)
                        {
                            status.setCompletionTime(cTime * 1000);
                        }

                        long eTime = item.Expires;
                        if (eTime > 0)
                        {
                            status.setExpirationTime(eTime);
                        }

                        status.setForfeited(item.Forfeited);
                        status.setCompleted(item.Completed);
                        ret.Quests.AddOrUpdate(q.getId(), status);
                        loadedQuestStatus.AddOrUpdate(item.Queststatusid, status);
                    }


                    // Quest progress
                    // opportunity for improvement on questprogress/medalmaps calls to DB
                    var questProgressFromDB = dbContext.Questprogresses.Where(x => x.Characterid == charid).ToList();
                    foreach (var item in questProgressFromDB)
                    {
                        var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
                        if (status != null)
                        {
                            status.setProgress(item.Progressid, item.Progress);
                        }
                    }

                    // Medal map visit progress
                    var medalMapFromDB = dbContext.Medalmaps.Where(x => x.Characterid == charid).ToList();
                    foreach (var item in medalMapFromDB)
                    {
                        var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
                        if (status != null)
                        {
                            status.addMedalMap(item.Mapid);
                        }
                    }

                    loadedQuestStatus.Clear();

                    // Skills
                    var skillInfoFromDB = dbContext.Skills.Where(x => x.Characterid == charid).ToList();
                    foreach (var item in skillInfoFromDB)
                    {
                        var pSkill = SkillFactory.getSkill(item.Skillid);
                        if (pSkill != null)  // edit reported by Shavit (=＾● ⋏ ●＾=), thanks Zein for noticing an NPE here
                        {
                            ret.Skills.AddOrUpdate(pSkill, new SkillEntry((sbyte)item.Skilllevel, item.Masterlevel, item.Expiration));
                        }
                    }

                    // Cooldowns (load)
                    var cdFromDB = dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ToList();
                    foreach (var item in cdFromDB)
                    {
                        int skillid = item.SkillId;
                        long length = item.Length;
                        long startTime = item.StartTime;
                        if (skillid != 5221999 && (length + startTime < Server.getInstance().getCurrentTime()))
                        {
                            continue;
                        }
                        ret.giveCoolDowns(skillid, startTime, length);

                    }

                    // Cooldowns (delete)
                    dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ExecuteDelete();

                    // Debuffs (load)
                    #region Playerdiseases
                    Dictionary<Disease, DiseaseExpiration> loadedDiseases = new();
                    var playerDiseaseFromDB = dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ToList();
                    foreach (var item in playerDiseaseFromDB)
                    {
                        Disease disease = Disease.ordinal(item.Disease);
                        if (disease == Disease.NULL)
                        {
                            continue;
                        }

                        int skillid = item.Mobskillid, skilllv = item.Mobskilllv;
                        long length = item.Length;

                        var ms = MobSkillFactory.getMobSkill(MobSkillTypeUtils.from(skillid), skilllv);
                        if (ms != null)
                        {
                            loadedDiseases.AddOrUpdate(disease, new(length, ms));
                        }
                    }

                    dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ExecuteDelete();
                    if (loadedDiseases.Count > 0)
                    {
                        Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(ret.Id, loadedDiseases);
                    }
                    #endregion

                    // Skill macros
                    var dbSkillMacros = dbContext.Skillmacros.Where(x => x.Characterid == ret.Id).OrderBy(x => x.Position).ToList();
                    dbSkillMacros.ForEach(o =>
                    {
                        ret.SkillMacros[o.Position] = new SkillMacro(o.Skill1, o.Skill2, o.Skill3, o.Name, o.Shout, o.Position);
                    });

                    // Key config
                    ret.KeyMap.Clear();
                    var keyMapFromDB = dbContext.Keymaps.Where(x => x.Characterid == ret.Id).Select(x => new { x.Key, x.Type, x.Action }).ToList();
                    foreach (var item in keyMapFromDB)
                    {
                        ret.KeyMap.AddOrUpdate(item.Key, new KeyBinding(item.Type, item.Action));
                    }

                    var savedLocFromDB = dbContext.Savedlocations.Where(x => x.Characterid == ret.Id).Select(x => new { x.Locationtype, x.Map, x.Portal }).ToList();
                    foreach (var item in savedLocFromDB)
                    {
                        ret.SavedLocations[(int)Enum.Parse<SavedLocationType>(item.Locationtype)] = new SavedLocation(item.Map, item.Portal);
                    }

                    // Fame history
                    var now = DateTimeOffset.Now;
                    var fameLogFromDB = dbContext.Famelogs.Where(x => x.Characterid == ret.Id && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();
                    if (fameLogFromDB.Count > 0)
                    {
                        ret.LastFameTime = fameLogFromDB.Max(x => x.When).ToUnixTimeMilliseconds();
                        ret.LastFameCIds = fameLogFromDB.Select(x => x.CharacteridTo).ToList();
                    }

                    ret.BuddyList.LoadFromDb();
                    ret.Storage = wserv.getAccountStorage(ret.AccountId);

                    int startHp = ret.Hp, startMp = ret.Mp;
                    ret.reapplyLocalStats();
                    ret.changeHpMp(startHp, startMp, true);
                    //ret.resetBattleshipHp();
                }

                ret.MountModel = new Mount(ret, ret.Bag[InventoryType.EQUIPPED].getItem(-18)?.getItemId() ?? 0);
                ret.MountModel.setExp(ret.MountExp);
                ret.MountModel.setLevel(ret.MountLevel);
                ret.MountModel.setTiredness(ret.Mounttiredness);
                ret.MountModel.setActive(false);

                // Quickslot key config
                var accKeyMapFromDB = dbContext.Quickslotkeymappeds.Where(x => x.Accountid == ret.getAccountID()).Select(x => (long?)x.Keymap).FirstOrDefault();
                if (accKeyMapFromDB != null)
                {
                    ret.QuickSlotLoaded = LongTool.LongToBytes(accKeyMapFromDB.Value);
                    ret.QuickSlotKeyMapped = new QuickslotBinding(ret.QuickSlotLoaded);
                }

                return ret;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
            return null;
        }


        public static IPlayer? GetPlayerById(int chrId, bool showEquipped = false)
        {
            return GetPlayersById([chrId], showEquipped).FirstOrDefault();
        }
        /// <summary>
        /// 简化版获取角色，用于角色不在线时作为关联数据获取
        /// </summary>
        /// <param name="idList">角色Id</param>
        /// <param name="showEquipped">是否加载穿戴的道具</param>
        /// <returns></returns>
        public static List<IPlayer> GetPlayersById(List<int> idList, bool showEquipped = false)
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.Characters.AsNoTracking().Where(x => idList.Contains(x.Id)).ToList();

            var players = new List<IPlayer>(dataList.Select(Mapper.Map<Player>).ToList());
            if (showEquipped)
            {
                foreach (var ret in players)
                {
                    var equipped = ItemFactory.loadEquippedItems(ret.Id);
                    // players can have no equipped items at all, ofc
                    Inventory inv = ret.Bag[InventoryType.EQUIPPED];
                    foreach (var item in equipped)
                    {
                        inv.addItemFromDB(item);
                    }
                }
            }
            return players;
        }

        public static IPlayer? GetPlayerByName(string name, bool showEquipped = false)
        {
            return GetPlayersByName([name], showEquipped).FirstOrDefault();
        }
        /// <summary>
        /// 简化版获取角色，用于角色不在线时作为关联数据获取
        /// </summary>
        /// <param name="nameList">角色name</param>
        /// <param name="showEquipped">是否加载穿戴的道具</param>
        /// <returns></returns>
        public static List<IPlayer> GetPlayersByName(List<string> nameList, bool showEquipped = false)
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.Characters.AsNoTracking().Where(x => nameList.Contains(x.Name)).ToList();

            var players = new List<IPlayer>(dataList.Select(Mapper.Map<Player>).ToList());
            if (showEquipped)
            {
                foreach (var ret in players)
                {
                    var equipped = ItemFactory.loadEquippedItems(ret.Id);
                    // players can have no equipped items at all, ofc
                    Inventory inv = ret.Bag[InventoryType.EQUIPPED];
                    foreach (var item in equipped)
                    {
                        inv.addItemFromDB(item);
                    }
                }
            }
            return players;
        }

        public static CharacterBaseInfo GetCharacterFromDatabase(string name)
        {
            using DBContext dbContext = new DBContext();
            var ds = dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.Id, x.AccountId, x.Name }).FirstOrDefault();
            if (ds == null)
                throw new BusinessDataNullException();

            return new CharacterBaseInfo(ds.AccountId, ds.Id, ds.Name);
        }

        public static string? checkWorldTransferEligibility(DBContext dbContext, int characterId, int oldWorld, int newWorld)
        {
            if (!YamlConfig.config.server.ALLOW_CASHSHOP_WORLD_TRANSFER)
            {
                return "World transfers disabled.";
            }
            int accountId = -1;
            try
            {
                var charInfoFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.AccountId, x.Level, x.GuildId, x.GuildRank, x.PartnerId, x.FamilyId }).FirstOrDefault();
                if (charInfoFromDB == null)
                    return "Character does not exist.";
                accountId = charInfoFromDB.AccountId;
                if (charInfoFromDB.Level < 20)
                    return "Character is under level 20.";
                if (charInfoFromDB.FamilyId != -1)
                    return "Character is in family.";
                if (charInfoFromDB.PartnerId != 0)
                    return "Character is married.";
                if (charInfoFromDB.GuildId != 0 && charInfoFromDB.GuildRank < 2)
                    return "Character is the leader of a guild.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            try
            {
                var accInfoFromDB = dbContext.Accounts.Where(x => x.Id == accountId).Select(x => new { x.Tempban }).FirstOrDefault();
                if (accInfoFromDB == null)
                    return "Account does not exist.";
                if (accInfoFromDB.Tempban != DateTimeOffset.MinValue && accInfoFromDB.Tempban != DefaultDates.getTempban())
                    return "Account has been banned.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            try
            {
                var rowcount = dbContext.Characters.Where(x => x.AccountId == accountId && x.World == newWorld).Count();

                if (rowcount >= 3) return "Too many characters on destination world.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            return null;
        }

        public static bool doWorldTransfer(DBContext dbContext, int characterId, int oldWorld, int newWorld, int worldTransferId)
        {
            int mesos = 0;
            try
            {
                var mesosFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.Meso }).FirstOrDefault();
                if (mesosFromDB == null)
                {
                    Log.Logger.Warning("Character data invalid for world transfer? chrId {CharacterId}", characterId);
                    return false;
                }
                mesos = mesosFromDB.Meso;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to do world transfer for chrId {CharacterId}", characterId);
                return false;
            }
            try
            {
                dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.World, newWorld)
                    .SetProperty(y => y.Meso, Math.Min(mesos, 1000000))
                    .SetProperty(y => y.GuildId, 0)
                    .SetProperty(y => y.GuildRank, 5));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to update chrId {CharacterId} during world transfer", characterId);
                return false;
            }
            try
            {
                dbContext.Buddies.Where(x => x.CharacterId == characterId || x.BuddyId == characterId).ExecuteDelete();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete buddies for chrId {CharacterId} during world transfer", characterId);
                return false;
            }
            if (worldTransferId != -1)
            {
                try
                {
                    dbContext.Worldtransfers.Where(x => x.Id == worldTransferId).ExecuteUpdate(x => x.SetProperty(y => y.CompletionTime, DateTimeOffset.Now));
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Failed to update world transfer for chrId {CharacterId}", characterId);
                    return false;
                }
            }
            return true;
        }

        public static void doNameChange(int characterId, string oldName, string newName, int nameChangeId)
        { //Don't do this while player is online
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                bool success = doNameChange(dbContext, characterId, oldName, newName, nameChangeId);
                if (!success)
                    dbTrans.Rollback();
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to get DB connection for chr name change");
            }
        }

        public static bool doNameChange(DBContext dbContext, int characterId, string oldName, string newName, int nameChangeId)
        {
            try
            {
                dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.Name, newName));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to perform chr name change in database for chrId {CharacterId}", characterId);
                return false;
            }

            try
            {
                dbContext.Rings.Where(x => x.PartnerName == oldName).ExecuteUpdate(x => x.SetProperty(y => y.PartnerName, newName));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to update rings during chr name change for chrId {CharacterId}", characterId);
                return false;
            }

            /*try (PreparedStatement ps = con.prepareStatement("UPDATE playernpcs SET name = ? WHERE name = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE gifts SET `from` = ? WHERE `from` = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }
            try (PreparedStatement ps = con.prepareStatement("UPDATE dueypackages SET SenderName = ? WHERE SenderName = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE dueypackages SET SenderName = ? WHERE SenderName = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE inventoryitems SET owner = ? WHERE owner = ?")) { //GMS doesn't do this
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE mts_items SET owner = ? WHERE owner = ?")) { //GMS doesn't do this
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE newyear SET sendername = ? WHERE sendername = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE newyear SET receivername = ? WHERE receivername = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE notes SET `to` = ? WHERE `to` = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE notes SET `from` = ? WHERE `from` = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }

            try (PreparedStatement ps = con.prepareStatement("UPDATE nxcode SET retriever = ? WHERE retriever = ?")) {
                ps.setString(1, newName);
                ps.setString(2, oldName);
                ps.executeUpdate();
            } catch(Exception e) { 
                Log.Logger.Error(e.ToString());
                FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
                return false;
            }*/

            if (nameChangeId != -1)
            {
                try
                {
                    dbContext.Namechanges.Where(x => x.Id == nameChangeId).ExecuteUpdate(x => x.SetProperty(y => y.CompletionTime, DateTimeOffset.Now));
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Failed to save chr name change for chrId {NameChangeId}", nameChangeId);
                    return false;
                }
            }
            return true;
        }

        public static void SaveQuickSlotMapped(DBContext dbContext, IPlayer player)
        {
            bool bQuickslotEquals = player.QuickSlotKeyMapped == null
                || (player.QuickSlotLoaded != null && player.QuickSlotKeyMapped.GetKeybindings().SequenceEqual(player.QuickSlotLoaded));
            if (!bQuickslotEquals)
            {
                long nQuickslotKeymapped = LongTool.BytesToLong(player.QuickSlotKeyMapped!.GetKeybindings());
                var m = dbContext.Quickslotkeymappeds.Where(x => x.Accountid == player.AccountId).FirstOrDefault();
                if (m == null)
                {
                    m = new Quickslotkeymapped() { Accountid = player.AccountId, Keymap = nQuickslotKeymapped };
                    dbContext.Quickslotkeymappeds.Add(m);
                }
                else
                {
                    m.Keymap = nQuickslotKeymapped;
                }
                dbContext.SaveChanges();
            }
        }

        public void saveCharToDB(IPlayer player, bool notAutosave)
        {
            lock (player.SaveToDBLock)
            {
                if (!player.IsOnlined)
                {
                    return;
                }

                // log.Debug("Attempting to {SaveMethod} chr {CharacterName}", notAutosave ? "save" : "autosave", player.Name);

                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var entity = dbContext.Characters.FirstOrDefault(x => x.Id == player.getId());
                if (entity == null)
                    throw new BusinessResException();

                try
                {
                    GlobalTools.Mapper.Map(player, entity);

                    if (player.MapModel == null || (player.CashShopModel != null && player.CashShopModel.isOpened()))
                    {
                        entity.Map = player.Map;
                    }
                    else
                    {
                        if (player.MapModel.getForcedReturnId() != 999999999)
                        {
                            entity.Map = player.MapModel.getForcedReturnId();
                        }
                        else
                        {
                            entity.Map = player.getHp() < 1 ? player.MapModel.getReturnMapId() : player.MapModel.getId();
                        }
                    }
                    if (player.MapModel == null || player.MapModel.getId() == 610020000 || player.MapModel.getId() == 610020001)
                    {  // reset to first spawnpoint on those maps
                        entity.Spawnpoint = 0;
                    }
                    else
                    {
                        var closest = player.MapModel.findClosestPlayerSpawnpoint(player.getPosition());
                        if (closest != null)
                        {
                            entity.Spawnpoint = closest.getId();
                        }
                        else
                        {
                            entity.Spawnpoint = 0;
                        }
                    }

                    entity.MessengerId = player.Messenger?.getId() ?? 0;
                    entity.MessengerPosition = player.Messenger == null ? 4 : player.MessengerPosition;

                    entity.MountLevel = player.MountModel?.getLevel() ?? 1;
                    entity.MountExp = player.MountModel?.getExp() ?? 0;
                    entity.Mounttiredness = player.MountModel?.getTiredness() ?? 0;

                    player.Monsterbook.saveCards(dbContext, player.getId());
                    dbContext.SaveChanges();

                    var pets = player.getPets();
                    foreach (var pet in pets)
                    {
                        pet?.saveToDb();
                    }

                    var ignoresPetIds = player.getExcluded().Select(x => x.Key).ToList();
                    dbContext.Petignores.Where(x => ignoresPetIds.Contains(x.Petid)).ExecuteDelete();
                    dbContext.Petignores.AddRange(player.getExcluded().SelectMany(x => x.Value.Select(y => new Petignore() { Petid = x.Key, Itemid = y })).ToList());
                    dbContext.SaveChanges();


                    dbContext.Keymaps.Where(x => x.Characterid == player.getId()).ExecuteDelete();
                    dbContext.Keymaps.AddRange(player.KeyMap.Select(x => new Keymap() { Characterid = player.Id, Key = x.Key, Type = x.Value.getType(), Action = x.Value.getAction() }));
                    dbContext.SaveChanges();

                    // No quickslots, or no change.
                    CharacterManager.SaveQuickSlotMapped(dbContext, player);


                    dbContext.Skillmacros.Where(x => x.Characterid == player.Id).ExecuteDelete();
                    for (int i = 0; i < 5; i++)
                    {
                        var macro = player.SkillMacros[i];
                        if (macro != null)
                        {
                            dbContext.Skillmacros.Add(new Skillmacro(player.Id, (sbyte)i, macro.Skill1, macro.Skill2, macro.Skill3, macro.Name, (sbyte)macro.Shout));
                        }
                    }
                    dbContext.SaveChanges();

                    List<ItemInventoryType> itemsWithType = new();
                    foreach (Inventory iv in player.Bag.GetValues())
                    {
                        foreach (Item item in iv.list())
                        {
                            itemsWithType.Add(new(item, iv.getType()));
                        }
                    }

                    ItemFactory.INVENTORY.saveItems(itemsWithType, player.Id, dbContext);

                    var characterSkills = dbContext.Skills.Where(x => x.Characterid == player.getId()).ToList();
                    foreach (var skill in player.Skills)
                    {
                        var dbSkill = characterSkills.FirstOrDefault(x => x.Skillid == skill.Key.getId());
                        if (dbSkill == null)
                        {
                            dbSkill = new SkillEntity() { Characterid = player.getId(), Skillid = skill.Key.getId() };
                            dbContext.Skills.Add(dbSkill);
                        }
                        dbSkill.Skilllevel = skill.Value.skillevel;
                        dbSkill.Masterlevel = skill.Value.masterlevel;
                        dbSkill.Expiration = skill.Value.expiration;
                    }
                    dbContext.SaveChanges();

                    dbContext.Savedlocations.Where(x => x.Characterid == player.getId()).ExecuteDelete();
                    dbContext.Savedlocations.AddRange(
                        Enum.GetValues<SavedLocationType>()
                        .Where(x => player.SavedLocations[(int)x] != null)
                        .Select(x => new Savedlocation(player.SavedLocations[(int)x]!.getMapId(), player.SavedLocations[(int)x]!.getPortal()))
                        );
                    dbContext.SaveChanges();

                    dbContext.Trocklocations.Where(x => x.Characterid == player.getId()).ExecuteDelete();
                    for (int i = 0; i < player.getTrockSize(); i++)
                    {
                        if (player.TrockMaps[i] != 999999999)
                        {
                            dbContext.Trocklocations.Add(new Trocklocation(player.getId(), player.TrockMaps[i], 0));
                        }
                    }

                    for (int i = 0; i < player.getVipTrockSize(); i++)
                    {
                        if (player.VipTrockMaps[i] != 999999999)
                        {
                            dbContext.Trocklocations.Add(new Trocklocation(player.getId(), player.VipTrockMaps[i], 1));
                        }
                    }
                    dbContext.SaveChanges();

                    dbContext.Buddies.Where(x => x.CharacterId == player.getId() && x.Pending == 0).ExecuteDelete();
                    foreach (var entry in player.BuddyList.getBuddies())
                    {
                        if (entry.Visible)
                        {
                            dbContext.Buddies.Add(new Buddy(player.getId(), entry.getCharacterId(), 0, entry.Group));
                        }
                    }
                    dbContext.SaveChanges();

                    dbContext.AreaInfos.Where(x => x.Charid == player.getId()).ExecuteDelete();
                    dbContext.AreaInfos.AddRange(player.AreaInfo.Select(x => new AreaInfo(player.getId(), x.Key, x.Value)));
                    dbContext.SaveChanges();

                    dbContext.Eventstats.Where(x => x.Characterid == player.getId()).ExecuteDelete();
                    dbContext.Eventstats.AddRange(player.Events.Select(x => new Eventstat(player.getId(), x.Key, x.Value.getInfo())));
                    dbContext.SaveChanges();

                    CharacterManager.deleteQuestProgressWhereCharacterId(dbContext, player.Id);


                    foreach (var qs in player.getQuests())
                    {
                        var questStatus = new Queststatus(player.getId(), qs.getQuest().getId(), (int)qs.getStatus(), (int)(qs.getCompletionTime() / 1000), qs.getExpirationTime(),
                           qs.getForfeited(), qs.getCompleted());
                        dbContext.Queststatuses.Add(questStatus);
                        dbContext.SaveChanges();

                        foreach (int mob in qs.getProgress().Keys)
                        {
                            dbContext.Questprogresses.Add(new Questprogress(player.getId(), questStatus.Queststatusid, mob, qs.getProgress(mob)));
                        }
                        foreach (var item in qs.getMedalMaps())
                        {
                            dbContext.Medalmaps.Add(new Medalmap(player.getId(), questStatus.Queststatusid, item));
                        }
                    }
                    dbContext.SaveChanges();

                    var familyEntry = player.getFamilyEntry(); //save family rep
                    if (familyEntry != null)
                    {
                        if (familyEntry.saveReputation(dbContext))
                            familyEntry.savedSuccessfully();
                        var senior = familyEntry.getSenior();
                        if (senior != null && senior.getChr() == null)
                        { //only save for offline family members
                            if (senior.saveReputation(dbContext))
                                senior.savedSuccessfully();

                            senior = senior.getSenior(); //save one level up as well
                            if (senior != null && senior.getChr() == null)
                            {
                                if (senior.saveReputation(dbContext))
                                    senior.savedSuccessfully();
                            }
                        }

                    }

                    if (player.CashShopModel != null)
                    {
                        player.CashShopModel.save(dbContext);
                    }

                    if (player.Storage != null && player.Storage.IsChanged)
                    {
                        player.Storage.saveToDB(dbContext);
                    }
                    dbTrans.Commit();

                }
                catch (Exception e)
                {
                    // log.Error(e, "Error saving chr {CharacterName}, level: {Level}, job: {JobId}", player.Name, player.Level, player.JobId);
                }
            }
        }

        public static void deleteGuild(IPlayer player)
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            dbContext.Characters.Where(x => x.GuildId == player.GuildId).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, 0).SetProperty(y => y.GuildRank, 5));
            dbContext.Guilds.Where(x => x.GuildId == player.GuildId).ExecuteDelete();
            dbTrans.Commit();
            player.GuildId = 0;
            player.GuildRank = 5;
        }
    }
}
