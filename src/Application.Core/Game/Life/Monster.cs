/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Shared.WzEntity;
using client;
using client.status;
using constants.id;
using constants.skills;
using net.packet;
using net.server.coordinator.world;
using net.server.services.task.channel;
using net.server.services.type;
using server;
using server.life;
using server.loot;
using server.maps;
using tools;

namespace Application.Core.Game.Life;

public class Monster : AbstractLifeObject
{
    private ILogger log;

    private ChangeableStats? ostats = null;  //unused, v83 WZs offers no support for changeable stats.
    private MonsterStats stats;
    private AtomicInteger hp = new AtomicInteger(1);
    private AtomicLong maxHpPlusHeal = new AtomicLong(1);
    private int mp;
    private WeakReference<IPlayer?> controller = new(null);
    private bool controllerHasAggro, controllerKnowsAboutAggro, controllerHasPuppet;
    private List<MonsterListener> listeners = new();
    private Dictionary<MonsterStatus, MonsterStatusEffect> stati = new();
    private List<MonsterStatus> alreadyBuffed = new();

    private int VenomMultiplier = 0;
    private bool fake = false;
    private bool _dropsDisabled = false;
    private HashSet<MobSkillId> usedSkills = new();
    private HashSet<int> usedAttacks = new();
    private HashSet<int>? calledMobOids = null;
    private WeakReference<Monster?> callerMob = new(null);
    private List<int> stolenItems = new(5);
    private int team;
    private int parentMobOid = 0;
    private int spawnEffect = 0;
    private Dictionary<int, AtomicLong> takenDamage = new();
    private ScheduledFuture? monsterItemDrop = null;
    private Action? removeAfterAction = null;
    private bool availablePuppetUpdate = true;

    private object externalLock = new object();
    private object monsterLock = new object();
    private object statiLock = new object();
    private object animationLock = new object();
    private object aggroUpdateLock = new object();

    public Monster(int id, MonsterStats stats) : base(id)
    {
        setStance(5);
        this.stats = stats.copy();
        hp.set(stats.getHp());
        mp = stats.getMp();

        maxHpPlusHeal.set(hp.get());

        log = LogFactory.GetLogger(LogType.Monster);
    }

    public Monster(Monster monster) : this(monster.getId(), monster.getStats())
    {
    }

    public void lockMonster()
    {
        Monitor.Enter(externalLock);
    }

    public void unlockMonster()
    {
        Monitor.Exit(externalLock);
    }

    public void setSpawnEffect(int effect)
    {
        spawnEffect = effect;
    }

    public int getSpawnEffect()
    {
        return spawnEffect;
    }

    public void disableDrops()
    {
        this._dropsDisabled = true;
    }

    public void enableDrops()
    {
        this._dropsDisabled = false;
    }

    public bool dropsDisabled()
    {
        return _dropsDisabled;
    }

    public int getParentMobOid()
    {
        return parentMobOid;
    }

    public void setParentMobOid(int parentMobId)
    {
        this.parentMobOid = parentMobId;
    }

    public int countAvailableMobSummons(int summonsSize, int skillLimit)
    {
        // limit prop for summons has another conotation, found thanks to MedicOP
        int summonsCount = this.calledMobOids?.Count ?? 0;
        return Math.Min(summonsSize, skillLimit - summonsCount);
    }

    public void addSummonedMob(Monster mob)
    {
        calledMobOids ??= [];

        calledMobOids.Add(mob.getObjectId());
        mob.setSummonerMob(this);
    }

    private void removeSummonedMob(int mobOid)
    {
        calledMobOids?.Remove(mobOid);
    }

    private void setSummonerMob(Monster mob)
    {
        this.callerMob = new(mob);
    }

    private void dispatchClearSummons()
    {
        if (this.callerMob.TryGetTarget(out var caller) && caller != null)
            caller.removeSummonedMob(this.getObjectId());

        this.calledMobOids = null;
    }

    public void pushRemoveAfterAction(Action run)
    {
        this.removeAfterAction = run;
    }

    public Action? popRemoveAfterAction()
    {
        var r = this.removeAfterAction;
        this.removeAfterAction = null;

        return r;
    }

    public int getHp()
    {
        return hp.get();
    }

    object addHpLock = new object();
    public void addHp(int hp)
    {
        lock (addHpLock)
        {
            if (this.hp.get() <= 0)
            {
                return;
            }
            this.hp.addAndGet(hp);
        }

    }

    Object setHpLock = new object();
    public void setStartingHp(int hp)
    {
        lock (setHpLock)
        {
            stats.setHp(hp);    // refactored mob stats after non-static HP pool suggestion thanks to twigs
            this.hp.set(hp);
        }

    }

    public int getMaxHp()
    {
        return stats.getHp();
    }

    public int getMp()
    {
        return mp;
    }

    public void setMp(int mp)
    {
        if (mp < 0)
        {
            mp = 0;
        }
        this.mp = mp;
    }

    public int getMaxMp()
    {
        return stats.getMp();
    }

    public int getExp()
    {
        return stats.getExp();
    }

    public int getLevel()
    {
        return stats.getLevel();
    }

    public int getCP()
    {
        return stats.getCP();
    }

    public int getTeam()
    {
        return team;
    }

    public void setTeam(int team)
    {
        this.team = team;
    }

    public int getVenomMulti()
    {
        return this.VenomMultiplier;
    }

    public void setVenomMulti(int multiplier)
    {
        this.VenomMultiplier = multiplier;
    }

    public MonsterStats getStats()
    {
        return stats;
    }

    public bool isBoss()
    {
        return stats.isBoss();
    }

    public int getAnimationTime(string name)
    {
        return stats.getAnimationTime(name);
    }

    private List<int> getRevives()
    {
        return stats.getRevives();
    }

    private byte getTagColor()
    {
        return stats.getTagColor();
    }

    private byte getTagBgColor()
    {
        return stats.getTagBgColor();
    }

    public void setHpZero()
    {
        // force HP = 0
        applyAndGetHpDamage(int.MaxValue, false);
    }

    private bool applyAnimationIfRoaming(int attackPos, MobSkill skill)
    {   // roam: not casting attack or skill animations
        if (!Monitor.TryEnter(animationLock))
        {
            return false;
        }

        try
        {
            long animationTime;

            if (skill == null)
            {
                animationTime = MonsterInformationProvider.getInstance().getMobAttackAnimationTime(this.getId(), attackPos);
            }
            else
            {
                animationTime = MonsterInformationProvider.getInstance().getMobSkillAnimationTime(skill);
            }

            if (animationTime > 0)
            {
                MobAnimationService service = (MobAnimationService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_ANIMATION);
                return service.registerMobOnAnimationEffect(MapModel.getId(), this.GetHashCode(), animationTime);
            }
            else
            {
                return true;
            }
        }
        finally
        {
            Monitor.Exit(animationLock);
        }
    }

    object applyHpDamageLock = new object();
    public int? applyAndGetHpDamage(int delta, bool stayAlive)
    {
        lock (applyHpDamageLock)
        {
            int curHp = hp.get();
            if (curHp <= 0)
            {       // this monster is already dead
                return null;
            }

            if (delta >= 0)
            {
                if (stayAlive)
                {
                    curHp--;
                }
                int trueDamage = Math.Min(curHp, delta);

                hp.addAndGet(-trueDamage);
                return trueDamage;
            }
            else
            {
                int trueHeal = -delta;
                int hp2Heal = curHp + trueHeal;
                int maxHp = getMaxHp();

                if (hp2Heal > maxHp)
                {
                    trueHeal -= (hp2Heal - maxHp);
                }

                hp.addAndGet(trueHeal);
                return trueHeal;
            }
        }
    }

    object disposeMapLock = new object();
    public void disposeMapObject()
    {     // mob is no longer associated with the map it was in
        lock (disposeMapLock)
        {
            hp.set(-1);
        }
    }

    public void broadcastMobHpBar(IPlayer from)
    {
        if (hasBossHPBar())
        {
            from.setPlayerAggro(this.GetHashCode());
            from.getMap().broadcastBossHpMessage(this, this.GetHashCode(), makeBossHPBarPacket(), getPosition());
        }
        else if (!isBoss())
        {
            int remainingHP = (int)Math.Max(1, hp.get() * 100f / getMaxHp());
            Packet packet = PacketCreator.showMonsterHP(getObjectId(), remainingHP);
            if (from.getParty() != null)
            {
                foreach (var mpc in from.getParty()!.getMembers())
                {
                    var member = from.getMap().getCharacterById(mpc.getId()); // god bless
                    if (member != null)
                    {
                        member.sendPacket(packet);
                    }
                }
            }
            else
            {
                from.sendPacket(packet);
            }
        }
    }

