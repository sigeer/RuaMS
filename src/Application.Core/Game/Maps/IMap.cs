using Application.Core.Channel;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Life;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.Scripting.Events;
using Application.Shared.Languages;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using client.inventory;
using net.server.coordinator.world;
using scripting.Event;
using server.events.gm;
using server.life;
using server.maps;

namespace Application.Core.Game.Maps
{
    public interface IMap : IDisposable, IClientMessenger
    {
        int Id { get; }
        /// <summary>
        /// "ChannelId_EventInstanceName_MapId";
        /// </summary>
        string InstanceName { get; }
        XiGuai? XiGuai { get; set; }
        public WorldChannel ChannelServer { get; }
        FootholdTree Footholds { get; }
        MapTemplate SourceTemplate { get; }
        /// <summary>
        /// 当存在小数时，则是概率生成
        /// </summary>
        public float MonsterRate { get; set; }
        float ActualMonsterRate { get; }
        public OxQuiz? Ox { get; set; }
        /// <summary>
        /// 似乎并没有派上用
        /// </summary>
        public TimeMob? TimeMob { get; set; }
        bool IsTrackedByEvent { get; set; }
        AbstractEventInstanceManager? EventInstanceManager { get; }
        void addAllMonsterSpawn(Monster monster, int mobTime, int team);
        void addMapObject(IMapObject mapobject);

