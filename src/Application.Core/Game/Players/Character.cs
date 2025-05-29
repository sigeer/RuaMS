/* 
 This file is part of the OdinMS Maple Story NewServer
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

using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Specials;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.Gameplay;
using Application.Core.Managers;
using Application.Shared.Items;
using Application.Shared.KeyMaps;
using client;
using client.autoban;
using client.creator;
using client.inventory;
using client.inventory.manipulator;
using client.keybind;
using client.processor.action;
using constants.game;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.guild;
using net.server.world;
using scripting;
using scripting.Event;
using server;
using server.events;
using server.events.gm;
using server.maps;
using server.partyquest;
using server.quest;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using tools;
using tools.packets;
using static client.inventory.Equip;

namespace Application.Core.Game.Players;

public partial class Player
{
    private ITeam? teamModel;
    public ITeam? TeamModel
    {
        get
        {
            return teamModel;
        }
        private set
        {
            teamModel = value;
            Party = teamModel?.getId() ?? 0;
        }
    }
    public IGuild? GuildModel => getGuild();
    public IAlliance? AllianceModel => getAlliance();
    public Storage Storage { get; set; } = null!;

    private ILogger? _log;
    public ILogger Log => _log ?? (_log = LogFactory.GetCharacterLog(AccountId, Id));

    public int InitialSpawnPoint { get; set; }
    private int currentPage, currentType = 0, currentTab = 1;

    private int energybar;

    private int ci = 0;

    // 替换Family，搁置
    public ISchool? SchoolModel { get; set; }
    private FamilyEntry? familyEntry;

    private int battleshipHp = 0;
    private int mesosTraded = 0;
    private int possibleReports = 10;
    private int dojoEnergy;
    private float expCoupon = 1, mesoCoupon = 1, dropCoupon = 1;
    private int owlSearch;
    private long lastUsedCashItem, lastExpression = 0, lastHealed, lastDeathtime = -1;
    private int localstr, localdex, localluk, localint_, localmagic, localwatk;
    private int equipstr, equipdex, equipluk, equipint_, equipmagic, equipwatk, localchairhp, localchairmp;
    private int localchairrate;
    private bool hidden, equipchanged = true, berserk, hasSandboxItem = false, whiteChat = false;
    private bool equippedMesoMagnet = false, equippedItemPouch = false, equippedPetItemIgnore = false;
    private bool usedSafetyCharm = false;
    private float autopotHpAlert, autopotMpAlert;

    public long LastFameTime { get; set; }
    public List<int> LastFameCIds { get; set; }

    public CharacterLink? Link { get; set; }

    private string? chalktext = null;
    private string? commandtext = null;
    private string? search = null;

    public AtomicInteger MesoValue { get; set; }

    private long totalExpGained = 0;

    private EventInstanceManager? eventInstance = null;


    private Job? jobModel = null;
    public Job JobModel
    {
        get
        {
            if (jobModel == null)
                jobModel = JobFactory.GetById(JobId);
            return jobModel;
        }
        set
        {
            jobModel = value;
            JobId = value.getId();
        }
    }

    private SkinColor? skinColorModel = null;
    public SkinColor SkinColorModel
    {
        get
        {
            if (skinColorModel == null)
                skinColorModel = SkinColorUtils.getById(Skincolor);
            return skinColorModel.Value;
        }
        set
        {
            skinColorModel = value;
            Skincolor = (int)value;
        }
    }


    public Messenger? Messenger { get; set; }

    private PlayerShop? playerShop = null;
    private Shop? shop = null;

    private Trade? trade = null;
    public MonsterBook Monsterbook { get; set; }
    public CashShop CashShopModel { get; set; }
    public PlayerSavedLocation SavedLocations { get; set; }

    private List<WeakReference<IMap>> lastVisitedMaps = new();
    private WeakReference<IMap?> ownedMap = new WeakReference<IMap?>(null);

    private ConcurrentDictionary<Monster, int> controlled = new();

    private ConcurrentDictionary<IMapObject, int> visibleMapObjects = new ConcurrentDictionary<IMapObject, int>();

    private Dictionary<int, CouponBuffEntry> activeCoupons = new();

    private Dictionary<int, Summon> summons = new();


    public byte[]? QuickSlotLoaded { get; set; }
    public QuickslotBinding? QuickSlotKeyMapped { get; set; }

    private Door? pdoor = null;


    private ScheduledFuture? dragonBloodSchedule;
    private ScheduledFuture? hpDecreaseTask;
    private ScheduledFuture? beholderHealingSchedule, beholderBuffSchedule, berserkSchedule;

    private ScheduledFuture? recoveryTask = null;
    private ScheduledFuture? extraRecoveryTask = null;

    private ScheduledFuture? pendantOfSpirit = null; //1122017
    private ScheduledFuture? cpqSchedule = null;

    private object chrLock = new object();
    private object evtLock = new object();

    private object prtLock = new object();

    /// <summary>
    /// PetId -> ItemId
    /// </summary>
    private Dictionary<long, HashSet<int>> excluded = new();
    private HashSet<int> excludedItems = new();
    private HashSet<int> disabledPartySearchInvites = new();

    private long portaldelay = 0, lastcombo = 0;
    private short combocounter = 0;
    private List<string> blockedPortals = new();
    public Dictionary<short, string> AreaInfo { get; set; } = new();
    public AutobanManager AutobanManager { get; set; }
    private bool isbanned = false;
    private bool blockCashShop = false;
    private bool allowExpGain = true;
    private byte pendantExp = 0, lastmobcount = 0;
    sbyte doorSlot = -1;

    public Dictionary<string, Events> Events { get; set; }

    private PartyQuest? partyQuest = null;

    private Dragon? dragon = null;


    private List<Ring> crushRings = new();
    private List<Ring> friendshipRings = new();
    private bool useCS;  //chaos scroll upon crafting item.
    private long npcCd;

    private byte extraHpRec = 0, extraMpRec = 0;
    private short extraRecInterval;
    private int targetHpBarHash = 0;
    private long targetHpBarTime = 0;
    private long nextWarningTime = 0;

    private bool pendingNameChange; //only used to change name on logout, not to be relied upon elsewhere
    private DateTimeOffset loginTime;
    private bool chasing = false;

    ReaderWriterLockSlim chLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    float expRateByLevel = 1;
    float mesoRateByLevel = 1;
    float dropRateByLevel = 1;
    public float ActualExpRate { get; private set; }
    public float ActualMesoRate { get; private set; }
    public float ActualDropRate { get; private set; }
    public float ActualQuestExpRate { get; private set; }
    public float ActualQuestMesoRate { get; private set; }
    public float ActualBossDropRate { get; private set; }

    public Job getJobStyle(byte opt)
    {
        return JobManager.GetJobStyleInternal(JobId, opt);
    }

    public Job getJobStyle()
    {
        return getJobStyle((byte)((this.getStr() > this.getDex()) ? 0x80 : 0x40));
    }


    public void updatePartySearchAvailability(bool psearchAvailable)
    {
        if (psearchAvailable)
        {
            if (PartySearch && getParty() == null)
            {
                this.getWorldServer().getPartySearchCoordinator().attachPlayer(this);
            }
        }
        else
        {
            if (PartySearch)
            {
                this.getWorldServer().getPartySearchCoordinator().detachPlayer(this);
            }
        }
    }

    public bool toggleRecvPartySearchInvite()
    {
        PartySearch = !PartySearch;

        if (PartySearch)
        {
            updatePartySearchAvailability(getParty() == null);
        }
        else
        {
            this.getWorldServer().getPartySearchCoordinator().detachPlayer(this);
        }

        return PartySearch;
    }

    public bool isRecvPartySearchInviteEnabled()
    {
        return PartySearch;
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
        Client.SetCharacterOnSessionTransitionState(this.getId());
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

    public void setOwlSearch(int Id)
    {
        owlSearch = Id;
    }

    public int getOwlSearch()
    {
        return owlSearch;
    }



    public void addCrushRing(Ring r)
    {
        crushRings.Add(r);
    }



    public int getRelationshipId()
    {
        return getChannelServer().Transport.GetRelationshipId(Id);
    }



    public int addDojoPointsByMap(int mapid)
    {
        int pts = 0;
        if (DojoPoints < 17000)
        {
            pts = 1 + ((mapid - 1) / 100 % 100) / 6;
            if (!MapId.isPartyDojo(this.getMapId()))
            {
                pts++;
            }
            this.DojoPoints += pts;
        }
        return pts;
    }

    public void addFame(int famechange)
    {
        this.Fame += famechange;
    }

    public void addFriendshipRing(Ring r)
    {
        friendshipRings.Add(r);
    }



    public void addMesosTraded(int gain)
    {
        this.mesosTraded += gain;
    }



    public void addSummon(int Id, Summon summon)
    {
        summons.AddOrUpdate(Id, summon);

        if (summon.isPuppet())
        {
            MapModel.addPlayerPuppet(this);
        }
    }

    public void addVisibleMapObject(IMapObject mo)
    {
        visibleMapObjects.TryAdd(mo, 0);
    }

    public void ban(string reason)
    {
        this.isbanned = true;
        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == Id).ExecuteUpdate(x => x.SetProperty(y => y.Banned, 1).SetProperty(y => y.Banreason, reason));
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
        var weapon_item = getInventory(InventoryType.EQUIPPED).getItem(EquipSlot.Weapon);
        if (weapon_item != null)
        {
            maxbasedamage = calculateMaxBaseDamage(watk, ItemInformationProvider.getInstance().getWeaponType(weapon_item.getItemId()));
        }
        else
        {
            if (JobModel.isA(Job.PIRATE) || JobModel.isA(Job.THUNDERBREAKER1))
            {
                double weapMulti = 3;
                if (JobId % 100 != 0)
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
    {
        //Used for skills that have mobCount at 1. (a/b)
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

    public void setClient(IChannelClient c)
    {
        this.Client = c;
    }

    private void RemoveWorldWatcher()
    {
        var channelServer = Client.CurrentServer;
        channelServer.OnWorldExpRateChanged -= UpdateActualExpRate;
        channelServer.OnWorldMesoRateChanged -= UpdateActualMesoRate;
        channelServer.OnWorldDropRateChanged -= UpdateActualDropRate;
        channelServer.OnWorldBossDropRateChanged -= UpdateActualBossDropRate;
        channelServer.OnWorldQuestRateChanged -= UpdateActualQuestExpRate;
        channelServer.OnWorldQuestRateChanged -= UpdateActualQuestMesoRate;
    }
    private void AddWorldWatcher()
    {
        var channelServer = Client.CurrentServer;
        channelServer.OnWorldExpRateChanged += UpdateActualExpRate;
        channelServer.OnWorldMesoRateChanged += UpdateActualMesoRate;
        channelServer.OnWorldDropRateChanged += UpdateActualDropRate;
        channelServer.OnWorldBossDropRateChanged += UpdateActualBossDropRate;
        channelServer.OnWorldQuestRateChanged += UpdateActualQuestExpRate;
        channelServer.OnWorldQuestRateChanged += UpdateActualQuestMesoRate;
    }

    public void LinkNewChannelClient(IChannelClient newClient)
    {
        RemoveWorldWatcher();

        this.setClient(newClient);

        AddWorldWatcher();
        this.setMap(newClient.CurrentServer.getMapFactory().getMap(getMapId()));
        var portal = MapModel.findClosestPlayerSpawnpoint(getPosition()) ?? MapModel.getPortal(0)!;
        this.setPosition(portal.getPosition());
        this.InitialSpawnPoint = portal.getId();
    }

    public string getMedalText()
    {
        var medalItem = getInventory(InventoryType.EQUIPPED).getItem(EquipSlot.Medal);
        return medalItem == null ? "" : "<" + ItemInformationProvider.getInstance().getName(medalItem.getItemId()) + "> ";
    }

    public void Hide(bool hide, bool login = false)
    {
        if (isGM() && hide != this.hidden)
        {
            if (!hide)
            {
                this.hidden = false;
                sendPacket(PacketCreator.getGMEffect(0x10, 0));
                List<BuffStat> dsstat = Collections.singletonList(BuffStat.DARKSIGHT);
                MapModel.broadcastGMMessage(this, PacketCreator.cancelForeignBuff(Id, dsstat), false);
                MapModel.broadcastSpawnPlayerMapObjectMessage(this, this, false);

                foreach (Summon ms in this.getSummonsValues())
                {
                    MapModel.broadcastNONGMMessage(this, PacketCreator.spawnSummon(ms, false), false);
                }

                foreach (IMapObject mo in this.MapModel.getMonsters())
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
                    MapModel.broadcastNONGMMessage(this, PacketCreator.removePlayerFromMap(getId()), false);
                }
                MapModel.broadcastGMMessage(this, PacketCreator.giveForeignBuff(Id, new BuffStatValue(BuffStat.DARKSIGHT, 0)), false);
                this.releaseControlledMonsters();
            }
            sendPacket(PacketCreator.enableActions());
        }
    }

    public void toggleHide(bool login)
    {
        Hide(!hidden, login);
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
        if (isLoggedinWorld())
        {
            UpdateLocalStats();
            sendPacket(PacketCreator.cancelBuff(buffstats));
            if (buffstats.Count > 0)
            {
                MapModel.broadcastMessage(this, PacketCreator.cancelForeignBuff(getId(), buffstats), false);
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
                        InventoryManipulator.removeFromSlot(Client, invType, item.getPosition(), item.getQuantity(), false);
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

    public FameStatus canGiveFame(IPlayer from)
    {
        if (this.isGM())
        {
            return FameStatus.OK;
        }
        else if (LastFameTime >= DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())
        {
            return FameStatus.NOT_TODAY;
        }
        else if (LastFameCIds.Contains(from.getId()))
        {
            return FameStatus.NOT_THIS_MONTH;
        }
        else
        {
            return FameStatus.OK;
        }
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
                var skill = SkillFactory.GetSkillTrust(skillId);
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
        foreach (IPlayer chr in MapModel.getAllPlayers())
        {
            var chrC = chr.Client;

            if (chrC != null)
            {     // propagate new job 3rd-person effects (FJ, Aran 1st strike, etc)
                this.sendDestroyData(chrC);
                this.sendSpawnData(chrC);
            }
        }

        TimerManager.getInstance().schedule(() =>
        {
            var thisChr = this;
            var map = thisChr.MapModel;

            if (map != null)
            {
                MapModel.broadcastMessage(thisChr, PacketCreator.showForeignEffect(thisChr.getId(), 8), false);
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

            if (PartySearch && TeamModel == null)
            {
                this.updatePartySearchAvailability(false);
                this.JobModel = newJob;
                this.updatePartySearchAvailability(true);
            }
            else
            {
                this.JobModel = newJob;
            }

            int spGain = 1;
            if (newJob.HasSPTable)
            {
                spGain += 2;
            }
            else
            {
                if (newJob.getId() % 10 == 2)
                {
                    spGain += 2;
                }

                if (YamlConfig.config.server.USE_ENFORCE_JOB_SP_RANGE)
                {
                    spGain = getChangedJobSp(newJob);
                }
            }

            if (spGain > 0)
            {
                gainSp(spGain, GameConstants.getSkillBook(newJob.getId()), true);
            }

            // thanks xinyifly for finding out missing AP awards (AP Reset can be used as a compass)
            if (newJob.getId() % 100 >= 1)
            {
                if (this.isCygnus())
                {
                    gainAp(7, true);
                }
                else
                {
                    if (YamlConfig.config.server.USE_STARTING_AP_4 || newJob.getId() % 10 >= 1)
                    {
                        gainAp(5, true);
                    }
                }
            }
            else
            {    // thanks Periwinks for noticing an AP shortage from lower levels
                if (YamlConfig.config.server.USE_STARTING_AP_4 && newJob.getId() % 1000 >= 1)
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
            int job_ = JobId % 1000; // lame temp "fix"
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
            int newJobId = newJobId;
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
                ChangeMaxHP(addhp);
                ChangeMaxMP(addmp);
                SetHP(ActualMaxHP);
                SetMP(ActualMaxMP);

                UpdateLocalStats();

                List<KeyValuePair<Stat, int>> statup = new(7);
                statup.Add(new(Stat.HP, HP));
                statup.Add(new(Stat.MP, MP));
                statup.Add(new(Stat.MAXHP, MaxHP));
                statup.Add(new(Stat.MAXMP, MaxMP));
                statup.Add(new(Stat.AVAILABLEAP, Ap));
                statup.Add(new(Stat.AVAILABLESP, RemainingSp[GameConstants.getSkillBook(JobId)]));
                statup.Add(new(Stat.JOB, JobId));
                sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }

            // setMPC(new PartyCharacter(this));
            silentPartyUpdate();

            if (dragon != null)
            {
                MapModel.broadcastMessage(PacketCreator.removeDragon(dragon.getObjectId()));
                dragon = null;
            }

            if (GuildModel != null)
            {
                GuildModel.broadcast(PacketCreator.jobMessage(0, JobId, Name), this.getId());
            }
            Family? family = getFamily();
            if (family != null)
            {
                family.broadcast(PacketCreator.jobMessage(1, JobId, Name), this.getId());
            }
            setMasteries(this.JobId);
            guildUpdate();

            broadcastChangeJob();

            if (newJob.HasDragon())
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
                    broadcastAcquaintances(6, "[" + GameConstants.ordinal(JobModel.Rank) + " Job] " + Name + " has just become a " + JobModel.Name + ".");    // thanks Vcoc for noticing job name appearing in uppercase here
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
        BuddyList.broadcast(packet);
        var family = getFamily();
        if (family != null)
        {
            family.broadcast(packet, Id);
        }

        var guild = getGuild();
        if (guild != null)
        {
            guild.broadcast(packet, Id);
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
            KeyMap.AddOrUpdate(key, keybinding);
        }
        else
        {
            KeyMap.Remove(key);
        }
    }

    public void changeQuickslotKeybinding(byte[] aQuickslotKeyMapped)
    {
        this.QuickSlotKeyMapped = new QuickslotBinding(aQuickslotKeyMapped);
    }

    public void broadcastStance(int newStance)
    {
        setStance(newStance);
        broadcastStance();
    }

    public void broadcastStance()
    {
        MapModel.broadcastMessage(this, PacketCreator.movePlayer(Id, this.getIdleMovement(), AbstractAnimatedMapObject.IDLE_MOVEMENT_PACKET_LENGTH), false);
    }



    private bool buffMapProtection()
    {
        int thisMapid = getMapId();
        int returnMapid = Client.CurrentServer.getMapFactory().getMap(thisMapid).getReturnMapId();

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
            foreach (var lv in lastVisitedMaps)
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

    public void partyOperationUpdate(ITeam party, List<IPlayer>? exPartyMembers)
    {
        List<WeakReference<IMap>> mapids;

        Monitor.Enter(petLock);
        try
        {
            mapids = new(lastVisitedMaps);
        }
        finally
        {
            Monitor.Exit(petLock);
        }

        List<IPlayer> partyMembers = new();
        foreach (var mc in (exPartyMembers != null) ? exPartyMembers : this.getPartyMembersOnline())
        {
            if (mc.isLoggedinWorld())
            {
                partyMembers.Add(mc);
            }
        }

        IPlayer? partyLeaver = null;
        if (exPartyMembers != null)
        {
            partyMembers.Remove(this);
            partyLeaver = this;
        }

        IMap map = MapModel;
        List<MapItem>? partyItems = null;

        int partyId = exPartyMembers != null ? -1 : this.getPartyId();
        foreach (var mapRef in mapids)
        {
            if (mapRef.TryGetTarget(out var mapObj))
            {
                List<MapItem> partyMapItems = mapObj.updatePlayerItemDropsToParty(partyId, Id, partyMembers, partyLeaver);
                if (MapModel.GetHashCode() == mapObj.GetHashCode())
                {
                    partyItems = partyMapItems;
                }
            }
        }

        if (partyItems != null && exPartyMembers == null)
        {
            MapModel.updatePartyItemDropsToNewcomer(this, partyItems);
        }

        updatePartyTownDoors(party, this, partyLeaver, partyMembers);
    }

    private static void addPartyPlayerDoor(IPlayer target)
    {
        var targetDoor = target.getPlayerDoor();
        if (targetDoor != null)
        {
            target.applyPartyDoor(targetDoor, true);
        }
    }

    private static void removePartyPlayerDoor(ITeam party, IPlayer target)
    {
        target.removePartyDoor(party);
    }


    private static void updatePartyTownDoors(ITeam party, IPlayer target, IPlayer? partyLeaver, List<IPlayer> partyMembers)
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

            foreach (IPlayer pchr in partyMembers)
            {
                Door? door = partyDoors.GetValueOrDefault(pchr.getId());
                if (door != null)
                {
                    door.updateDoorPortal(pchr);
                }
            }

            foreach (Door door in partyDoors.Values)
            {
                foreach (IPlayer pchar in partyMembers)
                {
                    DoorObject mdo = door.getTownDoor();
                    mdo.sendDestroyData(pchar.Client, true);
                    pchar.removeVisibleMapObject(mdo);
                }
            }

            if (partyLeaver != null)
            {
                var leaverDoors = partyLeaver.getDoors();
                foreach (Door door in leaverDoors)
                {
                    foreach (IPlayer pchar in partyMembers)
                    {
                        DoorObject mdo = door.getTownDoor();
                        mdo.sendDestroyData(pchar.Client, true);
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
                    foreach (IPlayer pchar in partyMembers)
                    {
                        DoorObject mdo = door.getTownDoor();
                        mdo.sendSpawnData(pchar.Client);
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
                    mdo.sendDestroyData(partyLeaver.Client, true);
                    partyLeaver.removeVisibleMapObject(mdo);
                }
            }

            foreach (Door door in leaverDoors)
            {
                DoorObject mdo = door.getTownDoor();
                mdo.sendDestroyData(partyLeaver.Client, true);
                partyLeaver.removeVisibleMapObject(mdo);
            }

            foreach (Door door in leaverDoors)
            {
                door.updateDoorPortal(partyLeaver);

                DoorObject mdo = door.getTownDoor();
                mdo.sendSpawnData(partyLeaver.Client);
                partyLeaver.addVisibleMapObject(mdo);
            }
        }
    }

    public void notifyMapTransferToPartner(int mapid)
    {
        if (PartnerId > 0)
        {
            var partner = getWorldServer().getPlayerStorage().getCharacterById(PartnerId);
            if (partner != null && partner.isLoggedinWorld())
            {
                partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(Id, mapid));
            }
        }
    }

    public void removeIncomingInvites()
    {
        Client.CurrentServer.Service.RemovePlayerIncomingInvites(Id);
    }

    public void checkBerserk(bool isHidden)
    {
        berserkSchedule?.cancel(false);

        IPlayerStats chr = this;
        if (JobModel.Equals(Job.DARKKNIGHT))
        {
            Skill BerserkX = SkillFactory.GetSkillTrust(DarkKnight.BERSERK);
            int skilllevel = getSkillLevel(BerserkX);
            if (skilllevel > 0)
            {
                berserk = (this.HP * 100 / this.ActualMaxHP) < BerserkX.getEffect(skilllevel).getX();
                berserkSchedule = TimerManager.getInstance().register(() =>
                {
                    if (awayFromWorld.Get())
                    {
                        return;
                    }

                    sendPacket(PacketCreator.showOwnBerserk(skilllevel, berserk));
                    if (!isHidden)
                    {
                        MapModel.broadcastMessage(this, PacketCreator.showBerserk(Id, skilllevel, berserk), false);
                    }
                    else
                    {
                        MapModel.broadcastGMMessage(this, PacketCreator.showBerserk(Id, skilllevel, berserk), false);
                    }
                }
                , 5000, 3000);
            }
        }
    }

    public void checkMessenger()
    {
        if (Messenger != null && MessengerPosition < 4 && MessengerPosition > -1)
        {
            var worldz = getWorldServer();
            worldz.silentJoinMessenger(Messenger.getId(), new MessengerCharacter(this, MessengerPosition), MessengerPosition);
            worldz.updateMessenger(Messenger.getId(), Name, Client.Channel);
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
                    List<IPlayer> partyMembers = this.getPartyMembersOnSameMap();
                    if (!ItemId.isPartyAllCure(itemId))
                    {
                        var mse = ii.getItemEffect(itemId);
                        if (partyMembers.Count > 0)
                        {
                            foreach (IPlayer mc in partyMembers)
                            {
                                if (mc.isAlive())
                                {
                                    mse?.applyTo(mc);
                                }
                            }
                        }
                        else if (this.isAlive())
                        {
                            mse?.applyTo(this);
                        }
                    }
                    else
                    {
                        if (partyMembers.Count > 0)
                        {
                            foreach (IPlayer mc in partyMembers)
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
                    ii.getItemEffect(itemId)?.applyTo(this);
                }

                if (ItemId.isMonsterCard(itemId))
                {
                    this.Monsterbook.addCard(itemId);
                }
                return true;
            }
        }
        return false;
    }

    public void pickupItem(IMapObject? ob, int petIndex = -1)
    {
        // yes, one picks the IMapObject, not the MapItem
        if (ob == null)
        {
            // pet index refers to the one picking up the item
            return;
        }

        var pickerProcessor = new PlayerPickupProcessor(this, petIndex);
        if (ob is MapItem mapitem)
        {
            pickerProcessor.Handle(mapitem);
        }
    }


    public int countItem(int itemid)
    {
        return Bag[ItemConstants.getInventoryType(itemid)].countById(itemid);
    }

    public bool canHold(int itemid, int quantity = 1)
    {
        return Client.getAbstractPlayerInteraction().canHold(itemid, quantity);
    }

    public bool canHoldUniques(List<int> itemids)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        return !itemids.Any(x => ii.isPickupRestricted(x) && haveItem(x));
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
            addCooldown(Corsair.BATTLE_SHIP, getChannelServer().getCurrentTime(), cooldown * 1000);
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


    private void nextPendingRequest(IChannelClient c)
    {
        CharacterNameAndId? pendingBuddyRequest = c.OnlinedCharacter.getBuddylist().pollPendingRequest();
        if (pendingBuddyRequest != null)
        {
            c.sendPacket(PacketCreator.requestBuddylistAdd(pendingBuddyRequest.id, c.OnlinedCharacter.getId(), pendingBuddyRequest.name));
        }
    }

    private void notifyRemoteChannel(IChannelClient c, int remoteChannel, int otherCid, BuddyList.BuddyOperation operation)
    {
        var player = c.OnlinedCharacter;
        if (remoteChannel != -1)
        {
            c.getWorldServer().buddyChanged(otherCid, player.getId(), player.getName(), c.Channel, operation);
        }
    }

    public void deleteBuddy(int otherCid)
    {
        if (BuddyList.containsVisible(otherCid))
        {
            notifyRemoteChannel(Client, getWorldServer().find(otherCid), otherCid, BuddyList.BuddyOperation.DELETED);
        }
        BuddyList.remove(otherCid);
        sendPacket(PacketCreator.updateBuddylist(BuddyList.getBuddies()));
        nextPendingRequest(Client);
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

            if (HP < ActualMaxHP)
            {
                if (healHP > 0)
                {
                    sendPacket(PacketCreator.showOwnRecovery(healHP));
                    MapModel.broadcastMessage(this, PacketCreator.showRecovery(Id, healHP), false);
                }
            }

            UpdateStatsChunk(() =>
            {
                ChangeHP(healHP);
                ChangeMP(healMP);
            });

        }, healInterval, healInterval);
    }

    public void disbandGuild()
    {
        if (GuildId < 1 || GuildRank != 1)
        {
            return;
        }
        try
        {
            GuildModel?.disbandGuild();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
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
                    {
                        // check discovered thanks to Croosade dev team
                        cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                    }
                }
            }
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

    /// <summary>
    /// F1 - F8的表情？
    /// </summary>
    /// <param name="emote"></param>
    public void changeFaceExpression(int emote)
    {
        long timeNow = getChannelServer().getCurrentTime();
        // IClient allows changing every 2 seconds. Give it a little bit of overhead for packet delays.
        if (timeNow - lastExpression > 1500)
        {
            lastExpression = timeNow;
            MapModel.broadcastMessage(this, PacketCreator.facialExpression(this, emote), false);
        }
    }

    public void doHurtHp()
    {
        if (!(this.getInventory(InventoryType.EQUIPPED).findById(MapModel.getHPDecProtect()) != null || buffMapProtection()))
        {
            UpdateStatsChunk(() =>
            {
                ChangeHP(-MapModel.getHPDec(), false);
            });
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

    public void equipChanged()
    {
        MapModel.broadcastUpdateCharLookMessage(this, this);
        equipchanged = true;
        UpdateLocalStats();
        if (Messenger != null)
        {
            getWorldServer().updateMessenger(Messenger, getName(), getWorld(), Client.Channel);
        }
    }

    public enum FameStatus
    {

        OK, NOT_TODAY, NOT_THIS_MONTH
    }

    public void forceUpdateItem(Item item)
    {
        List<ModifyInventory> mods = [new ModifyInventory(3, item), new ModifyInventory(0, item)];
        sendPacket(PacketCreator.modifyInventory(true, mods));
    }

    private KeyValuePair<int, int> applyFame(int delta)
    {
        Monitor.Enter(petLock);
        try
        {
            int newFame = Fame + delta;
            if (newFame < -30000)
            {
                delta = -(30000 + Fame);
            }
            else if (newFame > 30000)
            {
                delta = 30000 - Fame;
            }

            Fame += delta;
            return new(Fame, delta);
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

    public bool gainFame(int delta, IPlayer? fromPlayer, int mode)
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



    public void genericGuildMessage(int code)
    {
        this.sendPacket(GuildPackets.genericGuildMessage((byte)code));
    }

    public int getAccountID()
    {
        return AccountId;
    }



    public int getAllianceRank()
    {
        return AllianceRank;
    }



    public int getBattleshipHp()
    {
        return battleshipHp;
    }

    public BuddyList getBuddylist()
    {
        return BuddyList;
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

    //private void dropWorstEffectFromItemEffectHolder(BuffStat mbs)
    //{
    //    int min = int.MaxValue;
    //    int srcid = -1;
    //    foreach (var bpl in buffEffects)
    //    {
    //        BuffStatValueHolder? mbsvh = bpl.Value.GetValueOrDefault(mbs);
    //        if (mbsvh != null)
    //        {
    //            if (mbsvh.value < min)
    //            {
    //                min = mbsvh.value;
    //                srcid = bpl.Key;
    //            }
    //        }
    //    }

    //    removeEffectFromItemEffectHolder(srcid, mbs);
    //}

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


    public int getChair()
    {
        return chair.get();
    }

    public string? getChalkboard()
    {
        return this.chalktext;
    }

    public IChannelClient getClient()
    {
        return Client;
    }

    public AbstractPlayerInteraction getAbstractPlayerInteraction()
    {
        return Client.getAbstractPlayerInteraction();
    }



    public List<Ring> getCrushRings()
    {
        crushRings.Sort();
        return crushRings;
    }

    public void changeCI(int type)
    {
        this.ci = type;
    }
    public int getCurrentCI()
    {
        return ci;
    }

    public void changePage(int page)
    {
        this.currentPage = page;
    }
    public int getCurrentPage()
    {
        return currentPage;
    }
    public void changeTab(int tab)
    {
        this.currentTab = tab;
    }

    public int getCurrentTab()
    {
        return currentTab;
    }

    public void changeType(int type)
    {
        this.currentType = type;
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
        return DojoPoints;
    }

    public int getDojoStage()
    {
        return LastDojoStage;
    }

    public ICollection<Door> getDoors()
    {
        Monitor.Enter(prtLock);
        try
        {
            return (TeamModel != null ? new List<Door>(TeamModel.getDoors().Values.ToList()) : (pdoor != null ? Collections.singletonList(pdoor) : new()));
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
        return getDoors().FirstOrDefault(x => x.getTownPortal().getId() == 0x80);
    }

    public void applyPartyDoor(Door door, bool partyUpdate)
    {
        ITeam? chrParty;
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
                chrParty.addDoor(Id, door);
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
        ITeam? chrParty;

        Monitor.Enter(prtLock);
        try
        {
            chrParty = getParty();
            if (chrParty != null)
            {
                chrParty.removeDoor(Id);
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

    public void removePartyDoor(ITeam formerParty)
    {    // player is no longer registered at this party
        formerParty.removeDoor(Id);
    }

    public int getEnergyBar()
    {
        return energybar;
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



    public void resetExcluded(long petId)
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

    public void addExcluded(long petId, int x)
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
        var petExcluded = this.getExcluded();

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

    public void exportExcludedItems(IChannelClient c)
    {
        var petExcluded = this.getExcluded();
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

    public Dictionary<long, HashSet<int>> getExcluded()
    {
        chLock.EnterReadLock();
        try
        {
            return excluded.ToDictionary();
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
        return ExpValue.get();
    }



    public bool hasNoviceExpRate()
    {
        return YamlConfig.config.server.USE_ENFORCE_NOVICE_EXPRATE && isBeginnerJob() && Level < 11;
    }

    public float getExpRate()
    {
        if (hasNoviceExpRate())
        {   // base exp rate 1x for early levels idea thanks to Vcoc
            return 1;
        }

        return ActualExpRate;
    }

    public float getCouponExpRate()
    {
        return expCoupon;
    }

    public float getRawExpRate()
    {
        return expRateByLevel;
    }

    public float getDropRate()
    {
        return ActualDropRate;
    }

    public float getCouponDropRate()
    {
        return dropCoupon;
    }

    public float getRawDropRate()
    {
        return dropRateByLevel;
    }

    public float getBossDropRate()
    {
        return ActualBossDropRate;
    }

    public float getMesoRate()
    {
        return ActualMesoRate;
    }

    public float getCouponMesoRate()
    {
        return mesoCoupon;
    }

    public float getRawMesoRate()
    {
        return mesoRateByLevel;
    }

    public float getQuestExpRate()
    {
        if (hasNoviceExpRate())
        {
            return 1;
        }

        return ActualQuestExpRate;
    }

    public float getQuestMesoRate()
    {
        return ActualQuestMesoRate;
    }

    public float getCardRate(int itemid)
    {
        float rate = 100.0f;

        if (itemid == 0)
        {
            StatEffect? mseMeso = getBuffEffect(BuffStat.MESO_UP_BY_ITEM);
            if (mseMeso != null)
            {
                rate += mseMeso.getCardRate(getMapId(), itemid);
            }
        }
        else
        {
            StatEffect? mseItem = getBuffEffect(BuffStat.ITEM_UP_BY_ITEM);
            if (mseItem != null)
            {
                rate += mseItem.getCardRate(getMapId(), itemid);
            }
        }

        return rate / 100;
    }

    public int getFace()
    {
        return Face;
    }

    public int getFame()
    {
        return Fame;
    }

    public Family? getFamily()
    {
        return familyEntry?.getFamily();
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
        return FamilyId;
    }

    public bool getFinishedDojoTutorial()
    {
        return FinishedDojoTutorial;
    }

    public List<Ring> getFriendshipRings()
    {
        friendshipRings.Sort();
        return friendshipRings;
    }

    public int getGender()
    {
        return Gender;
    }

    public bool isMale()
    {
        return getGender() == 0;
    }

    public IGuild? getGuild()
    {
        try
        {
            return AllGuildStorage.GetGuildById(GuildId);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            return null;
        }
    }

    public IAlliance? getAlliance()
    {
        return getGuild()?.AllianceModel;
    }

    public int getGuildId()
    {
        return GuildId;
    }

    public int getGuildRank()
    {
        return GuildRank;
    }

    public int getHair()
    {
        return Hair;
    }



    public int getId()
    {
        return Id;
    }


    public int getInitialSpawnpoint()
    {
        return InitialSpawnPoint;
    }


    public Job getJob()
    {
        return JobModel;
    }

    public int getJobId()
    {
        return JobId;
    }

    public int getJobRank()
    {
        return JobRank;
    }

    public int getJobRankMove()
    {
        return JobRank;
    }

    public int getJobType()
    {
        return JobId / 1000;
    }

    public Dictionary<int, KeyBinding> getKeymap()
    {
        return KeyMap.GetDataSource();
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
        return Level;
    }

    public int getFh()
    {
        Point pos = this.getPosition();
        pos.Y -= 6;

        return MapModel.getFootholds()?.findBelow(pos)?.getY1() ?? 0;
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
        return isCygnus() ? 120 : NumericConfig.MaxLevel;
    }

    public int getMaxLevel()
    {
        if (!YamlConfig.config.server.USE_ENFORCE_JOB_LEVEL_RANGE || isGmJob())
        {
            return getMaxClassLevel();
        }

        return JobModel.MaxLevel;
    }

    public int getMeso()
    {
        return MesoValue.get();
    }


    public int getMesosTraded()
    {
        return mesosTraded;
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
        setTargetHpBarTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    public void resetPlayerAggro()
    {
        if (getChannelServer().ServerMessageController.unregisterDisabledServerMessage(Id))
        {
            Client.announceServerMessage();
        }

        setTargetHpBarHash(0);
        setTargetHpBarTime(0);
    }



    public MonsterBook getMonsterBook()
    {
        return Monsterbook;
    }
    public string getName()
    {
        return Name;
    }

    public int getNoPets()
    {
        Monitor.Enter(petLock);
        try
        {
            return pets.Count(x => x != null);
        }
        finally
        {
            Monitor.Exit(petLock);
        }
    }

    public PlayerShop? getPlayerShop()
    {
        return playerShop;
    }

    public void closePartySearchInteractions()
    {
        this.getWorldServer().getPartySearchCoordinator().unregisterPartyLeader(this);
        if (PartySearch)
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

        Client.closePlayerScriptInteractions();
        resetPlayerAggro();
    }

    public void closeNpcShop()
    {
        setShop(null);
    }

    public void closeTrade()
    {
        getTrade()?.CancelTrade(TradeResult.PARTNER_CANCEL);
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
                    InventoryManipulator.addFromDrop(this.Client, iItem, false);
                }
                else if (mpsi.isExist())
                {
                    InventoryManipulator.addFromDrop(this.Client, mpsi.getItem(), true);
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

    public void closePlayerMessenger()
    {
        Messenger? m = Messenger;
        if (m == null)
        {
            return;
        }

        var w = getWorldServer();
        MessengerCharacter messengerplayer = new MessengerCharacter(this, this.MessengerPosition);

        w.leaveMessenger(m.getId(), messengerplayer);
        this.setMessenger(null);
        this.setMessengerPosition(4);
    }


    public int getPossibleReports()
    {
        return possibleReports;
    }

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
        return Rank;
    }

    public int getRankMove()
    {
        return RankMove;
    }

    public void clearSavedLocation(SavedLocationType type)
    {
        SavedLocations.AddOrUpdate(type, null);
    }

    public int peekSavedLocation(string type)
    {
        var sl = SavedLocations.GetData(type);
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

    public string? getSearch()
    {
        return search;
    }

    public Shop? getShop()
    {
        return shop;
    }



    public SkinColor getSkinColor()
    {
        return SkinColorModel;
    }


    public StatEffect? getStatForBuff(BuffStat effect)
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        try
        {
            BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(effect);
            return mbsvh?.effect;
        }
        finally
        {
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    public Storage getStorage()
    {
        return Storage;
    }

    public ICollection<Summon> getSummonsValues()
    {
        return summons.Values;
    }

    public void clearSummons()
    {
        summons.Clear();
    }

    public Summon? getSummonByKey(int Id)
    {
        return summons.GetValueOrDefault(Id);
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
        return VanquisherKills;
    }

    public int getVanquisherStage()
    {
        return VanquisherStage;
    }

    public IMapObject[] getVisibleMapObjects()
    {
        return visibleMapObjects.Keys.ToArray();
    }

    public int getWorld()
    {
        return World;
    }

    public IWorld getWorldServer()
    {
        return Server.getInstance().getWorld(World);
    }

    public int gmLevel()
    {
        return Client.AccountEntity!.GMLevel;
    }

    private void guildUpdate()
    {
        if (this.GuildId < 1)
        {
            return;
        }

        try
        {
            if (GuildModel != null)
                GuildModel.memberLevelJobUpdate(this);

            //Server.getInstance().getGuild(guildid, world, mgc).gainGP(40);
            if (AllianceModel != null)
            {
                AllianceModel.broadcastMessage(GuildPackets.updateAllianceJobLevel(this), Id, -1);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    public void handleEnergyChargeGain()
    {
        // to get here energychargelevel has to be > 0
        Skill energycharge = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.ENERGY_CHARGE) : SkillFactory.GetSkillTrust(Marauder.ENERGY_CHARGE);
        StatEffect ceffect;
        ceffect = energycharge.getEffect(getSkillLevel(energycharge));
        var tMan = TimerManager.getInstance();
        if (energybar < 10000)
        {
            energybar += 102;
            if (energybar > 10000)
            {
                energybar = 10000;
            }
            var stat = new BuffStatValue(BuffStat.ENERGY_CHARGE, energybar);
            setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
            sendPacket(PacketCreator.giveBuff(energybar, 0, stat));
            sendPacket(PacketCreator.showOwnBuffEffect(energycharge.getId(), 2));
            MapModel.broadcastPacket(this, PacketCreator.showBuffEffect(Id, energycharge.getId(), 2));
            MapModel.broadcastPacket(this, PacketCreator.giveForeignPirateBuff(Id, energycharge.getId(),
                    ceffect.getDuration(), stat));
        }
        if (energybar >= 10000 && energybar < 11000)
        {
            energybar = 15000;
            IPlayer chr = this;
            tMan.schedule(() =>
            {
                energybar = 0;
                var stat = new BuffStatValue(BuffStat.ENERGY_CHARGE, energybar);
                setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
                sendPacket(PacketCreator.giveBuff(energybar, 0, stat));
                MapModel.broadcastPacket(chr, PacketCreator.cancelForeignFirstDebuff(Id, ((long)1) << 50));

            }, ceffect.getDuration());
        }
    }

    public void handleOrbconsume()
    {
        int skillid = isCygnus() ? DawnWarrior.COMBO : Crusader.COMBO;
        var combo = SkillFactory.GetSkillTrust(skillid);
        var stat = new BuffStatValue(BuffStat.COMBO, 1);
        setBuffedValue(BuffStat.COMBO, 1);
        sendPacket(PacketCreator.giveBuff(
            skillid,
            combo.getEffect(getSkillLevel(combo)).getDuration() + (int)((getBuffedStarttime(BuffStat.COMBO) ?? 0) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            stat));
        MapModel.broadcastMessage(this, PacketCreator.giveForeignBuff(getId(), stat), false);
    }

    public bool hasEntered(string script)
    {
        return entered.Values.Any(x => x == script);
    }

    public bool hasEntered(string script, int mapId)
    {
        return entered.GetValueOrDefault(mapId) == script;
    }

    public void hasGivenFame(IPlayer to)
    {
        try
        {
            LastFameTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            LastFameCIds.Add(to.getId());

            using var dbContext = new DBContext();
            var dbModel = new Famelog()
            {
                Characterid = Id,
                CharacteridTo = to.Id
            };
            dbContext.Famelogs.Add(dbModel);
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    public bool hasMerchant()
    {
        return HasMerchant;
    }

    public bool haveItem(int itemid)
    {
        return getItemQuantity(itemid, ItemConstants.isEquipment(itemid)) > 0;
    }

    public bool haveCleanItem(int itemid)
    {
        return getCleanItemQuantity(itemid, ItemConstants.isEquipment(itemid)) > 0;
    }

    public bool HasEmptySlotByItem(int itemId)
    {
        return getInventory(ItemConstants.getInventoryType(itemId)).getNextFreeSlot() > -1;
    }

    public bool hasEmptySlot(sbyte invType)
    {
        return getInventory(InventoryTypeUtils.getByType(invType)).getNextFreeSlot() > -1;
    }

    public void increaseGuildCapacity()
    {
        var guild = getGuild();
        if (guild == null)
            return;

        int cost = GuildManager.getIncreaseGuildCost(guild.getCapacity());

        if (getMeso() < cost)
        {
            dropMessage(1, "You don't have enough mesos.");
            return;
        }

        if (guild.increaseCapacity())
        {
            gainMeso(-cost, true, false, true);
        }
        else
        {
            dropMessage(1, "Your guild already reached the maximum capacity of players.");
        }
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
        return JobModel.IsGmJob();
    }

    public bool isCygnus()
    {
        return JobModel.Type == JobType.Cygnus;
    }

    public bool isAran()
    {
        return JobModel.IsAran();
    }

    public bool isBeginnerJob()
    {
        return JobModel.IsBeginningJob();
    }

    public bool isGM()
    {
        return gmLevel() > 1;
    }

    public bool isHidden()
    {
        return hidden;
    }

    public bool isMapObjectVisible(IMapObject mo)
    {
        return visibleMapObjects.ContainsKey(mo);
    }

    public bool isGuildLeader()
    {
        // true on guild master or jr. master
        return GuildId > 0 && GuildRank < 3;
    }

    public bool attemptCatchFish(int baitLevel)
    {
        return YamlConfig.config.server.USE_FISHING_SYSTEM && MapId.isFishingArea(getMapId()) &&
                this.getPosition().Y > 0 &&
                ItemConstants.isFishingChair(chair.get()) &&
                this.getWorldServer().FishingInstance.RegisterFisherPlayer(this, baitLevel);
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

        var arena = this.getAriantColiseum();
        if (arena != null)
        {
            arena.leaveArena(this);
        }
    }

    private int getChangedJobSp(Job newJob)
    {
        int curSp = getUsedSp(newJob) + getJobRemainingSp(newJob);
        int spGain = 0;
        int expectedSp = JobManager.GetJobLevelSp(Level - 10, newJob.getId(), newJob.Rank);
        if (curSp < expectedSp)
        {
            spGain += (expectedSp - curSp);
        }

        return getSpGain(spGain, curSp, newJob);
    }

    private int getUsedSp(Job job)
    {
        int jobId = job.getId();
        return getSkills().Where(x => GameConstants.isInJobTree(x.Key.getId(), jobId) && !x.Key.isBeginnerSkill()).Sum(x => x.Value.skillevel);
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
        int maxSp = JobManager.GetJobMaxSp(job);

        spGain = Math.Min(spGain, maxSp - curSp);
        return spGain;
    }

    private void levelUpGainSp()
    {
        if (JobModel.Rank == 0)
        {
            return;
        }

        int spGain = 3;
        if (YamlConfig.config.server.USE_ENFORCE_JOB_SP_RANGE && !JobModel.HasSPTable)
        {
            spGain = getSpGain(spGain, JobModel);
        }

        if (spGain > 0)
        {
            gainSp(spGain, GameConstants.getSkillBook(JobId), true);
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
            if (YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP && isBeginner && Level < 11)
            {
                Monitor.Enter(effLock);
                statLock.EnterWriteLock();
                try
                {
                    gainAp(5, true);

                    int str = 0, dex = 0;
                    if (Level < 6)
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
                    if (Level > 10)
                    {
                        if (Level <= 17)
                        {
                            remainingAp += 2;
                        }
                        else if (Level < 77)
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
            else if (JobModel.isA(Job.WARRIOR) || JobModel.isA(Job.DAWNWARRIOR1))
            {
                improvingMaxHP = isCygnus() ? SkillFactory.GetSkillTrust(DawnWarrior.MAX_HP_INCREASE) : SkillFactory.GetSkillTrust(Warrior.IMPROVED_MAXHP);
                if (JobModel.isA(Job.CRUSADER))
                {
                    improvingMaxMP = SkillFactory.GetSkillTrust(1210000);
                }
                else if (JobModel.isA(Job.DAWNWARRIOR2))
                {
                    improvingMaxMP = SkillFactory.GetSkillTrust(11110000);
                }
                improvingMaxHPLevel = getSkillLevel(improvingMaxHP);
                addhp += Randomizer.rand(24, 28);
                addmp += Randomizer.rand(4, 6);
            }
            else if (JobModel.isA(Job.MAGICIAN) || JobModel.isA(Job.BLAZEWIZARD1))
            {
                improvingMaxMP = isCygnus() ? SkillFactory.GetSkillTrust(BlazeWizard.INCREASING_MAX_MP) : SkillFactory.GetSkillTrust(Magician.IMPROVED_MAX_MP_INCREASE);
                improvingMaxMPLevel = getSkillLevel(improvingMaxMP);
                addhp += Randomizer.rand(10, 14);
                addmp += Randomizer.rand(22, 24);
            }
            else if (JobModel.isA(Job.BOWMAN) || JobModel.isA(Job.THIEF) || (JobId > 1299 && JobId < 1500))
            {
                addhp += Randomizer.rand(20, 24);
                addmp += Randomizer.rand(14, 16);
            }
            else if (JobModel.isA(Job.GM))
            {
                addhp += 30000;
                addmp += 30000;
            }
            else if (JobModel.isA(Job.PIRATE) || JobModel.isA(Job.THUNDERBREAKER1))
            {
                improvingMaxHP = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.IMPROVE_MAX_HP) : SkillFactory.GetSkillTrust(Brawler.IMPROVE_MAX_HP);
                improvingMaxHPLevel = getSkillLevel(improvingMaxHP);
                addhp += Randomizer.rand(22, 28);
                addmp += Randomizer.rand(18, 23);
            }
            else if (JobModel.isA(Job.ARAN1))
            {
                addhp += Randomizer.rand(44, 48);
                int aids = Randomizer.rand(4, 8);
                addmp += (int)(aids + Math.Floor(aids * 0.1));
            }
            if (improvingMaxHPLevel > 0 && (JobModel.isA(Job.WARRIOR) || JobModel.isA(Job.PIRATE) || JobModel.isA(Job.DAWNWARRIOR1) || JobModel.isA(Job.THUNDERBREAKER1)))
            {
                addhp += improvingMaxHP!.getEffect(improvingMaxHPLevel).getX();
            }
            if (improvingMaxMPLevel > 0 && (JobModel.isA(Job.MAGICIAN) || JobModel.isA(Job.CRUSADER) || JobModel.isA(Job.BLAZEWIZARD1)))
            {
                addmp += improvingMaxMP!.getEffect(improvingMaxMPLevel).getX();
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

            ChangeMaxHP(addhp);
            ChangeMaxMP(addmp);
            SetHP(ActualMaxHP);
            SetMP(ActualMaxMP);

            if (takeexp)
            {
                ExpValue.addAndGet(-ExpTable.getExpNeededForLevel(Level));
                if (ExpValue.get() < 0)
                {
                    ExpValue.set(0);
                }
            }

            setLevel(Level + 1);

            int maxClassLevel = getMaxClassLevel();
            if (Level >= maxClassLevel)
            {
                ExpValue.set(0);

                if (Level == maxClassLevel)
                {
                    if (!this.isGM())
                    {
                        if (YamlConfig.config.server.PLAYERNPC_AUTODEPLOY)
                        {
                            ThreadManager.getInstance().newTask(() =>
                            {
                                PlayerNPC.spawnPlayerNPC(GameConstants.getHallOfFameMapid(JobModel), this);
                            });
                        }

                        string names = (getMedalText() + Name);
                        getChannelServer().Transport.BroadcastMessage(PacketCreator.serverNotice(6, string.Format(GameConstants.LEVEL_200, names, maxClassLevel, names)));
                    }
                }

                setLevel(maxClassLevel);
            }

            levelUpGainSp();

            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                UpdateLocalStats();

                List<KeyValuePair<Stat, int>> statup = new(10);
                statup.Add(new(Stat.AVAILABLEAP, Ap));
                statup.Add(new(Stat.AVAILABLESP, RemainingSp[GameConstants.getSkillBook(JobId)]));
                statup.Add(new(Stat.EXP, ExpValue.get()));
                statup.Add(new(Stat.LEVEL, Level));
                statup.Add(new(Stat.MAXHP, MaxHP));
                statup.Add(new(Stat.MAXMP, MaxMP));
                statup.Add(new(Stat.HP, HP));
                statup.Add(new(Stat.MP, MP));
                statup.Add(new(Stat.STR, Str));
                statup.Add(new(Stat.DEX, Dex));

                sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }

            MapModel.broadcastMessage(this, PacketCreator.showForeignEffect(getId(), 0), false);
            // setMPC(new PartyCharacter(this));
            silentPartyUpdate();

            if (GuildModel != null)
            {
                GuildModel.broadcast(PacketCreator.levelUpMessage(2, Level, Name), this.getId());
            }

            if (Level % 20 == 0)
            {
                if (YamlConfig.config.server.USE_ADD_SLOTS_BY_LEVEL == true)
                {
                    if (!isGM())
                    {
                        for (byte i = 1; i < 5; i++)
                        {
                            gainSlots(i, 4, true);
                        }

                        this.yellowMessage("You reached level " + Level + ". Congratulations! As a token of your success, your inventory has been expanded a little bit.");
                    }
                }
                if (YamlConfig.config.server.USE_ADD_RATES_BY_LEVEL == true)
                {
                    this.yellowMessage("You managed to get level " + Level + "! Getting experience and items seems a little easier now, huh?");
                }
            }

            if (YamlConfig.config.server.USE_PERFECT_PITCH && Level >= 30)
            {
                //milestones?
                if (InventoryManipulator.checkSpace(Client, ItemId.PERFECT_PITCH, 1, ""))
                {
                    InventoryManipulator.addById(Client, ItemId.PERFECT_PITCH, 1, "");
                }
            }
            else if (Level == 10)
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

            var familyEntry = getFamilyEntry();
            if (familyEntry != null)
            {
                familyEntry.giveReputationToSenior(YamlConfig.config.server.FAMILY_REP_PER_LEVELUP, true);
                var senior = familyEntry.getSenior();
                if (senior != null)
                { //only send the message to direct senior
                    var seniorChr = senior.getChr();
                    if (seniorChr != null)
                    {
                        seniorChr.sendPacket(PacketCreator.levelUpMessage(1, Level, getName()));
                    }
                }
            }
        }
    }

    void UpdateActualRate()
    {
        UpdateActualExpRate();
        UpdateActualMesoRate();
        UpdateActualDropRate();
        UpdateActualBossDropRate();
        UpdateActualQuestExpRate();
        UpdateActualQuestMesoRate();
    }

    void UpdateActualExpRate()
    {
        ActualExpRate = expRateByLevel * getChannelServer().WorldExpRate * expCoupon;
    }

    void UpdateActualMesoRate()
    {
        ActualMesoRate = mesoRateByLevel * getChannelServer().WorldMesoRate * mesoCoupon;
    }

    void UpdateActualDropRate()
    {
        ActualDropRate = dropRateByLevel * getChannelServer().WorldDropRate * dropCoupon;
    }

    void UpdateActualBossDropRate()
    {
        ActualBossDropRate = getChannelServer().WorldBossDropRate;
    }

    void UpdateActualQuestExpRate()
    {
        ActualQuestExpRate = getChannelServer().WorldQuestRate;
    }

    void UpdateActualQuestMesoRate()
    {
        ActualQuestMesoRate = getChannelServer().WorldQuestRate;
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
            revertCouponsEffects();
            setCouponRates();
        }
        finally
        {
            cashInv.unlockInventory();
            chLock.ExitReadLock();
            Monitor.Exit(effLock);
        }
    }

    /// <summary>
    /// 移除倍率buff并重算
    /// </summary>
    private void revertCouponsEffects()
    {
        dispelBuffCoupons();

        this.expCoupon = 1;
        this.dropCoupon = 1;
        this.mesoCoupon = 1;
        UpdateActualExpRate();
        UpdateActualMesoRate();
        UpdateActualDropRate();
    }

    private List<int> activateCouponsEffects()
    {
        List<int> toCommitEffect = new();

        if (YamlConfig.config.server.USE_STACK_COUPON_RATES)
        {
            var stackExpCoupon = 0;
            var stackDropCoupon = 0;

            foreach (var coupon in activeCoupons)
            {
                int couponId = coupon.Key;
                int couponQty = coupon.Value.Count;

                toCommitEffect.Add(couponId);

                if (ItemConstants.isExpCoupon(couponId))
                {
                    stackExpCoupon += coupon.Value.Count * coupon.Value.Rate;
                }
                else
                {
                    stackDropCoupon += coupon.Value.Count * coupon.Value.Rate;
                }
            }
            expCoupon = stackExpCoupon == 0 ? 1 : stackExpCoupon;
            mesoCoupon = stackDropCoupon == 0 ? 1 : stackDropCoupon;
            dropCoupon = mesoCoupon;
        }
        else
        {
            int maxExpRate = 1, maxDropRate = 1, maxExpCouponId = -1, maxDropCouponId = -1;

            foreach (var coupon in activeCoupons)
            {
                int couponId = coupon.Key;

                if (ItemConstants.isExpCoupon(couponId))
                {
                    if (maxExpRate < coupon.Value.Rate)
                    {
                        maxExpCouponId = couponId;
                        maxExpRate = coupon.Value.Rate;
                    }
                }
                else
                {
                    if (maxDropRate < coupon.Value.Rate)
                    {
                        maxDropCouponId = couponId;
                        maxDropRate = coupon.Value.Rate;
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
        UpdateActualExpRate();
        UpdateActualMesoRate();
        UpdateActualDropRate();
        return toCommitEffect;
    }

    private void setActiveCoupons(ICollection<Item> cashItems)
    {
        activeCoupons.Clear();

        Dictionary<int, int> coupons = Client.CurrentServer.CouponRates;
        List<int> active = Client.CurrentServer.ActiveCoupons;

        foreach (Item it in cashItems)
        {
            if (ItemConstants.isRateCoupon(it.getItemId()) && active.Contains(it.getItemId()))
            {
                if (activeCoupons.TryGetValue(it.getItemId(), out var d))
                {
                    d.Count++;
                }
                else
                {
                    activeCoupons.AddOrUpdate(it.getItemId(), new CouponBuffEntry(1, coupons.GetValueOrDefault(it.getItemId())));
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
        var mse = ii.getItemEffect(couponid);
        mse?.applyTo(this);
    }

    /// <summary>
    /// 移除倍率道具buff
    /// </summary>
    private void dispelBuffCoupons()
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

    public int getRemainingSp()
    {
        return getRemainingSp(JobId); //default
    }

    public void updateRemainingSp(int remainingSp)
    {
        updateRemainingSp(remainingSp, GameConstants.getSkillBook(JobId));
    }


    public void message(string m)
    {
        dropMessage(5, m);
    }

    public void yellowMessage(string m)
    {
        sendPacket(PacketCreator.sendYellowTip(m));
    }


    private void playerDead()
    {
        if (this.MapModel.isCPQMap() && MapModel is ICPQMap cpqMap)
        {
            int losing = cpqMap.DeathCP;
            if (AvailableCP < losing)
            {
                losing = AvailableCP;
            }
            MapModel.broadcastMessage(PacketCreator.playerDiedMessage(getName(), losing, MCTeam!.TeamFlag));
            gainCP(-losing);
            return;
        }

        cancelAllBuffs(false);
        dispelDebuffs();
        lastDeathtime = getChannelServer().getCurrentTime();

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
            InventoryManipulator.removeById(Client, ItemConstants.getInventoryType(charmID[i]), charmID[i], 1, true, false);
            usedSafetyCharm = true;
        }
        else if (getJob() != Job.BEGINNER)
        { //Hmm...
            if (!FieldLimit.NO_EXP_DECREASE.check(MapModel.getFieldLimit()))
            {  // thanks Conrad for noticing missing FieldLimit check
                int XPdummy = ExpTable.getExpNeededForLevel(getLevel());

                if (MapModel.isTown())
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

        cancelEffectFromBuffStat(BuffStat.MORPH);

        cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);

        unsitChairInternal();
        sendPacket(PacketCreator.enableActions());
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

        UpdateStatsChunk(() =>
        {
            if (usedSafetyCharm)
            {
                // thanks kvmba for noticing safety charm not providing 30% HP/MP
                SetHP((int)Math.Ceiling(this.ActualMaxHP * 0.3));
                SetMP((int)Math.Ceiling(this.ActualMaxMP * 0.3));
            }
            else
            {
                SetHP(NumericConfig.MinHp);
            }
        });
        setStance(0);
    }



    private void RefreshByEquipChange()
    {
        if (equipchanged)
        {
            var equipmaxhp = 0;
            var equipmaxmp = 0;
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

            RefreshByEquip(equipmaxhp, equipmaxmp);
            equipchanged = false;
        }

        localdex += equipdex;
        localint_ += equipint_;
        localstr += equipstr;
        localluk += equipluk;
        localmagic += equipmagic;
        localwatk += equipwatk;
    }

    /// <summary>
    /// 重算所有人物属性
    /// </summary>
    private void reapplyLocalStats()
    {
        Monitor.Enter(effLock);
        chLock.EnterReadLock();
        statLock.EnterWriteLock();
        try
        {
            localdex = getDex();
            localint_ = getInt();
            localstr = getStr();
            localluk = getLuk();
            localmagic = localint_;
            localwatk = 0;
            localchairrate = -1;

            RefreshByEquipChange();
            RefreshByBuff();
            RecalculateMaxHP();
            RecalculateMaxMP();

            localmagic = Math.Min(localmagic, 2000);

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
            if (JobModel.isA(Job.BOWMAN))
            {
                Skill? expert = null;
                if (JobModel.isA(Job.MARKSMAN))
                {
                    expert = SkillFactory.getSkill(3220004);
                }
                else if (JobModel.isA(Job.BOWMASTER))
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

            if (JobModel.isA(Job.THIEF) || JobModel.isA(Job.BOWMAN) || JobModel.isA(Job.PIRATE) || JobModel.isA(Job.NIGHTWALKER1) || JobModel.isA(Job.WINDARCHER1))
            {
                var weapon_item = getInventory(InventoryType.EQUIPPED).getItem(EquipSlot.Weapon);
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
                            var item = inv.getItem(i);
                            if (item != null)
                            {
                                if ((claw && ItemConstants.isThrowingStar(item.getItemId()))
                                    || (gun && ItemConstants.isBullet(item.getItemId()))
                                    || (bow && ItemConstants.isArrowForBow(item.getItemId()))
                                    || (crossbow && ItemConstants.isArrowForCrossBow(item.getItemId())))
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

    public void UpdateLocalStats(bool isInitial = false)
    {
        Monitor.Enter(prtLock);
        Monitor.Enter(effLock);
        statLock.EnterWriteLock();
        try
        {
            int oldmaxhp = ActualMaxHP;
            int oldmaxmp = ActualMaxMP;
            reapplyLocalStats();

            //登录时不能发送 不然客户端会崩溃
            if (!isInitial)
                SendStats();

            if (oldmaxhp != ActualMaxHP)
            {
                // thanks Wh1SK3Y (Suwaidy) for pointing out a deadlock occuring related to party members HP
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



    public void removeVisibleMapObject(IMapObject mo)
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
                int tap = Ap + Str + Dex + Int + Luk, tsp = 1;
                int tstr = 4, tdex = 4, tint = 4, tluk = 4;

                switch (JobId)
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
                    updateStrDexIntLukSp(tstr, tdex, tint, tluk, tap, tsp, GameConstants.getSkillBook(JobId));
                }
                else
                {
                    Log.Warning("Chr {CharacterId} tried to have its stats reset without enough AP available", this.Id);
                }
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }
    }

    public void setBattleshipHp(int battleshipHp)
    {
        this.battleshipHp = battleshipHp;
    }

    public void resetBattleshipHp()
    {
        int bshipLevel = Math.Max(getLevel() - 120, 0);  // thanks alex12 for noticing battleship HP issues for low-level players
        this.battleshipHp = 400 * getSkillLevel(SkillFactory.GetSkillTrust(Corsair.BATTLE_SHIP)) + (bshipLevel * 200);
    }

    public void resetEnteredScript()
    {
        entered.Remove(MapModel.getId());
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


    public void saveGuildStatus()
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Characters.Where(x => x.Id == getId())
                .ExecuteUpdate(x => x.SetProperty(y => y.GuildId, GuildId).SetProperty(y => y.GuildRank, GuildRank).SetProperty(y => y.AllianceRank, AllianceRank));
        }
        catch (Exception se)
        {
            Log.Error(se.ToString());
        }
    }

    public void saveLocationOnWarp()
    {  // suggestion to remember the map before warp command thanks to Lei
        Portal? closest = MapModel.findClosestPortal(getPosition());
        int curMapid = getMapId();

        SavedLocations.FillData(new SavedLocation(curMapid, closest?.getId() ?? 0));
    }

    public void saveLocation(string type)
    {
        Portal? closest = MapModel.findClosestPortal(getPosition());
        SavedLocations.AddOrUpdate(type, new SavedLocation(getMapId(), closest?.getId() ?? 0));
    }

    public bool insertNewChar(CharacterFactoryRecipe recipe)
    {
        Str = recipe.getStr();
        Dex = recipe.getDex();
        Int = recipe.getInt();
        Luk = recipe.getLuk();
        SetHP(recipe.getMaxHp());
        SetMP(recipe.getMaxMp());
        SetMaxHP(recipe.getMaxHp());
        SetMaxMP(recipe.getMaxMp());
        Ap = recipe.getRemainingAp();
        RemainingSp[GameConstants.getSkillBook(JobId)] = recipe.getRemainingSp();
        MesoValue.set(recipe.getMeso());

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

        this.Events.AddOrUpdate("rescueGaga", new RescueGaga(0));


        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            var dbModel = GlobalTools.Mapper.Map<IPlayer, CharacterEntity>(this);
            dbContext.Characters.Add(dbModel);
            int updateRows = dbContext.SaveChanges();
            if (updateRows < 1)
            {
                Log.Error("Error trying to insert " + Name);
                return false;
            }
            this.Id = dbModel.Id;

            // Select a keybinding method
            KeyMap.SaveData(dbContext);

            // No quickslots, or no change.
            CharacterManager.SaveQuickSlotMapped(dbContext, this);

            itemsWithType = new();
            foreach (Inventory iv in Bag.GetValues())
            {
                foreach (Item item in iv.list())
                {
                    itemsWithType.Add(new(item, iv.getType()));
                }
            }

            ItemFactory.INVENTORY.saveItems(itemsWithType, Id, dbContext);

            Skills.SaveData(dbContext);

            dbTrans.Commit();
            return true;
        }
        catch (Exception t)
        {
            Log.Error(t, "Error creating chr {CharacterName}, level: {Level}, job: {JobId}", Name, Level, JobId);
            return false;
        }
    }


    public void sendPolice(int greason, string reason, int duration)
    {
        sendPacket(PacketCreator.sendPolice(string.Format("You have been blocked by the#b {0} Police for {1}.#k", "Cosmic", reason)));
        this.isbanned = true;
        TimerManager.getInstance().schedule(() =>
        {
            Client.Disconnect(false, false);
        }, duration);
    }

    public void sendPolice(string text)
    {
        string message = getName() + " received this - " + text;
        if (Server.getInstance().isGmOnline(this.getWorld()))
        {
            //Alert and log if a GM is online
            Client.CurrentServer.BroadcastWorldGMPacket(PacketCreator.sendYellowTip(message));
        }
        else
        {
            //Auto DC and log if no GM is online
            Client.Disconnect(false, false);
        }
        Log.Information(message);
        //NewServer.getInstance().broadcastGMMessage(0, PacketCreator.serverNotice(1, getName() + " received this - " + text));
        //sendPacket(PacketCreator.sendPolice(text));
        //this.isbanned = true;
        //TimerManager.getInstance().schedule(new Runnable() {
        //    public override    public void run() {
        //        Client.disconnect(false, false);
        //    }
        //}, 6000);
    }

    public void sendKeymap()
    {
        sendPacket(PacketCreator.getKeymap(KeyMap.GetDataSource()));
    }

    public void sendQuickmap()
    {
        // send quickslots to user
        var pQuickslotKeyMapped = this.QuickSlotKeyMapped ?? new QuickslotBinding(QuickslotBinding.DEFAULT_QUICKSLOTS);
        this.sendPacket(PacketCreator.QuickslotMappedInit(pQuickslotKeyMapped));
    }

    public void setBuddyCapacity(int capacity)
    {
        BuddyList.Capacity = capacity;
        sendPacket(PacketCreator.updateBuddyCapacity(capacity));
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
        this.DojoPoints = x;
    }

    public void setDojoStage(int x)
    {
        this.LastDojoStage = x;
    }

    public void setEnergyBar(int set)
    {
        energybar = set;
    }



    public void setExp(int amount)
    {
        this.ExpValue.set(amount);
    }



    public void setFace(int face)
    {
        this.Face = face;
    }

    public void setFame(int fame)
    {
        this.Fame = fame;
    }

    public void setFamilyId(int familyId)
    {
        this.FamilyId = familyId;
    }

    public void setFinishedDojoTutorial()
    {
        this.FinishedDojoTutorial = true;
    }

    public void setGender(int gender)
    {
        this.Gender = gender;
    }

    public void setGuildId(int _id)
    {
        GuildId = _id;
    }

    public void setGuildRank(int _rank)
    {
        GuildRank = _rank;
    }

    public void setAllianceRank(int _rank)
    {
        AllianceRank = _rank;
    }

    public void setHair(int hair)
    {
        this.Hair = hair;
    }

    /// <summary>
    /// 使用药品、技能带来的（血条蓝条）变化
    /// </summary>
    /// <param name="hpCon"></param>
    /// <param name="hpchange"></param>
    /// <param name="mpchange"></param>
    /// <returns>ture: 使用成功，false: 使用失败</returns>
    public bool applyHpMpChange(int hpCon, int hpchange, int mpchange)
    {
        bool zombify = hasDisease(Disease.ZOMBIFY);

        Monitor.Enter(effLock);
        statLock.EnterWriteLock();
        try
        {
            int nextHp = HP + hpchange, nextMp = MP + mpchange;
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

            UpdateStatsChunk(() =>
            {
                SetHP(nextHp);
                SetMP(nextMp);
            });
        }
        finally
        {
            statLock.ExitWriteLock();
            Monitor.Exit(effLock);
        }

        // autopot on HPMP deplete... thanks shavit for finding out D. Roar doesn't trigger autopot request
        if (hpchange < 0)
        {
            KeyBinding? autohpPot = this.KeyMap.GetData(KeyCode.VirtualAutoPotionHP);
            if (autohpPot != null)
            {
                int autohpItemid = autohpPot.getAction();
                float autohpAlert = this.getAutopotHpAlert();
                if (((float)this.HP / this.ActualMaxHP) <= autohpAlert)
                { // try within user settings... thanks Lame, Optimist, Stealth2800
                    var autohpItem = this.getInventory(InventoryType.USE).findById(autohpItemid);
                    if (autohpItem != null)
                    {
                        this.setAutopotHpAlert(0.9f * autohpAlert);
                        PetAutopotProcessor.runAutopotAction(Client, autohpItem.getPosition(), autohpItemid);
                    }
                }
            }
        }

        if (mpchange < 0)
        {
            KeyBinding? autompPot = this.KeyMap.GetData(KeyCode.VirtualAutoPotionMP);
            if (autompPot != null)
            {
                int autompItemid = autompPot.getAction();
                float autompAlert = this.getAutopotMpAlert();
                if (((float)this.MP) / this.ActualMaxMP <= autompAlert)
                {
                    var autompItem = this.getInventory(InventoryType.USE).findById(autompItemid);
                    if (autompItem != null)
                    {
                        this.setAutopotMpAlert(0.9f * autompAlert); // autoMP would stick to using pots at every depletion in some cases... thanks Rohenn
                        PetAutopotProcessor.runAutopotAction(Client, autompItem.getPosition(), autompItemid);
                    }
                }
            }
        }

        return true;
    }


    public void setJob(Job job)
    {
        this.JobModel = job;
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
        this.Level = level;

        if (YamlConfig.config.server.USE_ADD_RATES_BY_LEVEL && IsOnlined)
        {
            expRateByLevel = GameConstants.getPlayerBonusExpRate(this.Level / 20);
            mesoRateByLevel = GameConstants.getPlayerBonusMesoRate(this.Level / 20);
            dropRateByLevel = GameConstants.getPlayerBonusDropRate(this.Level / 20);
            UpdateActualExpRate();
            UpdateActualMesoRate();
            UpdateActualDropRate();
        }
    }

    public void setMessenger(Messenger? messenger)
    {
        this.Messenger = messenger;
        MessengerId = messenger?.getId() ?? 0;
    }

    public void setMessengerPosition(int position)
    {
        this.MessengerPosition = position;
    }

    public void setMonsterBookCover(int bookCover)
    {
        this.Monsterbookcover = bookCover;
    }

    public void setName(string name)
    {
        this.Name = name;
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
            doorSlot = TeamModel?.getPartyDoor(this.getId()) ?? 0;
            return doorSlot;
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

    public void setSearch(string? find)
    {
        search = find;
    }

    public void setSkinColor(SkinColor skinColor)
    {
        this.SkinColorModel = skinColor;
    }

    public int sellAllItemsFromName(sbyte invTypeId, string name)
    {
        //player decides from which inventory items should be sold.
        InventoryType type = InventoryTypeUtils.getByType(invTypeId);

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            var it = inv.findByName(name);
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
                mesoGain += standaloneSell(ii, type, i, inv.getItem(i)!.getQuantity());
            }
        }
        finally
        {
            inv.unlockInventory();
        }

        return (mesoGain);
    }

    private int standaloneSell(ItemInformationProvider ii, InventoryType type, short slot, short quantity)
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

            if (quantity <= iQuant && iQuant > 0)
            {
                InventoryManipulator.removeFromSlot(Client, type, (byte)slot, quantity, false);
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


    private List<Equip> getUpgradeableEquipped()
    {
        List<Equip> list = new();

        var ii = ItemInformationProvider.getInstance();
        return Bag[InventoryType.EQUIPPED].OfType<Equip>().Where(x => ii.isUpgradeable(x.getItemId())).ToList();
    }

    public bool mergeAllItemsFromName(string name)
    {
        InventoryType type = InventoryType.EQUIP;

        Inventory inv = getInventory(type);
        inv.lockInventory();
        try
        {
            var it = inv.findByName(name);
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
                List<Equip> statEquipped = ItemManager.GetEquipsWithStat(upgradeableEquipped, e.Key);
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
                    ItemManager.SetMergeFlag(eqp);

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
                standaloneMerge(statups, InventoryType.EQUIP, i, inv.getItem(i));
            }
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    private void standaloneMerge(Dictionary<StatUpgrade, float> statups, InventoryType type, short slot, Item? item)
    {
        short quantity;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (item == null || (quantity = item.getQuantity()) < 1 || ii.isCash(item.getItemId()) || !ii.isUpgradeable(item.getItemId()) || ItemManager.HasMergeFlag(item))
        {
            return;
        }

        Equip e = (Equip)item;
        foreach (var s in e.getStats())
        {
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

            if (statups.TryGetValue(s.Key, out var newVal))
            {
                newVal += incVal;
            }
            else
            {
                newVal = incVal;
            }

            statups.AddOrUpdate(s.Key, newVal);
        }

        InventoryManipulator.removeFromSlot(Client, type, (byte)slot, quantity, false);
    }

    public void setShop(Shop? shop)
    {
        this.shop = shop;
    }

    public void setTrade(Trade? trade)
    {
        this.trade = trade;
    }

    public void setVanquisherKills(int x)
    {
        this.VanquisherKills = x;
    }

    public void setVanquisherStage(int x)
    {
        this.VanquisherStage = x;
    }

    public void setWorld(int world)
    {
        this.World = world;
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
        return Client.CurrentServer.getDojoFinishTime(MapModel.getId()) - getChannelServer().getCurrentTime();
    }

    public void showDojoClock()
    {
        if (GameConstants.isDojoBossArea(MapModel.getId()))
        {
            sendPacket(PacketCreator.getClock((int)(getDojoTimeLeft() / 1000)));
        }
    }

    public void showUnderleveledInfo(Monster mob)
    {
        long curTime = getChannelServer().getCurrentTime();
        if (nextWarningTime < curTime)
        {
            nextWarningTime = (long)(curTime + TimeSpan.FromMinutes(1).TotalMilliseconds);   // show underlevel info again after 1 minute

            showHint("You have gained #rno experience#k from defeating #e#b" + mob.getName() + "#k#n (lv. #b" + mob.getLevel() + "#k)! Take note you must have around the same level as the mob to start earning EXP from it.");
        }
    }

    public void showHint(string msg, int length = 500)
    {
        Client.announceHint(msg, length);
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
        silentPartyUpdateInternal(TeamModel);
    }

    private void silentPartyUpdateInternal(ITeam? chrParty)
    {
        if (chrParty != null)
        {
            getWorldServer().updateParty(chrParty.getId(), PartyOperation.SILENT_UPDATE, this);
        }
    }

    public enum DelayedQuestUpdate
    {    // quest updates allow player actions during NPC talk...
        UPDATE, FORFEIT, COMPLETE, INFO
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
        Client.sendPacket(packet);
    }

    public override int getObjectId()
    {
        return getId();
    }

    public override MapObjectType getType()
    {
        return MapObjectType.PLAYER;
    }

    public override void sendDestroyData(IChannelClient Client)
    {
        Client.sendPacket(PacketCreator.removePlayerFromMap(this.getObjectId()));
    }

    public override void sendSpawnData(IChannelClient Client)
    {
        if (!this.isHidden() || Client.AccountEntity!.GMLevel > 1)
        {
            Client.sendPacket(PacketCreator.spawnPlayerMapObject(Client, this, false));

            if (buffEffects.ContainsKey(JobModel.getJobMapChair()))
            { // mustn't effLock, chrLock sendSpawnData
                Client.sendPacket(PacketCreator.giveForeignChairSkillEffect(Id));
            }
        }

        if (this.isHidden())
        {
            MapModel.broadcastGMMessage(this, PacketCreator.giveForeignBuff(getId(), new BuffStatValue(BuffStat.DARKSIGHT, 0)), false);
        }
    }

    public override void setObjectId(int Id) { }

    public override string ToString()
    {
        return Name;
    }

    public int getLinkedLevel()
    {
        return Link?.Level ?? 0;
    }

    public string? getLinkedName()
    {
        return Link?.Name;
    }

    public CashShop getCashShop()
    {
        return CashShopModel;
    }

    public void portalDelay(long delay)
    {
        this.portaldelay = DateTimeOffset.UtcNow.AddMilliseconds(delay).ToUnixTimeMilliseconds();
    }

    public long portalDelay()
    {
        return portaldelay;
    }

    public void blockPortal(string? scriptName)
    {
        if (scriptName != null && !blockedPortals.Contains(scriptName))
        {
            blockedPortals.Add(scriptName);
            sendPacket(PacketCreator.enableActions());
        }
    }

    public void unblockPortal(string? scriptName)
    {
        if (scriptName != null && blockedPortals.Contains(scriptName))
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
        if (AreaInfo.ContainsKey(area_))
        {
            return AreaInfo[area_].Contains(info);
        }
        return false;
    }

    public void updateAreaInfo(int area, string info)
    {
        AreaInfo.AddOrUpdate((short)area, info);
        sendPacket(PacketCreator.updateAreaInfo(area, info));
    }

    public string? getAreaInfo(int area)
    {
        return AreaInfo.GetValueOrDefault((short)area);
    }

    public Dictionary<short, string> getAreaInfos()
    {
        return AreaInfo;
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
            Client.Disconnect(false, false);

        }, 5000);

        Client.CurrentServer.BroadcastWorldGMPacket(PacketCreator.serverNotice(6, CharacterManager.makeMapleReadable(Name) + " was autobanned for " + reason));
    }

    public void block(int reason, int days, string desc)
    {
        try
        {
            var tempBan = DateTimeOffset.UtcNow.AddDays(days);
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == AccountId).ExecuteUpdate(x => x.SetProperty(y => y.Banreason, desc)
                .SetProperty(y => y.Tempban, tempBan)
                .SetProperty(y => y.Greason, reason));
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    public bool isBanned()
    {
        return isbanned;
    }

    public AutobanManager getAutobanManager()
    {
        return AutobanManager;
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

            }, TimeSpan.FromHours(1)); //1 hour
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
        var fullList = getInventory(InventoryType.EQUIPPED).list();
        if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_CASH)
        {
            return fullList;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        return fullList.Where(x => !ii.isCash(x.getItemId())).ToList();
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
                var itemName = ii.getName(nEquip.getItemId());
                if (itemName == null)
                {
                    continue;
                }

                nEquip.gainItemExp(Client, expGain);
            }
        }
    }

    public Dictionary<string, Events> getEvents()
    {
        return Events;
    }

    public PartyQuest? getPartyQuest()
    {
        return partyQuest;
    }

    public void setPartyQuest(PartyQuest? pq)
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

    public void Dispose()
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

        if (MountModel != null)
        {
            MountModel.empty();
            MountModel = null;
        }

        partyQuest = null;

        TeamModel = null;
        var familyEntry = getFamilyEntry();
        if (familyEntry != null)
        {
            familyEntry.setCharacter(null);
            setFamilyEntry(null);
        }
        // Bag.Dispose();
    }

    public void logOff()
    {
        RemoveWorldWatcher();
        Client.CurrentServer.Transport.SendAccountLogout(AccountId);
        setClient(new OfflineClient());
    }

    public void setLoginTime(DateTimeOffset time)
    {
        this.loginTime = time;
    }

    public DateTimeOffset getLoginTime()
    {
        return loginTime;
    }
    /// <summary>
    /// 获取登录时长
    /// </summary>
    /// <returns></returns>
    public TimeSpan getLoggedInTime()
    {
        return DateTimeOffset.UtcNow - loginTime;
    }

    public bool isLoggedin()
    {
        return IsOnlined;
    }


    public bool getWhiteChat()
    {
        return gmLevel() > 4 && whiteChat;
    }

    public void toggleWhiteChat()
    {
        whiteChat = !whiteChat;
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
    public bool registerNameChange(string newName)
    {
        try
        {
            using var dbContext = new DBContext();
            //check for pending name change
            var currentTimeMillis = DateTimeOffset.UtcNow;

            try
            {
                var dataList = dbContext.Namechanges.Where(x => x.Characterid == getId()).Select(x => x.CompletionTime).ToList();
                var nextChangeNameTime = currentTimeMillis.AddMilliseconds(-(double)YamlConfig.config.server.NAME_CHANGE_COOLDOWN);
                if (dataList.Any(x => x == null || x.Value > nextChangeNameTime))
                    return false;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to register name change for chr {CharacterName}", getName());
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
                Log.Error(e, "Failed to register name change for chr {CharacterName}", getName());
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get DB connection while registering name change");
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
            Log.Error(e, "Failed to cancel name change for chr {CharacterName}", getName());
            return false;
        }
    }
    public void doPendingNameChange()
    { //called on logout
        if (!pendingNameChange)
            return;
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
                Log.Error(e, "Failed to retrieve pending name changes for chr {CharacterName}", this.Name);
            }
            using var dbTrans = dbContext.Database.BeginTransaction();
            bool success = CharacterManager.doNameChange(dbContext, getId(), getName(), newName!, nameChangeId);
            if (!success)
                dbTrans.Rollback();
            else
                Log.Information("Name change applied: from {0} to {1}", this.Name, newName);
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get DB connection for pending chr name change");
        }
    }

    public string getLastCommandMessage()
    {
        return this.commandtext ?? string.Empty;
    }

    public void setLastCommandMessage(string text)
    {
        this.commandtext = text;
    }

    //EVENTS
    private sbyte team = 0;
    private Fitness? fitness;
    public Fitness? Fitness { get => fitness; set => fitness = value; }
    public Ola? Ola { get => ola; set => ola = value; }

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

    public long getLastSnowballAttack()
    {
        return snowballattack;
    }

    public void setLastSnowballAttack(long time)
    {
        this.snowballattack = time;
    }

    // MCPQ


    public bool isChallenged()
    {
        return challenged;
    }

    public void setChallenged(bool challenged)
    {
        this.challenged = challenged;
    }

    public int getLanguage()
    {
        return Client.AccountEntity!.Language;
    }

    public bool isChasing()
    {
        return chasing;
    }

    public void setChasing(bool chasing)
    {
        this.chasing = chasing;
    }

    public IWorldChannel getChannelServer()
    {
        return Client.CurrentServer;
    }

    public void LeaveGuild()
    {
        if (GuildModel == null)
            return;

        GuildModel.setOnline(Id, false, -1);
        if (GuildRank > 1)
        {
            GuildModel.leaveGuild(this);
        }
        else
        {
            GuildModel.disbandGuild();
        }
    }
}
