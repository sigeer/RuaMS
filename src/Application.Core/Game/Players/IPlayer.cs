using Application.Core.client.Characters;
using Application.Core.Duey;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Channel;
using Application.Core.Game.Trades;
using Application.Shared.Objects;
using client;
using client.autoban;
using client.creator;
using client.inventory;
using client.keybind;
using client.newyear;
using net.server;
using net.server.world;
using scripting;
using scripting.Event;
using server;
using server.events;
using server.events.gm;
using server.life;
using server.maps;
using server.minigame;
using server.partyquest;
using server.quest;
using static Application.Core.Game.Players.Player;

namespace Application.Core.Game.Players
{
    public interface IPlayer : IDB_Character, IAnimatedMapObject, IMapObject, IPlayerStats, IMapPlayer, ILife
    {
        public IChannelClient Client { get; }
        /// <summary>
        /// <para>-1 在cashshop</para>
        /// <para>0 离线</para>
        /// <para>&gt;0 频道号</para>
        /// </summary>
        public int Channel { get; }
        public bool IsOnlined => Client.IsOnlined;
        public BuddyList BuddyList { get; set; }
        public PlayerBag Bag { get; set; }
        public Storage Storage { get; set; }
        public CashShop CashShopModel { get; set; }
        public PlayerSavedLocation SavedLocations { get; set; }
        public PlayerKeyMap KeyMap { get; set; }
        public PlayerSkill Skills { get; set; }
        public SkillMacro?[] SkillMacros { get; set; }

        public Team? TeamModel { get; }
        public Guild? GuildModel { get; }
        public Alliance? AllianceModel { get; }
        public ISchool? SchoolModel { get; set; }
        public Dictionary<short, string> AreaInfo { get; set; }
        public MonsterBook Monsterbook { get; set; }
        public Messenger? Messenger { get; set; }

        public IMount? MountModel { get; }
        public Job JobModel { get; set; }
        public SkinColor SkinColorModel { get; set; }

        public PlayerTrockLocation PlayerTrockLocation { get; set; }
        public Dictionary<string, Events> Events { get; set; }

        public byte[]? QuickSlotLoaded { get; set; }
        public QuickslotBinding? QuickSlotKeyMapped { get; set; }
        public Fitness? Fitness { get; set; }
        public Ola? Ola { get; set; }

        public object SaveToDBLock { get; set; }

        public event EventHandler<IPlayer>? OnLevelUp;
        public event EventHandler<IPlayer>? OnJobUpdate;
        public event EventHandler<IPlayer>? OnLodgedUpdate;

        public MonsterCarnivalParty? MCTeam { get; set; }
        public int TotalCP { get; }
        public int AvailableCP { get; }

        public ILogger Log { get; }
        void LeaveGuild();
        void StartPlayerTask();
        void StopPlayerTask();
        void addCooldown(int skillId, long startTime, long length);
        void addCP(int ammount);
        void addCrushRing(Ring r);
        int addDojoPointsByMap(int mapid);
        void addExcluded(long petId, int x);
        void addFame(int famechange);
        void addFriendshipRing(Ring r);
        void addGachaExp(int gain);
        void addJailExpirationTime(long time);
        void addMarriageRing(Ring? r);
        void addMerchantMesos(int add);
        void addMesosTraded(int gain);
        void addNewYearRecord(NewYearCardRecord newyear);
        void addPet(Pet pet);
        void addPlayerRing(Ring ring);
        void addSummon(int id, Summon summon);
        void addTrockMap();
        void addVipTrockMap();
        void addVisibleMapObject(IMapObject mo);
        void announceBattleshipHp();
        void announceDiseases();
        void announceUpdateQuest(DelayedQuestUpdate questUpdateType, params object[] paramsValue);
        bool applyConsumeOnPickup(int itemId);
        bool applyHpMpChange(int hpCon, int hpchange, int mpchange);
        void applyPartyDoor(Door door, bool partyUpdate);
        bool attemptCatchFish(int baitLevel);
        void autoban(string reason);
        void awardQuestPoint(int awardedPoints);
        void ban(string reason);
        void block(int reason, int days, string desc);
        void blockPortal(string? scriptName);
        void unblockPortal(string? scriptName);
        void broadcastAcquaintances(int type, string message);
        void broadcastAcquaintances(Packet packet);
        void broadcastMarriageMessage();
        void broadcastStance();
        void broadcastStance(int newStance);
        void buffExpireTask();
        int calculateMaxBaseDamage(int watk);
        int calculateMaxBaseDamage(int watk, WeaponType weapon);
        int calculateMaxBaseMagicDamage(int matk);
        void cancelAllBuffs(bool softcancel);
        void cancelAllDebuffs();
        void cancelBuffExpireTask();
        void cancelBuffStats(BuffStat stat);
        void cancelDiseaseExpireTask();
        void cancelEffect(int itemId);
        bool cancelEffect(StatEffect effect, bool overwrite, long startTime);
        /// <summary>
        /// 内部会先判断buff是否存在，不需要先调用getBuffedValue != null
        /// </summary>
        /// <param name="stat"></param>
        void cancelEffectFromBuffStat(BuffStat stat);
        void cancelExpirationTask();
        void cancelMagicDoor();
        bool cancelPendingNameChange();
        void cancelQuestExpirationTask();
        void cancelSkillCooldownTask();
        bool canDoor();
        bool canGainSlots(int type, int slots);
        FameStatus canGiveFame(IPlayer from);
        bool canHold(int itemid, int quantity = 1);
        bool canHoldMeso(int gain);
        bool canHoldUniques(List<int> itemids);
        bool cannotEnterCashShop();
        void changeCI(int type);
        void changeFaceExpression(int emote);
        void changeJob(Job? newJob);
        void changeKeybinding(int key, KeyBinding keybinding);


