using Application.Core.Channel;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.scripting.Events.Instances;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using Application.Templates.Npc;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;
using client.inventory;
using net.server.coordinator.world;
using server.events.gm;
using server.life;
using server.maps;

namespace Application.Core.Game.Maps
{
    public interface IMap : IDisposable, IClientMessenger, ILoopTickable, IActorInstance<IMap>
    {
        int Id { get; }
        bool IsPirateDocked { get; }
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
        AbstractEventInstanceManager? EventInstanceManager { get; }
        MapEffect? MapEffect { get; set; }
        long RespawnInterval { get; set; }

        /// <summary>
        /// 地图大小超过视距*2（使用视野裁剪）
        /// </summary>
        public bool IsLargeMap { get; }
        #region Info
        void addSelfDestructive(Monster mob);
        void allowSummonState(bool b);
        Point calcDropPos(Point initial, Point fallback);
        float getRecovery();
        IMap getReturnMap();
        int getReturnMapId();
        int getSeats();
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
        Rectangle getMapArea();
        string getMapName();
        string getStreetName();
        bool getSummonState();
        int getTimeLimit();
        bool hasClock();
        void setDocked(bool isDocked);
        void setEventStarted(bool @event);
        void setMapName(string mapName);
        void setMuted(bool mute);
        void setOxQuiz(bool b);
        void setReactorState();
        void setStreetName(string streetName);

        void addMonsterSpawn(int mobId, Point pos, int cy, int f, int fh, int rx0, int rx1, int mobTime, bool hide, int team, SpawnPointTrigger act = SpawnPointTrigger.Killed);
        void addMonsterSpawn(int mobId, Point pos, int mobTime, int team, SpawnPointTrigger act = SpawnPointTrigger.Killed);
        #endregion