    public bool damage(IPlayer attacker, int damage, bool stayAlive)
    {
        bool lastHit = false;

        this.lockMonster();
        try
        {
            if (!this.isAlive())
            {
                return false;
            }

            /* pyramid not implemented
            Pair<int, int> cool = this.getStats().getCool();
            if (cool != null) {
                Pyramid pq = (Pyramid) chr.getPartyQuest();
                if (pq != null) {
                    if (damage > 0) {
                        if (damage >= cool.getLeft()) {
                            if ((Randomizer.nextDouble() * 100) < cool.getRight()) {
                                pq.cool();
                            } else {
                                pq.kill();
                            }
                        } else {
                            pq.kill();
                        }
                    } else {
                        pq.miss();
                    }
                    killed = true;
                }
            }
            */

            if (damage > 0)
            {
                this.applyDamage(attacker, damage, stayAlive, false);
                if (!this.isAlive())
                {  // monster just died
                    lastHit = true;
                }
            }
        }
        finally
        {
            this.unlockMonster();
        }

        return lastHit;
    }

    /**
     * @param from      the player that dealt the damage
     * @param damage
     * @param stayAlive
     */
    private void applyDamage(IPlayer from, int damage, bool stayAlive, bool fake)
    {
        var trueDamage = applyAndGetHpDamage(damage, stayAlive);
        if (trueDamage == null)
        {
            return;
        }

        if (YamlConfig.config.server.USE_DEBUG)
        {
            from.dropMessage(5, "Hitted MOB " + this.getId() + ", OID " + this.getObjectId());
        }

        if (!fake)
        {
            dispatchMonsterDamaged(from, trueDamage.Value);
        }

        if (!takenDamage.ContainsKey(from.getId()))
        {
            takenDamage.Add(from.getId(), new AtomicLong(trueDamage.Value));
        }
        else
        {
            takenDamage.GetValueOrDefault(from.getId())!.addAndGet(trueDamage.Value);
        }

        broadcastMobHpBar(from);
    }

    public void applyFakeDamage(IPlayer from, int damage, bool stayAlive)
    {
        applyDamage(from, damage, stayAlive, true);
    }

    public void heal(int hp, int mp)
    {
        var hpHealed = applyAndGetHpDamage(-hp, false);
        if (hpHealed == null)
        {
            return;
        }

        int mp2Heal = getMp() + mp;
        int maxMp = getMaxMp();
        if (mp2Heal >= maxMp)
        {
            mp2Heal = maxMp;
        }
        setMp(mp2Heal);

        if (hp > 0)
        {
            getMap().broadcastMessage(PacketCreator.healMonster(getObjectId(), hp, getHp(), getMaxHp()));
        }

        maxHpPlusHeal.addAndGet(hpHealed.Value);
        dispatchMonsterHealed(hpHealed.Value);
    }

    public bool isAttackedBy(IPlayer chr)
    {
        return takenDamage.ContainsKey(chr.getId());
    }

    private static bool isWhiteExpGain(IPlayer chr, Dictionary<int, float> personalRatio, double sdevRatio)
    {
        return personalRatio.TryGetValue(chr.getId(), out var pr) && pr >= sdevRatio;
    }

    private static double calcExperienceStandDevThreshold(List<float> entryExpRatio, int totalEntries)
    {
        float avgExpReward = 0.0f;
        foreach (float exp in entryExpRatio)
        {
            avgExpReward += exp;
        }

        // thanks Simon (HarborMS) for finding an issue with solo party player gaining yellow EXP when soloing mobs
        avgExpReward /= totalEntries;

        float varExpReward = 0.0f;
        foreach (float exp in entryExpRatio)
        {
            varExpReward += (float)Math.Pow(exp - avgExpReward, 2);
        }
        varExpReward /= entryExpRatio.Count;

        return avgExpReward + Math.Sqrt(varExpReward);
    }

    private void distributePlayerExperience(IPlayer chr, float exp, float partyBonusMod, int totalPartyLevel, bool highestPartyDamager, bool whiteExpGain, bool hasPartySharers)
    {
        float playerExp = (YamlConfig.config.server.EXP_SPLIT_COMMON_MOD * chr.getLevel()) / totalPartyLevel;
        if (highestPartyDamager)
        {
            playerExp += YamlConfig.config.server.EXP_SPLIT_MVP_MOD;
        }

        playerExp *= exp;
        float bonusExp = partyBonusMod * playerExp;

        this.giveExpToCharacter(chr, playerExp, bonusExp, whiteExpGain, hasPartySharers);
        giveFamilyRep(chr.getFamilyEntry());
    }

    private void distributePartyExperience(Dictionary<IPlayer, long> partyParticipation, float expPerDmg, HashSet<IPlayer> underleveled, Dictionary<int, float> personalRatio, double sdevRatio)
    {
        IntervalBuilder leechInterval = new IntervalBuilder();
        leechInterval.addInterval(this.getLevel() - YamlConfig.config.server.EXP_SPLIT_LEVEL_INTERVAL, this.getLevel() + YamlConfig.config.server.EXP_SPLIT_LEVEL_INTERVAL);

        long maxDamage = 0, partyDamage = 0;
        IPlayer? participationMvp = null;
        foreach (var e in partyParticipation)
        {
            long entryDamage = e.Value;
            partyDamage += entryDamage;

            if (maxDamage < entryDamage)
            {
                maxDamage = entryDamage;
                participationMvp = e.Key;
            }

            // thanks Thora for pointing out leech level limitation
            int chrLevel = e.Key.getLevel();
            leechInterval.addInterval(chrLevel - YamlConfig.config.server.EXP_SPLIT_LEECH_INTERVAL, chrLevel + YamlConfig.config.server.EXP_SPLIT_LEECH_INTERVAL);
        }

        List<IPlayer> expMembers = new();
        int totalPartyLevel = 0;

        // thanks G h o s t, Alfred, Vcoc, BHB for poiting out a bug in detecting party members after membership transactions in a party took place
        if (YamlConfig.config.server.USE_ENFORCE_MOB_LEVEL_RANGE)
        {
            foreach (IPlayer member in partyParticipation.Keys.First().getPartyMembersOnSameMap())
            {
                if (!leechInterval.inInterval(member.getLevel()))
                {
                    underleveled.Add(member);
                    continue;
                }

                totalPartyLevel += member.getLevel();
                expMembers.Add(member);
            }
        }
        else
        {    // thanks Ari for noticing unused server flag after EXP system overhaul
            foreach (IPlayer member in partyParticipation.Keys.First().getPartyMembersOnSameMap())
            {
                totalPartyLevel += member.getLevel();
                expMembers.Add(member);
            }
        }

        int membersSize = expMembers.Count;
        float participationExp = partyDamage * expPerDmg;

        // thanks Crypter for reporting an insufficiency on party exp bonuses
        bool hasPartySharers = membersSize > 1;
        float partyBonusMod = hasPartySharers ? 0.05f * membersSize : 0.0f;

        foreach (IPlayer mc in expMembers)
        {
            distributePlayerExperience(mc, participationExp, partyBonusMod, totalPartyLevel, mc == participationMvp, isWhiteExpGain(mc, personalRatio, sdevRatio), hasPartySharers);
            giveFamilyRep(mc.getFamilyEntry());
        }
    }

