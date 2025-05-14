using Application.Core.Datas;
using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.Core.Game.Players.Models;
using Application.Core.Managers;
using Application.Shared;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using client.inventory;
using client.keybind;
using client.newyear;
using client;
using constants.id;
using constants.inventory;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using Serilog;
using server.events;
using server.life;
using server.quest;
using tools;

namespace Application.Core.Channel.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IPlayer? GetPlayerByCharacerValueObject(IChannelClient c, CharacterValueObject? o)
        {
            if (o == null)
                return null;

            var player = _mapper.Map<Player>(o.CharacterEntity);
            player.Monsterbook = _mapper.Map<MonsterBook>(o.MonsterBookEntity);
            return player;
        }

        /// <summary>
        /// 角色
        /// </summary>
        /// <param name="charid"></param>
        /// <param name="client"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public static IPlayer? LoadPlayerFromDB(int charid, IChannelClient client, bool login)
        {
            try
            {
                var ret = new Player(client);
                using var dbContext = new DBContext();
                var dbModel = dbContext.Characters.FirstOrDefault(x => x.Id == charid) ?? throw new BusinessCharacterNotFoundException(charid);

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
                            var ring = RingManager.LoadFromDb(equip.getRingId())!;
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
                    client.SetPlayer(ret);

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


                ret.PlayerTrockLocation.LoadData(dbContext);

                var accountFromDB = dbContext.Accounts.Where(x => x.Id == ret.AccountId).AsNoTracking().FirstOrDefault();
                if (accountFromDB != null)
                {
                    client.SetAccount(accountFromDB);
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
                        status?.setProgress(item.Progressid, item.Progress);
                    }

                    // Medal map visit progress
                    var medalMapFromDB = dbContext.Medalmaps.Where(x => x.Characterid == charid).ToList();
                    foreach (var item in medalMapFromDB)
                    {
                        var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
                        status?.addMedalMap(item.Mapid);
                    }

                    loadedQuestStatus.Clear();

                    // Skills
                    ret.Skills.LoadData(dbContext);

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
                    ret.KeyMap.LoadData(dbContext);

                    ret.SavedLocations.LoadData(dbContext);

                    // Fame history
                    var now = DateTimeOffset.Now;
                    var fameLogFromDB = dbContext.Famelogs.Where(x => x.Characterid == ret.Id && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();
                    if (fameLogFromDB.Count > 0)
                    {
                        ret.LastFameTime = fameLogFromDB.Max(x => x.When).ToUnixTimeMilliseconds();
                        ret.LastFameCIds = fameLogFromDB.Select(x => x.CharacteridTo).ToList();
                    }

                    ret.BuddyList.LoadFromDb(dbContext);
                    ret.Storage = wserv.getAccountStorage(ret.AccountId);

                    ret.UpdateLocalStats(true);
                    //ret.resetBattleshipHp();
                }

                var mountItem = ret.Bag[InventoryType.EQUIPPED].getItem(EquipSlot.Mount);
                if (mountItem != null)
                {
                    var mountModel = new Mount(ret, mountItem.getItemId());
                    mountModel.setExp(ret.MountExp);
                    mountModel.setLevel(ret.MountLevel);
                    mountModel.setTiredness(ret.Mounttiredness);
                    mountModel.setActive(false);
                    ret.SetMount(mountModel);
                }

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
    }
}
