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

using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Players.Models;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Game.Trades;
using Application.Core.Gameplay;
using Application.Core.Managers;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Core.Server;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using client;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using client.keybind;
using net.server.guild;
using scripting;
using server;
using server.events;
using server.events.gm;
using server.maps;
using server.partyquest;
using server.quest;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using tools;
using static client.inventory.Equip;

namespace Application.Core.Game.Players;

public partial class Player
{
    public Storage Storage { get; set; } = null!;
    public RewardStorage GachaponStorage { get; set; } = null!;
    public AbstractStorage? CurrentStorage { get; set; }

    private ILogger? _log;
    public ILogger Log => _log ??= LogFactory.GetCharacterLog(AccountId, Id);

    public int InitialSpawnPoint { get; set; }
    private int currentPage, currentType = 0, currentTab = 1;

    private int energybar;

    private int ci = 0;

    // 替换Family，搁置
    // public ISchool? SchoolModel { get; set; }

    private int battleshipHp = 0;
    private int mesosTraded = 0;
    private int possibleReports = 10;
    private int dojoEnergy;
    private float expCoupon = 1, dropCoupon = 1;
    private long lastUsedCashItem, lastExpression = 0, lastHealed, lastDeathtime = -1;
    private int localstr, localdex, localluk, localint_, localmagic, localwatk;
    private int equipstr, equipdex, equipluk, equipint_, equipmagic, equipwatk, localchairhp, localchairmp;
    private int localchairrate;
    private bool hidden, equipchanged = true, hasSandboxItem = false, whiteChat = false;

    private bool usedSafetyCharm = false;
    public CharacterLink? Link { get; set; }

    private string? chalktext = null;
    private string? commandtext = null;
    private string? search = null;

    public AtomicInteger MesoValue { get; set; }

    private long totalExpGained = 0;

    private AbstractEventInstanceManager? eventInstance = null;


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

    public int ChatRoomId { get; set; }

    private Shop? shop = null;

    private Trade? trade = null;
    public MonsterBook Monsterbook { get; set; }
    public CashShop CashShopModel { get; set; }
    public AtomicInteger RewardNxCredit { get; set; } = new AtomicInteger();
    public PlayerSavedLocation SavedLocations { get; set; }

    private List<WeakReference<IMap>> lastVisitedMaps = new();
    private WeakReference<IMap?> ownedMap = new WeakReference<IMap?>(null);

    private ConcurrentDictionary<Monster, int> controlled = new();


    private Dictionary<int, CouponBuffEntry> activeCoupons = new();

    private Dictionary<int, Summon> summons = new();


    public byte[]? QuickSlotLoaded { get; set; }
    public QuickslotBinding? QuickSlotKeyMapped { get; set; }

    private ScheduledFuture? extraRecoveryTask = null;

    public long PendantOfSpiritEquippedTime { get; set; } = -1;
    public byte PendantExp { get; private set; } = 0;

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
    private byte lastmobcount = 0;

    public Dictionary<string, Events> Events { get; set; }

    private PartyQuest? partyQuest = null;

    private Dragon? dragon = null;


    private List<Ring> crushRings = new();
    private List<Ring> friendshipRings = new();
    private bool useCS;  //chaos scroll upon crafting item.
    private long npcCd;

    private sbyte extraHpRec = 0, extraMpRec = 0;
    private short extraRecInterval;
    private int targetHpBarHash = 0;
    private long targetHpBarTime = 0;
    private long nextWarningTime = 0;

    private bool pendingNameChange; //only used to change name on logout, not to be relied upon elsewhere
    private DateTimeOffset loginTime;
    private bool chasing = false;

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

    public bool CanTalkNpc()
    {
        return Client.CurrentServer.Node.getCurrentTime() - npcCd >= YamlConfig.config.server.BLOCK_NPC_RACE_CONDT;
    }