    private void distributeExperience(int killerId)
    {
        if (isAlive())
        {
            return;
        }

        Dictionary<ITeam, Dictionary<IPlayer, long>> partyExpDist = new();
        Dictionary<IPlayer, long> soloExpDist = new();

        Dictionary<int, IPlayer> mapPlayers = MapModel.getMapAllPlayers();

        int totalEntries = 0;   // counts "participant parties", players who no longer are available in the map is an "independent party"
        foreach (var e in takenDamage)
        {
            var chr = mapPlayers.GetValueOrDefault(e.Key);
            if (chr != null)
            {
                long damage = e.Value;

                var p = chr.getParty();
                if (p != null)
                {
                    var partyParticipation = partyExpDist.GetValueOrDefault(p);
                    if (partyParticipation == null)
                    {
                        partyParticipation = new(6);
                        partyExpDist.AddOrUpdate(p, partyParticipation);

                        totalEntries += 1;
                    }

                    partyParticipation.AddOrUpdate(chr, damage);
                }
                else
                {
                    soloExpDist.AddOrUpdate(chr, damage);
                    totalEntries += 1;
                }
            }
            else
            {
                totalEntries += 1;
            }
        }

        long totalDamage = maxHpPlusHeal.get();
        int mobExp = getExp();
        float expPerDmg = ((float)mobExp) / totalDamage;

        Dictionary<int, float> personalRatio = new();
        List<float> entryExpRatio = new();
        foreach (var e in soloExpDist)
        {
            float ratio = ((float)e.Value) / totalDamage;

            personalRatio.AddOrUpdate(e.Key.getId(), ratio);
            entryExpRatio.Add(ratio);
        }

        foreach (Dictionary<IPlayer, long> m in partyExpDist.Values)
        {
            float ratio = 0.0f;
            foreach (var e in m)
            {
                float chrRatio = ((float)e.Value) / totalDamage;

                personalRatio.AddOrUpdate(e.Key.getId(), chrRatio);
                ratio += chrRatio;
            }

            entryExpRatio.Add(ratio);
        }

        double sdevRatio = calcExperienceStandDevThreshold(entryExpRatio, totalEntries);

        // GMS-like player and party Split calculations found thanks to Russt, KaidaTan, Dusk, AyumiLove - src: https://ayumilovemaple.wordpress.com/maplestory_calculator_formula/
        HashSet<IPlayer> underleveled = new();
        foreach (var chrParticipation in soloExpDist)
        {
            float exp = chrParticipation.Value * expPerDmg;
            IPlayer chr = chrParticipation.Key;

            distributePlayerExperience(chr, exp, 0.0f, chr.getLevel(), true, isWhiteExpGain(chr, personalRatio, sdevRatio), false);
        }

        foreach (Dictionary<IPlayer, long> partyParticipation in partyExpDist.Values)
        {
            distributePartyExperience(partyParticipation, expPerDmg, underleveled, personalRatio, sdevRatio);
        }

        var eim = getMap().getEventInstance();
        if (eim != null)
        {
            var chr = mapPlayers.GetValueOrDefault(killerId);
            if (chr != null)
            {
                eim.monsterKilled(chr, this);
            }
        }

        foreach (IPlayer mc in underleveled)
        {
            mc.showUnderleveledInfo(this);
        }

    }

