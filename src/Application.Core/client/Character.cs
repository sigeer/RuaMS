/* 
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any otheer version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; witout even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.


 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Application.Core.EF.Entities;
using Application.Core.model;
using Application.Core.scripting.Event;
using client.autoban;
using client.creator;
using client.inventory;
using client.inventory.manipulator;
using client.keybind;
using client.newyear;
using client.processor.action;
using client.processor.npc;
using constants.game;
using constants.id;
using constants.inventory;
using constants.skills;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using net.packet;
using net.server;
using net.server.coordinator.world;
using net.server.guild;
using net.server.services.task.world;
using net.server.services.type;
using net.server.world;
using scripting;
using scripting.Event;
using scripting.item;
using server;
using server.events;
using server.events.gm;
using server.life;
using server.maps;
using server.minigame;
using server.partyquest;
using server.quest;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using tools;
using tools.packets;
using static client.inventory.Equip;
using static server.ItemInformationProvider;
using static server.maps.MiniGame;

namespace client;

public class Character : AbstractCharacterObject
{
    private ILogger log;
    private static string LEVEL_200 = "[Congrats] {0} has reached Level {1}! Congratulate {2} on such an amazing achievement!";

    private int world;
    private int accountid, id, level;
    private int rank, rankMove, jobRank, jobRankMove;
    private int gender, hair, face;
    private int fame, quest_fame;
    private int initialSpawnPoint;
    private int mapid;
    private int currentPage, currentType = 0, currentTab = 1;
    private int itemEffect;
    private int guildid, guildRank, allianceRank;
    private int messengerposition = 4;
    private int slots = 0;
    private int energybar;
    private int _gmLevel;
    private int ci = 0;
    private FamilyEntry? familyEntry;
    private int familyId;
    private int bookCover;
    private int battleshipHp = 0;
    private int mesosTraded = 0;
    private int possibleReports = 10;
    private int ariantPoints, dojoPoints, vanquisherStage, dojoStage, dojoEnergy, vanquisherKills;
    private int expRate = 1, mesoRate = 1, dropRate = 1, expCoupon = 1, mesoCoupon = 1, dropCoupon = 1;
    private int omokwins, omokties, omoklosses, matchcardwins, matchcardties, matchcardlosses;
    private int owlSearch;
    private long lastfametime, lastUsedCashItem, lastExpression = 0, lastHealed, lastDeathtime, jailExpiration = -1;
    private int localstr, localdex, localluk, localint_, localmagic, localwatk;
    private int equipmaxhp, equipmaxmp, equipstr, equipdex, equipluk, equipint_, equipmagic, equipwatk, localchairhp, localchairmp;
    private int localchairrate;
    private bool hidden, equipchanged = true, berserk, _hasMerchant, hasSandboxItem = false, whiteChat = false, canRecvPartySearchInvite = true;
    private bool equippedMesoMagnet = false, equippedItemPouch = false, equippedPetItemIgnore = false;
    private bool usedSafetyCharm = false;
    private float autopotHpAlert, autopotMpAlert;
    private int linkedLevel = 0;
    private string? linkedName = null;
    private bool finishedDojoTutorial;
    private bool usedStorage = false;
    private string name;
    private string? chalktext;
    private string commandtext;
    private string dataString;
    private string? search = null;
    private AtomicBoolean mapTransitioning = new AtomicBoolean(true);  // player client is currently trying to change maps or log in the game map
    private AtomicBoolean awayFromWorld = new AtomicBoolean(true);  // player is online, but on cash shop or mts
    private AtomicInteger exp = new AtomicInteger();
    private AtomicInteger gachaexp = new AtomicInteger();
    private AtomicInteger meso = new AtomicInteger();
    private AtomicInteger chair = new AtomicInteger(-1);
    private long totalExpGained = 0;
    private int merchantmeso;
    private BuddyList buddylist;
    private EventInstanceManager? eventInstance = null;
    private HiredMerchant? hiredMerchant = null;
    private Client client;
    private GuildCharacter? mgc = null;
    private PartyCharacter? mpc = null;
    private Inventory[] inventory;
    private Job job = Job.BEGINNER;
    private Messenger? messenger = null;
    private MiniGame miniGame;
    private RockPaperScissor? rps;
    private Mount? maplemount;
    private Party? party;
    private Pet?[] pets = new Pet?[3];
    private PlayerShop? playerShop = null;
    private Shop? shop = null;
    private SkinColor skinColor = SkinColor.NORMAL;
    private Storage? storage = null;
    private Trade? trade = null;
    private MonsterBook monsterbook;
    private CashShop cashshop;
    private HashSet<NewYearCardRecord> newyears = new();
    private SavedLocation?[] savedLocations;
    private SkillMacro[] skillMacros = new SkillMacro[5];
    private List<int> lastmonthfameids;
    private List<WeakReference<MapleMap>> lastVisitedMaps = new();
    private WeakReference<MapleMap?> ownedMap = new WeakReference<MapleMap?>(null);
    private Dictionary<short, QuestStatus> quests;
    private ConcurrentDictionary<Monster, int> controlled = new();
    private Dictionary<int, string> entered = new();
    private ConcurrentDictionary<MapObject, int> visibleMapObjects = new ConcurrentDictionary<MapObject, int>();
    private Dictionary<Skill, SkillEntry> skills = new();
    private Dictionary<int, int> activeCoupons = new();
    private Dictionary<int, int> activeCouponRates = new();
    private Dictionary<BuffStat, BuffStatValueHolder> effects = new();
    private Dictionary<BuffStat, sbyte> buffEffectsCount = new();
    private Dictionary<Disease, long> diseaseExpires = new();
    private Dictionary<int, Dictionary<BuffStat, BuffStatValueHolder>> buffEffects = new(); // non-overriding buffs thanks to Ronan
    private Dictionary<int, long> buffExpires = new();
    private Dictionary<int, KeyBinding> keymap = new();
    private Dictionary<int, Summon> summons = new();
    private Dictionary<int, CooldownValueHolder> coolDowns = new();
    private Dictionary<Disease, KeyValuePair<DiseaseValueHolder, MobSkill>> diseases = new();
    private byte[] m_aQuickslotLoaded;
    private QuickslotBinding m_pQuickslotKeyMapped;
    private Door? pdoor = null;
    private Dictionary<Quest, long> questExpirations = new();

    private ScheduledFuture? dragonBloodSchedule;
    private ScheduledFuture? hpDecreaseTask;
    private ScheduledFuture? beholderHealingSchedule, beholderBuffSchedule, berserkSchedule;
    private ScheduledFuture? _skillCooldownTask = null;
    private ScheduledFuture? _buffExpireTask = null;
    private ScheduledFuture? itemExpireTask = null;
    private ScheduledFuture? _diseaseExpireTask = null;
    private ScheduledFuture? questExpireTask = null;
    private ScheduledFuture? recoveryTask = null;
    private ScheduledFuture? extraRecoveryTask = null;
    private ScheduledFuture? chairRecoveryTask = null;
    private ScheduledFuture? pendantOfSpirit = null; //1122017
    private ScheduledFuture? cpqSchedule = null;

    private object chrLock = new object();
    private object evtLock = new object();
    private object petLock = new object();
    private object prtLock = new object();

    private Dictionary<int, HashSet<int>> excluded = new();
    private HashSet<int> excludedItems = new();
    private HashSet<int> disabledPartySearchInvites = new();
    private static string[] ariantroomleader = new string[3];
    private static int[] ariantroomslot = new int[3];
    private long portaldelay = 0, lastcombo = 0;
    private short combocounter = 0;
    private List<string> blockedPortals = new();
    private Dictionary<short, string> area_info = new();
    private AutobanManager _autoban;
    private bool isbanned = false;
    private bool blockCashShop = false;
    private bool allowExpGain = true;
    private byte pendantExp = 0, lastmobcount = 0;
    sbyte doorSlot = -1;
    private List<int> trockmaps = new();
    private List<int> viptrockmaps = new();
    private Dictionary<string, Events> events = new();
    private PartyQuest? partyQuest = null;
    private List<KeyValuePair<DelayedQuestUpdate, object[]>> npcUpdateQuests = new();
    private Dragon? dragon = null;
    private Ring? marriageRing;
    private int marriageItemid = -1;
    private int partnerId = -1;
    private List<Ring> crushRings = new();
    private List<Ring> friendshipRings = new();
    private bool loggedIn = false;
    private bool useCS;  //chaos scroll upon crafting item.
    private long npcCd;
    private int newWarpMap = -1;
    private bool canWarpMap = true;  //only one "warp" must be used per call, and this will define the right one.
    private int canWarpCounter = 0;     //counts how many times "inner warps" have been called.
    private byte extraHpRec = 0, extraMpRec = 0;
    private short extraRecInterval;
    private int targetHpBarHash = 0;
    private long targetHpBarTime = 0;
    private long nextWarningTime = 0;
    private DateTimeOffset lastExpGainTime;
    private bool pendingNameChange; //only used to change name on logout, not to be relied upon elsewhere
    private long loginTime;
    private bool chasing = false;

    ReaderWriterLockSlim chLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Character()
    {
        log = LogFactory.GetLogger($"Account/Account_{getAccountID()}/Character_{getId()}");
        var listener = new CharacterListener();
        listener.onHpChanged = (oldHp) =>
        {
            hpChangeAction(oldHp);
        };
        listener.onHpmpPoolUpdate = () =>
        {
            var hpmpupdate = recalcLocalStats();
            foreach (var p in hpmpupdate)
            {
                statUpdates.AddOrUpdate(p.Key, p.Value);
            }

            if (hp > localmaxhp)
            {
                setHp(localmaxhp);
                statUpdates.AddOrUpdate(Stat.HP, hp);
            }

            if (mp > localmaxmp)
            {
                setMp(localmaxmp);
                statUpdates.AddOrUpdate(Stat.MP, mp);
            }
        };
        listener.onStatUpdate = () =>
        {
            recalcLocalStats();
        };
        listener.onAnnounceStatPoolUpdate = () =>
        {
            List<KeyValuePair<Stat, int>> statup = new(8);
            foreach (var s in statUpdates)
            {
                statup.Add(new(s.Key, s.Value));
            }

            sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
        };
        setListener(listener);

        useCS = false;

        setStance(0);
        var typeList = Enum.GetValues<InventoryType>();
        inventory = new Inventory[typeList.Length];
        savedLocations = new SavedLocation[Enum.GetValues<SavedLocationType>().Length];

        for (int i = 0; i < typeList.Length; i++)
        {
            var type = typeList[i];
            byte b = 24;
            if (type == InventoryType.CASH)
            {
                b = 96;
            }
            inventory[i] = new Inventory(this, type, b);
        }
        inventory[InventoryType.CANHOLD.ordinal()] = new InventoryProof(this);

        for (int i = 0; i < Enum.GetValues<SavedLocationType>().Length; i++)
        {
            savedLocations[i] = null;
        }
        quests = new();
        setPosition(new Point(0, 0));
    }
    private static Job getJobStyleInternal(int jobid, byte opt)
    {
        int jobtype = jobid / 100;

        if (jobtype == Job.WARRIOR.getId() / 100 || jobtype == Job.DAWNWARRIOR1.getId() / 100 || jobtype == Job.ARAN1.getId() / 100)
        {
            return (Job.WARRIOR);
        }
        else if (jobtype == Job.MAGICIAN.getId() / 100 || jobtype == Job.BLAZEWIZARD1.getId() / 100 || jobtype == Job.EVAN1.getId() / 100)
        {
            return (Job.MAGICIAN);
        }
        else if (jobtype == Job.BOWMAN.getId() / 100 || jobtype == Job.WINDARCHER1.getId() / 100)
        {
            if (jobid / 10 == Job.CROSSBOWMAN.getId() / 10)
            {
                return (Job.CROSSBOWMAN);
            }
            else
            {
                return (Job.BOWMAN);
            }
        }
        else if (jobtype == Job.THIEF.getId() / 100 || jobtype == Job.NIGHTWALKER1.getId() / 100)
        {
            return (Job.THIEF);
        }
        else if (jobtype == Job.PIRATE.getId() / 100 || jobtype == Job.THUNDERBREAKER1.getId() / 100)
        {
            if (opt == 0x80)
            {
                return (Job.BRAWLER);
            }
            else
            {
                return (Job.GUNSLINGER);
            }
        }

        return (Job.BEGINNER);
    }

    public Job getJobStyle(byte opt)
    {
        return getJobStyleInternal(this.getJob().getId(), opt);
    }

    public Job getJobStyle()
    {
        return getJobStyle((byte)((this.getStr() > this.getDex()) ? 0x80 : 0x40));
    }

    public static Character GetDefaultCharacter(int world, int accountId)
    {
        Character ret = new Character();
        ret.setGMLevel(0);
        ret.hp = (50);
        ret.setMaxHp(50);
        ret.mp = 5;
        ret.setMaxMp(5);
        ret.str = 12;
        ret.dex = 5;
        ret.int_ = 4;
        ret.luk = 4;
        ret.map = null;
        ret.job = Job.BEGINNER;
        ret.level = 1;
        ret.world = world;
        ret.accountid = accountId;
        ret.buddylist = new BuddyList(20);
        ret.maplemount = null;
        ret.getInventory(InventoryType.EQUIP).setSlotLimit(24);
        ret.getInventory(InventoryType.USE).setSlotLimit(24);
        ret.getInventory(InventoryType.SETUP).setSlotLimit(24);
        ret.getInventory(InventoryType.ETC).setSlotLimit(24);

        // Select a keybinding method
        int[] selectedKey;
        int[] selectedType;
        int[] selectedAction;

        if (YamlConfig.config.server.USE_CUSTOM_KEYSET)
        {
            selectedKey = GameConstants.getCustomKey(true);
            selectedType = GameConstants.getCustomType(true);
            selectedAction = GameConstants.getCustomAction(true);
        }
        else
        {
            selectedKey = GameConstants.getCustomKey(false);
            selectedType = GameConstants.getCustomType(false);
            selectedAction = GameConstants.getCustomAction(false);
        }

        for (int i = 0; i < selectedKey.Length; i++)
        {
            ret.keymap.AddOrUpdate(selectedKey[i], new KeyBinding(selectedType[i], selectedAction[i]));
        }


        //to fix the map 0 lol
        for (int i = 0; i < 5; i++)
        {
            ret.trockmaps.Add(MapId.NONE);
        }
        for (int i = 0; i < 10; i++)
        {
            ret.viptrockmaps.Add(MapId.NONE);
        }

        return ret;
    }
    public static Character getDefault(Client c)
    {
        var m = GetDefaultCharacter(c.getWorld(), c.getAccID());
        m.client = c;

        return m;
    }

    public bool isLoggedinWorld()
    {
        return this.isLoggedin() && !this.isAwayFromWorld();
    }

    public bool isAwayFromWorld()
    {
        return awayFromWorld.Get();
    }

    public void setEnteredChannelWorld()
    {
        awayFromWorld.Set(false);
        client.getChannelServer().removePlayerAway(id);

        if (canRecvPartySearchInvite)
        {
            this.getWorldServer().getPartySearchCoordinator().attachPlayer(this);
        }
    }

    public void setAwayFromChannelWorld()
    {
        setAwayFromChannelWorld(false);
    }

    public void setDisconnectedFromChannelWorld()
    {
        setAwayFromChannelWorld(true);
    }

    private void setAwayFromChannelWorld(bool disconnect)
    {
        awayFromWorld.Set(true);

        if (!disconnect)
        {
            client.getChannelServer().insertPlayerAway(id);
        }
        else
        {
            client.getChannelServer().removePlayerAway(id);
        }
    }

    public void updatePartySearchAvailability(bool psearchAvailable)
    {
        if (psearchAvailable)
        {
            if (canRecvPartySearchInvite && getParty() == null)
            {
                this.getWorldServer().getPartySearchCoordinator().attachPlayer(this);
            }
        }
        else
        {
            if (canRecvPartySearchInvite)
            {
                this.getWorldServer().getPartySearchCoordinator().detachPlayer(this);
            }
        }
    }

    public bool toggleRecvPartySearchInvite()
    {
        canRecvPartySearchInvite = !canRecvPartySearchInvite;

        if (canRecvPartySearchInvite)
        {
            updatePartySearchAvailability(getParty() == null);
        }
        else
        {
            this.getWorldServer().getPartySearchCoordinator().detachPlayer(this);
        }

        return canRecvPartySearchInvite;
    }

    public bool isRecvPartySearchInviteEnabled()
    {
        return canRecvPartySearchInvite;
    }

    public void resetPartySearchInvite(int fromLeaderid)
    {
        disabledPartySearchInvites.Remove(fromLeaderid);
    }

    public void disablePartySearchInvite(int fromLeaderid)
    {
        disabledPartySearchInvites.Add(fromLeaderid);
    }

    public bool hasDisabledPartySearchInvite(int fromLeaderid)
    {
        return disabledPartySearchInvites.Contains(fromLeaderid);
    }

    public void setSessionTransitionState()
    {
        client.setCharacterOnSessionTransitionState(this.getId());
    }

    public bool getCS()
    {
        return useCS;
    }

    public void setCS(bool cs)
    {
        useCS = cs;
    }

    public long getNpcCooldown()
    {
        return npcCd;
    }

    public void setNpcCooldown(long d)
    {
        npcCd = d;
    }

    public void setOwlSearch(int id)
    {
        owlSearch = id;
    }

    public int getOwlSearch()
    {
        return owlSearch;
    }

    public void addCooldown(int skillId, long startTime, long length)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            this.coolDowns.AddOrUpdate(skillId, new CooldownValueHolder(skillId, startTime, length));
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void addCrushRing(Ring r)
    {
        crushRings.Add(r);
    }

    public Ring getRingById(int id)
    {
        foreach (Ring ring in getCrushRings())
        {
            if (ring.getRingId() == id)
            {
                return ring;
            }
        }
        foreach (Ring ring in getFriendshipRings())
        {
            if (ring.getRingId() == id)
            {
                return ring;
            }
        }

        if (marriageRing != null)
        {
            if (marriageRing.getRingId() == id)
            {
                return marriageRing;
            }
        }

        return null;
    }

    public int getMarriageItemId()
    {
        return marriageItemid;
    }

    public void setMarriageItemId(int itemid)
    {
        marriageItemid = itemid;
    }

    public int getPartnerId()
    {
        return partnerId;
    }

    public void setPartnerId(int partnerid)
    {
        partnerId = partnerid;
    }

    public int getRelationshipId()
    {
        return getWorldServer().getRelationshipId(id);
    }

    public bool isMarried()
    {
        return marriageRing != null && partnerId > 0;
    }

    public bool hasJustMarried()
    {
        var eim = getEventInstance();
        if (eim != null)
        {
            string prop = eim.getProperty("groomId");

            if (prop != null)
            {
                return (int.Parse(prop) == id || eim.getIntProperty("brideId") == id) &&
                        (mapid == MapId.CHAPEL_WEDDING_ALTAR || mapid == MapId.CATHEDRAL_WEDDING_ALTAR);
            }
        }

        return false;
    }

    public int addDojoPointsByMap(int mapid)
    {
        int pts = 0;
        if (dojoPoints < 17000)
        {
            pts = 1 + ((mapid - 1) / 100 % 100) / 6;
            if (!MapId.isPartyDojo(this.getMapId()))
            {
                pts++;
            }
            this.dojoPoints += pts;
        }
        return pts;
    }

    public void addFame(int famechange)
    {
        this.fame += famechange;
    }

    public void addFriendshipRing(Ring r)
    {
        friendshipRings.Add(r);
    }

    public void addMarriageRing(Ring? r)
    {
        marriageRing = r;
    }

    public void addMesosTraded(int gain)
    {
        this.mesosTraded += gain;
    }

    public void addPet(Pet pet)
    {
        Monitor.Enter(petLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] == null)
                {
                    pets[i] = pet;
                    return;
                }
            }
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public void addSummon(int id, Summon summon)
    {
        summons.AddOrUpdate(id, summon);

        if (summon.isPuppet())
        {
            map.addPlayerPuppet(this);
        }
    }

    public void addVisibleMapObject(MapObject mo)
    {
        visibleMapObjects.TryAdd(mo, 0);
    }

    public void ban(string reason)
    {
        this.isbanned = true;
        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == this.id).ExecuteUpdate(x => x.SetProperty(y => y.Banned, 1).SetProperty(y => y.Banreason, reason));
    }

    public static bool ban(string id, string reason, bool accountId)
    {
        using var dbContext = new DBContext();

        if (Regex.IsMatch(id, "/[0-9]{1,3}\\..*"))
        {
            dbContext.Ipbans.Add(new Ipban { Ip = id });
            dbContext.SaveChanges();
            return true;
        }
        int actualAccId = 0;
        if (accountId)
        {
            actualAccId = dbContext.Accounts.Where(x => x.Name == id).Select(x => x.Id).FirstOrDefault();

        }
        else
        {
            actualAccId = dbContext.Characters.Where(x => x.Name == id).Select(x => x.AccountId).FirstOrDefault();
        }
        dbContext.Accounts.Where(x => x.Id == actualAccId).ExecuteUpdate(x => x.SetProperty(y => y.Banned, 1).SetProperty(y => y.Banreason, reason));

        return true;
    }

    public int calculateMaxBaseDamage(int watk, WeaponType weapon)
    {
        int mainstat, secondarystat;
        if (getJob().isA(Job.THIEF) && weapon == WeaponType.DAGGER_OTHER)
        {
            weapon = WeaponType.DAGGER_THIEVES;
        }

        if (weapon == WeaponType.BOW || weapon == WeaponType.CROSSBOW || weapon == WeaponType.GUN)
        {
            mainstat = localdex;
            secondarystat = localstr;
        }
        else if (weapon == WeaponType.CLAW || weapon == WeaponType.DAGGER_THIEVES)
        {
            mainstat = localluk;
            secondarystat = localdex + localstr;
        }
        else
        {
            mainstat = localstr;
            secondarystat = localdex;
        }
        return (int)Math.Ceiling(((weapon.getMaxDamageMultiplier() * mainstat + secondarystat) / 100.0) * watk);
    }

    public int calculateMaxBaseDamage(int watk)
    {
        int maxbasedamage;
        Item? weapon_item = getInventory(InventoryType.EQUIPPED).getItem(-11);
        if (weapon_item != null)
        {
            maxbasedamage = calculateMaxBaseDamage(watk, ItemInformationProvider.getInstance().getWeaponType(weapon_item.getItemId()));
        }
        else
        {
            if (job.isA(Job.PIRATE) || job.isA(Job.THUNDERBREAKER1))
            {
                double weapMulti = 3;
                if (job.getId() % 100 != 0)
                {
                    weapMulti = 4.2;
                }

                int attack = (int)Math.Min(Math.Floor((double)((2 * getLevel() + 31) / 3)), 31);
                maxbasedamage = (int)Math.Ceiling((localstr * weapMulti + localdex) * attack / 100.0);
            }
            else
            {
                maxbasedamage = 1;
            }
        }
        return maxbasedamage;
    }

    public int calculateMaxBaseMagicDamage(int matk)
    {
        int maxbasedamage = matk;
        int totalint = getTotalInt();

        if (totalint > 2000)
        {
            maxbasedamage -= 2000;
            maxbasedamage += (int)((0.09033024267 * totalint) + 3823.8038);
        }
        else
        {
            maxbasedamage -= totalint;

            if (totalint > 1700)
            {
                maxbasedamage += (int)(0.1996049769 * Math.Pow(totalint, 1.300631341));
            }
            else
            {
                maxbasedamage += (int)(0.1996049769 * Math.Pow(totalint, 1.290631341));
            }
        }

        return (maxbasedamage * 107) / 100;
    }

    public void setCombo(short count)
    {
        if (count < combocounter)
        {
            cancelEffectFromBuffStat(BuffStat.ARAN_COMBO);
        }
        combocounter = Math.Min((short)30000, count);
        if (count > 0)
        {
            sendPacket(PacketCreator.showCombo(combocounter));
        }
    }

    public void setLastCombo(long time)
    {
        lastcombo = time;
    }

    public short getCombo()
    {
        return combocounter;
    }

    public long getLastCombo()
    {
        return lastcombo;
    }

    public int getLastMobCount()
    { //Used for skills that have mobCount at 1. (a/b)
        return lastmobcount;
    }

    public void setLastMobCount(byte count)
    {
        lastmobcount = count;
    }

    public bool cannotEnterCashShop()
    {
        return blockCashShop;
    }

    public void toggleBlockCashShop()
    {
        blockCashShop = !blockCashShop;
    }

    public void toggleExpGain()
    {
        allowExpGain = !allowExpGain;
    }

    public void setClient(Client c)
    {
        this.client = c;
    }

    public void newClient(Client c)
    {
        this.loggedIn = true;
        c.setAccountName(this.client.getAccountName());//No null's for accountName
        this.setClient(c);
        this.map = c.getChannelServer().getMapFactory().getMap(getMapId());
        Portal portal = map.findClosestPlayerSpawnpoint(getPosition());
        if (portal == null)
        {
            portal = map.getPortal(0);
        }
        this.setPosition(portal.getPosition());
        this.initialSpawnPoint = portal.getId();
    }

    public string getMedalText()
    {
        string medal = "";
        Item? medalItem = getInventory(InventoryType.EQUIPPED).getItem(-49);
        if (medalItem != null)
        {
            medal = "<" + ItemInformationProvider.getInstance().getName(medalItem.getItemId()) + "> ";
        }
        return medal;
    }

    public void Hide(bool hide, bool login)
    {
        if (isGM() && hide != this.hidden)
        {
            if (!hide)
            {
                this.hidden = false;
                sendPacket(PacketCreator.getGMEffect(0x10, 0));
                List<BuffStat> dsstat = Collections.singletonList(BuffStat.DARKSIGHT);
                getMap().broadcastGMMessage(this, PacketCreator.cancelForeignBuff(id, dsstat), false);
                getMap().broadcastSpawnPlayerMapObjectMessage(this, this, false);

                foreach (Summon ms in this.getSummonsValues())
                {
                    getMap().broadcastNONGMMessage(this, PacketCreator.spawnSummon(ms, false), false);
                }

                foreach (MapObject mo in this.getMap().getMonsters())
                {
                    Monster m = (Monster)mo;
                    m.aggroUpdateController();
                }
            }
            else
            {
                this.hidden = true;
                sendPacket(PacketCreator.getGMEffect(0x10, 1));
                if (!login)
                {
                    getMap().broadcastNONGMMessage(this, PacketCreator.removePlayerFromMap(getId()), false);
                }
                List<KeyValuePair<BuffStat, int>> ldsstat = Collections.singletonList(new KeyValuePair<BuffStat, int>(BuffStat.DARKSIGHT, 0));
                getMap().broadcastGMMessage(this, PacketCreator.giveForeignBuff(id, ldsstat), false);
                this.releaseControlledMonsters();
            }
            sendPacket(PacketCreator.enableActions());
        }
    }

    public void Hide(bool hide)
    {
        Hide(hide, false);
    }

    public void toggleHide(bool login)
    {
        Hide(!hidden);
    }

    public void cancelMagicDoor()
    {
        List<BuffStatValueHolder> mbsvhList = getAllStatups();
        foreach (BuffStatValueHolder mbsvh in mbsvhList)
        {
            if (mbsvh.effect.isMagicDoor())
            {
                cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                break;
            }
        }
    }

    private void cancelPlayerBuffs(List<BuffStat> buffstats)
    {
        if (client.getChannelServer().getPlayerStorage().getCharacterById(getId()) != null)
        {
            updateLocalStats();
            sendPacket(PacketCreator.cancelBuff(buffstats));
            if (buffstats.Count > 0)
            {
                getMap().broadcastMessage(this, PacketCreator.cancelForeignBuff(getId(), buffstats), false);
            }
        }
    }

    public bool canDoor()
    {
        var door = getPlayerDoor();
        return door == null || (door.isActive() && door.getElapsedDeployTime() > 5000);
    }

    public void setHasSandboxItem()
    {
        hasSandboxItem = true;
    }

    public void removeSandboxItems()
    {  // sandbox idea thanks to Morty
        if (!hasSandboxItem)
        {
            return;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (InventoryType invType in Enum.GetValues<InventoryType>())
        {
            Inventory inv = this.getInventory(invType);

            inv.lockInventory();
            try
            {
                foreach (Item item in inv.list())
                {
                    if (InventoryManipulator.isSandboxItem(item))
                    {
                        InventoryManipulator.removeFromSlot(client, invType, item.getPosition(), item.getQuantity(), false);
                        dropMessage(5, "[" + ii.getName(item.getItemId()) + "] has passed its trial conditions and will be removed from your inventory.");
                    }
                }
            }
            finally
            {
                inv.unlockInventory();
            }
        }

        hasSandboxItem = false;
    }

    public FameStatus canGiveFame(Character from)
    {
        if (this.isGM())
        {
            return FameStatus.OK;
        }
        else if (lastfametime >= DateTimeOffset.Now.ToUnixTimeMilliseconds() - 3600000 * 24)
        {
            return FameStatus.NOT_TODAY;
        }
        else if (lastmonthfameids.Contains(from.getId()))
        {
            return FameStatus.NOT_THIS_MONTH;
        }
        else
        {
            return FameStatus.OK;
        }
    }

    public void changeCI(int type)
    {
        this.ci = type;
    }

    public void setMasteries(int jobId)
    {
        int[] skills = new int[4];
        for (int i = 0; i > skills.Length; i++)
        {
            skills[i] = 0; //that initialization meng
        }
        if (jobId == 112)
        {
            skills[0] = Hero.ACHILLES;
            skills[1] = Hero.MONSTER_MAGNET;
            skills[2] = Hero.BRANDISH;
        }
        else if (jobId == 122)
        {
            skills[0] = Paladin.ACHILLES;
            skills[1] = Paladin.MONSTER_MAGNET;
            skills[2] = Paladin.BLAST;
        }
        else if (jobId == 132)
        {
            skills[0] = DarkKnight.BEHOLDER;
            skills[1] = DarkKnight.ACHILLES;
            skills[2] = DarkKnight.MONSTER_MAGNET;
        }
        else if (jobId == 212)
        {
            skills[0] = FPArchMage.BIG_BANG;
            skills[1] = FPArchMage.MANA_REFLECTION;
            skills[2] = FPArchMage.PARALYZE;
        }
        else if (jobId == 222)
        {
            skills[0] = ILArchMage.BIG_BANG;
            skills[1] = ILArchMage.MANA_REFLECTION;
            skills[2] = ILArchMage.CHAIN_LIGHTNING;
        }
        else if (jobId == 232)
        {
            skills[0] = Bishop.BIG_BANG;
            skills[1] = Bishop.MANA_REFLECTION;
            skills[2] = Bishop.HOLY_SHIELD;
        }
        else if (jobId == 312)
        {
            skills[0] = Bowmaster.BOW_EXPERT;
            skills[1] = Bowmaster.HAMSTRING;
            skills[2] = Bowmaster.SHARP_EYES;
        }
        else if (jobId == 322)
        {
            skills[0] = Marksman.MARKSMAN_BOOST;
            skills[1] = Marksman.BLIND;
            skills[2] = Marksman.SHARP_EYES;
        }
        else if (jobId == 412)
        {
            skills[0] = NightLord.SHADOW_STARS;
            skills[1] = NightLord.SHADOW_SHIFTER;
            skills[2] = NightLord.VENOMOUS_STAR;
        }
        else if (jobId == 422)
        {
            skills[0] = Shadower.SHADOW_SHIFTER;
            skills[1] = Shadower.VENOMOUS_STAB;
            skills[2] = Shadower.BOOMERANG_STEP;
        }
        else if (jobId == 512)
        {
            skills[0] = Buccaneer.BARRAGE;
            skills[1] = Buccaneer.ENERGY_ORB;
            skills[2] = Buccaneer.SPEED_INFUSION;
            skills[3] = Buccaneer.DRAGON_STRIKE;
        }
        else if (jobId == 522)
        {
            skills[0] = Corsair.ELEMENTAL_BOOST;
            skills[1] = Corsair.BULLSEYE;
            skills[2] = Corsair.WRATH_OF_THE_OCTOPI;
            skills[3] = Corsair.RAPID_FIRE;
        }
        else if (jobId == 2112)
        {
            skills[0] = Aran.OVER_SWING;
            skills[1] = Aran.HIGH_MASTERY;
            skills[2] = Aran.FREEZE_STANDING;
        }
        else if (jobId == 2217)
        {
            skills[0] = Evan.MAPLE_WARRIOR;
            skills[1] = Evan.ILLUSION;
        }
        else if (jobId == 2218)
        {
            skills[0] = Evan.BLESSING_OF_THE_ONYX;
            skills[1] = Evan.BLAZE;
        }
        foreach (int skillId in skills)
        {
            if (skillId != 0)
            {
                Skill skill = SkillFactory.getSkill(skillId);
                int skilllevel = getSkillLevel(skill);
                if (skilllevel > 0)
                {
                    continue;
                }

                changeSkillLevel(skill, 0, 10, -1);
            }
        }
    }

    private void broadcastChangeJob()
    {
        foreach (Character chr in map.getAllPlayers())
        {
            Client chrC = chr.getClient();

            if (chrC != null)
            {     // propagate new job 3rd-person effects (FJ, Aran 1st strike, etc)
                this.sendDestroyData(chrC);
                this.sendSpawnData(chrC);
            }
        }

        TimerManager.getInstance().schedule(() =>
        {
            Character thisChr = this;
            MapleMap map = thisChr.getMap();

            if (map != null)
            {
                map.broadcastMessage(thisChr, PacketCreator.showForeignEffect(thisChr.getId(), 8), false);
            }
        }, 777);
    }

    object changeJobLock = new object();
    public void changeJob(Job? newJob)
    {
        lock (changeJobLock)
        {
            if (newJob == null)
            {
                return;//the fuck you doing idiot!
            }

            if (canRecvPartySearchInvite && getParty() == null)
            {
                this.updatePartySearchAvailability(false);
                this.job = newJob.Value;
                this.updatePartySearchAvailability(true);
            }
            else
            {
                this.job = newJob.Value;
            }

            int spGain = 1;
            if (GameConstants.hasSPTable(newJob.Value))
            {
                spGain += 2;
            }
            else
            {
                if (newJob.Value.getId() % 10 == 2)
                {
                    spGain += 2;
                }

                if (YamlConfig.config.server.USE_ENFORCE_JOB_SP_RANGE)
                {
                    spGain = getChangedJobSp(newJob.Value);
                }
            }

            if (spGain > 0)
            {
                gainSp(spGain, GameConstants.getSkillBook(newJob.Value.getId()), true);
            }

            // thanks xinyifly for finding out missing AP awards (AP Reset can be used as a compass)
            if (newJob.Value.getId() % 100 >= 1)
            {
                if (this.isCygnus())
                {
                    gainAp(7, true);
                }
                else
                {
                    if (YamlConfig.config.server.USE_STARTING_AP_4 || newJob.Value.getId() % 10 >= 1)
                    {
                        gainAp(5, true);
                    }
                }
            }
            else
            {    // thanks Periwinks for noticing an AP shortage from lower levels
                if (YamlConfig.config.server.USE_STARTING_AP_4 && newJob.Value.getId() % 1000 >= 1)
                {
                    gainAp(4, true);
                }
            }

            if (!isGM())
            {
                for (byte i = 1; i < 5; i++)
                {
                    gainSlots(i, 4, true);
                }
            }

            int addhp = 0, addmp = 0;
            int job_ = job.getId() % 1000; // lame temp "fix"
            if (job_ == 100)
            {                      // 1st warrior
                addhp += Randomizer.rand(200, 250);
            }
            else if (job_ == 200)
            {               // 1st mage
                addmp += Randomizer.rand(100, 150);
            }
            else if (job_ % 100 == 0)
            {           // 1st others
                addhp += Randomizer.rand(100, 150);
                addmp += Randomizer.rand(25, 50);
            }
            else if (job_ > 0 && job_ < 200)
            {    // 2nd~4th warrior
                addhp += Randomizer.rand(300, 350);
            }
            else if (job_ < 300)
            {                // 2nd~4th mage
                addmp += Randomizer.rand(450, 500);
            }
            else if (job_ > 0)
            {                  // 2nd~4th others
                addhp += Randomizer.rand(300, 350);
                addmp += Randomizer.rand(150, 200);
            }

            /*
            //aran perks?
            int newJobId = newJob.getId();
            if(newJobId == 2100) {          // become aran1
                addhp += 275;
                addmp += 15;
            } else if(newJobId == 2110) {   // become aran2
                addmp += 275;
            } else if(newJobId == 2111) {   // become aran3
                addhp += 275;
                addmp += 275;
            }
            */

            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                addMaxMPMaxHP(addhp, addmp, true);
                recalcLocalStats();

                List<KeyValuePair<Stat, int>> statup = new(7);
                statup.Add(new(Stat.HP, hp));
                statup.Add(new(Stat.MP, mp));
                statup.Add(new(Stat.MAXHP, clientmaxhp));
                statup.Add(new(Stat.MAXMP, clientmaxmp));
                statup.Add(new(Stat.AVAILABLEAP, remainingAp));
                statup.Add(new(Stat.AVAILABLESP, remainingSp[GameConstants.getSkillBook(job.getId())]));
                statup.Add(new(Stat.JOB, job.getId()));
                sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }

            setMPC(new PartyCharacter(this));
            silentPartyUpdate();

            if (dragon != null)
            {
                getMap().broadcastMessage(PacketCreator.removeDragon(dragon.getObjectId()));
                dragon = null;
            }

            if (this.guildid > 0)
            {
                getGuild().broadcast(PacketCreator.jobMessage(0, job.getId(), name), this.getId());
            }
            Family? family = getFamily();
            if (family != null)
            {
                family.broadcast(PacketCreator.jobMessage(1, job.getId(), name), this.getId());
            }
            setMasteries(this.job.getId());
            guildUpdate();

            broadcastChangeJob();

            if (GameConstants.hasSPTable(newJob.Value) && newJob.Value.getId() != 2001)
            {
                if (getBuffedValue(BuffStat.MONSTER_RIDING) != null)
                {
                    cancelBuffStats(BuffStat.MONSTER_RIDING);
                }
                createDragon();
            }

            if (YamlConfig.config.server.USE_ANNOUNCE_CHANGEJOB)
            {
                if (!this.isGM())
                {
                    broadcastAcquaintances(6, "[" + GameConstants.ordinal(GameConstants.getJobBranch(newJob.Value)) + " Job] " + name + " has just become a " + GameConstants.getJobName(this.job.getId()) + ".");    // thanks Vcoc for noticing job name appearing in uppercase here
                }
            }

        }

    }

    public void broadcastAcquaintances(int type, string message)
    {
        broadcastAcquaintances(PacketCreator.serverNotice(type, message));
    }

    public void broadcastAcquaintances(Packet packet)
    {
        buddylist.broadcast(packet, getWorldServer().getPlayerStorage());
        Family? family = getFamily();
        if (family != null)
        {
            family.broadcast(packet, id);
        }

        Guild? guild = getGuild();
        if (guild != null)
        {
            guild.broadcast(packet, id);
        }

        /*
        if(partnerid > 0) {
            partner.sendPacket(packet); not yet implemented
        }
        */
        sendPacket(packet);
    }

    public void changeKeybinding(int key, KeyBinding keybinding)
    {
        if (keybinding.getType() != 0)
        {
            keymap.AddOrUpdate(key, keybinding);
        }
        else
        {
            keymap.Remove(key);
        }
    }

    public void changeQuickslotKeybinding(byte[] aQuickslotKeyMapped)
    {
        this.m_pQuickslotKeyMapped = new QuickslotBinding(aQuickslotKeyMapped);
    }

    public void broadcastStance(int newStance)
    {
        setStance(newStance);
        broadcastStance();
    }

    public void broadcastStance()
    {
        map.broadcastMessage(this, PacketCreator.movePlayer(id, this.getIdleMovement(), AbstractAnimatedMapObject.IDLE_MOVEMENT_PACKET_LENGTH), false);
    }

    public MapleMap getWarpMap(int map)
    {
        MapleMap warpMap;
        var eim = getEventInstance();
        if (eim != null)
        {
            warpMap = eim.getMapInstance(map);
        }
        else if (this.getMonsterCarnival() != null && this.getMonsterCarnival()!.getEventMap().getId() == map)
        {
            warpMap = this.getMonsterCarnival().getEventMap();
        }
        else
        {
            warpMap = client.getChannelServer().getMapFactory().getMap(map);
        }
        return warpMap;
    }

    // for use ONLY inside OnUserEnter map scripts that requires a player to change map while still moving between maps.
    public void warpAhead(int map)
    {
        newWarpMap = map;
    }

    private void eventChangedMap(int map)
    {
        var eim = getEventInstance();
        if (eim != null)
        {
            eim.changedMap(this, map);
        }
    }

    private void eventAfterChangedMap(int map)
    {
        var eim = getEventInstance();
        if (eim != null)
        {
            eim.afterChangedMap(this, map);
        }
    }

    public void changeMapBanish(BanishInfo banishInfo)
    {
        if (banishInfo.msg != null)
        {
            dropMessage(5, banishInfo.msg);
        }

        MapleMap map_ = getWarpMap(mapid);
        Portal portal_ = map_.getPortal(banishInfo.portal);
        changeMap(map_, portal_ != null ? portal_ : map_.getRandomPlayerSpawnpoint());
    }

    public void changeMap(int map)
    {
        MapleMap warpMap;
        var eim = getEventInstance();

        if (eim != null)
        {
            warpMap = eim.getMapInstance(map);
        }
        else
        {
            warpMap = client.getChannelServer().getMapFactory().getMap(map);
        }

        changeMap(warpMap, warpMap.getRandomPlayerSpawnpoint());
    }

    public void changeMap(int map, int portal)
    {
        MapleMap warpMap;
        var eim = getEventInstance();

        if (eim != null)
        {
            warpMap = eim.getMapInstance(map);
        }
        else
        {
            warpMap = client.getChannelServer().getMapFactory().getMap(map);
        }

        changeMap(warpMap, warpMap.getPortal(portal));
    }

    public void changeMap(int map, string portal)
    {
        MapleMap warpMap;
        var eim = getEventInstance();

        if (eim != null)
        {
            warpMap = eim.getMapInstance(map);
        }
        else
        {
            warpMap = client.getChannelServer().getMapFactory().getMap(map);
        }

        changeMap(warpMap, warpMap.getPortal(portal));
    }

    public void changeMap(int map, Portal portal)
    {
        MapleMap warpMap;
        var eim = getEventInstance();

        if (eim != null)
        {
            warpMap = eim.getMapInstance(map);
        }
        else
        {
            warpMap = client.getChannelServer().getMapFactory().getMap(map);
        }

        changeMap(warpMap, portal);
    }

    public void changeMap(MapleMap to)
    {
        changeMap(to, 0);
    }

    public void changeMap(MapleMap to, int portal)
    {
        changeMap(to, to.getPortal(portal));
    }

    public void changeMap(MapleMap target, Portal? pto)
    {
        canWarpCounter++;

        eventChangedMap(target.getId());    // player can be dropped from an event here, hence the new warping target.
        MapleMap to = getWarpMap(target.getId());
        if (pto == null)
        {
            pto = to.getPortal(0)!;
        }
        changeMapInternal(to, pto.getPosition(), PacketCreator.getWarpToMap(to, pto.getId(), this));
        canWarpMap = false;

        canWarpCounter--;
        if (canWarpCounter == 0)
        {
            canWarpMap = true;
        }

        eventAfterChangedMap(this.getMapId());
    }

    public void changeMap(MapleMap target, Point pos)
    {
        canWarpCounter++;

        eventChangedMap(target.getId());
        MapleMap to = getWarpMap(target.getId());
        changeMapInternal(to, pos, PacketCreator.getWarpToMap(to, 0x80, pos, this));
        canWarpMap = false;

        canWarpCounter--;
        if (canWarpCounter == 0)
        {
            canWarpMap = true;
        }

        eventAfterChangedMap(this.getMapId());
    }

    public void forceChangeMap(MapleMap target, Portal? pto)
    {
        // will actually enter the map given as parameter, regardless of being an eventmap or whatnot

        canWarpCounter++;
        eventChangedMap(MapId.NONE);

        var mapEim = target.getEventInstance();
        if (mapEim != null)
        {
            var playerEim = this.getEventInstance();
            if (playerEim != null)
            {
                playerEim.exitPlayer(this);
                if (playerEim.getPlayerCount() == 0)
                {
                    playerEim.dispose();
                }
            }

            // thanks Thora for finding an issue with players not being actually warped into the target event map (rather sent to the event starting map)
            mapEim.registerPlayer(this, false);
        }

        MapleMap to = target; // warps directly to the target intead of the target's map id, this allows GMs to patrol players inside instances.
        if (pto == null)
        {
            pto = to.getPortal(0)!;
        }
        changeMapInternal(to, pto.getPosition(), PacketCreator.getWarpToMap(to, pto.getId(), this));
        canWarpMap = false;

        canWarpCounter--;
        if (canWarpCounter == 0)
        {
            canWarpMap = true;
        }

        eventAfterChangedMap(this.getMapId());
    }

    private bool buffMapProtection()
    {
        int thisMapid = mapid;
        int returnMapid = client.getChannelServer().getMapFactory().getMap(thisMapid).getReturnMapId();

        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            foreach (var mbs in effects)
            {
                if (mbs.Key == BuffStat.MAP_PROTECTION)
                {
                    byte value = (byte)mbs.Value.value;

                    if (value == 1 && ((returnMapid == MapId.EL_NATH && thisMapid != MapId.ORBIS_TOWER_BOTTOM) || returnMapid == MapId.INTERNET_CAFE))
                    {
                        return true;        //protection from cold
                    }
                    else
                    {
                        return value == 2 && (returnMapid == MapId.AQUARIUM || thisMapid == MapId.ORBIS_TOWER_BOTTOM);        //breathing underwater
                    }
                }
            }
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }

        foreach (Item it in this.getInventory(InventoryType.EQUIPPED).list())
        {
            if ((it.getFlag() & ItemConstants.COLD) == ItemConstants.COLD &&
                    ((returnMapid == MapId.EL_NATH && thisMapid != MapId.ORBIS_TOWER_BOTTOM) || returnMapid == MapId.INTERNET_CAFE))
            {
                return true;        //protection from cold
            }
        }

        return false;
    }

    public List<int> getLastVisitedMapids()
    {
        List<int> lastVisited = new(5);

        Monitor.Enter(petLock);
        try
        {
            foreach (WeakReference<MapleMap> lv in lastVisitedMaps)
            {
                if (lv.TryGetTarget(out var lvm))
                    lastVisited.Add(lvm.getId());
            }
        }
        finally
        {
            Monitor.Exit(petLock);
        }

        return lastVisited;
    }

    public void partyOperationUpdate(Party party, List<Character>? exPartyMembers)
    {
        List<WeakReference<MapleMap>> mapids;

        Monitor.Enter(petLock);
        try
        {
            mapids = new(lastVisitedMaps);
        }
        finally
        {
            Monitor.Exit(petLock);
        }

        List<Character> partyMembers = new();
        foreach (Character mc in (exPartyMembers != null) ? exPartyMembers : this.getPartyMembersOnline())
        {
            if (mc.isLoggedinWorld())
            {
                partyMembers.Add(mc);
            }
        }

        Character? partyLeaver = null;
        if (exPartyMembers != null)
        {
            partyMembers.Remove(this);
            partyLeaver = this;
        }

        MapleMap map = this.getMap();
        List<MapItem>? partyItems = null;

        int partyId = exPartyMembers != null ? -1 : this.getPartyId();
        foreach (WeakReference<MapleMap> mapRef in mapids)
        {
            if (mapRef.TryGetTarget(out var mapObj))
            {
                List<MapItem> partyMapItems = mapObj.updatePlayerItemDropsToParty(partyId, id, partyMembers, partyLeaver);
                if (map.GetHashCode() == mapObj.GetHashCode())
                {
                    partyItems = partyMapItems;
                }
            }
        }

        if (partyItems != null && exPartyMembers == null)
        {
            map.updatePartyItemDropsToNewcomer(this, partyItems);
        }

        updatePartyTownDoors(party, this, partyLeaver, partyMembers);
    }

    private static void addPartyPlayerDoor(Character target)
    {
        var targetDoor = target.getPlayerDoor();
        if (targetDoor != null)
        {
            target.applyPartyDoor(targetDoor, true);
        }
    }

    private static void removePartyPlayerDoor(Party party, Character target)
    {
        target.removePartyDoor(party);
    }

    private static void updatePartyTownDoors(Party party, Character target, Character? partyLeaver, List<Character> partyMembers)
    {
        if (partyLeaver != null)
        {
            removePartyPlayerDoor(party, target);
        }
        else
        {
            addPartyPlayerDoor(target);
        }

        Dictionary<int, Door>? partyDoors = null;
        if (partyMembers.Count > 0)
        {
            partyDoors = party.getDoors();

            foreach (Character pchr in partyMembers)
            {
                Door? door = partyDoors.GetValueOrDefault(pchr.getId());
                if (door != null)
                {
                    door.updateDoorPortal(pchr);
                }
            }

            foreach (Door door in partyDoors.Values)
            {
                foreach (Character pchar in partyMembers)
                {
                    DoorObject mdo = door.getTownDoor();
                    mdo.sendDestroyData(pchar.getClient(), true);
                    pchar.removeVisibleMapObject(mdo);
                }
            }

            if (partyLeaver != null)
            {
                var leaverDoors = partyLeaver.getDoors();
                foreach (Door door in leaverDoors)
                {
                    foreach (Character pchar in partyMembers)
                    {
                        DoorObject mdo = door.getTownDoor();
                        mdo.sendDestroyData(pchar.getClient(), true);
                        pchar.removeVisibleMapObject(mdo);
                    }
                }
            }

            List<int> histMembers = party.getMembersSortedByHistory();
            foreach (int chrid in histMembers)
            {
                Door? door = partyDoors.GetValueOrDefault(chrid);
                if (door != null)
                {
                    foreach (Character pchar in partyMembers)
                    {
                        DoorObject mdo = door.getTownDoor();
                        mdo.sendSpawnData(pchar.getClient());
                        pchar.addVisibleMapObject(mdo);
                    }
                }
            }
        }

        if (partyLeaver != null)
        {
            var leaverDoors = partyLeaver.getDoors();

            if (partyDoors != null)
            {
                foreach (Door door in partyDoors.Values)
                {
                    DoorObject mdo = door.getTownDoor();
                    mdo.sendDestroyData(partyLeaver.getClient(), true);
                    partyLeaver.removeVisibleMapObject(mdo);
                }
            }

            foreach (Door door in leaverDoors)
            {
                DoorObject mdo = door.getTownDoor();
                mdo.sendDestroyData(partyLeaver.getClient(), true);
                partyLeaver.removeVisibleMapObject(mdo);
            }

            foreach (Door door in leaverDoors)
            {
                door.updateDoorPortal(partyLeaver);

                DoorObject mdo = door.getTownDoor();
                mdo.sendSpawnData(partyLeaver.getClient());
                partyLeaver.addVisibleMapObject(mdo);
            }
        }
    }

    private int getVisitedMapIndex(MapleMap map)
    {
        int idx = 0;

        foreach (WeakReference<MapleMap> mapRef in lastVisitedMaps)
        {
            if (mapRef.TryGetTarget(out var d) && map == d)
                return idx;

            idx++;
        }

        return -1;
    }

    public void visitMap(MapleMap map)
    {
        Monitor.Enter(petLock);
        try
        {
            int idx = getVisitedMapIndex(map);

            if (idx == -1)
            {
                if (lastVisitedMaps.Count == YamlConfig.config.server.MAP_VISITED_SIZE)
                {
                    lastVisitedMaps.RemoveAt(0);
                }
            }
            else
            {
                WeakReference<MapleMap> mapRef = lastVisitedMaps.remove(idx);
                lastVisitedMaps.Add(mapRef);
                return;
            }

            lastVisitedMaps.Add(new(map));
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public void setOwnedMap(MapleMap? map)
    {
        ownedMap = new(map);
    }

    public MapleMap? getOwnedMap()
    {
        if (ownedMap.TryGetTarget(out var d))
        {
            return d;

        }
        return null;
    }

    public void notifyMapTransferToPartner(int mapid)
    {
        if (partnerId > 0)
        {
            var partner = getWorldServer().getPlayerStorage().getCharacterById(partnerId);
            if (partner != null && !partner.isAwayFromWorld())
            {
                partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(id, mapid));
            }
        }
    }

    public void removeIncomingInvites()
    {
        InviteCoordinator.removePlayerIncomingInvites(id);
    }

    private void changeMapInternal(MapleMap to, Point pos, Packet warpPacket)
    {
        if (!canWarpMap)
        {
            return;
        }

        this.mapTransitioning.Set(true);

        this.unregisterChairBuff();
        Trade.cancelTrade(this, TradeResult.UNSUCCESSFUL_ANOTHER_MAP);
        this.closePlayerInteractions();

        Party? k = this.getParty()?.getEnemy();

        sendPacket(warpPacket);
        map.removePlayer(this);
        if (client.getChannelServer().getPlayerStorage().getCharacterById(getId()) != null)
        {
            map = to;
            setPosition(pos);
            map.addPlayer(this);
            visitMap(map);

            Monitor.Enter(prtLock);
            try
            {
                if (party != null)
                {
                    mpc.setMapId(to.getId());
                    sendPacket(PacketCreator.updateParty(client.getChannel(), party, PartyOperation.SILENT_UPDATE, null));
                    updatePartyMemberHPInternal();
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
            if (this.getParty() != null)
            {
                this.getParty()!.setEnemy(k);
            }
            silentPartyUpdateInternal(getParty());  // EIM script calls inside
        }
        else
        {
            log.Warning("Chr {CharacterName} got stuck when moving to map {MapId}", getName(), map.getId());
            client.disconnect(true, false);     // thanks BHB for noticing a player storage stuck case here
            return;
        }

        notifyMapTransferToPartner(map.getId());

        //alas, new map has been specified when a warping was being processed...
        if (newWarpMap != -1)
        {
            canWarpMap = true;

            int temp = newWarpMap;
            newWarpMap = -1;
            changeMap(temp);
        }
        else
        {
            // if this event map has a gate already opened, render it
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.recoverOpenedGate(this, map.getId());
            }

            // if this map has obstacle components moving, make it do so for this client
            sendPacket(PacketCreator.environmentMoveList(map.getEnvironment()));
        }
    }

    public bool isChangingMaps()
    {
        return this.mapTransitioning.Get();
    }

    public void setMapTransitionComplete()
    {
        this.mapTransitioning.Set(false);
    }

    public void changePage(int page)
    {
        this.currentPage = page;
    }

    public void changeSkillLevel(Skill skill, sbyte newLevel, int newMasterlevel, long expiration)
    {
        if (newLevel > -1)
        {
            skills.AddOrUpdate(skill, new SkillEntry(newLevel, newMasterlevel, expiration));
            if (!GameConstants.isHiddenSkills(skill.getId()))
            {
                sendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, expiration));
            }
        }
        else
        {
            skills.Remove(skill);
            sendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, -1)); //Shouldn't use expiration anymore :)
            using DBContext dbContext = new DBContext();
            dbContext.Skills.Where(x => x.Skillid == skill.getId() && x.Characterid == id).ExecuteDelete();
        }
    }

    public void changeTab(int tab)
    {
        this.currentTab = tab;
    }

    public void changeType(int type)
    {
        this.currentType = type;
    }

    public void checkBerserk(bool isHidden)
    {
        if (berserkSchedule != null)
        {
            berserkSchedule.cancel(false);
        }
        Character chr = this;
        if (job.Equals(Job.DARKKNIGHT))
        {
            Skill BerserkX = SkillFactory.GetSkillTrust(DarkKnight.BERSERK);
            int skilllevel = getSkillLevel(BerserkX);
            if (skilllevel > 0)
            {
                berserk = chr.getHp() * 100 / chr.getCurrentMaxHp() < BerserkX.getEffect(skilllevel).getX();
                berserkSchedule = TimerManager.getInstance().register(() =>
                {
                    if (awayFromWorld.Get())
                    {
                        return;
                    }

                    sendPacket(PacketCreator.showOwnBerserk(skilllevel, berserk));
                    if (!isHidden)
                    {
                        getMap().broadcastMessage(this, PacketCreator.showBerserk(getId(), skilllevel, berserk), false);
                    }
                    else
                    {
                        getMap().broadcastGMMessage(this, PacketCreator.showBerserk(getId(), skilllevel, berserk), false);
                    }
                }
                , 5000, 3000);
            }
        }
    }

    public void checkMessenger()
    {
        if (messenger != null && messengerposition < 4 && messengerposition > -1)
        {
            World worldz = getWorldServer();
            worldz.silentJoinMessenger(messenger.getId(), new MessengerCharacter(this, messengerposition), messengerposition);
            worldz.updateMessenger(getMessenger().getId(), name, client.getChannel());
        }
    }

    public void controlMonster(Monster monster)
    {
        controlled.AddOrUpdate(monster, monster.getId());
    }

    public void stopControllingMonster(Monster monster)
    {
        controlled.Remove(monster);
    }

    public int getNumControlledMonsters()
    {
        return controlled.Count;
    }

    public ICollection<Monster> getControlledMonsters()
    {
        return controlled.Keys.ToList();
    }

    public void releaseControlledMonsters()
    {
        var controlledMonsters = new List<Monster>(controlled.Keys);
        controlled.Clear();

        foreach (Monster monster in controlledMonsters)
        {
            monster.aggroRedirectController();
        }
    }

    public bool applyConsumeOnPickup(int itemId)
    {
        if (itemId / 1000000 == 2)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            if (ii.isConsumeOnPickup(itemId))
            {
                if (ItemConstants.isPartyItem(itemId))
                {
                    List<Character> partyMembers = this.getPartyMembersOnSameMap();
                    if (!ItemId.isPartyAllCure(itemId))
                    {
                        var mse = ii.getItemEffect(itemId);
                        if (partyMembers.Count > 0)
                        {
                            foreach (Character mc in partyMembers)
                            {
                                if (mc.isAlive())
                                {
                                    mse.applyTo(mc);
                                }
                            }
                        }
                        else if (this.isAlive())
                        {
                            mse.applyTo(this);
                        }
                    }
                    else
                    {
                        if (partyMembers.Count > 0)
                        {
                            foreach (Character mc in partyMembers)
                            {
                                mc.dispelDebuffs();
                            }
                        }
                        else
                        {
                            this.dispelDebuffs();
                        }
                    }
                }
                else
                {
                    ii.getItemEffect(itemId).applyTo(this);
                }

                if (itemId / 10000 == 238)
                {
                    this.getMonsterBook().addCard(client, itemId);
                }
                return true;
            }
        }
        return false;
    }

    public void pickupItem(MapObject ob)
    {
        pickupItem(ob, -1);
    }

    public void pickupItem(MapObject ob, int petIndex)
    {
        // yes, one picks the MapObject, not the MapItem
        if (ob == null)
        {                                               // pet index refers to the one picking up the item
            return;
        }

        if (ob is MapItem mapitem)
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - mapitem.getDropTime() < 400 || !mapitem.canBePickedBy(this))
            {
                sendPacket(PacketCreator.enableActions());
                return;
            }

            List<Character> mpcs = new();
            if (mapitem.getMeso() > 0 && !mapitem.isPickedUp())
            {
                mpcs = getPartyMembersOnSameMap();
            }

            ScriptedItem? itemScript = null;
            mapitem.lockItem();
            try
            {
                if (mapitem.isPickedUp())
                {
                    sendPacket(PacketCreator.showItemUnavailable());
                    sendPacket(PacketCreator.enableActions());
                    return;
                }

                bool isPet = petIndex > -1;
                Packet pickupPacket = PacketCreator.removeItemFromMap(mapitem.getObjectId(), (isPet) ? 5 : 2, this.getId(), isPet, petIndex);

                Item mItem = mapitem.getItem();
                bool hasSpaceInventory = true;
                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                if (ItemId.isNxCard(mapitem.getItemId()) || mapitem.getMeso() > 0 || ii.isConsumeOnPickup(mapitem.getItemId()) || (hasSpaceInventory = InventoryManipulator.checkSpace(client, mapitem.getItemId(), mItem.getQuantity(), mItem.getOwner())))
                {
                    int mapId = this.getMapId();

                    if ((MapId.isSelfLootableOnly(mapId)))
                    {//happyville trees and guild PQ
                        if (!mapitem.isPlayerDrop() || mapitem.getDropper().getObjectId() == client.getPlayer().getObjectId())
                        {
                            if (mapitem.getMeso() > 0)
                            {
                                if (mpcs.Count > 0)
                                {
                                    int mesosamm = mapitem.getMeso() / mpcs.Count;
                                    foreach (Character partymem in mpcs)
                                    {
                                        if (partymem.isLoggedinWorld())
                                        {
                                            partymem.gainMeso(mesosamm, true, true, false);
                                        }
                                    }
                                }
                                else
                                {
                                    this.gainMeso(mapitem.getMeso(), true, true, false);
                                }

                                this.getMap().pickItemDrop(pickupPacket, mapitem);
                            }
                            else if (ItemId.isNxCard(mapitem.getItemId()))
                            {
                                // Add NX to account, show effect and make item disappear
                                int nxGain = mapitem.getItemId() == ItemId.NX_CARD_100 ? 100 : 250;
                                this.getCashShop().gainCash(1, nxGain);

                                if (YamlConfig.config.server.USE_ANNOUNCE_NX_COUPON_LOOT)
                                {
                                    showHint("You have earned #e#b" + nxGain + " NX#k#n. (" + this.getCashShop().getCash(CashShop.NX_CREDIT) + " NX)", 300);
                                }

                                this.getMap().pickItemDrop(pickupPacket, mapitem);
                            }
                            else if (InventoryManipulator.addFromDrop(client, mItem, true))
                            {
                                this.getMap().pickItemDrop(pickupPacket, mapitem);
                            }
                            else
                            {
                                sendPacket(PacketCreator.enableActions());
                                return;
                            }
                        }
                        else
                        {
                            sendPacket(PacketCreator.showItemUnavailable());
                            sendPacket(PacketCreator.enableActions());
                            return;
                        }
                        sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    if (!this.needQuestItem(mapitem.getQuest(), mapitem.getItemId()))
                    {
                        sendPacket(PacketCreator.showItemUnavailable());
                        sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    if (mapitem.getMeso() > 0)
                    {
                        if (mpcs.Count > 0)
                        {
                            int mesosamm = mapitem.getMeso() / mpcs.Count;
                            foreach (Character partymem in mpcs)
                            {
                                if (partymem.isLoggedinWorld())
                                {
                                    partymem.gainMeso(mesosamm, true, true, false);
                                }
                            }
                        }
                        else
                        {
                            this.gainMeso(mapitem.getMeso(), true, true, false);
                        }
                    }
                    else if (mItem.getItemId() / 10000 == 243)
                    {
                        var info = ii.getScriptedItemInfo(mItem.getItemId());
                        if (info != null && info.runOnPickup())
                        {
                            itemScript = info;
                        }
                        else
                        {
                            if (!InventoryManipulator.addFromDrop(client, mItem, true))
                            {
                                sendPacket(PacketCreator.enableActions());
                                return;
                            }
                        }
                    }
                    else if (ItemId.isNxCard(mapitem.getItemId()))
                    {
                        // Add NX to account, show effect and make item disappear
                        int nxGain = mapitem.getItemId() == ItemId.NX_CARD_100 ? 100 : 250;
                        this.getCashShop().gainCash(1, nxGain);

                        if (YamlConfig.config.server.USE_ANNOUNCE_NX_COUPON_LOOT)
                        {
                            showHint("You have earned #e#b" + nxGain + " NX#k#n. (" + this.getCashShop().getCash(CashShop.NX_CREDIT) + " NX)", 300);
                        }
                    }
                    else if (applyConsumeOnPickup(mItem.getItemId()))
                    {
                    }
                    else if (InventoryManipulator.addFromDrop(client, mItem, true))
                    {
                        if (mItem.getItemId() == ItemId.ARPQ_SPIRIT_JEWEL)
                        {
                            updateAriantScore();
                        }
                    }
                    else
                    {
                        sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    this.getMap().pickItemDrop(pickupPacket, mapitem);
                }
                else if (!hasSpaceInventory)
                {
                    sendPacket(PacketCreator.getInventoryFull());
                    sendPacket(PacketCreator.getShowInventoryFull());
                }
            }
            finally
            {
                mapitem.unlockItem();
            }

            if (itemScript != null)
            {
                ItemScriptManager ism = ItemScriptManager.getInstance();
                ism.runItemScript(client, itemScript);
            }
        }
        sendPacket(PacketCreator.enableActions());
    }

    public int countItem(int itemid)
    {
        return inventory[ItemConstants.getInventoryType(itemid).ordinal()].countById(itemid);
    }

    public bool canHold(int itemid)
    {
        return canHold(itemid, 1);
    }

    public bool canHold(int itemid, int quantity)
    {
        return client.getAbstractPlayerInteraction().canHold(itemid, quantity);
    }

    public bool canHoldUniques(List<int> itemids)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (int itemid in itemids)
        {
            if (ii.isPickupRestricted(itemid) && this.haveItem(itemid))
            {
                return false;
            }
        }

        return true;
    }

    public bool isRidingBattleship()
    {
        return getBuffedValue(BuffStat.MONSTER_RIDING) == Corsair.BATTLE_SHIP;
    }

    public void announceBattleshipHp()
    {
        sendPacket(PacketCreator.skillCooldown(5221999, battleshipHp));
    }

    public void decreaseBattleshipHp(int decrease)
    {
        this.battleshipHp -= decrease;
        if (battleshipHp <= 0)
        {
            Skill battleship = SkillFactory.GetSkillTrust(Corsair.BATTLE_SHIP);
            int cooldown = battleship.getEffect(getSkillLevel(battleship)).getCooldown();
            sendPacket(PacketCreator.skillCooldown(Corsair.BATTLE_SHIP, cooldown));
            addCooldown(Corsair.BATTLE_SHIP, Server.getInstance().getCurrentTime(), cooldown * 1000);
            removeCooldown(5221999);
            cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);
        }
        else
        {
            announceBattleshipHp();
            addCooldown(5221999, 0, long.MaxValue);
        }
    }

    public void decreaseReports()
    {
        this.possibleReports--;
    }

    public void deleteGuild(int guildId)
    {
        using var dbContext = new DBContext();
        using var dbTrans = dbContext.Database.BeginTransaction();
        dbContext.Characters.Where(x => x.GuildId == guildId).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, 0).SetProperty(y => y.GuildRank, 5));
        dbContext.Guilds.Where(x => x.GuildId == guildId).ExecuteDelete();
        dbTrans.Commit();
    }

    private void nextPendingRequest(Client c)
    {
        CharacterNameAndId? pendingBuddyRequest = c.getPlayer().getBuddylist().pollPendingRequest();
        if (pendingBuddyRequest != null)
        {
            c.sendPacket(PacketCreator.requestBuddylistAdd(pendingBuddyRequest.id, c.getPlayer().getId(), pendingBuddyRequest.name));
        }
    }

    private void notifyRemoteChannel(Client c, int remoteChannel, int otherCid, BuddyList.BuddyOperation operation)
    {
        Character player = c.getPlayer();
        if (remoteChannel != -1)
        {
            c.getWorldServer().buddyChanged(otherCid, player.getId(), player.getName(), c.getChannel(), operation);
        }
    }

    public void deleteBuddy(int otherCid)
    {
        BuddyList bl = getBuddylist();

        if (bl.containsVisible(otherCid))
        {
            notifyRemoteChannel(client, getWorldServer().find(otherCid), otherCid, BuddyList.BuddyOperation.DELETED);
        }
        bl.remove(otherCid);
        sendPacket(PacketCreator.updateBuddylist(getBuddylist().getBuddies()));
        nextPendingRequest(client);
    }

    public static bool deleteCharFromDB(Character player, int senderAccId)
    {
        int cid = player.getId();
        if (!Server.getInstance().haveCharacterEntry(senderAccId, cid))
        {    // thanks zera (EpiphanyMS) for pointing a critical exploit with non-authed character deletion request
            return false;
        }

        int accId = senderAccId;
        int world = 0;
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            world = dbContext.Characters.Where(x => x.Id == cid).Select(x => x.World).FirstOrDefault();

            var dbBuddyIdList = dbContext.Buddies.Where(x => x.CharacterId == cid).Select(x => x.BuddyId).ToList();
            dbBuddyIdList.ForEach(buddyid =>
            {
                var buddy = Server.getInstance().getWorld(world)?.getPlayerStorage()?.getCharacterById(buddyid);

                if (buddy != null)
                {
                    buddy.deleteBuddy(cid);
                }
            });

            dbContext.Buddies.Where(x => x.CharacterId == cid).ExecuteDelete();

            var threadIdList = dbContext.BbsThreads.Where(x => x.Postercid == cid).Select(x => x.Threadid).ToList();
            dbContext.BbsReplies.Where(x => threadIdList.Contains(x.Threadid)).ExecuteDelete();
            dbContext.BbsThreads.Where(x => x.Postercid == cid).ExecuteDelete();


            var rs = dbContext.Characters.FirstOrDefault(x => x.Id == cid && x.AccountId == accId);
            if (rs == null)
                throw new BusinessDataNullException();

            Server.getInstance().deleteGuildCharacter(player);

            dbContext.Wishlists.Where(x => x.CharId == cid).ExecuteDelete();
            dbContext.Cooldowns.Where(x => x.Charid == cid).ExecuteDelete();
            dbContext.Playerdiseases.Where(x => x.Charid == cid).ExecuteDelete();
            dbContext.AreaInfos.Where(x => x.Charid == cid).ExecuteDelete();
            dbContext.Monsterbooks.Where(x => x.Charid == cid).ExecuteDelete();
            dbContext.Characters.Where(x => x.Id == cid).ExecuteDelete();
            dbContext.FamilyCharacters.Where(x => x.Cid == cid).ExecuteDelete();
            dbContext.Famelogs.Where(x => x.CharacteridTo == cid).ExecuteDelete();

            var inventoryItems = dbContext.Inventoryitems.Where(x => x.Characterid == cid).ToList();
            var inventoryItemIdList = inventoryItems.Select(x => x.Inventoryitemid).ToList();
            var inventoryEquipList = dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).ToList();
            inventoryItems.ForEach(rs =>
            {
                var ringsList = inventoryEquipList.Where(x => x.Inventoryitemid == rs.Inventoryitemid).Select(x => x.RingId).ToList();
                ringsList.ForEach(ringid =>
                {
                    if (ringid > -1)
                    {
                        dbContext.Rings.Where(x => x.Id == ringid).ExecuteDelete();
                        CashIdGenerator.freeCashId(ringid);
                    }
                });

                dbContext.Pets.Where(x => x.Petid == rs.Petid).ExecuteDelete();
                CashIdGenerator.freeCashId(rs.Petid);
            });
            dbContext.Inventoryitems.RemoveRange(inventoryItems);
            dbContext.Inventoryequipments.RemoveRange(inventoryEquipList);

            deleteQuestProgressWhereCharacterId(dbContext, cid);
            FredrickProcessor.removeFredrickLog(cid);   // thanks maple006 for pointing out the player's Fredrick items are not being deleted at character deletion

            var mtsCartIdList = dbContext.MtsCarts.Where(x => x.Cid == cid).Select(x => x.Id).ToList();
            dbContext.MtsItems.Where(x => mtsCartIdList.Contains(x.Id)).ExecuteDelete();
            dbContext.MtsCarts.Where(x => x.Cid == cid).ExecuteDelete();


            string[] toDel = { "famelog", "inventoryitems", "keymap", "queststatus", "savedlocations", "trocklocations", "skillmacros", "skills", "eventstats", "server_queue" };
            foreach (string s in toDel)
            {
                Character.deleteWhereCharacterId(dbContext, "DELETE FROM `" + s + "` WHERE characterid = ?", cid);
            }
            dbContext.SaveChanges();
            dbTrans.Commit();
            Server.getInstance().deleteCharacterEntry(accId, cid);
            return true;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return false;
        }
    }

    private static void deleteQuestProgressWhereCharacterId(DBContext dbContext, int cid)
    {
        dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();

        dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();

        dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();
    }

    private void deleteWhereCharacterId(DBContext dbContext, string sql)
    {
        dbContext.Database.ExecuteSqlRaw(sql, this.id);
    }

    public static void deleteWhereCharacterId(DBContext dbContext, string sql, int cid)
    {
        dbContext.Database.ExecuteSqlRaw(sql, cid);
    }

    private void stopChairTask()
    {
        chLock.EnterReadLock();
        try
        {
            if (chairRecoveryTask != null)
            {
                chairRecoveryTask.cancel(false);
                chairRecoveryTask = null;
            }
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    private static ChairHealStats getChairTaskIntervalRate(int maxhp, int maxmp)
    {
        float toHeal = Math.Max(maxhp, maxmp);
        float maxDuration = (float)TimeSpan.FromSeconds(YamlConfig.config.server.CHAIR_EXTRA_HEAL_MAX_DELAY).TotalMilliseconds;

        int rate = 0;
        int minRegen = 1, maxRegen = (256 * YamlConfig.config.server.CHAIR_EXTRA_HEAL_MULTIPLIER) - 1, midRegen = 1;
        while (minRegen < maxRegen)
        {
            midRegen = (int)((minRegen + maxRegen) * 0.94);

            float procsTemp = toHeal / midRegen;
            float newRate = maxDuration / procsTemp;
            rate = (int)newRate;

            if (newRate < 420)
            {
                minRegen = (int)(1.2 * midRegen);
            }
            else if (newRate > 5000)
            {
                maxRegen = (int)(0.8 * midRegen);
            }
            else
            {
                break;
            }
        }

        float procs = maxDuration / rate;
        int hpRegen, mpRegen;
        if (maxhp > maxmp)
        {
            hpRegen = midRegen;
            mpRegen = (int)Math.Ceiling(maxmp / procs);
        }
        else
        {
            hpRegen = (int)Math.Ceiling(maxhp / procs);
            mpRegen = midRegen;
        }

        return new(rate, hpRegen, mpRegen);
    }

    private void updateChairHealStats()
    {
        statLock.EnterReadLock();
        try
        {
            if (localchairrate != -1)
            {
                return;
            }
        }
        finally
        {
            statLock.ExitReadLock();
        }

        Monitor.Enter(effLock);
        statLock.EnterWriteLock();
        try
        {
            var p = getChairTaskIntervalRate(localmaxhp, localmaxmp);

            localchairrate = p.Rate;
            localchairhp = p.Hp;
            localchairmp = p.Mp;
        }
        finally
        {
            statLock.ExitWriteLock();
            Monitor.Exit(effLock);
        }
    }

    private void startChairTask()
    {
        if (chair.get() < 0)
        {
            return;
        }

        int healInterval;
        Monitor.Enter(effLock);
        try
        {
            updateChairHealStats();
            healInterval = localchairrate;
        }
        finally
        {
            Monitor.Exit(effLock);
        }

        chLock.EnterReadLock();
        try
        {
            if (chairRecoveryTask != null)
            {
                stopChairTask();
            }

            chairRecoveryTask = TimerManager.getInstance().register(() =>
            {
                updateChairHealStats();
                int healHP = localchairhp;
                int healMP = localchairmp;

                if (this.getHp() < localmaxhp)
                {
                    byte recHP = (byte)(healHP / YamlConfig.config.server.CHAIR_EXTRA_HEAL_MULTIPLIER);

                    sendPacket(PacketCreator.showOwnRecovery(recHP));
                    getMap().broadcastMessage(this, PacketCreator.showRecovery(id, recHP), false);
                }
                else if (this.getMp() >= localmaxmp)
                {
                    stopChairTask();    // optimizing schedule management when player is already with full pool.
                }

                addMPHP(healHP, healMP);

            }, healInterval, healInterval);
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    private void stopExtraTask()
    {
        chLock.EnterReadLock();
        try
        {
            if (extraRecoveryTask != null)
            {
                extraRecoveryTask.cancel(false);
                extraRecoveryTask = null;
            }
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    private void startExtraTask(byte healHP, byte healMP, short healInterval)
    {
        chLock.EnterReadLock();
        try
        {
            startExtraTaskInternal(healHP, healMP, healInterval);
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    private void startExtraTaskInternal(byte healHP, byte healMP, short healInterval)
    {
        extraRecInterval = healInterval;

        extraRecoveryTask = TimerManager.getInstance().register(() =>
        {
            if (getBuffSource(BuffStat.HPREC) == -1 && getBuffSource(BuffStat.MPREC) == -1)
            {
                stopExtraTask();
                return;
            }

            if (this.getHp() < localmaxhp)
            {
                if (healHP > 0)
                {
                    sendPacket(PacketCreator.showOwnRecovery(healHP));
                    getMap().broadcastMessage(this, PacketCreator.showRecovery(id, healHP), false);
                }
            }

            addMPHP(healHP, healMP);

        }, healInterval, healInterval);
    }

    public void disbandGuild()
    {
        if (guildid < 1 || guildRank != 1)
        {
            return;
        }
        try
        {
            Server.getInstance().disbandGuild(guildid);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public void dispel()
    {
        if (!(YamlConfig.config.server.USE_UNDISPEL_HOLY_SHIELD && this.hasActiveBuff(Bishop.HOLY_SHIELD)))
        {
            List<BuffStatValueHolder> mbsvhList = getAllStatups();
            foreach (BuffStatValueHolder mbsvh in mbsvhList)
            {
                if (mbsvh.effect.isSkill())
                {
                    if (mbsvh.effect.getBuffSourceId() != Aran.COMBO_ABILITY)
                    { // check discovered thanks to Croosade dev team
                        cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                    }
                }
            }
        }
    }

    public bool hasDisease(Disease dis)
    {
        chLock.EnterReadLock();
        try
        {
            return diseases.ContainsKey(dis);
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public int getDiseasesSize()
    {
        chLock.EnterReadLock();
        try
        {
            return diseases.Count;
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public Dictionary<Disease, DiseaseExpiration> getAllDiseases()
    {
        chLock.EnterReadLock();
        try
        {
            long curtime = Server.getInstance().getCurrentTime();
            Dictionary<Disease, DiseaseExpiration> ret = new();

            foreach (var de in diseaseExpires)
            {
                KeyValuePair<DiseaseValueHolder, MobSkill> dee = diseases.GetValueOrDefault(de.Key);
                DiseaseValueHolder mdvh = dee.Key;

                ret.AddOrUpdate(de.Key, new(mdvh.length - (curtime - mdvh.startTime), dee.Value));
            }

            return ret;
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void silentApplyDiseases(Dictionary<Disease, DiseaseExpiration> diseaseMap)
    {
        chLock.EnterReadLock();
        try
        {
            long curTime = Server.getInstance().getCurrentTime();

            foreach (var di in diseaseMap)
            {
                long expTime = curTime + di.Value.LeftTime;

                diseaseExpires.AddOrUpdate(di.Key, expTime);
                diseases.AddOrUpdate(di.Key, new(new DiseaseValueHolder(curTime, di.Value.LeftTime), di.Value.MobSkill));
            }
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void announceDiseases()
    {
        HashSet<KeyValuePair<Disease, KeyValuePair<DiseaseValueHolder, MobSkill>>> chrDiseases;

        chLock.EnterReadLock();
        try
        {
            // Poison damage visibility and diseases status visibility, extended through map transitions thanks to Ronan
            if (!this.isLoggedinWorld())
            {
                return;
            }

            chrDiseases = new(diseases);
        }
        finally
        {
            chLock.ExitReadLock();
        }

        foreach (var di in chrDiseases)
        {
            Disease disease = di.Key;
            MobSkill skill = di.Value.Value;
            List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

            if (disease != Disease.SLOW)
            {
                map.broadcastMessage(PacketCreator.giveForeignDebuff(id, debuff, skill));
            }
            else
            {
                map.broadcastMessage(PacketCreator.giveForeignSlowDebuff(id, debuff, skill));
            }
        }
    }

    public void collectDiseases()
    {
        foreach (Character chr in map.getAllPlayers())
        {
            int cid = chr.getId();

            foreach (var di in chr.getAllDiseases())
            {
                Disease disease = di.Key;
                MobSkill skill = di.Value.MobSkill;
                List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

                if (disease != Disease.SLOW)
                {
                    this.sendPacket(PacketCreator.giveForeignDebuff(cid, debuff, skill));
                }
                else
                {
                    this.sendPacket(PacketCreator.giveForeignSlowDebuff(cid, debuff, skill));
                }
            }
        }
    }

    public void giveDebuff(Disease disease, MobSkill skill)
    {
        if (!hasDisease(disease) && getDiseasesSize() < 2)
        {
            if (!(disease == Disease.SEDUCE || disease == Disease.STUN))
            {
                if (hasActiveBuff(Bishop.HOLY_SHIELD))
                {
                    return;
                }
            }

            chLock.EnterReadLock();
            try
            {
                long curTime = Server.getInstance().getCurrentTime();
                diseaseExpires.AddOrUpdate(disease, curTime + skill.getDuration());
                diseases.AddOrUpdate(disease, new(new DiseaseValueHolder(curTime, skill.getDuration()), skill));
            }
            finally
            {
                chLock.ExitReadLock();
            }

            if (disease == Disease.SEDUCE && chair.get() < 0)
            {
                sitChair(-1);
            }

            List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));
            sendPacket(PacketCreator.giveDebuff(debuff, skill));

            if (disease != Disease.SLOW)
            {
                map.broadcastMessage(this, PacketCreator.giveForeignDebuff(id, debuff, skill), false);
            }
            else
            {
                map.broadcastMessage(this, PacketCreator.giveForeignSlowDebuff(id, debuff, skill), false);
            }
        }
    }

    public void dispelDebuff(Disease debuff)
    {
        if (hasDisease(debuff))
        {
            long mask = (long)debuff.getValue();
            sendPacket(PacketCreator.cancelDebuff(mask));

            if (debuff != Disease.SLOW)
            {
                map.broadcastMessage(this, PacketCreator.cancelForeignDebuff(id, mask), false);
            }
            else
            {
                map.broadcastMessage(this, PacketCreator.cancelForeignSlowDebuff(id), false);
            }

            chLock.EnterReadLock();
            try
            {
                diseases.Remove(debuff);
                diseaseExpires.Remove(debuff);
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }
    }

    public void dispelDebuffs()
    {
        dispelDebuff(Disease.CURSE);
        dispelDebuff(Disease.DARKNESS);
        dispelDebuff(Disease.POISON);
        dispelDebuff(Disease.SEAL);
        dispelDebuff(Disease.WEAKEN);
        dispelDebuff(Disease.SLOW);    // thanks Conrad for noticing ZOMBIFY isn't dispellable
    }

    public void purgeDebuffs()
    {
        dispelDebuff(Disease.SEDUCE);
        dispelDebuff(Disease.ZOMBIFY);
        dispelDebuff(Disease.CONFUSE);
        dispelDebuffs();
    }

    public void cancelAllDebuffs()
    {
        chLock.EnterReadLock();
        try
        {
            diseases.Clear();
            diseaseExpires.Clear();
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void dispelSkill(int skillid)
    {
        List<BuffStatValueHolder> allBuffs = getAllStatups();
        foreach (BuffStatValueHolder mbsvh in allBuffs)
        {
            if (skillid == 0)
            {
                if (mbsvh.effect.isSkill() && (mbsvh.effect.getSourceId() % 10000000 == 1004 || dispelSkills(mbsvh.effect.getSourceId())))
                {
                    cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                }
            }
            else if (mbsvh.effect.isSkill() && mbsvh.effect.getSourceId() == skillid)
            {
                cancelEffect(mbsvh.effect, false, mbsvh.startTime);
            }
        }
    }

    private static bool dispelSkills(int skillid)
    {
        switch (skillid)
        {
            case DarkKnight.BEHOLDER:
            case FPArchMage.ELQUINES:
            case ILArchMage.IFRIT:
            case Priest.SUMMON_DRAGON:
            case Bishop.BAHAMUT:
            case Ranger.PUPPET:
            case Ranger.SILVER_HAWK:
            case Sniper.PUPPET:
            case Sniper.GOLDEN_EAGLE:
            case Hermit.SHADOW_PARTNER:
                return true;
            default:
                return false;
        }
    }

    public void changeFaceExpression(int emote)
    {
        long timeNow = Server.getInstance().getCurrentTime();
        // Client allows changing every 2 seconds. Give it a little bit of overhead for packet delays.
        if (timeNow - lastExpression > 1500)
        {
            lastExpression = timeNow;
            getMap().broadcastMessage(this, PacketCreator.facialExpression(this, emote), false);
        }
    }

    public void doHurtHp()
    {
        if (!(this.getInventory(InventoryType.EQUIPPED).findById(getMap().getHPDecProtect()) != null || buffMapProtection()))
        {
            addHP(-getMap().getHPDec());
        }
    }

    public void dropMessage(string message)
    {
        dropMessage(0, message);
    }

    public void dropMessage(int type, string message)
    {
        sendPacket(PacketCreator.serverNotice(type, message));
    }

    public void enteredScript(string script, int mapid)
    {
        if (!entered.ContainsKey(mapid))
        {
            entered.Add(mapid, script);
        }
    }

    public void equipChanged()
    {
        getMap().broadcastUpdateCharLookMessage(this, this);
        equipchanged = true;
        updateLocalStats();
        if (getMessenger() != null)
        {
            getWorldServer().updateMessenger(getMessenger()!, getName(), getWorld(), client.getChannel());
        }
    }

    public void cancelDiseaseExpireTask()
    {
        if (_diseaseExpireTask != null)
        {
            _diseaseExpireTask.cancel(false);
            _diseaseExpireTask = null;
        }
    }

    public void diseaseExpireTask()
    {
        if (_diseaseExpireTask == null)
        {
            _diseaseExpireTask = TimerManager.getInstance().register(() =>
            {
                HashSet<Disease> toExpire = new();

                chLock.EnterReadLock();
                try
                {
                    long curTime = Server.getInstance().getCurrentTime();

                    foreach (var de in diseaseExpires)
                    {
                        if (de.Value < curTime)
                        {
                            toExpire.Add(de.Key);
                        }
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                }

                foreach (Disease d in toExpire)
                {
                    dispelDebuff(d);
                }

            }, 1500);
        }
    }

    public void cancelBuffExpireTask()
    {
        if (_buffExpireTask != null)
        {
            _buffExpireTask.cancel(false);
            _buffExpireTask = null;
        }
    }

    public void buffExpireTask()
    {
        if (_buffExpireTask == null)
        {
            _buffExpireTask = TimerManager.getInstance().register(() =>
            {
                HashSet<KeyValuePair<int, long>> es;
                List<BuffStatValueHolder> toCancel = new();

                Monitor.Enter(effLock);
                chLock.EnterReadLock();
                try
                {
                    es = new(buffExpires);

                    long curTime = Server.getInstance().getCurrentTime();
                    foreach (var bel in es)
                    {
                        if (curTime >= bel.Value)
                        {
                            toCancel.Add(buffEffects.GetValueOrDefault(bel.Key)!.Values.First());    //rofl
                        }
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                    Monitor.Exit(effLock);
                }

                foreach (BuffStatValueHolder mbsvh in toCancel)
                {
                    cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                }

            }, 1500);
        }
    }

    public void cancelSkillCooldownTask()
    {
        if (_skillCooldownTask != null)
        {
            _skillCooldownTask.cancel(false);
            _skillCooldownTask = null;
        }
    }

    public void skillCooldownTask()
    {
        if (_skillCooldownTask == null)
        {
            _skillCooldownTask = TimerManager.getInstance().register(() =>
            {
                HashSet<KeyValuePair<int, CooldownValueHolder>> es;

                Monitor.Enter(effLock);
                chLock.EnterReadLock();
                try
                {
                    es = new(coolDowns);
                }
                finally
                {
                    chLock.ExitReadLock();
                    Monitor.Exit(effLock);
                }

                long curTime = Server.getInstance().getCurrentTime();
                foreach (var bel in es)
                {
                    CooldownValueHolder mcdvh = bel.Value;
                    if (curTime >= mcdvh.startTime + mcdvh.length)
                    {
                        removeCooldown(mcdvh.skillId);
                        sendPacket(PacketCreator.skillCooldown(mcdvh.skillId, 0));
                    }
                }
            }, 1500);
        }
    }

    public void cancelExpirationTask()
    {
        if (itemExpireTask != null)
        {
            itemExpireTask.cancel(false);
            itemExpireTask = null;
        }
    }

    public void expirationTask()
    {
        if (itemExpireTask == null)
        {
            itemExpireTask = TimerManager.getInstance().register(() =>
            {
                bool deletedCoupon = false;

                long expiration, currenttime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (var skill in getSkills())
                {
                    if (skill.Value.expiration != -1 && skill.Value.expiration < currenttime)
                    {
                        changeSkillLevel(skill.Key, -1, 0, -1);
                    }
                }
                List<Item> toberemove = new();
                foreach (Inventory inv in inventory)
                {
                    foreach (Item item in inv.list())
                    {
                        expiration = item.getExpiration();

                        if (expiration != -1 && (expiration < currenttime) && ((item.getFlag() & ItemConstants.LOCK) == ItemConstants.LOCK))
                        {
                            short lockObj = item.getFlag();
                            lockObj &= ~(ItemConstants.LOCK);
                            item.setFlag(lockObj); //Probably need a check, else people can make expiring items into permanent items...
                            item.setExpiration(-1);
                            forceUpdateItem(item);   //TEST :3
                        }
                        else if (expiration != -1 && expiration < currenttime)
                        {
                            if (!ItemConstants.isPet(item.getItemId()))
                            {
                                sendPacket(PacketCreator.itemExpired(item.getItemId()));
                                toberemove.Add(item);
                                if (ItemConstants.isRateCoupon(item.getItemId()))
                                {
                                    deletedCoupon = true;
                                }
                            }
                            else
                            {
                                var pet = item.getPet();   // thanks Lame for noticing pets not getting despawned after expiration time
                                if (pet != null)
                                {
                                    unequipPet(pet, true);
                                }

                                if (ItemConstants.isExpirablePet(item.getItemId()))
                                {
                                    sendPacket(PacketCreator.itemExpired(item.getItemId()));
                                    toberemove.Add(item);
                                }
                                else
                                {
                                    item.setExpiration(-1);
                                    forceUpdateItem(item);
                                }
                            }
                        }
                    }

                    if (toberemove.Count > 0)
                    {
                        foreach (Item item in toberemove)
                        {
                            InventoryManipulator.removeFromSlot(client, inv.getType(), item.getPosition(), item.getQuantity(), true);
                        }

                        ItemInformationProvider ii = ItemInformationProvider.getInstance();
                        foreach (Item item in toberemove)
                        {
                            List<int> toadd = new();
                            var replace = ii.getReplaceOnExpire(item.getItemId());
                            if (replace.Id > 0)
                            {
                                toadd.Add(replace.Id);
                                if (replace.Message.Length > 0)
                                {
                                    dropMessage(replace.Message);
                                }
                            }
                            foreach (int itemid in toadd)
                            {
                                InventoryManipulator.addById(client, itemid, 1);
                            }
                        }

                        toberemove.Clear();
                    }

                    if (deletedCoupon)
                    {
                        updateCouponRates();
                    }
                }

            }, 60000);
        }
    }

    public enum FameStatus
    {

        OK, NOT_TODAY, NOT_THIS_MONTH
    }

    public void forceUpdateItem(Item item)
    {
        List<ModifyInventory> mods = new();
        mods.Add(new ModifyInventory(3, item));
        mods.Add(new ModifyInventory(0, item));
        sendPacket(PacketCreator.modifyInventory(true, mods));
    }

    public void gainGachaExp()
    {
        int expgain = 0;
        long currentgexp = gachaexp.get();
        if ((currentgexp + exp.get()) >= ExpTable.getExpNeededForLevel(level))
        {
            expgain += ExpTable.getExpNeededForLevel(level) - exp.get();

            int nextneed = ExpTable.getExpNeededForLevel(level + 1);
            if (currentgexp - expgain >= nextneed)
            {
                expgain += nextneed;
            }

            this.gachaexp.set((int)(currentgexp - expgain));
        }
        else
        {
            expgain = this.gachaexp.getAndSet(0);
        }
        gainExp(expgain, false, true);
        updateSingleStat(Stat.GACHAEXP, this.gachaexp.get());
    }

    public void addGachaExp(int gain)
    {
        updateSingleStat(Stat.GACHAEXP, gachaexp.addAndGet(gain));
    }

    public void gainExp(int gain)
    {
        gainExp(gain, true, true);
    }

    public void gainExp(int gain, bool show, bool inChat)
    {
        gainExp(gain, show, inChat, true);
    }

    public void gainExp(int gain, bool show, bool inChat, bool white)
    {
        gainExp(gain, 0, show, inChat, white);
    }

    public void gainExp(int gain, int party, bool show, bool inChat, bool white)
    {
        if (hasDisease(Disease.CURSE))
        {
            gain *= (int)0.5;
            party *= (int)0.5;
        }

        if (gain < 0)
        {
            gain = int.MaxValue;   // integer overflow, heh.
        }

        if (party < 0)
        {
            party = int.MaxValue;  // integer overflow, heh.
        }

        int equip = (int)Math.Min((long)(gain / 10) * pendantExp, int.MaxValue);

        gainExpInternal(gain, equip, party, show, inChat, white);
    }

    public void loseExp(int loss, bool show, bool inChat)
    {
        loseExp(loss, show, inChat, true);
    }

    public void loseExp(int loss, bool show, bool inChat, bool white)
    {
        gainExpInternal(-loss, 0, 0, show, inChat, white);
    }

    private void announceExpGain(long gain, int equip, int party, bool inChat, bool white)
    {
        gain = Math.Min(gain, int.MaxValue);
        if (gain == 0)
        {
            if (party == 0)
            {
                return;
            }

            gain = party;
            party = 0;
            white = false;
        }

        sendPacket(PacketCreator.getShowExpGain((int)gain, equip, party, inChat, white));
    }

    object gainExpLock = new object();
    private void gainExpInternal(long gain, int equip, int party, bool show, bool inChat, bool white)
    {   // need of method synchonization here detected thanks to MedicOP
        lock (gainExpLock)
        {
            long total = Math.Max(gain + equip + party, -exp.get());

            if (level < getMaxLevel() && (allowExpGain || this.getEventInstance() != null))
            {
                long leftover = 0;
                long nextExp = exp.get() + total;

                if (nextExp > int.MaxValue)
                {
                    total = int.MaxValue - exp.get();
                    leftover = nextExp - int.MaxValue;
                }
                updateSingleStat(Stat.EXP, exp.addAndGet((int)total));
                totalExpGained += total;
                if (show)
                {
                    announceExpGain(gain, equip, party, inChat, white);
                }
                while (exp.get() >= ExpTable.getExpNeededForLevel(level))
                {
                    levelUp(true);
                    if (level == getMaxLevel())
                    {
                        setExp(0);
                        updateSingleStat(Stat.EXP, 0);
                        break;
                    }
                }

                if (leftover > 0)
                {
                    gainExpInternal(leftover, equip, party, false, inChat, white);
                }
                else
                {
                    lastExpGainTime = DateTimeOffset.Now;

                    if (YamlConfig.config.server.USE_EXP_GAIN_LOG)
                    {
                        ExpLogRecord expLogRecord = new ExpLogRecord(
                            id,
                            getWorldServer().getExpRate(),
                            expCoupon,
                            totalExpGained,
                            exp.get(),
                            lastExpGainTime
                        );
                        ExpLogger.putExpLogRecord(expLogRecord);
                    }

                    totalExpGained = 0;
                }
            }
        }

    }

    private KeyValuePair<int, int> applyFame(int delta)
    {
        Monitor.Enter(petLock);
        try
        {
            int newFame = fame + delta;
            if (newFame < -30000)
            {
                delta = -(30000 + fame);
            }
            else if (newFame > 30000)
            {
                delta = 30000 - fame;
            }

            fame += delta;
            return new(fame, delta);
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public void gainFame(int delta)
    {
        gainFame(delta, null, 0);
    }

    public bool gainFame(int delta, Character? fromPlayer, int mode)
    {
        KeyValuePair<int, int> fameRes = applyFame(delta);
        delta = fameRes.Value;
        if (delta != 0)
        {
            int thisFame = fameRes.Key;
            updateSingleStat(Stat.FAME, thisFame);

            if (fromPlayer != null)
            {
                fromPlayer.sendPacket(PacketCreator.giveFameResponse(mode, getName(), thisFame));
                sendPacket(PacketCreator.receiveFame(mode, fromPlayer.getName()));
            }
            else
            {
                sendPacket(PacketCreator.getShowFameGain(delta));
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool canHoldMeso(int gain)
    {  // thanks lucasziron for pointing out a need to check space availability for mesos on player transactions
        long nextMeso = (long)meso.get() + gain;
        return nextMeso <= int.MaxValue;
    }

    public void gainMeso(int gain)
    {
        gainMeso(gain, true, false, true);
    }

    public void gainMeso(int gain, bool show)
    {
        gainMeso(gain, show, false, false);
    }

    public void gainMeso(int gain, bool show, bool enableActions, bool inChat)
    {
        long nextMeso;
        Monitor.Enter(petLock);
        try
        {
            nextMeso = (long)meso.get() + gain;  // thanks Thora for pointing integer overflow here
            if (nextMeso > int.MaxValue)
            {
                gain -= (int)(nextMeso - int.MaxValue);
            }
            else if (nextMeso < 0)
            {
                gain = -meso.get();
            }
            nextMeso = meso.addAndGet(gain);
        }
        finally
        {
            Monitor.Exit(petLock);
        }

        if (gain != 0)
        {
            updateSingleStat(Stat.MESO, (int)nextMeso, enableActions);
            if (show)
            {
                sendPacket(PacketCreator.getShowMesoGain(gain, inChat));
            }
        }
        else
        {
            sendPacket(PacketCreator.enableActions());
        }
    }

    public void genericGuildMessage(int code)
    {
        this.sendPacket(GuildPackets.genericGuildMessage((byte)code));
    }

    public int getAccountID()
    {
        return accountid;
    }

    public List<PlayerCoolDownValueHolder> getAllCooldowns()
    {
        List<PlayerCoolDownValueHolder> ret = new();

        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            foreach (CooldownValueHolder mcdvh in coolDowns.Values)
            {
                ret.Add(new PlayerCoolDownValueHolder(mcdvh.skillId, mcdvh.startTime, mcdvh.length));
            }
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }

        return ret;
    }

    public int getAllianceRank()
    {
        return allianceRank;
    }

    public static string getAriantRoomLeaderName(int room)
    {
        return ariantroomleader[room];
    }

    public static int getAriantSlotsRoom(int room)
    {
        return ariantroomslot[room];
    }

    public void updateAriantScore()
    {
        updateAriantScore(0);
    }

    public void updateAriantScore(int dropQty)
    {
        AriantColiseum arena = this.getAriantColiseum();
        if (arena != null)
        {
            arena.updateAriantScore(this, countItem(ItemId.ARPQ_SPIRIT_JEWEL));

            if (dropQty > 0)
            {
                arena.addLostShards(dropQty);
            }
        }
    }

    public int getBattleshipHp()
    {
        return battleshipHp;
    }

    public BuddyList getBuddylist()
    {
        return buddylist;
    }

    public static Dictionary<string, string> getCharacterFromDatabase(string name)
    {
        using DBContext dbContext = new DBContext();
        var ds = dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.Id, x.AccountId, x.Name }).FirstOrDefault();

        Dictionary<string, string> character = new();
        character.Add(nameof(ds.Id), ds.Id.ToString());
        character.Add(nameof(ds.AccountId), ds.AccountId.ToString());
        character.Add(nameof(ds.Name), ds.Name);

        return character;
    }

    public long? getBuffedStarttime(BuffStat effect)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(effect);
            if (mbsvh == null)
            {
                return null;
            }
            return mbsvh.startTime;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public int? getBuffedValue(BuffStat effect)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(effect);
            if (mbsvh == null)
            {
                return null;
            }
            return mbsvh.value;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public int getBuffSource(BuffStat stat)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(stat);
            if (mbsvh == null)
            {
                return -1;
            }
            return mbsvh.effect.getSourceId();
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public StatEffect? getBuffEffect(BuffStat stat)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(stat);
            if (mbsvh == null)
            {
                return null;
            }
            else
            {
                return mbsvh.effect;
            }
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public HashSet<int> getAvailableBuffs()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            return new(buffEffects.Keys);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    private List<BuffStatValueHolder> getAllStatups()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            List<BuffStatValueHolder> ret = new();
            foreach (Dictionary<BuffStat, BuffStatValueHolder> bel in buffEffects.Values)
            {
                foreach (BuffStatValueHolder mbsvh in bel.Values)
                {
                    ret.Add(mbsvh);
                }
            }
            return ret;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public List<PlayerBuffValueHolder> getAllBuffs()
    {  // buff values will be stored in an arbitrary order
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            long curtime = Server.getInstance().getCurrentTime();

            Dictionary<int, PlayerBuffValueHolder> ret = new();
            foreach (Dictionary<BuffStat, BuffStatValueHolder> bel in buffEffects.Values)
            {
                foreach (BuffStatValueHolder mbsvh in bel.Values)
                {
                    int srcid = mbsvh.effect.getBuffSourceId();
                    if (!ret.ContainsKey(srcid))
                    {
                        ret.Add(srcid, new PlayerBuffValueHolder((int)(curtime - mbsvh.startTime), mbsvh.effect));
                    }
                }
            }
            return new(ret.Values);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    //public List<KeyValuePair<BuffStat, int>> getAllActiveStatups()
    //{
    //    Monitor.Enter(effLock);
    //    chLock.EnterReadLock();
    //    try
    //    {
    //        return effects.Select(x => new KeyValuePair<BuffStat, int>(x.Key, x.Value.value)).ToList();
    //    }
    //    finally
    //    {
    //        chLock.ExitReadLock();
    //        Monitor.Exit(effLock);
    //    }
    //}

    public bool hasBuffFromSourceid(int sourceid)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            return buffEffects.ContainsKey(sourceid);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public bool hasActiveBuff(int sourceid)
    {
        LinkedList<BuffStatValueHolder> allBuffs;

        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            allBuffs = new(effects.Values);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }

        foreach (BuffStatValueHolder mbsvh in allBuffs)
        {
            if (mbsvh.effect.getBuffSourceId() == sourceid)
            {
                return true;
            }
        }
        return false;
    }

    private List<KeyValuePair<BuffStat, int>> getActiveStatupsFromSourceid(int sourceid)
    {
        if (!buffEffects.ContainsKey(sourceid))
            return new List<KeyValuePair<BuffStat, int>>();
        // already under effLock & chrLock
        List<KeyValuePair<BuffStat, int>> ret = new();
        List<KeyValuePair<BuffStat, int>> singletonStatups = new();
        foreach (var bel in buffEffects[sourceid])
        {
            BuffStat mbs = bel.Key;
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(bel.Key);

            KeyValuePair<BuffStat, int> p;
            if (mbsvh != null)
            {
                p = new(mbs, mbsvh.value);
            }
            else
            {
                p = new(mbs, 0);
            }

            if (!isSingletonStatup(mbs))
            {   // thanks resinate, Daddy Egg for pointing out morph issues when updating it along with other statups
                ret.Add(p);
            }
            else
            {
                singletonStatups.Add(p);
            }
        }
        ret.Sort((p1, p2) => p1.Key.CompareTo(p2.Key));

        if (singletonStatups.Count > 0)
        {
            singletonStatups.Sort((p1, p2) => p1.Key.CompareTo(p2.Key));

            ret.AddRange(singletonStatups);
        }

        return ret;
    }

    private void addItemEffectHolder(int sourceid, long expirationtime, Dictionary<BuffStat, BuffStatValueHolder> statups)
    {
        buffEffects.AddOrUpdate(sourceid, statups);
        buffExpires.AddOrUpdate(sourceid, expirationtime);
    }

    private bool removeEffectFromItemEffectHolder(int sourceid, BuffStat buffStat)
    {
        Dictionary<BuffStat, BuffStatValueHolder>? lbe = buffEffects.GetValueOrDefault(sourceid);

        if (lbe != null && lbe.Remove(buffStat, out var d) && d != null)
        {
            buffEffectsCount.AddOrUpdate(buffStat, (sbyte)(buffEffectsCount.GetValueOrDefault(buffStat) - 1));

            if (lbe.Count == 0)
            {
                buffEffects.Remove(sourceid);
                buffExpires.Remove(sourceid);
            }

            return true;
        }

        return false;
    }

    private void removeItemEffectHolder(int sourceid)
    {
        buffEffects.Remove(sourceid, out var be);
        if (be != null)
        {
            foreach (var bei in be)
            {
                buffEffectsCount.AddOrUpdate(bei.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(bei.Key) - 1));
            }
        }

        buffExpires.Remove(sourceid);
    }

    private void dropWorstEffectFromItemEffectHolder(BuffStat mbs)
    {
        int min = int.MaxValue;
        int srcid = -1;
        foreach (var bpl in buffEffects)
        {
            BuffStatValueHolder? mbsvh = bpl.Value.GetValueOrDefault(mbs);
            if (mbsvh != null)
            {
                if (mbsvh.value < min)
                {
                    min = mbsvh.value;
                    srcid = bpl.Key;
                }
            }
        }

        removeEffectFromItemEffectHolder(srcid, mbs);
    }

    private BuffStatValueHolder? fetchBestEffectFromItemEffectHolder(BuffStat mbs)
    {
        KeyValuePair<int, int> max = new(int.MinValue, 0);
        BuffStatValueHolder? mbsvh = null;
        foreach (var bpl in buffEffects)
        {
            BuffStatValueHolder? mbsvhi = bpl.Value.GetValueOrDefault(mbs);
            if (mbsvhi != null)
            {
                if (!mbsvhi.effect.isActive(this))
                {
                    continue;
                }

                if (mbsvhi.value > max.Key)
                {
                    max = new(mbsvhi.value, mbsvhi.effect.getStatups().Count);
                    mbsvh = mbsvhi;
                }
                else if (mbsvhi.value == max.Key && mbsvhi.effect.getStatups().Count > max.Value)
                {
                    max = new(mbsvhi.value, mbsvhi.effect.getStatups().Count);
                    mbsvh = mbsvhi;
                }
            }
        }

        if (mbsvh != null)
        {
            effects.AddOrUpdate(mbs, mbsvh);
        }
        return mbsvh;
    }

    private void extractBuffValue(int sourceid, BuffStat stat)
    {
        chLock.EnterReadLock();
        try
        {
            removeEffectFromItemEffectHolder(sourceid, stat);
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void debugListAllBuffs()
    {
        //Monitor.Enter(effLock);
        //chLock.EnterReadLock();
        //try
        //{
        //    log.debug("-------------------");
        //    log.debug("CACHED BUFF COUNT: {}", buffEffectsCount.stream()
        //            .map(entry->entry.Key + ": " + entry.getValue())
        //            .collect(Collectors.joining(", "))
        //    );

        //    log.debug("-------------------");
        //    log.debug("CACHED BUFFS: {}", buffEffects.stream()
        //            .map(entry->entry.Key + ": (" + entry.getValue().stream()
        //                    .map(innerEntry->innerEntry.Key.name() + innerEntry.getValue().value)
        //                    .collect(Collectors.joining(", ")) + ")")
        //            .collect(Collectors.joining(", "))
        //    );

        //    log.debug("-------------------");
        //    log.debug("IN ACTION: {}", effects.stream()
        //            .map(entry->entry.Key.name() + " -> " + ItemInformationProvider.getInstance().getName(entry.getValue().effect.getSourceId()))
        //            .collect(Collectors.joining(", "))
        //    );
        //}
        //finally
        //{
        //    chLock.ExitReadLock();
        //    Monitor.Exit(effLock);
        //}
    }

    public void debugListAllBuffsCount()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            log.Debug("ALL BUFFS COUNT: {Buffs}", string.Join(", ", buffEffectsCount.Select(entry => entry.Key.name() + " -> " + entry.Value))
            ); ;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void cancelAllBuffs(bool softcancel)
    {
        if (softcancel)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                cancelEffectFromBuffStat(BuffStat.SUMMON);
                cancelEffectFromBuffStat(BuffStat.PUPPET);
                cancelEffectFromBuffStat(BuffStat.COMBO);

                effects.Clear();

                foreach (int srcid in buffEffects.Keys)
                {
                    removeItemEffectHolder(srcid);
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }
        else
        {
            Dictionary<StatEffect, long> mseBuffs = new();

            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                foreach (var bpl in buffEffects)
                {
                    foreach (var mbse in bpl.Value)
                    {
                        mseBuffs.AddOrUpdate(mbse.Value.effect, mbse.Value.startTime);
                    }
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }

            foreach (var mse in mseBuffs)
            {
                cancelEffect(mse.Key, false, mse.Value);
            }
        }
    }

    private void dropBuffStats(List<BuffStateValuePair> effectsToCancel)
    {
        foreach (var cancelEffectCancelTasks in effectsToCancel)
        {
            //bool nestedCancel = false;

            chLock.EnterReadLock();
            try
            {
                /*
                if (buffExpires.get(cancelEffectCancelTasks.getRight().effect.getBuffSourceId()) != null) {
                    nestedCancel = true;
                }*/

                if (cancelEffectCancelTasks.ValueHolder.bestApplied)
                {
                    fetchBestEffectFromItemEffectHolder(cancelEffectCancelTasks.BuffStat);
                }
            }
            finally
            {
                chLock.ExitReadLock();
            }

            /*
            if (nestedCancel) {
                this.cancelEffect(cancelEffectCancelTasks.getRight().effect, false, -1, false);
            }*/
        }
    }

    private List<BuffStateValuePair> deregisterBuffStats(Dictionary<BuffStat, BuffStatValueHolder> stats)
    {
        chLock.EnterReadLock();
        try
        {
            List<BuffStateValuePair> effectsToCancel = new(stats.Count);
            foreach (var stat in stats)
            {
                int sourceid = stat.Value.effect.getBuffSourceId();

                if (!buffEffects.ContainsKey(sourceid))
                {
                    buffExpires.Remove(sourceid);
                }

                BuffStat mbs = stat.Key;
                effectsToCancel.Add(new(mbs, stat.Value));

                BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
                if (mbsvh != null && mbsvh.effect.getBuffSourceId() == sourceid)
                {
                    mbsvh.bestApplied = true;
                    effects.Remove(mbs);

                    if (mbs == BuffStat.RECOVERY)
                    {
                        if (recoveryTask != null)
                        {
                            recoveryTask.cancel(false);
                            recoveryTask = null;
                        }
                    }
                    else if (mbs == BuffStat.SUMMON || mbs == BuffStat.PUPPET)
                    {
                        int summonId = mbsvh.effect.getSourceId();

                        Summon? summon = summons.GetValueOrDefault(summonId);
                        if (summon != null)
                        {
                            getMap().broadcastMessage(PacketCreator.removeSummon(summon, true), summon.getPosition());
                            getMap().removeMapObject(summon);
                            removeVisibleMapObject(summon);

                            summons.Remove(summonId);
                            if (summon.isPuppet())
                            {
                                map.removePlayerPuppet(this);
                            }
                            else if (summon.getSkill() == DarkKnight.BEHOLDER)
                            {
                                if (beholderHealingSchedule != null)
                                {
                                    beholderHealingSchedule.cancel(false);
                                    beholderHealingSchedule = null;
                                }
                                if (beholderBuffSchedule != null)
                                {
                                    beholderBuffSchedule.cancel(false);
                                    beholderBuffSchedule = null;
                                }
                            }
                        }
                    }
                    else if (mbs == BuffStat.DRAGONBLOOD)
                    {
                        dragonBloodSchedule?.cancel(false);
                        dragonBloodSchedule = null;
                    }
                    else if (mbs == BuffStat.HPREC || mbs == BuffStat.MPREC)
                    {
                        if (mbs == BuffStat.HPREC)
                        {
                            extraHpRec = 0;
                        }
                        else
                        {
                            extraMpRec = 0;
                        }

                        if (extraRecoveryTask != null)
                        {
                            extraRecoveryTask.cancel(false);
                            extraRecoveryTask = null;
                        }

                        if (extraHpRec != 0 || extraMpRec != 0)
                        {
                            startExtraTaskInternal(extraHpRec, extraMpRec, extraRecInterval);
                        }
                    }
                }
            }

            return effectsToCancel;
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void cancelEffect(int itemId)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        cancelEffect(ii.getItemEffect(itemId), false, -1);
    }

    public bool cancelEffect(StatEffect effect, bool overwrite, long startTime)
    {
        bool ret;

        Monitor.Enter(prtLock);
        Monitor.Enter(effLock);
        try
        {
            ret = cancelEffect(effect, overwrite, startTime, true);
        }
        finally
        {
            Monitor.Exit(effLock);
            Monitor.Exit(prtLock);
        }

        if (effect.isMagicDoor() && ret)
        {
            Monitor.Enter(prtLock);
            Monitor.Enter(effLock);
            try
            {
                if (!hasBuffFromSourceid(Priest.MYSTIC_DOOR))
                {
                    Door.attemptRemoveDoor(this);
                }
            }
            finally
            {
                Monitor.Exit(effLock);
                Monitor.Exit(prtLock);
            }
        }

        return ret;
    }

    private static StatEffect? getEffectFromBuffSource(Dictionary<BuffStat, BuffStatValueHolder> buffSource)
    {
        try
        {
            return buffSource.FirstOrDefault().Value?.effect;
        }
        catch
        {
            return null;
        }
    }

    private bool isUpdatingEffect(HashSet<StatEffect> activeEffects, StatEffect? mse)
    {
        if (mse == null)
        {
            return false;
        }

        // thanks xinyifly for noticing "Speed Infusion" crashing game when updating buffs during map transition
        bool active = mse.isActive(this);
        if (active)
        {
            return !activeEffects.Contains(mse);
        }
        else
        {
            return activeEffects.Contains(mse);
        }
    }

    public void updateActiveEffects()
    {
        Monitor.Enter(effLock);     // thanks davidlafriniere, maple006, RedHat for pointing a deadlock occurring here
        try
        {
            HashSet<BuffStat> updatedBuffs = new();
            HashSet<StatEffect> activeEffects = new();

            foreach (BuffStatValueHolder mse in effects.Values)
            {
                activeEffects.Add(mse.effect);
            }

            foreach (Dictionary<BuffStat, BuffStatValueHolder> buff in buffEffects.Values)
            {
                StatEffect? mse = getEffectFromBuffSource(buff);
                if (isUpdatingEffect(activeEffects, mse))
                {
                    foreach (var p in mse!.getStatups())
                    {
                        updatedBuffs.Add(p.Key);
                    }
                }
            }

            foreach (BuffStat mbs in updatedBuffs)
            {
                effects.Remove(mbs);
            }

            updateEffects(updatedBuffs);
        }
        finally
        {
            Monitor.Exit(effLock);
        }
    }

    private void updateEffects(HashSet<BuffStat> removedStats)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            HashSet<BuffStat> retrievedStats = new();

            foreach (BuffStat mbs in removedStats)
            {
                fetchBestEffectFromItemEffectHolder(mbs);

                BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
                if (mbsvh != null)
                {
                    foreach (var statup in mbsvh.effect.getStatups())
                    {
                        retrievedStats.Add(statup.Key);
                    }
                }
            }

            propagateBuffEffectUpdates(new(), retrievedStats, removedStats);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    private bool cancelEffect(StatEffect effect, bool overwrite, long startTime, bool firstCancel)
    {
        HashSet<BuffStat> removedStats = new();
        dropBuffStats(cancelEffectInternal(effect, overwrite, startTime, removedStats));
        updateLocalStats();
        updateEffects(removedStats);

        return removedStats.Count > 0;
    }

    private List<BuffStateValuePair> cancelEffectInternal(StatEffect effect, bool overwrite, long startTime, HashSet<BuffStat> removedStats)
    {
        Dictionary<BuffStat, BuffStatValueHolder>? buffstats = null;
        BuffStat? ombs;
        if (!overwrite)
        {   // is removing the source effect, meaning every effect from this srcid is being purged
            buffstats = extractCurrentBuffStats(effect);
        }
        else if ((ombs = getSingletonStatupFromEffect(effect)) != null)
        {   // removing all effects of a buff having non-shareable buff stat.
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(ombs);
            if (mbsvh != null)
            {
                buffstats = extractCurrentBuffStats(mbsvh.effect);
            }
        }

        if (buffstats == null)
        {            // all else, is dropping ALL current statups that uses same stats as the given effect
            buffstats = extractLeastRelevantStatEffectsIfFull(effect);
        }

        if (effect.isMapChair())
        {
            stopChairTask();
        }

        List<BuffStateValuePair> toCancel = deregisterBuffStats(buffstats);
        if (effect.isMonsterRiding())
        {
            this.getClient().getWorldServer().unregisterMountHunger(this);
            this.getMount().setActive(false);
        }

        if (!overwrite)
        {
            removedStats.addAll(buffstats.Keys);
        }

        return toCancel;
    }

    public void cancelEffectFromBuffStat(BuffStat stat)
    {
        BuffStatValueHolder? effect;

        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            effect = effects.GetValueOrDefault(stat);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
        if (effect != null)
        {
            cancelEffect(effect.effect, false, -1);
        }
    }

    public void cancelBuffStats(BuffStat stat)
    {
        Monitor.Enter(effLock);
        try
        {
            List<KeyValuePair<int, BuffStatValueHolder>> cancelList = new();

            chLock.EnterReadLock();
            try
            {
                foreach (var bel in this.buffEffects)
                {
                    BuffStatValueHolder? beli = bel.Value.GetValueOrDefault(stat);
                    if (beli != null)
                    {
                        cancelList.Add(new(bel.Key, beli));
                    }
                }
            }
            finally
            {
                chLock.ExitReadLock();
            }

            Dictionary<BuffStat, BuffStatValueHolder> buffStatList = new();
            foreach (var p in cancelList)
            {
                buffStatList.AddOrUpdate(stat, p.Value);
                extractBuffValue(p.Key, stat);
                dropBuffStats(deregisterBuffStats(buffStatList));
            }
        }
        finally
        {
            Monitor.Exit(effLock);
        }

        cancelPlayerBuffs(Arrays.asList(stat));
    }

    private Dictionary<BuffStat, BuffStatValueHolder> extractCurrentBuffStats(StatEffect effect)
    {
        chLock.EnterReadLock();
        try
        {
            Dictionary<BuffStat, BuffStatValueHolder> stats = new();
            buffEffects.Remove(effect.getBuffSourceId(), out var buffList);
            if (buffList != null)
            {
                foreach (var stateffect in buffList)
                {
                    stats.AddOrUpdate(stateffect.Key, stateffect.Value);
                    buffEffectsCount.AddOrUpdate(stateffect.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(stateffect.Key) - 1));
                }
            }

            return stats;
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    private Dictionary<BuffStat, BuffStatValueHolder> extractLeastRelevantStatEffectsIfFull(StatEffect effect)
    {
        Dictionary<BuffStat, BuffStatValueHolder> extractedStatBuffs = new();

        chLock.EnterReadLock();
        try
        {
            Dictionary<BuffStat, Byte> stats = new();
            Dictionary<BuffStat, BuffStatValueHolder> minStatBuffs = new();

            foreach (var mbsvhi in buffEffects)
            {
                foreach (var mbsvhe in mbsvhi.Value)
                {
                    BuffStat mbs = mbsvhe.Key;
                    var b = stats.get(mbs);

                    if (b != null)
                    {
                        stats.AddOrUpdate(mbs, (byte)(b + 1));
                        if (mbsvhe.Value.value < (minStatBuffs.GetValueOrDefault(mbs)?.value ?? 0))
                        {
                            minStatBuffs.AddOrUpdate(mbs, mbsvhe.Value);
                        }
                    }
                    else
                    {
                        stats.AddOrUpdate(mbs, (byte)1);
                        minStatBuffs.AddOrUpdate(mbs, mbsvhe.Value);
                    }
                }
            }

            HashSet<BuffStat> effectStatups = new();
            foreach (var efstat in effect.getStatups())
            {
                effectStatups.Add(efstat.Key);
            }

            foreach (var it in stats)
            {
                bool uniqueBuff = isSingletonStatup(it.Key);

                if (it.Value >= (!uniqueBuff ? YamlConfig.config.server.MAX_MONITORED_BUFFSTATS : 1) && effectStatups.Contains(it.Key))
                {
                    BuffStatValueHolder? mbsvh = minStatBuffs.GetValueOrDefault(it.Key)!;

                    Dictionary<BuffStat, BuffStatValueHolder>? lpbe = buffEffects.GetValueOrDefault(mbsvh.effect.getBuffSourceId());
                    lpbe.Remove(it.Key);
                    buffEffectsCount.AddOrUpdate(it.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(it.Key) - 1));

                    if (lpbe.Count == 0)
                    {
                        buffEffects.Remove(mbsvh.effect.getBuffSourceId());
                    }
                    extractedStatBuffs.AddOrUpdate(it.Key, mbsvh);
                }
            }
        }
        finally
        {
            chLock.ExitReadLock();
        }

        return extractedStatBuffs;
    }

    private void cancelInactiveBuffStats(HashSet<BuffStat> retrievedStats, HashSet<BuffStat> removedStats)
    {
        List<BuffStat> inactiveStats = new();
        foreach (BuffStat mbs in removedStats)
        {
            if (!retrievedStats.Contains(mbs))
            {
                inactiveStats.Add(mbs);
            }
        }

        if (inactiveStats.Count > 0)
        {
            sendPacket(PacketCreator.cancelBuff(inactiveStats));
            getMap().broadcastMessage(this, PacketCreator.cancelForeignBuff(getId(), inactiveStats), false);
        }
    }

    private static Dictionary<StatEffect, int> topologicalSortLeafStatCount(Dictionary<BuffStat, Stack<StatEffect>> buffStack)
    {
        Dictionary<StatEffect, int> leafBuffCount = new();

        foreach (var e in buffStack)
        {
            Stack<StatEffect> mseStack = e.Value;
            if (mseStack.Count == 0)
            {
                continue;
            }

            StatEffect mse = mseStack.Peek();

            leafBuffCount.AddOrUpdate(mse, leafBuffCount.GetValueOrDefault(mse) + 1);
        }

        return leafBuffCount;
    }

    private static List<StatEffect> topologicalSortRemoveLeafStats(Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats, Dictionary<BuffStat, Stack<StatEffect>> buffStack, Dictionary<StatEffect, int> leafStatCount)
    {
        List<StatEffect> clearedStatEffects = new();
        HashSet<BuffStat> clearedStats = new();

        foreach (var e in leafStatCount)
        {
            StatEffect mse = e.Key;

            if (stackedBuffStats.GetValueOrDefault(mse)?.Count <= e.Value)
            {
                clearedStatEffects.Add(mse);

                foreach (BuffStat mbs in stackedBuffStats.GetValueOrDefault(mse)!)
                {
                    clearedStats.Add(mbs);
                }
            }
        }

        foreach (BuffStat mbs in clearedStats)
        {
            if (buffStack.GetValueOrDefault(mbs)!.TryPop(out var mse))
                stackedBuffStats.GetValueOrDefault(mse)?.Remove(mbs);
        }

        return clearedStatEffects;
    }

    private static void topologicalSortRebaseLeafStats(Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats, Dictionary<BuffStat, Stack<StatEffect>> buffStack)
    {
        foreach (var e in buffStack)
        {
            Stack<StatEffect> mseStack = e.Value;

            if (mseStack.Count > 0)
            {
                if (mseStack.TryPop(out var mse))
                    stackedBuffStats.GetValueOrDefault(mse)?.Remove(e.Key);
            }
        }
    }

    private static List<StatEffect> topologicalSortEffects(Dictionary<BuffStat, List<KeyValuePair<StatEffect, int>>> buffEffects)
    {
        Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats = new();
        Dictionary<BuffStat, Stack<StatEffect>> buffStack = new();

        foreach (var e in buffEffects)
        {
            BuffStat mbs = e.Key;

            Stack<StatEffect> mbsStack = new();
            buffStack.AddOrUpdate(mbs, mbsStack);

            foreach (var emse in e.Value)
            {
                StatEffect mse = emse.Key;
                mbsStack.Push(mse);

                HashSet<BuffStat>? mbsStats = stackedBuffStats.GetValueOrDefault(mse);
                if (mbsStats == null)
                {
                    mbsStats = new();
                    stackedBuffStats.AddOrUpdate(mse, mbsStats);
                }

                mbsStats.Add(mbs);
            }
        }

        List<StatEffect> buffList = new();
        while (true)
        {
            Dictionary<StatEffect, int> leafStatCount = topologicalSortLeafStatCount(buffStack);
            if (leafStatCount.Count == 0)
            {
                break;
            }

            List<StatEffect> clearedNodes = topologicalSortRemoveLeafStats(stackedBuffStats, buffStack, leafStatCount);
            if (clearedNodes.Count == 0)
            {
                topologicalSortRebaseLeafStats(stackedBuffStats, buffStack);
            }
            else
            {
                buffList.AddRange(clearedNodes);
            }
        }

        return buffList;
    }

    private static List<StatEffect> sortEffectsList(Dictionary<StatEffect, int> updateEffectsList)
    {
        Dictionary<BuffStat, List<KeyValuePair<StatEffect, int>>> buffEffects = new();

        foreach (var p in updateEffectsList)
        {
            StatEffect mse = p.Key;

            foreach (var statup in mse.getStatups())
            {
                BuffStat stat = statup.Key;

                var statBuffs = buffEffects.GetValueOrDefault(stat);
                if (statBuffs == null)
                {
                    statBuffs = new();
                    buffEffects.AddOrUpdate(stat, statBuffs);
                }

                statBuffs.Add(new(mse, statup.Value));
            }
        }

        foreach (var statBuffs in buffEffects)
        {
            statBuffs.Value.Sort((o1, o2) => o2.Value.CompareTo(o1.Value));
        }

        return topologicalSortEffects(buffEffects);
    }

    private List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> propagatePriorityBuffEffectUpdates(HashSet<BuffStat> retrievedStats)
    {
        List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> priorityUpdateEffects = new();
        Dictionary<BuffStatValueHolder, StatEffect> yokeStats = new();

        // priority buffsources: override buffstats for the client to perceive those as "currently buffed"
        HashSet<BuffStatValueHolder> mbsvhList = new();
        foreach (BuffStatValueHolder mbsvh in getAllStatups())
        {
            mbsvhList.Add(mbsvh);
        }

        foreach (BuffStatValueHolder mbsvh in mbsvhList)
        {
            StatEffect mse = mbsvh.effect;
            int buffSourceId = mse.getBuffSourceId();
            if (isPriorityBuffSourceid(buffSourceId) && !hasActiveBuff(buffSourceId))
            {
                foreach (var ps in mse.getStatups())
                {
                    BuffStat mbs = ps.Key;
                    if (retrievedStats.Contains(mbs))
                    {
                        BuffStatValueHolder mbsvhe = effects.GetValueOrDefault(mbs)!;

                        // this shouldn't even be null...
                        //if (mbsvh != null) {
                        yokeStats.AddOrUpdate(mbsvh, mbsvhe.effect);
                        //}
                    }
                }
            }
        }

        foreach (var e in yokeStats)
        {
            BuffStatValueHolder mbsvhPriority = e.Key;
            StatEffect mseActive = e.Value;

            priorityUpdateEffects.Add(new(mseActive.getBuffSourceId(), new(mbsvhPriority.effect, mbsvhPriority.startTime)));
        }

        return priorityUpdateEffects;
    }

    private void propagateBuffEffectUpdates(Dictionary<int, KeyValuePair<StatEffect, long>> retrievedEffects, HashSet<BuffStat> retrievedStats, HashSet<BuffStat> removedStats)
    {
        cancelInactiveBuffStats(retrievedStats, removedStats);
        if (retrievedStats.Count == 0)
        {
            return;
        }

        Dictionary<BuffStat, KeyValuePair<int, StatEffect>?> maxBuffValue = new();
        foreach (BuffStat mbs in retrievedStats)
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
            if (mbsvh != null)
            {
                retrievedEffects.AddOrUpdate(mbsvh.effect.getBuffSourceId(), new(mbsvh.effect, mbsvh.startTime));
            }

            maxBuffValue.AddOrUpdate(mbs, new(int.MinValue, null));
        }

        Dictionary<StatEffect, int> updateEffects = new();

        List<StatEffect> recalcMseList = new();
        foreach (var re in retrievedEffects)
        {
            recalcMseList.Add(re.Value.Key);
        }

        bool mageJob = this.getJobStyle() == Job.MAGICIAN;
        do
        {
            List<StatEffect> mseList = recalcMseList;
            recalcMseList = new();

            foreach (StatEffect mse in mseList)
            {
                int maxEffectiveStatup = int.MinValue;
                foreach (var st in mse.getStatups())
                {
                    BuffStat mbs = st.Key;

                    bool relevantStatup = true;
                    if (mbs == BuffStat.WATK)
                    {  // not relevant for mages
                        if (mageJob)
                        {
                            relevantStatup = false;
                        }
                    }
                    else if (mbs == BuffStat.MATK)
                    { // not relevant for non-mages
                        if (!mageJob)
                        {
                            relevantStatup = false;
                        }
                    }

                    var mbv = maxBuffValue.GetValueOrDefault(mbs);
                    if (mbv == null)
                    {
                        continue;
                    }

                    if (mbv.Value.Key < st.Value)
                    {
                        StatEffect msbe = mbv.Value.Value;
                        if (msbe != null)
                        {
                            recalcMseList.Add(msbe);
                        }

                        maxBuffValue.AddOrUpdate(mbs, new(st.Value, mse));

                        if (relevantStatup)
                        {
                            if (maxEffectiveStatup < st.Value)
                            {
                                maxEffectiveStatup = st.Value;
                            }
                        }
                    }
                }

                updateEffects.AddOrUpdate(mse, maxEffectiveStatup);
            }
        } while (recalcMseList.Count > 0);

        List<StatEffect> updateEffectsList = sortEffectsList(updateEffects);

        List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> toUpdateEffects = new();
        foreach (StatEffect mse in updateEffectsList)
        {
            toUpdateEffects.Add(new(mse.getBuffSourceId(), retrievedEffects.GetValueOrDefault(mse.getBuffSourceId())));
        }

        List<KeyValuePair<BuffStat, int>> activeStatups = new();
        foreach (var lmse in toUpdateEffects)
        {
            KeyValuePair<StatEffect, long> msel = lmse.Value;

            foreach (var statup in getActiveStatupsFromSourceid(lmse.Key))
            {
                activeStatups.Add(statup);
            }

            msel.Key.updateBuffEffect(this, activeStatups, msel.Value);
            activeStatups.Clear();
        }

        List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> priorityEffects = propagatePriorityBuffEffectUpdates(retrievedStats);
        foreach (var lmse in priorityEffects)
        {
            var msel = lmse.Value;

            foreach (var statup in getActiveStatupsFromSourceid(lmse.Key))
            {
                activeStatups.Add(statup);
            }

            msel.Key.updateBuffEffect(this, activeStatups, msel.Value);
            activeStatups.Clear();
        }

        if (this.isRidingBattleship())
        {
            List<KeyValuePair<BuffStat, int>> statups = new(1);
            statups.Add(new(BuffStat.MONSTER_RIDING, 0));
            this.sendPacket(PacketCreator.giveBuff(ItemId.BATTLESHIP, 5221006, statups));
            this.announceBattleshipHp();
        }
    }

    private static BuffStat? getSingletonStatupFromEffect(StatEffect mse)
    {
        foreach (var mbs in mse.getStatups())
        {
            if (isSingletonStatup(mbs.Key))
            {
                return mbs.Key;
            }
        }

        return null;
    }

    private static bool isSingletonStatup(BuffStat mbs)
    {
        var list = new BuffStat[]
        {
            BuffStat.COUPON_EXP1,
            BuffStat.COUPON_EXP2,
            BuffStat.COUPON_EXP3,
            BuffStat.COUPON_EXP4,
            BuffStat.COUPON_DRP1,
            BuffStat.COUPON_DRP2,
            BuffStat.COUPON_DRP3,
            BuffStat.MESO_UP_BY_ITEM,
            BuffStat.ITEM_UP_BY_ITEM,
            BuffStat.RESPECT_PIMMUNE,
            BuffStat.RESPECT_MIMMUNE,
            BuffStat.DEFENSE_ATT,
            BuffStat.DEFENSE_STATE,
            BuffStat.WATK,
            BuffStat.WDEF,
            BuffStat.MATK,
            BuffStat.MDEF,
            BuffStat.ACC,
            BuffStat.AVOID,
            BuffStat.SPEED,
            BuffStat.JUMP
        };
        return !list.Contains(mbs);
    }

    private static bool isPriorityBuffSourceid(int sourceid)
    {
        switch (sourceid)
        {
            case -ItemId.ROSE_SCENT:
            case -ItemId.FREESIA_SCENT:
            case -ItemId.LAVENDER_SCENT:
                return true;

            default:
                return false;
        }
    }

    private void addItemEffectHolderCount(BuffStat stat)
    {
        var val = buffEffectsCount.GetValueOrDefault(stat) + 1;

        buffEffectsCount.AddOrUpdate(stat, (sbyte)val);
    }

    public void registerEffect(StatEffect effect, long starttime, long expirationtime, bool isSilent)
    {
        if (effect.isDragonBlood())
        {
            prepareDragonBlood(effect);
        }
        else if (effect.isBerserk())
        {
            checkBerserk(isHidden());
        }
        else if (effect.isBeholder())
        {
            int beholder = DarkKnight.BEHOLDER;
            if (beholderHealingSchedule != null)
            {
                beholderHealingSchedule.cancel(false);
            }
            if (beholderBuffSchedule != null)
            {
                beholderBuffSchedule.cancel(false);
            }
            Skill bHealing = SkillFactory.GetSkillTrust(DarkKnight.AURA_OF_BEHOLDER);
            int bHealingLvl = getSkillLevel(bHealing);
            if (bHealingLvl > 0)
            {
                StatEffect healEffect = bHealing.getEffect(bHealingLvl);
                var healInterval = TimeSpan.FromSeconds(healEffect.getX());
                beholderHealingSchedule = TimerManager.getInstance().register(() =>
                {
                    if (awayFromWorld.Get())
                    {
                        return;
                    }

                    addHP(healEffect.getHp());
                    sendPacket(PacketCreator.showOwnBuffEffect(beholder, 2));
                    getMap().broadcastMessage(this, PacketCreator.summonSkill(getId(), beholder, 5), true);
                    getMap().broadcastMessage(this, PacketCreator.showOwnBuffEffect(beholder, 2), false);

                }, healInterval, healInterval);
            }
            Skill bBuff = SkillFactory.GetSkillTrust(DarkKnight.HEX_OF_BEHOLDER);
            if (getSkillLevel(bBuff) > 0)
            {
                StatEffect buffEffect = bBuff.getEffect(getSkillLevel(bBuff));
                var buffInterval = TimeSpan.FromSeconds(buffEffect.getX());
                beholderBuffSchedule = TimerManager.getInstance().register(() =>
                {
                    if (awayFromWorld.Get())
                    {
                        return;
                    }

                    buffEffect.applyTo(this);
                    sendPacket(PacketCreator.showOwnBuffEffect(beholder, 2));
                    getMap().broadcastMessage(this, PacketCreator.summonSkill(getId(), beholder, (int)(Randomizer.nextDouble() * 3) + 6), true);
                    getMap().broadcastMessage(this, PacketCreator.showBuffEffect(getId(), beholder, 2), false);

                }, buffInterval, buffInterval);
            }
        }
        else if (effect.isRecovery())
        {
            int healInterval = (YamlConfig.config.server.USE_ULTRA_RECOVERY) ? 2000 : 5000;
            byte heal = (byte)effect.getX();

            chLock.EnterReadLock();
            try
            {
                if (recoveryTask != null)
                {
                    recoveryTask.cancel(false);
                }

                recoveryTask = TimerManager.getInstance().register(() =>
                {
                    if (getBuffSource(BuffStat.RECOVERY) == -1)
                    {
                        chLock.EnterReadLock();
                        try
                        {
                            if (recoveryTask != null)
                            {
                                recoveryTask.cancel(false);
                                recoveryTask = null;
                            }
                        }
                        finally
                        {
                            chLock.ExitReadLock();
                        }

                        return;
                    }

                    addHP(heal);
                    sendPacket(PacketCreator.showOwnRecovery(heal));
                    getMap().broadcastMessage(this, PacketCreator.showRecovery(id, heal), false);

                }, healInterval, healInterval);
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }
        else if (effect.getHpRRate() > 0 || effect.getMpRRate() > 0)
        {
            if (effect.getHpRRate() > 0)
            {
                extraHpRec = effect.getHpR();
                extraRecInterval = effect.getHpRRate();
            }

            if (effect.getMpRRate() > 0)
            {
                extraMpRec = effect.getMpR();
                extraRecInterval = effect.getMpRRate();
            }

            chLock.EnterReadLock();
            try
            {
                stopExtraTask();
                startExtraTask(extraHpRec, extraMpRec, extraRecInterval);   // HP & MP sharing the same task holder
            }
            finally
            {
                chLock.ExitReadLock();
            }

        }
        else if (effect.isMapChair())
        {
            startChairTask();
        }

        Monitor.Enter(prtLock);
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            int sourceid = effect.getBuffSourceId();
            Dictionary<BuffStat, BuffStatValueHolder> toDeploy;
            Dictionary<BuffStat, BuffStatValueHolder> appliedStatups = new();

            foreach (var ps in effect.getStatups())
            {
                appliedStatups.AddOrUpdate(ps.Key, new BuffStatValueHolder(effect, starttime, ps.Value));
            }

            bool active = effect.isActive(this);
            if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                toDeploy = new();
                Dictionary<int, KeyValuePair<StatEffect, long>> retrievedEffects = new();
                HashSet<BuffStat> retrievedStats = new();
                foreach (var statup in appliedStatups)
                {
                    BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(statup.Key);
                    BuffStatValueHolder statMbsvh = statup.Value;

                    if (active)
                    {
                        if (mbsvh == null || mbsvh.value < statMbsvh.value || (mbsvh.value == statMbsvh.value && mbsvh.effect.getStatups().Count <= statMbsvh.effect.getStatups().Count))
                        {
                            toDeploy.AddOrUpdate(statup.Key, statMbsvh);
                        }
                        else
                        {
                            if (!isSingletonStatup(statup.Key))
                            {
                                foreach (var mbs in mbsvh.effect.getStatups())
                                {
                                    retrievedStats.Add(mbs.Key);
                                }
                            }
                        }
                    }

                    addItemEffectHolderCount(statup.Key);
                }

                // should also propagate update from buffs shared with priority sourceids
                var updated = appliedStatups.Keys;
                foreach (BuffStatValueHolder mbsvh in this.getAllStatups())
                {
                    if (isPriorityBuffSourceid(mbsvh.effect.getBuffSourceId()))
                    {
                        foreach (var p in mbsvh.effect.getStatups())
                        {
                            if (updated.Contains(p.Key))
                            {
                                retrievedStats.Add(p.Key);
                            }
                        }
                    }
                }

                if (!isSilent)
                {
                    addItemEffectHolder(sourceid, expirationtime, appliedStatups);
                    foreach (var statup in toDeploy)
                    {
                        effects.AddOrUpdate(statup.Key, statup.Value);
                    }

                    if (active)
                    {
                        retrievedEffects.AddOrUpdate(sourceid, new(effect, starttime));
                    }

                    propagateBuffEffectUpdates(retrievedEffects, retrievedStats, new());
                }
            }
            else
            {
                foreach (var statup in appliedStatups)
                {
                    addItemEffectHolderCount(statup.Key);
                }

                toDeploy = (active ? appliedStatups : new());
            }

            addItemEffectHolder(sourceid, expirationtime, appliedStatups);
            foreach (var statup in toDeploy)
            {
                effects.AddOrUpdate(statup.Key, statup.Value);
            }
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
            Monitor.Exit(prtLock);
        }

        updateLocalStats();
    }

    private static int getJobMapChair(Job job)
    {
        switch (job.getId() / 1000)
        {
            case 0:
                return Beginner.MAP_CHAIR;
            case 1:
                return Noblesse.MAP_CHAIR;
            default:
                return Legend.MAP_CHAIR;
        }
    }

    public bool unregisterChairBuff()
    {
        if (!YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
        {
            return false;
        }

        int skillId = getJobMapChair(job);
        int skillLv = getSkillLevel(skillId);
        if (skillLv > 0)
        {
            StatEffect mapChairSkill = SkillFactory.getSkill(skillId)!.getEffect(skillLv);
            return cancelEffect(mapChairSkill, false, -1);
        }

        return false;
    }

    public bool registerChairBuff()
    {
        if (!YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
        {
            return false;
        }

        int skillId = getJobMapChair(job);
        int skillLv = getSkillLevel(skillId);
        if (skillLv > 0)
        {
            StatEffect mapChairSkill = SkillFactory.getSkill(skillId)!.getEffect(skillLv);
            mapChairSkill.applyTo(this);
            return true;
        }

        return false;
    }

    public int getChair()
    {
        return chair.get();
    }

    public string getChalkboard()
    {
        return this.chalktext;
    }

    public Client getClient()
    {
        return client;
    }

    public AbstractPlayerInteraction getAbstractPlayerInteraction()
    {
        return client.getAbstractPlayerInteraction();
    }

    private List<QuestStatus> getQuests()
    {
        lock (quests)
        {
            return new(quests.Values);
        }
    }

    public List<QuestStatus> getCompletedQuests()
    {
        return getQuests().Where(x => x.getStatus() == QuestStatus.Status.COMPLETED).ToList();
    }

    public List<Ring> getCrushRings()
    {
        crushRings.Sort();
        return crushRings;
    }

    public int getCurrentCI()
    {
        return ci;
    }

    public int getCurrentPage()
    {
        return currentPage;
    }

    public int getCurrentTab()
    {
        return currentTab;
    }

    public int getCurrentType()
    {
        return currentType;
    }

    public int getDojoEnergy()
    {
        return dojoEnergy;
    }

    public int getDojoPoints()
    {
        return dojoPoints;
    }

    public int getDojoStage()
    {
        return dojoStage;
    }

    public ICollection<Door> getDoors()
    {
        Monitor.Enter(prtLock);
        try
        {
            return (party != null ? new List<Door>(party.getDoors().Values.ToList()) : (pdoor != null ? Collections.singletonList(pdoor) : new()));
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public Door? getPlayerDoor()
    {
        Monitor.Enter(prtLock);
        try
        {
            return pdoor;
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public Door? getMainTownDoor()
    {
        foreach (Door door in getDoors())
        {
            if (door.getTownPortal().getId() == 0x80)
            {
                return door;
            }
        }

        return null;
    }

    public void applyPartyDoor(Door door, bool partyUpdate)
    {
        Party? chrParty;
        Monitor.Enter(prtLock);
        try
        {
            if (!partyUpdate)
            {
                pdoor = door;
            }

            chrParty = getParty();
            if (chrParty != null)
            {
                chrParty.addDoor(id, door);
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        silentPartyUpdateInternal(chrParty);
    }

    public Door? removePartyDoor(bool partyUpdate)
    {
        Door? ret = null;
        Party? chrParty;

        Monitor.Enter(prtLock);
        try
        {
            chrParty = getParty();
            if (chrParty != null)
            {
                chrParty.removeDoor(id);
            }

            if (!partyUpdate)
            {
                ret = pdoor;
                pdoor = null;
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        silentPartyUpdateInternal(chrParty);
        return ret;
    }

    private void removePartyDoor(Party formerParty)
    {    // player is no longer registered at this party
        formerParty.removeDoor(id);
    }

    public int getEnergyBar()
    {
        return energybar;
    }

    public EventInstanceManager? getEventInstance()
    {
        Monitor.Enter(evtLock);
        try
        {
            return eventInstance;
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    public Marriage? getMarriageInstance()
    {
        return getEventInstance() as Marriage;
    }

    public void resetExcluded(int petId)
    {
        chLock.EnterReadLock();
        try
        {
            HashSet<int>? petExclude = excluded.GetValueOrDefault(petId);

            if (petExclude != null)
            {
                petExclude.Clear();
            }
            else
            {
                excluded.AddOrUpdate(petId, new());
            }
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void addExcluded(int petId, int x)
    {
        chLock.EnterReadLock();
        try
        {
            excluded.GetValueOrDefault(petId)?.Add(x);
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public void commitExcludedItems()
    {
        Dictionary<int, HashSet<int>> petExcluded = this.getExcluded();

        chLock.EnterReadLock();
        try
        {
            excludedItems.Clear();
        }
        finally
        {
            chLock.ExitReadLock();
        }

        foreach (var pe in petExcluded)
        {
            sbyte petIndex = this.getPetIndex(pe.Key);
            if (petIndex < 0)
            {
                continue;
            }

            HashSet<int> exclItems = pe.Value;
            if (exclItems.Count > 0)
            {
                sendPacket(PacketCreator.loadExceptionList(this.getId(), pe.Key, petIndex, new(exclItems)));

                chLock.EnterReadLock();
                try
                {
                    foreach (int itemid in exclItems)
                    {
                        excludedItems.Add(itemid);
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                }
            }
        }
    }

    public void exportExcludedItems(Client c)
    {
        Dictionary<int, HashSet<int>> petExcluded = this.getExcluded();
        foreach (var pe in petExcluded)
        {
            sbyte petIndex = this.getPetIndex(pe.Key);
            if (petIndex < 0)
            {
                continue;
            }

            HashSet<int> exclItems = pe.Value;
            if (exclItems.Count > 0)
            {
                c.sendPacket(PacketCreator.loadExceptionList(this.getId(), pe.Key, petIndex, new(exclItems)));
            }
        }
    }

    public Dictionary<int, HashSet<int>> getExcluded()
    {
        chLock.EnterReadLock();
        try
        {
            return excluded;
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public HashSet<int> getExcludedItems()
    {
        chLock.EnterReadLock();
        try
        {
            return excludedItems.ToHashSet();
        }
        finally
        {
            chLock.ExitReadLock();
        }
    }

    public int getExp()
    {
        return exp.get();
    }

    public int getGachaExp()
    {
        return gachaexp.get();
    }

    public bool hasNoviceExpRate()
    {
        return YamlConfig.config.server.USE_ENFORCE_NOVICE_EXPRATE && isBeginnerJob() && level < 11;
    }

    public int getExpRate()
    {
        if (hasNoviceExpRate())
        {   // base exp rate 1x for early levels idea thanks to Vcoc
            return 1;
        }

        return expRate;
    }

    public int getCouponExpRate()
    {
        return expCoupon;
    }

    public int getRawExpRate()
    {
        return expRate / (expCoupon * getWorldServer().getExpRate());
    }

    public int getDropRate()
    {
        return dropRate;
    }

    public int getCouponDropRate()
    {
        return dropCoupon;
    }

    public int getRawDropRate()
    {
        return dropRate / (dropCoupon * getWorldServer().getDropRate());
    }

    public int getBossDropRate()
    {
        World w = getWorldServer();
        return (dropRate / w.getDropRate()) * w.getBossDropRate();
    }

    public int getMesoRate()
    {
        return mesoRate;
    }

    public int getCouponMesoRate()
    {
        return mesoCoupon;
    }

    public int getRawMesoRate()
    {
        return mesoRate / (mesoCoupon * getWorldServer().getMesoRate());
    }

    public int getQuestExpRate()
    {
        if (hasNoviceExpRate())
        {
            return 1;
        }

        World w = getWorldServer();
        return w.getExpRate() * w.getQuestRate();
    }

    public int getQuestMesoRate()
    {
        World w = getWorldServer();
        return w.getMesoRate() * w.getQuestRate();
    }

    public float getCardRate(int itemid)
    {
        float rate = 100.0f;

        if (itemid == 0)
        {
            StatEffect? mseMeso = getBuffEffect(BuffStat.MESO_UP_BY_ITEM);
            if (mseMeso != null)
            {
                rate += mseMeso.getCardRate(mapid, itemid);
            }
        }
        else
        {
            StatEffect? mseItem = getBuffEffect(BuffStat.ITEM_UP_BY_ITEM);
            if (mseItem != null)
            {
                rate += mseItem.getCardRate(mapid, itemid);
            }
        }

        return rate / 100;
    }

    public int getFace()
    {
        return face;
    }

    public int getFame()
    {
        return fame;
    }

    public Family? getFamily()
    {
        if (familyEntry != null)
        {
            return familyEntry.getFamily();
        }
        else
        {
            return null;
        }
    }

    public FamilyEntry? getFamilyEntry()
    {
        return familyEntry;
    }

    public void setFamilyEntry(FamilyEntry? entry)
    {
        if (entry != null)
        {
            setFamilyId(entry.getFamily().getID());
        }
        this.familyEntry = entry;
    }

    public int getFamilyId()
    {
        return familyId;
    }

    public bool getFinishedDojoTutorial()
    {
        return finishedDojoTutorial;
    }

    public void setUsedStorage()
    {
        usedStorage = true;
    }

    public List<Ring> getFriendshipRings()
    {
        friendshipRings.Sort();
        return friendshipRings;
    }

    public int getGender()
    {
        return gender;
    }

    public bool isMale()
    {
        return getGender() == 0;
    }

    public Guild? getGuild()
    {
        try
        {
            return Server.getInstance().getGuild(getGuildId(), getWorld(), this);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
            return null;
        }
    }

    public Alliance? getAlliance()
    {
        if (mgc != null)
        {
            try
            {
                return Server.getInstance().getAlliance(getGuild()!.getAllianceId());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }

        return null;
    }

    public int getGuildId()
    {
        return guildid;
    }

    public int getGuildRank()
    {
        return guildRank;
    }

    public int getHair()
    {
        return hair;
    }

    public HiredMerchant? getHiredMerchant()
    {
        return hiredMerchant;
    }

    public int getId()
    {
        return id;
    }

    public static int getAccountIdByName(string name)
    {
        using DBContext dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.AccountId }).FirstOrDefault()?.AccountId ?? -1;
    }

    public static int getIdByName(string name)
    {
        using DBContext dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.Name == name).Select(x => new { x.Id }).FirstOrDefault()?.Id ?? -1;
    }

    public static string getNameById(int id)
    {
        using DBContext dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault()!;
    }

    public int getInitialSpawnpoint()
    {
        return initialSpawnPoint;
    }

    public Inventory getInventory(InventoryType type)
    {
        return inventory[type.ordinal()];
    }

    public int getItemEffect()
    {
        return itemEffect;
    }

    public bool haveItemWithId(int itemid, bool checkEquipped)
    {
        return (inventory[ItemConstants.getInventoryType(itemid).ordinal()].findById(itemid) != null)
                || (checkEquipped && inventory[InventoryType.EQUIPPED.ordinal()].findById(itemid) != null);
    }

    public bool haveItemEquipped(int itemid)
    {
        return (inventory[InventoryType.EQUIPPED.ordinal()].findById(itemid) != null);
    }

    public bool haveWeddingRing()
    {
        int[] rings = { ItemId.WEDDING_RING_STAR, ItemId.WEDDING_RING_MOONSTONE, ItemId.WEDDING_RING_GOLDEN, ItemId.WEDDING_RING_SILVER };

        foreach (int ringid in rings)
        {
            if (haveItemWithId(ringid, true))
            {
                return true;
            }
        }

        return false;
    }

    public int getItemQuantity(int itemid, bool checkEquipped)
    {
        int count = inventory[ItemConstants.getInventoryType(itemid).ordinal()].countById(itemid);
        if (checkEquipped)
        {
            count += inventory[InventoryType.EQUIPPED.ordinal()].countById(itemid);
        }
        return count;
    }

    public int getCleanItemQuantity(int itemid, bool checkEquipped)
    {
        int count = inventory[ItemConstants.getInventoryType(itemid).ordinal()].countNotOwnedById(itemid);
        if (checkEquipped)
        {
            count += inventory[InventoryType.EQUIPPED.ordinal()].countNotOwnedById(itemid);
        }
        return count;
    }

    public Job getJob()
    {
        return job;
    }

    public int getJobRank()
    {
        return jobRank;
    }

    public int getJobRankMove()
    {
        return jobRankMove;
    }

    public int getJobType()
    {
        return job.getId() / 1000;
    }

    public Dictionary<int, KeyBinding> getKeymap()
    {
        return keymap;
    }

    public long getLastHealed()
    {
        return lastHealed;
    }

    public long getLastUsedCashItem()
    {
        return lastUsedCashItem;
    }

    public int getLevel()
    {
        return level;
    }

    public int getFh()
    {
        Point pos = this.getPosition();
        pos.Y -= 6;

        if (map.getFootholds().findBelow(pos) == null)
        {
            return 0;
        }
        else
        {
            return map.getFootholds().findBelow(pos).getY1();
        }
    }

    public int getMapId()
    {
        if (map != null)
        {
            return map.getId();
        }
        return mapid;
    }

    public Ring? getMarriageRing()
    {
        return partnerId > 0 ? marriageRing : null;
    }

    public int getMasterLevel(int skill)
    {
        var skillData = SkillFactory.getSkill(skill);
        if (skillData == null)
        {
            return 0;
        }
        return getMasterLevel(skill);
    }

    public int getMasterLevel(Skill skill)
    {
        var characterSkill = skills.GetValueOrDefault(skill);
        if (characterSkill == null)
        {
            return 0;
        }
        return characterSkill.masterlevel;
    }

    public int getTotalStr()
    {
        return localstr;
    }

    public int getTotalDex()
    {
        return localdex;
    }

    public int getTotalInt()
    {
        return localint_;
    }

    public int getTotalLuk()
    {
        return localluk;
    }

    public int getTotalMagic()
    {
        return localmagic;
    }

    public int getTotalWatk()
    {
        return localwatk;
    }

    public int getMaxClassLevel()
    {
        return isCygnus() ? 120 : 200;
    }

    public int getMaxLevel()
    {
        if (!YamlConfig.config.server.USE_ENFORCE_JOB_LEVEL_RANGE || isGmJob())
        {
            return getMaxClassLevel();
        }

        return GameConstants.getJobMaxLevel(job);
    }

    public int getMeso()
    {
        return meso.get();
    }

    public int getMerchantMeso()
    {
        return merchantmeso;
    }

    public int getMerchantNetMeso()
    {
        int elapsedDays = 0;

        using var dbContext = new DBContext();
        var dbModel = dbContext.Fredstorages.Where(x => x.Cid == getId()).Select(x => new { x.Timestamp }).FirstOrDefault();
        if (dbModel != null)
            elapsedDays = FredrickProcessor.timestampElapsedDays(dbModel.Timestamp, DateTimeOffset.Now);

        if (elapsedDays > 100)
        {
            elapsedDays = 100;
        }

        long netMeso = merchantmeso; // negative mesos issues found thanks to Flash, Vcoc
        netMeso = (netMeso * (100 - elapsedDays)) / 100;
        return (int)netMeso;
    }

    public int getMesosTraded()
    {
        return mesosTraded;
    }

    public int getMessengerPosition()
    {
        return messengerposition;
    }

    public GuildCharacter? getMGC()
    {
        return mgc;
    }

    public void setMGC(GuildCharacter mgc)
    {
        this.mgc = mgc;
    }

    public PartyCharacter getMPC()
    {
        if (mpc == null)
        {
            mpc = new PartyCharacter(this);
        }
        return mpc;
    }

    public void setMPC(PartyCharacter? mpc)
    {
        this.mpc = mpc;
    }

    public int getTargetHpBarHash()
    {
        return this.targetHpBarHash;
    }

    public void setTargetHpBarHash(int mobHash)
    {
        this.targetHpBarHash = mobHash;
    }

    public long getTargetHpBarTime()
    {
        return this.targetHpBarTime;
    }

    public void setTargetHpBarTime(long timeNow)
    {
        this.targetHpBarTime = timeNow;
    }

    public void setPlayerAggro(int mobHash)
    {
        setTargetHpBarHash(mobHash);
        setTargetHpBarTime(DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public void resetPlayerAggro()
    {
        if (getWorldServer().unregisterDisabledServerMessage(id))
        {
            client.announceServerMessage();
        }

        setTargetHpBarHash(0);
        setTargetHpBarTime(0);
    }

    public MiniGame getMiniGame()
    {
        return miniGame;
    }

    public int getMiniGamePoints(MiniGameResult type, bool omok)
    {
        if (omok)
        {
            switch (type)
            {
                case MiniGameResult.WIN:
                    return omokwins;
                case MiniGameResult.LOSS:
                    return omoklosses;
                default:
                    return omokties;
            }
        }
        else
        {
            switch (type)
            {
                case MiniGameResult.WIN:
                    return matchcardwins;
                case MiniGameResult.LOSS:
                    return matchcardlosses;
                default:
                    return matchcardties;
            }
        }
    }

    public MonsterBook getMonsterBook()
    {
        return monsterbook;
    }

    public int getMonsterBookCover()
    {
        return bookCover;
    }

    public Mount? getMount()
    {
        return maplemount;
    }

    public Messenger? getMessenger()
    {
        return messenger;
    }

    public string getName()
    {
        return name;
    }

    public int getNextEmptyPetIndex()
    {
        Monitor.Enter(petLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] == null)
                {
                    return i;
                }
            }
            return 3;
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public int getNoPets()
    {
        Monitor.Enter(petLock);
        try
        {
            int ret = 0;
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    ret++;
                }
            }
            return ret;
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public Party? getParty()
    {
        Monitor.Enter(prtLock);
        try
        {
            return party;
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public int getPartyId()
    {
        Monitor.Enter(prtLock);
        try
        {
            return (party != null ? party.getId() : -1);
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public List<Character> getPartyMembersOnline()
    {
        List<Character> list = new();

        Monitor.Enter(prtLock);
        try
        {
            if (party != null)
            {
                foreach (PartyCharacter mpc in party.getMembers())
                {
                    Character mc = mpc.getPlayer();
                    if (mc != null)
                    {
                        list.Add(mc);
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        return list;
    }

    public List<Character> getPartyMembersOnSameMap()
    {
        List<Character> list = new();
        int thisMapHash = this.getMap().GetHashCode();

        Monitor.Enter(prtLock);
        try
        {
            if (party != null)
            {
                foreach (PartyCharacter mpc in party.getMembers())
                {
                    Character chr = mpc.getPlayer();
                    if (chr != null)
                    {
                        MapleMap chrMap = chr.getMap();
                        // 用hashcode判断地图是否相同？ -- 同一频道、同一mapid
                        if (chrMap != null && chrMap.GetHashCode() == thisMapHash && chr.isLoggedinWorld())
                        {
                            list.Add(chr);
                        }
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        return list;
    }

    public bool isPartyMember(Character chr)
    {
        return isPartyMember(chr.getId());
    }

    public bool isPartyMember(int cid)
    {
        Monitor.Enter(prtLock);
        try
        {
            if (party != null)
            {
                return party.getMemberById(cid) != null;
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        return false;
    }

    public PlayerShop? getPlayerShop()
    {
        return playerShop;
    }

    public RockPaperScissor? getRPS()
    { // thanks inhyuk for suggesting RPS addition
        return rps;
    }

    public void setGMLevel(int level)
    {
        this._gmLevel = Math.Min(level, 6);
        this._gmLevel = Math.Max(level, 0);

        whiteChat = _gmLevel >= 4;   // thanks ozanrijen for suggesting default white chat
    }

    public void closePartySearchInteractions()
    {
        this.getWorldServer().getPartySearchCoordinator().unregisterPartyLeader(this);
        if (canRecvPartySearchInvite)
        {
            this.getWorldServer().getPartySearchCoordinator().detachPlayer(this);
        }
    }

    public void closePlayerInteractions()
    {
        closeNpcShop();
        closeTrade();
        closePlayerShop();
        closeMiniGame(true);
        closeRPS();
        closeHiredMerchant(false);
        closePlayerMessenger();

        client.closePlayerScriptInteractions();
        resetPlayerAggro();
    }

    public void closeNpcShop()
    {
        setShop(null);
    }

    public void closeTrade()
    {
        Trade.cancelTrade(this, TradeResult.PARTNER_CANCEL);
    }

    public void closePlayerShop()
    {
        PlayerShop? mps = this.getPlayerShop();
        if (mps == null)
        {
            return;
        }

        if (mps.isOwner(this))
        {
            mps.setOpen(false);
            getWorldServer().unregisterPlayerShop(mps);

            foreach (PlayerShopItem mpsi in mps.getItems())
            {
                if (mpsi.getBundles() >= 2)
                {
                    Item iItem = mpsi.getItem().copy();
                    iItem.setQuantity((short)(mpsi.getBundles() * iItem.getQuantity()));
                    InventoryManipulator.addFromDrop(this.getClient(), iItem, false);
                }
                else if (mpsi.isExist())
                {
                    InventoryManipulator.addFromDrop(this.getClient(), mpsi.getItem(), true);
                }
            }
            mps.closeShop();
        }
        else
        {
            mps.removeVisitor(this);
        }
        this.setPlayerShop(null);
    }

    public void closeMiniGame(bool forceClose)
    {
        MiniGame game = this.getMiniGame();
        if (game == null)
        {
            return;
        }

        if (game.isOwner(this))
        {
            game.closeRoom(forceClose);
        }
        else
        {
            game.removeVisitor(forceClose, this);
        }
    }

    public void closeHiredMerchant(bool closeMerchant)
    {
        HiredMerchant? merchant = this.getHiredMerchant();
        if (merchant == null)
        {
            return;
        }

        if (closeMerchant)
        {
            if (merchant.isOwner(this) && merchant.getItems().Count == 0)
            {
                merchant.forceClose();
            }
            else
            {
                merchant.removeVisitor(this);
                this.setHiredMerchant(null);
            }
        }
        else
        {
            if (merchant.isOwner(this))
            {
                merchant.setOpen(true);
            }
            else
            {
                merchant.removeVisitor(this);
            }
            try
            {
                merchant.saveItems(false);
            }
            catch (Exception e)
            {
                log.Error(e, "Error while saving {name}'s Hired Merchant items.", name);
            }
        }
    }

    public void closePlayerMessenger()
    {
        Messenger? m = this.getMessenger();
        if (m == null)
        {
            return;
        }

        World w = getWorldServer();
        MessengerCharacter messengerplayer = new MessengerCharacter(this, this.getMessengerPosition());

        w.leaveMessenger(m.getId(), messengerplayer);
        this.setMessenger(null);
        this.setMessengerPosition(4);
    }

    public Pet?[] getPets()
    {
        Monitor.Enter(petLock);
        try
        {
            return Arrays.copyOf(pets, pets.Length);
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public Pet? getPet(int index)
    {
        if (index < 0)
        {
            return null;
        }

        Monitor.Enter(petLock);
        try
        {
            return pets[index];
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public sbyte getPetIndex(int petId)
    {
        Monitor.Enter(petLock);
        try
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == petId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public sbyte getPetIndex(Pet pet)
    {
        Monitor.Enter(petLock);
        try
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == pet.getUniqueId())
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public int getPossibleReports()
    {
        return possibleReports;
    }

    public byte getQuestStatus(int quest)
    {
        lock (quests)
        {
            QuestStatus? mqs = quests.GetValueOrDefault((short)quest);
            if (mqs != null)
            {
                return (byte)mqs.getStatus();
            }
            else
            {
                return 0;
            }
        }
    }

    public QuestStatus getQuest(int quest)
    {
        return getQuest(Quest.getInstance(quest));
    }

    public QuestStatus getQuest(Quest quest)
    {
        lock (quests)
        {
            short questid = quest.getId();
            QuestStatus? qs = quests.GetValueOrDefault(questid);
            if (qs == null)
            {
                qs = new QuestStatus(quest, QuestStatus.Status.NOT_STARTED);
                quests.AddOrUpdate(questid, qs);
            }
            return qs;
        }
    }

    //---- \/ \/ \/ \/ \/ \/ \/  NOT TESTED  \/ \/ \/ \/ \/ \/ \/ \/ \/ ----

    public void setQuestAdd(Quest quest, byte status, string customData)
    {
        lock (quests)
        {
            if (!quests.ContainsKey(quest.getId()))
            {
                QuestStatus stat = new QuestStatus(quest, (QuestStatus.Status)(status));
                stat.setCustomData(customData);
                quests.AddOrUpdate(quest.getId(), stat);
            }
        }
    }

    public QuestStatus? getQuestNAdd(Quest quest)
    {
        lock (quests)
        {
            if (!quests.ContainsKey(quest.getId()))
            {
                QuestStatus status = new QuestStatus(quest, QuestStatus.Status.NOT_STARTED);
                quests.AddOrUpdate(quest.getId(), status);
                return status;
            }
            return quests.GetValueOrDefault(quest.getId());
        }
    }

    public QuestStatus? getQuestNoAdd(Quest quest)
    {
        lock (quests)
        {
            return quests.GetValueOrDefault(quest.getId());
        }
    }

    public QuestStatus? getQuestRemove(Quest quest)
    {
        lock (quests)
        {
            if (quests.Remove(quest.getId(), out var d))
                return d;
            return null;
        }
    }

    //---- /\ /\ /\ /\ /\ /\ /\  NOT TESTED  /\ /\ /\ /\ /\ /\ /\ /\ /\ ----

    public bool needQuestItem(int questid, int itemid)
    {
        if (questid <= 0)
        { //For non quest items :3
            return true;
        }

        int amountNeeded, questStatus = this.getQuestStatus(questid);
        if (questStatus == 0)
        {
            amountNeeded = Quest.getInstance(questid).getStartItemAmountNeeded(itemid);
            if (amountNeeded == int.MinValue)
            {
                return false;
            }
        }
        else if (questStatus != 1)
        {
            return false;
        }
        else
        {
            amountNeeded = Quest.getInstance(questid).getCompleteItemAmountNeeded(itemid);
            if (amountNeeded == int.MaxValue)
            {
                return true;
            }
        }

        return getInventory(ItemConstants.getInventoryType(itemid)).countById(itemid) < amountNeeded;
    }

    public int getRank()
    {
        return rank;
    }

    public int getRankMove()
    {
        return rankMove;
    }

    public void clearSavedLocation(SavedLocationType type)
    {
        savedLocations[(int)type] = null;
    }

    public int peekSavedLocation(string type)
    {
        SavedLocation? sl = savedLocations[(int)SavedLocationTypeUtils.fromString(type)];
        if (sl == null)
        {
            return -1;
        }
        return sl.getMapId();
    }

    public int getSavedLocation(string type)
    {
        int m = peekSavedLocation(type);
        clearSavedLocation(SavedLocationTypeUtils.fromString(type));

        return m;
    }

    public string getSearch()
    {
        return search;
    }

    public Shop? getShop()
    {
        return shop;
    }

    public Dictionary<Skill, SkillEntry> getSkills()
    {
        return new Dictionary<Skill, SkillEntry>(skills);
    }

    public int getSkillLevel(int skill)
    {
        var skillData = SkillFactory.getSkill(skill);
        if (skillData == null)
            return 0;
        return getSkillLevel(skillData);
    }

    public sbyte getSkillLevel(Skill skill)
    {
        return skills.GetValueOrDefault(skill)?.skillevel ?? 0;
    }

    public long getSkillExpiration(int skill)
    {
        var skillData = SkillFactory.getSkill(skill);
        if (skillData == null)
            return -1;
        return getSkillExpiration(skillData);
    }

    public long getSkillExpiration(Skill skill)
    {
        return skills.GetValueOrDefault(skill)?.expiration ?? -1;
    }

    public SkinColor getSkinColor()
    {
        return skinColor;
    }

    public int getSlot()
    {
        return slots;
    }

    public List<QuestStatus> getStartedQuests()
    {
        return getQuests().Where(x => x.getStatus() == QuestStatus.Status.STARTED).ToList();
    }

    public StatEffect? getStatForBuff(BuffStat effect)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(effect);
            if (mbsvh == null)
            {
                return null;
            }
            return mbsvh.effect;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public Storage getStorage()
    {
        return storage;
    }

    public ICollection<Summon> getSummonsValues()
    {
        return summons.Values;
    }

    public void clearSummons()
    {
        summons.Clear();
    }

    public Summon? getSummonByKey(int id)
    {
        return summons.GetValueOrDefault(id);
    }

    public bool isSummonsEmpty()
    {
        return summons.Count == 0;
    }

    public bool containsSummon(Summon summon)
    {
        return summons.ContainsValue(summon);
    }

    public Trade? getTrade()
    {
        return trade;
    }

    public int getVanquisherKills()
    {
        return vanquisherKills;
    }

    public int getVanquisherStage()
    {
        return vanquisherStage;
    }

    public MapObject[] getVisibleMapObjects()
    {
        return visibleMapObjects.Keys.ToArray();
    }

    public int getWorld()
    {
        return world;
    }

    public World getWorldServer()
    {
        return Server.getInstance().getWorld(world);
    }

    public void giveCoolDowns(int skillid, long starttime, long length)
    {
        if (skillid == 5221999)
        {
            this.battleshipHp = (int)length;
            addCooldown(skillid, 0, length);
        }
        else
        {
            long timeNow = Server.getInstance().getCurrentTime();
            int time = (int)((length + starttime) - timeNow);
            addCooldown(skillid, timeNow, time);
        }
    }

    public int gmLevel()
    {
        return _gmLevel;
    }

    private void guildUpdate()
    {
        mgc.setLevel(level);
        mgc.setJobId(job.getId());

        if (this.guildid < 1)
        {
            return;
        }

        try
        {
            Server.getInstance().memberLevelJobUpdate(this.mgc);
            //Server.getInstance().getGuild(guildid, world, mgc).gainGP(40);
            int allianceId = getGuild().getAllianceId();
            if (allianceId > 0)
            {
                Server.getInstance().allianceMessage(allianceId, GuildPackets.updateAllianceJobLevel(this), getId(), -1);
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public void handleEnergyChargeGain()
    { // to get here energychargelevel has to be > 0
        Skill energycharge = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.ENERGY_CHARGE) : SkillFactory.GetSkillTrust(Marauder.ENERGY_CHARGE);
        StatEffect ceffect;
        ceffect = energycharge.getEffect(getSkillLevel(energycharge));
        TimerManager tMan = TimerManager.getInstance();
        if (energybar < 10000)
        {
            energybar += 102;
            if (energybar > 10000)
            {
                energybar = 10000;
            }
            List<KeyValuePair<BuffStat, int>> stat = Collections.singletonList(new KeyValuePair<BuffStat, int>(BuffStat.ENERGY_CHARGE, energybar));
            setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
            sendPacket(PacketCreator.giveBuff(energybar, 0, stat));
            sendPacket(PacketCreator.showOwnBuffEffect(energycharge.getId(), 2));
            getMap().broadcastPacket(this, PacketCreator.showBuffEffect(id, energycharge.getId(), 2));
            getMap().broadcastPacket(this, PacketCreator.giveForeignPirateBuff(id, energycharge.getId(),
                    ceffect.getDuration(), stat));
        }
        if (energybar >= 10000 && energybar < 11000)
        {
            energybar = 15000;
            Character chr = this;
            tMan.schedule(() =>
            {
                energybar = 0;
                List<KeyValuePair<BuffStat, int>> stat = Collections.singletonList(new KeyValuePair<BuffStat, int>(BuffStat.ENERGY_CHARGE, energybar));
                setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
                sendPacket(PacketCreator.giveBuff(energybar, 0, stat));
                getMap().broadcastPacket(chr, PacketCreator.cancelForeignFirstDebuff(id, ((long)1) << 50));

            }, ceffect.getDuration());
        }
    }

    public void handleOrbconsume()
    {
        int skillid = isCygnus() ? DawnWarrior.COMBO : Crusader.COMBO;
        Skill combo = SkillFactory.getSkill(skillid);
        List<KeyValuePair<BuffStat, int>> stat = Collections.singletonList(new KeyValuePair<BuffStat, int>(BuffStat.COMBO, 1));
        setBuffedValue(BuffStat.COMBO, 1);
        sendPacket(PacketCreator.giveBuff(skillid,
            combo.getEffect(getSkillLevel(combo)).getDuration() + (int)((getBuffedStarttime(BuffStat.COMBO) ?? 0) - DateTimeOffset.Now.ToUnixTimeMilliseconds()),
            stat));
        getMap().broadcastMessage(this, PacketCreator.giveForeignBuff(getId(), stat), false);
    }

    public bool hasEntered(string script)
    {
        return entered.Values.Any(x => x == script);
    }

    public bool hasEntered(string script, int mapId)
    {
        return entered.GetValueOrDefault(mapId) == script;
    }

    public void hasGivenFame(Character to)
    {
        lastfametime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        lastmonthfameids.Add(to.getId());
        try
        {
            using var dbContext = new DBContext();
            var dbModel = new Famelog()
            {
                Characterid = getId(),
                CharacteridTo = to.getId()
            };
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public bool hasMerchant()
    {
        return _hasMerchant;
    }

    public bool haveItem(int itemid)
    {
        return getItemQuantity(itemid, ItemConstants.isEquipment(itemid)) > 0;
    }

    public bool haveCleanItem(int itemid)
    {
        return getCleanItemQuantity(itemid, ItemConstants.isEquipment(itemid)) > 0;
    }

    public bool hasEmptySlot(int itemId)
    {
        return getInventory(ItemConstants.getInventoryType(itemId)).getNextFreeSlot() > -1;
    }

    public bool hasEmptySlot(sbyte invType)
    {
        return getInventory(InventoryTypeUtils.getByType(invType)).getNextFreeSlot() > -1;
    }

    public void increaseGuildCapacity()
    {
        int cost = Guild.getIncreaseGuildCost(getGuild().getCapacity());

        if (getMeso() < cost)
        {
            dropMessage(1, "You don't have enough mesos.");
            return;
        }

        if (Server.getInstance().increaseGuildCapacity(guildid))
        {
            gainMeso(-cost, true, false, true);
        }
        else
        {
            dropMessage(1, "Your guild already reached the maximum capacity of players.");
        }
    }

    private static string getTimeRemaining(long timeLeft)
    {
        int seconds = (int)Math.Floor((double)timeLeft / 1000) % 60;
        int minutes = (int)Math.Floor((double)timeLeft / 60000) % 60;

        return (minutes > 0 ? (string.Format("%02d", minutes) + " minutes, ") : "") + string.Format("%02d", seconds) + " seconds";
    }

    public bool isBuffFrom(BuffStat stat, Skill skill)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(stat);
            if (mbsvh == null)
            {
                return false;
            }
            return mbsvh.effect.isSkill() && mbsvh.effect.getSourceId() == skill.getId();
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public bool isGmJob()
    {
        int jn = job.getJobNiche();
        return jn >= 8 && jn <= 9;
    }

    public bool isCygnus()
    {
        return getJobType() == 1;
    }

    public bool isAran()
    {
        return job.getId() >= 2000 && job.getId() <= 2112;
    }

    public bool isBeginnerJob()
    {
        return (job.getId() == 0 || job.getId() == 1000 || job.getId() == 2000);
    }

    public bool isGM()
    {
        return _gmLevel > 1;
    }

    public bool isHidden()
    {
        return hidden;
    }

    public bool isMapObjectVisible(MapObject mo)
    {
        return visibleMapObjects.ContainsKey(mo);
    }

    public bool isPartyLeader()
    {
        Monitor.Enter(prtLock);
        try
        {
            Party? party = getParty();
            return party != null && party.getLeaderId() == getId();
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public bool isGuildLeader()
    {    // true on guild master or jr. master
        return guildid > 0 && guildRank < 3;
    }

    public bool attemptCatchFish(int baitLevel)
    {
        return YamlConfig.config.server.USE_FISHING_SYSTEM && MapId.isFishingArea(mapid) &&
                this.getPosition().Y > 0 &&
                ItemConstants.isFishingChair(chair.get()) &&
                this.getWorldServer().registerFisherPlayer(this, baitLevel);
    }

    public void leaveMap()
    {
        releaseControlledMonsters();
        visibleMapObjects.Clear();
        setChair(-1);
        if (hpDecreaseTask != null)
        {
            hpDecreaseTask.cancel(false);
        }

        AriantColiseum arena = this.getAriantColiseum();
        if (arena != null)
        {
            arena.leaveArena(this);
        }
    }

    private int getChangedJobSp(Job newJob)
    {
        int curSp = getUsedSp(newJob) + getJobRemainingSp(newJob);
        int spGain = 0;
        int expectedSp = getJobLevelSp(level - 10, newJob, GameConstants.getJobBranch(newJob));
        if (curSp < expectedSp)
        {
            spGain += (expectedSp - curSp);
        }

        return getSpGain(spGain, curSp, newJob);
    }

    private int getUsedSp(Job job)
    {
        int jobId = job.getId();
        int spUsed = 0;

        foreach (var s in this.getSkills())
        {
            Skill skill = s.Key;
            if (GameConstants.isInJobTree(skill.getId(), jobId) && !skill.isBeginnerSkill())
            {
                spUsed += s.Value.skillevel;
            }
        }

        return spUsed;
    }

    private int getJobLevelSp(int level, Job job, int jobBranch)
    {
        if (getJobStyleInternal(job.getId(), 0x40) == Job.MAGICIAN)
        {
            level += 2;  // starts earlier, level 8
        }

        return 3 * level + GameConstants.getChangeJobSpUpgrade(jobBranch);
    }

    private int getJobMaxSp(Job job)
    {
        int jobBranch = GameConstants.getJobBranch(job);
        int jobRange = GameConstants.getJobUpgradeLevelRange(jobBranch);
        return getJobLevelSp(jobRange, job, jobBranch);
    }

    private int getJobRemainingSp(Job job)
    {
        int skillBook = GameConstants.getSkillBook(job.getId());

        int ret = 0;
        for (int i = 0; i <= skillBook; i++)
        {
            ret += this.getRemainingSp(i);
        }

        return ret;
    }

    private int getSpGain(int spGain, Job job)
    {
        int curSp = getUsedSp(job) + getJobRemainingSp(job);
        return getSpGain(spGain, curSp, job);
    }

    private int getSpGain(int spGain, int curSp, Job job)
    {
        int maxSp = getJobMaxSp(job);

        spGain = Math.Min(spGain, maxSp - curSp);
        int jobBranch = GameConstants.getJobBranch(job);
        return spGain;
    }

    private void levelUpGainSp()
    {
        if (GameConstants.getJobBranch(job) == 0)
        {
            return;
        }

        int spGain = 3;
        if (YamlConfig.config.server.USE_ENFORCE_JOB_SP_RANGE && !GameConstants.hasSPTable(job))
        {
            spGain = getSpGain(spGain, job);
        }

        if (spGain > 0)
        {
            gainSp(spGain, GameConstants.getSkillBook(job.getId()), true);
        }
    }

    object levelUpLock = new object();
    public void levelUp(bool takeexp)
    {
        lock (levelUpLock)
        {


            Skill? improvingMaxHP = null;
            Skill? improvingMaxMP = null;
            int improvingMaxHPLevel = 0;
            int improvingMaxMPLevel = 0;

            bool isBeginner = isBeginnerJob();
            if (YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP && isBeginner && level < 11)
            {
                Monitor.Enter(effLock);
                statLock.EnterWriteLock();
                try
                {
                    gainAp(5, true);

                    int str = 0, dex = 0;
                    if (level < 6)
                    {
                        str += 5;
                    }
                    else
                    {
                        str += 4;
                        dex += 1;
                    }

                    assignStrDexIntLuk(str, dex, 0, 0);
                }
                finally
                {
                    statLock.ExitWriteLock();
                    Monitor.Exit(effLock);
                }
            }
            else
            {
                int remainingAp = 5;

                if (isCygnus())
                {
                    if (level > 10)
                    {
                        if (level <= 17)
                        {
                            remainingAp += 2;
                        }
                        else if (level < 77)
                        {
                            remainingAp++;
                        }
                    }
                }

                gainAp(remainingAp, true);
            }

            int addhp = 0, addmp = 0;
            if (isBeginner)
            {
                addhp += Randomizer.rand(12, 16);
                addmp += Randomizer.rand(10, 12);
            }
            else if (job.isA(Job.WARRIOR) || job.isA(Job.DAWNWARRIOR1))
            {
                improvingMaxHP = isCygnus() ? SkillFactory.GetSkillTrust(DawnWarrior.MAX_HP_INCREASE) : SkillFactory.GetSkillTrust(Warrior.IMPROVED_MAXHP);
                if (job.isA(Job.CRUSADER))
                {
                    improvingMaxMP = SkillFactory.GetSkillTrust(1210000);
                }
                else if (job.isA(Job.DAWNWARRIOR2))
                {
                    improvingMaxMP = SkillFactory.GetSkillTrust(11110000);
                }
                improvingMaxHPLevel = getSkillLevel(improvingMaxHP);
                addhp += Randomizer.rand(24, 28);
                addmp += Randomizer.rand(4, 6);
            }
            else if (job.isA(Job.MAGICIAN) || job.isA(Job.BLAZEWIZARD1))
            {
                improvingMaxMP = isCygnus() ? SkillFactory.GetSkillTrust(BlazeWizard.INCREASING_MAX_MP) : SkillFactory.GetSkillTrust(Magician.IMPROVED_MAX_MP_INCREASE);
                improvingMaxMPLevel = getSkillLevel(improvingMaxMP);
                addhp += Randomizer.rand(10, 14);
                addmp += Randomizer.rand(22, 24);
            }
            else if (job.isA(Job.BOWMAN) || job.isA(Job.THIEF) || (job.getId() > 1299 && job.getId() < 1500))
            {
                addhp += Randomizer.rand(20, 24);
                addmp += Randomizer.rand(14, 16);
            }
            else if (job.isA(Job.GM))
            {
                addhp += 30000;
                addmp += 30000;
            }
            else if (job.isA(Job.PIRATE) || job.isA(Job.THUNDERBREAKER1))
            {
                improvingMaxHP = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.IMPROVE_MAX_HP) : SkillFactory.GetSkillTrust(Brawler.IMPROVE_MAX_HP);
                improvingMaxHPLevel = getSkillLevel(improvingMaxHP);
                addhp += Randomizer.rand(22, 28);
                addmp += Randomizer.rand(18, 23);
            }
            else if (job.isA(Job.ARAN1))
            {
                addhp += Randomizer.rand(44, 48);
                int aids = Randomizer.rand(4, 8);
                addmp += (int)(aids + Math.Floor(aids * 0.1));
            }
            if (improvingMaxHPLevel > 0 && (job.isA(Job.WARRIOR) || job.isA(Job.PIRATE) || job.isA(Job.DAWNWARRIOR1) || job.isA(Job.THUNDERBREAKER1)))
            {
                addhp += improvingMaxHP.getEffect(improvingMaxHPLevel).getX();
            }
            if (improvingMaxMPLevel > 0 && (job.isA(Job.MAGICIAN) || job.isA(Job.CRUSADER) || job.isA(Job.BLAZEWIZARD1)))
            {
                addmp += improvingMaxMP.getEffect(improvingMaxMPLevel).getX();
            }

            if (YamlConfig.config.server.USE_RANDOMIZE_HPMP_GAIN)
            {
                if (getJobStyle() == Job.MAGICIAN)
                {
                    addmp += localint_ / 20;
                }
                else
                {
                    addmp += localint_ / 10;
                }
            }

            addMaxMPMaxHP(addhp, addmp, true);

            if (takeexp)
            {
                exp.addAndGet(-ExpTable.getExpNeededForLevel(level));
                if (exp.get() < 0)
                {
                    exp.set(0);
                }
            }

            level++;
            if (level >= getMaxClassLevel())
            {
                exp.set(0);

                int maxClassLevel = getMaxClassLevel();
                if (level == maxClassLevel)
                {
                    if (!this.isGM())
                    {
                        if (YamlConfig.config.server.PLAYERNPC_AUTODEPLOY)
                        {
                            ThreadManager.getInstance().newTask(() =>
                            {
                                PlayerNPC.spawnPlayerNPC(GameConstants.getHallOfFameMapid(job), this);
                            });
                        }

                        string names = (getMedalText() + name);
                        getWorldServer().broadcastPacket(PacketCreator.serverNotice(6, string.Format(LEVEL_200, names, maxClassLevel, names)));
                    }
                }

                level = maxClassLevel; //To prevent levels past the maximum
            }

            levelUpGainSp();

            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                recalcLocalStats();
                changeHpMp(localmaxhp, localmaxmp, true);

                List<KeyValuePair<Stat, int>> statup = new(10);
                statup.Add(new(Stat.AVAILABLEAP, remainingAp));
                statup.Add(new(Stat.AVAILABLESP, remainingSp[GameConstants.getSkillBook(job.getId())]));
                statup.Add(new(Stat.HP, hp));
                statup.Add(new(Stat.MP, mp));
                statup.Add(new(Stat.EXP, exp.get()));
                statup.Add(new(Stat.LEVEL, level));
                statup.Add(new(Stat.MAXHP, clientmaxhp));
                statup.Add(new(Stat.MAXMP, clientmaxmp));
                statup.Add(new(Stat.STR, str));
                statup.Add(new(Stat.DEX, dex));

                sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }

            getMap().broadcastMessage(this, PacketCreator.showForeignEffect(getId(), 0), false);
            setMPC(new PartyCharacter(this));
            silentPartyUpdate();

            if (this.guildid > 0)
            {
                getGuild().broadcast(PacketCreator.levelUpMessage(2, level, name), this.getId());
            }

            if (level % 20 == 0)
            {
                if (YamlConfig.config.server.USE_ADD_SLOTS_BY_LEVEL == true)
                {
                    if (!isGM())
                    {
                        for (byte i = 1; i < 5; i++)
                        {
                            gainSlots(i, 4, true);
                        }

                        this.yellowMessage("You reached level " + level + ". Congratulations! As a token of your success, your inventory has been expanded a little bit.");
                    }
                }
                if (YamlConfig.config.server.USE_ADD_RATES_BY_LEVEL == true)
                { //For the rate upgrade
                    revertLastPlayerRates();
                    setPlayerRates();
                    this.yellowMessage("You managed to get level " + level + "! Getting experience and items seems a little easier now, huh?");
                }
            }

            if (YamlConfig.config.server.USE_PERFECT_PITCH && level >= 30)
            {
                //milestones?
                if (InventoryManipulator.checkSpace(client, ItemId.PERFECT_PITCH, 1, ""))
                {
                    InventoryManipulator.addById(client, ItemId.PERFECT_PITCH, 1, "", -1);
                }
            }
            else if (level == 10)
            {
                Action r = () =>
                {
                    if (leaveParty())
                    {
                        showHint("You have reached #blevel 10#k, therefore you must leave your #rstarter party#k.");

                    }
                };

                ThreadManager.getInstance().newTask(r);
            }

            guildUpdate();

            FamilyEntry? familyEntry = getFamilyEntry();
            if (familyEntry != null)
            {
                familyEntry.giveReputationToSenior(YamlConfig.config.server.FAMILY_REP_PER_LEVELUP, true);
                FamilyEntry senior = familyEntry.getSenior();
                if (senior != null)
                { //only send the message to direct senior
                    Character seniorChr = senior.getChr();
                    if (seniorChr != null)
                    {
                        seniorChr.sendPacket(PacketCreator.levelUpMessage(1, level, getName()));
                    }
                }
            }
        }
    }

    public bool leaveParty()
    {
        Party? party;
        bool partyLeader;

        Monitor.Enter(prtLock);
        try
        {
            party = getParty();
            partyLeader = isPartyLeader();
        }
        finally
        {
            Monitor.Exit(prtLock);
        }

        if (party != null)
        {
            if (partyLeader)
            {
                party.assignNewLeader(client);
            }
            Party.leaveParty(party, client);

            return true;
        }
        else
        {
            return false;
        }
    }
    public void setPlayerRates()
    {
        this.expRate *= GameConstants.getPlayerBonusExpRate(this.level / 20);
        this.mesoRate *= GameConstants.getPlayerBonusMesoRate(this.level / 20);
        this.dropRate *= GameConstants.getPlayerBonusDropRate(this.level / 20);
    }

    public void revertLastPlayerRates()
    {
        this.expRate /= GameConstants.getPlayerBonusExpRate((this.level - 1) / 20);
        this.mesoRate /= GameConstants.getPlayerBonusMesoRate((this.level - 1) / 20);
        this.dropRate /= GameConstants.getPlayerBonusDropRate((this.level - 1) / 20);
    }

    public void revertPlayerRates()
    {
        this.expRate /= GameConstants.getPlayerBonusExpRate(this.level / 20);
        this.mesoRate /= GameConstants.getPlayerBonusMesoRate(this.level / 20);
        this.dropRate /= GameConstants.getPlayerBonusDropRate(this.level / 20);
    }

    public void setWorldRates()
    {
        World worldz = getWorldServer();
        this.expRate *= worldz.getExpRate();
        this.mesoRate *= worldz.getMesoRate();
        this.dropRate *= worldz.getDropRate();
    }

    public void revertWorldRates()
    {
        World worldz = getWorldServer();
        this.expRate /= worldz.getExpRate();
        this.mesoRate /= worldz.getMesoRate();
        this.dropRate /= worldz.getDropRate();
    }

    private void setCouponRates()
    {
        List<int> couponEffects;

        var cashItems = this.getInventory(InventoryType.CASH).list();
        chLock.EnterReadLock();
        try
        {
            setActiveCoupons(cashItems);
            couponEffects = activateCouponsEffects();
        }
        finally
        {
            chLock.ExitReadLock();
        }

        foreach (int couponId in couponEffects)
        {
            commitBuffCoupon(couponId);
        }
    }

    private void revertCouponRates()
    {
        revertCouponsEffects();
    }

    public void updateCouponRates()
    {
        Inventory cashInv = this.getInventory(InventoryType.CASH);
        if (cashInv == null)
        {
            return;
        }

        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        cashInv.lockInventory();
        try
        {
            revertCouponRates();
            setCouponRates();
        }
        finally
        {
            cashInv.unlockInventory();
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void resetPlayerRates()
    {
        expRate = 1;
        mesoRate = 1;
        dropRate = 1;

        expCoupon = 1;
        mesoCoupon = 1;
        dropCoupon = 1;
    }

    private int getCouponMultiplier(int couponId)
    {
        return activeCouponRates.GetValueOrDefault(couponId);
    }

    private void setExpCouponRate(int couponId, int couponQty)
    {
        this.expCoupon *= (getCouponMultiplier(couponId) * couponQty);
    }

    private void setDropCouponRate(int couponId, int couponQty)
    {
        this.dropCoupon *= (getCouponMultiplier(couponId) * couponQty);
        this.mesoCoupon *= (getCouponMultiplier(couponId) * couponQty);
    }

    private void revertCouponsEffects()
    {
        dispelBuffCoupons();

        this.expRate /= this.expCoupon;
        this.dropRate /= this.dropCoupon;
        this.mesoRate /= this.mesoCoupon;

        this.expCoupon = 1;
        this.dropCoupon = 1;
        this.mesoCoupon = 1;
    }

    private List<int> activateCouponsEffects()
    {
        List<int> toCommitEffect = new();

        if (YamlConfig.config.server.USE_STACK_COUPON_RATES)
        {
            foreach (var coupon in activeCoupons)
            {
                int couponId = coupon.Key;
                int couponQty = coupon.Value;

                toCommitEffect.Add(couponId);

                if (ItemConstants.isExpCoupon(couponId))
                {
                    setExpCouponRate(couponId, couponQty);
                }
                else
                {
                    setDropCouponRate(couponId, couponQty);
                }
            }
        }
        else
        {
            int maxExpRate = 1, maxDropRate = 1, maxExpCouponId = -1, maxDropCouponId = -1;

            foreach (var coupon in activeCoupons)
            {
                int couponId = coupon.Key;

                if (ItemConstants.isExpCoupon(couponId))
                {
                    if (maxExpRate < getCouponMultiplier(couponId))
                    {
                        maxExpCouponId = couponId;
                        maxExpRate = getCouponMultiplier(couponId);
                    }
                }
                else
                {
                    if (maxDropRate < getCouponMultiplier(couponId))
                    {
                        maxDropCouponId = couponId;
                        maxDropRate = getCouponMultiplier(couponId);
                    }
                }
            }

            if (maxExpCouponId > -1)
            {
                toCommitEffect.Add(maxExpCouponId);
            }
            if (maxDropCouponId > -1)
            {
                toCommitEffect.Add(maxDropCouponId);
            }

            this.expCoupon = maxExpRate;
            this.dropCoupon = maxDropRate;
            this.mesoCoupon = maxDropRate;
        }

        this.expRate *= this.expCoupon;
        this.dropRate *= this.dropCoupon;
        this.mesoRate *= this.mesoCoupon;

        return toCommitEffect;
    }

    private void setActiveCoupons(ICollection<Item> cashItems)
    {
        activeCoupons.Clear();
        activeCouponRates.Clear();

        Dictionary<int, int> coupons = Server.getInstance().getCouponRates();
        List<int> active = Server.getInstance().getActiveCoupons();

        foreach (Item it in cashItems)
        {
            if (ItemConstants.isRateCoupon(it.getItemId()) && active.Contains(it.getItemId()))
            {
                int? count = activeCoupons.get(it.getItemId());

                if (count != null)
                {
                    activeCoupons.AddOrUpdate(it.getItemId(), count.Value + 1);
                }
                else
                {
                    activeCoupons.AddOrUpdate(it.getItemId(), 1);
                    activeCouponRates.AddOrUpdate(it.getItemId(), coupons.GetValueOrDefault(it.getItemId()));
                }
            }
        }
    }

    private void commitBuffCoupon(int couponid)
    {
        if (!isLoggedin() || getCashShop().isOpened())
        {
            return;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        StatEffect mse = ii.getItemEffect(couponid);
        mse.applyTo(this);
    }

    public void dispelBuffCoupons()
    {
        List<BuffStatValueHolder> allBuffs = getAllStatups();

        foreach (BuffStatValueHolder mbsvh in allBuffs)
        {
            if (ItemConstants.isRateCoupon(mbsvh.effect.getSourceId()))
            {
                cancelEffect(mbsvh.effect, false, mbsvh.startTime);
            }
        }
    }

    public IReadOnlyCollection<int> getActiveCoupons()
    {
        Monitor.Enter(chrLock);
        try
        {
            return new ReadOnlyCollection<int>(activeCoupons.Keys.ToList());
        }
        finally
        {
            Monitor.Exit(chrLock);
        }
    }

    public void addPlayerRing(Ring ring)
    {
        int ringItemId = ring.getItemId();
        if (ItemId.isWeddingRing(ringItemId))
        {
            this.addMarriageRing(ring);
        }
        else if (ring.getItemId() > 1112012)
        {
            this.addFriendshipRing(ring);
        }
        else
        {
            this.addCrushRing(ring);
        }
    }

    public static Character loadCharacterEntryFromDB(DB_Character rs, List<Item>? equipped)
    {
        Character ret = new Character();

        try
        {
            ret.accountid = rs.AccountId;
            ret.id = rs.Id;
            ret.name = rs.Name;
            ret.gender = rs.Gender;
            ret.skinColor = SkinColorUtils.getById(rs.Skincolor);
            ret.face = rs.Face;
            ret.hair = rs.Hair;

            // skipping pets, probably unneeded here

            ret.level = rs.Level;
            ret.job = JobUtils.getById(rs.Job);
            ret.str = rs.Str;
            ret.dex = rs.Dex;
            ret.int_ = rs.Int;
            ret.luk = rs.Luk;
            ret.hp = (rs.Hp);
            ret.setMaxHp(rs.Maxhp);
            ret.mp = rs.Mp;
            ret.setMaxMp(rs.Maxmp);
            ret.remainingAp = rs.Ap;
            ret.loadCharSkillPoints(rs.Sp.Split(","));
            ret.exp.set(rs.Exp);
            ret.fame = rs.Fame;
            ret.gachaexp.set(rs.Gachaexp);
            ret.mapid = rs.Map;
            ret.initialSpawnPoint = rs.Spawnpoint;
            ret.setGMLevel(rs.Gm);
            ret.world = rs.World;
            ret.rank = rs.Rank;
            ret.rankMove = rs.RankMove;
            ret.jobRank = rs.JobRank;
            ret.jobRankMove = rs.JobRankMove;

            if (equipped != null)
            {
                // players can have no equipped items at all, ofc
                Inventory inv = ret.inventory[InventoryType.EQUIPPED.ordinal()];
                foreach (Item item in equipped)
                {
                    inv.addItemFromDB(item);
                }
            }
        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }

        return ret;
    }

    public Character generateCharacterEntry()
    {
        Character ret = new Character();

        ret.accountid = this.getAccountID();
        ret.id = this.getId();
        ret.name = this.getName();
        ret.gender = this.getGender();
        ret.skinColor = this.getSkinColor();
        ret.face = this.getFace();
        ret.hair = this.getHair();

        // skipping pets, probably unneeded here

        ret.level = this.getLevel();
        ret.job = this.getJob();
        ret.str = this.getStr();
        ret.dex = this.getDex();
        ret.int_ = this.getInt();
        ret.luk = this.getLuk();
        ret.hp = this.getHp();
        ret.setMaxHp(this.getMaxHp());
        ret.mp = this.getMp();
        ret.setMaxMp(this.getMaxMp());
        ret.remainingAp = this.getRemainingAp();
        ret.setRemainingSp(this.getRemainingSps());
        ret.exp.set(this.getExp());
        ret.fame = this.getFame();
        ret.gachaexp.set(this.getGachaExp());
        ret.mapid = this.getMapId();
        ret.initialSpawnPoint = this.getInitialSpawnpoint();

        ret.inventory[InventoryType.EQUIPPED.ordinal()] = this.getInventory(InventoryType.EQUIPPED);

        ret.setGMLevel(this.gmLevel());
        ret.world = this.getWorld();
        ret.rank = this.getRank();
        ret.rankMove = this.getRankMove();
        ret.jobRank = this.getJobRank();
        ret.jobRankMove = this.getJobRankMove();

        return ret;
    }

    private void loadCharSkillPoints(string[] skillPoints)
    {
        int[] sps = new int[skillPoints.Length];
        for (int i = 0; i < skillPoints.Length; i++)
        {
            sps[i] = int.Parse(skillPoints[i]);
        }

        setRemainingSp(sps);
    }

    public int getRemainingSp()
    {
        return getRemainingSp(job.getId()); //default
    }

    public void updateRemainingSp(int remainingSp)
    {
        updateRemainingSp(remainingSp, GameConstants.getSkillBook(job.getId()));
    }

    public static Character loadCharFromDB(int charid, Client client, bool channelserver)
    {
        Character ret = new Character();
        ret.client = client;
        ret.id = charid;

        try
        {
            World wserv;


            using var dbContext = new DBContext();
            var dbModel = dbContext.Characters.FirstOrDefault(x => x.Id == charid);
            if (dbModel == null)
            {
                throw new Exception("Loading char failed (not found)");
            }

            ret.name = dbModel.Name;
            ret.level = dbModel.Level;
            ret.fame = dbModel.Fame;
            ret.quest_fame = dbModel.Fquest;
            ret.str = dbModel.Str;
            ret.dex = dbModel.Dex;
            ret.int_ = dbModel.Int;
            ret.luk = dbModel.Luk;
            ret.exp.set(dbModel.Exp);
            ret.gachaexp.set(dbModel.Gachaexp);
            ret.hp = (dbModel.Hp);
            ret.setMaxHp(dbModel.Maxhp);
            ret.mp = dbModel.Mp;
            ret.setMaxMp(dbModel.Maxmp);
            ret.hpMpApUsed = dbModel.HpMpUsed;
            ret._hasMerchant = dbModel.HasMerchant == true;
            ret.remainingAp = dbModel.Ap;
            ret.loadCharSkillPoints(dbModel.Sp.Split(","));
            ret.meso.set(dbModel.Meso);
            ret.merchantmeso = dbModel.MerchantMesos ?? 0;
            ret.setGMLevel(dbModel.Gm);
            ret.skinColor = SkinColorUtils.getById(dbModel.Skincolor);
            ret.gender = dbModel.Gender;
            ret.job = JobUtils.getById(dbModel.Job);
            ret.finishedDojoTutorial = dbModel.FinishedDojoTutorial;
            ret.vanquisherKills = dbModel.VanquisherKills;
            ret.omokwins = dbModel.Omokwins;
            ret.omoklosses = dbModel.Omoklosses;
            ret.omokties = dbModel.Omokties;
            ret.matchcardwins = dbModel.Matchcardwins;
            ret.matchcardlosses = dbModel.Matchcardlosses;
            ret.matchcardties = dbModel.Matchcardties;
            ret.hair = dbModel.Hair;
            ret.face = dbModel.Face;
            ret.accountid = dbModel.AccountId;
            ret.mapid = dbModel.Map;
            ret.jailExpiration = dbModel.Jailexpire;
            ret.initialSpawnPoint = dbModel.Spawnpoint;
            ret.world = dbModel.World;
            ret.rank = dbModel.Rank;
            ret.rankMove = dbModel.RankMove;
            ret.jobRank = dbModel.JobRank;
            ret.jobRankMove = dbModel.JobRankMove;
            int mountexp = dbModel.MountExp;
            int mountlevel = dbModel.MountLevel;
            int mounttiredness = dbModel.Mounttiredness;
            ret.guildid = dbModel.GuildId;
            ret.guildRank = dbModel.GuildRank;
            ret.allianceRank = dbModel.AllianceRank;
            ret.familyId = dbModel.FamilyId;
            ret.bookCover = dbModel.Monsterbookcover;
            ret.monsterbook = new MonsterBook();
            ret.monsterbook.loadCards(charid);
            ret.vanquisherStage = dbModel.VanquisherStage;
            ret.ariantPoints = dbModel.AriantPoints;
            ret.dojoPoints = dbModel.DojoPoints;
            ret.dojoStage = dbModel.LastDojoStage;
            ret.dataString = dbModel.DataString;
            ret.mgc = new GuildCharacter(ret);
            int buddyCapacity = dbModel.BuddyCapacity;
            ret.buddylist = new BuddyList(buddyCapacity);
            ret.lastExpGainTime = dbModel.LastExpGainTime;
            ret.canRecvPartySearchInvite = dbModel.PartySearch;

            wserv = Server.getInstance().getWorld(ret.world);

            ret.getInventory(InventoryType.EQUIP).setSlotLimit(dbModel.Equipslots);
            ret.getInventory(InventoryType.USE).setSlotLimit(dbModel.Useslots);
            ret.getInventory(InventoryType.SETUP).setSlotLimit(dbModel.Setupslots);
            ret.getInventory(InventoryType.ETC).setSlotLimit(dbModel.Etcslots);

            short sandboxCheck = 0x0;
            foreach (var item in ItemFactory.INVENTORY.loadItems(ret.id, !channelserver))
            {
                sandboxCheck |= item.Item.getFlag();

                ret.getInventory(item.Type).addItemFromDB(item.Item);
                Item itemz = item.Item;
                if (itemz.getPetId() > -1)
                {
                    var pet = itemz.getPet();
                    if (pet != null && pet.isSummoned())
                    {
                        ret.addPet(pet);
                    }
                    continue;
                }

                InventoryType mit = item.Type;
                if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                {
                    Equip equip = (Equip)item.Item;
                    if (equip.getRingId() > -1)
                    {
                        Ring ring = Ring.loadFromDb(equip.getRingId())!;
                        if (ring != null && item.Type.Equals(InventoryType.EQUIPPED))
                        {
                            ring.equip();
                        }

                        ret.addPlayerRing(ring);
                    }
                }
            }

            if ((sandboxCheck & ItemConstants.SANDBOX) == ItemConstants.SANDBOX)
            {
                ret.setHasSandboxItem();
            }

            ret.partnerId = dbModel.PartnerId;
            ret.marriageItemid = dbModel.MarriageItemId;
            if (ret.marriageItemid > 0 && ret.partnerId <= 0)
            {
                ret.marriageItemid = -1;
            }
            else if (ret.partnerId > 0 && wserv.getRelationshipId(ret.id) <= 0)
            {
                ret.marriageItemid = -1;
                ret.partnerId = -1;
            }

            NewYearCardRecord.loadPlayerNewYearCards(ret);

            //PreparedStatement ps2, ps3;
            //ResultSet rs2, rs3;

            // Items excluded from pet loot
            var petDataFromDB = (from a in dbContext.Inventoryitems.Where(x => x.Characterid == charid && x.Petid > -1)
                                 let excluded = dbContext.Petignores.Where(x => x.Petid == a.Petid).ToList()
                                 select new { a.Petid, excluded }).ToList();
            foreach (var item in petDataFromDB)
            {
                int petId = item.Petid;
                ret.resetExcluded(petId);

                foreach (var ex in item.excluded)
                {
                    ret.addExcluded(petId, ex.Itemid);
                }
            }

            ret.commitExcludedItems();


            if (channelserver)
            {
                MapManager mapManager = client.getChannelServer().getMapFactory();
                ret.map = mapManager.getMap(ret.mapid);

                if (ret.map == null)
                {
                    ret.map = mapManager.getMap(MapId.HENESYS);
                }
                Portal? portal = ret.map.getPortal(ret.initialSpawnPoint);
                if (portal == null)
                {
                    portal = ret.map.getPortal(0)!;
                    ret.initialSpawnPoint = 0;
                }
                ret.setPosition(portal.getPosition());
                int partyid = dbModel.Party;
                Party party = wserv.getParty(partyid);
                if (party != null)
                {
                    ret.mpc = party.getMemberById(ret.id);
                    if (ret.mpc != null)
                    {
                        ret.mpc = new PartyCharacter(ret);
                        ret.party = party;
                    }
                }
                int messengerid = dbModel.MessengerId;
                int position = dbModel.MessengerPosition;
                if (messengerid > 0 && position < 4 && position > -1)
                {
                    var messenger = wserv.getMessenger(messengerid);
                    if (messenger != null)
                    {
                        ret.messenger = messenger;
                        ret.messengerposition = position;
                    }
                }
                ret.loggedIn = true;
            }


            var trockLocList = dbContext.Trocklocations.Where(x => x.Characterid == charid).Select(x => new { x.Vip, x.Mapid }).Take(15).ToList();

            byte vip = 0;
            byte reg = 0;
            foreach (var item in trockLocList)
            {
                if (item.Vip == 1)
                {
                    ret.viptrockmaps.Add(item.Mapid);
                    vip++;
                }
                else
                {
                    ret.trockmaps.Add(item.Mapid);
                    reg++;
                }
            }
            while (vip < 10)
            {
                ret.viptrockmaps.Add(MapId.NONE);
                vip++;
            }
            while (reg < 5)
            {
                ret.trockmaps.Add(MapId.NONE);
                reg++;
            }



            var accountFromDB = dbContext.Accounts.Where(x => x.Id == ret.accountid).Select(x => new { x.Name, x.Characterslots, x.Language }).FirstOrDefault();
            if (accountFromDB != null)
            {
                Client retClient = ret.getClient();

                retClient.setAccountName(accountFromDB.Name);
                retClient.setCharacterSlots(accountFromDB.Characterslots);
                retClient.setLanguage(accountFromDB.Language);   // thanks Zein for noticing user language not overriding default once player is in-game
            }

            var areaInfoFromDB = dbContext.AreaInfos.Where(x => x.Charid == ret.id).Select(x => new { x.Area, x.Info }).ToList();
            foreach (var item in areaInfoFromDB)
            {
                ret.area_info.AddOrUpdate((short)item.Area, item.Info);
            }

            var eventStatsFromDB = dbContext.Eventstats.Where(x => x.Characterid == ret.id).Select(x => new { x.Name, x.Info }).ToList();
            foreach (var item in eventStatsFromDB)
            {
                string name = item.Name;
                // 全是rescueGaga才插入，应该会有重复键bug
                if (item.Name == "rescueGaga")
                {
                    ret.events.AddOrUpdate(name, new RescueGaga(item.Info));
                }
            }

            ret.cashshop = new CashShop(ret.accountid, ret.id, ret.getJobType());
            ret._autoban = new AutobanManager(ret);

            // Blessing of the Fairy
            var otherCharFromDB = dbContext.Characters.Where(x => x.AccountId == ret.accountid && x.Id != charid)
                .OrderByDescending(x => x.Level).Select(x => new { x.Name, x.Level }).FirstOrDefault();
            if (otherCharFromDB != null)
            {
                ret.linkedName = otherCharFromDB.Name;
                ret.linkedLevel = otherCharFromDB.Level;
            }

            if (channelserver)
            {
                Dictionary<int, QuestStatus> loadedQuestStatus = new();

                var statusFromDB = dbContext.Queststatuses.Where(x => x.Characterid == charid).ToList();
                foreach (var item in statusFromDB)
                {
                    Quest q = Quest.getInstance(item.Quest);
                    QuestStatus status = new QuestStatus(q, (QuestStatus.Status)item.Status);
                    long cTime = item.Time;
                    if (cTime > -1)
                    {
                        status.setCompletionTime(cTime * 1000);
                    }

                    long eTime = item.Expires;
                    if (eTime > 0)
                    {
                        status.setExpirationTime(eTime);
                    }

                    status.setForfeited(item.Forfeited);
                    status.setCompleted(item.Completed);
                    ret.quests.AddOrUpdate(q.getId(), status);
                    loadedQuestStatus.AddOrUpdate(item.Queststatusid, status);
                }


                // Quest progress
                // opportunity for improvement on questprogress/medalmaps calls to DB
                var questProgressFromDB = dbContext.Questprogresses.Where(x => x.Characterid == charid).ToList();
                foreach (var item in questProgressFromDB)
                {
                    var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
                    if (status != null)
                    {
                        status.setProgress(item.Progressid, item.Progress);
                    }
                }

                // Medal map visit progress
                var medalMapFromDB = dbContext.Medalmaps.Where(x => x.Characterid == charid).ToList();
                foreach (var item in medalMapFromDB)
                {
                    var status = loadedQuestStatus.GetValueOrDefault(item.Queststatusid);
                    if (status != null)
                    {
                        status.addMedalMap(item.Mapid);
                    }
                }

                loadedQuestStatus.Clear();

                // Skills
                var skillInfoFromDB = dbContext.Skills.Where(x => x.Characterid == charid).ToList();
                foreach (var item in skillInfoFromDB)
                {
                    var pSkill = SkillFactory.getSkill(item.Skillid);
                    if (pSkill != null)  // edit reported by Shavit (=＾● ⋏ ●＾=), thanks Zein for noticing an NPE here
                    {
                        ret.skills.AddOrUpdate(pSkill, new SkillEntry((sbyte)item.Skilllevel, item.Masterlevel, item.Expiration));
                    }
                }

                // Cooldowns (load)
                var cdFromDB = dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ToList();
                foreach (var item in cdFromDB)
                {
                    int skillid = item.SkillId;
                    long length = item.Length;
                    long startTime = item.StartTime;
                    if (skillid != 5221999 && (length + startTime < Server.getInstance().getCurrentTime()))
                    {
                        continue;
                    }
                    ret.giveCoolDowns(skillid, startTime, length);

                }

                // Cooldowns (delete)
                dbContext.Cooldowns.Where(x => x.Charid == ret.getId()).ExecuteDelete();

                // Debuffs (load)
                #region Playerdiseases
                Dictionary<Disease, DiseaseExpiration> loadedDiseases = new();
                var playerDiseaseFromDB = dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ToList();
                foreach (var item in playerDiseaseFromDB)
                {
                    Disease disease = Disease.ordinal(item.Disease);
                    if (disease == Disease.NULL)
                    {
                        continue;
                    }

                    int skillid = item.Mobskillid, skilllv = item.Mobskilllv;
                    long length = item.Length;

                    MobSkill? ms = MobSkillFactory.getMobSkill(MobSkillTypeUtils.from(skillid), skilllv);
                    if (ms != null)
                    {
                        loadedDiseases.AddOrUpdate(disease, new(length, ms));
                    }
                }

                dbContext.Playerdiseases.Where(x => x.Charid == ret.getId()).ExecuteDelete();
                if (loadedDiseases.Count > 0)
                {
                    Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(ret.id, loadedDiseases);
                }
                #endregion

                // Skill macros
                var skillMacroFromDB = dbContext.Skillmacros.Where(x => x.Characterid == charid).ToList();
                foreach (var item in skillMacroFromDB)
                {
                    int position = item.Position;
                    SkillMacro macro = new SkillMacro(item.Skill1, item.Skill2, item.Skill3, item.Name, item.Shout, position);
                    ret.skillMacros[position] = macro;
                }

                // Key config
                var keyMapFromDB = dbContext.Keymaps.Where(x => x.Characterid == charid).Select(x => new { x.Key, x.Type, x.Action }).ToList();
                foreach (var item in keyMapFromDB)
                {
                    ret.keymap.AddOrUpdate(item.Key, new KeyBinding(item.Type, item.Action));
                }


                var savedLocFromDB = dbContext.Savedlocations.Where(x => x.Characterid == charid).Select(x => new { x.Locationtype, x.Map, x.Portal }).ToList();
                foreach (var item in savedLocFromDB)
                {
                    ret.savedLocations[(int)Enum.Parse<SavedLocationType>(item.Locationtype)] = new SavedLocation(item.Map, item.Portal);
                }

                // Fame history
                var now = DateTimeOffset.Now;
                var fameLogFromDB = dbContext.Famelogs.Where(x => x.Characterid == charid && EF.Functions.DateDiffDay(now, x.When) < 30);
                foreach (var item in fameLogFromDB)
                {
                    ret.lastfametime = Math.Max(ret.lastfametime, item.When.ToUnixTimeMilliseconds());
                    ret.lastmonthfameids.Add(item.CharacteridTo);
                }

                ret.buddylist.loadFromDb(charid);
                ret.storage = wserv.getAccountStorage(ret.accountid);

                /* double-check storage incase player is first time on server
                 * The storage won't exist so nothing to load
                 */
                if (ret.storage == null)
                {
                    wserv.loadAccountStorage(ret.accountid);
                    ret.storage = wserv.getAccountStorage(ret.accountid);
                }

                int startHp = ret.hp, startMp = ret.mp;
                ret.reapplyLocalStats();
                ret.changeHpMp(startHp, startMp, true);
                //ret.resetBattleshipHp();

            }

            int mountid = ret.getJobType() * 10000000 + 1004;
            if (ret.getInventory(InventoryType.EQUIPPED).getItem(-18) != null)
            {
                ret.maplemount = new Mount(ret, ret.getInventory(InventoryType.EQUIPPED).getItem(-18)!.getItemId(), mountid);
            }
            else
            {
                ret.maplemount = new Mount(ret, 0, mountid);
            }
            ret.maplemount.setExp(mountexp);
            ret.maplemount.setLevel(mountlevel);
            ret.maplemount.setTiredness(mounttiredness);
            ret.maplemount.setActive(false);

            // Quickslot key config
            var accKeyMapFromDB = dbContext.Quickslotkeymappeds.Where(x => x.Accountid == ret.getAccountID()).Select(x => (long?)x.Keymap).FirstOrDefault();
            if (accKeyMapFromDB != null)
            {
                ret.m_aQuickslotLoaded = LongTool.LongToBytes(accKeyMapFromDB.Value);
                ret.m_pQuickslotKeyMapped = new QuickslotBinding(ret.m_aQuickslotLoaded);
            }

            return ret;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
        return null;
    }

    public void reloadQuestExpirations()
    {
        foreach (QuestStatus mqs in getStartedQuests())
        {
            if (mqs.getExpirationTime() > 0)
            {
                questTimeLimit2(mqs.getQuest(), mqs.getExpirationTime());
            }
        }
    }

    public static string makeMapleReadable(string input)
    {
        string i = input.Replace('I', 'i');
        i = i.Replace('l', 'L');
        i = i.Replace("rn", "Rn");
        i = i.Replace("vv", "Vv");
        i = i.Replace("VV", "Vv");

        return i;
    }

    public class BuffStatValueHolder
    {

        public StatEffect effect;
        public long startTime;
        public int value;
        public bool bestApplied;

        public BuffStatValueHolder(StatEffect effect, long startTime, int value)
        {
            this.effect = effect;
            this.startTime = startTime;
            this.value = value;
            this.bestApplied = false;
        }
    }

    public class CooldownValueHolder
    {

        public int skillId;
        public long startTime, length;

        public CooldownValueHolder(int skillId, long startTime, long length)
        {
            this.skillId = skillId;
            this.startTime = startTime;
            this.length = length;
        }
    }

    public void message(string m)
    {
        dropMessage(5, m);
    }

    public void yellowMessage(string m)
    {
        sendPacket(PacketCreator.sendYellowTip(m));
    }

    public void raiseQuestMobCount(int id)
    {
        // It seems nexon uses monsters that don't exist in the WZ (except string) to merge multiple mobs together for these 3 monsters.
        // We also want to run mobKilled for both since there are some quest that don't use the updated ID...
        if (id == MobId.GREEN_MUSHROOM || id == MobId.DEJECTED_GREEN_MUSHROOM)
        {
            raiseQuestMobCount(MobId.GREEN_MUSHROOM_QUEST);
        }
        else if (id == MobId.ZOMBIE_MUSHROOM || id == MobId.ANNOYED_ZOMBIE_MUSHROOM)
        {
            raiseQuestMobCount(MobId.ZOMBIE_MUSHROOM_QUEST);
        }
        else if (id == MobId.GHOST_STUMP || id == MobId.SMIRKING_GHOST_STUMP)
        {
            raiseQuestMobCount(MobId.GHOST_STUMP_QUEST);
        }

        int lastQuestProcessed = 0;
        try
        {
            lock (quests)
            {
                foreach (QuestStatus qs in getQuests())
                {
                    lastQuestProcessed = qs.getQuest().getId();
                    if (qs.getStatus() == QuestStatus.Status.COMPLETED || qs.getQuest().canComplete(this, null))
                    {
                        continue;
                    }

                    if (qs.progress(id))
                    {
                        announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
                        if (qs.getInfoNumber() > 0)
                        {
                            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            log.Warning(e, "Character.mobKilled. chrId {CharacterId}, last quest processed: {LastQuestProcessed}", this.id, lastQuestProcessed);
        }
    }

    public Mount mount(int id, int skillid)
    {
        Mount mount = maplemount!;
        mount.setItemId(id);
        mount.setSkillId(skillid);
        return mount;
    }

    private void playerDead()
    {
        if (this.getMap().isCPQMap())
        {
            int losing = getMap().getDeathCP();
            if (getCP() < losing)
            {
                losing = getCP();
            }
            getMap().broadcastMessage(PacketCreator.playerDiedMessage(getName(), losing, getTeam()));
            gainCP(-losing);
            return;
        }

        cancelAllBuffs(false);
        dispelDebuffs();
        lastDeathtime = Server.getInstance().getCurrentTime();

        EventInstanceManager? eim = getEventInstance();
        if (eim != null)
        {
            eim.playerKilled(this);
        }
        int[] charmID = { ItemId.SAFETY_CHARM, ItemId.EASTER_BASKET, ItemId.EASTER_CHARM };
        int possesed = 0;
        int i;
        for (i = 0; i < charmID.Length; i++)
        {
            int quantity = getItemQuantity(charmID[i], false);
            if (possesed == 0 && quantity > 0)
            {
                possesed = quantity;
                break;
            }
        }
        if (possesed > 0 && !MapId.isDojo(getMapId()))
        {
            message("You have used a safety charm, so your EXP points have not been decreased.");
            InventoryManipulator.removeById(client, ItemConstants.getInventoryType(charmID[i]), charmID[i], 1, true, false);
            usedSafetyCharm = true;
        }
        else if (getJob() != Job.BEGINNER)
        { //Hmm...
            if (!FieldLimit.NO_EXP_DECREASE.check(getMap().getFieldLimit()))
            {  // thanks Conrad for noticing missing FieldLimit check
                int XPdummy = ExpTable.getExpNeededForLevel(getLevel());

                if (getMap().isTown())
                {    // thanks MindLove, SIayerMonkey, HaItsNotOver for noting players only lose 1% on town maps
                    XPdummy /= 100;
                }
                else
                {
                    if (getLuk() < 50)
                    {    // thanks Taiketo, Quit, Fishanelli for noting player EXP loss are fixed, 50-LUK threshold
                        XPdummy /= 10;
                    }
                    else
                    {
                        XPdummy /= 20;
                    }
                }

                int curExp = getExp();
                if (curExp > XPdummy)
                {
                    loseExp(XPdummy, false, false);
                }
                else
                {
                    loseExp(curExp, false, false);
                }
            }
        }

        if (getBuffedValue(BuffStat.MORPH) != null)
        {
            cancelEffectFromBuffStat(BuffStat.MORPH);
        }

        if (getBuffedValue(BuffStat.MONSTER_RIDING) != null)
        {
            cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);
        }

        unsitChairInternal();
        sendPacket(PacketCreator.enableActions());
    }

    private void unsitChairInternal()
    {
        int chairid = chair.get();
        if (chairid >= 0)
        {
            if (ItemConstants.isFishingChair(chairid))
            {
                this.getWorldServer().unregisterFisherPlayer(this);
            }

            setChair(-1);
            if (unregisterChairBuff())
            {
                getMap().broadcastMessage(this, PacketCreator.cancelForeignChairSkillEffect(this.getId()), false);
            }

            getMap().broadcastMessage(this, PacketCreator.showChair(this.getId(), 0), false);
        }

        sendPacket(PacketCreator.cancelChair(-1));
    }

    public void sitChair(int itemId)
    {
        if (this.isLoggedinWorld())
        {
            if (itemId >= 1000000)
            {    // sit on item chair
                if (chair.get() < 0)
                {
                    setChair(itemId);
                    getMap().broadcastMessage(this, PacketCreator.showChair(this.getId(), itemId), false);
                }
                sendPacket(PacketCreator.enableActions());
            }
            else if (itemId >= 0)
            {    // sit on map chair
                if (chair.get() < 0)
                {
                    setChair(itemId);
                    if (registerChairBuff())
                    {
                        getMap().broadcastMessage(this, PacketCreator.giveForeignChairSkillEffect(this.getId()), false);
                    }
                    sendPacket(PacketCreator.cancelChair(itemId));
                }
            }
            else
            {    // stand up
                unsitChairInternal();
            }
        }
    }

    private void setChair(int chair)
    {
        this.chair.set(chair);
    }

    public void respawn(int returnMap)
    {
        respawn(null, returnMap);    // unspecified EIM, don't force EIM unregister in this case
    }

    public void respawn(EventInstanceManager? eim, int returnMap)
    {
        if (eim != null)
        {
            eim.unregisterPlayer(this);    // some event scripts uses this...
        }
        changeMap(returnMap);

        cancelAllBuffs(false);  // thanks Oblivium91 for finding out players still could revive in area and take damage before returning to town

        if (usedSafetyCharm)
        {  // thanks kvmba for noticing safety charm not providing 30% HP/MP
            addMPHP((int)Math.Ceiling(this.getClientMaxHp() * 0.3), (int)Math.Ceiling(this.getClientMaxMp() * 0.3));
        }
        else
        {
            updateHp(50);
        }

        setStance(0);
    }

    private void prepareDragonBlood(StatEffect bloodEffect)
    {
        if (dragonBloodSchedule != null)
        {
            dragonBloodSchedule.cancel(false);
        }
        dragonBloodSchedule = TimerManager.getInstance().register(() =>
        {
            if (awayFromWorld.Get())
            {
                return;
            }

            addHP(-bloodEffect.getX());
            sendPacket(PacketCreator.showOwnBuffEffect(bloodEffect.getSourceId(), 5));
            getMap().broadcastMessage(this, PacketCreator.showBuffEffect(getId(), bloodEffect.getSourceId(), 5), false);

        }, 4000, 4000);
    }

    private void recalcEquipStats()
    {
        if (equipchanged)
        {
            equipmaxhp = 0;
            equipmaxmp = 0;
            equipdex = 0;
            equipint_ = 0;
            equipstr = 0;
            equipluk = 0;
            equipmagic = 0;
            equipwatk = 0;
            //equipspeed = 0;
            //equipjump = 0;

            foreach (Item item in getInventory(InventoryType.EQUIPPED))
            {
                Equip equip = (Equip)item;
                equipmaxhp += equip.getHp();
                equipmaxmp += equip.getMp();
                equipdex += equip.getDex();
                equipint_ += equip.getInt();
                equipstr += equip.getStr();
                equipluk += equip.getLuk();
                equipmagic += equip.getMatk() + equip.getInt();
                equipwatk += equip.getWatk();
                //equipspeed += equip.getSpeed();
                //equipjump += equip.getJump();
            }

            equipchanged = false;
        }

        localmaxhp += equipmaxhp;
        localmaxmp += equipmaxmp;
        localdex += equipdex;
        localint_ += equipint_;
        localstr += equipstr;
        localluk += equipluk;
        localmagic += equipmagic;
        localwatk += equipwatk;
    }

    private void reapplyLocalStats()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        statLock.EnterWriteLock();
        try
        {
            localmaxhp = getMaxHp();
            localmaxmp = getMaxMp();
            localdex = getDex();
            localint_ = getInt();
            localstr = getStr();
            localluk = getLuk();
            localmagic = localint_;
            localwatk = 0;
            localchairrate = -1;

            recalcEquipStats();

            localmagic = Math.Min(localmagic, 2000);

            int? hbhp = getBuffedValue(BuffStat.HYPERBODYHP);
            if (hbhp != null)
            {
                localmaxhp += (hbhp.Value / 100) * localmaxhp;
            }
            int? hbmp = getBuffedValue(BuffStat.HYPERBODYMP);
            if (hbmp != null)
            {
                localmaxmp += (hbmp.Value / 100) * localmaxmp;
            }

            localmaxhp = Math.Min(30000, localmaxhp);
            localmaxmp = Math.Min(30000, localmaxmp);

            StatEffect? combo = getBuffEffect(BuffStat.ARAN_COMBO);
            if (combo != null)
            {
                localwatk += combo.getX();
            }

            if (energybar == 15000)
            {
                Skill energycharge = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.ENERGY_CHARGE) : SkillFactory.GetSkillTrust(Marauder.ENERGY_CHARGE);
                StatEffect ceffect = energycharge.getEffect(getSkillLevel(energycharge));
                localwatk += ceffect.getWatk();
            }

            int? mwarr = getBuffedValue(BuffStat.MAPLE_WARRIOR);
            if (mwarr != null)
            {
                localstr += getStr() * mwarr.Value / 100;
                localdex += getDex() * mwarr.Value / 100;
                localint_ += getInt() * mwarr.Value / 100;
                localluk += getLuk() * mwarr.Value / 100;
            }
            if (job.isA(Job.BOWMAN))
            {
                Skill? expert = null;
                if (job.isA(Job.MARKSMAN))
                {
                    expert = SkillFactory.getSkill(3220004);
                }
                else if (job.isA(Job.BOWMASTER))
                {
                    expert = SkillFactory.getSkill(3120005);
                }
                if (expert != null)
                {
                    int boostLevel = getSkillLevel(expert);
                    if (boostLevel > 0)
                    {
                        localwatk += expert.getEffect(boostLevel).getX();
                    }
                }
            }

            int? watkbuff = getBuffedValue(BuffStat.WATK);
            if (watkbuff != null)
            {
                localwatk += watkbuff.Value;
            }
            int? matkbuff = getBuffedValue(BuffStat.MATK);
            if (matkbuff != null)
            {
                localmagic += matkbuff.Value;
            }

            /*
            int speedbuff = getBuffedValue(BuffStat.SPEED);
            if (speedbuff != null) {
                localspeed += speedbuff;
            }
            int jumpbuff = getBuffedValue(BuffStat.JUMP);
            if (jumpbuff != null) {
                localjump += jumpbuff;
            }
            */

            int blessing = getSkillLevel(10000000 * getJobType() + 12);
            if (blessing > 0)
            {
                localwatk += blessing;
                localmagic += blessing * 2;
            }

            if (job.isA(Job.THIEF) || job.isA(Job.BOWMAN) || job.isA(Job.PIRATE) || job.isA(Job.NIGHTWALKER1) || job.isA(Job.WINDARCHER1))
            {
                Item? weapon_item = getInventory(InventoryType.EQUIPPED).getItem(-11);
                if (weapon_item != null)
                {
                    ItemInformationProvider ii = ItemInformationProvider.getInstance();
                    WeaponType weapon = ii.getWeaponType(weapon_item.getItemId());
                    bool bow = weapon == WeaponType.BOW;
                    bool crossbow = weapon == WeaponType.CROSSBOW;
                    bool claw = weapon == WeaponType.CLAW;
                    bool gun = weapon == WeaponType.GUN;
                    if (bow || crossbow || claw || gun)
                    {
                        // Also calc stars into this.
                        Inventory inv = getInventory(InventoryType.USE);
                        for (short i = 1; i <= inv.getSlotLimit(); i++)
                        {
                            Item? item = inv.getItem(i);
                            if (item != null)
                            {
                                if ((claw && ItemConstants.isThrowingStar(item.getItemId())) || (gun && ItemConstants.isBullet(item.getItemId())) || (bow && ItemConstants.isArrowForBow(item.getItemId())) || (crossbow && ItemConstants.isArrowForCrossBow(item.getItemId())))
                                {
                                    if (item.getQuantity() > 0)
                                    {
                                        // Finally there!
                                        localwatk += ii.getWatkForProjectile(item.getItemId());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                // Add throwing stars to dmg.
            }
        }
        finally
        {
            statLock.ExitWriteLock();
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    private List<KeyValuePair<Stat, int>> recalcLocalStats()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        statLock.EnterWriteLock();
        try
        {
            List<KeyValuePair<Stat, int>> hpmpupdate = new(2);
            int oldlocalmaxhp = localmaxhp;
            int oldlocalmaxmp = localmaxmp;

            reapplyLocalStats();

            if (YamlConfig.config.server.USE_FIXED_RATIO_HPMP_UPDATE)
            {
                if (localmaxhp != oldlocalmaxhp)
                {
                    KeyValuePair<Stat, int> hpUpdate;

                    if (transienthp == float.NegativeInfinity)
                    {
                        hpUpdate = calcHpRatioUpdate(localmaxhp, oldlocalmaxhp);
                    }
                    else
                    {
                        hpUpdate = calcHpRatioTransient();
                    }

                    hpmpupdate.Add(hpUpdate);
                }

                if (localmaxmp != oldlocalmaxmp)
                {
                    KeyValuePair<Stat, int> mpUpdate;

                    if (transientmp == float.NegativeInfinity)
                    {
                        mpUpdate = calcMpRatioUpdate(localmaxmp, oldlocalmaxmp);
                    }
                    else
                    {
                        mpUpdate = calcMpRatioTransient();
                    }

                    hpmpupdate.Add(mpUpdate);
                }
            }

            return hpmpupdate;
        }
        finally
        {
            statLock.ExitWriteLock();
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    private void updateLocalStats()
    {
        Monitor.Enter(prtLock);
        Monitor.Enter(effLock);
        statLock.EnterWriteLock();
        try
        {
            int oldmaxhp = localmaxhp;
            List<KeyValuePair<Stat, int>> hpmpupdate = recalcLocalStats();
            enforceMaxHpMp();

            if (hpmpupdate.Count > 0)
            {
                sendPacket(PacketCreator.updatePlayerStats(hpmpupdate, true, this));
            }

            if (oldmaxhp != localmaxhp)
            {   // thanks Wh1SK3Y (Suwaidy) for pointing out a deadlock occuring related to party members HP
                updatePartyMemberHP();
            }
        }
        finally
        {
            statLock.ExitWriteLock();
            Monitor.Exit(effLock);
            Monitor.Exit(prtLock);
        }
    }

    public void receivePartyMemberHP()
    {
        Monitor.Enter(prtLock);
        try
        {
            if (party != null)
            {
                foreach (Character partychar in this.getPartyMembersOnSameMap())
                {
                    sendPacket(PacketCreator.updatePartyMemberHP(partychar.getId(), partychar.getHp(), partychar.getCurrentMaxHp()));
                }
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public void removeAllCooldownsExcept(int id, bool packet)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            List<CooldownValueHolder> list = new(coolDowns.Values);
            foreach (CooldownValueHolder mcvh in list)
            {
                if (mcvh.skillId != id)
                {
                    coolDowns.Remove(mcvh.skillId);
                    if (packet)
                    {
                        sendPacket(PacketCreator.skillCooldown(mcvh.skillId, 0));
                    }
                }
            }
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public static void removeAriantRoom(int room)
    {
        ariantroomleader[room] = "";
        ariantroomslot[room] = 0;
    }

    public void removeCooldown(int skillId)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            this.coolDowns.Remove(skillId);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void removePet(Pet pet, bool shift_left)
    {
        Monitor.Enter(petLock);
        try
        {
            int slot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == pet.getUniqueId())
                    {
                        pets[i] = null;
                        slot = i;
                        break;
                    }
                }
            }
            if (shift_left)
            {
                if (slot > -1)
                {
                    for (int i = slot; i < 3; i++)
                    {
                        if (i != 2)
                        {
                            pets[i] = pets[i + 1];
                        }
                        else
                        {
                            pets[i] = null;
                        }
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public void removeVisibleMapObject(MapObject mo)
    {
        visibleMapObjects.Remove(mo);
    }

    object resetStateLock = new object();
    public void resetStats()
    {
        lock (resetStateLock)
        {


            if (!YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP)
            {
                return;
            }

            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                int tap = remainingAp + str + dex + int_ + luk, tsp = 1;
                int tstr = 4, tdex = 4, tint = 4, tluk = 4;

                switch (job.getId())
                {
                    case 100:
                    case 1100:
                    case 2100:
                        tstr = 35;
                        tsp += ((getLevel() - 10) * 3);
                        break;
                    case 200:
                    case 1200:
                        tint = 20;
                        tsp += ((getLevel() - 8) * 3);
                        break;
                    case 300:
                    case 1300:
                    case 400:
                    case 1400:
                        tdex = 25;
                        tsp += ((getLevel() - 10) * 3);
                        break;
                    case 500:
                    case 1500:
                        tdex = 20;
                        tsp += ((getLevel() - 10) * 3);
                        break;
                }

                tap -= tstr;
                tap -= tdex;
                tap -= tint;
                tap -= tluk;

                if (tap >= 0)
                {
                    updateStrDexIntLukSp(tstr, tdex, tint, tluk, tap, tsp, GameConstants.getSkillBook(job.getId()));
                }
                else
                {
                    log.Warning("Chr {CharacterId} tried to have its stats reset without enough AP available", this.id);
                }
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }
    }

    public void resetBattleshipHp()
    {
        int bshipLevel = Math.Max(getLevel() - 120, 0);  // thanks alex12 for noticing battleship HP issues for low-level players
        this.battleshipHp = 400 * getSkillLevel(SkillFactory.getSkill(Corsair.BATTLE_SHIP)) + (bshipLevel * 200);
    }

    public void resetEnteredScript()
    {
        entered.Remove(map.getId());
    }

    public void resetEnteredScript(int mapId)
    {
        entered.Remove(mapId);
    }

    public void resetEnteredScript(string script)
    {
        foreach (int mapId in entered.Keys)
        {
            if (entered[mapId].Equals(script))
            {
                entered.Remove(mapId);
            }
        }
    }

    object saveCdLock = new object();
    public void saveCooldowns()
    {
        lock (saveCdLock)
        {
            List<PlayerCoolDownValueHolder> listcd = getAllCooldowns();

            using var dbContext = new DBContext();
            if (listcd.Count > 0)
            {


                dbContext.Cooldowns.Where(x => x.Charid == getId()).ExecuteDelete();
                dbContext.Cooldowns.AddRange(listcd.Select(x => new Cooldown(getId(), x.skillId, x.length, x.startTime)));
            }
            var listds = getAllDiseases();
            if (listds.Count != 0)
            {
                dbContext.Playerdiseases.Where(x => x.Charid == getId()).ExecuteDelete();
                dbContext.Playerdiseases.AddRange(listds.Select(x =>
                {
                    var ms = x.Value.MobSkill.getId();
                    return new Playerdisease(getId(), x.Key.ordinal(), (int)ms.type, ms.level, (int)x.Value.LeftTime);
                }));
            }
        }
    }

    public void saveGuildStatus()
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Characters.Where(x => x.Id == getId())
                .ExecuteUpdate(x => x.SetProperty(y => y.GuildId, guildid).SetProperty(y => y.GuildRank, guildRank).SetProperty(y => y.AllianceRank, allianceRank));
        }
        catch (Exception se)
        {
            Log.Logger.Error(se.ToString());
        }
    }

    public void saveLocationOnWarp()
    {  // suggestion to remember the map before warp command thanks to Lei
        Portal? closest = map.findClosestPortal(getPosition());
        int curMapid = getMapId();

        for (int i = 0; i < savedLocations.Length; i++)
        {
            if (savedLocations[i] == null)
            {
                savedLocations[i] = new SavedLocation(curMapid, closest != null ? closest.getId() : 0);
            }
        }
    }

    public void saveLocation(string type)
    {
        Portal? closest = map.findClosestPortal(getPosition());
        savedLocations[(int)SavedLocationTypeUtils.fromString(type)] = new SavedLocation(getMapId(), closest != null ? closest.getId() : 0);
    }

    public bool insertNewChar(CharacterFactoryRecipe recipe)
    {
        str = recipe.getStr();
        dex = recipe.getDex();
        int_ = recipe.getInt();
        luk = recipe.getLuk();
        setMaxHp(recipe.getMaxHp());
        setMaxMp(recipe.getMaxMp());
        hp = maxhp;
        mp = maxmp;
        level = recipe.getLevel();
        remainingAp = recipe.getRemainingAp();
        remainingSp[GameConstants.getSkillBook(job.getId())] = recipe.getRemainingSp();
        mapid = recipe.getMap();
        meso.set(recipe.getMeso());

        List<KeyValuePair<Skill, int>> startingSkills = recipe.getStartingSkillLevel();
        foreach (KeyValuePair<Skill, int> skEntry in startingSkills)
        {
            Skill skill = skEntry.Key;
            this.changeSkillLevel(skill, (sbyte)skEntry.Value, skill.getMaxLevel(), -1);
        }

        List<ItemInventoryType> itemsWithType = recipe.getStartingItems();
        foreach (var itEntry in itemsWithType)
        {
            this.getInventory(itEntry.Type).addItem(itEntry.Item);
        }

        this.events.AddOrUpdate("rescueGaga", new RescueGaga(0));


        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();

            var dbModel = new DB_Character(accountid, 
                world, 
                name, 
                level, 
                exp, 
                gachaexp, 
                str, dex, luk, int_, hp, mp, maxhp, maxmp, 
                Math.Abs(meso.get()), 
                getJob().getId(), 
                (int)skinColor, 
                gender, hair, face, 
                mapid, 
                0, 
                (sbyte)_gmLevel, 
                remainingAp, 
                string.Join(',', remainingSp));
            dbContext.Characters.Add(dbModel);
            int updateRows = dbContext.SaveChanges();
            if (updateRows < 1)
            {
                log.Error("Error trying to insert " + name);
                return false;
            }
            this.id = dbModel.Id;
            // Select a keybinding method
            int[] selectedKey;
            int[] selectedType;
            int[] selectedAction;

            if (YamlConfig.config.server.USE_CUSTOM_KEYSET)
            {
                selectedKey = GameConstants.getCustomKey(true);
                selectedType = GameConstants.getCustomType(true);
                selectedAction = GameConstants.getCustomAction(true);
            }
            else
            {
                selectedKey = GameConstants.getCustomKey(false);
                selectedType = GameConstants.getCustomType(false);
                selectedAction = GameConstants.getCustomAction(false);
            }

            // Key config
            List<Keymap> keyMapData = new List<Keymap>();
            for (int i = 0; i < selectedKey.Length; i++)
            {
                keyMapData.Add(new Keymap
                {
                    Characterid = dbModel.Id,
                    Key = selectedKey[i],
                    Type = selectedType[i],
                    Action = selectedAction[i],
                });
            }
            dbContext.Keymaps.AddRange(keyMapData);
            dbContext.SaveChanges();

            // No quickslots, or no change.
            bool bQuickslotEquals = this.m_pQuickslotKeyMapped == null || (this.m_aQuickslotLoaded != null && Arrays.Equals(this.m_pQuickslotKeyMapped.GetKeybindings(), this.m_aQuickslotLoaded));
            if (!bQuickslotEquals)
            {
                long nQuickslotKeymapped = LongTool.BytesToLong(this.m_pQuickslotKeyMapped!.GetKeybindings());

                var accountKeyMapped = dbContext.Quickslotkeymappeds.FirstOrDefault(x => x.Accountid == getAccountID());
                if (accountKeyMapped == null)
                {
                    accountKeyMapped = new Quickslotkeymapped { Accountid = getAccountID(), Keymap = nQuickslotKeymapped };
                    dbContext.Quickslotkeymappeds.Add(accountKeyMapped);
                }
                else
                {
                    accountKeyMapped.Keymap = nQuickslotKeymapped;
                }
                dbContext.SaveChanges();
            }

            itemsWithType = new();
            foreach (Inventory iv in inventory)
            {
                foreach (Item item in iv.list())
                {
                    itemsWithType.Add(new(item, iv.getType()));
                }
            }

            ItemFactory.INVENTORY.saveItems(itemsWithType, id, dbContext);

            if (skills.Count > 0)
            {
                // Skills
                dbContext.Skills.AddRange(skills.Select(x => new DB_Skill { Characterid = dbModel.Id, Skillid = x.Key.getId(), Skilllevel = x.Value.skillevel, Masterlevel = x.Value.masterlevel, Expiration = x.Value.expiration }));
                dbContext.SaveChanges();
            }


            dbTrans.Commit();
            return true;
        }
        catch (Exception t)
        {
            log.Error(t, "Error creating chr {CharacterName}, level: {Level}, job: {JobId}", name, level, job.getId());
            return false;
        }
    }

    public void saveCharToDB()
    {
        if (YamlConfig.config.server.USE_AUTOSAVE)
        {
            CharacterSaveService service = (CharacterSaveService)getWorldServer()!.getServiceAccess(WorldServices.SAVE_CHARACTER);
            service.registerSaveCharacter(this.getId(), () =>
            {
                saveCharToDB(true);
            });
        }
        else
        {
            saveCharToDB(true);
        }
    }

    //ItemFactory saveItems and monsterbook.saveCards are the most time consuming here.

    object saveCharLock = new object();
    public void saveCharToDB(bool notAutosave)
    {
        lock (saveCharLock)
        {
            if (!loggedIn)
            {
                return;
            }

            log.Debug("Attempting to {0} chr {CharacterName}", notAutosave ? "save" : "autosave", name);

            Server.getInstance().updateCharacterEntry(this);

            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            var entity = dbContext.Characters.FirstOrDefault(x => x.Id == getId());
            if (entity == null)
                throw new BusinessDataNullException();

            try
            {
                entity.Level = level;
                entity.Fame = fame;

                Monitor.Enter(effLock);
                statLock.EnterWriteLock();
                try
                {
                    entity.Str = str;
                    entity.Dex = dex;
                    entity.Luk = luk;
                    entity.Int = int_;
                    entity.Exp = exp.get();
                    entity.Gachaexp = gachaexp.get();
                    entity.Hp = hp;
                    entity.Mp = mp;
                    entity.Maxhp = maxhp;
                    entity.Maxmp = maxmp;
                    entity.Sp = string.Join(',', remainingSp);
                    entity.Ap = remainingAp;
                }
                finally
                {
                    statLock.ExitWriteLock();
                    Monitor.Exit(effLock);
                }

                entity.Gm = (sbyte)_gmLevel;
                entity.Job = job.getId();
                entity.Hair = hair;
                entity.Face = face;
                if (map == null || (cashshop != null && cashshop.isOpened()))
                {
                    entity.Map = mapid;
                }
                else
                {
                    if (map.getForcedReturnId() != 999999999)
                    {
                        entity.Map = map.getForcedReturnId();
                    }
                    else
                    {
                        entity.Map = getHp() < 1 ? map.getReturnMapId() : map.getId();
                    }
                }
                entity.Meso = meso.get();
                entity.HpMpUsed = hpMpApUsed;
                if (map == null || map.getId() == 610020000 || map.getId() == 610020001)
                {  // reset to first spawnpoint on those maps
                    entity.Spawnpoint = 0;
                }
                else
                {
                    Portal? closest = map.findClosestPlayerSpawnpoint(getPosition());
                    if (closest != null)
                    {
                        entity.Spawnpoint = closest.getId();
                    }
                    else
                    {
                        entity.Spawnpoint = 0;
                    }
                }

                Monitor.Enter(prtLock);
                try
                {
                    entity.Party = party?.getId() ?? -1;
                }
                finally
                {
                    Monitor.Exit(prtLock);
                }

                entity.BuddyCapacity = buddylist.getCapacity();
                entity.MessengerId = messenger?.getId() ?? 0;
                entity.MessengerPosition = messenger == null ? 4 : messengerposition;

                entity.MountLevel = maplemount?.getLevel() ?? 1;
                entity.MountExp = maplemount?.getExp() ?? 0;
                entity.Mounttiredness = maplemount?.getTiredness() ?? 0;

                entity.Equipslots = getSlots(1);
                entity.Useslots = getSlots(2);
                entity.Setupslots = getSlots(3);
                entity.Etcslots = getSlots(4);

                monsterbook.saveCards(dbContext, getId());
                entity.Monsterbookcover = bookCover;
                entity.VanquisherStage = vanquisherStage;
                entity.DojoPoints = dojoPoints;
                entity.LastDojoStage = dojoStage;
                entity.FinishedDojoTutorial = finishedDojoTutorial;
                entity.VanquisherKills = vanquisherKills;
                entity.Matchcardwins = matchcardwins;
                entity.Matchcardlosses = matchcardlosses;
                entity.Matchcardties = matchcardties;
                entity.Omokwins = omokwins;
                entity.Omoklosses = omoklosses;
                entity.Omokties = omokties;
                entity.DataString = dataString;
                entity.Fquest = quest_fame;
                entity.Jailexpire = jailExpiration;
                entity.PartnerId = partnerId;
                entity.MarriageItemId = marriageItemid;
                entity.LastExpGainTime = lastExpGainTime;
                entity.AriantPoints = ariantPoints;
                entity.PartySearch = canRecvPartySearchInvite;
                dbContext.SaveChanges();

                List<Pet> petList = new();
                Monitor.Enter(petLock);
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (pets[i] != null)
                        {
                            petList.Add(pets[i]!);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(petLock);
                }

                foreach (Pet pet in petList)
                {
                    pet.saveToDb();
                }

                var ignoresPetIds = getExcluded().Select(x => x.Key).ToList();
                dbContext.Petignores.Where(x => ignoresPetIds.Contains(x.Petid)).ExecuteDelete();
                dbContext.Petignores.AddRange(getExcluded().SelectMany(x => x.Value.Select(y => new Petignore() { Petid = x.Key, Itemid = y })).ToList());
                dbContext.SaveChanges();


                dbContext.Keymaps.Where(x => x.Characterid == getId()).ExecuteDelete();
                dbContext.Keymaps.AddRange(keymap.Select(x => new Keymap() { Characterid = id, Key = x.Key, Type = x.Value.getType(), Action = x.Value.getAction() }));
                dbContext.SaveChanges();

                // No quickslots, or no change.
                bool bQuickslotEquals = this.m_pQuickslotKeyMapped == null || (this.m_aQuickslotLoaded != null && Arrays.Equals(this.m_pQuickslotKeyMapped.GetKeybindings(), this.m_aQuickslotLoaded));
                if (!bQuickslotEquals)
                {
                    long nQuickslotKeymapped = LongTool.BytesToLong(this.m_pQuickslotKeyMapped.GetKeybindings());
                    var m = dbContext.Quickslotkeymappeds.Where(x => x.Accountid == getAccountID()).FirstOrDefault();
                    if (m == null)
                    {
                        m = new Quickslotkeymapped() { Accountid = getAccountID(), Keymap = nQuickslotKeymapped };
                        dbContext.Quickslotkeymappeds.Add(m);
                    }
                    else
                    {
                        m.Keymap = nQuickslotKeymapped;
                    }
                    dbContext.SaveChanges();
                }


                dbContext.Skillmacros.Where(x => x.Characterid == getId()).ExecuteDelete();
                for (int i = 0; i < 5; i++)
                {
                    SkillMacro macro = skillMacros[i];
                    if (macro != null)
                    {
                        dbContext.Skillmacros.Add(new Skillmacro(getId(), (sbyte)macro.getPosition(), macro.getSkill1(), macro.getSkill2(), macro.getSkill3(), macro.getName(), (sbyte)macro.getShout()));
                    }
                }
                dbContext.SaveChanges();

                List<ItemInventoryType> itemsWithType = new();
                foreach (Inventory iv in inventory)
                {
                    foreach (Item item in iv.list())
                    {
                        itemsWithType.Add(new(item, iv.getType()));
                    }
                }

                ItemFactory.INVENTORY.saveItems(itemsWithType, id, dbContext);

                var characterSkills = dbContext.Skills.Where(x => x.Characterid == getId()).ToList();
                foreach (var skill in skills)
                {
                    var dbSkill = characterSkills.FirstOrDefault(x => x.Skillid == skill.Key.getId());
                    if (dbSkill == null)
                    {
                        dbSkill = new DB_Skill() { Characterid = getId(), Skillid = skill.Key.getId() };
                        dbContext.Skills.Add(dbSkill);
                    }
                    dbSkill.Skilllevel = skill.Value.skillevel;
                    dbSkill.Masterlevel = skill.Value.masterlevel;
                    dbSkill.Expiration = skill.Value.expiration;
                }
                dbContext.SaveChanges();

                dbContext.Savedlocations.Where(x => x.Characterid == getId()).ExecuteDelete();
                dbContext.Savedlocations.AddRange(
                    Enum.GetValues<SavedLocationType>()
                    .Where(x => savedLocations[(int)x] != null)
                    .Select(x => new Savedlocation(savedLocations[(int)x]!.getMapId(), savedLocations[(int)x]!.getPortal()))
                    );
                dbContext.SaveChanges();

                dbContext.Trocklocations.Where(x => x.Characterid == getId()).ExecuteDelete();
                for (int i = 0; i < getTrockSize(); i++)
                {
                    if (trockmaps[i] != 999999999)
                    {
                        dbContext.Trocklocations.Add(new Trocklocation(getId(), trockmaps[i], 0));
                    }
                }

                for (int i = 0; i < getVipTrockSize(); i++)
                {
                    if (viptrockmaps[i] != 999999999)
                    {
                        dbContext.Trocklocations.Add(new Trocklocation(getId(), viptrockmaps[i], 1));
                    }
                }
                dbContext.SaveChanges();

                dbContext.Buddies.Where(x => x.CharacterId == getId() && x.Pending == 0).ExecuteDelete();
                foreach (BuddylistEntry entry in buddylist.getBuddies())
                {
                    if (entry.isVisible())
                    {
                        dbContext.Buddies.Add(new Buddy(getId(), entry.getCharacterId(), 0, entry.getGroup()));
                    }
                }
                dbContext.SaveChanges();

                dbContext.AreaInfos.Where(x => x.Charid == getId()).ExecuteDelete();
                dbContext.AreaInfos.AddRange(area_info.Select(x => new AreaInfo(getId(), x.Key, x.Value)));
                dbContext.SaveChanges();

                dbContext.Eventstats.Where(x => x.Characterid == getId()).ExecuteDelete();
                dbContext.Eventstats.AddRange(events.Select(x => new Eventstat(getId(), x.Key, x.Value.getInfo())));
                dbContext.SaveChanges();

                deleteQuestProgressWhereCharacterId(dbContext, id);


                foreach (QuestStatus qs in getQuests())
                {
                    var questStatus = new Queststatus(getId(), qs.getQuest().getId(), (int)qs.getStatus(), (int)(qs.getCompletionTime() / 1000), qs.getExpirationTime(),
                       qs.getForfeited(), qs.getCompleted());
                    dbContext.Queststatuses.Add(questStatus);
                    dbContext.SaveChanges();

                    foreach (int mob in qs.getProgress().Keys)
                    {
                        dbContext.Questprogresses.Add(new Questprogress(getId(), questStatus.Queststatusid, mob, qs.getProgress(mob)));
                    }
                    foreach (var item in qs.getMedalMaps())
                    {
                        dbContext.Medalmaps.Add(new Medalmap(getId(), questStatus.Queststatusid, item));
                    }
                }
                dbContext.SaveChanges();

                FamilyEntry? familyEntry = getFamilyEntry(); //save family rep
                if (familyEntry != null)
                {
                    if (familyEntry.saveReputation(dbContext))
                        familyEntry.savedSuccessfully();
                    FamilyEntry? senior = familyEntry.getSenior();
                    if (senior != null && senior.getChr() == null)
                    { //only save for offline family members
                        if (senior.saveReputation(dbContext))
                            senior.savedSuccessfully();

                        senior = senior.getSenior(); //save one level up as well
                        if (senior != null && senior.getChr() == null)
                        {
                            if (senior.saveReputation(dbContext))
                                senior.savedSuccessfully();
                        }
                    }

                }

                if (cashshop != null)
                {
                    cashshop.save(dbContext);
                }

                if (storage != null && usedStorage)
                {
                    storage.saveToDB(dbContext);
                    usedStorage = false;
                }

                dbTrans.Commit();

            }
            catch (Exception e)
            {
                log.Error(e, "Error saving chr {CharacterName}, level: {Level}, job: {JobId}", name, level, job.getId());
            }
        }
    }

    public void sendPolice(int greason, string reason, int duration)
    {
        sendPacket(PacketCreator.sendPolice(string.Format("You have been blocked by the#b {0} Police for {1}.#k", "Cosmic", reason)));
        this.isbanned = true;
        TimerManager.getInstance().schedule(() =>
        {
            client.disconnect(false, false);
        }, duration);
    }

    public void sendPolice(string text)
    {
        string message = getName() + " received this - " + text;
        if (Server.getInstance().isGmOnline(this.getWorld()))
        { //Alert and log if a GM is online
            Server.getInstance().broadcastGMMessage(this.getWorld(), PacketCreator.sendYellowTip(message));
        }
        else
        { //Auto DC and log if no GM is online
            client.disconnect(false, false);
        }
        log.Information(message);
        //Server.getInstance().broadcastGMMessage(0, PacketCreator.serverNotice(1, getName() + " received this - " + text));
        //sendPacket(PacketCreator.sendPolice(text));
        //this.isbanned = true;
        //TimerManager.getInstance().schedule(new Runnable() {
        //    public override    public void run() {
        //        client.disconnect(false, false);
        //    }
        //}, 6000);
    }

    public void sendKeymap()
    {
        sendPacket(PacketCreator.getKeymap(keymap));
    }

    public void sendQuickmap()
    {
        // send quickslots to user
        QuickslotBinding pQuickslotKeyMapped = this.m_pQuickslotKeyMapped;

        if (pQuickslotKeyMapped == null)
        {
            pQuickslotKeyMapped = new QuickslotBinding(QuickslotBinding.DEFAULT_QUICKSLOTS);
        }

        this.sendPacket(PacketCreator.QuickslotMappedInit(pQuickslotKeyMapped));
    }

    public void sendMacros()
    {
        // Always send the macro packet to fix a client side bug when switching characters.
        sendPacket(PacketCreator.getMacros(skillMacros));
    }

    public SkillMacro[] getMacros()
    {
        return skillMacros;
    }

    public static void setAriantRoomLeader(int room, string charname)
    {
        ariantroomleader[room] = charname;
    }

    public static void setAriantSlotRoom(int room, int slot)
    {
        ariantroomslot[room] = slot;
    }

    public void setBattleshipHp(int battleshipHp)
    {
        this.battleshipHp = battleshipHp;
    }

    public void setBuddyCapacity(int capacity)
    {
        buddylist.setCapacity(capacity);
        sendPacket(PacketCreator.updateBuddyCapacity(capacity));
    }

    public void setBuffedValue(BuffStat effect, int value)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(effect);
            if (mbsvh == null)
            {
                return;
            }
            mbsvh.value = value;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void setChalkboard(string? text)
    {
        this.chalktext = text;
    }

    public void setDojoEnergy(int x)
    {
        this.dojoEnergy = Math.Min(x, 10000);
    }

    public void setDojoPoints(int x)
    {
        this.dojoPoints = x;
    }

    public void setDojoStage(int x)
    {
        this.dojoStage = x;
    }

    public void setEnergyBar(int set)
    {
        energybar = set;
    }

    public void setEventInstance(EventInstanceManager? eventInstance)
    {
        Monitor.Enter(evtLock);
        try
        {
            this.eventInstance = eventInstance;
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    public void setExp(int amount)
    {
        this.exp.set(amount);
    }

    public void setGachaExp(int amount)
    {
        this.gachaexp.set(amount);
    }

    public void setFace(int face)
    {
        this.face = face;
    }

    public void setFame(int fame)
    {
        this.fame = fame;
    }

    public void setFamilyId(int familyId)
    {
        this.familyId = familyId;
    }

    public void setFinishedDojoTutorial()
    {
        this.finishedDojoTutorial = true;
    }

    public void setGender(int gender)
    {
        this.gender = gender;
    }

    public void setGM(int level)
    {
        this._gmLevel = level;
    }

    public void setGuildId(int _id)
    {
        guildid = _id;
    }

    public void setGuildRank(int _rank)
    {
        guildRank = _rank;
    }

    public void setAllianceRank(int _rank)
    {
        allianceRank = _rank;
    }

    public void setHair(int hair)
    {
        this.hair = hair;
    }

    public void setHasMerchant(bool set)
    {
        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.HasMerchant, set));
        _hasMerchant = set;
    }

    public void addMerchantMesos(int add)
    {
        int newAmount = (int)Math.Min((long)merchantmeso + add, int.MaxValue);

        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.MerchantMesos, newAmount));


        merchantmeso = newAmount;
    }

    public void setMerchantMeso(int set)
    {

        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.MerchantMesos, set));

        merchantmeso = set;
    }

    object withDrawMerchantLock = new object();
    public void withdrawMerchantMesos()
    {
        lock (withDrawMerchantLock)
        {
            int merchantMeso = this.getMerchantNetMeso();
            int playerMeso = this.getMeso();

            if (merchantMeso > 0)
            {
                int possible = int.MaxValue - playerMeso;

                if (possible > 0)
                {
                    if (possible < merchantMeso)
                    {
                        this.gainMeso(possible, false);
                        this.setMerchantMeso(merchantMeso - possible);
                    }
                    else
                    {
                        this.gainMeso(merchantMeso, false);
                        this.setMerchantMeso(0);
                    }
                }
            }
            else
            {
                int nextMeso = playerMeso + merchantMeso;

                if (nextMeso < 0)
                {
                    this.gainMeso(-playerMeso, false);
                    this.setMerchantMeso(merchantMeso + playerMeso);
                }
                else
                {
                    this.gainMeso(merchantMeso, false);
                    this.setMerchantMeso(0);
                }
            }
        }

    }

    public void setHiredMerchant(HiredMerchant? merchant)
    {
        this.hiredMerchant = merchant;
    }

    private void hpChangeAction(int oldHp)
    {
        bool playerDied = false;
        if (hp <= 0)
        {
            if (oldHp > hp)
            {
                playerDied = true;
            }
        }

        bool chrDied = playerDied;
        if (map != null)
        {
            map.registerCharacterStatUpdate(() =>
            {
                updatePartyMemberHP();    // thanks BHB (BHB88) for detecting a deadlock case within player stats.

                if (chrDied)
                {
                    playerDead();
                }
                else
                {
                    checkBerserk(isHidden());
                }
            });
        }
    }

    private KeyValuePair<Stat, int> calcHpRatioUpdate(int newHp, int oldHp)
    {
        int delta = newHp - oldHp;
        this.hp = calcHpRatioUpdate(hp, oldHp, delta);

        hpChangeAction(short.MinValue);
        return new(Stat.HP, hp);
    }

    private KeyValuePair<Stat, int> calcMpRatioUpdate(int newMp, int oldMp)
    {
        int delta = newMp - oldMp;
        this.mp = calcMpRatioUpdate(mp, oldMp, delta);
        return new(Stat.MP, mp);
    }

    private static int calcTransientRatio(float transientpoint)
    {
        int ret = (int)transientpoint;
        return !(ret <= 0 && transientpoint > 0.0f) ? ret : 1;
    }

    private KeyValuePair<Stat, int> calcHpRatioTransient()
    {
        this.hp = calcTransientRatio(transienthp * localmaxhp);

        hpChangeAction(short.MinValue);
        return new(Stat.HP, hp);
    }

    private KeyValuePair<Stat, int> calcMpRatioTransient()
    {
        this.mp = calcTransientRatio(transientmp * localmaxmp);
        return new(Stat.MP, mp);
    }

    private int calcHpRatioUpdate(int curpoint, int maxpoint, int diffpoint)
    {
        int curMax = maxpoint;
        int nextMax = Math.Min(30000, maxpoint + diffpoint);

        float temp = curpoint * nextMax;
        int ret = (int)Math.Ceiling(temp / curMax);

        transienthp = (maxpoint > nextMax) ? ((float)curpoint) / maxpoint : ((float)ret) / nextMax;
        return ret;
    }

    private int calcMpRatioUpdate(int curpoint, int maxpoint, int diffpoint)
    {
        int curMax = maxpoint;
        int nextMax = Math.Min(30000, maxpoint + diffpoint);

        float temp = curpoint * nextMax;
        int ret = (int)Math.Ceiling(temp / curMax);

        transientmp = (maxpoint > nextMax) ? ((float)curpoint) / maxpoint : ((float)ret) / nextMax;
        return ret;
    }

    public bool applyHpMpChange(int hpCon, int hpchange, int mpchange)
    {
        bool zombify = hasDisease(Disease.ZOMBIFY);

        Monitor.Enter(effLock);
        statLock.EnterWriteLock();
        try
        {
            int nextHp = hp + hpchange, nextMp = mp + mpchange;
            bool cannotApplyHp = hpchange != 0 && nextHp <= 0 && (!zombify || hpCon > 0);
            bool cannotApplyMp = mpchange != 0 && nextMp < 0;

            if (cannotApplyHp || cannotApplyMp)
            {
                if (!isGM())
                {
                    return false;
                }

                if (cannotApplyHp)
                {
                    nextHp = 1;
                }
            }

            updateHpMp(nextHp, nextMp);
        }
        finally
        {
            statLock.ExitWriteLock();
            Monitor.Exit(effLock);
        }

        // autopot on HPMP deplete... thanks shavit for finding out D. Roar doesn't trigger autopot request
        if (hpchange < 0)
        {
            KeyBinding? autohpPot = this.getKeymap().GetValueOrDefault(91);
            if (autohpPot != null)
            {
                int autohpItemid = autohpPot.getAction();
                float autohpAlert = this.getAutopotHpAlert();
                if (((float)this.getHp()) / this.getCurrentMaxHp() <= autohpAlert)
                { // try within user settings... thanks Lame, Optimist, Stealth2800
                    Item autohpItem = this.getInventory(InventoryType.USE).findById(autohpItemid);
                    if (autohpItem != null)
                    {
                        this.setAutopotHpAlert(0.9f * autohpAlert);
                        PetAutopotProcessor.runAutopotAction(client, autohpItem.getPosition(), autohpItemid);
                    }
                }
            }
        }

        if (mpchange < 0)
        {
            KeyBinding? autompPot = this.getKeymap().GetValueOrDefault(92);
            if (autompPot != null)
            {
                int autompItemid = autompPot.getAction();
                float autompAlert = this.getAutopotMpAlert();
                if (((float)this.getMp()) / this.getCurrentMaxMp() <= autompAlert)
                {
                    Item autompItem = this.getInventory(InventoryType.USE).findById(autompItemid);
                    if (autompItem != null)
                    {
                        this.setAutopotMpAlert(0.9f * autompAlert); // autoMP would stick to using pots at every depletion in some cases... thanks Rohenn
                        PetAutopotProcessor.runAutopotAction(client, autompItem.getPosition(), autompItemid);
                    }
                }
            }
        }

        return true;
    }

    public void setInventory(InventoryType type, Inventory inv)
    {
        inventory[type.ordinal()] = inv;
    }

    public void setItemEffect(int itemEffect)
    {
        this.itemEffect = itemEffect;
    }

    public void setJob(Job job)
    {
        this.job = job;
    }

    public void setLastHealed(long time)
    {
        this.lastHealed = time;
    }

    public void setLastUsedCashItem(long time)
    {
        this.lastUsedCashItem = time;
    }

    public void setLevel(int level)
    {
        this.level = level;
    }

    public void setMap(int PmapId)
    {
        this.mapid = PmapId;
    }

    public void setMessenger(Messenger? messenger)
    {
        this.messenger = messenger;
    }

    public void setMessengerPosition(int position)
    {
        this.messengerposition = position;
    }

    public void setMiniGame(MiniGame miniGame)
    {
        this.miniGame = miniGame;
    }

    public void setMiniGamePoints(Character visitor, int winnerslot, bool omok)
    {
        if (omok)
        {
            if (winnerslot == 1)
            {
                this.omokwins++;
                visitor.omoklosses++;
            }
            else if (winnerslot == 2)
            {
                visitor.omokwins++;
                this.omoklosses++;
            }
            else
            {
                this.omokties++;
                visitor.omokties++;
            }
        }
        else
        {
            if (winnerslot == 1)
            {
                this.matchcardwins++;
                visitor.matchcardlosses++;
            }
            else if (winnerslot == 2)
            {
                visitor.matchcardwins++;
                this.matchcardlosses++;
            }
            else
            {
                this.matchcardties++;
                visitor.matchcardties++;
            }
        }
    }

    public void setMonsterBookCover(int bookCover)
    {
        this.bookCover = bookCover;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public void setRPS(RockPaperScissor? rps)
    {
        this.rps = rps;
    }

    public void closeRPS()
    {
        RockPaperScissor? rps = this.rps;
        if (rps != null)
        {
            rps.dispose(client);
            setRPS(null);
        }
    }

    public int getDoorSlot()
    {
        if (doorSlot != -1)
        {
            return doorSlot;
        }
        return fetchDoorSlot();
    }

    public int fetchDoorSlot()
    {
        Monitor.Enter(prtLock);
        try
        {
            doorSlot = party?.getPartyDoor(this.getId()) ?? 0;
            return doorSlot;
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public void setParty(Party? p)
    {
        Monitor.Enter(prtLock);
        try
        {
            if (p == null)
            {
                this.mpc = null;
                doorSlot = -1;

                party = null;
            }
            else
            {
                party = p;
            }
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    public void setPlayerShop(PlayerShop? playerShop)
    {
        this.playerShop = playerShop;
    }

    public void setSearch(string find)
    {
        search = find;
    }

    public void setSkinColor(SkinColor skinColor)
    {
        this.skinColor = skinColor;
    }

    public byte getSlots(int type)
    {
        return (byte)(type == InventoryType.CASH.getType() ? 96 : inventory[type].getSlotLimit());
    }

    public bool canGainSlots(int type, int slots)
    {
        slots += inventory[type].getSlotLimit();
        return slots <= 96;
    }

    public bool gainSlots(int type, int slots)
    {
        return gainSlots(type, slots, true);
    }

    public bool gainSlots(int type, int slots, bool update)
    {
        int newLimit = gainSlotsInternal(type, slots);
        if (newLimit != -1)
        {
            this.saveCharToDB();
            if (update)
            {
                sendPacket(PacketCreator.updateInventorySlotLimit(type, newLimit));
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private int gainSlotsInternal(int type, int slots)
    {
        inventory[type].lockInventory();
        try
        {
            if (canGainSlots(type, slots))
            {
                int newLimit = inventory[type].getSlotLimit() + slots;
                inventory[type].setSlotLimit(newLimit);
                return newLimit;
            }
            else
            {
                return -1;
            }
        }
        finally
        {
            inventory[type].unlockInventory();
        }
    }

    public int sellAllItemsFromName(sbyte invTypeId, string name)
    {
        //player decides from which inventory items should be sold.
        InventoryType type = InventoryTypeUtils.getByType(invTypeId);

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            Item it = inv.findByName(name);
            if (it == null)
            {
                return (-1);
            }

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            return (sellAllItemsFromPosition(ii, type, it.getPosition()));
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    public int sellAllItemsFromPosition(ItemInformationProvider ii, InventoryType type, short pos)
    {
        int mesoGain = 0;

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            for (short i = pos; i <= inv.getSlotLimit(); i++)
            {
                if (inv.getItem(i) == null)
                {
                    continue;
                }
                mesoGain += standaloneSell(getClient(), ii, type, i, inv.getItem(i)!.getQuantity());
            }
        }
        finally
        {
            inv.unlockInventory();
        }

        return (mesoGain);
    }

    private int standaloneSell(Client c, ItemInformationProvider ii, InventoryType type, short slot, short quantity)
    {
        // quantity == 0xFFFF || 这里quantity永远小于0xFFFF，有什么意义？
        if (quantity == 0)
        {
            quantity = 1;
        }

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            Item? item = inv.getItem(slot);
            if (item == null)
            { //Basic check
                return (0);
            }

            int itemid = item.getItemId();
            if (ItemConstants.isRechargeable(itemid))
            {
                quantity = item.getQuantity();
            }
            else if (ItemId.isWeddingToken(itemid) || ItemId.isWeddingRing(itemid))
            {
                return (0);
            }

            if (quantity < 0)
            {
                return (0);
            }
            short iQuant = item.getQuantity();
            if (iQuant == 0xFFFF)
            {
                iQuant = 1;
            }

            if (quantity <= iQuant && iQuant > 0)
            {
                InventoryManipulator.removeFromSlot(c, type, (byte)slot, quantity, false);
                int recvMesos = ii.getPrice(itemid, quantity);
                if (recvMesos > 0)
                {
                    gainMeso(recvMesos, false);
                    return (recvMesos);
                }
            }

            return (0);
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    private static bool hasMergeFlag(Item item)
    {
        return (item.getFlag() & ItemConstants.MERGE_UNTRADEABLE) == ItemConstants.MERGE_UNTRADEABLE;
    }

    private static void setMergeFlag(Item item)
    {
        short flag = item.getFlag();
        flag |= ItemConstants.MERGE_UNTRADEABLE;
        flag |= ItemConstants.UNTRADEABLE;
        item.setFlag(flag);
    }

    private List<Equip> getUpgradeableEquipped()
    {
        List<Equip> list = new();

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (Item item in getInventory(InventoryType.EQUIPPED))
        {
            if (ii.isUpgradeable(item.getItemId()))
            {
                list.Add((Equip)item);
            }
        }

        return list;
    }

    private static List<Equip> getEquipsWithStat(List<KeyValuePair<Equip, Dictionary<StatUpgrade, int>>> equipped, StatUpgrade stat)
    {
        List<Equip> equippedWithStat = new();

        foreach (var eq in equipped)
        {
            if (eq.Value.ContainsKey(stat))
            {
                equippedWithStat.Add(eq.Key);
            }
        }

        return equippedWithStat;
    }

    public bool mergeAllItemsFromName(string name)
    {
        InventoryType type = InventoryType.EQUIP;

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            Item it = inv.findByName(name);
            if (it == null)
            {
                return false;
            }

            Dictionary<StatUpgrade, float> statups = new();
            mergeAllItemsFromPosition(statups, it.getPosition());

            List<KeyValuePair<Equip, Dictionary<StatUpgrade, int>>> upgradeableEquipped = new();
            Dictionary<Equip, List<KeyValuePair<StatUpgrade, int>>> equipUpgrades = new();
            foreach (Equip eq in getUpgradeableEquipped())
            {
                upgradeableEquipped.Add(new(eq, eq.getStats()));
                equipUpgrades.AddOrUpdate(eq, new());
            }

            /*
            foreach(Entry<StatUpgrade, float> es in statups) {
                Console.WriteLine(es);
            }
            */

            foreach (var e in statups)
            {
                double ev = Math.Sqrt(e.Value);

                HashSet<Equip> extraEquipped = new(equipUpgrades.Keys);
                List<Equip> statEquipped = getEquipsWithStat(upgradeableEquipped, e.Key);
                float extraRate = (float)(0.2 * Randomizer.nextDouble());

                if (statEquipped.Count > 0)
                {
                    float statRate = 1.0f - extraRate;

                    int statup = (int)Math.Ceiling((ev * statRate) / statEquipped.Count);
                    foreach (Equip statEq in statEquipped)
                    {
                        equipUpgrades.GetValueOrDefault(statEq)?.Add(new(e.Key, statup));
                        extraEquipped.Remove(statEq);
                    }
                }

                if (extraEquipped.Count > 0)
                {
                    int statup = (int)Math.Round((ev * extraRate) / extraEquipped.Count);
                    if (statup > 0)
                    {
                        foreach (Equip extraEq in extraEquipped)
                        {
                            equipUpgrades.GetValueOrDefault(extraEq)?.Add(new(e.Key, statup));
                        }
                    }
                }
            }

            dropMessage(6, "EQUIPMENT MERGE operation results:");
            foreach (var eqpUpg in equipUpgrades)
            {
                List<KeyValuePair<StatUpgrade, int>> eqpStatups = eqpUpg.Value;
                if (eqpStatups.Count > 0)
                {
                    Equip eqp = eqpUpg.Key;
                    setMergeFlag(eqp);

                    string showStr = " '" + ItemInformationProvider.getInstance().getName(eqp.getItemId()) + "': ";
                    string upgdStr = eqp.gainStats(eqpStatups).Key;

                    this.forceUpdateItem(eqp);

                    showStr += upgdStr;
                    dropMessage(6, showStr);
                }
            }

            return true;
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    public void mergeAllItemsFromPosition(Dictionary<StatUpgrade, float> statups, short pos)
    {
        Inventory inv = getInventory(InventoryType.EQUIP);
        inv.lockInventory();
        try
        {
            for (short i = pos; i <= inv.getSlotLimit(); i++)
            {
                standaloneMerge(statups, getClient(), InventoryType.EQUIP, i, inv.getItem(i));
            }
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    private void standaloneMerge(Dictionary<StatUpgrade, float> statups, Client c, InventoryType type, short slot, Item? item)
    {
        short quantity;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (item == null || (quantity = item.getQuantity()) < 1 || ii.isCash(item.getItemId()) || !ii.isUpgradeable(item.getItemId()) || hasMergeFlag(item))
        {
            return;
        }

        Equip e = (Equip)item;
        foreach (var s in e.getStats())
        {
            float? newVal = statups.get(s.Key);

            float incVal = s.Value;
            switch (s.Key)
            {
                case StatUpgrade.incPAD:
                case StatUpgrade.incMAD:
                case StatUpgrade.incPDD:
                case StatUpgrade.incMDD:
                    incVal = (float)Math.Log(incVal);
                    break;
            }

            if (newVal != null)
            {
                newVal += incVal;
            }
            else
            {
                newVal = incVal;
            }

            statups.AddOrUpdate(s.Key, newVal ?? 0);
        }

        InventoryManipulator.removeFromSlot(c, type, (byte)slot, quantity, false);
    }

    public void setShop(Shop? shop)
    {
        this.shop = shop;
    }

    public void setSlot(int slotid)
    {
        slots = slotid;
    }

    public void setTrade(Trade? trade)
    {
        this.trade = trade;
    }

    public void setVanquisherKills(int x)
    {
        this.vanquisherKills = x;
    }

    public void setVanquisherStage(int x)
    {
        this.vanquisherStage = x;
    }

    public void setWorld(int world)
    {
        this.world = world;
    }

    public void shiftPetsRight()
    {
        Monitor.Enter(petLock);
        try
        {
            if (pets[2] == null)
            {
                pets[2] = pets[1];
                pets[1] = pets[0];
                pets[0] = null;
            }
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    private long getDojoTimeLeft()
    {
        return client.getChannelServer().getDojoFinishTime(map.getId()) - Server.getInstance().getCurrentTime();
    }

    public void showDojoClock()
    {
        if (GameConstants.isDojoBossArea(map.getId()))
        {
            sendPacket(PacketCreator.getClock((int)(getDojoTimeLeft() / 1000)));
        }
    }

    public void showUnderleveledInfo(Monster mob)
    {
        long curTime = Server.getInstance().getCurrentTime();
        if (nextWarningTime < curTime)
        {
            nextWarningTime = (long)(curTime + TimeSpan.FromMinutes(1).TotalMilliseconds);   // show underlevel info again after 1 minute

            showHint("You have gained #rno experience#k from defeating #e#b" + mob.getName() + "#k#n (lv. #b" + mob.getLevel() + "#k)! Take note you must have around the same level as the mob to start earning EXP from it.");
        }
    }

    public void showMapOwnershipInfo(Character mapOwner)
    {
        long curTime = Server.getInstance().getCurrentTime();
        if (nextWarningTime < curTime)
        {
            nextWarningTime = (long)(curTime + TimeSpan.FromMinutes(1).TotalMilliseconds); // show underlevel info again after 1 minute

            string medal = "";
            Item? medalItem = mapOwner.getInventory(InventoryType.EQUIPPED).getItem(-49);
            if (medalItem != null)
            {
                medal = "<" + ItemInformationProvider.getInstance().getName(medalItem.getItemId()) + "> ";
            }

            List<string> strLines = new();
            strLines.Add("");
            strLines.Add("");
            strLines.Add("");
            strLines.Insert(this.getClient().getChannelServer().getServerMessage().Count() == 0 ? 0 : 1, "Get off my lawn!!");

            this.sendPacket(PacketCreator.getAvatarMega(mapOwner, medal, this.getClient().getChannel(), ItemId.ROARING_TIGER_MESSENGER, strLines, true));
        }
    }

    public void showHint(string msg)
    {
        showHint(msg, 500);
    }

    public void showHint(string msg, int length)
    {
        client.announceHint(msg, length);
    }

    public void silentGiveBuffs(List<KeyValuePair<long, PlayerBuffValueHolder>> buffs)
    {
        foreach (var mbsv in buffs)
        {
            PlayerBuffValueHolder mbsvh = mbsv.Value;
            mbsvh.effect.silentApplyBuff(this, mbsv.Key);
        }
    }

    public void silentPartyUpdate()
    {
        silentPartyUpdateInternal(getParty());
    }

    private void silentPartyUpdateInternal(Party? chrParty)
    {
        if (chrParty != null)
        {
            getWorldServer().updateParty(chrParty.getId(), PartyOperation.SILENT_UPDATE, getMPC());
        }
    }

    public class SkillEntry
    {

        public int masterlevel;
        public sbyte skillevel;
        public long expiration;

        public SkillEntry(sbyte skillevel, int masterlevel, long expiration)
        {
            this.skillevel = skillevel;
            this.masterlevel = masterlevel;
            this.expiration = expiration;
        }

        public override string ToString()
        {
            return skillevel + ":" + masterlevel;
        }
    }

    public bool skillIsCooling(int skillId)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            return coolDowns.ContainsKey(skillId);
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public void runFullnessSchedule(int petSlot)
    {
        Pet? pet = getPet(petSlot);
        if (pet == null)
        {
            return;
        }

        int newFullness = pet.getFullness() - PetDataFactory.getHunger(pet.getItemId());
        if (newFullness <= 5)
        {
            pet.setFullness(15);
            pet.saveToDb();
            unequipPet(pet, true);
            dropMessage(6, "Your pet grew hungry! Treat it some pet food to keep it healthy!");
        }
        else
        {
            pet.setFullness(newFullness);
            pet.saveToDb();
            Item? petz = getInventory(InventoryType.CASH).getItem(pet.getPosition());
            if (petz != null)
            {
                forceUpdateItem(petz);
            }
        }
    }

    public bool runTirednessSchedule()
    {
        if (maplemount != null)
        {
            int tiredness = maplemount.incrementAndGetTiredness();

            this.getMap().broadcastMessage(PacketCreator.updateMount(this.getId(), maplemount, false));
            if (tiredness > 99)
            {
                maplemount.setTiredness(99);
                this.dispelSkill(this.getJobType() * 10000000 + 1004);
                this.dropMessage(6, "Your mount grew tired! Treat it some revitalizer before riding it again!");
                return false;
            }
        }

        return true;
    }

    public void startMapEffect(string msg, int itemId)
    {
        startMapEffect(msg, itemId, 30000);
    }

    public void startMapEffect(string msg, int itemId, int duration)
    {
        MapEffect mapEffect = new MapEffect(msg, itemId);
        sendPacket(mapEffect.makeStartData());
        TimerManager.getInstance().schedule(() =>
    {
        sendPacket(mapEffect.makeDestroyData());

    }, duration);
    }

    public void unequipAllPets()
    {
        for (int i = 0; i < 3; i++)
        {
            var pet = getPet(i);
            if (pet != null)
            {
                unequipPet(pet, true);
            }
        }
    }

    public void unequipPet(Pet pet, bool shift_left)
    {
        unequipPet(pet, shift_left, false);
    }

    public void unequipPet(Pet pet, bool shift_left, bool hunger)
    {
        sbyte petIdx = this.getPetIndex(pet);
        Pet? chrPet = this.getPet(petIdx);

        if (chrPet != null)
        {
            chrPet.setSummoned(false);
            chrPet.saveToDb();
        }

        this.getClient().getWorldServer().unregisterPetHunger(this, petIdx);
        getMap().broadcastMessage(this, PacketCreator.showPet(this, pet, true, hunger), true);

        removePet(pet, shift_left);
        commitExcludedItems();

        sendPacket(PacketCreator.petStatUpdate(this));
        sendPacket(PacketCreator.enableActions());
    }

    public void updateMacros(int position, SkillMacro updateMacro)
    {
        skillMacros[position] = updateMacro;
    }

    public void updatePartyMemberHP()
    {
        Monitor.Enter(prtLock);
        try
        {
            updatePartyMemberHPInternal();
        }
        finally
        {
            Monitor.Exit(prtLock);
        }
    }

    private void updatePartyMemberHPInternal()
    {
        if (party != null)
        {
            int curmaxhp = getCurrentMaxHp();
            int curhp = getHp();
            foreach (Character partychar in this.getPartyMembersOnSameMap())
            {
                partychar.sendPacket(PacketCreator.updatePartyMemberHP(getId(), curhp, curmaxhp));
            }
        }
    }

    public void setQuestProgress(int id, int infoNumber, string progress)
    {
        Quest q = Quest.getInstance(id);
        QuestStatus qs = getQuest(q);

        if (qs.getInfoNumber() == infoNumber && infoNumber > 0)
        {
            Quest iq = Quest.getInstance(infoNumber);
            QuestStatus iqs = getQuest(iq);
            iqs.setProgress(0, progress);
        }
        else
        {
            qs.setProgress(infoNumber, progress);   // quest progress is thoroughly a string match, infoNumber is actually another questid
        }

        announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
        if (qs.getInfoNumber() > 0)
        {
            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
        }
    }

    public void awardQuestPoint(int awardedPoints)
    {
        if (YamlConfig.config.server.QUEST_POINT_REQUIREMENT < 1 || awardedPoints < 1)
        {
            return;
        }

        int delta;
        lock (quests)
        {
            quest_fame += awardedPoints;

            delta = quest_fame / YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
            quest_fame %= YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
        }

        if (delta > 0)
        {
            gainFame(delta);
        }
    }

    public enum DelayedQuestUpdate
    {    // quest updates allow player actions during NPC talk...
        UPDATE, FORFEIT, COMPLETE, INFO
    }

    private void announceUpdateQuestInternal(Character chr, KeyValuePair<DelayedQuestUpdate, object[]> questUpdate)
    {
        object[] objs = questUpdate.Value;

        switch (questUpdate.Key)
        {
            case DelayedQuestUpdate.UPDATE:
                sendPacket(PacketCreator.updateQuest(chr, (QuestStatus)objs[0], (bool)objs[1]));
                break;

            case DelayedQuestUpdate.FORFEIT:
                sendPacket(PacketCreator.forfeitQuest((short)objs[0]));
                break;

            case DelayedQuestUpdate.COMPLETE:
                sendPacket(PacketCreator.completeQuest((short)objs[0], (long)objs[1]));
                break;

            case DelayedQuestUpdate.INFO:
                QuestStatus qs = (QuestStatus)objs[0];
                sendPacket(PacketCreator.updateQuestInfo(qs.getQuest().getId(), qs.getNpc()));
                break;
        }
    }

    public void announceUpdateQuest(DelayedQuestUpdate questUpdateType, params object[] paramsValue)
    {
        KeyValuePair<DelayedQuestUpdate, object[]> p = new(questUpdateType, paramsValue);
        Client c = this.getClient();
        if (c.getQM() != null || c.getCM() != null)
        {
            lock (npcUpdateQuests)
            {
                npcUpdateQuests.Add(p);
            }
        }
        else
        {
            announceUpdateQuestInternal(this, p);
        }
    }

    public void flushDelayedUpdateQuests()
    {
        List<KeyValuePair<DelayedQuestUpdate, object[]>> qmQuestUpdateList;

        lock (npcUpdateQuests)
        {
            qmQuestUpdateList = new(npcUpdateQuests);
            npcUpdateQuests.Clear();
        }

        foreach (var q in qmQuestUpdateList)
        {
            announceUpdateQuestInternal(this, q);
        }
    }

    public void updateQuestStatus(QuestStatus qs)
    {
        lock (quests)
        {
            quests.AddOrUpdate(qs.getQuestID(), qs);
        }
        if (qs.getStatus().Equals(QuestStatus.Status.STARTED))
        {
            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
            if (qs.getInfoNumber() > 0)
            {
                announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
            }
            announceUpdateQuest(DelayedQuestUpdate.INFO, qs);
        }
        else if (qs.getStatus().Equals(QuestStatus.Status.COMPLETED))
        {
            Quest mquest = qs.getQuest();
            short questid = mquest.getId();
            if (!mquest.isSameDayRepeatable() && !Quest.isExploitableQuest(questid))
            {
                awardQuestPoint(YamlConfig.config.server.QUEST_POINT_PER_QUEST_COMPLETE);
            }
            qs.setCompleted(qs.getCompleted() + 1);   // Jayd's idea - count quest completed

            announceUpdateQuest(DelayedQuestUpdate.COMPLETE, questid, qs.getCompletionTime());
            //announceUpdateQuest(DelayedQuestUpdate.INFO, qs); // happens after giving rewards, for non-next quests only
        }
        else if (qs.getStatus().Equals(QuestStatus.Status.NOT_STARTED))
        {
            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
            if (qs.getInfoNumber() > 0)
            {
                announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
            }
            // reminder: do not reset quest progress of infoNumbers, some quests cannot backtrack
        }
    }

    private void expireQuest(Quest quest)
    {
        if (quest.forfeit(this))
        {
            sendPacket(PacketCreator.questExpire(quest.getId()));
        }
    }

    public void cancelQuestExpirationTask()
    {
        Monitor.Enter(evtLock);
        try
        {
            if (questExpireTask != null)
            {
                questExpireTask.cancel(false);
                questExpireTask = null;
            }
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    public void forfeitExpirableQuests()
    {
        Monitor.Enter(evtLock);
        try
        {
            foreach (Quest quest in questExpirations.Keys)
            {
                quest.forfeit(this);
            }

            questExpirations.Clear();
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    public void questExpirationTask()
    {
        Monitor.Enter(evtLock);
        try
        {
            if (questExpirations.Count > 0)
            {
                if (questExpireTask == null)
                {
                    questExpireTask = TimerManager.getInstance().register(() =>
                    {
                        runQuestExpireTask();

                    }, TimeSpan.FromSeconds(10));
                }
            }
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    private void runQuestExpireTask()
    {
        Monitor.Enter(evtLock);
        try
        {
            long timeNow = Server.getInstance().getCurrentTime();
            List<Quest> expireList = new();

            foreach (var qe in questExpirations)
            {
                if (qe.Value <= timeNow)
                {
                    expireList.Add(qe.Key);
                }
            }

            if (expireList.Count > 0)
            {
                foreach (Quest quest in expireList)
                {
                    expireQuest(quest);
                    questExpirations.Remove(quest);
                }

                if (questExpirations.Count == 0)
                {
                    questExpireTask.cancel(false);
                    questExpireTask = null;
                }
            }
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    private void registerQuestExpire(Quest quest, TimeSpan time)
    {
        Monitor.Enter(evtLock);
        try
        {
            if (questExpireTask == null)
            {
                questExpireTask = TimerManager.getInstance().register(() =>
                {
                    runQuestExpireTask();

                }, TimeSpan.FromSeconds(10));
            }

            questExpirations.AddOrUpdate(quest, (long)(Server.getInstance().getCurrentTime() + time.TotalMilliseconds));
        }
        finally
        {
            Monitor.Exit(evtLock);
        }
    }

    public void questTimeLimit(Quest quest, int seconds)
    {
        registerQuestExpire(quest, TimeSpan.FromSeconds(seconds));
        sendPacket(PacketCreator.addQuestTimeLimit(quest.getId(), seconds * 1000));
    }

    public void questTimeLimit2(Quest quest, long expires)
    {
        long timeLeft = expires - DateTimeOffset.Now.ToUnixTimeMilliseconds();

        if (timeLeft <= 0)
        {
            expireQuest(quest);
        }
        else
        {
            registerQuestExpire(quest, TimeSpan.FromMilliseconds(timeLeft));
        }
    }

    public void updateSingleStat(Stat stat, int newval)
    {
        updateSingleStat(stat, newval, false);
    }

    private void updateSingleStat(Stat stat, int newval, bool itemReaction)
    {
        sendPacket(PacketCreator.updatePlayerStats(Collections.singletonList(new KeyValuePair<Stat, int>(stat, newval)), itemReaction, this));
    }

    public void sendPacket(Packet packet)
    {
        client.sendPacket(packet);
    }

    public override int getObjectId()
    {
        return getId();
    }

    public override MapObjectType getType()
    {
        return MapObjectType.PLAYER;
    }

    public override void sendDestroyData(Client client)
    {
        client.sendPacket(PacketCreator.removePlayerFromMap(this.getObjectId()));
    }

    public override void sendSpawnData(Client client)
    {
        if (!this.isHidden() || client.getPlayer().gmLevel() > 1)
        {
            client.sendPacket(PacketCreator.spawnPlayerMapObject(client, this, false));

            if (buffEffects.ContainsKey(getJobMapChair(job)))
            { // mustn't effLock, chrLock sendSpawnData
                client.sendPacket(PacketCreator.giveForeignChairSkillEffect(id));
            }
        }

        if (this.isHidden())
        {
            List<KeyValuePair<BuffStat, int>> dsstat = Collections.singletonList(new KeyValuePair<BuffStat, int>(BuffStat.DARKSIGHT, 0));
            getMap().broadcastGMMessage(this, PacketCreator.giveForeignBuff(getId(), dsstat), false);
        }
    }

    public override void setObjectId(int id) { }

    public override string ToString()
    {
        return name;
    }

    public int getLinkedLevel()
    {
        return linkedLevel;
    }

    public string getLinkedName()
    {
        return linkedName;
    }

    public CashShop getCashShop()
    {
        return cashshop;
    }

    public HashSet<NewYearCardRecord> getNewYearRecords()
    {
        return newyears;
    }

    public HashSet<NewYearCardRecord> getReceivedNewYearRecords()
    {
        HashSet<NewYearCardRecord> received = new();

        foreach (NewYearCardRecord nyc in newyears)
        {
            if (nyc.isReceiverCardReceived())
            {
                received.Add(nyc);
            }
        }

        return received;
    }

    public NewYearCardRecord? getNewYearRecord(int cardid)
    {
        foreach (NewYearCardRecord nyc in newyears)
        {
            if (nyc.getId() == cardid)
            {
                return nyc;
            }
        }

        return null;
    }

    public void addNewYearRecord(NewYearCardRecord newyear)
    {
        newyears.Add(newyear);
    }

    public void removeNewYearRecord(NewYearCardRecord newyear)
    {
        newyears.Remove(newyear);
    }

    public void portalDelay(long delay)
    {
        this.portaldelay = DateTimeOffset.Now.ToUnixTimeMilliseconds() + delay;
    }

    public long portalDelay()
    {
        return portaldelay;
    }

    public void blockPortal(string scriptName)
    {
        if (!blockedPortals.Contains(scriptName) && scriptName != null)
        {
            blockedPortals.Add(scriptName);
            sendPacket(PacketCreator.enableActions());
        }
    }

    public void unblockPortal(string scriptName)
    {
        if (blockedPortals.Contains(scriptName) && scriptName != null)
        {
            blockedPortals.Remove(scriptName);
        }
    }

    public List<string> getBlockedPortals()
    {
        return blockedPortals;
    }

    public bool containsAreaInfo(int area, string info)
    {
        short area_ = ((short)area);
        if (area_info.ContainsKey(area_))
        {
            return area_info[area_].Contains(info);
        }
        return false;
    }

    public void updateAreaInfo(int area, string info)
    {
        area_info.AddOrUpdate((short)area, info);
        sendPacket(PacketCreator.updateAreaInfo(area, info));
    }

    public string? getAreaInfo(int area)
    {
        return area_info.GetValueOrDefault((short)area);
    }

    public Dictionary<short, string> getAreaInfos()
    {
        return area_info;
    }

    public void autoban(string reason)
    {
        if (this.isGM() || this.isBanned())
        {  // thanks RedHat for noticing GM's being able to get banned
            return;
        }

        this.ban(reason);
        sendPacket(PacketCreator.sendPolice(string.Format("You have been blocked by the#b {0} Police for HACK reason.#k", "Cosmic")));
        TimerManager.getInstance().schedule(() =>
    {
        client.disconnect(false, false);

    }, 5000);

        Server.getInstance().broadcastGMMessage(this.getWorld(), PacketCreator.serverNotice(6, Character.makeMapleReadable(this.name) + " was autobanned for " + reason));
    }

    public void block(int reason, int days, string desc)
    {
        try
        {
            var tempBan = DateTimeOffset.Now.AddDays(days);
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == accountid).ExecuteUpdate(x => x.SetProperty(y => y.Banreason, desc)
                .SetProperty(y => y.Tempban, tempBan)
                .SetProperty(y => y.Greason, reason));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public bool isBanned()
    {
        return isbanned;
    }

    public List<int> getTrockMaps()
    {
        return trockmaps;
    }

    public List<int> getVipTrockMaps()
    {
        return viptrockmaps;
    }

    public int getTrockSize()
    {
        int ret = trockmaps.IndexOf(MapId.NONE);
        if (ret == -1)
        {
            ret = 5;
        }

        return ret;
    }

    public void deleteFromTrocks(int map)
    {
        trockmaps.Remove(map);
        while (trockmaps.Count < 10)
        {
            trockmaps.Add(MapId.NONE);
        }
    }

    public void addTrockMap()
    {
        int index = trockmaps.IndexOf(MapId.NONE);
        if (index != -1)
        {
            trockmaps.set(index, getMapId());
        }
    }

    public bool isTrockMap(int id)
    {
        int index = trockmaps.IndexOf(id);
        return index != -1;
    }

    public int getVipTrockSize()
    {
        int ret = viptrockmaps.IndexOf(MapId.NONE);

        if (ret == -1)
        {
            ret = 10;
        }

        return ret;
    }

    public void deleteFromVipTrocks(int map)
    {
        viptrockmaps.Remove(map);
        while (viptrockmaps.Count < 10)
        {
            viptrockmaps.Add(MapId.NONE);
        }
    }

    public void addVipTrockMap()
    {
        int index = viptrockmaps.IndexOf(MapId.NONE);
        if (index != -1)
        {
            viptrockmaps.set(index, getMapId());
        }
    }

    public bool isVipTrockMap(int id)
    {
        int index = viptrockmaps.IndexOf(id);
        return index != -1;
    }

    public AutobanManager getAutobanManager()
    {
        return _autoban;
    }

    public void equippedItem(Equip equip)
    {
        int itemid = equip.getItemId();

        if (itemid == ItemId.PENDANT_OF_THE_SPIRIT)
        {
            this.equipPendantOfSpirit();
        }
        else if (itemid == ItemId.MESO_MAGNET)
        {
            equippedMesoMagnet = true;
        }
        else if (itemid == ItemId.ITEM_POUCH)
        {
            equippedItemPouch = true;
        }
        else if (itemid == ItemId.ITEM_IGNORE)
        {
            equippedPetItemIgnore = true;
        }
    }

    public void unequippedItem(Equip equip)
    {
        int itemid = equip.getItemId();

        if (itemid == ItemId.PENDANT_OF_THE_SPIRIT)
        {
            this.unequipPendantOfSpirit();
        }
        else if (itemid == ItemId.MESO_MAGNET)
        {
            equippedMesoMagnet = false;
        }
        else if (itemid == ItemId.ITEM_POUCH)
        {
            equippedItemPouch = false;
        }
        else if (itemid == ItemId.ITEM_IGNORE)
        {
            equippedPetItemIgnore = false;
        }
    }

    public bool isEquippedMesoMagnet()
    {
        return equippedMesoMagnet;
    }

    public bool isEquippedItemPouch()
    {
        return equippedItemPouch;
    }

    public bool isEquippedPetItemIgnore()
    {
        return equippedPetItemIgnore;
    }

    private void equipPendantOfSpirit()
    {
        if (pendantOfSpirit == null)
        {
            pendantOfSpirit = TimerManager.getInstance().register(() =>
            {
                if (pendantExp < 3)
                {
                    pendantExp++;
                    message("Pendant of the Spirit has been equipped for " + pendantExp + " hour(s), you will now receive " + pendantExp + "0% bonus exp.");
                }
                else
                {
                    pendantOfSpirit!.cancel(false);
                }

            }, 3600000); //1 hour
        }
    }

    private void unequipPendantOfSpirit()
    {
        if (pendantOfSpirit != null)
        {
            pendantOfSpirit.cancel(false);
            pendantOfSpirit = null;
        }
        pendantExp = 0;
    }

    private ICollection<Item> getUpgradeableEquipList()
    {
        ICollection<Item> fullList = getInventory(InventoryType.EQUIPPED).list();
        if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_CASH)
        {
            return fullList;
        }

        Collection<Item> eqpList = new();
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (Item it in fullList)
        {
            if (!ii.isCash(it.getItemId()))
            {
                eqpList.Add(it);
            }
        }

        return eqpList;
    }

    public void increaseEquipExp(int expGain)
    {
        if (allowExpGain)
        {     // thanks Vcoc for suggesting equip EXP gain conditionally
            if (expGain < 0)
            {
                expGain = int.MaxValue;
            }

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (Item item in getUpgradeableEquipList())
            {
                Equip nEquip = (Equip)item;
                string itemName = ii.getName(nEquip.getItemId());
                if (itemName == null)
                {
                    continue;
                }

                nEquip.gainItemExp(client, expGain);
            }
        }
    }

    public void showAllEquipFeatures()
    {
        string showMsg = "";

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (Item item in getInventory(InventoryType.EQUIPPED).list())
        {
            Equip nEquip = (Equip)item;
            string itemName = ii.getName(nEquip.getItemId());
            if (itemName == null)
            {
                continue;
            }

            showMsg += nEquip.showEquipFeatures(client);
        }

        if (showMsg.Count() > 0)
        {
            this.showHint("#ePLAYER EQUIPMENTS:#n\r\n\r\n" + showMsg, 400);
        }
    }

    public void broadcastMarriageMessage()
    {
        Guild? guild = this.getGuild();
        if (guild != null)
        {
            guild.broadcast(PacketCreator.marriageMessage(0, name));
        }

        Family? family = this.getFamily();
        if (family != null)
        {
            family.broadcast(PacketCreator.marriageMessage(1, name));
        }
    }

    public Dictionary<string, Events> getEvents()
    {
        return events;
    }

    public PartyQuest? getPartyQuest()
    {
        return partyQuest;
    }

    public void setPartyQuest(PartyQuest pq)
    {
        this.partyQuest = pq;
    }

    public void setCpqTimer(ScheduledFuture timer)
    {
        this.cpqSchedule = timer;
    }

    public void clearCpqTimer()
    {
        if (cpqSchedule != null)
        {
            cpqSchedule.cancel(true);
        }
        cpqSchedule = null;
    }

    public void empty(bool remove)
    {
        if (dragonBloodSchedule != null)
        {
            dragonBloodSchedule.cancel(true);
        }
        dragonBloodSchedule = null;

        if (hpDecreaseTask != null)
        {
            hpDecreaseTask.cancel(true);
        }
        hpDecreaseTask = null;

        if (beholderHealingSchedule != null)
        {
            beholderHealingSchedule.cancel(true);
        }
        beholderHealingSchedule = null;

        if (beholderBuffSchedule != null)
        {
            beholderBuffSchedule.cancel(true);
        }
        beholderBuffSchedule = null;

        if (berserkSchedule != null)
        {
            berserkSchedule.cancel(true);
        }
        berserkSchedule = null;

        unregisterChairBuff();
        cancelBuffExpireTask();
        cancelDiseaseExpireTask();
        cancelSkillCooldownTask();
        cancelExpirationTask();

        if (questExpireTask != null)
        {
            questExpireTask.cancel(true);
        }
        questExpireTask = null;

        if (recoveryTask != null)
        {
            recoveryTask.cancel(true);
        }
        recoveryTask = null;

        if (extraRecoveryTask != null)
        {
            extraRecoveryTask.cancel(true);
        }
        extraRecoveryTask = null;

        // already done on unregisterChairBuff
        /* if (chairRecoveryTask != null) { chairRecoveryTask.cancel(true); }
        chairRecoveryTask = null; */

        if (pendantOfSpirit != null)
        {
            pendantOfSpirit.cancel(true);
        }
        pendantOfSpirit = null;

        clearCpqTimer();

        Monitor.Enter(evtLock);
        try
        {
            if (questExpireTask != null)
            {
                questExpireTask.cancel(false);
                questExpireTask = null;

                questExpirations.Clear();
            }
        }
        finally
        {
            Monitor.Exit(evtLock);
        }

        if (maplemount != null)
        {
            maplemount.empty();
            maplemount = null;
        }
        if (remove)
        {
            partyQuest = null;
            mpc = null;
            mgc = null;
            party = null;
            FamilyEntry? familyEntry = getFamilyEntry();
            if (familyEntry != null)
            {
                familyEntry.setCharacter(null);
                setFamilyEntry(null);
            }

            getWorldServer().registerTimedMapObject(() =>
            {
                client = null;  // clients still triggers handlers a few times after disconnecting
                map = null;
                setListener(null);

                // thanks Shavit for noticing a memory leak with inventories holding owner object
                for (int i = 0; i < inventory.Length; i++)
                {
                    inventory[i].dispose();
                }
                inventory = null;

            }, (long)TimeSpan.FromMinutes(5).TotalMilliseconds);
        }
    }

    public void logOff()
    {
        this.loggedIn = false;
        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.Id == getId()).ExecuteUpdate(x => x.SetProperty(y => y.LastLogoutTime, DateTimeOffset.Now));
    }

    public void setLoginTime(long time)
    {
        this.loginTime = time;
    }

    public long getLoginTime()
    {
        return loginTime;
    }

    public long getLoggedInTime()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() - loginTime;
    }

    public bool isLoggedin()
    {
        return loggedIn;
    }

    public void setMapId(int mapid)
    {
        this.mapid = mapid;
    }

    public bool getWhiteChat()
    {
        return isGM() && whiteChat;
    }

    public void toggleWhiteChat()
    {
        whiteChat = !whiteChat;
    }

    // These need to be renamed, but I am too lazy right now to go through the scripts and rename them...
    public string getPartyQuestItems()
    {
        return dataString;
    }

    public bool gotPartyQuestItem(string partyquestchar)
    {
        return dataString.Contains(partyquestchar);
    }

    public void removePartyQuestItem(string letter)
    {
        if (gotPartyQuestItem(letter))
        {
            dataString = dataString!.Substring(0, dataString.IndexOf(letter)) + dataString.Substring(dataString.IndexOf(letter) + letter.Length);
        }
    }

    public void setPartyQuestItemObtained(string partyquestchar)
    {
        if (!dataString.Contains(partyquestchar))
        {
            this.dataString += partyquestchar;
        }
    }

    public void createDragon()
    {
        dragon = new Dragon(this);
    }

    public Dragon? getDragon()
    {
        return dragon;
    }

    public void setDragon(Dragon dragon)
    {
        this.dragon = dragon;
    }

    public void setAutopotHpAlert(float hpPortion)
    {
        autopotHpAlert = hpPortion;
    }

    public float getAutopotHpAlert()
    {
        return autopotHpAlert;
    }

    public void setAutopotMpAlert(float mpPortion)
    {
        autopotMpAlert = mpPortion;
    }

    public float getAutopotMpAlert()
    {
        return autopotMpAlert;
    }

    public long getJailExpirationTimeLeft()
    {
        return jailExpiration - DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private void setFutureJailExpiration(long time)
    {
        jailExpiration = DateTimeOffset.Now.ToUnixTimeMilliseconds() + time;
    }

    public void addJailExpirationTime(long time)
    {
        long timeLeft = getJailExpirationTimeLeft();

        if (timeLeft <= 0)
        {
            setFutureJailExpiration(time);
        }
        else
        {
            setFutureJailExpiration(timeLeft + time);
        }
    }

    public void removeJailExpirationTime()
    {
        jailExpiration = 0;
    }

    public bool registerNameChange(string newName)
    {
        try
        {
            using var dbContext = new DBContext();
            //check for pending name change
            var currentTimeMillis = DateTimeOffset.Now;

            try
            {
                var dataList = dbContext.Namechanges.Where(x => x.Characterid == getId()).Select(x => x.CompletionTime).ToList();
                var nextChangeNameTime = currentTimeMillis.AddMilliseconds(-(double)YamlConfig.config.server.NAME_CHANGE_COOLDOWN);
                if (dataList.Any(x => x == null || x.Value > nextChangeNameTime))
                    return false;
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to register name change for chr {CharacterName}", getName());
                return false;
            }

            try
            {
                var dbModel = new Namechange()
                {
                    Characterid = getId(),
                    Old = getName(),
                    New = newName
                };
                dbContext.Namechanges.Add(dbModel);
                dbContext.SaveChanges();
                this.pendingNameChange = true;
                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to register name change for chr {CharacterName}", getName());
            }
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to get DB connection while registering name change");
        }
        return false;
    }

    public bool cancelPendingNameChange()
    {
        try
        {
            using var dbContext = new DBContext();

            int affectedRows = dbContext.Namechanges.Where(x => x.Characterid == getId() && x.CompletionTime == null).ExecuteDelete();
            if (affectedRows > 0) pendingNameChange = false;
            return affectedRows > 0; //rows affected
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to cancel name change for chr {CharacterName}", getName());
            return false;
        }
    }
    public void doPendingNameChange()
    { //called on logout
        if (!pendingNameChange) return;
        try
        {
            using var dbContext = new DBContext();
            int nameChangeId = -1;
            string? newName = null;
            try
            {
                var dbModel = dbContext.Namechanges.Where(x => x.Characterid == getId() && x.CompletionTime == null).FirstOrDefault();

                if (dbModel == null)
                    return;
                nameChangeId = dbModel.Id;
                newName = dbModel.New;
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to retrieve pending name changes for chr {CharacterName}", this.name);
            }
            using var dbTrans = dbContext.Database.BeginTransaction();
            bool success = doNameChange(dbContext, getId(), getName(), newName!, nameChangeId);
            if (!success)
                dbTrans.Rollback();
            else
                log.Information("Name change applied: from {0} to {1}", this.name, newName);
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to get DB connection for pending chr name change");
        }
    }

    public static void doNameChange(int characterId, string oldName, string newName, int nameChangeId)
    { //Don't do this while player is online
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            bool success = doNameChange(dbContext, characterId, oldName, newName, nameChangeId);
            if (!success) dbTrans.Rollback();
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to get DB connection for chr name change");
        }
    }

    public static bool doNameChange(DBContext dbContext, int characterId, string oldName, string newName, int nameChangeId)
    {
        try
        {
            dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.Name, newName));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to perform chr name change in database for chrId {CharacterId}", characterId);
            return false;
        }

        try
        {
            dbContext.Rings.Where(x => x.PartnerName == oldName).ExecuteUpdate(x => x.SetProperty(y => y.PartnerName, newName));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to update rings during chr name change for chrId {CharacterId}", characterId);
            return false;
        }

        /*try (PreparedStatement ps = con.prepareStatement("UPDATE playernpcs SET name = ? WHERE name = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE gifts SET `from` = ? WHERE `from` = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }
        try (PreparedStatement ps = con.prepareStatement("UPDATE dueypackages SET SenderName = ? WHERE SenderName = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE dueypackages SET SenderName = ? WHERE SenderName = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE inventoryitems SET owner = ? WHERE owner = ?")) { //GMS doesn't do this
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE mts_items SET owner = ? WHERE owner = ?")) { //GMS doesn't do this
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE newyear SET sendername = ? WHERE sendername = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE newyear SET receivername = ? WHERE receivername = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE notes SET `to` = ? WHERE `to` = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE notes SET `from` = ? WHERE `from` = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }

        try (PreparedStatement ps = con.prepareStatement("UPDATE nxcode SET retriever = ? WHERE retriever = ?")) {
            ps.setString(1, newName);
            ps.setString(2, oldName);
            ps.executeUpdate();
        } catch(Exception e) { 
            Log.Logger.Error(e.ToString());
            FilePrinter.printError(FilePrinter.CHANGE_CHARACTER_NAME, e, "Character ID : " + characterId);
            return false;
        }*/

        if (nameChangeId != -1)
        {
            try
            {
                dbContext.Namechanges.Where(x => x.Id == nameChangeId).ExecuteUpdate(x => x.SetProperty(y => y.CompletionTime, DateTimeOffset.Now));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to save chr name change for chrId {NameChangeId}", nameChangeId);
                return false;
            }
        }
        return true;
    }

    public int checkWorldTransferEligibility()
    {
        if (getLevel() < 20)
        {
            return 2;
        }
        else if (getClient().getTempBanCalendar() != null && getClient().getTempBanCalendar().Value.AddDays(30) < DateTimeOffset.Now)
        {
            return 3;
        }
        else if (isMarried())
        {
            return 4;
        }
        else if (getGuildRank() < 2)
        {
            return 5;
        }
        else if (getFamily() != null)
        {
            return 8;
        }
        else
        {
            return 0;
        }
    }

    public static string? checkWorldTransferEligibility(DBContext dbContext, int characterId, int oldWorld, int newWorld)
    {
        if (!YamlConfig.config.server.ALLOW_CASHSHOP_WORLD_TRANSFER)
        {
            return "World transfers disabled.";
        }
        int accountId = -1;
        try
        {
            var charInfoFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.AccountId, x.Level, x.GuildId, x.GuildRank, x.PartnerId, x.FamilyId }).FirstOrDefault();
            if (charInfoFromDB == null)
                return "Character does not exist.";
            accountId = charInfoFromDB.AccountId;
            if (charInfoFromDB.Level < 20)
                return "Character is under level 20.";
            if (charInfoFromDB.FamilyId != -1)
                return "Character is in family.";
            if (charInfoFromDB.PartnerId != 0)
                return "Character is married.";
            if (charInfoFromDB.GuildId != 0 && charInfoFromDB.GuildRank < 2)
                return "Character is the leader of a guild.";
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Change character name");
            return "SQL Error";
        }
        try
        {
            var accInfoFromDB = dbContext.Accounts.Where(x => x.Id == accountId).Select(x => new { x.Tempban }).FirstOrDefault();
            if (accInfoFromDB == null)
                return "Account does not exist.";
            if (accInfoFromDB.Tempban != DateTimeOffset.MinValue && accInfoFromDB.Tempban != DefaultDates.getTempban())
                return "Account has been banned.";
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Change character name");
            return "SQL Error";
        }
        try
        {
            var rowcount = dbContext.Characters.Where(x => x.AccountId == accountId && x.World == newWorld).Count();

            if (rowcount >= 3) return "Too many characters on destination world.";
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Change character name");
            return "SQL Error";
        }
        return null;
    }

    public bool registerWorldTransfer(int newWorld)
    {
        try
        {
            using var dbContext = new DBContext();
            try
            {
                //check for pending world transfer
                var charTransfters = dbContext.Worldtransfers.Where(x => x.Characterid == getId()).Select(x => x.CompletionTime).ToList();
                if (charTransfters.Any(x => x == null))
                    return false;
                if (charTransfters.Any(x => x!.Value.AddMilliseconds(YamlConfig.config.server.WORLD_TRANSFER_COOLDOWN) > DateTimeOffset.Now))
                    return false;
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
                return false;
            }

            try
            {
                var dbModel = new Worldtransfer(getId(), (sbyte)getWorld(), (sbyte)newWorld);
                dbContext.Worldtransfers.Add(dbModel);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
            }
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to get DB connection while registering world transfer");
        }
        return false;
    }

    public bool cancelPendingWorldTranfer()
    {
        try
        {
            using var dbContext = new DBContext();
            return dbContext.Worldtransfers.Where(x => x.Characterid == getId() && x.CompletionTime == null).ExecuteDelete() > 0;
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to cancel pending world transfer for chr {CharacterName}", getName());
            return false;
        }
    }

    public static bool doWorldTransfer(DBContext dbContext, int characterId, int oldWorld, int newWorld, int worldTransferId)
    {
        int mesos = 0;
        try
        {
            var mesosFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.Meso }).FirstOrDefault();
            if (mesosFromDB == null)
            {
                Log.Logger.Warning("Character data invalid for world transfer? chrId {CharacterId}", characterId);
                return false;
            }
            mesos = mesosFromDB.Meso;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to do world transfer for chrId {CharacterId}", characterId);
            return false;
        }
        try
        {
            dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.World, newWorld)
                .SetProperty(y => y.Meso, Math.Min(mesos, 1000000))
                .SetProperty(y => y.GuildId, 0)
                .SetProperty(y => y.GuildRank, 5));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to update chrId {CharacterId} during world transfer", characterId);
            return false;
        }
        try
        {
            dbContext.Buddies.Where(x => x.CharacterId == characterId || x.BuddyId == characterId).ExecuteDelete();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to delete buddies for chrId {CharacterId} during world transfer", characterId);
            return false;
        }
        if (worldTransferId != -1)
        {
            try
            {
                dbContext.Worldtransfers.Where(x => x.Id == worldTransferId).ExecuteUpdate(x => x.SetProperty(y => y.CompletionTime, DateTimeOffset.Now));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to update world transfer for chrId {CharacterId}", characterId);
                return false;
            }
        }
        return true;
    }

    public string getLastCommandMessage()
    {
        return this.commandtext;
    }

    public void setLastCommandMessage(string text)
    {
        this.commandtext = text;
    }

    public int getRewardPoints()
    {
        try
        {
            using var dbContext = new DBContext();
            return dbContext.Accounts.Where(x => x.Id == accountid).Select(x => new { x.Rewardpoints }).FirstOrDefault()?.Rewardpoints ?? -1;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
        return -1;
    }

    public void setRewardPoints(int value)
    {

        try
        {
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == accountid).ExecuteUpdate(x => x.SetProperty(y => y.Rewardpoints, value));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    //EVENTS
    private sbyte team = 0;
    private Fitness? fitness;
    private Ola? ola;
    private long snowballattack;

    public sbyte getTeam()
    {
        return team;
    }

    public void setTeam(int team)
    {
        this.team = (sbyte)team;
    }

    public Ola? getOla()
    {
        return ola;
    }

    public void setOla(Ola? ola)
    {
        this.ola = ola;
    }

    public Fitness? getFitness()
    {
        return fitness;
    }

    public void setFitness(Fitness? fit)
    {
        this.fitness = fit;
    }

    public long getLastSnowballAttack()
    {
        return snowballattack;
    }

    public void setLastSnowballAttack(long time)
    {
        this.snowballattack = time;
    }

    // MCPQ

    public AriantColiseum? ariantColiseum;
    private MonsterCarnival? monsterCarnival;
    private MonsterCarnivalParty? monsterCarnivalParty = null;

    private int cp = 0;
    private int totCP = 0;
    private int FestivalPoints;
    private bool challenged = false;
    public int totalCP, availableCP;

    public void gainFestivalPoints(int gain)
    {
        this.FestivalPoints += gain;
    }

    public int getFestivalPoints()
    {
        return this.FestivalPoints;
    }

    public void setFestivalPoints(int pontos)
    {
        this.FestivalPoints = pontos;
    }

    public int getCP()
    {
        return cp;
    }

    public void addCP(int ammount)
    {
        totalCP += ammount;
        availableCP += ammount;
    }

    public void useCP(int ammount)
    {
        availableCP -= ammount;
    }

    public void gainCP(int gain)
    {
        if (this.getMonsterCarnival() != null)
        {
            if (gain > 0)
            {
                this.setTotalCP(this.getTotalCP() + gain);
            }
            this.setCP(this.getCP() + gain);
            if (this.getParty() != null)
            {
                this.getMonsterCarnival().setCP(this.getMonsterCarnival().getCP(team) + gain, team);
                if (gain > 0)
                {
                    this.getMonsterCarnival().setTotalCP(this.getMonsterCarnival().getTotalCP(team) + gain, team);
                }
            }
            if (this.getCP() > this.getTotalCP())
            {
                this.setTotalCP(this.getCP());
            }
            sendPacket(PacketCreator.CPUpdate(false, this.getCP(), this.getTotalCP(), getTeam()));
            if (this.getParty() != null && getTeam() != -1)
            {
                this.getMap().broadcastMessage(PacketCreator.CPUpdate(true, this.getMonsterCarnival().getCP(team), this.getMonsterCarnival().getTotalCP(team), getTeam()));
            }
            else
            {
            }
        }
    }

    public void setTotalCP(int a)
    {
        this.totCP = a;
    }

    public void setCP(int a)
    {
        this.cp = a;
    }

    public int getTotalCP()
    {
        return totCP;
    }

    public int getAvailableCP()
    {
        return availableCP;
    }

    public void resetCP()
    {
        this.cp = 0;
        this.totCP = 0;
        this.monsterCarnival = null;
    }

    public MonsterCarnival? getMonsterCarnival()
    {
        return monsterCarnival;
    }

    public void setMonsterCarnival(MonsterCarnival? monsterCarnival)
    {
        this.monsterCarnival = monsterCarnival;
    }

    public AriantColiseum? getAriantColiseum()
    {
        return ariantColiseum;
    }

    public void setAriantColiseum(AriantColiseum? ariantColiseum)
    {
        this.ariantColiseum = ariantColiseum;
    }

    public MonsterCarnivalParty? getMonsterCarnivalParty()
    {
        return this.monsterCarnivalParty;
    }

    public void setMonsterCarnivalParty(MonsterCarnivalParty? mcp)
    {
        this.monsterCarnivalParty = mcp;
    }

    public bool isChallenged()
    {
        return challenged;
    }

    public void setChallenged(bool challenged)
    {
        this.challenged = challenged;
    }

    public void gainAriantPoints(int points)
    {
        this.ariantPoints += points;
    }

    public int getAriantPoints()
    {
        return this.ariantPoints;
    }


    public void setLanguage(int num)
    {
        getClient().setLanguage(num);
        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == getClient().getAccID()).ExecuteUpdate(x => x.SetProperty(y => y.Language, num));
    }

    public int getLanguage()
    {
        return getClient().getLanguage();
    }

    public bool isChasing()
    {
        return chasing;
    }

    public void setChasing(bool chasing)
    {
        this.chasing = chasing;
    }
}