        void changePage(int page);
        void changeQuickslotKeybinding(byte[] aQuickslotKeyMapped);
        void changeSkillLevel(Skill skill, sbyte newLevel, int newMasterlevel, long expiration);
        void changeTab(int tab);
        void changeType(int type);
        void checkBerserk(bool isHidden);
        void checkMessenger();
        void clearCpqTimer();
        void clearSavedLocation(SavedLocationType type);
        void clearSummons();
        void closeHiredMerchant(bool closeMerchant);
        void closeMiniGame(bool forceClose);
        void closeNpcShop();
        void closePartySearchInteractions();
        void closePlayerInteractions();
        void closePlayerMessenger();
        void closePlayerShop();
        void closeRPS();
        void closeTrade();
        void collectDiseases();
        void commitExcludedItems();
        bool containsAreaInfo(int area, string info);
        bool containsSummon(Summon summon);
        void controlMonster(Monster monster);
        int countItem(int itemid);
        void createDragon();
        void debugListAllBuffs();
        void debugListAllBuffsCount();
        void decreaseBattleshipHp(int decrease);
        void decreaseReports();
        void deleteBuddy(int otherCid);
        void deleteFromTrocks(int map);
        void deleteFromVipTrocks(int map);
        void disbandGuild();
        void diseaseExpireTask();
        void dispel();
        void dispelDebuff(Disease debuff);
        void dispelDebuffs();
        void dispelSkill(int skillid);
        void doHurtHp();
        void doPendingNameChange();
        void dropMessage(int type, string message);
        void dropMessage(string message);
        void Dispose();

        void equipChanged();
        void equippedItem(Equip equip);
        void expirationTask();
        void exportExcludedItems(IChannelClient c);
        int fetchDoorSlot();
        void flushDelayedUpdateQuests();

        void forceUpdateItem(Item item);
        void forfeitExpirableQuests();
        void gainAriantPoints(int points);
        void gainCP(int gain);
        void gainExp(int gain);
        void gainExp(int gain, bool show, bool inChat);
        void gainExp(int gain, bool show, bool inChat, bool white);
        void gainExp(int gain, int party, bool show, bool inChat, bool white);
        void gainFame(int delta);
        bool gainFame(int delta, IPlayer? fromPlayer, int mode);
        void gainFestivalPoints(int gain);
        void gainGachaExp();
        void gainMeso(int gain, bool show = true, bool enableActions = false, bool inChat = false);
        bool gainSlots(int type, int slots);
        bool gainSlots(int type, int slots, bool update);

