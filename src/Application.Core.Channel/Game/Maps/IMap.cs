using Application.Core.Game.Gameplay;
using Application.Core.Game.Life;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.Game.TheWorld;
using Application.Shared.WzEntity;
using client.inventory;
using net.server.coordinator.world;
using scripting.Event;
using server.events.gm;
using server.life;
using server.maps;

namespace Application.Core.Game.Maps
{
    public interface IMap : IDisposable
    {
        int Id { get; }
        XiGuai? XiGuai { get; set; }
        public IWorldChannel ChannelServer { get; }
        /// <summary>
        /// 当存在小数时，则是概率生成
        /// </summary>
        public float MonsterRate { get; set; }
        float ActualMonsterRate { get; }
        AtomicInteger droppedItemCount { get; set; }
        public OxQuiz? Ox { get; set; }
        /// <summary>
        /// 似乎并没有派上用
        /// </summary>
        public TimeMob? TimeMob { get; set; }
        void addAllMonsterSpawn(Monster monster, int mobTime, int team);

        void addMapleArea(Rectangle rec);
        void addMapObject(IMapObject mapobject);

        void addMonsterSpawn(Monster monster, int mobTime, int team);
        void addPartyMember(Player chr, int partyid);
        void addPlayer(Player chr);
        void addPlayerNPCMapObject(PlayerNPC pnpcobject);
        void addPlayerPuppet(Player player);
        void addPortal(Portal myPortal);
        void addSelfDestructive(Monster mob);
        void allowSummonState(bool b);
        void broadcastBalrogVictory(string leaderName);
        void broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null);
        void broadcastEnemyShip(bool state);
        void broadcastGMMessage(Player source, Packet packet, bool repeatToSource);
        void broadcastGMMessage(Packet packet);
        void broadcastGMPacket(Player source, Packet packet);
        void broadcastGMSpawnPlayerMapObjectMessage(Player source, Player player, bool enteringField);
        void broadcastHorntailVictory();
        void broadcastMessage(Player source, Packet packet, bool repeatToSource, bool ranged = false);
        void broadcastMessage(Player? source, Packet packet, Point rangedFrom);
        void broadcastMessage(Packet packet);
        void broadcastMessage(Packet packet, Point rangedFrom);
        void broadcastNightEffect();
        void broadcastNONGMMessage(Player source, Packet packet, bool repeatToSource);
        void broadcastPacket(Player source, Packet packet);
        void broadcastPinkBeanVictory(int channel);
        void broadcastShip(bool state);
        void broadcastSpawnPlayerMapObjectMessage(Player source, Player player, bool enteringField);
        void broadcastStringMessage(int type, string message);
        void broadcastUpdateCharLookMessage(Player source, Player player);
        void broadcastZakumVictory();
        Point calcDropPos(Point initial, Point fallback);
        bool canDeployDoor(Point pos);
        void checkMapOwnerActivity();
        bool claimOwnership(Player chr);
        void clearDrops();
        void clearDrops(Player player);
        void clearMapObjects();
        void closeMapSpawnPoints();
        bool containsNPC(int npcid);
        int countAlivePlayers();
        int countBosses();
        int countItems();
        int countMonster(int id);
        int countMonster(int minid, int maxid);
        int countMonsters();
        int countPlayers();
        int countReactors();
        bool damageMonster(Player chr, Monster monster, int damage, short delay = 0);
        void destroyNPC(int npcid);
        void destroyReactor(int oid);
        void destroyReactors(int first, int last);
        void disappearingItemDrop(IMapObject dropper, Player owner, Item item, Point pos);
        void disappearingMesoDrop(int meso, IMapObject dropper, Player owner, Point pos);
        void dismissRemoveAfter(Monster monster);
        void dropFromFriendlyMonster(Player chr, Monster mob);
        void dropFromReactor(Player chr, Reactor reactor, Item drop, Point dropPos, short questid, short delay = 0);
        byte dropGlobalItemsFromMonsterOnMap(List<DropEntry> globalEntry, Point pos, byte d, byte droptype, int mobpos, Player chr, Monster mob, short delay);
        void DropItemFromMonsterBySteal(List<DropEntry> list, Player chr, Monster mob, short delay);
        void dropMessage(int type, string message);
        bool eventStarted();
        Portal? findClosestPlayerSpawnpoint(Point from);
        Portal? findClosestPortal(Point from);
        SpawnPoint? findClosestSpawnpoint(Point from);
        Portal? findClosestTeleportPortal(Point from);
        Portal? findMarketPortal();
        void generateMapDropRangeCache();
        MonsterAggroCoordinator getAggroCoordinator();
        List<Monster> getAllMonsters();
        List<IMapObject> getAllPlayer();
        List<Player> getAllPlayers();
        List<Reactor> getAllReactors();
        Rectangle getArea(int index);
        List<Rectangle> getAreas();
        IWorldChannel getChannelServer();
        Player? getCharacterById(int id);
        Player? getCharacterByName(string name);
        IReadOnlyCollection<Player> getCharacters();
        bool getDocked();
        Portal? getDoorPortal(int doorid);
        KeyValuePair<string, int>? getDoorPositionStatus(Point pos);
        int getDroppedItemCount();
        int getDroppedItemsCountById(int itemid);
        IDictionary<string, int> getEnvironment();
        EventInstanceManager? getEventInstance();
        string? getEventNPC();
        bool getEverlast();
        int getFieldLimit();
        FootholdTree? getFootholds();
        int getForcedReturnId();
        IMap getForcedReturnMap();
        Point getGroundBelow(Point pos);
        int getHPDec();
        int getHPDecProtect();
        int getId();
        List<IMapObject> getItems();
        Dictionary<int, Player> getMapAllPlayers();
        Rectangle getMapArea();
        string getMapName();
        IMapObject? getMapObject(int oid);
        List<IMapObject> getMapObjects();
        List<IMapObject> getMapObjectsInBox(Rectangle box, List<MapObjectType> types);
        List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, List<MapObjectType> types);
        Dictionary<int, Player> getMapPlayers();

        short getMobInterval();

        Monster? getMonsterById(int id);
        Monster? getMonsterByOid(int oid);
        List<IMapObject> getMonsters();
        NPC? getNPCById(int id);
        int getNumPlayersInArea(int index);
        int getNumPlayersInRect(Rectangle rect);
        string getOnFirstUserEnter();
        string getOnUserEnter();
        List<IMapObject> getPlayers();
        List<Player> getPlayersInRange(Rectangle box);
        Point? getPointBelow(Point pos);
        Portal? getPortal(int portalid);
        Portal? getPortal(string portalname);
        Portal getRandomPlayerSpawnpoint();
        Reactor? getReactorById(int Id);
        Reactor? getReactorByName(string name);
        Reactor? getReactorByOid(int oid);
        List<IMapObject> getReactors();
        List<Reactor> getReactorsByIdRange(int first, int last);
        float getRecovery();
        IMap getReturnMap();
        int getReturnMapId();
        int getSeats();

        int getSpawnedMonstersOnMap();
        string getStreetName();
        bool getSummonState();
        int getTimeLimit();
        int getWorld();
        bool hasClock();
        bool hasEventNPC();
        void instanceMapForceRespawn();
        void instanceMapRespawn();
        bool isAllReactorState(int reactorId, int state);
        bool isBlueCPQMap();
        bool isCPQLobby();
        bool isCPQLoserMap();
        /// <summary>
        /// Carnival Party Quest?
        /// </summary>
        /// <returns></returns>
        bool isCPQMap();
        bool isCPQMap2();
        bool isCPQWinnerMap();
        bool isEventMap();
        bool isHorntailDefeated();
        bool isMuted();
        bool isOwnershipRestricted(Player chr);
        bool isOxQuiz();
        bool isPurpleCPQMap();
        bool isStartingEventMap();
        bool isTown();
        void killAllMonsters();
        void killAllMonstersNotFriendly();
        void killFriendlies(Monster mob);
        void killMonster(int mobId);
        public void killMonster(Monster? monster, Player? chr, bool withDrops, short dropDelay = 0);
        void killMonster(Monster? monster, Player? chr, bool withDrops, int animation, short dropDelay);
        void killMonsterWithDrops(int mobId);
        bool makeDisappearItemFromMap(MapItem mapitem);
        bool makeDisappearItemFromMap(IMapObject? mapobj);
        void makeMonsterReal(Monster monster);
        void mobMpRecovery();
        void moveEnvironment(string ms, int type);
        void moveMonster(Monster monster, Point reportedPos);
        void movePlayer(Player player, Point newPosition);
        void pickItemDrop(Packet pickupPacket, MapItem mdrop);
        void registerCharacterStatUpdate(Action r);
        void removeAllMonsterSpawn(int mobId, int x, int y);
        void removeMapObject(int num);
        void removeMapObject(IMapObject obj);
        void removeMonsterSpawn(int mobId, int x, int y);
        void removeParty(int partyid);
        void removePartyMember(Player chr, int partyid);
        void removePlayer(Player chr);
        void removePlayerPuppet(Player player);
        bool removeSelfDestructive(int mapobjectid);
        void reportMonsterSpawnPoints(Player chr);
        void resetFully();
        void resetMapObjects();
        void resetMapObjects(int difficulty, bool isPq);
        void resetPQ(int difficulty = 1);
        void resetReactors();
        void resetReactors(List<Reactor> list);
        void respawn();
        void restoreMapSpawnPoints();
        void searchItemReactors(Reactor react);
        void sendNightEffect(Player chr);
        void setAllowSpawnPointInBox(bool allow, Rectangle box);
        void setAllowSpawnPointInRange(bool allow, Point from, double rangeSq);
        void setBackgroundTypes(Dictionary<int, int> backTypes);
        void setBoat(bool hasBoat);
        void setClock(bool hasClock);
        void setDocked(bool isDocked);
        void setEventInstance(EventInstanceManager? eim);
        void setEventStarted(bool @event);
        void setEverlast(bool everlast);
        void setFieldLimit(int fieldLimit);
        void setFieldType(int fieldType);
        void setFootholds(FootholdTree footholds);
        void setForcedReturnMap(int map);
        void setHPDec(int delta);
        void setHPDecProtect(int delta);
        void setMapLineBoundings(int vrTop, int vrBottom, int vrLeft, int vrRight);
        void setMapName(string mapName);
        void setMapPointBoundings(int px, int py, int h, int w);
        void setMobCapacity(int capacity);
        void setMobInterval(short interval);
        void setMuted(bool mute);
        void setOnFirstUserEnter(string onFirstUserEnter);
        void setOnUserEnter(string onUserEnter);
        void setOxQuiz(bool b);
        void setReactorState();
        void setRecovery(float recRate);
        void setSeats(int seats);
        void setStreetName(string streetName);

        void setTimeLimit(int timeLimit);
        void setTown(bool isTown);
        void shuffleReactors(int first = 0, int last = int.MaxValue);
        void shuffleReactors(List<object> list);
        void softKillAllMonsters();
        void spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false);
        void spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false);
        void spawnDojoMonster(Monster monster);
        void spawnDoor(DoorObject door);
        void spawnFakeMonster(Monster monster);
        void spawnFakeMonsterOnGroundBelow(Monster mob, Point pos);

        void spawnHorntailOnGroundBelow(Point targetPoint);
        void spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, bool ffaDrop, bool playerDrop);
        void spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, byte dropType, bool playerDrop);
        void spawnKite(Kite kite);
        void spawnMesoDrop(int meso, Point position, IMapObject dropper, Player owner, bool playerDrop, byte droptype, short delay = 0);
        void spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery);
        void spawnMonster(Monster monster, int difficulty = 1, bool isPq = false);
        void spawnMonsterOnGroundBelow(int id, int x, int y);
        void spawnMonsterOnGroundBelow(Monster? mob, Point pos);
        void spawnMonsterWithEffect(Monster monster, int effect, Point pos);
        void spawnReactor(Reactor reactor);
        void spawnRevives(Monster monster);
        void spawnSummon(Summon summon);
        void startEvent();
        void startEvent(Player chr);
        void startMapEffect(string msg, int itemId, long time = 30000);
        void toggleDrops();
        void toggleEnvironment(string ms);
        void toggleHiddenNPC(int id);
        Player? unclaimOwnership();
        bool unclaimOwnership(Player? chr);
        void unregisterItemDrop(MapItem mapitem);
        void updatePartyItemDropsToNewcomer(Player newcomer, List<MapItem> partyItems);
        List<MapItem> updatePlayerItemDropsToParty(int partyid, int charid, List<Player> partyMembers, Player? partyLeaver);
        void warpEveryone(int to);
        void warpEveryone(int to, int pto);
        void warpOutByTeam(int team, int mapid);
    }
}
