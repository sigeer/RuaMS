using Application.Core.Game.Items;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Shared.Models;
using AutoMapper;
using client;
using client.inventory;
using client.keybind;
using net.server;
using server;
using server.events;
using server.quest;
using System.Runtime.InteropServices;
using tools;
using ZstdSharp.Unsafe;

namespace Application.Core.Servers.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IPlayer? Serialize(IChannelClient c, Dto.PlayerGetterDto? o)
        {
            if (o == null)
                return null;

            var player = new Player(c);
            _mapper.Map(o.Character, player);

            player.Monsterbook.LoadData(o.MonsterBooks);

            List<Item> cashItems;
            switch (player.CashShopModel.Factory)
            {
                case Shared.Items.ItemType.CashExplorer:
                    cashItems = _mapper.Map<List<Item>>(o.AccountGame.CashExplorerItems);
                    break;
                case Shared.Items.ItemType.CashCygnus:
                    cashItems = _mapper.Map<List<Item>>(o.AccountGame.CashCygnusItems);
                    break;
                case Shared.Items.ItemType.CashAran:
                    cashItems = _mapper.Map<List<Item>>(o.AccountGame.CashAranItems);
                    break;
                default:
                    cashItems = _mapper.Map<List<Item>>(o.AccountGame.CashOverallItems);
                    break;
            }
            player.CashShopModel.LoadData(
                o.AccountGame.NxCredit,
                o.AccountGame.MaplePoint,
                o.AccountGame.NxPrepaid,
                o.WishItems.ToList(),
                cashItems
                );

            player.Link = o.Link == null ? null : new CharacterLink(o.Link.Name, o.Link.Level);

            int sandboxCheck = 0x0;
            foreach (var item in o.InventoryItems)
            {
                sandboxCheck |= item.Flag;

                InventoryType mit = item.InventoryType.GetByType();
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
                    if (itemObj is Pet petObj)
                    {
                        if (petObj.isSummoned())
                        {
                            player.addPet(petObj);
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

            player.Storage = _mapper.Map<Storage>(o.AccountGame.Storage);
            player.Storage.LoadItems(_mapper.Map<Item[]>(o.AccountGame.StorageItems));

            c.SetAccount(_mapper.Map<AccountCtrl>(o.Account));
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
            player.setParty(c.CurrentServer.TeamManager.GetParty(player.Party));

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
                var petId = item.PetId;
                player.resetExcluded(petId);

                foreach (var itemId in item.ExcludedItems)
                {
                    player.addExcluded(petId, itemId);
                }
            }
            player.commitExcludedItems();

            player.PlayerTrockLocation.LoadData(o.TrockLocations.ToArray());
            player.AreaInfo = o.Areas.ToDictionary(x => (short)x.Area, x => x.Info);
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
                    status.setProgress(progress.ProgressId, progress.Progress);
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
            if (o.AccountGame.QuickSlot != null)
            {
                var bytes = LongTool.LongToBytes(o.AccountGame.QuickSlot.LongValue);
                player.QuickSlotLoaded = bytes;
                player.QuickSlotKeyMapped = new QuickslotBinding(bytes);
            }

            player.BuddyList.LoadFromDb(o.BuddyList);
            player.UpdateLocalStats(true);
            return player;
        }

        public Dto.PlayerSaveDto Deserialize(IPlayer player)
        {
            List<Dto.QuestStatusDto> questStatusList = new();
            foreach (var qs in player.getQuests())
            {
                var questDto = new Dto.QuestStatusDto()
                {
                    Characterid = player.Id,
                    Expires = qs.getExpirationTime(),
                    Status = (int)qs.getStatus(),
                    Time = (int)(qs.getCompletionTime() / 1000),
                    QuestId = qs.getQuestID(),
                    Completed = qs.getCompleted(),
                    Forfeited = qs.getForfeited(),
                };
                questDto.MedalMap.AddRange(qs.getMedalMaps().Select(x => new Dto.MedalMapDto { MapId = x }));
                questDto.Progress.AddRange(qs.getProgress().Select(x => new Dto.QuestProgressDto { ProgressId = x.Key, Progress = x.Value }));
                questStatusList.Add(questDto);
            }

            bool hasQuickSlotChanged = player.QuickSlotLoaded == null
                ? player.QuickSlotKeyMapped != null
                : !player.QuickSlotKeyMapped!.GetKeybindings().SequenceEqual(player.QuickSlotLoaded);

            var quickSlotDto = hasQuickSlotChanged
                ? new Dto.QuickSlotDto()
                {
                    LongValue = LongTool.BytesToLong(player.QuickSlotKeyMapped!.GetKeybindings()),
                }
                : null;


            var playerDto = _mapper.Map<Dto.CharacterDto>(player);
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
            var itemType = ItemFactory.INVENTORY.getValue();
            var d = player.Bag.GetValues().SelectMany(x => _mapper.Map<Dto.ItemDto[]>(x.list(), opt =>
            {
                opt.Items["InventoryType"] = (int)x.getType();
                opt.Items["Type"] = itemType;
            })).ToArray();
            #endregion

            var data = new Dto.PlayerSaveDto()
            {
                Channel = player.Channel,
                Character = playerDto
            };
            data.Areas.AddRange(player.AreaInfo.Select(x => new Dto.AreaDto() { Area = x.Key, Info = x.Value }));
            data.MonsterBooks.AddRange(player.Monsterbook.ToDto());
            data.SavedLocations.AddRange(player.SavedLocations.ToDto());
            data.Events.AddRange(player.Events.Select(x => new Dto.EventDto { Characterid = player.Id, Name = x.Key, Info = x.Value.getInfo() }));
            data.Skills.AddRange(player.Skills.ToDto());
            data.SkillMacros.AddRange(_mapper.Map<Dto.SkillMacroDto[]>(player.SkillMacros.Where(x => x != null)));
            data.TrockLocations.AddRange(player.PlayerTrockLocation.ToDto());
            data.KeyMaps.AddRange(player.KeyMap.ToDto());
            data.QuestStatuses.AddRange(questStatusList);
            data.PetIgnores.AddRange(player.getExcluded().Select(x =>
            {
                var m = new Dto.PetIgnoreDto { PetId = x.Key };
                m.ExcludedItems.AddRange(x.Value);
                return m;
            }));
            data.WishItems.AddRange(player.CashShopModel.getWishList());
            data.BuddyList.AddRange(player.BuddyList.ToDto());
            data.CoolDowns.AddRange(_mapper.Map<Dto.CoolDownDto[]>(player.getAllCooldowns()));
            data.InventoryItems.AddRange(d);
            data.AccountGame = new Dto.AccountGameDto()
            {
                NxCredit = player.CashShopModel?.NxCredit ?? 0,
                NxPrepaid = player.CashShopModel?.NxPrepaid ?? 0,
                MaplePoint = player.CashShopModel?.MaplePoint ?? 0,
                Id = playerDto.AccountId,
                Storage = new Dto.StorageDto
                {
                    Accountid = playerDto.AccountId,
                    Meso = player.Storage.getMeso(),
                    Slots = player.Storage.getSlots()
                },
                QuickSlot = quickSlotDto,
            };
            data.AccountGame.StorageItems.AddRange(_mapper.Map<Dto.ItemDto[]>(player.Storage.getItems(), opt =>
            {
                opt.Items["Type"] = ItemFactory.STORAGE.getValue();
            }));
            var cashFactoryType = player.CashShopModel.Factory;
            if (cashFactoryType == ItemType.CashOverall)
                data.AccountGame.CashOverallItems.AddRange(_mapper.Map<Dto.ItemDto[]>(player.CashShopModel.getInventory(), opt =>
                {
                    opt.Items["Type"] = ItemFactory.CASH_OVERALL.getValue();
                }));
            if (cashFactoryType == ItemType.CashAran)
                data.AccountGame.CashAranItems.AddRange(_mapper.Map<Dto.ItemDto[]>(player.CashShopModel.getInventory(), opt =>
                {
                    opt.Items["Type"] = ItemFactory.CASH_ARAN.getValue();
                }));
            if (cashFactoryType == ItemType.CashExplorer)
                data.AccountGame.CashExplorerItems.AddRange(_mapper.Map<Dto.ItemDto[]>(player.CashShopModel.getInventory(), opt =>
                {
                    opt.Items["Type"] = ItemFactory.CASH_EXPLORER.getValue();
                }));
            if (cashFactoryType == ItemType.CashCygnus)
                data.AccountGame.CashCygnusItems.AddRange(_mapper.Map<Dto.ItemDto[]>(player.CashShopModel.getInventory(), opt =>
                {
                    opt.Items["Type"] = ItemFactory.CASH_CYGNUS.getValue();
                }));
            return data;
        }

        //public PlayerSaveDto DeserializeCashShop(IPlayer player)
        //{
        //    var cashShopItems = player.CashShopModel.getInventory();
        //    var cashShopDto  = new CashShopDto()
        //    {
        //        Items = _mapper.Map<ItemDto[]>(cashShopItems, opt =>
        //        {
        //            opt.Items["Type"] = player.CashShopModel.Factory.getValue();
        //        }),
        //        WishItems = player.CashShopModel.getWishList().ToArray(),
        //        FactoryType = player.CashShopModel.Factory.getValue(),
        //        NxCredit = player.CashShopModel.NxCredit,
        //        NxPrepaid = player.CashShopModel.NxPrepaid,
        //        MaplePoint = player.CashShopModel.MaplePoint
        //    };

        //    var d = player.Bag.GetValues().SelectMany(x => _mapper.Map<List<ItemDto>>(x.list(), opt =>
        //    {
        //        opt.Items["InventoryType"] = (int)x.getType();
        //        opt.Items["Type"] = 1;
        //    })).ToArray();

        //    return new PlayerSaveDto
        //    {
        //        CashShop = cashShopDto,
        //        InventoryItems = _mapper.Map<ItemDto[]>(d),
        //    };
        //}

        public Dto.PlayerBuffSaveDto DeserializeBuff(IPlayer player)
        {
            var data = new Dto.PlayerBuffSaveDto();
            data.Buffs.AddRange(player.getAllBuffs().Select(x => new Dto.BuffDto
            {
                IsSkill = x.effect.isSkill(),
                SkillLevel = x.effect.SkillLevel,
                SourceId = x.effect.getSourceId(),
                UsedTime = x.usedTime,
            }));
            data.Diseases.AddRange(player.getAllDiseases().Select(x => new Dto.DiseaseDto
            {
                DiseaseOrdinal = x.Key.ordinal(),
                LeftTime = x.Value.LeftTime,
                MobSkillId = x.Value.MobSkill.getId().type.getId(),
                MobSkillLevel = x.Value.MobSkill.getId().level
            }));
            return data;
        }
        /// <summary>
        /// 创建角色使用
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Dto.NewPlayerSaveDto DeserializeNew(IPlayer player)
        {
            var playerDto = _mapper.Map<Dto.CharacterDto>(player);
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
            var itemType = ItemFactory.INVENTORY.getValue();
            var d = player.Bag.GetValues().SelectMany(x => _mapper.Map<Dto.ItemDto[]>(x.list(), opt =>
            {
                opt.Items["InventoryType"] = (int)x.getType();
                opt.Items["Type"] = itemType;
            })).ToArray();

            #endregion

            var data = new Dto.NewPlayerSaveDto()
            {
                Character = playerDto
            };
            data.InventoryItems.AddRange(d);
            data.Events.AddRange(player.Events.Select(x => new Dto.EventDto { Characterid = player.Id, Name = x.Key, Info = x.Value.getInfo() }));
            data.Skills.AddRange(player.Skills.ToDto());
            data.KeyMaps.AddRange(player.KeyMap.ToDto());

            return data;
        }

        public Dto.CharacterMapChangeDto DeserializeMap(IPlayer player)
        {
            return new Dto.CharacterMapChangeDto { Id = player.Id, Map = player.Map };
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
