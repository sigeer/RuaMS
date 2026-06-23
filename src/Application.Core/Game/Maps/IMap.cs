using Application.Core.Channel;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Life;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.scripting.Events.Instances;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using Application.Templates.Mob;
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
    public interface IMap : IAsyncDisposable, IClientMessenger, ILoopTickable, IActorInstance<IMap>
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
        bool UseRangedView { get; }
        #region Info
        void addSelfDestructive(Monster mob);
        void allowSummonState(bool b);
        Point calcDropPos(Point initial, Point fallback);
        float getRecovery();
        Task<IMap> getReturnMap();
        int getReturnMapId();
        int getSeats();
        IDictionary<string, int> getEnvironment();
        AbstractEventInstanceManager? getEventInstance();
        bool getEverlast();
        int getFieldLimit();
        int getForcedReturnId();
        Task<IMap> getForcedReturnMap();
        int FindFh(Point pos);
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
        void setMuted(bool mute);
        void setOxQuiz(bool b);
        Task setReactorState();
        Task addMonsterSpawn(int mobId, Point pos, int cy, int f, int fh, int rx0, int rx1, int mobTime, bool hide, int team);
        Task addMonsterSpawn(int mobId, Point pos, int mobTime, int team);
        #endregion

        #region MapObjects
        Task<bool> AddMapObject(IMapObject mapobject, Func<IChannelClient, Task>? packetbakery, bool allocateMabObjectId = true);
        Task ProcessMapObject(Func<IMapObject, bool> codition, Func<IMapObject, Task> action);
        IMapObject? getMapObject(int oid);
        List<IMapObject> getMapObjects();
        List<IMapObject> GetMapObjects(Func<IMapObject, bool> func);
        List<IMapObject> getMapObjectsInBox(Rectangle box, HashSet<MapObjectType> types);
        List<IMapObject> getMapObjectsInRange(Point from, double rangeSq, HashSet<MapObjectType> types);
        List<TObject> GetRequiredMapObjects<TObject>(MapObjectType type, Func<TObject, bool> func) where TObject : IMapObject;
        /// <summary>
        /// 清理掉落物、怪物、反应堆
        /// </summary>
        /// <returns></returns>
        Task clearMapObjects();
        /// <summary>
        /// 清理后立即生成怪物
        /// </summary>
        /// <returns></returns>
        Task resetMapObjects();
        Task resetPQ(int difficulty = 1);
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
        Task movePlayer(Player player, Point newPosition);
        /// <summary>
        /// 非玩家的可移动对象移动时
        /// </summary>
        /// <param name="mapObject"></param>
        Task MoveMapObject(AbstractAnimatedMapObject mapObject);
        bool IsMapObjectVisibleForPlayerCached(Player player, IMapObject mapObj);
        Task SetPlayerVisibleObject(Player chr, IMapObject mapObj, bool sendSpawnData = true);
        Task SetPlayerInvisibleObject(Player chr, IMapObject mapObj, bool sendDestroyData = true);
        Task removePlayer(Player chr);
        Task addPlayer(Player chr);
        #endregion

        #region Monster
        int countMonster(int id);
        int countMonster(int minid, int maxid);
        int countMonsters();
        int countBosses();

        Task ProcessMonster(Func<Monster, Task> action);

        Monster? getMonsterById(int id);
        Monster? getMonsterByOid(int oid);

        Task spawnFakeMonster(Monster monster);
        Task spawnFakeMonsterOnGroundBelow(MobTemplate mobData, Point pos, Action<Monster>? handleMob = null);
        Task spawnHorntailOnGroundBelow(Point targetPoint);
        /// <summary>
        /// 召唤扎昆（复合型Mob）
        /// </summary>
        /// <param name="targetPoint"></param>
        Task SpawnZakumOnGroundBelow(Point targetPoint);
        Task spawnMonster(Monster monster, int difficulty = 1, bool isPq = false);
        Task spawnMonsterOnGroundBelow(int id, int x, int y);
        Monster CreateMonster(MobTemplate mobData, Point pos);
        Task spawnMonsterOnGroundBelow(MobTemplate mobData, Point pos, Action<Monster>? handleMob = null);
        Task spawnDojoMonster(MobTemplate monster);
        Task spawnAllMonsterIdFromMapSpawnList(int id, int difficulty = 1, bool isPq = false);
        Task spawnAllMonstersFromMapSpawnList(int difficulty = 1, bool isPq = false);

        #endregion




        Task broadcastBossHpMessage(Monster mm, int bossHash, Packet packet, Point? rangedFrom = null);
        Task broadcastEnemyShip(bool state);

        Task broadcastMessage(Packet packet);
        Task broadcastMessage(Player source, Packet packet, bool repeatToSource, bool ranged = false);
        Task broadcastMessage(Player? source, Packet packet, Point rangedFrom);

        Task broadcastNightEffect();

        Task broadcastHorntailVictory();
        Task broadcastPinkBeanVictory(int channel);
        Task broadcastShip(bool state);
        Task checkMapOwnerActivity();
        bool claimOwnership(Player chr);


        void closeMapSpawnPoints();
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
        Task SpawnNpc(int npcId, Point pos);
        NPC? getNPCById(int id);
        bool containsNPC(int npcid);
        Task destroyNPC(int npcid);
        #endregion

        #region Portal
        Portal? getPortal(int portalid);
        Portal? getPortal(string portalname);
        #endregion

        Point? getPointBelow(Point pos);

        Portal getRandomPlayerSpawnpoint();
        int getSpawnedMonstersOnMap();

        Task instanceMapForceRespawn();
        Task instanceMapRespawn();

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
        Task<bool> isOwnershipRestricted(Player chr);
        bool isOxQuiz();
        bool isPurpleCPQMap();
        bool isStartingEventMap();
        #region Battle
        /// <summary>
        /// 杀死所有怪物，不会掉落物品，不重生
        /// </summary>
        Task killAllMonsters();
        /// <summary>
        /// 杀死所有怪物（友方单位除外）不会掉落物品，不重生
        /// </summary>
        Task killAllMonstersNotFriendly();
        /// <summary>
        /// 击杀友方单位
        /// </summary>
        /// <param name="mob"></param>
        Task killFriendlies(Monster mob);
        Task killMonster(int mobId, bool withDrops = false);

        /// <summary>
        /// 从地图上移除Mob
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="killer"></param>
        /// <param name="withDrops"></param>
        /// <param name="animation"></param>
        /// <param name="dropDelay"></param>
        Task RemoveMob(Monster? monster, ICombatantObject? killer, bool withDrops, int animation = 1, short dropDelay = 0);
        #endregion


        Task makeMonsterReal(Monster monster);


        Task<bool> RemoveMapObject(IMapObject obj, Func<Player, Task>? removePacketAction);
        void removeMonsterSpawn(int mobId, int x, int y);

        Task addPlayerPuppet(Player player);
        Task removePlayerPuppet(Player player);
        bool removeSelfDestructive(int mapobjectid);
        Task reportMonsterSpawnPoints(Player chr);



        Task respawn();
        void restoreMapSpawnPoints();
        Task sendNightEffect(Player chr);
        void setAllowSpawnPointInBox(bool allow, Rectangle box);

        Task spawnDoor(DoorObject door);
        Task spawnKite(Kite kite);
        Task spawnMesoDrop(int meso, Point position, IMapObject dropper, Player owner, bool playerDrop, DropType droptype, short delay = 0);
        Task spawnMist(Mist mist, int duration, bool poison, bool fake, bool recovery);


        Task spawnSummon(Summon summon);
        void startEvent();
        Task startEvent(Player chr);
        Task startMapEffect(string msg, int itemId, long time = 30000);
        void toggleDrops();
        Task toggleEnvironment(string ms);
        Task moveEnvironment(string ms, int type);
        Player? unclaimOwnership();
        bool unclaimOwnership(Player? chr);

        Task warpEveryone(int to);
        Task warpEveryone(int to, int pto);
        Task warpOutByTeam(int team, int mapid);

        /// <summary>
        /// 广播，无距离筛选
        /// </summary>
        /// <param name="effectPlayer"></param>
        /// <param name="exceptId"></param>
        Task BroadcastAll(Func<Player, Task> effectPlayer, int exceptId = -1);
        Task Broadcast(int exceptChrId, double rangeSq, Point? rangedFrom, Func<Player, Task> effectPlayer);
        Task SetupAreaBoss(string name, int bossId, int mobTime, List<RandomPoint> points, string spawnMessage);
        void ClearAreaBoss(string names);

        #region Reactors
        Task spawnReactor(Reactor reactor);
        int countReactors();
        Task destroyReactor(int oid);
        Task destroyReactors(int first, int last);
        Reactor? getReactorById(int Id);
        Reactor? getReactorByName(string name);
        Reactor? getReactorByOid(int oid);
        List<IMapObject> getReactors();
        List<Reactor> getReactorsByIdRange(int first, int last);
        bool isAllReactorState(int reactorId, int state);
        Task resetReactors();
        Task resetReactors(List<Reactor> list);
        void shuffleReactors(int first = 0, int last = int.MaxValue);
        void shuffleReactors(List<object> list);
        List<Reactor> getAllReactors();
        Task TryHitReactorByMapItem(MapItem mapItem);
        bool CanHitReactor(MapItem mapItem);
        #endregion

        #region Drop
        Task updatePartyItemDropsToNewcomer(Player newcomer, List<MapItem> partyItems);
        Task<List<MapItem>> updatePlayerItemDropsToParty(int partyid, int charid, List<Player> partyMembers, Player? partyLeaver);
        Task DropItemDestroy(int itemId, Point dropperPos);
        Task dropFromFriendlyMonster(Player chr, Monster mob);
        Task dropFromReactor(Player chr, Reactor reactor, Item drop, Point dropPos, short questid, short delay = 0);
        Task DropItemFromMonsterBySteal(List<DropEntry> list, Player chr, Monster mob, short delay);
        Task pickItemDrop(Packet pickupPacket, MapItem mdrop);
        Task spawnItemDrop(IMapObject dropper, Player owner, Item item, Point pos, bool ffaDrop, bool playerDrop);
        int countItems();
        int getDroppedItemsCountById(int itemid);
        Task clearDrops();
        List<MapItem> getItems();
        #endregion
    }
}