        // IPlayer generateCharacterEntry();
        void genericGuildMessage(int code);
        AbstractPlayerInteraction getAbstractPlayerInteraction();
        int getAccountID();
        IReadOnlyCollection<int> getActiveCoupons();
        List<PlayerBuffValueHolder> getAllBuffs();
        List<PlayerCoolDownValueHolder> getAllCooldowns();
        Dictionary<Disease, DiseaseExpiration> getAllDiseases();
        Alliance? getAlliance();
        int getAllianceRank();
        string? getAreaInfo(int area);
        Dictionary<short, string> getAreaInfos();
        AriantColiseum? getAriantColiseum();
        int getAriantPoints();
        AutobanManager getAutobanManager();
        float getAutopotHpAlert();
        float getAutopotMpAlert();
        int getBattleshipHp();
        List<string> getBlockedPortals();
        float getBossDropRate();
        BuddyList getBuddylist();
        long? getBuffedStarttime(BuffStat effect);
        int? getBuffedValue(BuffStat effect);
        StatEffect? getBuffEffect(BuffStat stat);
        bool HasBuff(BuffStat stat);
        int getBuffSource(BuffStat stat);
        float getCardRate(int itemid);
        CashShop getCashShop();
        int getChair();
        string? getChalkboard();
        WorldChannel getChannelServer();
        int getCleanItemQuantity(int itemid, bool checkEquipped);
        IChannelClient getClient();
        short getCombo();
        List<QuestStatus> getCompletedQuests();
        ICollection<Monster> getControlledMonsters();
        float getCouponDropRate();
        float getCouponExpRate();
        float getCouponMesoRate();
        List<Ring> getCrushRings();
        bool getCS();
        int getCurrentCI();
        int getCurrentPage();
        int getCurrentTab();
        int getCurrentType();
        int getDiseasesSize();
        int getDojoEnergy();
        int getDojoPoints();
        int getDojoStage();
        ICollection<Door> getDoors();
        int getDoorSlot();
        Dragon? getDragon();
        float getDropRate();
        int getEnergyBar();
        EventInstanceManager? getEventInstance();
        Dictionary<string, Events> getEvents();
        Dictionary<long, HashSet<int>> getExcluded();
        HashSet<int> getExcludedItems();
        int getExp();
        float getExpRate();
        int getFace();
        int getFame();
        Family? getFamily();
        FamilyEntry? getFamilyEntry();
        int getFamilyId();
        int getFestivalPoints();
        int getFh();
        bool getFinishedDojoTutorial();
        List<Ring> getFriendshipRings();
        int getGachaExp();
        int getGender();
        Guild? getGuild();
        int getGuildId();
        int getGuildRank();
        int getHair();
        HiredMerchant? getHiredMerchant();
        int getId();
        int getInitialSpawnpoint();
        Inventory getInventory(InventoryType type);
        int getItemEffect();
        int getItemQuantity(int itemid, bool checkEquipped);
        long getJailExpirationTimeLeft();
        Job getJob();
        int getJobId();
        int getJobRank();
        int getJobRankMove();
        Job getJobStyle();
        Job getJobStyle(byte opt);
        int getJobType();
        Dictionary<int, KeyBinding> getKeymap();
        int getLanguage();
        long getLastCombo();
        string getLastCommandMessage();
        long getLastHealed();
        int getLastMobCount();
        long getLastSnowballAttack();
        long getLastUsedCashItem();
        List<int> getLastVisitedMapids();
        int getLevel();
        int getLinkedLevel();
        string? getLinkedName();
        TimeSpan getLoggedInTime();
        DateTimeOffset getLoginTime();
        SkillMacro?[] getMacros();
        Door? getMainTownDoor();
        int getMapId();
        Marriage? getMarriageInstance();
        int getMarriageItemId();
        Ring? getMarriageRing();
        int getMasterLevel(int skill);
        int getMasterLevel(Skill? skill);
        int getMaxClassLevel();
        int getMaxLevel();
        string getMedalText();
        int getMerchantMeso();
        int getMerchantNetMeso();
        int getMeso();
        float getMesoRate();
        int getMesosTraded();
        int getMiniGamePoints(MiniGame.MiniGameResult type, bool omok);
        MonsterBook getMonsterBook();
        MonsterCarnival? getMonsterCarnival();
        void SetMount(IMount? mount);
        IMount? getMount();
        string getName();
        NewYearCardRecord? getNewYearRecord(int cardid);
        HashSet<NewYearCardRecord> getNewYearRecords();
        int getNoPets();
        long getNpcCooldown();
        int getNumControlledMonsters();
        int getOwlSearch();
        IMap? getOwnedMap();
        int getPartnerId();
        Team? getParty();
        public bool isLeader();
        /// <summary>
        /// 不存在队伍时为 -1
        /// </summary>
        /// <returns></returns>
        int getPartyId();
        List<IPlayer> getPartyMembersOnline();
        List<IPlayer> getPartyMembersOnSameMap();
        PartyQuest? getPartyQuest();
        string getPartyQuestItems();
        Pet? getPet(int index);
        sbyte getPetIndex(long petId);
        sbyte getPetIndex(Pet pet);
        Pet?[] getPets();
        Door? getPlayerDoor();
        PlayerShop? getPlayerShop();
        int getPossibleReports();
        QuestStatus getQuest(int quest);
        QuestStatus getQuest(Quest quest);
        float getQuestExpRate();
        float getQuestMesoRate();
        QuestStatus? getQuestNAdd(Quest quest);
        QuestStatus? getQuestNoAdd(Quest quest);
        QuestStatus? getQuestRemove(Quest quest);
        byte getQuestStatus(int quest);
        int getRank();
        int getRankMove();
        float getRawDropRate();
        float getRawExpRate();
        float getRawMesoRate();
        HashSet<NewYearCardRecord> getReceivedNewYearRecords();
        int getRelationshipId();
        int getRemainingSp();
        Ring? getRingById(long id);
        RockPaperScissor? getRPS();
        int getSavedLocation(string type);
        string? getSearch();
        Shop? getShop();
        long getSkillExpiration(int skill);
        long getSkillExpiration(Skill? skill);
        int getSkillLevel(int skill);
        sbyte getSkillLevel(Skill? skill);
        StatEffect GetPlayerSkillEffect(int skillId);
        StatEffect GetPlayerSkillEffect(Skill skill);
        Dictionary<Skill, SkillEntry> getSkills();
        SkinColor getSkinColor();
        byte getSlots(int type);
        List<QuestStatus> getStartedQuests();
        StatEffect? getStatForBuff(BuffStat effect);
        Storage getStorage();
        Summon? getSummonByKey(int id);
        ICollection<Summon> getSummonsValues();
        int getTargetHpBarHash();
        long getTargetHpBarTime();
        sbyte getTeam();
        int getTotalDex();
        int getTotalInt();
        int getTotalLuk();
        int getTotalMagic();
        int getTotalStr();
        int getTotalWatk();
        Trade? getTrade();
        int[] getTrockMaps();
        int getVanquisherKills();
        int getVanquisherStage();
        int[] getVipTrockMaps();
        IMapObject[] getVisibleMapObjects();
        IMap getWarpMap(int map);
        bool getWhiteChat();
        int getWorld();
        World getWorldServer();
        void giveCoolDowns(int skillid, long starttime, long length);
        void giveDebuff(Disease disease, MobSkill skill);
        int gmLevel();
        bool gotPartyQuestItem(string partyquestchar);
        void handleEnergyChargeGain();
        void handleOrbconsume();
        bool hasActiveBuff(int sourceid);
        bool hasBuffFromSourceid(int sourceid);
        bool hasDisease(Disease dis);
        bool HasEmptySlotByItem(int itemId);
        bool hasEmptySlot(sbyte invType);
        bool hasEntered(string script);
        bool hasEntered(string script, int mapId);
        void hasGivenFame(IPlayer to);
        bool hasJustMarried();
        bool hasMerchant();
        bool hasNoviceExpRate();
        bool haveCleanItem(int itemid);
        bool haveItem(int itemid);
        bool haveItemEquipped(int itemid);
        bool haveItemWithId(int itemid, bool checkEquipped);
        bool haveWeddingRing();
        void Hide(bool hide, bool login = false);
        void increaseEquipExp(int expGain);
        void increaseGuildCapacity();
        bool isAran();

