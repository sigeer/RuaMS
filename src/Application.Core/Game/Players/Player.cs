using Application.Core.Game.Maps;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using client;
using client.autoban;
using server;
using server.events;
using server.maps;

namespace Application.Core.Game.Players
{
    public partial class Player : AbstractAnimatedMapObject, IPlayer
    {
        public int Channel => awayFromWorld ? -1 : Client.CurrentServer.getId();
        public int ActualChannel  => Client.CurrentServer.getId();
        public IChannelClient Client { get; private set; }
        public bool IsOnlined => Client.IsOnlined;

        public PlayerBag Bag { get; set; }
        public BuddyList BuddyList { get; set; }


        public PlayerKeyMap KeyMap { get; set; }
        public MapManager MapManager => Client.CurrentServer.getMapFactory();

        public object SaveToDBLock { get; set; } = new object();

        public event EventHandler<IPlayer>? OnLevelUp;
        public event EventHandler<IPlayer>? OnJobUpdate;
        public event EventHandler<IPlayer>? OnLodgedUpdate;

        public Player(int world, int accountId, int hp, int mp, int str, int dex, int @int, int luk, Job job, int level) : this()
        {
            World = world;
            AccountId = accountId;
            HP = hp;
            MaxHP = hp;
            MP = mp;
            MaxMP = mp;
            Str = str;
            Dex = dex;
            Int = @int;
            Luk = luk;
            JobModel = job;
            Level = level;
        }

        public Player(IChannelClient client)
        {
            Client = client;

            AutobanManager = new AutobanManager(this);
            Skills = new(this);
            SkillMacros = new SkillMacro[5];

            MesoValue = new AtomicInteger();
            ExpValue = new AtomicInteger();
            GachaExpValue = new AtomicInteger();

            BuddyList = new BuddyList(this, 20);
            LastFameCIds = new List<int>();

            KeyMap = new(this);

            PlayerTrockLocation = new(this, 5, 10);

            Events = new Dictionary<string, Events>();
            SavedLocations = new(this);

            Bag = new PlayerBag(this);
            Monsterbook = new MonsterBook(this);
            CashShopModel = new CashShop(this);

            setStance(0);

            quests = new();
            setPosition(new Point(0, 0));

            if (Client is not OfflineClient)
            {
                AddWorldWatcher();

                UpdateActualRate();
            }
        }

        public Player() : this(new OfflineClient())
        {

        }

        public void Reload()
        {
            BuddyList.Capacity = BuddyCapacity;
            Bag[InventoryType.EQUIP].setSlotLimit(Equipslots);
            Bag[InventoryType.USE].setSlotLimit(Useslots);
            Bag[InventoryType.SETUP].setSlotLimit(Setupslots);
            Bag[InventoryType.ETC].setSlotLimit(Etcslots);
            Bag[InventoryType.CASH].setSlotLimit(BagConfig.CashSize);
        }


        public void StartPlayerTask()
        {
            buffExpireTask();
            diseaseExpireTask();
            skillCooldownTask();
            expirationTask();
            questExpirationTask();
        }

        public void StopPlayerTask()
        {
            cancelAllBuffs(true);
            cancelAllDebuffs();
            cancelBuffExpireTask();
            cancelDiseaseExpireTask();
            cancelSkillCooldownTask();
            cancelExpirationTask();

            forfeitExpirableQuests();
            cancelQuestExpirationTask();
        }
        public void SetFly(bool v)
        {
            Client.CurrentServerContainer.SetFly(Id, v);
        }
    }
}
