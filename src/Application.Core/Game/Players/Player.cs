using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Models;
using Application.Core.scripting.npc;
using Application.Shared.Objects;
using client;
using client.autoban;
using server;
using server.events;
using server.maps;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player : AbstractAnimatedMapObject, IAnimatedMapObject, IMapObject, IPlayerStats, IMapPlayer, ILife, IClientMessenger
    {
        public int Channel => CashShopModel.isOpened() ? -1 : ActualChannel;
        public int ActualChannel => Client.Channel;
        public IChannelClient Client { get; private set; }
        /// <summary>
        /// offlineclient or channelclient.player disposed
        /// </summary>
        public bool IsOnlined => Client.IsOnlined;

        public PlayerBag Bag { get; set; }
        public BuddyList BuddyList { get; set; }


        public PlayerKeyMap KeyMap { get; set; }
        public MapManager MapManager => Client.CurrentServer.getMapFactory();

        public List<FameLogObject> FameLogs { get; set; }


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

            BuddyList = new BuddyList(this);

            KeyMap = new(this);

            PlayerTrockLocation = new(this, 5, 10);

            Events = new Dictionary<string, Events>();
            SavedLocations = new(this);

            Bag = new PlayerBag(this);
            Monsterbook = new MonsterBook(this);
            CashShopModel = new CashShop(this);

            FameLogs = new();

            setStance(0);

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
            cancelBuffExpireTask();
            cancelDiseaseExpireTask();
            cancelSkillCooldownTask();
            cancelExpirationTask();
            cancelQuestExpirationTask();
        }
        public override int GetSourceId()
        {
            return Id;
        }

        public override string GetName()
        {
            return Name;
        }

        public void TypedMessage(int type, string messageKey, params string[] param)
        {
            if (type == -1)
            {
                Yellow(messageKey, param);
            }
            else if (type == -2)
            {
                sendPacket(PacketCreator.earnTitleMessage(GetMessageByKey(messageKey, param)));
            }
            sendPacket(PacketCommon.serverNotice(type, GetMessageByKey(messageKey, param)));
        }
        public void Notice(string key, params string[] param)
        {
            TypedMessage(0, key, param);
        }

        public void Popup(string key, params string[] param)
        {
            TypedMessage(1, key, param);
        }

        public void Dialog(string key, params string[] param)
        {
            TempConversation.Create(Client)?.RegisterTalk(GetMessageByKey(key, param));
        }

        public void Pink(string key, params string[] param)
        {
            TypedMessage(5, key, param);
        }

        public void LightBlue(string key, params string[] param)
        {
            TypedMessage(6, key, param);
        }

        public void LightBlue(Func<ClientCulture, string> action)
        {
            sendPacket(PacketCommon.serverNotice(6, action(Client.CurrentCulture)));
        }

        public void TopScrolling(string key, params string[] param)
        {
            sendPacket(PacketCommon.serverMessage(GetMessageByKey(key, param)));
        }

        public void Yellow(string key, params string[] param)
        {
            sendPacket(PacketCommon.SendYellowTip(GetMessageByKey(key, param)));
        }
    }
}