        bool isBanned();
        bool isBeginnerJob();
        bool isBuffFrom(BuffStat stat, Skill skill);
        bool isChallenged();
        bool isChangingMaps();
        bool isChasing();
        bool isCygnus();
        bool isEquippedItemPouch();
        bool isEquippedMesoMagnet();
        bool isEquippedPetItemIgnore();
        bool isGM();
        bool isGmJob();
        bool isGuildLeader();
        bool isHidden();
        /// <summary>
        /// 已登录
        /// </summary>
        /// <returns></returns>
        bool isLoggedin();
        /// <summary>
        /// 已登录 且 不在商城/拍卖
        /// </summary>
        /// <returns></returns>
        bool isLoggedinWorld();
        /// <summary>
        /// 在商城/拍卖
        /// </summary>
        /// <returns></returns>
        bool isAwayFromWorld();
        void setAwayFromChannelWorld();
        void setEnteredChannelWorld(int channel);
        bool isMale();
        bool isMapObjectVisible(IMapObject mo);
        bool isMarried();
        bool isPartyLeader();
        bool isPartyMember(IPlayer chr);
        bool isPartyMember(int cid);
        bool isRidingBattleship();
        bool isSummonsEmpty();
        void leaveMap();
        bool leaveParty();
        void levelUp(bool takeexp);
        void logOff();
        void loseExp(int loss, bool show, bool inChat);
        void loseExp(int loss, bool show, bool inChat, bool white);
        bool mergeAllItemsFromName(string name);
        void mergeAllItemsFromPosition(Dictionary<Equip.StatUpgrade, float> statups, short pos);
        void message(string m);
        IMount mount(int id, int skillid);
        bool needQuestItem(int questid, int itemid);
        void LinkNewChannelClient(IChannelClient newClient);
        void notifyMapTransferToPartner(int mapid);
        void partyOperationUpdate(Team party, List<IPlayer>? exPartyMembers);
        int peekSavedLocation(string type);
        void pickupItem(IMapObject? ob, int petIndex = -1);
        long portalDelay();
        void portalDelay(long delay);
        void purgeDebuffs();
        void questExpirationTask();
        void questTimeLimit(Quest quest, int seconds);
        void questTimeLimit2(Quest quest, long expires);
        void raiseQuestMobCount(int id);
        void receivePartyMemberHP();
        bool registerChairBuff();
        void registerEffect(StatEffect effect, long starttime, long expirationtime, bool isSilent);
        bool registerNameChange(string newName);
        void releaseControlledMonsters();
        void reloadQuestExpirations();
        void removeAllCooldownsExcept(int id, bool packet);
        void removeCooldown(int skillId);
        void removeIncomingInvites();
        void removeJailExpirationTime();
        void removeNewYearRecord(NewYearCardRecord newyear);
        Door? removePartyDoor(bool partyUpdate);
        void removePartyDoor(Team formerParty);
        void removePartyQuestItem(string letter);
        void removePet(Pet pet, bool shift_left);
        void removeSandboxItems();
        void removeVisibleMapObject(IMapObject mo);
        void resetBattleshipHp();
        void resetCP();
        void resetEnteredScript();
        void resetEnteredScript(int mapId);
        void resetEnteredScript(string script);
        void resetExcluded(long petId);
        void resetPlayerAggro();
        void resetStats();
        void respawn(EventInstanceManager? eim, int returnMap);
        void respawn(int returnMap);
        void runFullnessSchedule(int petSlot);
        bool runTirednessSchedule();
        //void saveCharToDB();
        void saveCharToDB(bool notAutosave = true, bool isLogoff = false);
        void saveLocation(string type);
        void saveLocationOnWarp();
        int sellAllItemsFromName(sbyte invTypeId, string name);
        int sellAllItemsFromPosition(ItemInformationProvider ii, InventoryType type, short pos);
        void sendKeymap();
        void sendMacros();
        void sendPacket(Packet packet);
        void sendPolice(int greason, string reason, int duration);
        void sendPolice(string text);
        void sendQuickmap();

