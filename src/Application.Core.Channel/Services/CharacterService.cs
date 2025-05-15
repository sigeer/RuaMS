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
using Org.BouncyCastle.Ocsp;
using Application.EF;
using Microsoft.EntityFrameworkCore;
using Application.Core.Game.Skills;
using Application.EF.Entities;
using Application.Core.Game.Relation;

namespace Application.Core.Channel.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IPlayer? GeneratePlayerByDto(IChannelClient c, CharacterValueObject? o)
        {
            if (o == null)
                return null;

            var player = _mapper.Map<Player>(o.Character);
            player.Monsterbook = new MonsterBook(o.MonsterBooks);

            short sandboxCheck = 0x0;
            foreach (var item in o.Items.Where(x => x.Type == ItemFactory.INVENTORY.getValue()))
            {
                sandboxCheck |= item.Flag;

                var itemObj = _mapper.Map<Item>(item);
                player.Bag[item.Type].addItemFromDB(itemObj);
                if (itemObj.getPetId() > -1)
                {
                    var pet = itemObj.getPet();
                    if (pet != null && pet.isSummoned())
                    {
                        player.addPet(pet);
                    }
                    continue;
                }

                InventoryType mit = item.InventoryType.getByType();
                if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                {
                    Equip equip = (Equip)itemObj;
                    if (equip.getRingId() > -1)
                    {
                        var ring = RingManager.LoadFromDb(equip.getRingId())!;
                        if (ring != null)
                        {
                            if (item.Type.Equals(InventoryType.EQUIPPED))
                            {
                                ring.equip();
                            }

                            player.addPlayerRing(ring);
                        }
                    }
                }
            }

            if ((sandboxCheck & ItemConstants.SANDBOX) == ItemConstants.SANDBOX)
            {
                player.setHasSandboxItem();
            }
            player.CheckMarriageData();

            player.Storage = new server.Storage(player.AccountId, o.StorageInfo, _mapper.Map<Item[]>(o.Items.Where(x => x.Type == ItemFactory.STORAGE.getValue())));

            var mapManager = c.CurrentServer.getMapFactory();
            player.setMap(mapManager.getMap(player.Map) ?? mapManager.getMap(MapId.HENESYS));

            var portal = player.MapModel.getPortal(player.InitialSpawnPoint);
            if (portal == null)
            {
                portal = player.MapModel.getPortal(0)!;
                player.InitialSpawnPoint = 0;
            }
            player.setPosition(portal.getPosition());

            var wserv = Server.getInstance().getWorld(0);
            player.setParty(wserv.getParty(player.Party));

            int messengerid = player.MessengerId;
            int position = player.MessengerPosition;
            if (messengerid > 0 && position < 4 && position > -1)
            {
                var messenger = wserv.getMessenger(messengerid);
                if (messenger != null)
                {
                    player.Messenger = messenger;
                }
            }

            foreach (var item in o.PetIgnores)
            {
                int petId = item.PetId;
                player.resetExcluded(petId);

                foreach (var itemId in item.ExcludedItems)
                {
                    player.addExcluded(petId, itemId);
                }
            }
            player.commitExcludedItems();

            player.PlayerTrockLocation.LoadData(o.TrockLocations);
            player.AreaInfo = o.Areas.ToDictionary(x => x.Area, x => x.Info);
            player.Events = o.Events.ToDictionary(x => x.Name, x => new RescueGaga(x.Info) as Events);

            Dictionary<int, QuestStatus> loadedQuestStatus = new();

            var statusFromDB = o.QuestStatuses;
            foreach (var item in statusFromDB)
            {
                var q = Quest.getInstance(item.QuestId);
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
                player.Quests.AddOrUpdate(q.getId(), status);
                loadedQuestStatus.AddOrUpdate(item.Id, status);
            }


            foreach (var item in o.QuestProgresses)
            {
                var status = loadedQuestStatus.GetValueOrDefault(item.QuestStatusId);
                status?.setProgress(item.Id, item.Progress);
            }

            foreach (var item in o.MedalMaps)
            {
                var status = loadedQuestStatus.GetValueOrDefault(item.QuestStatusId);
                status?.addMedalMap(item.MapId);
            }


            player.Skills.LoadData(o.Skills);

            foreach (var item in o.CoolDowns)
            {
                int skillid = item.SkillId;
                long length = item.Length;
                long startTime = item.StartTime;
                if (skillid != 5221999 && (length + startTime < Server.getInstance().getCurrentTime()))
                {
                    continue;
                }
                player.giveCoolDowns(skillid, startTime, length);

            }

            foreach (var item in o.SkillMacros)
            {
                player.SkillMacros[item.Position] = new SkillMacro(item.Skill1, item.Skill2, item.Skill3, item.Name, item.Shout, item.Position);
            }

            player.KeyMap.LoadData(o.KeyMaps);
            player.SavedLocations.LoadData(o.SavedLocations);

            player.LastFameTime = o.FameRecord.LastUpdateTime;
            player.LastFameCIds = o.FameRecord.ChararacterIds.ToList();

            player.UpdateLocalStats(true);

            var mountItem = player.Bag[InventoryType.EQUIPPED].getItem(EquipSlot.Mount);
            if (mountItem != null)
            {
                var mountModel = new Mount(player, mountItem.getItemId());
                mountModel.setExp(player.MountExp);
                mountModel.setLevel(player.MountLevel);
                mountModel.setTiredness(player.Mounttiredness);
                mountModel.setActive(false);
                player.SetMount(mountModel);
            }

            // Quickslot key config
            if (o.QuickSlot != null)
            {
                player.QuickSlotLoaded = LongTool.LongToBytes(o.QuickSlot.Value);
                player.QuickSlotKeyMapped = new QuickslotBinding(o.QuickSlot.QuickSlotLoaded);
            }

            player.BuddyList.LoadFromDb(o.BuddyList);

            c.SetAccount(o.Account);
            c.SetPlayer(player);
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

                var wserv = Server.getInstance().getWorld(ret.World);

                // Blessing of the Fairy
                //var otherCharFromDB = dbContext.Characters.Where(x => x.AccountId == ret.AccountId && x.Id != charid)
                //    .OrderByDescending(x => x.Level).Select(x => new { x.Name, x.Level }).FirstOrDefault();
                //if (otherCharFromDB != null)
                //{
                //    ret.Link = new CharacterLink(otherCharFromDB.Name, otherCharFromDB.Level);
                //}

                if (login)
                {

                    // Debuffs (load)
                    //#region Playerdiseases
                    //Dictionary<Disease, DiseaseExpiration> loadedDiseases = new();
                    //var playerDiseaseFromDB = dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ToList();
                    //foreach (var item in playerDiseaseFromDB)
                    //{
                    //    Disease disease = Disease.ordinal(item.Disease);
                    //    if (disease == Disease.NULL)
                    //    {
                    //        continue;
                    //    }

                    //    int skillid = item.Mobskillid, skilllv = item.Mobskilllv;
                    //    long length = item.Length;

                    //    var ms = MobSkillFactory.getMobSkill(MobSkillTypeUtils.from(skillid), skilllv);
                    //    if (ms != null)
                    //    {
                    //        loadedDiseases.AddOrUpdate(disease, new(length, ms));
                    //    }
                    //}

                    //dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ExecuteDelete();
                    //if (loadedDiseases.Count > 0)
                    //{
                    //    Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(ret.Id, loadedDiseases);
                    //}
                    //#endregion

                    // Fame history


                    //ret.resetBattleshipHp();
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