    public void addCrushRing(Ring r)
    {
        crushRings.Add(r);
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

    public async Task setCombo(short count)
    {
        if (count < combocounter)
        {
            await cancelEffectFromBuffStat(BuffStat.ARAN_COMBO);
        }
        combocounter = Math.Min((short)30000, count);
        if (count > 0)
        {
            await SendPacket(PacketCreator.showCombo(combocounter));
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

    public void RemoveWorldWatcher()
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

    public string getMedalText()
    {
        var medalItem = getInventory(InventoryType.EQUIPPED).getItem(EquipSlot.Medal);
        if (medalItem == null)
            return string.Empty;

        var medalItemName = Client.CurrentCulture.GetItemName(medalItem.getItemId());
        if (string.IsNullOrEmpty(medalItemName))
            return string.Empty;

        return "<" + medalItemName + "> ";
    }

    public async Task Hide(bool hide, bool fromLogin = false)
    {
        if (isGM() && hide != this.hidden)
        {
            if (!hide)
            {
                this.hidden = false;
                await SendPacket(PacketCreator.getGMEffect(0x10, 0));
                List<BuffStat> dsstat = Collections.singletonList(BuffStat.DARKSIGHT);
                foreach (var mapChr in MapModel.getAllPlayers())
                {
                    if (mapChr == this)
                    {
                        continue;
                    }

                    if (mapChr.isGM())
                    {
                        await mapChr.SendPacket(PacketCreator.cancelForeignBuff(Id, dsstat));
                    }
                    else
                    {
                        await MapModel.SetPlayerVisibleObject(mapChr, this);
                    }


                }

                await this.MapModel.ProcessMonster(async m =>
                {
                    await m.aggroUpdateController();
                });
            }
            else
            {
                this.hidden = true;

                if (!fromLogin)
                {
                    await SendPacket(PacketCreator.getGMEffect(0x10, 1));
                    foreach (var mapChr in MapModel.getAllPlayers())
                    {
                        if (mapChr == this)
                        {
                            continue;
                        }

                        if (mapChr.isGM())
                        {
                            await mapChr.SendPacket(PacketCreator.giveForeignBuff(Id, new BuffStatValue(BuffStat.DARKSIGHT, 0)));
                        }
                        else
                        {
                            await MapModel.SetPlayerInvisibleObject(mapChr, this);
                        }
                    }
                    await this.releaseControlledMonsters();
                }

            }
            await SendPacket(PacketCreator.enableActions());
        }
    }

    public async Task toggleHide(bool login)
    {
        await Hide(!hidden, login);
    }

    public async Task cancelMagicDoor()
    {
        List<BuffStatValueHolder> mbsvhList = getAllStatups();
        foreach (BuffStatValueHolder mbsvh in mbsvhList)
        {
            if (mbsvh.Effect.isMagicDoor())
            {
                await cancelEffect(mbsvh.Effect, false);
                break;
            }
        }
    }

    private async Task cancelPlayerBuffs(List<BuffStat> buffstats)
    {
        if (isLoggedinWorld())
        {
            await UpdateLocalStats();
            await SendPacket(PacketCreator.cancelBuff(buffstats));
            if (buffstats.Count > 0)
            {
                await BroadcastMap(PacketCreator.cancelForeignBuff(getId(), buffstats), Id);
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

    public async Task removeSandboxItems()
    {  // sandbox idea thanks to Morty
        if (!hasSandboxItem)
        {
            return;
        }

        foreach (InventoryType invType in Enum.GetValues<InventoryType>())
        {
            var inv = this.getInventory(invType);

            foreach (Item item in inv.list())
            {
                if (InventoryManipulator.isSandboxItem(item))
                {
                    await InventoryManipulator.removeFromSlot(Client, invType, item.getPosition(), item.getQuantity(), false);
                    await Pink("[" + Client.CurrentCulture.GetItemName(item.getItemId()) + "] has passed its trial conditions and will be removed from your inventory.");
                }
            }
        }

        hasSandboxItem = false;
    }

    public FameStatus canGiveFame(Player from)
    {
        if (this.isGM())
        {
            return FameStatus.OK;
        }

        if (FameLogs.Count == 0)
            return FameStatus.OK;

        if (TimeSpan.FromMilliseconds(FameLogs[FameLogs.Count - 1].Time - Client.CurrentServer.Node.getCurrentTime()) < TimeSpan.FromDays(1))
            return FameStatus.NOT_TODAY;

        if (FameLogs.Any(x => x.ToId == from.getId()))
            return FameStatus.NOT_THIS_MONTH;

        return FameStatus.OK;
    }



    public async Task setMasteries(int jobId)
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

                await changeSkillLevel(skill, 0, 10, -1);
            }
        }
    }

    private async Task broadcastChangeJob()
    {
        foreach (Player chr in MapModel.getAllPlayers())
        {
            var chrC = chr.Client;

            // 转职需要在地图上重新生成角色？ 没有单独的更新职业的数据包，部分职业转职时造型/特效发生变化
            if (chrC != null)
            {     // propagate new job 3rd-person effects (FJ, Aran 1st strike, etc)
                await this.sendDestroyData(chrC);
                await this.sendSpawnData(chrC);
            }
        }

        await Client.CurrentServer.TimerManager.schedule(() =>
        {
            MapModel.Send(m =>
            {
                return m.BroadcastAll(chr => chr.SendPacket(PacketCreator.showForeignEffect(Id, 8)), Id);
            });
        }, 777);
    }

    public async Task changeJob(Job? newJob)
    {
        if (newJob == null)
        {
            return;//the fuck you doing idiot!
        }

        this.JobModel = newJob;

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
            await gainSp(spGain, GameConstants.getSkillBook(newJob.getId()), true);
        }

        // thanks xinyifly for finding out missing AP awards (AP Reset can be used as a compass)
        if (newJob.getId() % 100 >= 1)
        {
            if (this.isCygnus())
            {
                await gainAp(7, true);
            }
            else
            {
                if (YamlConfig.config.server.USE_STARTING_AP_4 || newJob.getId() % 10 >= 1)
                {
                    await gainAp(5, true);
                }
            }
        }
        else
        {    // thanks Periwinks for noticing an AP shortage from lower levels
            if (YamlConfig.config.server.USE_STARTING_AP_4 && newJob.getId() % 1000 >= 1)
            {
                await gainAp(4, true);
            }
        }

        if (!isGM())
        {
            for (byte i = 1; i < 5; i++)
            {
                await gainSlots(i, 4, true);
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

        await ChangeMaxHP(addhp);
        ChangeMaxMP(addmp);
        await SetHP(ActualMaxHP);
        SetMP(ActualMaxMP);

        await UpdateLocalStats();

        List<KeyValuePair<Stat, int>> statup = new(7);
        statup.Add(new(Stat.HP, HP));
        statup.Add(new(Stat.MP, MP));
        statup.Add(new(Stat.MAXHP, MaxHP));
        statup.Add(new(Stat.MAXMP, MaxMP));
        statup.Add(new(Stat.AVAILABLEAP, Ap));
        statup.Add(new(Stat.AVAILABLESP, RemainingSp[GameConstants.getSkillBook(JobId)]));
        statup.Add(new(Stat.JOB, JobId));
        await SendPacket(PacketCreator.updatePlayerStats(statup, true, this));


        await SyncCharAsync(trigger: SyncCharacterTrigger.JobChanged);

        // setMPC(new PartyCharacter(this));

        if (dragon != null)
        {
            await BroadcastMap(PacketCreator.removeDragon(dragon.getObjectId()));
            dragon = null;
        }

        await setMasteries(this.JobId);

        await broadcastChangeJob();

        if (newJob.HasDragon())
        {
            if (getBuffedValue(BuffStat.MONSTER_RIDING) != null)
            {
                await cancelBuffStats(BuffStat.MONSTER_RIDING);
            }
            createDragon();

        }

    }

    async Task broadcastAcquaintances(Packet packet)
    {
        // guild已经有转职提示
        //var guild = getGuild();
        //if (guild != null)
        //{
        //    guild.broadcast(packet, Id);
        //}

        /*
        if(partnerid > 0) {
            partner.sendPacket(packet); not yet implemented
        }
        */
        await SendPacket(packet);
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

    public async Task broadcastStance(int newStance)
    {
        setStance(newStance);
        await broadcastStance();
    }

    public async Task broadcastStance()
    {
        await BroadcastMap(PacketCreator.MovePlayerIdle(Id, GetIdleMovementBytes()), Id);
    }

    private bool buffMapProtection()
    {
        int thisMapid = getMapId();
        int returnMapid = MapModel.getReturnMapId();


        foreach (var mbs in ActiveEffects)
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

        foreach (var lv in lastVisitedMaps)
        {
            if (lv.TryGetTarget(out var lvm))
                lastVisited.Add(lvm.getId());
        }

        return lastVisited;
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

    public async Task releaseControlledMonsters()
    {
        var controlledMonsters = new List<Monster>(controlled.Keys);
        controlled.Clear();

        foreach (Monster monster in controlledMonsters)
        {
            await monster.aggroRedirectController();
        }
    }

    public async Task<bool> applyConsumeOnPickup(Item item)
    {
        if (item.SourceTemplate is ConsumeItemTemplate template)
        {
            if (template.ConsumeOnPickup || template.ConsumeOnPickupEx)
            {
                var mse = ItemInformationProvider.getInstance().getItemEffect(item.getItemId())!;
                if (template.Party)
                {
                    List<Player> partyMembers = getPartyMembersOnSameMap();
                    foreach (Player mc in partyMembers)
                    {
                        if (mc.isAlive())
                        {
                            await mse.applyTo(mc);
                        }
                    }
                }
                else
                {
                    if (isAlive())
                        await mse.applyTo(this);
                }

                return true;
            }
        }
        return false;
    }
    Dictionary<int, PlayerPickupProcessor> _pickerProcessor = new();
    public async Task pickupItem(IMapObject? ob, int petIndex = -1)
    {
        // yes, one picks the IMapObject, not the MapItem
        if (ob == null)
        {
            // pet index refers to the one picking up the item
            return;
        }

        if (!_pickerProcessor.TryGetValue(petIndex, out var pickerProcessor))
        {
            pickerProcessor = new PlayerPickupProcessor(this, petIndex);
            _pickerProcessor[petIndex] = pickerProcessor;
        }

        if (ob is MapItem mapitem)
        {
            await pickerProcessor.Handle(mapitem);
        }
    }

    public bool isRidingBattleship()
    {
        return getBuffedValue(BuffStat.MONSTER_RIDING) == Corsair.BATTLE_SHIP;
    }

    public async Task announceBattleshipHp()
    {
        await SendPacket(PacketCreator.skillCooldown(Corsair.BATTLE_SHIP_HP, battleshipHp));
    }

    public async Task decreaseBattleshipHp(int decrease)
    {
        this.battleshipHp -= decrease;
        if (battleshipHp <= 0)
        {
            Skill battleship = SkillFactory.GetSkillTrust(Corsair.BATTLE_SHIP);
            int cooldown = battleship.getEffect(getSkillLevel(battleship)).getCooldown();

            await SendPacket(PacketCreator.skillCooldown(Corsair.BATTLE_SHIP, cooldown));
            addCooldown(Corsair.BATTLE_SHIP, Client.CurrentServer.Node.getCurrentTime(), cooldown * 1000);

            removeCooldown(Corsair.BATTLE_SHIP_HP);
            await cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);
        }
        else
        {
            await announceBattleshipHp();
            addCooldown(Corsair.BATTLE_SHIP_HP, 0, battleshipHp);
        }
    }

    public void decreaseReports()
    {
        this.possibleReports--;
    }

    private void stopExtraTask()
    {
        if (extraRecoveryTask != null)
        {
            extraRecoveryTask.cancel(false);
            extraRecoveryTask = null;
        }
    }

    private async Task startExtraTask(sbyte healHP, sbyte healMP, short healInterval)
    {
        await startExtraTaskInternal(healHP, healMP, healInterval);
    }

    private async Task startExtraTaskInternal(sbyte healHP, sbyte healMP, short healInterval)
    {
        extraRecInterval = healInterval;

        extraRecoveryTask = await Client.CurrentServer.TimerManager.register(() =>
        {
            MapModel.Send(async m =>
            {
                await ApplyExtralRecovery(healHP, healMP);
            });
        }, healInterval, healInterval);
    }

    public async Task ApplyExtralRecovery(sbyte healHP, sbyte healMP)
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
                await SendPacket(PacketCreator.showOwnRecovery(healHP));
                await BroadcastMap(PacketCreator.showRecovery(Id, healHP), Id);
            }
        }

        await UpdateStatsChunk(async () =>
        {
            await ChangeHP(healHP);
            ChangeMP(healMP);
        });

    }


    public async Task dispel()
    {
        if (!(YamlConfig.config.server.USE_UNDISPEL_HOLY_SHIELD && this.hasActiveBuff(Bishop.HOLY_SHIELD)))
        {
            List<BuffStatValueHolder> mbsvhList = getAllStatups();
            foreach (BuffStatValueHolder mbsvh in mbsvhList)
            {
                if (mbsvh.Effect.isSkill())
                {
                    if (mbsvh.Effect.getBuffSourceId() != Aran.COMBO_ABILITY)
                    {
                        // check discovered thanks to Croosade dev team
                        await cancelEffect(mbsvh.Effect, false);
                    }
                }
            }
        }
    }

    public async Task dispelSkill(int skillid)
    {
        List<BuffStatValueHolder> allBuffs = getAllStatups();
        foreach (BuffStatValueHolder mbsvh in allBuffs)
        {
            if (skillid == 0)
            {
                if (mbsvh.Effect.isSkill() && (mbsvh.Effect.getSourceId() % 10000000 == 1004 || dispelSkills(mbsvh.Effect.getSourceId())))
                {
                    await cancelEffect(mbsvh.Effect, false);
                }
            }
            else if (mbsvh.Effect.isSkill() && mbsvh.Effect.getSourceId() == skillid)
            {
                await cancelEffect(mbsvh.Effect, false);
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
    public async Task changeFaceExpression(int emote)
    {
        long timeNow = Client.CurrentServer.Node.getCurrentTime();
        // IClient allows changing every 2 seconds. Give it a little bit of overhead for packet delays.
        if (timeNow - lastExpression > 1500)
        {
            lastExpression = timeNow;
            await BroadcastMap(PacketCreator.facialExpression(this, emote), Id);
        }
    }

    public async Task doHurtHp()
    {
        if (!(this.getInventory(InventoryType.EQUIPPED).findById(MapModel.getHPDecProtect()) != null || buffMapProtection()))
        {
            await UpdateStatsChunk(async () =>
             {
                 await ChangeHP(-MapModel.getHPDec(), false);
             });
            await SendPacket(PacketCreator.onNotifyHPDecByField(MapModel.getHPDec()));
        }
    }


    public async Task dropMessage(int type, string message)
    {
        await SendPacket(PacketCommon.serverNotice(type, message));
    }

    public async Task Debug(int type, string message)
    {
        if (isGM())
        {
            await TypedMessage(type, message);
            Log.Debug(message);
        }
    }

    public async Task equipChanged()
    {
        equipchanged = true;
        await BroadcastMap(PacketCreator.updateCharLook(Client, this), Id);
        await UpdateLocalStats();
    }

    public enum FameStatus
    {

        OK, NOT_TODAY, NOT_THIS_MONTH
    }

    public async Task forceUpdateItem(Item item)
    {
        await SyncClientInventory([new InventoryAdd(item.getInventoryType(), item, item.getPosition())]);
    }

    public async Task SyncClientInventory(IInventoryOperationCommand? op, bool updateTick = true)
    {
        if (op == null)
            return;

        await SyncClientInventory([op], true);
    }

    public async Task SyncClientInventory(IEnumerable<IInventoryOperationCommand> ops, bool updateTick = true)
    {
        if (ops.Count() == 0)
            return;

        await SendPacket(PacketCreator.InventoryOperation(updateTick, ops));

        if (ops.Any(x => x.InventoryType == InventoryType.EQUIP && (x.CurrentPosition < 0 || (x is InventoryMove move && move.NewPosition < 0))))
        {
            await equipChanged();
        }
    }

    private KeyValuePair<int, int> applyFame(int delta)
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

    public async Task gainFame(int delta)
    {
        await gainFame(delta, null, 0);
    }

    public async Task<bool> gainFame(int delta, Player? fromPlayer, int mode)
    {
        KeyValuePair<int, int> fameRes = applyFame(delta);
        delta = fameRes.Value;
        if (delta != 0)
        {
            int thisFame = fameRes.Key;
            await updateSingleStat(Stat.FAME, thisFame);

            if (fromPlayer != null)
            {
                await fromPlayer.SendPacket(PacketCreator.giveFameResponse(mode, getName(), thisFame));
                await SendPacket(PacketCreator.receiveFame(mode, fromPlayer.getName()));
            }
            else
            {
                await SendPacket(PacketCreator.getShowFameGain(delta));
            }

            return true;
        }
        else
        {
            return false;
        }
    }



    public async Task genericGuildMessage(int code)
    {
        await this.SendPacket(GuildPackets.genericGuildMessage((byte)code));
    }

    public int getAccountID()
    {
        return AccountId;
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
    //    effLock.Enter();
    //    chLock.EnterReadLock();
    //    try
    //    {
    //        return effects.Select(x => new KeyValuePair<BuffStat, int>(x.Key, x.Value.value)).ToList();
    //    }
    //    finally
    //    {
    //        chLock.ExitReadLock();
    //        effLock.Exit();
    //    }
    //}



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
                if (!mbsvhi.Effect.isActive(this))
                {
                    continue;
                }

                if (mbsvhi.value > max.Key)
                {
                    max = new(mbsvhi.value, mbsvhi.Effect.getStatups().Count);
                    mbsvh = mbsvhi;
                }
                else if (mbsvhi.value == max.Key && mbsvhi.Effect.getStatups().Count > max.Value)
                {
                    max = new(mbsvhi.value, mbsvhi.Effect.getStatups().Count);
                    mbsvh = mbsvhi;
                }
            }
        }

        if (mbsvh != null)
        {
            ActiveEffects.AddOrUpdate(mbs, mbsvh);
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

    private long _lastNpcClick;
    public bool canClickNPC()
    {
        return _lastNpcClick + 500 < Client.CurrentServer.Node.getCurrentTime();
    }
    public void setClickedNPC()
    {
        _lastNpcClick = Client.CurrentServer.Node.getCurrentTime();
    }
    public void removeClickedNPC()
    {
        _lastNpcClick = 0;
    }

    private int _csAttempt;
    public bool attemptCsCoupon()
    {
        if (_csAttempt > 2)
        {
            resetCsCoupon();
            return false;
        }
        _csAttempt++;
        return true;
    }
    public void resetCsCoupon() => _csAttempt = 0;

    public Task enableCSActions() => Client.SendPacket(PacketCreator.enableCSUse(this));

    public Task closePlayerScriptInteractions() => Task.CompletedTask;

    AbstractPlayerInteraction? _pi;
    public AbstractPlayerInteraction getAbstractPlayerInteraction()
    {
        return _pi ??= new AbstractPlayerInteraction(Client);
    }

    public Task announceServerMessage()
    {
        return Client.SendPacket(PacketCreator.serverMessage(Client.CurrentServer.WorldServerMessage));
    }

    public async Task announceHint(string msg, int length, int height = 10)
    {
        await Client.SendPacket(PacketCreator.sendHint(msg, length, height));
        await Client.SendPacket(PacketCreator.enableActions());
    }

    public async Task announceBossHpBar(Monster mm, int mobHash, Packet packet)
    {
        long timeNow = Client.CurrentServer.Node.getCurrentTime();
        int targetHash = getTargetHpBarHash();

        if (mobHash != targetHash)
        {
            if (timeNow - getTargetHpBarTime() >= 5 * 1000)
            {
                if (!getChannelServer().ServerMessageManager.registerDisabledServerMessage(Id))
                {
                    await Client.SendPacket(PacketCreator.serverMessage(""));
                }
                await Client.SendPacket(packet);

                setTargetHpBarHash(mobHash);
                setTargetHpBarTime(timeNow);
            }
        }
        else
        {
            if (!getChannelServer().ServerMessageManager.registerDisabledServerMessage(Id))
            {
                await Client.SendPacket(PacketCreator.serverMessage(""));
            }
            await Client.SendPacket(packet);

            setTargetHpBarTime(timeNow);
        }
    }

    public AbstractEventManager? getEventManager(string @event)
    {
        return Client.CurrentServer.getEventSM().getEventManager(@event);
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


    public Door? getPlayerDoor()
    {
        return Client.CurrentServer.PlayerDoors.GetValueOrDefault(Id);
    }

    public void applyPartyDoor(Door door)
    {
        Client.CurrentServer.PlayerDoors[Id] = door;
        silentPartyUpdate();
    }


    public int getEnergyBar()
    {
        return energybar;
    }

    public void setEventInstance(AbstractEventInstanceManager? eventInstance)
    {
        this.eventInstance = eventInstance;
    }
    public AbstractEventInstanceManager? getEventInstance()
    {
        return eventInstance;
    }



    public void resetExcluded(long petId)
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

    public void addExcluded(long petId, int x)
    {
        excluded.GetValueOrDefault(petId)?.Add(x);
    }

    public async Task commitExcludedItems()
    {
        var petExcluded = this.getExcluded();

        excludedItems.Clear();

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
                await SendPacket(PacketCreator.loadExceptionList(this.getId(), pe.Key, petIndex, new(exclItems)));

                foreach (int itemid in exclItems)
                {
                    excludedItems.Add(itemid);
                }
            }
        }
    }

    public async Task exportExcludedItems(IChannelClient c)
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
                await c.SendPacket(PacketCreator.loadExceptionList(this.getId(), pe.Key, petIndex, new(exclItems)));
            }
        }
    }

    public Dictionary<long, HashSet<int>> getExcluded()
    {
        return excluded.ToDictionary();
    }

    public HashSet<int> getExcludedItems()
    {
        return excludedItems.ToHashSet();
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
        return dropCoupon;
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

    public int getFace()
    {
        return Face;
    }

    public int getFame()
    {
        return Fame;
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

        // 其他获取Fh都是取id，为什么这里取y1
        return MapModel.Footholds.FindBelowFoothold(pos)?.getY1() ?? 0;
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
        setTargetHpBarTime(Client.CurrentServer.Node.getCurrentTime());
    }

    public async Task resetPlayerAggro()
    {
        if (getChannelServer().ServerMessageManager.unregisterDisabledServerMessage(Id))
        {
            await announceServerMessage();
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


    public void closePartySearchInteractions()
    {

    }

    public async Task closePlayerInteractions()
    {
        closeNpcShop();
        await closeTrade();
        await closeMiniGame(true);
        await closeRPS();

        await LeaveVisitingShop();

        await closePlayerScriptInteractions();
        await resetPlayerAggro();
    }

    public void closeNpcShop()
    {
        setShop(null);
    }

    public async Task closeTrade(TradeResult reseaon = TradeResult.PARTNER_CANCEL)
    {
        var localTrade = getTrade();
        if (localTrade != null)
        {
            await localTrade.CancelTrade(TradeResult.PARTNER_CANCEL);
        }
    }

    public int getPossibleReports()
    {
        return possibleReports;
    }

    public bool needQuestItem(int questid, int itemid)
    {
        if (questid <= 0)
        {
            //For non quest items :3
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
        BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(effect);
        return mbsvh?.Effect;
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



    public int gmLevel()
    {
        return Client.AccountEntity!.GMLevel;
    }

    public async Task handleEnergyChargeGain()
    {
        // to get here energychargelevel has to be > 0
        Skill energycharge = isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.ENERGY_CHARGE) : SkillFactory.GetSkillTrust(Marauder.ENERGY_CHARGE);
        StatEffect ceffect;
        ceffect = energycharge.getEffect(getSkillLevel(energycharge));

        if (energybar < 10000)
        {
            energybar += 102;
            if (energybar > 10000)
            {
                energybar = 10000;
            }
            var stat = new BuffStatValue(BuffStat.ENERGY_CHARGE, energybar);
            setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
            await SendPacket(PacketCreator.giveBuff(energybar, 0, stat));
            await SendPacket(PacketCreator.showOwnBuffEffect(energycharge.getId(), 2));
            await BroadcastMap(PacketCreator.showBuffEffect(Id, energycharge.getId(), 2), Id);
            await BroadcastMap(PacketCreator.giveForeignPirateBuff(Id, energycharge.getId(),
                    ceffect.getDuration(), stat), Id);
        }
        if (energybar >= 10000 && energybar < 11000)
        {
            energybar = 15000;
            Player chr = this;
            await Client.CurrentServer.NodeService.TimerManager.schedule(() =>
            {
                MapModel.Send(async m =>
                {
                    await ApplyEnergeCharge();
                });
            }, ceffect.getDuration());
        }
    }

    public async Task ApplyEnergeCharge()
    {
        energybar = 0;
        var stat = new BuffStatValue(BuffStat.ENERGY_CHARGE, energybar);
        setBuffedValue(BuffStat.ENERGY_CHARGE, energybar);
        await SendPacket(PacketCreator.giveBuff(energybar, 0, stat));
        await MapModel.BroadcastAll(chr => chr.SendPacket(PacketCreator.cancelForeignFirstDebuff(Id, ((long)1) << 50)), Id);
    }

    public async Task handleOrbconsume()
    {
        int skillid = isCygnus() ? DawnWarrior.COMBO : Crusader.COMBO;
        var combo = SkillFactory.GetSkillTrust(skillid);
        var stat = new BuffStatValue(BuffStat.COMBO, 1);
        setBuffedValue(BuffStat.COMBO, 1);
        await SendPacket(PacketCreator.giveBuff(
            skillid,
            combo.getEffect(getSkillLevel(combo)).getDuration() + (int)((getBuffedStarttime(BuffStat.COMBO) ?? 0) - Client.CurrentServer.Node.getCurrentTime()),
            stat));
        await BroadcastMap(PacketCreator.giveForeignBuff(getId(), stat), Id);
    }


    public void hasGivenFame(Player to)
    {
        FameLogs.Add(new Core.Models.FameLogObject(to.Id, Client.CurrentServer.Node.getCurrentTime()));
    }


    public bool isBuffFrom(BuffStat stat, Skill skill)
    {
        BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(stat);
        if (mbsvh == null)
        {
            return false;
        }
        return mbsvh.Effect.isSkill() && mbsvh.Effect.getSourceId() == skill.getId();
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



    public bool isGuildLeader()
    {
        // true on guild master or jr. master
        return GuildId > 0 && GuildRank < 3;
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

    private async Task levelUpGainSp()
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
            await gainSp(spGain, GameConstants.getSkillBook(JobId), true);
        }
    }

    public async Task levelUp(bool takeexp)
    {

        Skill? improvingMaxHP = null;
        Skill? improvingMaxMP = null;
        int improvingMaxHPLevel = 0;
        int improvingMaxMPLevel = 0;

        bool isBeginner = isBeginnerJob();
        if (YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP && isBeginner && Level < 11)
        {
            await gainAp(5, true);

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

            await assignStrDexIntLuk(str, dex, 0, 0);
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

            await gainAp(remainingAp, true);
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

        await ChangeMaxHP(addhp);
        ChangeMaxMP(addmp);
        await SetHP(ActualMaxHP);
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

            setLevel(maxClassLevel);
        }

        await levelUpGainSp();


        await UpdateLocalStats();

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

        await SendPacket(PacketCreator.updatePlayerStats(statup, true, this));


        await SyncCharAsync(trigger: SyncCharacterTrigger.LevelChanged);

        await BroadcastMap(PacketCreator.showForeignEffect(getId(), 0), Id);
        // setMPC(new PartyCharacter(this));

        if (Level % 20 == 0)
        {
            if (YamlConfig.config.server.USE_ADD_SLOTS_BY_LEVEL == true)
            {
                if (!isGM())
                {
                    for (byte i = 1; i < 5; i++)
                    {
                        await gainSlots(i, 4, true);
                    }

                    await Yellow("You reached level " + Level + ". Congratulations! As a token of your success, your inventory has been expanded a little bit.");
                }
            }
            if (YamlConfig.config.server.USE_ADD_RATES_BY_LEVEL == true)
            {
                await Yellow("You managed to get level " + Level + "! Getting experience and items seems a little easier now, huh?");
            }
        }

        if (YamlConfig.config.server.USE_PERFECT_PITCH && Level >= 30)
        {
            //milestones?
            await GainItem(ItemId.PERFECT_PITCH, 1);
        }
        else if (Level == 10)
        {
            if (Party > 0)
            {
                await Client.CurrentServer.NodeService.TeamManager.LeaveStarterParty(this);
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
        ActualMesoRate = mesoRateByLevel * getChannelServer().WorldMesoRate * dropCoupon;
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

    public List<int> getActiveCoupons()
    {
        var now = Client.CurrentServer.Node.getCurrentTime();
        var nowDt = DateTimeOffset.FromUnixTimeMilliseconds(now).ToLocalTime().DateTime;
        return GetInventory(InventoryType.CASH).ListExsitedEnumerable().Where(item =>
                        item.SourceTemplate is CouponItemTemplate couponItemTemplate
                        && (item.getExpiration() == -1 ? couponItemTemplate.TimeRangeF.Any(x => x.Contains(nowDt)) : item.getExpiration() > now))
                    .Select(x => x.getItemId()).ToList();
    }

    public void addPlayerRing(Ring? ring)
    {
        if (ring == null)
            return;

        int ringItemId = ring.getItemId();
        if (ItemId.isWeddingRing(ringItemId))
        {
            this.addMarriageRing(ring);
        }
        else if (ringItemId > 1112012)
        {
            this.addFriendshipRing(ring);
        }
        else
        {
            this.addCrushRing(ring);
        }
    }

    public void AddPlayerRing(RingSourceModel? ringSource)
    {
        if (ringSource == null)
            return;

        var ring = GetRingFromTotal(ringSource);
        addPlayerRing(ring);
    }

    public int getRemainingSp()
    {
        return getRemainingSp(JobId); //default
    }

    public async Task updateRemainingSp(int remainingSp)
    {
        await updateRemainingSp(remainingSp, GameConstants.getSkillBook(JobId));
    }

    public string GetMessageByKey(string key, params string[] paramsValue)
    {
        return Client.CurrentCulture.GetMessageByKey(key, paramsValue);
    }

    public async Task MessageI18N(string key, params string[] paramsValue)
    {
        var message = GetMessageByKey(key, paramsValue);
        if (!string.IsNullOrEmpty(message))
        {
            await this.Pink(message);
        }
    }

    public async Task respawn(int returnMap)
    {
        if (returnMap == MapId.NONE)
        {
            returnMap = MapModel.Id;
        }


        await cancelAllBuffs(false);  // thanks Oblivium91 for finding out players still could revive in area and take damage before returning to town

        await UpdateStatsChunk(async () =>
        {
            if (usedSafetyCharm)
            {
                // thanks kvmba for noticing safety charm not providing 30% HP/MP
                await SetHP((int)Math.Ceiling(this.ActualMaxHP * 0.3));
                SetMP((int)Math.Ceiling(this.ActualMaxMP * 0.3));
            }
            else
            {
                await SetHP(NumericConfig.MinHp);
            }
            await changeMap(returnMap);
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
    private async Task reapplyLocalStats()
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
        await RecalculateMaxHP();
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
                    var inv = getInventory(InventoryType.USE);
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

    public async Task UpdateLocalStats(bool isInitial = false)
    {

        int oldmaxhp = ActualMaxHP;
        int oldmaxmp = ActualMaxMP;
        await reapplyLocalStats();

        //登录时不能发送 不然客户端会崩溃
        if (!isInitial)
            await SendStats();

        if (oldmaxhp != ActualMaxHP)
        {
            // thanks Wh1SK3Y (Suwaidy) for pointing out a deadlock occuring related to party members HP
            await updatePartyMemberHP();
        }
    }





    public async Task resetStats()
    {
        if (!YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP)
        {
            return;
        }


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
            await updateStrDexIntLukSp(tstr, tdex, tint, tluk, tap, tsp, GameConstants.getSkillBook(JobId));
        }
        else
        {
            Log.Warning("Chr {CharacterId} tried to have its stats reset without enough AP available", this.Id);
        }
    }

    public void resetBattleshipHp()
    {
        // a1: skillid, a2: skillLevel, a3: level
        //  int __cdecl sub_7665F1(int a1, int a2, int a3)
        //  {
        //    if ( a1 == 5221006 )
        //      return 200 * (a3 + 2 * a2 - 120);
        //    else
        //      return -1;
        //  }
        int bshipLevel = Math.Max(getLevel() - 120, 0);  // thanks alex12 for noticing battleship HP issues for low-level players
        this.battleshipHp = 400 * getSkillLevel(SkillFactory.GetSkillTrust(Corsair.BATTLE_SHIP)) + (bshipLevel * 200);
    }

    public void clearSavedLocation(SavedLocationType type)
    {
        SavedLocations.AddOrUpdate(type, null);
    }

    int peekSavedLocation(string type)
    {
        var sl = SavedLocations.GetData(type);
        if (sl == null)
        {
            return -1;
        }
        return sl.getMapId();
    }

    public int PeekSavedLocation(SavedLocationType type)
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
    public int GetSavedLocation(SavedLocationType type)
    {
        int m = PeekSavedLocation(type);
        clearSavedLocation(type);

        return m;
    }

    public async Task<bool> TryWarpBackSavedLocation(SavedLocationType type)
    {
        var sl = SavedLocations.GetData(type);
        if (sl != null)
        {
            await changeMap(sl.getMapId(), sl.getPortal());
            clearSavedLocation(type);
            return true;
        }
        return false;
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

    public void SaveLocation(SavedLocationType type)
    {
        Portal? closest = MapModel.findClosestPortal(getPosition());
        SavedLocations.AddOrUpdate(type, new SavedLocation(getMapId(), closest?.getId() ?? 0));
    }

    public async Task sendKeymap()
    {
        await SendPacket(PacketCreator.getKeymap(KeyMap.GetDataSource()));
    }

    public async Task sendQuickmap()
    {
        // send quickslots to user
        var pQuickslotKeyMapped = this.QuickSlotKeyMapped ?? new QuickslotBinding(QuickslotBinding.DEFAULT_QUICKSLOTS);
        await this.SendPacket(PacketCreator.QuickslotMappedInit(pQuickslotKeyMapped));
    }

    public async Task setBuddyCapacity(int capacity)
    {
        BuddyList.Capacity = capacity;
        await SendPacket(PacketCreator.updateBuddyCapacity(capacity));
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
    public async Task<bool> applyHpMpChange(int hpCon, int hpchange, int mpchange)
    {
        bool zombify = hasDisease(Disease.ZOMBIFY);


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

        await UpdateStatsChunk(async () =>
        {
            await SetHP(nextHp);
            SetMP(nextMp);
        });

        //// autopot on HPMP deplete... thanks shavit for finding out D. Roar doesn't trigger autopot request
        if (hpchange < 0)
        {
            await SendPacket(PacketCreator.onNotifyHPDecByField(-hpchange));
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

    public void setMonsterBookCover(int bookCover)
    {
        this.Monsterbookcover = bookCover;
    }

    public void setName(string name)
    {
        this.Name = name;
    }

    public void setSearch(string? find)
    {
        search = find;
    }

    public void setSkinColor(SkinColor skinColor)
    {
        this.SkinColorModel = skinColor;
    }

    public async Task<int> sellAllItemsFromName(sbyte invTypeId, string name)
    {
        //player decides from which inventory items should be sold.
        InventoryType type = InventoryTypeUtils.getByType(invTypeId);

        var inv = getInventory(type);
        var it = inv.findByName(name);
        if (it == null)
        {
            return (-1);
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        return (await sellAllItemsFromPosition(ii, type, it.getPosition()));
    }

    public async Task<int> sellAllItemsFromPosition(ItemInformationProvider ii, InventoryType type, short pos)
    {
        int mesoGain = 0;

        var inv = getInventory(type);

        for (short i = pos; i <= inv.getSlotLimit(); i++)
        {
            if (inv.getItem(i) == null)
            {
                continue;
            }
            mesoGain += await standaloneSell(ii, type, i, inv.getItem(i)!.getQuantity());
        }

        return (mesoGain);
    }

    private async Task<int> standaloneSell(ItemInformationProvider ii, InventoryType type, short slot, short quantity)
    {
        // quantity == 0xFFFF || 这里quantity永远小于0xFFFF，有什么意义？
        if (quantity == 0)
        {
            quantity = 1;
        }

        var inv = getInventory(type);
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
            await InventoryManipulator.removeFromSlot(Client, type, (byte)slot, quantity, false);
            int recvMesos = ii.getPrice(itemid, quantity);
            if (recvMesos > 0)
            {
                await GainMeso(recvMesos);
                return (recvMesos);
            }
        }

        return (0);
    }


    private List<Equip> getUpgradeableEquipped()
    {
        List<Equip> list = new();

        var ii = ItemInformationProvider.getInstance();
        return Bag[InventoryType.EQUIPPED].OfType<Equip>().Where(x => x.SourceTemplate.IsUpgradeable()).ToList();
    }

    public async Task<bool> mergeAllItemsFromName(string name)
    {
        InventoryType type = InventoryType.EQUIP;

        var inv = getInventory(type);
        var it = inv.findByName(name);
        if (it == null)
        {
            return false;
        }

        Dictionary<StatUpgrade, float> statups = new();
        await mergeAllItemsFromPosition(statups, it.getPosition());

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

        await LightBlue("EQUIPMENT MERGE operation results:");
        foreach (var eqpUpg in equipUpgrades)
        {
            List<KeyValuePair<StatUpgrade, int>> eqpStatups = eqpUpg.Value;
            if (eqpStatups.Count > 0)
            {
                Equip eqp = eqpUpg.Key;
                ItemManager.SetMergeFlag(eqp);

                string showStr = " '" + Client.CurrentCulture.GetItemName(eqp.getItemId()) + "': ";
                string upgdStr = eqp.gainStats(eqpStatups).Key;

                await this.forceUpdateItem(eqp);

                showStr += upgdStr;
                await LightBlue(showStr);
            }
        }

        return true;
    }

    public async Task mergeAllItemsFromPosition(Dictionary<StatUpgrade, float> statups, short pos)
    {
        var inv = getInventory(InventoryType.EQUIP);
        for (short i = pos; i <= inv.getSlotLimit(); i++)
        {
            await standaloneMerge(statups, InventoryType.EQUIP, i, inv.getItem(i) as Equip);
        }
    }

    private async Task standaloneMerge(Dictionary<StatUpgrade, float> statups, InventoryType type, short slot, Equip? e)
    {
        short quantity;
        if (e == null || (quantity = e.getQuantity()) < 1 || e.SourceTemplate.Cash || !e.SourceTemplate.IsUpgradeable() || ItemManager.HasMergeFlag(e))
        {
            return;
        }

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

        await InventoryManipulator.removeFromSlot(Client, type, (byte)slot, quantity, false);
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


    private long getDojoTimeLeft()
    {
        return Client.CurrentServer.getDojoFinishTime(MapModel.getId()) - Client.CurrentServer.Node.getCurrentTime();
    }

    public async Task showDojoClock()
    {
        if (GameConstants.isDojoBossArea(MapModel.getId()))
        {
            await SendPacket(PacketCreator.getClock((int)(getDojoTimeLeft() / 1000)));
        }
    }

    public async Task showUnderleveledInfo(Monster mob)
    {
        long curTime = Client.CurrentServer.Node.getCurrentTime();
        if (nextWarningTime < curTime)
        {
            nextWarningTime = (long)(curTime + TimeSpan.FromMinutes(1).TotalMilliseconds);   // show underlevel info again after 1 minute

            await showHint("You have gained #rno experience#k from defeating #e#b" + mob.getName() + "#k#n (lv. #b" + mob.getLevel() + "#k)! Take note you must have around the same level as the mob to start earning EXP from it.");
        }
    }

    public async Task showHint(string msg, int length = 500)
    {
        await announceHint(msg, length);
    }


    public void silentPartyUpdate()
    {
        silentPartyUpdateInternal(Party);
    }

    private void silentPartyUpdateInternal(int partyId)
    {
        if (partyId > 0)
        {
            Client.CurrentServer.NodeService.TeamManager.ChannelNotify(this);
            // _ = Client.CurrentServerContainer.TeamManager.UpdateTeam(partyId, PartyOperation.SILENT_UPDATE, this, this.Id);
        }
    }

    public enum DelayedQuestUpdate
    {    // quest updates allow player actions during NPC talk...
        UPDATE, FORFEIT, COMPLETE, INFO
    }


    public async Task updateSingleStat(Stat stat, int newval)
    {
        await updateSingleStat(stat, newval, false);
    }

    private async Task updateSingleStat(Stat stat, int newval, bool itemReaction)
    {
        await SendPacket(PacketCreator.updatePlayerStats(Collections.singletonList(new KeyValuePair<Stat, int>(stat, newval)), itemReaction, this));
    }


    public Task SendPacket(Packet packet) => Client.SendPacket(packet);

    public override int getObjectId()
    {
        return getId();
    }

    public override MapObjectType getType()
    {
        return MapObjectType.PLAYER;
    }

    public override async Task sendDestroyData(IChannelClient mapChrClient)
    {
        await mapChrClient.SendPacket(PacketCreator.removePlayerFromMap(this.getObjectId()));
    }

    public override async Task sendSpawnData(IChannelClient mapChrClient)
    {
        if (!this.isHidden() || mapChrClient.AccountEntity!.IsGmAccount())
        {
            await mapChrClient.SendPacket(PacketCreator.spawnPlayerMapObject(mapChrClient, this, false));

            if (buffEffects.ContainsKey(JobModel.getJobMapChair()))
            { // mustn't effLock, chrLock sendSpawnData
                await mapChrClient.SendPacket(PacketCreator.giveForeignChairSkillEffect(Id));
            }

            foreach (Summon ms in this.getSummonsValues())
            {
                await mapChrClient.SendPacket(PacketCreator.spawnSummon(ms, false));
            }
        }

        if (this.isHidden() && mapChrClient.OnlinedCharacter.isGM())
        {
            await mapChrClient.SendPacket(PacketCreator.giveForeignBuff(getId(), new BuffStatValue(BuffStat.DARKSIGHT, 0)));
        }
    }

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
        this.portaldelay = Client.CurrentServer.Node.getCurrentTime();
    }

    public long portalDelay()
    {
        return portaldelay;
    }

    public async Task blockPortal(string? scriptName)
    {
        if (scriptName != null && !blockedPortals.Contains(scriptName))
        {
            blockedPortals.Add(scriptName);
            await SendPacket(PacketCreator.enableActions());
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

    public async Task updateAreaInfo(int area, string info)
    {
        AreaInfo.AddOrUpdate((short)area, info);
        await SendPacket(PacketCreator.updateAreaInfo(area, info));
    }

    public string? getAreaInfo(int area)
    {
        return AreaInfo.GetValueOrDefault((short)area);
    }

    public Dictionary<short, string> getAreaInfos()
    {
        return AreaInfo;
    }

    public async Task autoban(string reason)
    {
        if (this.isGM() || this.isBanned())
        {  // thanks RedHat for noticing GM's being able to get banned
            return;
        }

        await Client.CurrentServer.NodeService.AdminService.AutoBan(this, (int)BanReason.HACK, reason, -1);
    }


    public bool isBanned()
    {
        return isbanned;
    }

    public AutobanManager getAutobanManager()
    {
        return AutobanManager;
    }


    public bool isEquippedMesoMagnet(int petIndex)
    {
        if (petIndex < 0 || petIndex > 2)
            return false;
        return GetEquipped().HasEquipped(EquipSlot.PetEquipSlots[petIndex].MesoMagnet);
    }
    public bool isEquippedItemPouch(int petIndex)
    {
        if (petIndex < 0 || petIndex > 2)
            return false;
        return GetEquipped().HasEquipped(EquipSlot.PetEquipSlots[petIndex].ItemPouch);
    }
    public bool isEquippedPetItemIgnore(int petIndex)
    {
        if (petIndex < 0 || petIndex > 2)
            return false;
        return GetEquipped().HasEquipped(EquipSlot.PetEquipSlots[petIndex].ItemIgnore);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="now"></param>
    public async Task CalculateSpiritPendant(long now, bool active)
    {
        if (!active)
        {
            PendantOfSpiritEquippedTime = -1;
            PendantExp = 0;
        }
        else
        {
            if (PendantOfSpiritEquippedTime <= 0 || PendantOfSpiritEquippedTime > now)
                PendantOfSpiritEquippedTime = now;

            if (PendantOfSpiritEquippedTime > 0)
            {
                var hasEquippedLength = TimeSpan.FromMilliseconds(now - PendantOfSpiritEquippedTime);
                var bonusExp = (byte)Math.Min(hasEquippedLength.Hours + 1, 3); // 10% ~ 30%
                if (PendantExp != bonusExp)
                {
                    PendantExp = bonusExp;
                    await SendPacket(PacketCreator.BonusExpRateChanged(-EquipSlot.Pendant, hasEquippedLength.Hours, PendantExp * 10));
                }
            }
        }
    }

    public void CalculateCoupon(long now)
    {
        var nowDt = DateTimeOffset.FromUnixTimeMilliseconds(now).ToLocalTime().DateTime;
        var allActiveCoupons = GetInventory(InventoryType.CASH).ListExsitedEnumerable().Where(item =>
                item.SourceTemplate is CouponItemTemplate couponItemTemplate
                && couponItemTemplate.TimeRangeF.Any(x => x.Contains(nowDt)))
            .Select(x => (x.SourceTemplate as CouponItemTemplate)!).ToList();

        if (allActiveCoupons.Count > 0)
        {
            expCoupon = Math.Max(allActiveCoupons.Where(x => x.IsExp).Max(x => x.Rate), 1);
            dropCoupon = Math.Max(allActiveCoupons.Where(x => x.IsDrop).Max(x => x.Rate), 1);

            // YamlConfig.config.server.USE_STACK_COUPON_RATES
            // TODO: 叠加逻辑

            UpdateActualExpRate();
            UpdateActualMesoRate();
            UpdateActualDropRate();
        }
    }

    private ICollection<Item> getUpgradeableEquipList()
    {
        var fullList = getInventory(InventoryType.EQUIPPED).list();
        if (YamlConfig.config.server.USE_EQUIPMNT_LVLUP_CASH)
        {
            return fullList;
        }

        return fullList.Where(x => !x.SourceTemplate.Cash).ToList();
    }

    public async Task increaseEquipExp(int expGain)
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
                if (!ii.HasTemplate(nEquip.getItemId()))
                {
                    continue;
                }

                await nEquip.gainItemExp(Client, expGain);
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

    public virtual async ValueTask DisposeAsync()
    {
        if (extraRecoveryTask != null)
        {
            await extraRecoveryTask.CancelAsync(true);
            extraRecoveryTask = null;
        }

        // already done on unregisterChairBuff
        /* if (chairRecoveryTask != null) { chairRecoveryTask.cancel(true); }
        chairRecoveryTask = null; */

        _pickerProcessor.Clear();

        if (MountModel != null)
        {
            MountModel.Dispose();
            MountModel = null;
        }

        partyQuest = null;

        Bag.Dispose();
    }

    public async Task logOff()
    {
        // 切换频道/退出商城的保存不能放在断开连接时处理
        await Client.CurrentServer.Send(async w =>
        {
            await SyncCharAsync(SyncCharacterTrigger.Logoff);

            RemoveWorldWatcher();
            setClient(new OfflineClient());
        });
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
        return Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset() - loginTime;
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


    public bool isChasing()
    {
        return chasing;
    }

    public void setChasing(bool chasing)
    {
        this.chasing = chasing;
    }

    public WorldChannel getChannelServer()
    {
        return Client.CurrentServer;
    }

    /// <summary>
    /// 功能已被移除，供js调用
    /// </summary>
    /// <returns></returns>
    public bool isRecvPartySearchInviteEnabled()
    {
        return false;
    }


    protected override bool IsVisibleForPlayerWithoutRange(Player chr)
    {
        return base.IsVisibleForPlayerWithoutRange(chr) && (!isHidden() || chr.isGM());
    }

    public override Player? Controller => this;

    public override async Task BroadcastMovement(Packet packet, Point pos)
    {
        foreach (var mapChr in MapModel.getAllPlayers())
        {
            if (mapChr == Controller)
            {
                continue;
            }

            if (IsVisibleForPlayerWithoutRange(mapChr))
            {
                await mapChr.SendPacket(packet);
            }
        }
    }



    public bool HideSummon { get; set; }
    public bool HidePet { get; set; }
}
