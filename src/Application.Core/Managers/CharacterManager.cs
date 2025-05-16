using Application.Core.Game.Items;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Core.scripting.npc;
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
using server;
using server.events;
using server.life;
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


        public static void deleteQuestProgressWhereCharacterId(DBContext dbContext, int cid)
        {
            dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();

            dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();

            dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();
        }

        public static void SavePlayerQuestInfo(DBContext dbContext, IPlayer player)
        {
            foreach (var qs in player.getQuests())
            {
                var questStatus = new QuestStatusEntity(player.Id, qs.getQuestID(), (int)qs.getStatus(), (int)(qs.getCompletionTime() / 1000), qs.getExpirationTime(),
                   qs.getForfeited(), qs.getCompleted());
                dbContext.Queststatuses.Add(questStatus);
                dbContext.SaveChanges();

                foreach (int mob in qs.getProgress().Keys)
                {
                    dbContext.Questprogresses.Add(new Questprogress(player.Id, questStatus.Queststatusid, mob, qs.getProgress(mob)));
                }
                foreach (var item in qs.getMedalMaps())
                {
                    dbContext.Medalmaps.Add(new Medalmap(player.Id, questStatus.Queststatusid, item));
                }
            }
            dbContext.SaveChanges();
        }


        public static bool Ban(string id, string reason, bool isAccountId)
        {
            using var dbContext = new DBContext();

            // IP的正则？
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

        ///// <summary>
        ///// 角色
        ///// </summary>
        ///// <param name="charid"></param>
        ///// <param name="client"></param>
        ///// <param name="login"></param>
        ///// <returns></returns>
        //public static IPlayer? LoadPlayerFromDB(int charid, IChannelClient client, bool login)
        //{
        //    try
        //    {
        //        var ret = new Player(client);
        //        using var dbContext = new DBContext();
        //        var dbModel = dbContext.Characters.FirstOrDefault(x => x.Id == charid) ?? throw new BusinessCharacterNotFoundException(charid);

        //        Mapper.Map(dbModel, ret);

        //        ret.Monsterbook.loadCards(dbContext, ret.Id);

        //        var wserv = Server.getInstance().getWorld(ret.World);

        //        short sandboxCheck = 0x0;
        //        foreach (var item in ItemFactory.INVENTORY.loadItems(ret.Id, !login))
        //        {
        //            sandboxCheck |= item.Item.getFlag();

        //            ret.Bag[item.Type].addItemFromDB(item.Item);
        //            Item itemz = item.Item;
        //            if (itemz.getPetId() > -1)
        //            {
        //                var pet = itemz.getPet();
        //                if (pet != null && pet.isSummoned())
        //                {
        //                    ret.addPet(pet);
        //                }
        //                continue;
        //            }

        //            InventoryType mit = item.Type;
        //            if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
        //            {
        //                Equip equip = (Equip)item.Item;
        //                if (equip.getRingId() > -1)
        //                {
        //                    var ring = RingManager.LoadFromDb(equip.getRingId())!;
        //                    if (ring != null)
        //                    {
        //                        if (item.Type.Equals(InventoryType.EQUIPPED))
        //                        {
        //                            ring.equip();
        //                        }

        //                        ret.addPlayerRing(ring);
        //                    }
        //                }
        //            }
        //        }

        //        if ((sandboxCheck & ItemConstants.SANDBOX) == ItemConstants.SANDBOX)
        //        {
        //            ret.setHasSandboxItem();
        //        }

        //        ret.CheckMarriageData();

        //        NewYearCardRecord.loadPlayerNewYearCards(ret);

        //        //PreparedStatement ps2, ps3;
        //        //ResultSet rs2, rs3;

        //        // Items excluded from pet loot
        //        var petDataFromDB = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == charid && x.Petid > -1)
        //                             let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).ToList()
        //                             select new { a.Petid, excluded }).ToList();
        //        foreach (var item in petDataFromDB)
        //        {
        //            int petId = item.Petid;
        //            ret.resetExcluded(petId);

        //            foreach (var ex in item.excluded)
        //            {
        //                ret.addExcluded(petId, ex.Itemid);
        //            }
        //        }

        //        ret.commitExcludedItems();


        //        if (login)
        //        {
        //            client.SetPlayer(ret);

        //            var mapManager = client.getChannelServer().getMapFactory();
        //            ret.setMap(mapManager.getMap(ret.Map) ?? mapManager.getMap(MapId.HENESYS));

        //            var portal = ret.MapModel.getPortal(ret.InitialSpawnPoint);
        //            if (portal == null)
        //            {
        //                portal = ret.MapModel.getPortal(0)!;
        //                ret.InitialSpawnPoint = 0;
        //            }
        //            ret.setPosition(portal.getPosition());

        //            ret.setParty(wserv.getParty(dbModel.Party));

        //            int messengerid = dbModel.MessengerId;
        //            int position = dbModel.MessengerPosition;
        //            if (messengerid > 0 && position < 4 && position > -1)
        //            {
        //                var messenger = wserv.getMessenger(messengerid);
        //                if (messenger != null)
        //                {
        //                    ret.Messenger = messenger;
        //                }
        //            }
        //        }


        //        ret.PlayerTrockLocation.LoadData(dbContext);

        //        var accountFromDB = dbContext.Accounts.Where(x => x.Id == ret.AccountId).AsNoTracking().FirstOrDefault();
        //        if (accountFromDB != null)
        //        {
        //            client.SetAccount(accountFromDB);
        //        }

        //        var areaInfoFromDB = dbContext.AreaInfos.Where(x => x.Charid == ret.Id).Select(x => new { x.Area, x.Info }).ToList();
        //        foreach (var item in areaInfoFromDB)
        //        {
        //            ret.AreaInfo.AddOrUpdate((short)item.Area, item.Info);
        //        }

        //        var eventStatsFromDB = dbContext.Eventstats.Where(x => x.Characterid == ret.Id).Select(x => new { x.Name, x.Info }).ToList();
        //        foreach (var item in eventStatsFromDB)
        //        {
        //            if (item.Name == "rescueGaga")
        //            {
        //                ret.Events.AddOrUpdate(item.Name, new RescueGaga(item.Info));
        //            }
        //        }

        //        // Blessing of the Fairy
        //        var otherCharFromDB = dbContext.Characters.Where(x => x.AccountId == ret.AccountId && x.Id != charid)
        //            .OrderByDescending(x => x.Level).Select(x => new { x.Name, x.Level }).FirstOrDefault();
        //        if (otherCharFromDB != null)
        //        {
        //            ret.Link = new CharacterLink(otherCharFromDB.Name, otherCharFromDB.Level);
        //        }

        //        if (login)
        //        {
        //            Dictionary<int, QuestStatus> loadedQuestStatus = new();

        //            var statusFromDB = dbContext.Queststatuses.Where(x => x.Characterid == charid).ToList();
        //            foreach (var item in statusFromDB)
        //            {
        //                var q = Quest.getInstance(item.Quest);
        //                QuestStatus status = new QuestStatus(q, (QuestStatus.Status)item.Status);
        //                long cTime = item.Time;
        //                if (cTime > -1)
        //                {
        //                    status.setCompletionTime(cTime * 1000);
        //                }

        //                long eTime = item.Expires;
        //                if (eTime > 0)
        //                {
        //                    status.setExpirationTime(eTime);
        //                }

        //                status.setForfeited(item.Forfeited);
        //                status.setCompleted(item.Completed);
        //                ret.Quests.AddOrUpdate(q.getId(), status);
        //                loadedQuestStatus.AddOrUpdate(item.Queststatusid, status);
        //            }


        //            // Quest progress
        //            // opportunity for improvement on questprogress/medalmaps calls to DB
        //            var questProgressFromDB = dbContext.Questprogresses.Where(x => x.Characterid == charid).ToList();
        //            foreach (var item in questProgressFromDB)
        //            {
        //                var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
        //                status?.setProgress(item.Progressid, item.Progress);
        //            }

        //            // Medal map visit progress
        //            var medalMapFromDB = dbContext.Medalmaps.Where(x => x.Characterid == charid).ToList();
        //            foreach (var item in medalMapFromDB)
        //            {
        //                var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
        //                status?.addMedalMap(item.Mapid);
        //            }

        //            loadedQuestStatus.Clear();

        //            // Skills
        //            ret.Skills.LoadData(dbContext);

        //            // Cooldowns (load)
        //            var cdFromDB = dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ToList();
        //            foreach (var item in cdFromDB)
        //            {
        //                int skillid = item.SkillId;
        //                long length = item.Length;
        //                long startTime = item.StartTime;
        //                if (skillid != 5221999 && (length + startTime < Server.getInstance().getCurrentTime()))
        //                {
        //                    continue;
        //                }
        //                ret.giveCoolDowns(skillid, startTime, length);

        //            }

        //            // Cooldowns (delete)
        //            dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ExecuteDelete();

        //            // Debuffs (load)
        //            #region Playerdiseases
        //            Dictionary<Disease, DiseaseExpiration> loadedDiseases = new();
        //            var playerDiseaseFromDB = dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ToList();
        //            foreach (var item in playerDiseaseFromDB)
        //            {
        //                Disease disease = Disease.ordinal(item.Disease);
        //                if (disease == Disease.NULL)
        //                {
        //                    continue;
        //                }

        //                int skillid = item.Mobskillid, skilllv = item.Mobskilllv;
        //                long length = item.Length;

        //                var ms = MobSkillFactory.getMobSkill(MobSkillTypeUtils.from(skillid), skilllv);
        //                if (ms != null)
        //                {
        //                    loadedDiseases.AddOrUpdate(disease, new(length, ms));
        //                }
        //            }

        //            dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ExecuteDelete();
        //            if (loadedDiseases.Count > 0)
        //            {
        //                Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(ret.Id, loadedDiseases);
        //            }
        //            #endregion

        //            // Skill macros
        //            var dbSkillMacros = dbContext.Skillmacros.Where(x => x.Characterid == ret.Id).OrderBy(x => x.Position).ToList();
        //            dbSkillMacros.ForEach(o =>
        //            {
        //                ret.SkillMacros[o.Position] = new SkillMacro(o.Skill1, o.Skill2, o.Skill3, o.Name, o.Shout, o.Position);
        //            });

        //            // Key config
        //            ret.KeyMap.LoadData(dbContext);

        //            ret.SavedLocations.LoadData(dbContext);

        //            // Fame history
        //            var now = DateTimeOffset.Now;
        //            var fameLogFromDB = dbContext.Famelogs.Where(x => x.Characterid == ret.Id && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();
        //            if (fameLogFromDB.Count > 0)
        //            {
        //                ret.LastFameTime = fameLogFromDB.Max(x => x.When).ToUnixTimeMilliseconds();
        //                ret.LastFameCIds = fameLogFromDB.Select(x => x.CharacteridTo).ToList();
        //            }

        //            ret.BuddyList.LoadFromDb(dbContext);
        //            ret.Storage = wserv.getAccountStorage(ret.AccountId);

        //            ret.UpdateLocalStats(true);
        //            //ret.resetBattleshipHp();
        //        }

        //        var mountItem = ret.Bag[InventoryType.EQUIPPED].getItem(EquipSlot.Mount);
        //        if (mountItem != null)
        //        {
        //            var mountModel = new Mount(ret, mountItem.getItemId());
        //            mountModel.setExp(ret.MountExp);
        //            mountModel.setLevel(ret.MountLevel);
        //            mountModel.setTiredness(ret.Mounttiredness);
        //            mountModel.setActive(false);
        //            ret.SetMount(mountModel);
        //        }

        //        // Quickslot key config
        //        var accKeyMapFromDB = dbContext.Quickslotkeymappeds.Where(x => x.Accountid == ret.getAccountID()).Select(x => (long?)x.Keymap).FirstOrDefault();
        //        if (accKeyMapFromDB != null)
        //        {
        //            ret.QuickSlotLoaded = LongTool.LongToBytes(accKeyMapFromDB.Value);
        //            ret.QuickSlotKeyMapped = new QuickslotBinding(ret.QuickSlotLoaded);
        //        }

        //        return ret;
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Logger.Error(e.ToString());
        //    }
        //    return null;
        //}


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
            return ds == null ? throw new BusinessCharacterNotFoundException(name) : new CharacterBaseInfo(ds.AccountId, ds.Id, ds.Name);
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
                var entity = dbContext.Characters.FirstOrDefault(x => x.Id == player.getId()) ?? throw new BusinessCharacterNotFoundException(player.getId());
                try
                {
                    GlobalTools.Mapper.Map(player, entity);

                    if (player.MapModel == null || (player.CashShopModel != null && player.CashShopModel.isOpened()))
                    {
                        entity.Map = player.Map;
                    }
                    else
                    {
                        if (player.MapModel.getForcedReturnId() != MapId.NONE)
                        {
                            entity.Map = player.MapModel.getForcedReturnId();
                        }
                        else
                        {
                            entity.Map = player.HP < 1 ? player.MapModel.getReturnMapId() : player.MapModel.getId();
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

                    var pets = player.getPets();
                    foreach (var pet in pets)
                    {
                        pet?.saveToDb();
                    }

                    var ignoresPetIds = player.getExcluded().Select(x => x.Key).ToList();
                    dbContext.Petignores.Where(x => ignoresPetIds.Contains(x.Petid)).ExecuteDelete();
                    dbContext.Petignores.AddRange(player.getExcluded().SelectMany(x => x.Value.Select(y => new Petignore() { Petid = x.Key, Itemid = y })).ToList());
                    dbContext.SaveChanges();


                    player.KeyMap.SaveData(dbContext);

                    // No quickslots, or no change.
                    CharacterManager.SaveQuickSlotMapped(dbContext, player);


                    dbContext.Skillmacros.Where(x => x.Characterid == player.Id).ExecuteDelete();
                    for (int i = 0; i < 5; i++)
                    {
                        var macro = player.SkillMacros[i];
                        if (macro != null)
                        {
                            dbContext.Skillmacros.Add(new SkillMacroEntity(player.Id, (sbyte)i, macro.Skill1, macro.Skill2, macro.Skill3, macro.Name, (sbyte)macro.Shout));
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

                    player.Skills.SaveData(dbContext);

                    player.SavedLocations.SaveData(dbContext);

                    player.PlayerTrockLocation.SaveData(dbContext);

                    player.BuddyList.Save(dbContext);

                    dbContext.AreaInfos.Where(x => x.Charid == player.getId()).ExecuteDelete();
                    dbContext.AreaInfos.AddRange(player.AreaInfo.Select(x => new AreaInfo(player.getId(), x.Key, x.Value)));
                    dbContext.SaveChanges();

                    dbContext.Eventstats.Where(x => x.Characterid == player.getId()).ExecuteDelete();
                    dbContext.Eventstats.AddRange(player.Events.Select(x => new Eventstat(player.getId(), x.Key, x.Value.getInfo())));
                    dbContext.SaveChanges();

                    deleteQuestProgressWhereCharacterId(dbContext, player.Id);

                    SavePlayerQuestInfo(dbContext, player);

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

                    player.CashShopModel?.save(dbContext);

                    if (player.Storage != null && player.Storage.IsChanged)
                    {
                        player.Storage.saveToDB(dbContext);
                    }
                    dbTrans.Commit();

                }
                catch (Exception e)
                {
                    player.Log.Error(e, "Error saving chr {CharacterName}, level: {Level}, job: {JobId}", player.Name, player.Level, player.JobId);
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

        public static void ShowAllEquipFeatures(IPlayer player)
        {
            string showMsg = "";

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (Item item in player.getInventory(InventoryType.EQUIPPED).list())
            {
                Equip nEquip = (Equip)item;
                var itemName = ii.getName(nEquip.getItemId());
                if (itemName == null)
                {
                    continue;
                }

                showMsg += ItemManager.ShowEquipFeatures(nEquip);
            }

            if (showMsg.Count() > 0)
            {
                player.showHint("#ePLAYER EQUIPMENTS:#n\r\n\r\n" + showMsg, 400);
            }
        }

    }
}