        #region MapObjects
        bool AddMapObject(IMapObject mapobject, Action<IChannelClient>? packetbakery, bool allocateMabObjectId = true);
        void ProcessMapObject(Func<IMapObject, bool> codition, Action<IMapObject> action);
        IMapObject? getMapObject(int oid);
        List<IMapObject> getMapObjects();
        List<IMapObject> GetMapObjects(Func<IMapObject, bool> func);
        List<IMapObject> getMapObjectsInBox(Rectangle box, HashSet<MapObjectType> types);
        List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, HashSet<MapObjectType> types);
        List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type, Func<TObject, bool> func) where TObject : IMapObject;
        void clearMapObjects();
        #endregion

        #region Players
        Dictionary<int, Player> getMapPlayers();
        Player? getCharacterById(int id);
        Player? getCharacterByName(string name);
        List<Player> getAllPlayers();
        int countPlayers();
        int countAlivePlayers();
        int getNumPlayersInArea(int index);
        int getNumPlayersInRect(Rectangle rect);
        List<Player> getPlayersInRange(Rectangle box);
        void movePlayer(Player player, Point newPosition);
        /// <summary>
        /// 非玩家的可移动对象移动时
        /// </summary>
        /// <param name="mapObject"></param>
        void MoveMapObject(AbstractAnimatedMapObject mapObject);
        void removePlayer(Player chr);
        void addPlayer(Player chr);
        #endregion

        #region Monster
        int countMonster(int id);
        int countMonster(int minid, int maxid);
        int countMonsters();
        int countBosses();

        void ProcessMonster(Action<Monster> action);

        Monster? getMonsterById(int id);
        Monster? getMonsterByOid(int oid);

        void spawnFakeMonster(Monster monster);
        void spawnFakeMonsterOnGroundBelow(MonsterCore mobData, Point pos, Action<Monster>? handleMob = null);
        void spawnHorntailOnGroundBelow(Point targetPoint);
        /// <summary>
        /// 召唤扎昆（复合型Mob）
        /// </summary>
        /// <param name="targetPoint"></param>
        void SpawnZakumOnGroundBelow(Point targetPoint);
        void spawnMonster(Monster monster, int difficulty = 1, bool isPq = false);
        void spawnMonsterOnGroundBelow(int id, int x, int y);
        Monster CreateMonster(MonsterCore mobData, Point pos);
        void spawnMonsterOnGroundBelow(MonsterCore mobData, Point pos, Action<Monster>? handleMob = null);
        void spawnDojoMonster(MonsterCore monster);
        void spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false);
        void spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false);

        #endregion


        void addPlayerPuppet(Player player);

        void broadcastBalrogVictory(string leaderName);
        void broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null);
        void broadcastEnemyShip(bool state);

        void broadcastMessage(Packet packet);
        void broadcastMessage(Player source, Packet packet, bool repeatToSource, bool ranged = false);
        void broadcastMessage(Player? source, Packet packet, Point rangedFrom);

        void broadcastNightEffect();

        void broadcastHorntailVictory();
        void broadcastPinkBeanVictory(int channel);
        void broadcastShip(bool state);

        bool canDeployDoor(Point pos);
        void checkMapOwnerActivity();
        bool claimOwnership(Player chr);


        void closeMapSpawnPoints();
        void dropMessage(int type, string message);
        bool eventStarted();
        Portal? findClosestPlayerSpawnpoint(Point from);
        Portal? findClosestPortal(Point from);
        SpawnPoint? findClosestSpawnpoint(Point from);
        Portal? findClosestTeleportPortal(Point from);
        Portal? findMarketPortal();
        void generateMapDropRangeCache();
        MonsterAggroCoordinator getAggroCoordinator();


        Rectangle getArea(int index);
        List<Rectangle> getAreas();
        WorldChannel getChannelServer();

        Portal? getDoorPortal(int doorid);
        bool TryGetEffectiveDoorPortal(out MysticDoorPortal? portal);


        #region Npc
        NPC CreateNPC(NpcTemplate template, Point pos);
        void SpawnNpc(int npcId, Point pos);
        NPC? getNPCById(int id);
        bool containsNPC(int npcid);
        void destroyNPC(int npcid);
        #endregion

        #region Portal
        Portal? getPortal(int portalid);
        Portal? getPortal(string portalname);
        #endregion

        Point? getPointBelow(Point pos);

        Portal getRandomPlayerSpawnpoint();
        int getSpawnedMonstersOnMap();

        void instanceMapForceRespawn();
        void instanceMapRespawn();

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
        #region Battle
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
        /// 从地图上移除Mob
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="killer"></param>
        /// <param name="withDrops"></param>
        /// <param name="animation"></param>
        /// <param name="dropDelay"></param>
        void RemoveMob(Monster? monster, ICombatantObject? killer, bool withDrops, int animation = 1, short dropDelay = 0);
        #endregion


        void makeMonsterReal(Monster monster);
        void moveEnvironment(string ms, int type);

        bool RemoveMapObject(IMapObject obj, Action<Player>? removePacketAction);
        void removeMonsterSpawn(int mobId, int x, int y);

        void removePlayerPuppet(Player player);
        bool removeSelfDestructive(int mapobjectid);
        void reportMonsterSpawnPoints(Player chr);
        void resetFully();
        void resetMapObjects();
        void resetPQ(int difficulty = 1);

        void respawn();
        void restoreMapSpawnPoints();
        void sendNightEffect(Player chr);
        void setAllowSpawnPointInBox(bool allow, Rectangle box);

        void spawnDoor(DoorObject door);
        void spawnKite(Kite kite);
        void spawnMesoDrop(int meso, Point position, IMapObject dropper, Player owner, bool playerDrop, DropType droptype, short delay = 0);
        void spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery);


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

        /// <summary>
        /// 广播，无距离筛选
        /// </summary>
        /// <param name="effectPlayer"></param>
        /// <param name="exceptId"></param>
        void BroadcastAll(Action<Player> effectPlayer, int exceptId = -1);
        void Broadcast(int exceptChrId, double rangeSq, Point? rangedFrom, Action<Player> effectPlayer);
        void SetupAreaBoss(string name, int bossId, int mobTime, List<RandomPoint> points, string spawnMessage);

        #region Reactors
        void spawnReactor(Reactor reactor);
        int countReactors();
        void destroyReactor(int oid);
        void destroyReactors(int first, int last);
        Reactor? getReactorById(int Id);
        Reactor? getReactorByName(string name);
        Reactor? getReactorByOid(int oid);
        List<IMapObject> getReactors();
        List<Reactor> getReactorsByIdRange(int first, int last);
        bool isAllReactorState(int reactorId, int state);
        void resetReactors();
        void resetReactors(List<Reactor> list);
        void shuffleReactors(int first = 0, int last = int.MaxValue);
        void shuffleReactors(List<object> list);
        List<Reactor> getAllReactors();
        void TryHitReactorByMapItem(MapItem mapItem);
        bool CanHitReactor(MapItem mapItem);
        #endregion

        #region Drop
        void DropItemDestroy(int itemId, Point dropperPos);
        void dropFromFriendlyMonster(Player chr, Monster mob);
        void dropFromReactor(Player chr, Reactor reactor, Item drop, Point dropPos, short questid, short delay = 0);
        void DropItemFromMonsterBySteal(List<DropEntry> list, Player chr, Monster mob, short delay);
        void pickItemDrop(Packet pickupPacket, MapItem mdrop);
        void spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, bool ffaDrop, bool playerDrop);
        int countItems();
        int getDroppedItemsCountById(int itemid);
        void clearDrops();
        List<MapItem> getItems();
        #endregion
    }
}
