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


using Application.Core.Channel.Commands;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Events;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Players;
using Application.Core.Game.Skills;
using Application.Resources.Messages;
using Application.Shared.Objects;
using Application.Shared.WzEntity;
using client.status;
using Google.Protobuf.Collections;
using Jint.Native.ShadowRealm;
using net.server.coordinator.world;
using net.server.services.task.channel;
using server.life;
using server.loot;
using System.Threading;
using tools;
using ZLinq;
using static Application.Core.Channel.Internal.Handlers.PlayerFieldHandlers;
using static Application.Templates.Mob.MobTemplate;

namespace Application.Core.Game.Life;

public class Monster : AbstractLifeObject, ICombatantObject
{
    private ILogger log;

    private ChangeableStats? ostats = null;  //unused, v83 WZs offers no support for changeable stats.
    private MonsterStats stats;
    private AtomicInteger hp = new AtomicInteger(1);
    private AtomicLong maxHpPlusHeal = new AtomicLong(1);
    private int mp;
    private WeakReference<Player?> controller = new(null);
    private bool controllerHasAggro, controllerKnowsAboutAggro, controllerHasPuppet;

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
    /// <summary>
    /// 玩家对其造成的伤害记录
    /// </summary>
    private Dictionary<int, AtomicLong> takenDamage = new();
    private ScheduledFuture? monsterItemDrop = null;
    private IWorldChannelCommand? removeAfterAction = null;
    private bool availablePuppetUpdate = true;
    /// <summary>
    /// 有值时，不走默认的drop_data
    /// </summary>
    public List<DropEntry>? CustomeDrops { get; set; }

    /// <summary>
    /// 地图上生成
    /// </summary>
    public event EventHandler? OnSpawned;
    /// <summary>
    /// 被击杀。死亡动画延迟
    /// </summary>
    public event EventHandler<MonsterKilledEventArgs>? OnKilled;
    public event EventHandler<MonsterDamagedEventArgs>? OnDamaged;
    public event EventHandler<int>? OnHealed;
    /// <summary>
    /// 被击杀后，衍生物生成时触发。生成的衍生物
    /// </summary>
    public event EventHandler<MonsterReviveEventArgs>? OnRevive;
    /// <summary>
    /// 被击杀时无衍生物，或者所有衍生物被击杀后触发
    /// </summary>
    public event EventHandler? OnLifeCleared;
    /// <summary>
    /// X死亡时生成
    /// </summary>
    public Monster? RevivedFrom { get; set; }
    /// <summary>
    /// 死亡时生成
    /// </summary>
    public List<Monster> RevivingMonsters => _revivingMonsters.Value;
    Lazy<List<Monster>> _revivingMonsters;
    public Dictionary<int, MobAttackTemplate> AttackInfoHolders { get; }

    public Monster(int id, MonsterStats stats, MobAttackTemplate[] attackInfo) : base(id)
    {
        setStance(5);
        this.stats = stats.copy();
        hp.set(stats.getHp());
        mp = stats.getMp();

        maxHpPlusHeal.set(hp.get());

        log = LogFactory.GetLogger(LogType.Monster);

        _revivingMonsters = new Lazy<List<Monster>>(() => stats.getRevives().Select(x => LifeFactory.Instance.getMonster(x)).Where(x => x != null).Select(x => x!).ToList());
        AttackInfoHolders = attackInfo.ToDictionary(x => x.Index);
    }