    private float getStatusExpMultiplier(IPlayer attacker, bool hasPartySharers)
    {
        float multiplier = 1.0f;

        // thanks Prophecy & Aika for finding out Holy Symbol not being applied on party bonuses
        var holySymbol = attacker.getBuffedValue(BuffStat.HOLY_SYMBOL);
        if (holySymbol != null)
        {
            if (YamlConfig.config.server.USE_FULL_HOLY_SYMBOL)
            { // thanks Mordred, xinyifly, AyumiLove, andy33 for noticing HS hands out 20% of its potential on less than 3 players
                multiplier *= (1.0f + (holySymbol.Value / 100.0f));
            }
            else
            {
                multiplier *= (1.0f + (holySymbol.Value / (hasPartySharers ? 100.0f : 500.0f)));
            }
        }

        Monitor.Enter(statiLock);
        try
        {
            var mse = stati.GetValueOrDefault(MonsterStatus.SHOWDOWN);
            if (mse != null)
            {
                multiplier *= (1.0f + (mse.getStati().GetValueOrDefault(MonsterStatus.SHOWDOWN, 0) / 100.0f));
            }
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        return multiplier;
    }

    private static int expValueToInteger(double exp)
    {
        if (exp > int.MaxValue)
        {
            exp = int.MaxValue;
        }
        else if (exp < int.MinValue)
        {
            exp = int.MinValue;
        }

        return (int)Math.Round(exp);    // operations on float point are not point-precise... thanks IxianMace for noticing -1 EXP gains
    }

    private void giveExpToCharacter(IPlayer attacker, float? personalExp, float? partyExp, bool white, bool hasPartySharers)
    {
        if (attacker.isAlive())
        {
            if (personalExp != null)
            {
                personalExp *= getStatusExpMultiplier(attacker, hasPartySharers);
                personalExp *= attacker.getExpRate();
            }
            else
            {
                personalExp = 0.0f;
            }

            var expBonus = attacker.getBuffedValue(BuffStat.EXP_INCREASE);
            if (expBonus != null)
            {     // exp increase player buff found thanks to HighKey21
                personalExp += expBonus.Value;
            }

            var expBuff = attacker.getBuffedValue(BuffStat.EXP_BUFF);
            if (expBuff != null)
            {
                personalExp *= expBuff.Value / 100;
            }

            int _personalExp = expValueToInteger(personalExp.Value); // assuming no negative xp here

            if (partyExp != null)
            {
                partyExp *= getStatusExpMultiplier(attacker, hasPartySharers);
                partyExp *= attacker.getExpRate();
                partyExp *= YamlConfig.config.server.PARTY_BONUS_EXP_RATE;
            }
            else
            {
                partyExp = 0.0f;
            }

            int _partyExp = expValueToInteger(partyExp.Value);

            if (attacker.hasDisease(Disease.CURSE))
            {
                _personalExp = (int)(_personalExp * 0.5);
                _partyExp = (int)(_partyExp * 0.5);
            }

            attacker.gainExp(_personalExp, _partyExp, true, false, white);
            attacker.increaseEquipExp(_personalExp);
            attacker.raiseQuestMobCount(getId());
        }
    }

    /// <summary>
    /// 只生成击杀玩家可收集的道具
    /// </summary>
    /// <returns></returns>
    public List<DropEntry> retrieveRelevantDrops()
    {
        if (this.getStats().isFriendly())
        {
            // thanks Conrad for noticing friendly mobs not spawning loots after a recent update
            return MonsterInformationProvider.getInstance().retrieveEffectiveDrop(this.getId());
        }

        Dictionary<int, IPlayer> pchars = MapModel.getMapAllPlayers();

        List<IPlayer> lootChars = new();
        foreach (int cid in takenDamage.Keys)
        {
            var chr = pchars.GetValueOrDefault(cid);
            if (chr != null && chr.isLoggedinWorld())
            {
                lootChars.Add(chr);
            }
        }

        return LootManager.retrieveRelevantDrops(this.getId(), lootChars);
    }

    public IPlayer? killBy(IPlayer? killer)
    {
        distributeExperience(killer != null ? killer.getId() : 0);

        var lastController = aggroRemoveController();
        List<int> toSpawn = this.getRevives();
        if (toSpawn != null)
        {
            var reviveMap = MapModel;
            if (toSpawn.Contains(MobId.TRANSPARENT_ITEM) && reviveMap.getId() > 925000000 && reviveMap.getId() < 926000000)
            {
                reviveMap.broadcastMessage(PacketCreator.playSound("Dojang/clear"));
                reviveMap.broadcastMessage(PacketCreator.showEffect("dojang/end/clear"));
            }
            var timeMob = reviveMap.TimeMob;
            if (timeMob != null)
            {
                if (toSpawn.Contains(timeMob.MobId))
                {
                    reviveMap.broadcastMessage(PacketCreator.serverNotice(6, timeMob.Message));
                }
            }

            if (toSpawn.Count > 0)
            {
                var eim = this.getMap().getEventInstance();

                TimerManager.getInstance().schedule(() =>
                {
                    var controller = lastController.Key;
                    bool aggro = lastController.Value;

                    foreach (int mid in toSpawn)
                    {
                        var mob = LifeFactory.GetMonsterTrust(mid);
                        mob.setPosition(getPosition());
                        mob.setFh(getFh());
                        mob.setParentMobOid(getObjectId());

                        if (dropsDisabled())
                        {
                            mob.disableDrops();
                        }
                        reviveMap.spawnMonster(mob);

                        if (MobId.isDeadHorntailPart(mob.getId()) && reviveMap.isHorntailDefeated())
                        {
                            bool htKilled = false;
                            var ht = reviveMap.getMonsterById(MobId.HORNTAIL);

                            if (ht != null)
                            {
                                ht.lockMonster();
                                try
                                {
                                    htKilled = ht.isAlive();
                                    ht.setHpZero();
                                }
                                finally
                                {
                                    ht.unlockMonster();
                                }

                                if (htKilled)
                                {
                                    reviveMap.killMonster(ht, killer, true);
                                }
                            }

                            for (int i = MobId.DEAD_HORNTAIL_MAX; i >= MobId.DEAD_HORNTAIL_MIN; i--)
                            {
                                reviveMap.killMonster(reviveMap.getMonsterById(i), killer, true);
                            }
                        }
                        else if (controller != null)
                        {
                            mob.aggroSwitchController(controller, aggro);
                        }

                        if (eim != null)
                        {
                            eim.reviveMonster(mob);
                        }
                    }
                }, getAnimationTime("die1"));
            }
        }
        else
        {  // is this even necessary?
            log.Warning("[CRITICAL LOSS] toSpawn is null for {MonsterName}", getName());
        }

        return MapModel.getCharacterById(getHighestDamagerId()) ?? killer;
    }

    public void dropFromFriendlyMonster(long delay)
    {
        Monster m = this;
        monsterItemDrop = TimerManager.getInstance().register(() =>
        {
            if (!m.isAlive())
            {
                if (monsterItemDrop != null)
                {
                    monsterItemDrop.cancel(false);
                }

                return;
            }

            var map = m.getMap();
            List<IPlayer> chrList = map.getAllPlayers();
            if (chrList.Count > 0)
            {
                IPlayer chr = chrList.get(0);

                var eim = map.getEventInstance();
                if (eim != null)
                {
                    eim.friendlyItemDrop(m);
                }

                map.dropFromFriendlyMonster(chr, m);
            }
        }, delay, delay);
    }

    private void dispatchRaiseQuestMobCount()
    {
        var attackerChrids = takenDamage.Keys;
        if (attackerChrids.Count > 0)
        {
            Dictionary<int, IPlayer> mapChars = MapModel.getMapPlayers();
            if (mapChars.Count > 0)
            {
                int mobid = getId();

                foreach (int chrid in attackerChrids)
                {
                    var chr = mapChars.GetValueOrDefault(chrid);

                    if (chr != null && chr.isLoggedinWorld())
                    {
                        chr.raiseQuestMobCount(mobid);
                    }
                }
            }
        }
    }

    public void dispatchMonsterKilled(bool hasKiller)
    {
        processMonsterKilled(hasKiller);

        var eim = getMap().getEventInstance();
        if (eim != null)
        {
            if (!this.getStats().isFriendly())
            {
                eim.monsterKilled(this, hasKiller);
            }
            else
            {
                eim.friendlyKilled(this, hasKiller);
            }
        }
    }

    object monsterKilledLock = new object();
    private void processMonsterKilled(bool hasKiller)
    {
        lock (monsterKilledLock)
        {

            if (!hasKiller)
            {    // players won't gain EXP from a mob that has no killer, but a quest count they should
                dispatchRaiseQuestMobCount();
            }

            this.aggroClearDamages();
            this.dispatchClearSummons();

            MonsterListener[] listenersList;
            Monitor.Enter(statiLock);
            try
            {
                listenersList = listeners.ToArray();
            }
            finally
            {
                Monitor.Exit(statiLock);
            }

            foreach (MonsterListener listener in listenersList)
            {
                listener.monsterKilled?.Invoke(getAnimationTime("die1"));
            }

            Monitor.Enter(statiLock);
            try
            {
                stati.Clear();
                alreadyBuffed.Clear();
                listeners.Clear();
            }
            finally
            {
                Monitor.Exit(statiLock);
            }

        }
    }

    private void dispatchMonsterDamaged(IPlayer from, int trueDmg)
    {
        MonsterListener[] listenersList;
        Monitor.Enter(statiLock);
        try
        {
            listenersList = listeners.ToArray();
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        foreach (MonsterListener listener in listenersList)
        {
            listener.monsterDamaged?.Invoke(from, trueDmg);
        }
    }

    private void dispatchMonsterHealed(int trueHeal)
    {
        MonsterListener[] listenersList;
        Monitor.Enter(statiLock);
        try
        {
            listenersList = listeners.ToArray();
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        foreach (MonsterListener listener in listenersList)
        {
            listener.monsterHealed?.Invoke(trueHeal);
        }
    }

    private void giveFamilyRep(FamilyEntry? entry)
    {
        if (entry != null)
        {
            int repGain = isBoss() ? YamlConfig.config.server.FAMILY_REP_PER_BOSS_KILL : YamlConfig.config.server.FAMILY_REP_PER_KILL;
            if (getMaxHp() <= 1)
            {
                repGain = 0; //don't count trash mobs
            }
            entry.giveReputationToSenior(repGain, true);
        }
    }

    public int getHighestDamagerId()
    {
        int curId = 0;
        long curDmg = 0;

        foreach (var damage in takenDamage)
        {
            curId = damage.Value.get() >= curDmg ? damage.Key : curId;
            curDmg = damage.Key == curId ? damage.Value.get() : curDmg;
        }

        return curId;
    }

    public bool isAlive()
    {
        return this.hp.get() > 0;
    }

    public void addListener(MonsterListener listener)
    {
        Monitor.Enter(statiLock);
        try
        {
            listeners.Add(listener);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    public IPlayer? getController()
    {
        return controller.TryGetTarget(out var d) ? d : null;
    }

    private void setController(IPlayer? controller)
    {
        this.controller = new(controller);
    }

    public bool isControllerHasAggro()
    {
        return !fake && controllerHasAggro;
    }

    private void setControllerHasAggro(bool controllerHasAggro)
    {
        if (!fake)
        {
            this.controllerHasAggro = controllerHasAggro;
        }
    }

    public bool isControllerKnowsAboutAggro()
    {
        return !fake && controllerKnowsAboutAggro;
    }

    private void setControllerKnowsAboutAggro(bool controllerKnowsAboutAggro)
    {
        if (!fake)
        {
            this.controllerKnowsAboutAggro = controllerKnowsAboutAggro;
        }
    }

    private void setControllerHasPuppet(bool controllerHasPuppet)
    {
        this.controllerHasPuppet = controllerHasPuppet;
    }

    public Packet makeBossHPBarPacket()
    {
        return PacketCreator.showBossHP(getId(), getHp(), getMaxHp(), getTagColor(), getTagBgColor());
    }

    public bool hasBossHPBar()
    {
        return isBoss() && getTagColor() > 0;
    }

    public override void sendSpawnData(IClient client)
    {
        if (hp.get() <= 0)
        { // mustn't monsterLock this function
            return;
        }
        if (fake)
        {
            client.sendPacket(PacketCreator.spawnFakeMonster(this, 0));
        }
        else
        {
            client.sendPacket(PacketCreator.spawnMonster(this, false));
        }

        if (hasBossHPBar())
        {
            client.announceBossHpBar(this, this.GetHashCode(), makeBossHPBarPacket());
        }
    }

    public override void sendDestroyData(IClient client)
    {
        client.sendPacket(PacketCreator.killMonster(getObjectId(), false));
        client.sendPacket(PacketCreator.killMonster(getObjectId(), true));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.MONSTER;
    }

    public bool isMobile()
    {
        return stats.isMobile();
    }

    public override bool isFacingLeft()
    {
        int fixedStance = stats.getFixedStance();    // thanks DimDiDima for noticing inconsistency on some AOE mobskills
        if (fixedStance != 0)
        {
            return Math.Abs(fixedStance) % 2 == 1;
        }

        return base.isFacingLeft();
    }

    public ElementalEffectiveness getElementalEffectiveness(Element e)
    {
        Monitor.Enter(statiLock);
        try
        {
            if (stati.GetValueOrDefault(MonsterStatus.DOOM) != null)
            {
                return ElementalEffectiveness.NORMAL; // like blue snails
            }
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        return getMonsterEffectiveness(e);
    }

    private ElementalEffectiveness getMonsterEffectiveness(Element e)
    {
        Monitor.Enter(monsterLock);
        try
        {
            return stats.getEffectiveness(e);
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    private IPlayer? getActiveController()
    {
        var chr = getController();

        if (chr != null && chr.isLoggedinWorld() && chr.getMap() == this.getMap())
        {
            return chr;
        }
        else
        {
            return null;
        }
    }

    private void broadcastMonsterStatusMessage(Packet packet)
    {
        MapModel.broadcastMessage(packet, getPosition());

        var chrController = getActiveController();
        if (chrController != null && !chrController.isMapObjectVisible(this))
        {
            chrController.sendPacket(packet);
        }
    }

    private int broadcastStatusEffect(MonsterStatusEffect status)
    {
        int animationTime = status.getSkill()!.getAnimationTime();
        Packet packet = PacketCreator.applyMonsterStatus(getObjectId(), status, null);
        broadcastMonsterStatusMessage(packet);

        return animationTime;
    }

    public bool applyStatus(IPlayer from, MonsterStatusEffect status, bool poison, long duration, bool venom = false)
    {
        var effectSkill = status.getSkill()!;
        switch (getMonsterEffectiveness(effectSkill.getElement()))
        {
            case ElementalEffectiveness.IMMUNE:
            case ElementalEffectiveness.STRONG:
            case ElementalEffectiveness.NEUTRAL:
                return false;
            case ElementalEffectiveness.NORMAL:
            case ElementalEffectiveness.WEAK:
                break;
            default:
                {
                    log.Warning("Unknown elemental effectiveness: {MonsterEffectiveness}", getMonsterEffectiveness(effectSkill.getElement()));
                    return false;
                }
        }

        if (effectSkill.getId() == FPMage.ELEMENT_COMPOSITION)
        {
            // fp compo
            ElementalEffectiveness effectiveness = getMonsterEffectiveness(Element.POISON);
            if (effectiveness == ElementalEffectiveness.IMMUNE || effectiveness == ElementalEffectiveness.STRONG)
            {
                return false;
            }
        }
        else if (effectSkill.getId() == ILMage.ELEMENT_COMPOSITION)
        {
            // il compo
            ElementalEffectiveness effectiveness = getMonsterEffectiveness(Element.ICE);
            if (effectiveness == ElementalEffectiveness.IMMUNE || effectiveness == ElementalEffectiveness.STRONG)
            {
                return false;
            }
        }
        else if (effectSkill.getId() == NightLord.VENOMOUS_STAR || effectSkill.getId() == Shadower.VENOMOUS_STAB || effectSkill.getId() == NightWalker.VENOM)
        {
            // venom
            if (getMonsterEffectiveness(Element.POISON) == ElementalEffectiveness.WEAK)
            {
                return false;
            }
        }
        if (poison && hp.get() <= 1)
        {
            return false;
        }

        Dictionary<MonsterStatus, int> statis = status.getStati();
        if (stats.isBoss())
        {
            if (!(statis.ContainsKey(MonsterStatus.SPEED)
                    && statis.ContainsKey(MonsterStatus.NINJA_AMBUSH)
                    && statis.ContainsKey(MonsterStatus.WATK)))
            {
                return false;
            }
        }

        int mapid = MapModel.getId();
        if (statis.Count > 0)
        {
            Monitor.Enter(statiLock);
            try
            {
                foreach (MonsterStatus stat in statis.Keys)
                {
                    var oldEffect = stati.GetValueOrDefault(stat);
                    if (oldEffect != null)
                    {
                        oldEffect.removeActiveStatus(stat);
                        if (oldEffect.getStati().Count == 0)
                        {
                            MobStatusService serviced = (MobStatusService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_STATUS);
                            serviced.interruptMobStatus(mapid, oldEffect);
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(statiLock);
            }
        }

        Action cancelTask = () =>
        {
            if (isAlive())
            {
                Packet packet = PacketCreator.cancelMonsterStatus(getObjectId(), status.getStati());
                broadcastMonsterStatusMessage(packet);
            }

            Monitor.Enter(statiLock);
            try
            {
                foreach (MonsterStatus stat in status.getStati().Keys)
                {
                    stati.Remove(stat);
                }
            }
            finally
            {
                Monitor.Exit(statiLock);
            }

            setVenomMulti(0);
        };
        AbstractRunnable? overtimeAction = null;
        int overtimeDelay = -1;

        int animationTime;
        if (poison)
        {
            int poisonLevel = from.getSkillLevel(status.getSkill());
            int poisonDamage = Math.Min(short.MaxValue, (int)(getMaxHp() / (70.0 - poisonLevel) + 0.999));
            status.setValue(MonsterStatus.POISON, poisonDamage);
            animationTime = broadcastStatusEffect(status);

            overtimeAction = new DamageTask(this, poisonDamage, from, status, 0);
            overtimeDelay = 1000;
        }
        else if (venom)
        {
            if (from.getJob() == Job.NIGHTLORD || from.getJob() == Job.SHADOWER || from.getJob().isA(Job.NIGHTWALKER3))
            {
                int poisonLevel, matk, jobid = from.getJob().getId();
                int skillid = (jobid == JobId.NIGHTLORD ? NightLord.VENOMOUS_STAR : (jobid == JobId.SHADOWER ? Shadower.VENOMOUS_STAB : NightWalker.VENOM));
                var skill = SkillFactory.getSkill(skillid);
                poisonLevel = from.getSkillLevel(skill);
                if (poisonLevel <= 0)
                {
                    return false;
                }
                matk = skill!.getEffect(poisonLevel).getMatk();
                int luk = from.getLuk();
                int maxDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.2 * luk * matk));
                int minDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.1 * luk * matk));
                int gap = maxDmg - minDmg;
                if (gap == 0)
                {
                    gap = 1;
                }
                int poisonDamage = 0;
                for (int i = 0; i < getVenomMulti(); i++)
                {
                    poisonDamage += (Randomizer.nextInt(gap) + minDmg);
                }
                poisonDamage = Math.Min(short.MaxValue, poisonDamage);
                status.setValue(MonsterStatus.VENOMOUS_WEAPON, poisonDamage);
                status.setValue(MonsterStatus.POISON, poisonDamage);
                animationTime = broadcastStatusEffect(status);

                overtimeAction = new DamageTask(this, poisonDamage, from, status, 0);
                overtimeDelay = 1000;
            }
            else
            {
                return false;
            }
            /*
        } else if (status.getSkill().getId() == Hermit.SHADOW_WEB || status.getSkill().getId() == NightWalker.SHADOW_WEB) { //Shadow Web
            int webDamage = (int) (getMaxHp() / 50.0 + 0.999);
            status.setValue(MonsterStatus.SHADOW_WEB, webDamage);
            animationTime = broadcastStatusEffect(status);
            
            overtimeAction = new DamageTask(webDamage, from, status, 1);
            overtimeDelay = 3500;
            */
        }
        else if (effectSkill.getId() == NightLord.NINJA_AMBUSH || effectSkill.getId() == Shadower.NINJA_AMBUSH)
        {
            // Ninja Ambush
            var skill = SkillFactory.GetSkillTrust(effectSkill.getId());
            var level = from.getSkillLevel(skill);
            int damage = (int)((from.getStr() + from.getLuk()) * ((3.7 * skill.getEffect(level).getDamage()) / 100));

            status.setValue(MonsterStatus.NINJA_AMBUSH, damage);
            animationTime = broadcastStatusEffect(status);

            overtimeAction = new DamageTask(this, damage, from, status, 2);
            overtimeDelay = 1000;
        }
        else
        {
            animationTime = broadcastStatusEffect(status);
        }

        Monitor.Enter(statiLock);
        try
        {
            foreach (MonsterStatus stat in status.getStati().Keys)
            {
                stati.AddOrUpdate(stat, status);
                alreadyBuffed.Add(stat);
            }
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        MobStatusService service = (MobStatusService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_STATUS);
        service.registerMobStatus(mapid, status, cancelTask, duration + animationTime - 100, overtimeAction, overtimeDelay);
        return true;
    }

    public void dispelSkill(MobSkill skill)
    {
        List<MonsterStatus> toCancel = new();
        foreach (var effects in stati)
        {
            MonsterStatusEffect mse = effects.Value;
            if (mse.getMobSkill()?.getType() == skill.getType())
            {
                //not checking for level.
                toCancel.Add(effects.Key);
            }
        }
        foreach (MonsterStatus stat in toCancel)
        {
            debuffMobStat(stat);
        }
    }

    public void applyMonsterBuff(Dictionary<MonsterStatus, int> stats, int x, long duration, MobSkill skill, List<int> reflection)
    {
        Action cancelTask = () =>
        {
            if (isAlive())
            {
                Packet packet = PacketCreator.cancelMonsterStatus(getObjectId(), stats);
                broadcastMonsterStatusMessage(packet);

                Monitor.Enter(statiLock);
                try
                {
                    foreach (MonsterStatus stat in stats.Keys)
                    {
                        stati.Remove(stat);
                    }
                }
                finally
                {
                    Monitor.Exit(statiLock);
                }
            }
        };
        MonsterStatusEffect effect = new MonsterStatusEffect(stats, skill);
        Packet packet = PacketCreator.applyMonsterStatus(getObjectId(), effect, reflection);
        broadcastMonsterStatusMessage(packet);

        Monitor.Enter(statiLock);
        try
        {
            foreach (MonsterStatus stat in stats.Keys)
            {
                stati.AddOrUpdate(stat, effect);
                alreadyBuffed.Add(stat);
            }
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        MobStatusService service = (MobStatusService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_STATUS);
        service.registerMobStatus(MapModel.getId(), effect, cancelTask, duration);
    }

    public void refreshMobPosition()
    {
        resetMobPosition(getPosition());
    }

    public void resetMobPosition(Point newPoint)
    {
        aggroRemoveController();

        setPosition(newPoint);
        MapModel.broadcastMessage(
            PacketCreator.moveMonster(this.getObjectId(), false, -1, 0, 0, 0, this.getPosition(), this.getIdleMovement(), AbstractAnimatedMapObject.IDLE_MOVEMENT_PACKET_LENGTH));
        MapModel.moveMonster(this, this.getPosition());

        aggroUpdateController();
    }

    private void debuffMobStat(MonsterStatus stat)
    {
        MonsterStatusEffect? oldEffect;
        Monitor.Enter(statiLock);
        try
        {
            stati.Remove(stat, out oldEffect);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }

        if (oldEffect != null)
        {
            Packet packet = PacketCreator.cancelMonsterStatus(getObjectId(), oldEffect.getStati());
            broadcastMonsterStatusMessage(packet);
        }
    }

    public void debuffMob(int skillid)
    {
        MonsterStatus[] statups = { MonsterStatus.WEAPON_ATTACK_UP, MonsterStatus.WEAPON_DEFENSE_UP, MonsterStatus.MAGIC_ATTACK_UP, MonsterStatus.MAGIC_DEFENSE_UP };
        Monitor.Enter(statiLock);
        try
        {
            if (skillid == Hermit.SHADOW_MESO)
            {
                debuffMobStat(statups[1]);
                debuffMobStat(statups[3]);
            }
            else if (skillid == Priest.DISPEL)
            {
                foreach (MonsterStatus ms in statups)
                {
                    debuffMobStat(ms);
                }
            }
            else
            {    // is a crash skill
                int i = (skillid == Crusader.ARMOR_CRASH ? 1 : (skillid == WhiteKnight.MAGIC_CRASH ? 2 : 0));
                debuffMobStat(statups[i]);

                if (YamlConfig.config.server.USE_ANTI_IMMUNITY_CRASH)
                {
                    if (skillid == Crusader.ARMOR_CRASH)
                    {
                        if (!isBuffed(MonsterStatus.WEAPON_REFLECT))
                        {
                            debuffMobStat(MonsterStatus.WEAPON_IMMUNITY);
                        }
                        if (!isBuffed(MonsterStatus.MAGIC_REFLECT))
                        {
                            debuffMobStat(MonsterStatus.MAGIC_IMMUNITY);
                        }
                    }
                    else if (skillid == WhiteKnight.MAGIC_CRASH)
                    {
                        if (!isBuffed(MonsterStatus.MAGIC_REFLECT))
                        {
                            debuffMobStat(MonsterStatus.MAGIC_IMMUNITY);
                        }
                    }
                    else
                    {
                        if (!isBuffed(MonsterStatus.WEAPON_REFLECT))
                        {
                            debuffMobStat(MonsterStatus.WEAPON_IMMUNITY);
                        }
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    public bool isBuffed(MonsterStatus status)
    {
        Monitor.Enter(statiLock);
        try
        {
            return stati.ContainsKey(status);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    public void setFake(bool fake)
    {
        Monitor.Enter(monsterLock);
        try
        {
            this.fake = fake;
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    public bool isFake()
    {
        Monitor.Enter(monsterLock);
        try
        {
            return fake;
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    public MonsterAggroCoordinator getMapAggroCoordinator()
    {
        return MapModel.getAggroCoordinator();
    }

    public HashSet<MobSkillId> getSkills()
    {
        return stats.getSkills();
    }

    public bool hasSkill(int skillId, int level)
    {
        return stats.hasSkill(skillId, level);
    }

    public bool canUseSkill(MobSkill? toUse, bool apply)
    {
        if (toUse == null || isBuffed(MonsterStatus.SEAL_SKILL))
        {
            return false;
        }

        if (isReflectSkill(toUse))
        {
            if (this.isBuffed(MonsterStatus.WEAPON_REFLECT) || this.isBuffed(MonsterStatus.MAGIC_REFLECT))
            {
                return false;
            }
        }

        Monitor.Enter(monsterLock);
        try
        {
            if (usedSkills.Contains(toUse.getId()))
            {
                return false;
            }

            int mpCon = toUse.getMpCon();
            if (mp < mpCon)
            {
                return false;
            }

            /*
            if (!this.applyAnimationIfRoaming(-1, toUse)) {
                return false;
            }
            */

            if (apply)
            {
                this.usedSkill(toUse);
            }
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }

        return true;
    }

    private bool isReflectSkill(MobSkill mobSkill)
    {
        return (mobSkill.getType()) switch
        {
            MobSkillType.PHYSICAL_COUNTER or
             MobSkillType.MAGIC_COUNTER or MobSkillType.PHYSICAL_AND_MAGIC_COUNTER => true,
            _ => false
        };
    }

    private void usedSkill(MobSkill skill)
    {
        MobSkillId msId = skill.getId();
        Monitor.Enter(monsterLock);
        try
        {
            mp -= skill.getMpCon();

            this.usedSkills.Add(msId);
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }

        Monster mons = this;
        var mmap = mons.getMap();
        var r = () => mons.clearSkill(skill.getId());

        MobClearSkillService service = (MobClearSkillService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_CLEAR_SKILL);
        service.registerMobClearSkillAction(mmap.getId(), r, skill.getCoolTime());
    }

    private void clearSkill(MobSkillId msId)
    {
        Monitor.Enter(monsterLock);
        try
        {
            usedSkills.Remove(msId);
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    public int canUseAttack(int attackPos, bool isSkill)
    {
        Monitor.Enter(monsterLock);
        try
        {
            /*
            if (usedAttacks.Contains(attackPos)) {
                return -1;
            }
            */

            var attackInfo = MonsterInformationProvider.getInstance().getMobAttackInfo(this.getId(), attackPos);
            if (attackInfo == null)
            {
                return -1;
            }

            int mpCon = attackInfo.Value.Key;
            if (mp < mpCon)
            {
                return -1;
            }

            /*
            if (!this.applyAnimationIfRoaming(attackPos, null)) {
                return -1;
            }
            */

            usedAttack(attackPos, mpCon, attackInfo.Value.Value);
            return 1;
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    private void usedAttack(int attackPos, int mpCon, int cooltime)
    {
        Monitor.Enter(monsterLock);
        try
        {
            mp -= mpCon;
            usedAttacks.Add(attackPos);

            Monster mons = this;
            var mmap = mons.getMap();
            var r = () => mons.clearAttack(attackPos);

            MobClearSkillService service = (MobClearSkillService)MapModel.getChannelServer().getServiceAccess(ChannelServices.MOB_CLEAR_SKILL);
            service.registerMobClearSkillAction(mmap.getId(), r, cooltime);
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    private void clearAttack(int attackPos)
    {
        Monitor.Enter(monsterLock);
        try
        {
            usedAttacks.Remove(attackPos);
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    public bool hasAnySkill()
    {
        return this.stats.getNoSkills() > 0;
    }

    public MobSkillId? getRandomSkill()
    {
        HashSet<MobSkillId> skills = stats.getSkills();
        if (skills.Count == 0)
        {
            return null;
        }

        // There is no simple way of getting a random element from a Set. Have to make do with this.
        return Randomizer.Select(skills);
    }

    public bool isFirstAttack()
    {
        return this.stats.isFirstAttack();
    }

    public int getBuffToGive()
    {
        return this.stats.getBuffToGive();
    }

    public class DamageTask : AbstractRunnable
    {

        private int dealDamage;
        private IPlayer chr;
        private MonsterStatusEffect status;
        private int type;
        private IMap map;
        readonly Monster _monster;

        public DamageTask(Monster mapleMonster, int dealDamage, IPlayer chr, MonsterStatusEffect status, int type)
        {
            _monster = mapleMonster;
            this.dealDamage = dealDamage;
            this.chr = chr;
            this.status = status;
            this.type = type;
            this.map = chr.getMap();
        }

        public override void HandleRun()
        {
            int curHp = _monster.hp.get();
            if (curHp <= 1)
            {
                MobStatusService service = (MobStatusService)map.getChannelServer().getServiceAccess(ChannelServices.MOB_STATUS);
                service.interruptMobStatus(map.getId(), status);
                return;
            }

            int damage = dealDamage;
            if (damage >= curHp)
            {
                damage = curHp - 1;
                if (type == 1 || type == 2)
                {
                    MobStatusService service = (MobStatusService)map.getChannelServer().getServiceAccess(ChannelServices.MOB_STATUS);
                    service.interruptMobStatus(map.getId(), status);
                }
            }
            if (damage > 0)
            {
                _monster.lockMonster();
                try
                {
                    _monster.applyDamage(chr, damage, true, false);
                }
                finally
                {
                    _monster.unlockMonster();
                }

                if (type == 1)
                {
                    map.broadcastMessage(PacketCreator.damageMonster(_monster.getObjectId(), damage), _monster.getPosition());
                }
                else if (type == 2)
                {
                    if (damage < dealDamage)
                    {    // ninja ambush (type 2) is already displaying DOT to the caster
                        map.broadcastMessage(PacketCreator.damageMonster(_monster.getObjectId(), damage), _monster.getPosition());
                    }
                }
            }
        }
    }

    public string getName()
    {
        return stats.getName();
    }

    public void addStolen(int itemId)
    {
        stolenItems.Add(itemId);
    }

    public List<int> getStolen()
    {
        return stolenItems;
    }

    public void setTempEffectiveness(Element e, ElementalEffectiveness ee, long milli)
    {
        Monitor.Enter(monsterLock);
        try
        {
            Element fE = e;
            ElementalEffectiveness fEE = stats.getEffectiveness(e);
            if (!fEE.Equals(ElementalEffectiveness.WEAK))
            {
                stats.setEffectiveness(e, ee);

                var mmap = this.getMap();
                var r = () =>
                {
                    Monitor.Enter(monsterLock);
                    try
                    {
                        stats.removeEffectiveness(fE);
                        stats.setEffectiveness(fE, fEE);
                    }
                    finally
                    {
                        Monitor.Exit(monsterLock);
                    }
                };

                MobClearSkillService service = (MobClearSkillService)mmap.getChannelServer().getServiceAccess(ChannelServices.MOB_CLEAR_SKILL);
                service.registerMobClearSkillAction(mmap.getId(), r, milli);
            }
        }
        finally
        {
            Monitor.Exit(monsterLock);
        }
    }

    public ICollection<MonsterStatus> alreadyBuffedStats()
    {
        Monitor.Enter(statiLock);
        try
        {
            return new List<MonsterStatus>(alreadyBuffed);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    public BanishInfo? getBanish()
    {
        return stats.getBanishInfo();
    }

    public void setBoss(bool boss)
    {
        this.stats.setBoss(boss);
    }

    public int getDropPeriodTime()
    {
        return stats.getDropPeriod();
    }

    public int getPADamage()
    {
        return stats.getPADamage();
    }

    public Dictionary<MonsterStatus, MonsterStatusEffect> getStati()
    {
        Monitor.Enter(statiLock);
        try
        {
            return new(stati);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    public MonsterStatusEffect? getStati(MonsterStatus ms)
    {
        Monitor.Enter(statiLock);
        try
        {
            return stati.GetValueOrDefault(ms);
        }
        finally
        {
            Monitor.Exit(statiLock);
        }
    }

    // ---- one can always have fun trying these pieces of codes below in-game rofl ----

    public ChangeableStats? getChangedStats()
    {
        return ostats;
    }

    public int getMobMaxHp()
    {
        if (ostats != null)
        {
            return ostats.hp;
        }
        return stats.getHp();
    }

    public void setOverrideStats(OverrideMonsterStats ostats)
    {
        this.ostats = new ChangeableStats(stats, ostats);
        this.hp.set(ostats.getHp());
        this.mp = ostats.getMp();
    }

    public void changeLevel(int newLevel, bool pqMob = true)
    {
        if (!stats.isChangeable())
        {
            return;
        }
        this.ostats = new ChangeableStats(stats, newLevel, pqMob);
        this.hp.set(ostats.getHp());
        this.mp = ostats.getMp();
    }

    private float getDifficultyRate(int difficulty)
    {
        switch (difficulty)
        {
            case 6:
                return (7.7f);
            case 5:
                return (5.6f);
            case 4:
                return (3.2f);
            case 3:
                return (2.1f);
            case 2:
                return (1.4f);
            default:
                return 1.0f;
        }
    }

    private void changeLevelByDifficulty(int difficulty, bool pqMob)
    {
        changeLevel((int)(this.getLevel() * getDifficultyRate(difficulty)), pqMob);
    }

    public void changeDifficulty(int difficulty, bool pqMob)
    {
        changeLevelByDifficulty(difficulty, pqMob);
    }

    // ---------------------------------------------------------------------------------

    private bool isPuppetInVicinity(Summon summon)
    {
        return summon.getPosition().distanceSq(this.getPosition()) < 177777;
    }

    public bool isCharacterPuppetInVicinity(IPlayer chr)
    {
        var mse = chr.getBuffEffect(BuffStat.PUPPET);
        if (mse != null)
        {
            var summon = chr.getSummonByKey(mse.getSourceId());

            // check whether mob is currently under a puppet's field of action or not
            if (summon != null)
            {
                return isPuppetInVicinity(summon);
            }
            else
            {
                MapModel.getAggroCoordinator().removePuppetAggro(chr.getId());
            }
        }

        return false;
    }

    public bool isLeadingPuppetInVicinity()
    {
        IPlayer? chrController = this.getActiveController();

        if (chrController != null)
        {
            return this.isCharacterPuppetInVicinity(chrController);
        }

        return false;
    }

    private IPlayer? getNextControllerCandidate()
    {
        int mincontrolled = int.MaxValue;
        IPlayer? newController = null;

        int mincontrolleddead = int.MaxValue;
        IPlayer? newControllerDead = null;

        IPlayer? newControllerWithPuppet = null;

        foreach (IPlayer chr in getMap().getAllPlayers())
        {
            if (!chr.isHidden())
            {
                int ctrlMonsSize = chr.getNumControlledMonsters();

                if (isCharacterPuppetInVicinity(chr))
                {
                    newControllerWithPuppet = chr;
                    break;
                }
                else if (chr.isAlive())
                {
                    if (ctrlMonsSize < mincontrolled)
                    {
                        mincontrolled = ctrlMonsSize;
                        newController = chr;
                    }
                }
                else
                {
                    if (ctrlMonsSize < mincontrolleddead)
                    {
                        mincontrolleddead = ctrlMonsSize;
                        newControllerDead = chr;
                    }
                }
            }
        }

        if (newControllerWithPuppet != null)
        {
            return newControllerWithPuppet;
        }
        else if (newController != null)
        {
            return newController;
        }
        else
        {
            return newControllerDead;
        }
    }

    /**
     * Removes controllability status from the current controller of this mob.
     */
    public KeyValuePair<IPlayer?, bool> aggroRemoveController()
    {
        IPlayer? chrController;
        bool hadAggro;

        Monitor.Enter(aggroUpdateLock);
        try
        {
            chrController = getActiveController();
            hadAggro = isControllerHasAggro();

            this.setController(null);
            this.setControllerHasAggro(false);
            this.setControllerKnowsAboutAggro(false);
        }
        finally
        {
            Monitor.Exit(aggroUpdateLock);
        }

        if (chrController != null)
        {
            // this can/should only happen when a hidden gm attacks the monster
            if (!this.isFake())
            {
                chrController.sendPacket(PacketCreator.stopControllingMonster(this.getObjectId()));
            }
            chrController.stopControllingMonster(this);
        }

        return new(chrController, hadAggro);
    }

    /**
     * Pass over the mob controllability and updates aggro status on the new
     * player controller.
     */
    public void aggroSwitchController(IPlayer? newController, bool immediateAggro)
    {
        if (Monitor.TryEnter(aggroUpdateLock))
        {
            try
            {
                var prevController = getController();
                if (prevController == newController)
                {
                    return;
                }

                aggroRemoveController();
                if (!(newController != null && newController.isLoggedinWorld() && newController.getMap() == this.getMap()))
                {
                    return;
                }

                this.setController(newController);
                this.setControllerHasAggro(immediateAggro);
                this.setControllerKnowsAboutAggro(false);
                this.setControllerHasPuppet(false);
            }
            finally
            {
                Monitor.Exit(aggroUpdateLock);
            }

            this.aggroUpdatePuppetVisibility();
            aggroMonsterControl(newController.getClient(), this, immediateAggro);
            newController.controlMonster(this);
        }
    }

    public void aggroAddPuppet(IPlayer player)
    {
        var mmac = MapModel.getAggroCoordinator();
        mmac.addPuppetAggro(player);

        aggroUpdatePuppetController(player);

        if (this.isControllerHasAggro())
        {
            this.aggroUpdatePuppetVisibility();
        }
    }

    public void aggroRemovePuppet(IPlayer player)
    {
        var mmac = MapModel.getAggroCoordinator();
        mmac.removePuppetAggro(player.getId());

        aggroUpdatePuppetController(null);

        if (this.isControllerHasAggro())
        {
            this.aggroUpdatePuppetVisibility();
        }
    }

    /**
     * Automagically finds a new controller for the given monster from the chars
     * on the map it is from...
     */
    public void aggroUpdateController()
    {
        IPlayer? chrController = this.getActiveController();
        if (chrController != null && chrController.isAlive())
        {
            return;
        }

        IPlayer? newController = getNextControllerCandidate();
        if (newController == null)
        {
            // was a new controller found? (if not no one is on the map)
            return;
        }

        this.aggroSwitchController(newController, false);
    }

    /**
     * Finds a new controller for the given monster from the chars with deployed
     * puppet nearby on the map it is from...
     */
    private void aggroUpdatePuppetController(IPlayer? newController)
    {
        var chrController = this.getActiveController();
        bool updateController = false;

        if (chrController != null && chrController.isAlive())
        {
            if (isCharacterPuppetInVicinity(chrController))
            {
                return;
            }
        }
        else
        {
            updateController = true;
        }

        if (newController == null || !isCharacterPuppetInVicinity(newController))
        {
            var mmac = MapModel.getAggroCoordinator();

            List<int> puppetOwners = mmac.getPuppetAggroList();
            List<int> toRemovePuppets = new();

            foreach (int cid in puppetOwners)
            {
                var chr = MapModel.getCharacterById(cid);

                if (chr != null)
                {
                    if (isCharacterPuppetInVicinity(chr))
                    {
                        newController = chr;
                        break;
                    }
                }
                else
                {
                    toRemovePuppets.Add(cid);
                }
            }

            foreach (int cid in toRemovePuppets)
            {
                mmac.removePuppetAggro(cid);
            }

            if (newController == null)
            {    // was a new controller found? (if not there's no puppet nearby)
                if (updateController)
                {
                    aggroUpdateController();
                }

                return;
            }
        }
        else if (chrController == newController)
        {
            this.aggroUpdatePuppetVisibility();
        }

        this.aggroSwitchController(newController, this.isControllerHasAggro());
    }

    /**
     * Ensures controllability removal of the current player controller, and
     * fetches for any player on the map to start controlling in place.
     */
    public void aggroRedirectController()
    {
        this.aggroRemoveController();   // don't care if new controller not found, at least remove current controller
        this.aggroUpdateController();
    }

    /**
     * Returns the current aggro status on the specified player, or null if the
     * specified player is currently not this mob's controller.
     */
    public bool? aggroMoveLifeUpdate(IPlayer player)
    {
        var chrController = getController();
        if (chrController != null && player.getId() == chrController.getId())
        {
            bool aggro = this.isControllerHasAggro();
            if (aggro)
            {
                this.setControllerKnowsAboutAggro(true);
            }

            return aggro;
        }
        else
        {
            return null;
        }
    }

    /**
     * Refreshes auto aggro for the player passed as parameter, does nothing if
     * there is already an active controller for this mob.
     */
    public void aggroAutoAggroUpdate(IPlayer player)
    {
        var chrController = this.getActiveController();

        if (chrController == null)
        {
            this.aggroSwitchController(player, true);
        }
        else if (chrController.getId() == player.getId())
        {
            this.setControllerHasAggro(true);
            if (!YamlConfig.config.server.USE_AUTOAGGRO_NEARBY)
            {
                // thanks Lichtmager for noticing autoaggro not updating the player properly
                aggroMonsterControl(player.getClient(), this, true);
            }
        }
    }

    /**
     * Applied damage input for this mob, enough damage taken implies an aggro
     * target update for the attacker shortly.
     */
    public void aggroMonsterDamage(IPlayer attacker, int damage)
    {
        MonsterAggroCoordinator mmac = this.getMapAggroCoordinator();
        mmac.addAggroDamage(this, attacker.getId(), damage);

        var chrController = this.getController();    // aggro based on DPS rather than first-come-first-served, now live after suggestions thanks to MedicOP, Thora, Vcoc
        if (chrController != attacker)
        {
            if (this.getMapAggroCoordinator().isLeadingCharacterAggro(this, attacker))
            {
                this.aggroSwitchController(attacker, true);
            }
            else
            {
                this.setControllerHasAggro(true);
                this.aggroUpdatePuppetVisibility();
            }

            /*
            For some reason, some mobs loses aggro on controllers if other players also attacks them.
            Maybe Nexon intended to interchange controllers at every attack...
            
            else if (chrController != null) {
                chrController.sendPacket(PacketCreator.stopControllingMonster(this.getObjectId()));
                aggroMonsterControl(chrController.getClient(), this, true);
            }
            */
        }
        else
        {
            this.setControllerHasAggro(true);
            this.aggroUpdatePuppetVisibility();
        }
    }

    private static void aggroMonsterControl(IClient c, Monster mob, bool immediateAggro)
    {
        c.sendPacket(PacketCreator.controlMonster(mob, false, immediateAggro));
    }

    private void aggroRefreshPuppetVisibility(IPlayer chrController, Summon puppet)
    {
        // lame patch for client to redirect all aggro to the puppet

        List<Monster> puppetControlled = new();
        foreach (var mob in chrController.getControlledMonsters())
        {
            if (mob.isPuppetInVicinity(puppet))
            {
                puppetControlled.Add(mob);
            }
        }

        foreach (Monster mob in puppetControlled)
        {
            chrController.sendPacket(PacketCreator.stopControllingMonster(mob.getObjectId()));
        }
        chrController.sendPacket(PacketCreator.removeSummon(puppet, false));

        var c = chrController.getClient();
        foreach (Monster mob in puppetControlled)
        {
            // thanks BHB for noticing puppets disrupting mobstatuses for bowmans
            aggroMonsterControl(c, mob, mob.isControllerKnowsAboutAggro());
        }
        chrController.sendPacket(PacketCreator.spawnSummon(puppet, false));
    }

    public void aggroUpdatePuppetVisibility()
    {
        if (!availablePuppetUpdate)
        {
            return;
        }

        availablePuppetUpdate = false;
        Action r = () =>
        {
            try
            {
                var chrController = this.getActiveController();
                if (chrController == null)
                {
                    return;
                }

                var puppetEffect = chrController.getBuffEffect(BuffStat.PUPPET);
                if (puppetEffect != null)
                {
                    var puppet = chrController.getSummonByKey(puppetEffect.getSourceId());

                    if (puppet != null && isPuppetInVicinity(puppet))
                    {
                        controllerHasPuppet = true;
                        aggroRefreshPuppetVisibility(chrController, puppet);
                        return;
                    }
                }

                if (controllerHasPuppet)
                {
                    controllerHasPuppet = false;

                    chrController.sendPacket(PacketCreator.stopControllingMonster(this.getObjectId()));
                    aggroMonsterControl(chrController.getClient(), this, this.isControllerHasAggro());
                }
            }
            finally
            {
                availablePuppetUpdate = true;
            }
        };

        // had to schedule this since mob wouldn't stick to puppet aggro who knows why
        OverallService service = (OverallService)this.getMap().getChannelServer().getServiceAccess(ChannelServices.OVERALL);
        service.registerOverallAction(this.getMap().getId(), r, YamlConfig.config.server.UPDATE_INTERVAL);
    }

    /**
     * Clears all applied damage input for this mob, doesn't refresh target
     * aggro.
     */
    public void aggroClearDamages()
    {
        this.getMapAggroCoordinator().removeAggroEntries(this);
    }

    /**
     * Clears this mob aggro on the current controller.
     */
    public void aggroResetAggro()
    {
        Monitor.Enter(aggroUpdateLock);
        try
        {
            this.setControllerHasAggro(false);
            this.setControllerKnowsAboutAggro(false);
        }
        finally
        {
            Monitor.Exit(aggroUpdateLock);
        }
    }

    public int getRemoveAfter()
    {
        return stats.removeAfter();
    }

    public void dispose()
    {
        if (monsterItemDrop != null)
        {
            monsterItemDrop.cancel(false);
        }

        this.getMap().dismissRemoveAfter(this);
    }
}
