using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using client;
using client.autoban;
using client.inventory;
using client.keybind;
using constants.game;
using constants.id;
using server.events;
using server.maps;
using System.Numerics;

namespace Application.Core.Game.Players
{
    public partial class Player : AbstractAnimatedMapObject, IPlayer
    {
        public int? Channel { get; set; }

        public IClient Client { get; private set; }
        public bool IsOnlined => Client.IsGameOnlined;

        public PlayerBag Bag { get; set; }
        public BuddyList BuddyList { get; set; }

        public List<int> TrockMaps { get; set; }
        public List<int> VipTrockMaps { get; set; }
        public Dictionary<int, KeyBinding> KeyMap { get; set; }
        public MapManager MapManager => Client.getChannelServer().getMapFactory();

        public object SaveToDBLock { get; set; } = new object();

        public event EventHandler<IPlayer>? OnLevelUp;
        public event EventHandler<IPlayer>? OnJobUpdate;
        public event EventHandler<IPlayer>? OnLodgedUpdate;

        public Player(int world, int accountId, int hp, int mp, int str, int dex, int @int, int luk, Job job, int level) : this()
        {
            World = world;
            AccountId = accountId;
            Hp = hp;
            Maxhp = hp;
            Mp = mp;
            Maxmp = mp;
            Str = str;
            Dex = dex;
            Int = @int;
            Luk = luk;
            JobId = (int)job;
            Level = level;
        }

        public Player(IClient client)
        {
            Client = client;

            AutobanManager = new AutobanManager(this);
            Skills = new Dictionary<Skill, SkillEntry>();
            SkillMacros = new SkillMacro[5];

            MesoValue = new AtomicInteger();
            ExpValue = new AtomicInteger();
            GachaExpValue = new AtomicInteger();

            BuddyList = new BuddyList(this, 20);
            LastFameCIds = new List<int>();

            KeyMap = new Dictionary<int, KeyBinding>();
            LoadKeyMapDefault();

            TrockMaps = Enumerable.Range(0, 5).Select(x => MapId.NONE).ToList();
            VipTrockMaps = Enumerable.Range(0, 10).Select(x => MapId.NONE).ToList();

            Events = new Dictionary<string, Events>();
            SavedLocations = new SavedLocation[Enum.GetValues<SavedLocationType>().Length];

            Bag = new PlayerBag(this);
            Monsterbook = new MonsterBook();

            setStance(0);

            quests = new();
            setPosition(new Point(0, 0));

            RegisterStatsListener();
        }

        public Player() : this(new OfflineClient())
        {

        }

        public void Reload()
        {
            BuddyList.setCapacity(BuddyCapacity);

            Bag[InventoryType.EQUIP].setSlotLimit(Equipslots);
            Bag[InventoryType.USE].setSlotLimit(Useslots);
            Bag[InventoryType.SETUP].setSlotLimit(Setupslots);
            Bag[InventoryType.ETC].setSlotLimit(Etcslots);
            Bag[InventoryType.CASH].setSlotLimit(PlayerBag.DEFAULT_CASH_BAG_SIZE);
        }

        public void LoadKeyMapDefault()
        {
            KeyMap.Clear();
            var selectedKey = GameConstants.getCustomKey(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            var selectedType = GameConstants.getCustomType(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            var selectedAction = GameConstants.getCustomAction(YamlConfig.config.server.USE_CUSTOM_KEYSET);
            for (int i = 0; i < selectedKey.Length; i++)
            {
                KeyMap.AddOrUpdate(selectedKey[i], new KeyBinding(selectedType[i], selectedAction[i]));
            }
        }

        public IPlayer generateCharacterEntry()
        {
            return this;
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
    }
}