        void setAllianceRank(int _rank);
        void setAriantColiseum(AriantColiseum? ariantColiseum);
        void setAutopotHpAlert(float hpPortion);
        void setAutopotMpAlert(float mpPortion);

        void setBattleshipHp(int battleshipHp);
        void setBuddyCapacity(int capacity);
        void setBuffedValue(BuffStat effect, int value);
        void setChalkboard(string? text);
        void setChallenged(bool challenged);
        void setChasing(bool chasing);
        void setClient(IChannelClient c);
        void setCombo(short count);
        void setCpqTimer(ScheduledFuture timer);
        void setCS(bool cs);
        void setDisconnectedFromChannelWorld();
        void setDojoEnergy(int x);
        void setDojoPoints(int x);
        void setDojoStage(int x);
        void setDragon(Dragon dragon);
        void setEnergyBar(int set);

        void setEventInstance(EventInstanceManager? eventInstance);
        void setExp(int amount);
        void setFace(int face);
        void setFame(int fame);
        void setFamilyEntry(FamilyEntry? entry);
        void setFamilyId(int familyId);
        void setFestivalPoints(int pontos);
        void setFinishedDojoTutorial();
        void setGachaExp(int amount);
        void setGender(int gender);
        void setGuildId(int _id);
        void setGuildRank(int _rank);
        void setHair(int hair);
        void setHasMerchant(bool set);
        void setHasSandboxItem();
        void setHiredMerchant(HiredMerchant? merchant);
        void setInventory(InventoryType type, Inventory inv);
        void setItemEffect(int itemEffect);
        void setJob(Job job);
        void setLastCombo(long time);
        void setLastCommandMessage(string text);
        void setLastHealed(long time);
        void setLastMobCount(byte count);
        void setLastSnowballAttack(long time);
        void setLastUsedCashItem(long time);
        void setLevel(int level);
        void setLoginTime(DateTimeOffset time);
        void setMap(int PmapId);
        void setMapTransitionComplete();
        void setMarriageItemId(int itemid);
        void setMasteries(int jobId);
        void setMerchantMeso(int set);
        void setMessenger(Messenger? messenger);
        void setMessengerPosition(int position);
        MiniGame? getMiniGame();
        void setMiniGame(MiniGame? miniGame);
        void setMiniGamePoints(IPlayer visitor, int winnerslot, bool omok);
        void setMonsterBookCover(int bookCover);
        void setMonsterCarnival(MonsterCarnival? monsterCarnival);
        void setName(string name);
        void setNpcCooldown(long d);
        // void setOfflineGuildRank(int newRank);
        void setOwlSearch(int id);
        void setOwnedMap(IMap? map);
        void setPartnerId(int partnerid);
        void setParty(Team? p);
        void setPartyQuest(PartyQuest? pq);
        void setPartyQuestItemObtained(string partyquestchar);
        void setPlayerAggro(int mobHash);
        void setPlayerShop(PlayerShop? playerShop);
        void setQuestAdd(Quest quest, byte status, string customData);
        void setQuestProgress(int id, int infoNumber, string progress);
        void setRPS(RockPaperScissor? rps);
        void setSearch(string? find);
        void setSessionTransitionState();
        void setShop(Shop? shop);
        void setSkinColor(SkinColor skinColor);