        void addMonsterSpawn(Monster monster, int mobTime, int team);
        void addPlayer(IPlayer chr);
        void addPlayerNPCMapObject(IMapObject pnpcobject);
        void addPlayerPuppet(IPlayer player);
        void addSelfDestructive(Monster mob);
        void allowSummonState(bool b);
        void broadcastBalrogVictory(string leaderName);
        void broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null);
        void broadcastEnemyShip(bool state);
        void broadcastGMMessage(IPlayer source, Packet packet, bool repeatToSource);
        void broadcastGMMessage(Packet packet);
        void broadcastGMPacket(IPlayer source, Packet packet);
        void broadcastGMSpawnPlayerMapObjectMessage(IPlayer source, IPlayer player, bool enteringField);
        void broadcastHorntailVictory();
        void broadcastMessage(IPlayer source, Packet packet, bool repeatToSource, bool ranged = false);
        void broadcastMessage(IPlayer? source, Packet packet, Point rangedFrom);
        void broadcastMessage(Packet packet);
        void broadcastMessage(Packet packet, Point rangedFrom);
        void broadcastNightEffect();
        void broadcastNONGMMessage(IPlayer source, Packet packet, bool repeatToSource);
        void broadcastPacket(IPlayer source, Packet packet);
        void broadcastPinkBeanVictory(int channel);
        void broadcastShip(bool state);
        void broadcastSpawnPlayerMapObjectMessage(IPlayer source, IPlayer player, bool enteringField);
        void broadcastUpdateCharLookMessage(IPlayer source, IPlayer player);
        void broadcastZakumVictory();
        Point calcDropPos(Point initial, Point fallback);
        bool canDeployDoor(Point pos);
        void checkMapOwnerActivity();
        bool claimOwnership(IPlayer chr);
        void clearDrops();
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
        bool damageMonster(IPlayer chr, Monster monster, int damage, short delay = 0);
        void destroyNPC(int npcid);
        void destroyReactor(int oid);
        void destroyReactors(int first, int last);
        void disappearingItemDrop(IMapObject dropper, IPlayer owner, Item item, Point pos);
        void disappearingMesoDrop(int meso, IMapObject dropper, IPlayer owner, Point pos);
        void dismissRemoveAfter(Monster monster);
        void dropFromFriendlyMonster(IPlayer chr, Monster mob);
        void dropFromReactor(IPlayer chr, Reactor reactor, Item drop, Point dropPos, short questid, short delay = 0);
        void DropItemFromMonsterBySteal(List<DropEntry> list, IPlayer chr, Monster mob, short delay);
        void dropMessage(int type, string message);
        bool eventStarted();
        Portal? findClosestPlayerSpawnpoint(Point from);
        Portal? findClosestPortal(Point from);
        SpawnPoint? findClosestSpawnpoint(Point from);
        Portal? findClosestTeleportPortal(Point from);
        Portal? findMarketPortal();
        void generateMapDropRangeCache();
        MonsterAggroCoordinator getAggroCoordinator();
        List<IPlayer> getAllPlayers();
        List<Reactor> getAllReactors();
        Rectangle getArea(int index);
        List<Rectangle> getAreas();
        WorldChannel getChannelServer();
        IPlayer? getCharacterById(int id);
        IPlayer? getCharacterByName(string name);
        bool getDocked();
        Portal? getDoorPortal(int doorid);
        KeyValuePair<string, int>? getDoorPositionStatus(Point pos);
        int getDroppedItemsCountById(int itemid);
        IDictionary<string, int> getEnvironment();
        AbstractEventInstanceManager? getEventInstance();
        bool getEverlast();
        int getFieldLimit();
        int getForcedReturnId();
        IMap getForcedReturnMap();
        Point getGroundBelow(Point pos);
        int getHPDec();
        int getHPDecProtect();
        int getId();
        List<MapItem> getItems();
        Rectangle getMapArea();
        string getMapName();
        void ProcessMonster(Action<Monster> action);
        void ProcessMapObject(Func<IMapObject, bool> codition, Action<IMapObject> action);
        IMapObject? getMapObject(int oid);
        List<IMapObject> getMapObjects();
        List<IMapObject> GetMapObjects(Func<IMapObject, bool> func);
        List<IMapObject> getMapObjectsInBox(Rectangle box, List<MapObjectType> types);
        List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, List<MapObjectType> types);
        Dictionary<int, IPlayer> getMapPlayers();
        Monster? getMonsterById(int id);
        Monster? getMonsterByOid(int oid);
        NPC? getNPCById(int id);
        int getNumPlayersInArea(int index);
        int getNumPlayersInRect(Rectangle rect);
        List<IPlayer> getPlayersInRange(Rectangle box);
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
        bool hasClock();
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
        bool isHorntailDefeated();
        bool isMuted();
        bool isOwnershipRestricted(IPlayer chr);
        bool isOxQuiz();
        bool isPurpleCPQMap();
        bool isStartingEventMap();
        void killAllMonsters();
        void killAllMonstersNotFriendly();
        void killFriendlies(Monster mob);
        void killMonster(int mobId);
        public void killMonster(Monster? monster, IPlayer? chr, bool withDrops, short dropDelay = 0);
        void killMonster(Monster? monster, IPlayer? chr, bool withDrops, int animation, short dropDelay);
        public bool removeKilledMonsterObject(Monster monster);
        void killMonsterWithDrops(int mobId);
        bool makeDisappearItemFromMap(MapItem mapitem);
        bool makeDisappearItemFromMap(IMapObject? mapobj);
        void makeMonsterReal(Monster monster);
        void mobMpRecovery();
        void moveEnvironment(string ms, int type);
        void moveMonster(Monster monster, Point reportedPos);
        void movePlayer(IPlayer player, Point newPosition);
        void pickItemDrop(Packet pickupPacket, MapItem mdrop);
        void registerCharacterStatUpdate(Action r);
        void removeAllMonsterSpawn(int mobId, int x, int y);
        void removeMapObject(int num);
        void removeMapObject(IMapObject obj);
        void removeMonsterSpawn(int mobId, int x, int y);
        void removePlayer(IPlayer chr);
        void removePlayerPuppet(IPlayer player);
        bool removeSelfDestructive(int mapobjectid);
        void reportMonsterSpawnPoints(IPlayer chr);
        void resetFully();
        void resetMapObjects();
        void resetMapObjects(int difficulty, bool isPq);
        void resetPQ(int difficulty = 1);
        void resetReactors();
        void resetReactors(List<Reactor> list);
        void respawn();
        void restoreMapSpawnPoints();
        void searchItemReactors(Reactor react);
        void sendNightEffect(IPlayer chr);
        void setAllowSpawnPointInBox(bool allow, Rectangle box);
        void setDocked(bool isDocked);
        void setEventStarted(bool @event);
        void setMapName(string mapName);
        void setMuted(bool mute);
        void setOxQuiz(bool b);
        void setReactorState();
        void setStreetName(string streetName);
        void shuffleReactors(int first = 0, int last = int.MaxValue);
        void shuffleReactors(List<object> list);
        void spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false);
        void spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false);
        void spawnDojoMonster(Monster monster);
        void spawnDoor(DoorObject door);
        void spawnFakeMonster(Monster monster);
        void spawnFakeMonsterOnGroundBelow(Monster mob, Point pos);

        void spawnHorntailOnGroundBelow(Point targetPoint);
        void spawnItemDrop(IMapObject dropper, IPlayer owner, Item item, Point pos, bool ffaDrop, bool playerDrop);
        void spawnKite(Kite kite);
        void spawnMesoDrop(int meso, Point position, IMapObject dropper, IPlayer owner, bool playerDrop, DropType droptype, short delay = 0);
        void spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery);
        void spawnMonster(Monster monster, int difficulty = 1, bool isPq = false);
        void spawnMonsterOnGroundBelow(int id, int x, int y);
        void spawnMonsterOnGroundBelow(Monster? mob, Point pos);
        void spawnMonsterWithEffect(Monster monster, int effect, Point pos);
        void spawnReactor(Reactor reactor);
        void spawnRevives(Monster monster);
        void spawnSummon(Summon summon);
        void startEvent();
        void startEvent(IPlayer chr);
        void startMapEffect(string msg, int itemId, long time = 30000);
        void toggleDrops();
        void toggleEnvironment(string ms);
        IPlayer? unclaimOwnership();
        bool unclaimOwnership(IPlayer? chr);
        void updatePartyItemDropsToNewcomer(IPlayer newcomer, List<MapItem> partyItems);
        List<MapItem> updatePlayerItemDropsToParty(int partyid, int charid, List<IPlayer> partyMembers, IPlayer? partyLeaver);
        void warpEveryone(int to);
        void warpEveryone(int to, int pto);
        void warpOutByTeam(int team, int mapid);

        bool IsActive();
        void BroadcastAll(Action<IPlayer> effectPlayer);
    }
}