    public override void setMap(IMap map)
    {
        base.setMap(map);
        DispatchMonsterSpawned();
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

    public void pushRemoveAfterAction(IWorldChannelCommand run)
    {
        this.removeAfterAction = run;
    }

    public IWorldChannelCommand? popRemoveAfterAction()
    {
        var r = this.removeAfterAction;
        this.removeAfterAction = null;

        return r;
    }

    public int getHp()
    {
        return hp.get();
    }

    public void addHp(int hp)
    {
        if (this.hp.get() <= 0)
        {
            return;
        }
        this.hp.addAndGet(hp);
    }

    public void setStartingHp(int hp)
    {
        stats.setHp(hp);    // refactored mob stats after non-static HP pool suggestion thanks to twigs
        this.hp.set(hp);
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

    //private bool applyAnimationIfRoaming(int attackPos, MobSkill skill)
    //{   // roam: not casting attack or skill animations
    //    if (!Monitor.TryEnter(animationLock))
    //    {
    //        return false;
    //    }

    //    try
    //    {
    //        long animationTime;

    //        if (skill == null)
    //        {
    //            animationTime = MonsterInformationProvider.getInstance().getMobAttackAnimationTime(this.getId(), attackPos);
    //        }
    //        else
    //        {
    //            animationTime = MonsterInformationProvider.getInstance().getMobSkillAnimationTime(skill);
    //        }

    //        if (animationTime > 0)
    //        {
    //            MobAnimationService service = MapModel.getChannelServer().MobAnimationService;
    //            return service.registerMobOnAnimationEffect(MapModel.getId(), this.GetHashCode(), animationTime);
    //        }
    //        else
    //        {
    //            return true;
    //        }
    //    }
    //    finally
    //    {
    //        Monitor.Exit(animationLock);
    //    }
    //}

    int? applyAndGetHpDamage(int delta, bool stayAlive)
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

    public void disposeMapObject()
    {     // mob is no longer associated with the map it was in

        hp.set(-1);
    }

    public void broadcastMobHpBar(Player from)
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

            var team = from.getParty();
            if (team != null)
            {
                foreach (var mpc in team.GetTeamMembers())
                {
                    var member = from.getMap().getCharacterById(mpc.Id); // god bless
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

    public bool DamageBy(ICombatantObject attacker, int damageValue, short delay, bool stayAlive = false)
    {
        if (!this.isAlive())
        {
            return false;
        }

        if (isFake())
        {
            return false;
        }

        if (damageValue > 0)
        {
            this.applyDamage(attacker, damageValue, stayAlive, false);

            var selfDestr = getStats().selfDestruction();
            if (selfDestr != null && selfDestr.Hp > -1)
            {
                // should work ;p
                if (getHp() <= selfDestr.Hp)
                {
                    MapModel.RemoveMob(this, attacker, true, selfDestr.Action, delay);
                    return true;
                }
            }

            if (!this.isAlive())
            {
                MapModel.RemoveMob(this, attacker, true, delay);
            }
            return true;
        }
        return false;
    }

    /**
     * @param from      the player that dealt the damage
     * @param damage
     * @param stayAlive
     */
    void applyDamage(ICombatantObject from, int damage, bool stayAlive, bool fake)
    {
        var trueDamage = applyAndGetHpDamage(damage, stayAlive);
        if (trueDamage == null)
        {
            return;
        }

        if (!fake)
        {
            OnDamaged?.Invoke(this, new MonsterDamagedEventArgs(from, trueDamage.Value));
        }

        if (getStats().isFriendly())
        {
            MapModel.EventInstanceManager?.friendlyDamaged(this);
        }

        if (from is Player chr)
        {
            if (takenDamage.TryGetValue(chr.Id, out var d))
            {
                d.addAndGet(trueDamage.Value);
            }
            else
            {
                takenDamage.Add(chr.Id, new AtomicLong(trueDamage.Value));
            }


            if (YamlConfig.config.server.USE_DEBUG)
                chr.dropMessage(5, "Hitted MOB " + this.getId() + ", OID " + this.getObjectId());

            broadcastMobHpBar(chr);
        }
    }

    public void applyFakeDamage(ICombatantObject from, int damage, bool stayAlive)
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

        OnHealed?.Invoke(this, hpHealed.Value);
    }

    public bool isAttackedBy(Player chr)
    {
        return takenDamage.ContainsKey(chr.Id);
    }

    private static bool isWhiteExpGain(Player chr, Dictionary<int, float> personalRatio, double sdevRatio)
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

    private void distributePlayerExperience(Player chr, float exp, float partyBonusMod, int totalPartyLevel, bool highestPartyDamager, bool whiteExpGain, bool hasPartySharers)
    {
        float playerExp = (YamlConfig.config.server.EXP_SPLIT_COMMON_MOD * chr.getLevel()) / totalPartyLevel;
        if (highestPartyDamager)
        {
            playerExp += YamlConfig.config.server.EXP_SPLIT_MVP_MOD;
        }

        playerExp *= exp;
        float bonusExp = partyBonusMod * playerExp;

        this.giveExpToCharacter(chr, playerExp, bonusExp, whiteExpGain, hasPartySharers);
        foreach (var module in MapModel.ChannelServer.NodeService.Modules)
        {
            module.OnMonsterReward(new MonsterRewardEvent(chr, this));
        }
    }

    private void distributePartyExperience(Dictionary<Player, long> partyParticipation, float expPerDmg, HashSet<Player> underleveled, Dictionary<int, float> personalRatio, double sdevRatio)
    {
        IntervalBuilder leechInterval = new IntervalBuilder();
        leechInterval.addInterval(this.getLevel() - YamlConfig.config.server.EXP_SPLIT_LEVEL_INTERVAL, this.getLevel() + YamlConfig.config.server.EXP_SPLIT_LEVEL_INTERVAL);

        long maxDamage = 0, partyDamage = 0;
        Player? participationMvp = null;
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

        List<Player> expMembers = new();
        int totalPartyLevel = 0;

        // thanks G h o s t, Alfred, Vcoc, BHB for poiting out a bug in detecting party members after membership transactions in a party took place
        if (YamlConfig.config.server.USE_ENFORCE_MOB_LEVEL_RANGE)
        {
            foreach (Player member in partyParticipation.Keys.First().getPartyMembersOnSameMap())
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
            foreach (Player member in partyParticipation.Keys.First().getPartyMembersOnSameMap())
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

        foreach (Player mc in expMembers)
        {
            distributePlayerExperience(mc, participationExp, partyBonusMod, totalPartyLevel, mc == participationMvp, isWhiteExpGain(mc, personalRatio, sdevRatio), hasPartySharers);
            foreach (var module in MapModel.ChannelServer.NodeService.Modules)
            {
                module.OnMonsterReward(new MonsterRewardEvent(mc, this));
            }
        }
    }

    private void distributeExperience(int killerId)
    {
        if (isAlive())
        {
            return;
        }

        Dictionary<int, Dictionary<Player, long>> partyExpDist = new();
        Dictionary<Player, long> soloExpDist = new();

        Dictionary<int, Player> mapPlayers = MapModel.getMapPlayers();

        int totalEntries = 0;   // counts "participant parties", players who no longer are available in the map is an "independent party"
        foreach (var e in takenDamage)
        {
            var chr = mapPlayers.GetValueOrDefault(e.Key);
            if (chr != null)
            {
                long damage = e.Value;

                var p = chr.getPartyId();
                if (p > 0)
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

        foreach (Dictionary<Player, long> m in partyExpDist.Values)
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
        HashSet<Player> underleveled = new();
        foreach (var chrParticipation in soloExpDist)
        {
            float exp = chrParticipation.Value * expPerDmg;
            Player chr = chrParticipation.Key;

            distributePlayerExperience(chr, exp, 0.0f, chr.getLevel(), true, isWhiteExpGain(chr, personalRatio, sdevRatio), false);
        }

        foreach (Dictionary<Player, long> partyParticipation in partyExpDist.Values)
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

        foreach (Player mc in underleveled)
        {
            mc.showUnderleveledInfo(this);
        }

    }

    private float getStatusExpMultiplier(Player attacker, bool hasPartySharers)
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

        var mse = stati.GetValueOrDefault(MonsterStatus.SHOWDOWN);
        if (mse != null)
        {
            multiplier *= (1.0f + (mse.getStati().GetValueOrDefault(MonsterStatus.SHOWDOWN, 0) / 100.0f));
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

    private void giveExpToCharacter(Player attacker, float? personalExp, float? partyExp, bool white, bool hasPartySharers)
    {
        if (attacker.isAlive())
        {
            var expFromHolySymbol = getStatusExpMultiplier(attacker, hasPartySharers);
            if (personalExp != null)
            {
                personalExp *= expFromHolySymbol;
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
                partyExp *= expFromHolySymbol;
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
    /// 只生成造成伤害者可获取的掉落物（看起来同一队伍其他玩家如果没有造成伤害，也不会掉落他们的任务物品）
    /// </summary>
    /// <returns></returns>
    public List<DropEntry> retrieveRelevantDrops()
    {
        if (this.getStats().isFriendly())
        {
            // thanks Conrad for noticing friendly mobs not spawning loots after a recent update
            return MonsterInformationProvider.getInstance().retrieveEffectiveDrop(this.getId());
        }

        Dictionary<int, Player> pchars = MapModel.getMapPlayers();

        List<Player> lootChars = new();
        foreach (int cid in takenDamage.Keys)
        {
            var chr = pchars.GetValueOrDefault(cid);
            if (chr != null && chr.isLoggedinWorld())
            {
                lootChars.Add(chr);
            }
        }

        return MonsterInformationProvider.getInstance().retrieveEffectiveDrop(this.getId())
            .AsValueEnumerable()
            .Where(x => x.QuestId <= 0 || lootChars.Any(chr => chr.needQuestItem(x.QuestId, x.ItemId))).ToList();
    }

    /// <summary>
    /// 分发击杀奖励（经验）
    /// </summary>
    /// <param name="killer"></param>
    /// <returns></returns>
    public Player? killBy(Player? killer)
    {
        distributeExperience(killer != null ? killer.getId() : 0);

        // TODO: 文本大致意思是显示，不太可能用于被击杀时触发
        //var timeMob = reviveMap.TimeMob;
        //if (timeMob != null)
        //{
        //    if (RevivingMonsters.Any(x => x.getId() == timeMob.MobId))
        //    {
        //        reviveMap.dropMessage(6, timeMob.Message);
        //    }
        //}
        return MapModel.getCharacterById(getHighestDamagerId()) ?? killer;
    }

    void dropFromFriendlyMonster(long delay)
    {
        Monster m = this;
        monsterItemDrop = MapModel.ChannelServer.Node.TimerManager.register(() =>
        {
            MapModel.ChannelServer.Post(new MonsterFriendlyDropCommand(m));
        }, delay, delay);
    }

    public void FriendlyDrop()
    {
        if (!isAlive())
        {
            if (monsterItemDrop != null)
            {
                monsterItemDrop.cancel(false);
            }

            return;
        }

        var map = getMap();
        List<Player> chrList = map.getAllPlayers();
        if (chrList.Count > 0)
        {
            Player chr = chrList.get(0);

            var eim = map.getEventInstance();
            if (eim != null)
            {
                eim.friendlyItemDrop(this);
            }

            map.dropFromFriendlyMonster(chr, this);
        }
    }

    private void dispatchRaiseQuestMobCount()
    {
        var attackerChrids = takenDamage.Keys;
        if (attackerChrids.Count > 0)
        {
            Dictionary<int, Player> mapChars = MapModel.getMapPlayers();
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

    public void dispatchMonsterKilled(ICombatantObject? killer)
    {
        processMonsterKilled(killer);

        var lastController = aggroRemoveController();

        if (RevivingMonsters.Count > 0)
        {
            if (RevivingMonsters.Any(x => x.getId() == (MobId.TRANSPARENT_ITEM)) && getId() > 925000000 && getId() < 926000000)
            {
                MapModel.broadcastMessage(PacketCreator.playSound("Dojang/clear"));
                MapModel.broadcastMessage(PacketCreator.showEffect("dojang/end/clear"));
            }

            MapModel.ChannelServer.Node.TimerManager.schedule(() =>
            {
                MapModel.ChannelServer.Post(new MonsterReviveCommand(this, killer, lastController));
            }, getAnimationTime("die1"));
        }
        else
        {
            DispatchMonsterAllKilled();
        }

        if (RevivedFrom != null && RevivedFrom.RevivingMonsters.All(x => !x.isAlive()))
        {
            RevivedFrom.DispatchMonsterAllKilled();
        }

        var eim = getMap().getEventInstance();
        if (eim != null)
        {
            if (!this.getStats().isFriendly())
            {
                eim.monsterKilled(this, killer != null);
            }
            else
            {
                eim.friendlyKilled(this, killer != null);
            }
        }
        getMap().dismissRemoveAfter(this);
    }

    private void processMonsterKilled(ICombatantObject? killer)
    {
        if (killer == null)
        {
            // players won't gain EXP from a mob that has no killer, but a quest count they should
            dispatchRaiseQuestMobCount();
        }

        this.aggroClearDamages();
        this.dispatchClearSummons();

        var dieAni = getAnimationTime("die1");
        OnKilled?.Invoke(this, new MonsterKilledEventArgs(killer, dieAni));

        if (getStats().isFriendly())
        {
            switch (getId())
            {
                case MobId.WATCH_HOG:
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_WatchHog), e.GetMobName(getId())));
                    break;
                case MobId.MOON_BUNNY: //moon bunny
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_MoonBunny), e.GetMobName(getId())));
                    break;
                case MobId.TYLUS: //tylus
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Tylus), e.GetMobName(getId())));
                    break;
                case MobId.JULIET: //juliet
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Juliet), e.GetMobName(getId())));
                    break;
                case MobId.ROMEO: //romeo
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Romeo), e.GetMobName(getId())));
                    break;
                case MobId.GIANT_SNOWMAN_LV1_EASY:
                case MobId.GIANT_SNOWMAN_LV1_MEDIUM:
                case MobId.GIANT_SNOWMAN_LV1_HARD:
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Snownman)));
                    break;
                case MobId.DELLI: //delli
                    MapModel.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Delli), e.GetMobName(getId())));
                    break;
            }
        }

        if (MobId.isZakumArm(getId()))
        {
            var objects = MapModel.GetRequiredMapObjects<Monster>(MapObjectType.MONSTER, x => x.getParentMobOid() == getParentMobOid());
            if (objects.Count == 0)
            {
                var mainMob = MapModel.getMonsterByOid(getParentMobOid());
                if (mainMob != null)
                {
                    MapModel.makeMonsterReal(mainMob);
                }
            }
        }


        stati.Clear();
        alreadyBuffed.Clear();
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


    public Player? getController()
    {
        return controller.TryGetTarget(out var d) ? d : null;
    }

    private void setController(Player? controller)
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

    public override void sendSpawnData(IChannelClient client)
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

    public override void sendDestroyData(IChannelClient client)
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
        if (stati.GetValueOrDefault(MonsterStatus.DOOM) != null)
        {
            return ElementalEffectiveness.NORMAL; // like blue snails
        }
        return getMonsterEffectiveness(e);
    }

    private ElementalEffectiveness getMonsterEffectiveness(Element e)
    {
        return stats.getEffectiveness(e);
    }

    private Player? getActiveController()
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

    public void broadcastMonsterStatusMessage(Packet packet)
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

    public bool applyStatus(Player from, MonsterStatusEffect status, bool poison, long duration, bool venom = false)
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
            foreach (MonsterStatus stat in statis.Keys)
            {
                var oldEffect = stati.GetValueOrDefault(stat);
                if (oldEffect != null)
                {
                    oldEffect.removeActiveStatus(stat);
                    if (oldEffect.getStati().Count == 0)
                    {
                        MobStatusService serviced = MapModel.getChannelServer().MobStatusService;
                        serviced.interruptMobStatus(mapid, oldEffect);
                    }
                }
            }
        }

        Action cancelTask = () =>
        {
            MapModel.ChannelServer.Post(new MonsterStatusRemoveCommand(this, status));
        };
        IWorldChannelCommand? overtimeAction = null;
        int overtimeDelay = -1;

        int animationTime;
        if (poison)
        {
            int poisonLevel = from.getSkillLevel(status.getSkill());
            int poisonDamage = Math.Min(short.MaxValue, (int)(getMaxHp() / (70.0 - poisonLevel) + 0.999));
            status.setValue(MonsterStatus.POISON, poisonDamage);
            animationTime = broadcastStatusEffect(status);

            overtimeAction = new MonsterApplyDamageCommand(this, from, status, poisonDamage, 0);
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

                overtimeAction = new MonsterApplyDamageCommand(this, from, status, poisonDamage, 0);
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
            int damage = (int)((from.getStr() + from.getLuk()) * ((3.7 * skill.getEffect(level).getDamage()) / 100.0));

            status.setValue(MonsterStatus.NINJA_AMBUSH, damage);
            animationTime = broadcastStatusEffect(status);

            overtimeAction = new MonsterApplyDamageCommand(this, from, status, damage, 2);
            overtimeDelay = 1000;
        }
        else
        {
            animationTime = broadcastStatusEffect(status);
        }

        foreach (MonsterStatus stat in status.getStati().Keys)
        {
            stati.AddOrUpdate(stat, status);
            alreadyBuffed.Add(stat);
        }

        MobStatusService service = MapModel.getChannelServer().MobStatusService;
        service.registerMobStatus(mapid, status, new MonsterStatusRemoveCommand(this, status), duration + animationTime - 100, overtimeAction, overtimeDelay);
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
        MonsterStatusEffect effect = new MonsterStatusEffect(stats, skill);
        Packet packet = PacketCreator.applyMonsterStatus(getObjectId(), effect, reflection);
        broadcastMonsterStatusMessage(packet);

        foreach (MonsterStatus stat in stats.Keys)
        {
            stati.AddOrUpdate(stat, effect);
            alreadyBuffed.Add(stat);
        }

        MobStatusService service = MapModel.getChannelServer().MobStatusService;
        service.registerMobStatus(MapModel.getId(), effect, new MonsterBuffRemoveCommand(this, stats), duration);
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
            PacketCreator.MoveMonsterIdle(this.getObjectId(), false, -1, 0, 0, 0, this.getPosition(), this.GetIdleMovementBytes()));
        MapModel.moveMonster(this, this.getPosition());

        aggroUpdateController();
    }

    private void debuffMobStat(MonsterStatus stat)
    {
        MonsterStatusEffect? oldEffect;

        stati.Remove(stat, out oldEffect);

        if (oldEffect != null)
        {
            Packet packet = PacketCreator.cancelMonsterStatus(getObjectId(), oldEffect.getStati());
            broadcastMonsterStatusMessage(packet);
        }
    }

    public void debuffMob(int skillid)
    {
        MonsterStatus[] statups = { MonsterStatus.WEAPON_ATTACK_UP, MonsterStatus.WEAPON_DEFENSE_UP, MonsterStatus.MAGIC_ATTACK_UP, MonsterStatus.MAGIC_DEFENSE_UP };

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

    public bool isBuffed(MonsterStatus status)
    {
        return stati.ContainsKey(status);
    }

    public void setFake(bool fake)
    {
        this.fake = fake;
    }

    public bool isFake()
    {
        return fake;
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

        mp -= skill.getMpCon();

        this.usedSkills.Add(msId);

        Monster mons = this;
        var mmap = mons.getMap();

        MobClearSkillService service = MapModel.getChannelServer().MobClearSkillService;
        service.registerMobClearSkillAction(mmap.getId(), new MonsterClearSkillCommand(this, skill), skill.getCoolTime());
    }

    public void clearSkill(MobSkillId msId)
    {
        usedSkills.Remove(msId);
    }

    public int canUseAttack(int attackPos, bool isSkill)
    {

        /*
        if (usedAttacks.Contains(attackPos)) {
            return -1;
        }
        */

        var attackInfo = AttackInfoHolders.GetValueOrDefault(attackPos);
        if (attackInfo == null)
        {
            return -1;
        }

        if (mp < attackInfo.ConMP)
        {
            return -1;
        }

        /*
        if (!this.applyAnimationIfRoaming(attackPos, null)) {
            return -1;
        }
        */

        usedAttack(attackPos, attackInfo.ConMP, attackInfo.AttackAfter);
        return 1;
    }

    private void usedAttack(int attackPos, int mpCon, int cooltime)
    {
        mp -= mpCon;
        usedAttacks.Add(attackPos);

        Monster mons = this;
        var mmap = mons.getMap();

        MobClearSkillService service = MapModel.getChannelServer().MobClearSkillService;
        service.registerMobClearSkillAction(mmap.getId(), new MonsterClearAttackCommand(this, attackPos), cooltime);
    }

    public void clearAttack(int attackPos)
    {
        usedAttacks.Remove(attackPos);
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
        private Player chr;
        private MonsterStatusEffect status;
        private int type;
        private IMap map;
        readonly Monster _monster;

        public DamageTask(Monster mapleMonster, int dealDamage, Player chr, MonsterStatusEffect status, int type)
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
            chr.Client.CurrentServer.Post(new MonsterApplyDamageCommand(_monster, chr, status, dealDamage, type));
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
        Element fE = e;
        ElementalEffectiveness fEE = stats.getEffectiveness(e);
        if (!fEE.Equals(ElementalEffectiveness.WEAK))
        {
            stats.setEffectiveness(e, ee);

            var mmap = this.getMap();
            MobClearSkillService service = mmap.getChannelServer().MobClearSkillService;
            service.registerMobClearSkillAction(mmap.getId(), new MonsterClearEffectCommand(this, e), milli);
        }
    }

    public ICollection<MonsterStatus> alreadyBuffedStats()
    {
        return new List<MonsterStatus>(alreadyBuffed);
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
        return new(stati);
    }

    public MonsterStatusEffect? getStati(MonsterStatus ms)
    {
        return stati.GetValueOrDefault(ms);
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

    public bool isCharacterPuppetInVicinity(Player chr)
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
        Player? chrController = this.getActiveController();

        if (chrController != null)
        {
            return this.isCharacterPuppetInVicinity(chrController);
        }

        return false;
    }

    private Player? getNextControllerCandidate()
    {
        int mincontrolled = int.MaxValue;
        Player? newController = null;

        int mincontrolleddead = int.MaxValue;
        Player? newControllerDead = null;

        Player? newControllerWithPuppet = null;

        foreach (Player chr in getMap().getAllPlayers())
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
    public MonsterControllerPair aggroRemoveController()
    {
        Player? chrController;
        bool hadAggro;

        chrController = getActiveController();
        hadAggro = isControllerHasAggro();

        this.setController(null);
        this.setControllerHasAggro(false);
        this.setControllerKnowsAboutAggro(false);

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
    public void aggroSwitchController(Player? newController, bool immediateAggro)
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

        this.aggroUpdatePuppetVisibility();
        aggroMonsterControl(newController.getClient(), this, immediateAggro);
        newController.controlMonster(this);

    }

    public void aggroAddPuppet(Player player)
    {
        var mmac = MapModel.getAggroCoordinator();
        mmac.addPuppetAggro(player);

        aggroUpdatePuppetController(player);

        if (this.isControllerHasAggro())
        {
            this.aggroUpdatePuppetVisibility();
        }
    }

    public void aggroRemovePuppet(Player player)
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
        Player? chrController = this.getActiveController();
        if (chrController != null && chrController.isAlive())
        {
            return;
        }

        Player? newController = getNextControllerCandidate();
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
    private void aggroUpdatePuppetController(Player? newController)
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
    public bool? aggroMoveLifeUpdate(Player player)
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
    public void aggroAutoAggroUpdate(Player player)
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
    public void aggroMonsterDamage(Player attacker, int damage)
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

    private static void aggroMonsterControl(IChannelClient c, Monster mob, bool immediateAggro)
    {
        c.sendPacket(PacketCreator.controlMonster(mob, false, immediateAggro));
    }

    private void aggroRefreshPuppetVisibility(Player chrController, Summon puppet)
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
        // had to schedule this since mob wouldn't stick to puppet aggro who knows why
        OverallService service = this.getMap().getChannelServer().OverallService;
        service.registerOverallAction(this.getMap().getId(), new MonsterPuppetAggroCommand(this), YamlConfig.config.server.UPDATE_INTERVAL);
    }

    public void ApplyPuppetAggro()
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
        this.setControllerHasAggro(false);
        this.setControllerKnowsAboutAggro(false);
    }

    public int getRemoveAfter()
    {
        return stats.removeAfter();
    }

    public void SetCustomeDrop(List<DropItemEntry> data)
    {
        CustomeDrops = data.Select(x => DropEntry.MobDrop(this.getId(), x.ItemId, x.Chance, x.MinCount, x.MaxCount, 0)).ToList();
    }
    public List<DropEntry> GetDropEntryList()
    {
        if (CustomeDrops == null)
            return YamlConfig.config.server.USE_SPAWN_RELEVANT_LOOT ? retrieveRelevantDrops() : MonsterInformationProvider.getInstance().retrieveEffectiveDrop(this.getId());
        return CustomeDrops;
    }

    public void dispose()
    {
        if (monsterItemDrop != null)
        {
            monsterItemDrop.cancel(false);
        }

        this.getMap().dismissRemoveAfter(this);
    }

    public override string GetName()
    {
        return getName();
    }

    public override string GetReadableName(IChannelClient c)
    {
        return c.CurrentCulture.GetMobName(getId());
    }

    public override int GetSourceId()
    {
        return getId();
    }

    void DispatchMonsterSpawned()
    {
        OnSpawned?.Invoke(this, EventArgs.Empty);

        var dropPeriodTime = getDropPeriodTime();
        if (dropPeriodTime > 0)
        {
            if (getId() == MobId.MOON_BUNNY)
            {
                dropPeriodTime = dropPeriodTime / 3;
            }

            dropFromFriendlyMonster(dropPeriodTime);
        }

        if (getId() == MobId.SUMMON_HORNTAIL)
        {
            var soul = RevivingMonsters.FirstOrDefault(x => x.getId() == MobId.HORNTAIL)!;
            soul.OnDamaged += (sender, args) =>
            {
                soul.addHp(args.Damage);
            };
            soul.OnHealed += (sender, args) =>
            {
                soul.addHp(-args);
            };

            List<Monster> deathBody = [];
            var others = RevivingMonsters.Where(x => x.getId() != MobId.HORNTAIL).ToList();
            foreach (var m in others)
            {
                m.OnDamaged += (sender, args) =>
                {
                    soul.applyFakeDamage(args.Attacker, args.Damage, true);
                };

                m.OnHealed += (sender, args) =>
                {
                    soul.addHp(args);
                };

                m.OnRevive += (sender, args) =>
                {
                    deathBody.Add(args.NextMob);

                    if (deathBody.Count == others.Count)
                    {
                        soul.setHpZero();
                        MapModel.RemoveMob(soul, args.Killer, true);

                        foreach (var item in deathBody)
                        {
                            MapModel.RemoveMob(item, null, false);
                        }
                    }
                };
            }
        }
    }

    void DispatchMonsterAllKilled()
    {
        OnLifeCleared?.Invoke(this, EventArgs.Empty);
    }

    public void Revive(ICombatantObject? killer, MonsterControllerPair lastController)
    {
        var curMob = this;

        var reviveMap = MapModel;
        var eim = reviveMap.getEventInstance();

        var controller = lastController.Controller;
        bool aggro = lastController.HasAggro;

        foreach (var mob in RevivingMonsters)
        {
            mob.setPosition(curMob.getPosition());
            mob.setFh(curMob.getFh());
            mob.setParentMobOid(curMob.getObjectId());

            if (dropsDisabled())
            {
                mob.disableDrops();
            }
            reviveMap.spawnMonster(mob);
            OnRevive?.Invoke(curMob, new MonsterReviveEventArgs(mob, killer));
            mob.RevivedFrom = curMob;

            if (controller != null)
            {
                mob.aggroSwitchController(controller, aggro);
            }

            if (eim != null)
            {
                eim.reviveMonster(mob);
            }
        }
    }
}
