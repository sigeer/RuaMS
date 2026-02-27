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
        MapEffect? MapEffect { get; set; }
        void addMapObject(IMapObject mapobject);

        void addMonsterSpawn(int mobId, Point pos, int cy, int f, int fh, int rx0, int rx1, int mobTime, bool hide, int team, SpawnPointTrigger act = SpawnPointTrigger.Killed);
        void addMonsterSpawn(int mobId, Point pos, int mobTime, int team, SpawnPointTrigger act = SpawnPointTrigger.Killed);
        void addPlayer(Player chr);
        void addPlayerNPCMapObject(IMapObject pnpcobject);
        void addPlayerPuppet(Player player);
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
        void broadcastUpdateCharLookMessage(Player source, Player player);
        void broadcastZakumVictory();
        Point calcDropPos(Point initial, Point fallback);
        bool canDeployDoor(Point pos);
        void checkMapOwnerActivity();
        bool claimOwnership(Player chr);
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
        
        void destroyNPC(int npcid);
        void destroyReactor(int oid);
        void destroyReactors(int first, int last);
        void disappearingItemDrop(IMapObject dropper, Player owner, Item item, Point pos);
        void disappearingMesoDrop(int meso, IMapObject dropper, Player owner, Point pos);
        void dismissRemoveAfter(Monster monster);
        void dropFromFriendlyMonster(Player chr, Monster mob);
        void dropFromReactor(Player chr, Reactor reactor, Item drop, Point dropPos, short questid, short delay = 0);
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
        List<Player> getAllPlayers();
        List<Reactor> getAllReactors();
        Rectangle getArea(int index);
        List<Rectangle> getAreas();
        WorldChannel getChannelServer();
        Player? getCharacterById(int id);
        Player? getCharacterByName(string name);
        bool getDocked();
        Portal? getDoorPortal(int doorid);
        bool TryGetEffectiveDoorPortal(out MysticDoorPortal? portal);
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
        List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type, Func<TObject, bool> func) where TObject : IMapObject;
        Dictionary<int, Player> getMapPlayers();
        Monster? getMonsterById(int id);
        Monster? getMonsterByOid(int oid);
        NPC? getNPCById(int id);
        int getNumPlayersInArea(int index);
        int getNumPlayersInRect(Rectangle rect);
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
        /// <summary>
        /// 通过是否生成所有的‘死亡的XXX’来判断暗黑龙王是否被击败
        /// </summary>
        /// <returns></returns>
        bool isHorntailDefeated();
        bool isMuted();
        bool isOwnershipRestricted(Player chr);
        bool isOxQuiz();
        bool isPurpleCPQMap();
        bool isStartingEventMap();
        #region Attack Mob
        bool damageMonster(ICombatantObject chr, Monster monster, int damage, short delay = 0);
        /// <summary>
        /// 杀死所有怪物，不会掉落物品，不重生
        /// </summary>
        void killAllMonsters();
        /// <summary>
        /// 杀死所有怪物（友方单位除外）不会掉落物品，不重生
        /// </summary>
        void killAllMonstersNotFriendly();
        /// <summary>
        /// 击杀友方单位
        /// </summary>
        /// <param name="mob"></param>
        void killFriendlies(Monster mob);
        void killMonster(int mobId, bool withDrops = false);
        /// <summary>
        /// 击杀怪物
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="chr">击杀者</param>
        /// <param name="withDrops">是否掉落</param>
        /// <param name="dropDelay"></param>
        void killMonster(Monster? monster, ICombatantObject? chr, bool withDrops, short dropDelay = 0);
        void killMonster(Monster? monster, ICombatantObject? chr, bool withDrops, int animation, short dropDelay);
        #endregion

        bool makeDisappearItemFromMap(MapItem mapitem);
        bool makeDisappearItemFromMap(IMapObject? mapobj);
        void makeMonsterReal(Monster monster);
        void mobMpRecovery();
        void moveEnvironment(string ms, int type);
        void moveMonster(Monster monster, Point reportedPos);
        void movePlayer(Player player, Point newPosition);
        void pickItemDrop(Packet pickupPacket, MapItem mdrop);
        void registerCharacterStatUpdate(Action r);
        void removeMapObject(int num);
        void removeMapObject(IMapObject obj);
        void removeMonsterSpawn(int mobId, int x, int y);
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
        /// <summary>
        /// 召唤扎昆（复合型Mob）
        /// </summary>
        /// <param name="targetPoint"></param>
        void SpawnZakumOnGroundBelow(Point targetPoint);
        void spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, bool ffaDrop, bool playerDrop);
        void spawnKite(Kite kite);
        void spawnMesoDrop(int meso, Point position, IMapObject dropper, Player owner, bool playerDrop, DropType droptype, short delay = 0);
        void spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery);
        void spawnMonster(Monster monster, int difficulty = 1, bool isPq = false);
        void spawnMonsterOnGroundBelow(int id, int x, int y);
        void spawnMonsterOnGroundBelow(Monster? mob, Point pos);
        void spawnMonsterWithEffect(Monster monster, int effect, Point pos);
        void spawnReactor(Reactor reactor);
        void spawnSummon(Summon summon);
        void startEvent();
        void startEvent(Player chr);
        void startMapEffect(string msg, int itemId, long time = 30000);
        void toggleDrops();
        void toggleEnvironment(string ms);
        Player? unclaimOwnership();
        bool unclaimOwnership(Player? chr);
        void updatePartyItemDropsToNewcomer(Player newcomer, List<MapItem> partyItems);
        List<MapItem> updatePlayerItemDropsToParty(int partyid, int charid, List<Player> partyMembers, Player? partyLeaver);
        void warpEveryone(int to);
        void warpEveryone(int to, int pto);
        void warpOutByTeam(int team, int mapid);

        bool IsActive();
        void BroadcastAll(Action<Player> effectPlayer, int exceptId = -1);
        void SetupAreaBoss(string name, int bossId, int mobTime, List<object> points, string spawnMessage);
        void makeDisappearExpiredItemDrops();
        void unregisterItemDrop(MapItem mapItem);
        void ProcessItemMonitor();
    }
}