        void setTargetHpBarHash(int mobHash);
        void setTargetHpBarTime(long timeNow);
        void setTeam(int team);
        void setTrade(Trade? trade);
        void setVanquisherKills(int x);
        void setVanquisherStage(int x);
        void setWorld(int world);
        void shiftPetsRight();
        void showDojoClock();
        void showHint(string msg, int length = 500);

        void showUnderleveledInfo(Monster mob);
        void silentApplyDiseases(Dictionary<Disease, DiseaseExpiration> diseaseMap);
        void silentGiveBuffs(List<KeyValuePair<long, PlayerBuffValueHolder>> buffs);
        void silentPartyUpdate();
        void sitChair(int itemId);
        void skillCooldownTask();
        bool skillIsCooling(int skillId);

        void stopControllingMonster(Monster monster);
        void toggleBlockCashShop();
        void toggleExpGain();
        void toggleHide(bool login);
        void toggleWhiteChat();
        void unequipAllPets();
        void unequippedItem(Equip equip);
        void unequipPet(Pet pet, bool shift_left, bool hunger = false);
        bool unregisterChairBuff();
        void updateActiveEffects();
        void updateAreaInfo(int area, string info);
        void updateAriantScore();
        void updateAriantScore(int dropQty);
        void updateCouponRates();
        void updateMacros(int position, SkillMacro? updateMacro);
        void updatePartyMemberHP();
        void updateQuestStatus(QuestStatus qs);
        void updateRemainingSp(int remainingSp);
        void updateSingleStat(Stat stat, int newval);
        void useCP(int ammount);

        void withdrawMerchantMesos();
        void yellowMessage(string m);

        List<QuestStatus> getQuests();
        DueyPackageObject[] GetDueyPackages();
    }
}
