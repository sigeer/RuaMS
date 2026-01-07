using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Models;
using Application.Core.ServerTransports;
using Application.Shared.Events;
using AutoMapper;
using client;
using client.creator;
using client.inventory;
using client.keybind;
using ExpeditionProto;
using net.server;
using net.server.guild;
using server;
using server.events;
using server.life;
using server.quest;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel.Services
{
    public class DataService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;
        Dictionary<int, List<LifeProto.PLifeDto>> _plifeCache;
        public DataService(IMapper mapper, IChannelServerTransport transport, WorldChannelServer server)
        {
            _mapper = mapper;
            _transport = transport;
            _server = server;
            _plifeCache = new();
        }

        public SyncProto.PlayerGetterDto? GetPlayerData(int channelId, string clientSession, int cid)
        {
            return _transport.GetPlayerData(clientSession, cid);
        }

        public async Task SaveChar(Player player, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown)
        {
            var dto = Deserialize(player);
            if (trigger == SyncCharacterTrigger.Logoff)
            {
                dto.Channel = 0;
            }
            if (trigger == SyncCharacterTrigger.PreEnterChannel || trigger == SyncCharacterTrigger.EnterCashShop)
                await _transport.SyncPlayer(dto, trigger); // 切换服务器时会马上请求数据，批量保存存在延迟可能有问题
            else
                _server.BatchSyncPlayerManager.Enqueue(dto);
        }

        public void BatchSyncChar(List<Player> playerList, bool saveDB = false)
        {
            List<SyncProto.PlayerSaveDto> list = [];
            foreach (var player in playerList)
            {
                list.Add(Deserialize(player));
            }
            _transport.BatchSyncPlayer(list, saveDB);
        }

        public Player? Serialize(IChannelClient c, SyncProto.PlayerGetterDto o)
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
                    if (equipObj.Ring != null && mit.Equals(InventoryType.EQUIPPED))
                    {
                        equipObj.Ring.equip();
                    }

                    player.addPlayerRing(equipObj.Ring);
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

            player.Storage = new Storage(player,
                o.AccountGame.Storage.OwnerId, (byte)o.AccountGame.Storage.Slots, o.AccountGame.Storage.Meso,
                _mapper.Map<Item[]>(o.AccountGame.Storage.Items));
            player.GachaponStorage = new(player, o.GachaponStorage.Meso, _mapper.Map<Item[]>(o.GachaponStorage.Items));

            c.SetAccount(_mapper.Map<AccountCtrl>(o.Account));
            c.SetPlayer(player);

            var mapManager = c.CurrentServer.getMapFactory();
            player.setMap(mapManager.getMap(player.Map) ?? mapManager.getMap(MapId.HENESYS));

            player.InitialSpawnPoint = o.Character.Spawnpoint;
            var portal = player.MapModel.getPortal(player.InitialSpawnPoint);
            if (portal == null)
            {
                portal = player.MapModel.getPortal(0)!;
                player.InitialSpawnPoint = 0;
            }
            player.setPosition(portal.getPosition());

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

            player.PlayerTrockLocation.LoadData(o.TrockLocations);
            player.AreaInfo = o.Areas.ToDictionary(x => (short)x.Area, x => x.Info);
            player.Events = o.Events.ToDictionary(x => x.Name, x => new RescueGaga(x.Info) as server.events.Events);

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
            player.QuestExpirations = o.RunningTimerQuests.ToDictionary(x => Quest.getInstance(x.QuestId), x => x.ExpiredTime);

            player.Skills.LoadData(o.Skills);

            foreach (var item in o.CoolDowns)
            {
                int skillid = item.SkillId;
                long length = item.Length;
                long startTime = item.StartTime;
                if (skillid != 5221999 && (length + startTime < c.CurrentServerContainer.getCurrentTime()))
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

            player.FameLogs = _mapper.Map<List<FameLogObject>>(o.FameLogs);

            foreach (var card in o.NewYearCards)
            {
                player.addNewYearRecord(_mapper.Map<NewYearCardObject>(card));
            }

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

            player.BuddyList.LoadFromRemote(_mapper.Map<BuddyCharacter[]>(o.BuddyList));
            player.UpdateLocalStats(true);
            return player;
        }

        private SyncProto.PlayerSaveDto Deserialize(Player player)
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

            var data = new SyncProto.PlayerSaveDto()
            {
                Channel = player.Channel,
                Character = playerDto
            };
            data.FameLogs.AddRange(_mapper.Map<Dto.FameLogRecordDto[]>(player.FameLogs));
            data.Areas.AddRange(player.AreaInfo.Select(x => new Dto.AreaDto() { Area = x.Key, Info = x.Value }));
            data.MonsterBooks.AddRange(player.Monsterbook.ToDto());
            data.SavedLocations.AddRange(player.SavedLocations.ToDto());
            data.Events.AddRange(player.Events.Select(x => new Dto.EventDto { Characterid = player.Id, Name = x.Key, Info = x.Value.getInfo() }));
            data.Skills.AddRange(player.Skills.ToDto());
            data.SkillMacros.AddRange(_mapper.Map<Dto.SkillMacroDto[]>(player.SkillMacros.Where(x => x != null)));
            data.TrockLocations.AddRange(player.PlayerTrockLocation.ToDto());
            data.KeyMaps.AddRange(player.KeyMap.ToDto());
            data.QuestStatuses.AddRange(questStatusList);
            data.RunningTimerQuests.AddRange(player.QuestExpirations.Select(x => new SyncProto.PlayerTimerQuestDto { QuestId = x.Key.getId(), ExpiredTime = x.Value }));
            data.PetIgnores.AddRange(player.getExcluded().Select(x =>
            {
                var m = new Dto.PetIgnoreDto { PetId = x.Key };
                m.ExcludedItems.AddRange(x.Value);
                return m;
            }));
            data.WishItems.AddRange(player.CashShopModel.getWishList());
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
                    OwnerId = playerDto.AccountId,
                    Meso = player.Storage.Meso,
                    Slots = player.Storage.Slots,
                },
                QuickSlot = quickSlotDto,
            };
            data.AccountGame.Storage.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(player.Storage.GetItems(), opt =>
            {
                opt.Items["Type"] = ItemFactory.STORAGE.getValue();
            }));
            data.GachaponStorage = new Dto.StorageDto { Meso = player.GachaponStorage.Meso };
            data.GachaponStorage.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(player.GachaponStorage.GetItems(), opt =>
            {
                opt.Items["Type"] = ItemFactory.ExtraStorage_Gachapon.getValue();
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

        public async Task CompleteLogin(Player chr, SyncProto.PlayerGetterDto o)
        {
            await _transport.SetPlayerOnlined(chr.Id, chr.ActualChannel);

            if (o.LoginInfo.IsNewCommer)
            {
                chr.setLoginTime(_server.GetCurrentTimeDateTimeOffSet());
            }

            if (o.Guild != null)
            {
                chr.sendPacket(GuildPackets.ShowGuildInfo(o.Guild));

                chr.SetGuildSnapshot(o.Guild);
            }

            if (o.Alliance != null)
            {
                chr.sendPacket(GuildPackets.UpdateAllianceInfo(o.Alliance));
                chr.sendPacket(GuildPackets.allianceNotice(o.Alliance.AllianceId, o.Alliance.Notice));

                chr.SetAllianceSnapshot(o.Alliance);
            }

            _server.RemoteCallService.RunEventAfterLogin(chr, o.RemoteCallList);
        }

        //public PlayerSaveDto DeserializeCashShop(Player player)
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

        public SyncProto.PlayerBuffDto DeserializeBuff(Player player)
        {
            var data = new SyncProto.PlayerBuffDto();
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
        public CreatorProto.NewPlayerSaveDto DeserializeNew(Player player)
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

            var data = new CreatorProto.NewPlayerSaveDto()
            {
                Character = playerDto
            };
            data.InventoryItems.AddRange(d);
            data.Events.AddRange(player.Events.Select(x => new Dto.EventDto { Characterid = player.Id, Name = x.Key, Info = x.Value.getInfo() }));
            data.Skills.AddRange(player.Skills.ToDto());
            data.KeyMaps.AddRange(player.KeyMap.ToDto());

            return data;
        }

        public void SaveBuff(Player player)
        {
            _transport.SendBuffObject(player.getId(), DeserializeBuff(player));
        }

        private List<KeyValuePair<long, PlayerBuffValueHolder>> getLocalStartTimes(List<PlayerBuffValueHolder> lpbvl)
        {
            long curtime = _server.getCurrentTime();
            return lpbvl.Select(x => new KeyValuePair<long, PlayerBuffValueHolder>(curtime - x.usedTime, x)).OrderBy(x => x.Key).ToList();
        }
        public void RecoverCharacterBuff(Player player)
        {
            var buffdto = _transport.GetBuffObject(player.Id);
            var buffs = buffdto.Buffs.Select(x => new PlayerBuffValueHolder(x.UsedTime,
                x.IsSkill ? SkillFactory.GetSkillTrust(x.SourceId).getEffect(x.SkillLevel) : ItemInformationProvider.getInstance().getItemEffect(x.SourceId)!)).ToList();

            var timedBuffs = getLocalStartTimes(buffs);
            player.silentGiveBuffs(timedBuffs);

            var diseases = buffdto.Diseases.ToDictionary(
                x => Disease.ordinal(x.DiseaseOrdinal),
                x => new DiseaseExpiration(x.LeftTime, MobSkillFactory.getMobSkillOrThrow((MobSkillType)x.MobSkillId, x.MobSkillLevel)));

            player.silentApplyDiseases(diseases);

            foreach (var e in diseases)
            {
                var debuff = Collections.singletonList(new KeyValuePair<Disease, int>(e.Key, e.Value.MobSkill.getX()));
                player.sendPacket(PacketCreator.giveDebuff(debuff, e.Value.MobSkill));
            }
        }

        public async Task CreatePLife(Player chr, int lifeId, string lifeType, int mobTime = -1)
        {
            if (lifeType == LifeType.Monster)
            {
                var mob = LifeFactory.Instance.getMonster(lifeId);
                if (mob == null || string.IsNullOrEmpty(mob.getName()) || mob.getName().Equals("MISSINGNO"))
                {
                    chr.dropMessage("You have entered an invalid mob id.");
                    return;
                }
            }

            if (lifeType == LifeType.NPC)
            {
                var npc = LifeFactory.Instance.getNPC(lifeId);
                if (npc == null || string.IsNullOrEmpty(npc.getName()) || npc.getName().Equals("MISSINGNO"))
                {
                    chr.dropMessage("You have entered an invalid npc id.");
                    return;
                }
            }

            int mapId = chr.getMapId();
            var checkpos = chr.getMap().getGroundBelow(chr.getPosition());
            int xpos = checkpos.X;
            int ypos = checkpos.Y;
            int fh = chr.getMap().Footholds.FindBelowFoothold(checkpos)!.getId();

            await _transport.SendCreatePLife(new LifeProto.CreatePLifeRequest
            {
                MasterId = chr.Id,
                Data = new LifeProto.PLifeDto
                {
                    LifeId = lifeId,
                    Cy = ypos,
                    MapId = mapId,
                    Type = lifeType,
                    X = xpos,
                    Y = ypos,
                    Fh = fh,
                    Mobtime = mobTime,
                    Rx0 = xpos + 50,
                    Rx1 = xpos - 50,
                    Team = -1,
                }
            });
        }

        public void OnPLifeCreated(LifeProto.CreatePLifeRequest data)
        {
            Player? chr = null;
            foreach (var ch in _server.Servers.Values)
            {
                chr ??= ch.Players.getCharacterById(data.MasterId);
                if (ch.getMapFactory().isMapLoaded(data.Data.MapId))
                {
                    var map = ch.getMapFactory().getMap(data.Data.MapId);
                    if (data.Data.Type == LifeType.NPC)
                    {
                        var npc = LifeFactory.Instance.getNPC(data.Data.LifeId);
                        if (npc != null && npc.getName() == "MISSINGNO")
                        {
                            npc.setPosition(new Point(data.Data.X, data.Data.Y));
                            npc.setCy(data.Data.Cy);
                            npc.setRx0(data.Data.Rx0);
                            npc.setRx1(data.Data.Rx1);
                            npc.setFh(data.Data.Fh);

                            map.addMapObject(npc);
                            map.broadcastMessage(PacketCreator.spawnNPC(npc));

                        }
                    }
                    else if (data.Data.Type == LifeType.Monster)
                    {
                        var mob = LifeFactory.Instance.getMonsterStats(data.Data.LifeId);
                        if (mob != null && !mob.Stats.getName().Equals("MISSINGNO"))
                        {
                            map.addMonsterSpawn(data.Data.LifeId, new Point(data.Data.X, data.Data.Y), 
                                data.Data.Cy, data.Data.F, data.Data.Fh, data.Data.Rx0, data.Data.Rx1, data.Data.Mobtime, data.Data.Hide > 0, data.Data.Team);
                        }
                    }
                }
            }

            if (chr != null)
            {
                if (data.Data.Type == LifeType.NPC)
                {
                    chr.yellowMessage("Pnpc created.");
                }
                if (data.Data.Type == LifeType.Monster)
                {
                    chr.yellowMessage("Pmob created.");
                }
            }

            LoadAllPLife();
        }

        public async Task RemovePLife(Player chr, string lifeType, int lifeId = -1)
        {
            var pos = chr.getPosition();
            await _transport.SendRemovePLife(new LifeProto.RemovePLifeRequest { LifeId = lifeId, LifeType = lifeType, MapId = chr.getMapId(), MasterId = chr.Id, PosX = pos.X, PosY = pos.Y });
        }

        public void OnPLifeRemoved(LifeProto.RemovePLifeResponse res)
        {
            Player? chr = null;
            foreach (var ch in _server.Servers.Values)
            {
                chr ??= ch.Players.getCharacterById(res.MasterId);
                foreach (var data in res.RemovedItems)
                {
                    if (ch.getMapFactory().isMapLoaded(data.MapId))
                    {
                        var map = ch.getMapFactory().getMap(data.MapId);
                        if (data.Type == LifeType.NPC)
                        {
                            map.destroyNPC(data.LifeId);
                        }
                        else if (data.Type == LifeType.Monster)
                        {
                            map.removeMonsterSpawn(data.LifeId, data.X, data.Y);
                        }
                    }
                }
            }

            if (chr != null)
            {
                if (res.LifeType == LifeType.NPC)
                {
                    chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pNPC placements.");
                }
                if (res.LifeType == LifeType.Monster)
                {
                    chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pmob placements.");
                }
            }

            LoadAllPLife();
        }

        public void LoadAllPLife()
        {
            _plifeCache = _transport.RequestPLifeByMapId(new LifeProto.GetPLifeByMapIdRequest()).List.GroupBy(x => x.MapId).ToDictionary(x => x.Key, x => x.ToList());
        }

        internal List<LifeProto.PLifeDto> LoadPLife(int mapId)
        {
            return _plifeCache.GetValueOrDefault(mapId, []);
        }

        private Dictionary<int, List<DropEntry>> reactorDropData = new();
        public void LoadAllReactorDrops()
        {
            var allItems = _transport.RequestAllReactorDrops();
            reactorDropData = allItems.Items.GroupBy(x => x.DropperId).ToDictionary(x => x.Key, x => _mapper.Map<List<DropEntry>>(x.ToArray()));
        }

        public void ClearReactorDrops()
        {
            reactorDropData.Clear();
        }

        public List<DropEntry> GetReactorDrops(int reactorId)
        {
            return reactorDropData.GetValueOrDefault(reactorId) ?? [];
        }


        public CreatorProto.CreateCharResponseDto CreatePlayer(CreatorProto.CreateCharRequestDto request)
        {
            var code = CharacterFactory.GetNoviceCreator(request.Type, this)
                .createCharacter(request.AccountId, request.Name, request.Face, request.Hair, request.SkinColor, request.Top, request.Bottom, request.Shoes, request.Weapon, request.Gender);
            return new CreatorProto.CreateCharResponseDto { Code = code };
        }

        public int SendNewPlayer(Player player)
        {
            var dto = DeserializeNew(player);
            return _transport.SendNewPlayer(dto).Code;
        }

        public int CreatePlayer(IChannelClient client, int type, string name, int face, int hair, int skin, int gender, int improveSp)
        {
            var checkResult = _transport.CreatePlayerCheck(new CreatorProto.CreateCharCheckRequest { AccountId = client.AccountId, Name = name }).Code;
            if (checkResult != CreateCharResult.Success)
                return checkResult;

            return CharacterFactory.GetVeteranCreator(type, this)
                .createCharacter(client.AccountId, name, face, hair, skin, gender, improveSp);
        }

        public QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            var res = new QueryChannelExpedtionResponse();
            foreach (var channel in _server.Servers.Values)
            {
                var item = new ExpeditionProto.ChannelExpeditionDto() { Channel = channel.getId() };

                var expeds = channel.getExpeditions();
                foreach (var exped in expeds)
                {
                    var dto = new ExpeditionInfoDto
                    {
                        LeaderId = exped.getLeader().Id,
                        Status = exped.isRegistering() ? 1 : 0,
                        Type = exped.getType().ordinal()
                    };
                    dto.Members.AddRange(exped.getMembers().Select(x => new ExpeditionMemberDto { Id = x.Key, Name = x.Value }).OrderBy(x => x.Id == dto.LeaderId));
                    item.Expeditions.Add(dto);
                }
                res.List.Add(item);
            }
            return res;
        }
    }
}
