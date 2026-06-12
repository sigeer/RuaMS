using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Models;
using Application.Core.scripting.npc;
using Application.Shared.Objects;
using Application.Utility.Tickables;
using client;
using client.autoban;
using server;
using server.events;
using server.life;
using server.maps;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player : AbstractAnimatedMapObject, IAnimatedMapObject, IMapObject, IPlayerStats, IMapPlayer, ILife, IClientPlayer, ICombatantObject, ILoopTickable
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
        public List<FameLogObject> FameLogs { get; set; }


        public Player(IChannelClient client, IMap map, Portal portal, SyncProto.PlayerGetterDto o) : base(map, portal.getPosition(), 0)
        {
            Client = client;

            HP = o.Character.Hp;
            MaxHP = o.Character.Maxhp;
            MP = o.Character.Mp;
            MaxMP = o.Character.Maxmp;
            RemainingSp = o.Character.Sp.Split(",").Select(int.Parse).ToArray();

            AutobanManager = new AutobanManager(this);
            Skills = new(this);
            SkillMacros = new SkillMacro[5];

            MesoValue = new AtomicInteger(o.Character.Meso);
            ExpValue = new AtomicInteger(o.Character.Exp);
            GachaExpValue = new AtomicInteger(o.Character.Gachaexp);

            BuddyList = new BuddyList(this, o.Character.BuddyCapacity);

            KeyMap = new(this);

            PlayerTrockLocation = new(this, 5, 10);

            Events = new Dictionary<string, Events>();
            SavedLocations = new(this);

            Bag = new PlayerBag(this, o.Character.Equipslots, o.Character.Useslots, o.Character.Setupslots, o.Character.Etcslots);
            Monsterbook = new MonsterBook(this);
            CashShopModel = new CashShop(this);

            FameLogs = new();


            if (Client is not OfflineClient)
            {
                AddWorldWatcher();

                UpdateActualRate();
            }
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
            if (string.IsNullOrEmpty(messageKey))
            {
                return;
            }

            if (type == -1)
            {
                sendPacket(PacketCommon.SendYellowTip(GetMessageByKey(messageKey, param)));
            }
            else if (type == -2)
            {
                sendPacket(PacketCreator.earnTitleMessage(GetMessageByKey(messageKey, param)));
            }
            else if (type == -3)
            {
                TempConversation.Create(Client)?.RegisterTalk(GetMessageByKey(messageKey, param));
            }
            else if (type == 4)
            {
                sendPacket(PacketCommon.serverMessage(GetMessageByKey(messageKey, param)));
            }
            else
            {
                sendPacket(PacketCommon.serverNotice(type, GetMessageByKey(messageKey, param)));
            }
        }
        public void Notice(string key, params string[] param) => TypedMessage(0, key, param);

        public void Popup(string key, params string[] param) => TypedMessage(1, key, param);

        public void TopScrolling(string key, params string[] param) => TypedMessage(4, key, param);

        public void Pink(string key, params string[] param) => TypedMessage(5, key, param);

        public void LightBlue(string key, params string[] param) => TypedMessage(6, key, param);

        public void Yellow(string key, params string[] param) => TypedMessage(-1, key, param);
        public void EarnTitle(string key, params string[] param) => TypedMessage(-2, key, param);
        public void Dialog(string key, params string[] param) => TypedMessage(-3, key, param);

        public void LightBlue(Func<ClientCulture, string> action)
        {
            sendPacket(PacketCommon.serverNotice(6, action(Client.CurrentCulture)));
        }


        public long Period => 1_500;

        public long Next { get; private set; }

        public long MapDamagePeriod { get; } = YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL * YamlConfig.config.server.MAP_DAMAGE_OVERTIME_COUNT;
        public long MapDamageNext { get; set; }

        long _diseaseAnnounceNext;
        long _diseaseAnnouncePeriod = YamlConfig.config.server.UPDATE_INTERVAL;

        public TickableStatus Status { get; private set; }

        public void OnTick(long now)
        {
            foreach (var item in getAllStatups().OfType<ITickable>())
            {
                item.OnTick(now);
            }

            Bag.OnTick(now);

            MountModel?.OnTick(now);

            foreach (var item in pets)
            {
                item?.OnTick(now);
            }

            if (_mapEffect != null)
            {
                _mapEffect.OnTick(now);

                if (_mapEffect.Status == TickableStatus.Remove)
                {
                    sendPacket(_mapEffect.makeDestroyData());
                    _mapEffect = null;
                }
            }

            if (MapModel.getHPDec() > 0 && MapDamageNext <= now)
            {
                doHurtHp();
                MapDamageNext = now + MapDamagePeriod;
            }

            if (_diseaseAnnounceNext <= now)
            {
                announceDiseases();
                collectDiseases();

                _diseaseAnnounceNext = now + _diseaseAnnouncePeriod;
            }

            if (Next <= now)
            {
                ClearExpiredBuffs();
                ClearExpiredDisease(now);
                ClearExpiredSkills(now);
                ClearExpiredQuests(now);
                ClearExpiredSkillCooldown(now);

                Next = now + Period;
            }
        }

        public void OpenNpc(int npcId, string? customeScript = null)
        {
            var script = customeScript ?? LifeFactory.Instance.GetNPCTemplateTrust(npcId)?.Script;
            if (script != null)
            {
                var npcObj = MapModel.getNPCById(npcId);
                _ = Client.CurrentServer.NodeService.ScriptManager.StartNpcConversation(Client, npcId, MapModel.getNPCById(npcId), script);
            }

        }
    }
}
