using Application.Core.Datas;
using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.EF;
using Application.Shared;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using client;
using client.inventory;
using client.keybind;
using net.server;
using Serilog;
using server;
using server.events;
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

        private Item MapToItem(ItemDto itemDto)
        {
            InventoryType mit = itemDto.InventoryType.getByType();
            if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                return _mapper.Map<Equip>(itemDto);
            else
                return _mapper.Map<Item>(itemDto);
        }

        public IPlayer? Serialize(IChannelClient c, CharacterValueObject? o)
        {
            if (o == null)
                return null;

            var player = new Player(c);
            _mapper.Map(o.Character, player);

            player.Monsterbook.LoadData(o.MonsterBooks);
            player.CashShopModel.LoadData(o.CashShop.NxCredit, o.CashShop.MaplePoint, o.CashShop.NxPrepaid, o.CashShop.WishItems.ToList(), o.CashShop.Items.Select(MapToItem).ToList());

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


                foreach (var progress in item.Progress)
                {
                    status.setProgress(item.Id, progress.Progress);
                }
                foreach (var medalMap in item.MedalMap)
                {
                    status.addMedalMap(medalMap.MapId);
                }
            }

            player.Skills.LoadData(o.Skills);

            foreach (var item in o.CoolDowns)
            {
                int skillid = item.SkillId;
                long length = item.Length;
                long startTime = item.StartTime;
                if (skillid != 5221999 && (length + startTime < c.CurrentServer.getCurrentTime()))
                {
                    continue;
                }
                player.giveCoolDowns(skillid, startTime, length);
            }

            // TODO Disease

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
                var bytes = LongTool.LongToBytes(o.QuickSlot.LongValue);
                player.QuickSlotLoaded = bytes;
                player.QuickSlotKeyMapped = new QuickslotBinding(bytes);
            }

            player.BuddyList.LoadFromDb(o.BuddyList);
            player.UpdateLocalStats(true);
            return player;
        }

        public PlayerSaveDto Deserialize(IPlayer player)
        {
            List<QuestStatusDto> questStatusList = new();
            foreach (var qs in player.getQuests())
            {
                var questDto = new QuestStatusDto()
                {
                    Characterid = player.Id,
                    Expires = qs.getExpirationTime(),
                    Status = (int)qs.getStatus(),
                    Time = (int)(qs.getCompletionTime() / 1000),
                    QuestId = qs.getQuestID(),
                    Completed = qs.getCompleted(),
                    Forfeited = qs.getForfeited(),
                    MedalMap = qs.getMedalMaps().Select(x => new MedalMapDto { MapId = x }).ToArray(),
                    Progress = qs.getProgress().Select(x => new QuestProgressDto { ProgressId = x.Key, Progress = x.Value }).ToArray()
                };
                questStatusList.Add(questDto);
            }

            bool hasQuickSlotChanged = player.QuickSlotLoaded == null
                ? player.QuickSlotKeyMapped != null
                : !player.QuickSlotKeyMapped!.GetKeybindings().SequenceEqual(player.QuickSlotLoaded);

            var quickSlotDto = hasQuickSlotChanged
                ? new QuickSlotDto()
                {
                    LongValue = LongTool.BytesToLong(player.QuickSlotKeyMapped!.GetKeybindings()),
                }
                : null;

            var cashShopDto = new CashShopDto()
            {
                Items = _mapper.Map<ItemDto[]>(player.CashShopModel.getInventory(), opt =>
                {
                    opt.Items["Type"] = player.CashShopModel.Factory.getValue();
                }),
                WishItems = player.CashShopModel.getWishList().ToArray(),
                FactoryType = player.CashShopModel.Factory.getValue(),
                NxCredit = player.CashShopModel.NxCredit,
                NxPrepaid = player.CashShopModel.NxPrepaid,
                MaplePoint = player.CashShopModel.MaplePoint
            };

            var playerDto = _mapper.Map<CharacterDto>(player);
            if (player.MapModel == null || (player.CashShopModel != null && player.CashShopModel.isOpened()))
            {
                playerDto.Map = player.Map;
            }
            else
            {
                if (player.MapModel.getForcedReturnId() != MapId.NONE)
                {
                    playerDto.Map = player.MapModel.getForcedReturnId();
                }
                else
                {
                    playerDto.Map = player.HP < 1 ? player.MapModel.getReturnMapId() : player.MapModel.getId();
                }
            }
            if (player.MapModel == null || player.MapModel.getId() == 610020000 || player.MapModel.getId() == 610020001)
            {
                // reset to first spawnpoint on those maps
                playerDto.Spawnpoint = 0;
            }
            else
            {
                var closest = player.MapModel.findClosestPlayerSpawnpoint(player.getPosition());
                if (closest != null)
                {
                    playerDto.Spawnpoint = closest.getId();
                }
                else
                {
                    playerDto.Spawnpoint = 0;
                }
            }

            #region inventory mapping
            var d = player.Bag.GetValues().SelectMany(x => _mapper.Map<List<ItemDto>>(x.list(), opt =>
            {
                opt.Items["InventoryType"] = (int)x.getType();
                opt.Items["Type"] = 1;
            })).ToArray();

            var storageDto = _mapper.Map<StorageDto>(player.Storage);
            storageDto.Items = _mapper.Map<ItemDto[]>(player.Storage.getItems(), opt =>
            {
                opt.Items["Type"] = ItemFactory.STORAGE.getValue();
            });
            #endregion

            return new PlayerSaveDto()
            {
                Channel = player.Channel,
                Character = playerDto,
                Areas = player.AreaInfo.Select(x => new AreaDto() { Area = x.Key, Info = x.Value }).ToArray(),
                MonsterBooks = player.Monsterbook.ToDto(),
                SavedLocations = player.SavedLocations.ToDto(),
                Events = player.Events.Select(x => new EventDto { Characterid = player.Id, Name = x.Key, Info = x.Value.getInfo() }).ToArray(),
                Skills = player.Skills.ToDto(),
                TrockLocations = player.PlayerTrockLocation.ToDto(),
                PetIgnores = player.getExcluded().Select(x => new PetIgnoreDto { PetId = x.Key, ExcludedItems = x.Value.ToArray() }).ToArray(),
                KeyMaps = player.KeyMap.ToDto(),
                QuestStatuses = questStatusList.ToArray(),
                SkillMacros = _mapper.Map<SkillMacroDto[]>(player.SkillMacros.Where(x => x != null)),
                BuddyList = player.BuddyList.ToDto(),
                StorageInfo = storageDto,
                InventoryItems = _mapper.Map<ItemDto[]>(d),
                CashShop = cashShopDto,
                QuickSlot = quickSlotDto,
                CoolDowns = _mapper.Map<CoolDownDto[]>(player.getAllCooldowns())
            };
        }

        public PlayerBuffSaveDto DeserializeBuff(IPlayer player)
        {
            return new PlayerBuffSaveDto
            {
                Buffs = player.getAllBuffs().Select(x => new BuffDto
                {
                    IsSkill = x.effect.isSkill(),
                    SkillLevel = x.effect.SkillLevel,
                    SourceId = x.effect.getSourceId(),
                    UsedTime = x.usedTime,
                }).ToArray(),
                Diseases = player.getAllDiseases().Select(x => new DiseaseDto
                {
                    DiseaseOrdinal = x.Key.ordinal(),
                    LeftTime = x.Value.LeftTime,
                    MobSkillId = x.Value.MobSkill.getId().type.getId(),
                    MobSkillLevel = x.Value.MobSkill.getId().level
                }).ToArray()
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
