using Application.Core.Managers;
using client;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.services.task.world;
using net.server.services.type;
using server;
using server.maps;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public int World { get; set; }

        public string Name { get; set; } = null!;

        public int Level { get; set; }

        public int Exp { get; set; }

        public int Gachaexp { get; set; }

        public int Str { get; set; }

        public int Dex { get; set; }

        public int Luk { get; set; }

        public int Int { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public int Maxhp { get; set; }

        public int Maxmp { get; set; }

        public int Meso { get; set; }

        public int HpMpUsed { get; set; }

        public int JobId { get; private set; }

        public int Skincolor { get; set; }

        public int Gender { get; set; }

        public int Fame { get; set; }

        public int Fquest { get; set; }

        public int Hair { get; set; }

        public int Face { get; set; }

        public int Ap { get; set; }

        public string Sp { get; set; } = null!;

        public int Map { get; set; }

        public int Spawnpoint { get; set; }

        public sbyte Gm { get; set; }

        public int Party { get; set; }

        public int BuddyCapacity { get; set; } = 25;

        public DateTimeOffset CreateDate { get; set; }

        public int Rank { get; set; }

        public int RankMove { get; set; }

        public int JobRank { get; set; }

        public int JobRankMove { get; set; }

        public int GuildId { get; set; }

        public int GuildRank { get; set; }

        public int MessengerId { get; set; }

        public int MessengerPosition { get; set; } = 4;

        public int MountLevel { get; set; }

        public int MountExp { get; set; }

        public int Mounttiredness { get; set; }

        public int Omokwins { get; set; }

        public int Omoklosses { get; set; }

        public int Omokties { get; set; }

        public int Matchcardwins { get; set; }

        public int Matchcardlosses { get; set; }

        public int Matchcardties { get; set; }

        public int? MerchantMesos { get; set; }

        public bool? HasMerchant { get; set; } = false;

        public int Equipslots { get; set; }

        public int Useslots { get; set; }

        public int Setupslots { get; set; }

        public int Etcslots { get; set; }

        public int FamilyId { get; set; } = -1;

        public int Monsterbookcover { get; set; }

        public int AllianceRank { get; set; }

        public int VanquisherStage { get; set; }

        public int AriantPoints { get; set; }

        public int DojoPoints { get; set; }

        public int LastDojoStage { get; set; }

        public bool FinishedDojoTutorial { get; set; }

        public int VanquisherKills { get; set; }

        public int SummonValue { get; set; }

        public int PartnerId { get; set; }

        public int MarriageItemId { get; set; }

        public int Reborns { get; set; }

        public int Pqpoints { get; set; }

        public string DataString { get; set; } = null!;

        public DateTimeOffset LastLogoutTime { get; set; }

        public DateTimeOffset LastExpGainTime { get; set; }

        public bool PartySearch { get; set; } = true;

        public long Jailexpire { get; set; }


        //public void saveCharToDB()
        //{
        //    if (YamlConfig.config.server.USE_AUTOSAVE)
        //    {
        //        // 和 CharacterAutosaverTask 是否功能重合？
        //        CharacterSaveService service = (CharacterSaveService)getWorldServer()!.getServiceAccess(WorldServices.SAVE_CHARACTER);
        //        service.registerSaveCharacter(this.getId(), () =>
        //        {
        //            saveCharToDB(true);
        //        });
        //    }
        //    else
        //    {
        //        saveCharToDB(true);
        //    }
        //}

        object saveCharLock = new object();
        public void saveCharToDB(bool notAutosave = true)
        {
            lock (saveCharLock)
            {
                if (!IsOnlined)
                {
                    return;
                }

                Log.Debug("Attempting to {SaveMethod} chr {CharacterName}", notAutosave ? "save" : "autosave", Name);

                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var entity = dbContext.Characters.FirstOrDefault(x => x.Id == getId());
                if (entity == null)
                    throw new BusinessCharacterNotFoundException(getId());

                try
                {
                    Monitor.Enter(effLock);
                    statLock.EnterWriteLock();
                    try
                    {
                        GlobalTools.Mapper.Map(this, entity);
                    }
                    finally
                    {
                        statLock.ExitWriteLock();
                        Monitor.Exit(effLock);
                    }

                    if (base.MapModel == null || (CashShopModel != null && CashShopModel.isOpened()))
                    {
                        entity.Map = Map;
                    }
                    else
                    {
                        if (MapModel.getForcedReturnId() != 999999999)
                        {
                            entity.Map = MapModel.getForcedReturnId();
                        }
                        else
                        {
                            entity.Map = getHp() < 1 ? MapModel.getReturnMapId() : MapModel.getId();
                        }
                    }
                    if (base.MapModel == null || MapModel.getId() == 610020000 || MapModel.getId() == 610020001)
                    {  // reset to first spawnpoint on those maps
                        entity.Spawnpoint = 0;
                    }
                    else
                    {
                        var closest = MapModel.findClosestPlayerSpawnpoint(base.getPosition());
                        if (closest != null)
                        {
                            entity.Spawnpoint = closest.getId();
                        }
                        else
                        {
                            entity.Spawnpoint = 0;
                        }
                    }

                    entity.MessengerId = Messenger?.getId() ?? 0;
                    entity.MessengerPosition = Messenger == null ? 4 : MessengerPosition;

                    entity.MountLevel = MountModel?.getLevel() ?? 1;
                    entity.MountExp = MountModel?.getExp() ?? 0;
                    entity.Mounttiredness = MountModel?.getTiredness() ?? 0;

                    Monsterbook.saveCards(dbContext, getId());
                    dbContext.SaveChanges();

                    Monitor.Enter(petLock);
                    try
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (pets[i] != null)
                            {
                                pets[i]!.saveToDb();
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(petLock);
                    }


                    var ignoresPetIds = getExcluded().Select(x => x.Key).ToList();
                    dbContext.Petignores.Where(x => ignoresPetIds.Contains(x.Petid)).ExecuteDelete();
                    dbContext.Petignores.AddRange(getExcluded().SelectMany(x => x.Value.Select(y => new Petignore() { Petid = x.Key, Itemid = y })).ToList());
                    dbContext.SaveChanges();


                    dbContext.Keymaps.Where(x => x.Characterid == getId()).ExecuteDelete();
                    dbContext.Keymaps.AddRange(KeyMap.Select(x => new Keymap() { Characterid = Id, Key = x.Key, Type = x.Value.getType(), Action = x.Value.getAction() }));
                    dbContext.SaveChanges();

                    // No quickslots, or no change.
                    CharacterManager.SaveQuickSlotMapped(dbContext, this);


                    dbContext.Skillmacros.Where(x => x.Characterid == getId()).ExecuteDelete();
                    for (int i = 0; i < 5; i++)
                    {
                        var macro = SkillMacros[i];
                        if (macro != null)
                        {
                            dbContext.Skillmacros.Add(new Skillmacro(Id, (sbyte)i, macro.Skill1, macro.Skill2, macro.Skill3, macro.Name, (sbyte)macro.Shout));
                        }
                    }
                    dbContext.SaveChanges();

                    List<ItemInventoryType> itemsWithType = new();
                    foreach (Inventory iv in Bag.GetValues())
                    {
                        foreach (Item item in iv.list())
                        {
                            itemsWithType.Add(new(item, iv.getType()));
                        }
                    }

                    ItemFactory.INVENTORY.saveItems(itemsWithType, Id, dbContext);

                    var characterSkills = dbContext.Skills.Where(x => x.Characterid == getId()).ToList();
                    foreach (var skill in Skills)
                    {
                        var dbSkill = characterSkills.FirstOrDefault(x => x.Skillid == skill.Key.getId());
                        if (dbSkill == null)
                        {
                            dbSkill = new SkillEntity() { Characterid = getId(), Skillid = skill.Key.getId() };
                            dbContext.Skills.Add(dbSkill);
                        }
                        dbSkill.Skilllevel = skill.Value.skillevel;
                        dbSkill.Masterlevel = skill.Value.masterlevel;
                        dbSkill.Expiration = skill.Value.expiration;
                    }
                    dbContext.SaveChanges();

                    dbContext.Savedlocations.Where(x => x.Characterid == getId()).ExecuteDelete();
                    dbContext.Savedlocations.AddRange(
                        Enum.GetValues<SavedLocationType>()
                        .Where(x => SavedLocations[(int)x] != null)
                        .Select(x => new Savedlocation(SavedLocations[(int)x]!.getMapId(), SavedLocations[(int)x]!.getPortal()))
                        );
                    dbContext.SaveChanges();

                    dbContext.Trocklocations.Where(x => x.Characterid == Id).ExecuteDelete();
                    for (int i = 0; i < getTrockSize(); i++)
                    {
                        if (TrockMaps[i] != 999999999)
                        {
                            dbContext.Trocklocations.Add(new Trocklocation(Id, TrockMaps[i], 0));
                        }
                    }

                    for (int i = 0; i < getVipTrockSize(); i++)
                    {
                        if (VipTrockMaps[i] != 999999999)
                        {
                            dbContext.Trocklocations.Add(new Trocklocation(Id, VipTrockMaps[i], 1));
                        }
                    }
                    dbContext.SaveChanges();

                    dbContext.Buddies.Where(x => x.CharacterId == getId() && x.Pending == 0).ExecuteDelete();
                    foreach (var entry in BuddyList.getBuddies())
                    {
                        if (entry.Visible)
                        {
                            dbContext.Buddies.Add(new Buddy(getId(), entry.getCharacterId(), 0, entry.Group));
                        }
                    }
                    dbContext.SaveChanges();

                    dbContext.AreaInfos.Where(x => x.Charid == getId()).ExecuteDelete();
                    dbContext.AreaInfos.AddRange(AreaInfo.Select(x => new AreaInfo(getId(), x.Key, x.Value)));
                    dbContext.SaveChanges();

                    dbContext.Eventstats.Where(x => x.Characterid == getId()).ExecuteDelete();
                    dbContext.Eventstats.AddRange(Events.Select(x => new Eventstat(getId(), x.Key, x.Value.getInfo())));
                    dbContext.SaveChanges();

                    CharacterManager.deleteQuestProgressWhereCharacterId(dbContext, Id);


                    foreach (var qs in getQuests())
                    {
                        var questStatus = new Queststatus(getId(), qs.getQuest().getId(), (int)qs.getStatus(), (int)(qs.getCompletionTime() / 1000), qs.getExpirationTime(),
                           qs.getForfeited(), qs.getCompleted());
                        dbContext.Queststatuses.Add(questStatus);
                        dbContext.SaveChanges();

                        foreach (int mob in qs.getProgress().Keys)
                        {
                            dbContext.Questprogresses.Add(new Questprogress(getId(), questStatus.Queststatusid, mob, qs.getProgress(mob)));
                        }
                        foreach (var item in qs.getMedalMaps())
                        {
                            dbContext.Medalmaps.Add(new Medalmap(getId(), questStatus.Queststatusid, item));
                        }
                    }
                    dbContext.SaveChanges();

                    var familyEntry = getFamilyEntry(); //save family rep
                    if (familyEntry != null)
                    {
                        if (familyEntry.saveReputation(dbContext))
                            familyEntry.savedSuccessfully();
                        var senior = familyEntry.getSenior();
                        if (senior != null && senior.getChr() == null)
                        {
                            //only save for offline family members
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

                    if (CashShopModel != null)
                    {
                        CashShopModel.save(dbContext);
                    }

                    if (Storage != null && Storage.IsChanged)
                    {
                        Storage.saveToDB(dbContext);
                    }

                    dbTrans.Commit();

                }
                catch (Exception e)
                {
                    Log.Error(e, "Error saving chr {CharacterName}, level: {Level}, job: {JobId}", Name, Level, JobId);
                }
            }
        }
    }
}
