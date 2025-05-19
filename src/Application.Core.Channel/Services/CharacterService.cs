using Application.Core.Datas;
using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Managers;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Items;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using client;
using client.inventory;
using client.keybind;
using constants.id;
using constants.inventory;
using net.server;
using Serilog;
using server;
using server.events;
using server.maps;
using server.quest;

namespace Application.Core.Channel.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IPlayer? Serialize(IChannelClient c, CharacterValueObject? o)
        {
            if (o == null)
                return null;

            var player = new Player(c);
            _mapper.Map(o.Character, player);

            player.Monsterbook = new MonsterBook(o.MonsterBooks);

            player.Link = o.Link == null ? null : new CharacterLink(o.Link.Name, o.Link.Level);

            short sandboxCheck = 0x0;
            foreach (var item in o.InventoryItems)
            {
                sandboxCheck |= item.Flag;

                InventoryType mit = item.InventoryType.getByType();
                if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                {
                    var equipObj = _mapper.Map<Equip>(item);
                    player.Bag[mit.ordinal()].addItemFromDB(equipObj);
                    if (item.EquipInfo!.RingInfo != null)
                    {
                        var ring = _mapper.Map<Ring>(item.EquipInfo.RingInfo);
                        if (ring != null)
                        {
                            if (item.InventoryType.Equals(InventoryType.EQUIPPED))
                            {
                                ring.equip();
                            }

                            player.addPlayerRing(ring);
                        }
                    }
                }
                else
                {
                    var itemObj = _mapper.Map<Item>(item);
                    player.Bag[item.InventoryType].addItemFromDB(itemObj);
                    if (itemObj.getPetId() > -1)
                    {
                        var pet = itemObj.getPet();
                        if (pet != null && pet.isSummoned())
                        {
                            player.addPet(pet);
                        }
                        continue;
                    }
                }
            }

            if ((sandboxCheck & ItemConstants.SANDBOX) == ItemConstants.SANDBOX)
            {
                player.setHasSandboxItem();
            }
            player.CheckMarriageData();

            player.Storage = _mapper.Map<Storage>(o.StorageInfo);

            c.SetAccount(o.Account);
            c.SetPlayer(player);

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
                player.QuickSlotLoaded = o.QuickSlot.QuickSlotLoaded;
                player.QuickSlotKeyMapped = new QuickslotBinding(o.QuickSlot.QuickSlotLoaded);
            }

            player.BuddyList.LoadFromDb(o.BuddyList);
            player.UpdateLocalStats(true);
            return player;
        }

        public CharacterValueObject Deserialize(IPlayer player)
        {
            return new CharacterValueObject()
            {
                Character = _mapper.Map<CharacterDto>(player),
                Areas = player.AreaInfo.Select(x => new AreaDto() { Area = x.Key, Info = x.Value}).ToArray(),
                MonsterBooks = player.Monsterbook.ToDto(player.Id),
                SavedLocations = player.SavedLocations.ToDto(),
            };
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
