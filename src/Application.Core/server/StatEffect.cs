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


using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Maps.Mists;
using Application.Core.Game.Skills;
using Application.Core.tools.RandomUtils;
using Application.Shared.Constants.Skill;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Skill;
using Application.Templates.StatEffectProps;
using client.inventory;
using client.inventory.manipulator;
using client.status;
using server.life;
using server.maps;
using server.partyquest;
using tools;

namespace server;

/**
 * @author Matze
 * @author Frz
 * @author Ronan
 */
public class StatEffect
{
    private short watk, matk, wdef, mdef, acc, avoid, speed, jump;
    private short hp, mp;
    private double hpR, mpR;
    private short mhpRRate, mmpRRate;
    private sbyte mhpR, mmpR;
    private short mpCon, hpCon;
    private int duration = -1;
    private bool overTime;
    private int sourceid;
    private List<Disease> cureDebuffs;
    private bool skill;
    private List<BuffStatValue> statups;
    private Dictionary<MonsterStatus, int> monsterStatus;
    private int x, y, mobCount = 1, moneyCon, cooldown;
    private int activeMorphId = 0;
    private int prop = 100;
    private int itemCon, itemConNo;
    private int damage = 100, attackCount = 1, fixdamage = -1;
    private Point? lt, rb;
    private short bulletCount = 1, bulletConsume;

    public int SkillLevel { get; set; }

    /// <summary>
    /// 对技能而言，来自技能的各等级属性
    /// </summary>
    public IStatEffectProp EffectTemplate { get; }
    /// <summary>
    /// 对技能而言，来自技能
    /// </summary>
    public IStatEffectSource SourceTemplate { get; }


    public List<ScopedEffect> ScopedEffects { get; } = [];
    public int Prob { get; private set; }
    public char? DefenseAtt { get; private set; }
    public Disease? DefenseState { get; private set; }


    public bool isActive(Player applyto)
    {
        return ScopedEffects.Count == 0 || ScopedEffects.Any(x => x.IsActive(applyto.getMapId(), applyto.Party > 0));
    }

    public int getCardRate(Player chr, int itemid)
    {
        //if (!isActive(chr))
        //{
        //    return 0;
        //}
        if (itemid == 0 && EffectTemplate is IItemStatEffectMesoUp mesoUp)
        {
            return mesoUp.Prob;
        }
        if (itemid > 0 && EffectTemplate is IItemStatEffectItemUp itemUp)
        {
            if (itemUp.ItemUp == 1)
            {
                return itemUp.Prob;
            }

            else if (itemUp.ItemUp == 2 && itemid == itemUp.ItemCode)
            {
                return itemUp.Prob;
            }

            else if (itemUp.ItemUp == 3 && itemid / 10000 == itemUp.ItemRange)
            {
                return itemUp.Prob;
            }
        }

        return 0;
    }


    private static void addBuffStatPairToListIfNotZero(List<BuffStatValue> list, BuffStat buffstat, int val)
    {
        if (val != 0)
        {
            list.Add(new(buffstat, val));
        }
    }

    private static byte GetMapProtection(int sourceid)
    {
        if (sourceid == ItemId.RED_BEAN_PORRIDGE || sourceid == ItemId.SOFT_WHITE_BUN)
        {
            return 1;   //elnath cold
        }
        else if (sourceid == ItemId.AIR_BUBBLE || sourceid == 2022187)
        {
            return 2;   //aqua road underwater
        }
        else
        {
            return 0;
        }
    }
    public StatEffect(IStatEffectProp template, IStatEffectSource sourceTemplate, bool isBuff)
    {
        EffectTemplate = template;
        SourceTemplate = sourceTemplate;

        sourceid = sourceTemplate.SourceId;

        bool isSummonProp = false;
        if (sourceTemplate is SkillTemplate skillSource)
        {
            skill = true;
            duration = template.Time > 0 ? template.Time * 1000 : -1;
            overTime = isBuff;

            isSummonProp = skillSource.HasSummonNode;
        }
        else
        {
            duration = template.Time > 0 ? template.Time : -1;
            overTime = duration > -1;
        }

        if (template is IStatEffectHeal heal)
        {
            hp = (short)heal.HP;
            hpR = heal.HPR / 100.0;
            mp = (short)heal.MP;
            mpR = heal.MPR / 100.0;
        }

        cureDebuffs = new();
        if (template is IStatEffectCure cure)
        {
            if (cure.Cure_Poison)
                cureDebuffs.Add(Disease.POISON);
            if (cure.Cure_Seal)
                cureDebuffs.Add(Disease.SEAL);
            if (cure.Cure_Darkness)
                cureDebuffs.Add(Disease.DARKNESS);
            if (cure.Cure_Weakness)
            {
                cureDebuffs.Add(Disease.WEAKEN);
                cureDebuffs.Add(Disease.SLOW);
            }
            if (cure.Cure_Curse)
                cureDebuffs.Add(Disease.CURSE);
        }

        statups = new();
        if (template is IStatEffectPower power)
        {
            mhpR = (sbyte)power.MHPR;
            mhpRRate = (short)(power.MHPRate * 100);
            mmpR = (sbyte)power.MMPR;
            mmpRRate = (short)(power.MMPRate * 100);

            watk = (short)power.PAD;
            wdef = (short)power.PDD;
            matk = (short)power.MAD;
            mdef = (short)power.MDD;
            acc = (short)power.ACC;
            avoid = (short)power.EVA;
            speed = (short)power.Speed;
            jump = (short)power.Jump;

            // 强化疾风步
            if (YamlConfig.config.server.USE_ULTRA_NIMBLE_FEET
                    && (sourceid == Beginner.NIMBLE_FEET || sourceid == Noblesse.NIMBLE_FEET || sourceid == Evan.NIMBLE_FEET || sourceid == Legend.AGILE_BODY))
            {
                jump = (short)(speed * 4);
                speed *= 15;
            }

            // 属性可能是使用技能时对技能的临时加成，仅对buff起效（召唤也是overtime 需要排除）
            if (overTime && !isSummonProp)
            {
                // buffstat名字上看是回复，但是属性又是提升上限
                addBuffStatPairToListIfNotZero(statups, BuffStat.HPREC, mhpR);
                addBuffStatPairToListIfNotZero(statups, BuffStat.MPREC, mmpR);

                addBuffStatPairToListIfNotZero(statups, BuffStat.WATK, watk);
                addBuffStatPairToListIfNotZero(statups, BuffStat.WDEF, wdef);
                addBuffStatPairToListIfNotZero(statups, BuffStat.MATK, matk);
                addBuffStatPairToListIfNotZero(statups, BuffStat.MDEF, mdef);
                addBuffStatPairToListIfNotZero(statups, BuffStat.ACC, acc);
                addBuffStatPairToListIfNotZero(statups, BuffStat.AVOID, avoid);
                addBuffStatPairToListIfNotZero(statups, BuffStat.SPEED, speed);
                addBuffStatPairToListIfNotZero(statups, BuffStat.JUMP, jump);
            }
        }

        if (template is IStatEffectExp exp && exp.ExpBuffRate > 0)
        {
            addBuffStatPairToListIfNotZero(statups, BuffStat.EXP_BUFF, exp.ExpBuffRate);
        }

        if (template is IStatEffectExpInc expInc && expInc.ExpInc > 0)
        {
            // 不像buff  更像一次性获得经验
            addBuffStatPairToListIfNotZero(statups, BuffStat.EXP_INCREASE, expInc.ExpInc);
        }

        if (template is IStatEffectMorphGhost morphGhost)
        {
            addBuffStatPairToListIfNotZero(statups, BuffStat.GHOST_MORPH, morphGhost.Ghost);
        }

        if (template is IStatEffectMorph morphEffect)
        {
            hp = (short)morphEffect.HP;
        }

        if (template is CouponItemTemplate coupon)
        {
            if (coupon.IsExp)
            {
                switch (coupon.Rate)
                {
                    case 1:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_EXP1, 1);
                        break;

                    case 2:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_EXP2, 1);
                        break;

                    case 3:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_EXP3, 1);
                        break;

                    case 4:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_EXP4, 1);
                        break;
                }
            }
            if (coupon.IsDrop)
            {
                switch (coupon.Rate)
                {
                    case 1:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_DRP1, 1);
                        break;

                    case 2:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_DRP2, 1);
                        break;

                    case 3:
                        addBuffStatPairToListIfNotZero(statups, BuffStat.COUPON_DRP3, 1);
                        break;
                }
            }
        }

        if (template is PotionItemTemplate other)
        {
            addBuffStatPairToListIfNotZero(statups, BuffStat.AURA, other.Barrier);
            addBuffStatPairToListIfNotZero(statups, BuffStat.BERSERK, other.Berserk);
            addBuffStatPairToListIfNotZero(statups, BuffStat.BOOSTER, other.Booster);
        }

        {
            if (template is MonsterCardItemTemplate mobCard)
            {
                Prob = mobCard.Prob;
                // 客户端计算
                if (mobCard.RespectPimmune)
                {
                    addBuffStatPairToListIfNotZero(statups, BuffStat.RESPECT_PIMMUNE, mobCard.Prob);
                }

                if (mobCard.RespectMimmune)
                {
                    addBuffStatPairToListIfNotZero(statups, BuffStat.RESPECT_MIMMUNE, mobCard.Prob);
                }

                if (!string.IsNullOrEmpty(mobCard.DefenseAtt))
                {
                    DefenseAtt = mobCard.DefenseAtt[0];
                    addBuffStatPairToListIfNotZero(statups, BuffStat.DEFENSE_ATT, mobCard.Prob);
                }

                if (!string.IsNullOrEmpty(mobCard.DefenseState))
                {
                    DefenseState = Disease.GetDiseaseByAb(mobCard.DefenseState);
                    addBuffStatPairToListIfNotZero(statups, BuffStat.DEFENSE_STATE, mobCard.Prob);
                }

                foreach (var conData in mobCard.Con)
                {
                    ScopedEffects.Add(new(conData.StartMap, conData.EndMap, conData.InParty));
                }
            }

            if (template is IItemStatEffectMesoUp mesoUp && mesoUp.MesoUp)
            {
                Prob = mesoUp.Prob;
                // 为什么是4？
                // 改成prob，当存在多个有效buff时，取用倍率最高的
                addBuffStatPairToListIfNotZero(statups, BuffStat.MESO_UP_BY_ITEM, mesoUp.Prob);
            }

            if (template is IItemStatEffectItemUp itemUp && itemUp.ItemUp > 0)
            {
                Prob = itemUp.Prob;
                addBuffStatPairToListIfNotZero(statups, BuffStat.ITEM_UP_BY_ITEM, itemUp.Prob);
            }
        }

        if (template is IStatEffectMapProtection mapProtect && mapProtect.Thaw != 0)
        {
            // 10. 水下保护，-6. 寒冷保护，直接把Thaw的值写进value是否会有问题？
            if (mapProtect.Thaw == 10)
                addBuffStatPairToListIfNotZero(statups, BuffStat.MAP_PROTECTION, 1);
            else if (mapProtect.Thaw == -6)
                addBuffStatPairToListIfNotZero(statups, BuffStat.MAP_PROTECTION, 2);
        }

        monsterStatus = new();
        if (template is SkillLevelData skillData)
        {
            SkillLevel = skillData.Level;
            lt = skillData.LeftTop;
            rb = skillData.RightBottom;

            if (YamlConfig.config.server.USE_MAXRANGE_ECHO_OF_HERO
                && (sourceid == Beginner.ECHO_OF_HERO || sourceid == Noblesse.ECHO_OF_HERO || sourceid == Legend.ECHO_OF_HERO || sourceid == Evan.ECHO_OF_HERO))
            {
                lt = new Point(int.MinValue, int.MinValue);
                rb = new Point(int.MaxValue, int.MaxValue);
            }

            x = skillData.X;

            if (YamlConfig.config.server.USE_ULTRA_RECOVERY
                && (sourceid == Beginner.RECOVERY || sourceid == Noblesse.RECOVERY || sourceid == Evan.RECOVERY || sourceid == Legend.RECOVERY))
            {
                x *= 10;
            }
            y = skillData.Y;

            damage = skillData.Damage;
            fixdamage = skillData.FixDamage;
            attackCount = skillData.AttackCount;
            bulletConsume = (short)skillData.BulletConsume;
            bulletCount = (short)skillData.BulletCount;
            mobCount = skillData.MobCount;
            prop = skillData.Prop;
            cooldown = skillData.Cooltime;

            moneyCon = skillData.MoneyCon;
            itemCon = skillData.ItemCon;
            itemConNo = skillData.ItemConNo;

            mpCon = (short)skillData.MpCon;
            hpCon = (short)skillData.HpCon;

            if (isMapChair(sourceid))
            {
                addBuffStatPairToListIfNotZero(statups, BuffStat.MAP_CHAIR, 1);
            }


            switch (sourceid)
            {
                // BEGINNER
                case Beginner.RECOVERY:
                case Noblesse.RECOVERY:
                case Legend.RECOVERY:
                case Evan.RECOVERY:
                    statups.Add(new(BuffStat.RECOVERY, x));
                    break;
                case Beginner.ECHO_OF_HERO:
                case Noblesse.ECHO_OF_HERO:
                case Legend.ECHO_OF_HERO:
                case Evan.ECHO_OF_HERO:
                    statups.Add(new(BuffStat.ECHO_OF_HERO, x));
                    break;
                case Beginner.MONSTER_RIDER:
                case Noblesse.MONSTER_RIDER:
                case Legend.MONSTER_RIDER:
                case Corsair.BATTLE_SHIP:
                case Beginner.SPACESHIP:
                case Noblesse.SPACESHIP:
                case Beginner.YETI_MOUNT1:
                case Beginner.YETI_MOUNT2:
                case Noblesse.YETI_MOUNT1:
                case Noblesse.YETI_MOUNT2:
                case Legend.YETI_MOUNT1:
                case Legend.YETI_MOUNT2:
                case Beginner.WITCH_BROOMSTICK:
                case Noblesse.WITCH_BROOMSTICK:
                case Legend.WITCH_BROOMSTICK:
                case Beginner.BALROG_MOUNT:
                case Noblesse.BALROG_MOUNT:
                case Legend.BALROG_MOUNT:
                    statups.Add(new(BuffStat.MONSTER_RIDING, sourceid));
                    break;
                case Beginner.INVINCIBLE_BARRIER:
                case Noblesse.INVINCIBLE_BARRIER:
                case Legend.INVICIBLE_BARRIER:
                case Evan.INVINCIBLE_BARRIER:
                    statups.Add(new(BuffStat.DIVINE_BODY, 1));
                    break;
                case Fighter.POWER_GUARD:
                case Page.POWER_GUARD:
                    statups.Add(new(BuffStat.POWERGUARD, x));
                    break;
                case Spearman.HYPER_BODY:
                case GM.HYPER_BODY:
                case SuperGM.HYPER_BODY:
                    statups.Add(new(BuffStat.HYPERBODYHP, x));
                    statups.Add(new(BuffStat.HYPERBODYMP, y));
                    break;
                case Crusader.COMBO:
                case DawnWarrior.COMBO:
                    statups.Add(new(BuffStat.COMBO, 1));
                    break;
                case WhiteKnight.BW_FIRE_CHARGE:
                case WhiteKnight.BW_ICE_CHARGE:
                case WhiteKnight.BW_LIT_CHARGE:
                case WhiteKnight.SWORD_FIRE_CHARGE:
                case WhiteKnight.SWORD_ICE_CHARGE:
                case WhiteKnight.SWORD_LIT_CHARGE:
                case Paladin.BW_HOLY_CHARGE:
                case Paladin.SWORD_HOLY_CHARGE:
                case DawnWarrior.SOUL_CHARGE:
                case ThunderBreaker.LIGHTNING_CHARGE:
                    statups.Add(new(BuffStat.WK_CHARGE, x));
                    break;
                case DragonKnight.DRAGON_BLOOD:
                    statups.Add(new(BuffStat.DRAGONBLOOD, x));
                    break;
                case Hero.STANCE:
                case Paladin.STANCE:
                case DarkKnight.STANCE:
                case Aran.FREEZE_STANDING:
                    statups.Add(new(BuffStat.STANCE, prop));
                    break;
                case DawnWarrior.FINAL_ATTACK:
                case WindArcher.FINAL_ATTACK:
                    statups.Add(new(BuffStat.FINALATTACK, x));
                    break;
                // MAGICIAN
                case Magician.MAGIC_GUARD:
                case BlazeWizard.MAGIC_GUARD:
                case Evan.MAGIC_GUARD:
                    statups.Add(new(BuffStat.MAGIC_GUARD, x));
                    break;
                case Cleric.INVINCIBLE:
                    statups.Add(new(BuffStat.INVINCIBLE, x));
                    break;
                case Priest.HOLY_SYMBOL:
                case SuperGM.HOLY_SYMBOL:
                    statups.Add(new(BuffStat.HOLY_SYMBOL, x));
                    break;
                case FPArchMage.INFINITY:
                case ILArchMage.INFINITY:
                case Bishop.INFINITY:
                    statups.Add(new(BuffStat.INFINITY, x));
                    break;
                case FPArchMage.MANA_REFLECTION:
                case ILArchMage.MANA_REFLECTION:
                case Bishop.MANA_REFLECTION:
                    statups.Add(new(BuffStat.MANA_REFLECTION, 1));
                    break;
                case Bishop.HOLY_SHIELD:
                    statups.Add(new(BuffStat.HOLY_SHIELD, x));
                    break;
                case BlazeWizard.ELEMENTAL_RESET:
                case Evan.ELEMENTAL_RESET:
                    statups.Add(new(BuffStat.ELEMENTAL_RESET, x));
                    break;
                case Evan.MAGIC_SHIELD:
                    statups.Add(new(BuffStat.MAGIC_SHIELD, x));
                    break;
                case Evan.MAGIC_RESISTANCE:
                    statups.Add(new(BuffStat.MAGIC_RESISTANCE, x));
                    break;
                case Evan.SLOW:
                    statups.Add(new(BuffStat.SLOW, x));
                    goto case Priest.MYSTIC_DOOR;
                // BOWMAN
                case Priest.MYSTIC_DOOR:
                case Hunter.SOUL_ARROW:
                case Crossbowman.SOUL_ARROW:
                case WindArcher.SOUL_ARROW:
                    statups.Add(new(BuffStat.SOULARROW, x));
                    break;
                case Ranger.PUPPET:
                case Sniper.PUPPET:
                case WindArcher.PUPPET:
                case Outlaw.OCTOPUS:
                case Corsair.WRATH_OF_THE_OCTOPI:
                    statups.Add(new(BuffStat.PUPPET, 1));
                    break;
                case Bowmaster.CONCENTRATE:
                    statups.Add(new(BuffStat.CONCENTRATE, x));
                    break;
                case Bowmaster.HAMSTRING:
                    statups.Add(new(BuffStat.HAMSTRING, x));
                    monsterStatus.AddOrUpdate(MonsterStatus.SPEED, x);
                    break;
                case Marksman.BLIND:
                    statups.Add(new(BuffStat.BLIND, x));
                    monsterStatus.AddOrUpdate(MonsterStatus.ACC, x);
                    break;
                case Bowmaster.SHARP_EYES:
                case Marksman.SHARP_EYES:
                    statups.Add(new(BuffStat.SHARP_EYES, x << 8 | y));
                    break;
                case WindArcher.WIND_WALK:
                    statups.Add(new(BuffStat.WIND_WALK, x));
                    goto case Rogue.DARK_SIGHT;
                //break;    thanks Vcoc for noticing WW not showing for other players when changing maps
                case Rogue.DARK_SIGHT:
                case NightWalker.DARK_SIGHT:
                    statups.Add(new(BuffStat.DARKSIGHT, x));
                    break;
                case Hermit.MESO_UP:
                    statups.Add(new(BuffStat.MESOUP, x));
                    break;
                case Hermit.SHADOW_PARTNER:
                case NightWalker.SHADOW_PARTNER:
                    statups.Add(new(BuffStat.SHADOWPARTNER, x));
                    break;
                case ChiefBandit.MESO_GUARD:
                    statups.Add(new(BuffStat.MESOGUARD, x));
                    break;
                case ChiefBandit.PICKPOCKET:
                    statups.Add(new(BuffStat.PICKPOCKET, x));
                    break;
                case NightLord.SHADOW_STARS:
                    statups.Add(new(BuffStat.SHADOW_CLAW, 0));
                    break;
                // PIRATE
                case Pirate.DASH:
                case ThunderBreaker.DASH:
                case Beginner.SPACE_DASH:
                case Noblesse.SPACE_DASH:
                    statups.Add(new(BuffStat.DASH2, x));
                    statups.Add(new(BuffStat.DASH, y));
                    break;
                case Corsair.SPEED_INFUSION:
                case Buccaneer.SPEED_INFUSION:
                case ThunderBreaker.SPEED_INFUSION:
                    statups.Add(new(BuffStat.SPEED_INFUSION, x));
                    break;
                case Outlaw.HOMING_BEACON:
                case Corsair.BULLSEYE:
                    statups.Add(new(BuffStat.HOMING_BEACON, x));
                    break;
                case ThunderBreaker.SPARK:
                    statups.Add(new(BuffStat.SPARK, x));
                    break;
                // MULTIPLE
                case Aran.POLEARM_BOOSTER:
                case Fighter.AXE_BOOSTER:
                case Fighter.SWORD_BOOSTER:
                case Page.BW_BOOSTER:
                case Page.SWORD_BOOSTER:
                case Spearman.POLEARM_BOOSTER:
                case Spearman.SPEAR_BOOSTER:
                case Hunter.BOW_BOOSTER:
                case Crossbowman.CROSSBOW_BOOSTER:
                case Assassin.CLAW_BOOSTER:
                case Bandit.DAGGER_BOOSTER:
                case FPMage.SPELL_BOOSTER:
                case ILMage.SPELL_BOOSTER:
                case Brawler.KNUCKLER_BOOSTER:
                case Gunslinger.GUN_BOOSTER:
                case DawnWarrior.SWORD_BOOSTER:
                case BlazeWizard.SPELL_BOOSTER:
                case WindArcher.BOW_BOOSTER:
                case NightWalker.CLAW_BOOSTER:
                case ThunderBreaker.KNUCKLER_BOOSTER:
                case Evan.MAGIC_BOOSTER:
                case Beginner.POWER_EXPLOSION:
                case Noblesse.POWER_EXPLOSION:
                case Legend.POWER_EXPLOSION:
                    statups.Add(new(BuffStat.BOOSTER, x));
                    break;
                case Hero.MAPLE_WARRIOR:
                case Paladin.MAPLE_WARRIOR:
                case DarkKnight.MAPLE_WARRIOR:
                case FPArchMage.MAPLE_WARRIOR:
                case ILArchMage.MAPLE_WARRIOR:
                case Bishop.MAPLE_WARRIOR:
                case Bowmaster.MAPLE_WARRIOR:
                case Marksman.MAPLE_WARRIOR:
                case NightLord.MAPLE_WARRIOR:
                case Shadower.MAPLE_WARRIOR:
                case Corsair.MAPLE_WARRIOR:
                case Buccaneer.MAPLE_WARRIOR:
                case Aran.MAPLE_WARRIOR:
                case Evan.MAPLE_WARRIOR:
                    statups.Add(new(BuffStat.MAPLE_WARRIOR, x));
                    break;
                // SUMMON
                case Ranger.SILVER_HAWK:
                case Sniper.GOLDEN_EAGLE:
                    statups.Add(new(BuffStat.SUMMON, 1));
                    monsterStatus.AddOrUpdate(MonsterStatus.STUN, 1);
                    break;
                case FPArchMage.ELQUINES:
                case Marksman.FROST_PREY:
                    statups.Add(new(BuffStat.SUMMON, 1));
                    monsterStatus.AddOrUpdate(MonsterStatus.FREEZE, 1);
                    break;
                case Priest.SUMMON_DRAGON:
                case Bowmaster.PHOENIX:
                case ILArchMage.IFRIT:
                case Bishop.BAHAMUT:
                case DarkKnight.BEHOLDER:
                case Outlaw.GAVIOTA:
                case DawnWarrior.SOUL:
                case BlazeWizard.FLAME:
                case WindArcher.STORM:
                case NightWalker.DARKNESS:
                case ThunderBreaker.LIGHTNING:
                case BlazeWizard.IFRIT:
                    statups.Add(new(BuffStat.SUMMON, 1));
                    break;
                // ----------------------------- MONSTER STATUS ---------------------------------- //
                case Crusader.ARMOR_CRASH:
                case DragonKnight.POWER_CRASH:
                case WhiteKnight.MAGIC_CRASH:
                    monsterStatus.AddOrUpdate(MonsterStatus.SEAL_SKILL, 1);
                    break;
                case Rogue.DISORDER:
                    monsterStatus.AddOrUpdate(MonsterStatus.WATK, x);
                    monsterStatus.AddOrUpdate(MonsterStatus.WDEF, y);
                    break;
                case Corsair.HYPNOTIZE:
                    monsterStatus.AddOrUpdate(MonsterStatus.INERTMOB, 1);
                    break;
                case NightLord.NINJA_AMBUSH:
                case Shadower.NINJA_AMBUSH:
                    monsterStatus.AddOrUpdate(MonsterStatus.NINJA_AMBUSH, damage);
                    break;
                case Page.THREATEN:
                    monsterStatus.AddOrUpdate(MonsterStatus.WATK, x);
                    monsterStatus.AddOrUpdate(MonsterStatus.WDEF, y);
                    break;
                case DragonKnight.DRAGON_ROAR:
                    hpR = -x / 100.0;
                    monsterStatus.AddOrUpdate(MonsterStatus.STUN, 1);
                    break;
                case Crusader.AXE_COMA:
                case Crusader.SWORD_COMA:
                case Crusader.SHOUT:
                case WhiteKnight.CHARGE_BLOW:
                case Hunter.ARROW_BOMB:
                case ChiefBandit.ASSAULTER:
                case Shadower.BOOMERANG_STEP:
                case Brawler.BACK_SPIN_BLOW:
                case Brawler.DOUBLE_UPPERCUT:
                case Buccaneer.DEMOLITION:
                case Buccaneer.SNATCH:
                case Buccaneer.BARRAGE:
                case Gunslinger.BLANK_SHOT:
                case DawnWarrior.COMA:
                case ThunderBreaker.BARRAGE:
                case Aran.ROLLING_SPIN:
                case Evan.FIRE_BREATH:
                case Evan.BLAZE:
                case Paladin.GUARDIAN:
                case Hero.GUARDIAN:
                    monsterStatus.AddOrUpdate(MonsterStatus.STUN, 1);
                    break;
                case NightLord.TAUNT:
                case Shadower.TAUNT:
                    monsterStatus.AddOrUpdate(MonsterStatus.SHOWDOWN, x);
                    monsterStatus.AddOrUpdate(MonsterStatus.MDEF, x);
                    monsterStatus.AddOrUpdate(MonsterStatus.WDEF, x);
                    break;
                case ILWizard.COLD_BEAM:
                case ILMage.ICE_STRIKE:
                case ILArchMage.BLIZZARD:
                case ILMage.ELEMENT_COMPOSITION:
                case Sniper.BLIZZARD:
                case Outlaw.ICE_SPLITTER:
                case FPArchMage.PARALYZE:
                case Aran.COMBO_TEMPEST:
                case Evan.ICE_BREATH:
                    monsterStatus.AddOrUpdate(MonsterStatus.FREEZE, 1);
                    duration *= 2; // freezing skills are a little strange
                    break;
                case FPWizard.SLOW:
                case ILWizard.SLOW:
                case BlazeWizard.SLOW:
                    monsterStatus.AddOrUpdate(MonsterStatus.SPEED, x);
                    break;
                case FPWizard.POISON_BREATH:
                case FPMage.ELEMENT_COMPOSITION:
                    monsterStatus.AddOrUpdate(MonsterStatus.POISON, 1);
                    break;
                case Priest.DOOM:
                    monsterStatus.AddOrUpdate(MonsterStatus.DOOM, 1);
                    break;
                case ILMage.SEAL:
                case FPMage.SEAL:
                case BlazeWizard.SEAL:
                    monsterStatus.AddOrUpdate(MonsterStatus.SEAL, 1);
                    break;
                case Hermit.SHADOW_WEB: // shadow web
                case NightWalker.SHADOW_WEB:
                    monsterStatus.AddOrUpdate(MonsterStatus.SHADOW_WEB, 1);
                    break;
                case FPArchMage.FIRE_DEMON:
                case ILArchMage.ICE_DEMON:
                    monsterStatus.AddOrUpdate(MonsterStatus.POISON, 1);
                    monsterStatus.AddOrUpdate(MonsterStatus.FREEZE, 1);
                    break;
                case Evan.PHANTOM_IMPRINT:
                    monsterStatus.AddOrUpdate(MonsterStatus.PHANTOM_IMPRINT, x);
                    goto case Aran.COMBO_ABILITY;
                //ARAN
                case Aran.COMBO_ABILITY:
                    statups.Add(new(BuffStat.ARAN_COMBO, 100));
                    break;
                case Aran.COMBO_BARRIER:
                    statups.Add(new(BuffStat.COMBO_BARRIER, x));
                    break;
                case Aran.COMBO_DRAIN:
                    statups.Add(new(BuffStat.COMBO_DRAIN, x));
                    break;
                case Aran.SMART_KNOCKBACK:
                    statups.Add(new(BuffStat.SMART_KNOCKBACK, x));
                    break;
                case Aran.BODY_PRESSURE:
                    statups.Add(new(BuffStat.BODY_PRESSURE, x));
                    break;
                case Aran.SNOW_CHARGE:
                    statups.Add(new(BuffStat.WK_CHARGE, duration));
                    break;
                default:
                    break;
            }
        }


        statups.TrimExcess();
    }

    /**
     * @param applyto
     * @param obj
     * @param attack  damage done by the skill
     */
    public void applyPassive(Player applyto, IMapObject obj, int attack)
    {
        if (makeChanceResult())
        {
            switch (sourceid)
            { // MP eater
                case FPWizard.MP_EATER:
                case ILWizard.MP_EATER:
                case Cleric.MP_EATER:
                    if (obj == null || obj.getType() != MapObjectType.MONSTER)
                    {
                        return;
                    }
                    Monster mob = (Monster)obj; // x is absorb percentage
                    if (!mob.isBoss())
                    {
                        int absorbMp = Math.Min((int)(mob.getMaxMp() * (getX() / 100.0)), mob.getMp());
                        if (absorbMp > 0)
                        {
                            mob.setMp(mob.getMp() - absorbMp);
                            applyto.UpdateStatsChunk(() =>
                            {
                                applyto.ChangeMP(absorbMp);
                            });

                            applyto.sendPacket(PacketCreator.showOwnBuffEffect(sourceid, 1));
                            applyto.getMap().broadcastMessage(applyto, PacketCreator.showBuffEffect(applyto.getId(), sourceid, 1), false);
                        }
                    }
                    break;
            }
        }
    }

    public bool applyEchoOfHero(Player applyfrom)
    {
        var mapPlayers = applyfrom.getMap().getAllPlayers();

        bool hwResult = applyTo(applyfrom);
        foreach (Player chr in mapPlayers)
        {
            if (chr == applyfrom)
                continue;
            // Echo of Hero not buffing players in the map detected thanks to Masterrulax
            applyTo(applyfrom, chr, false, null, false, 1);
        }

        return hwResult;
    }

    public bool applyTo(Player chr)
    {
        return applyTo(chr, chr, true, null, false, 1);
    }

    public bool applyTo(Player chr, bool useMaxRange)
    {
        return applyTo(chr, chr, true, null, useMaxRange, 1);
    }

    public bool applyTo(Player chr, Point? pos)
    {
        return applyTo(chr, chr, true, pos, false, 1);
    }

    // primary: the player caster of the buff
    private bool applyTo(Player applyfrom, Player applyto, bool primary, Point? pos, bool useMaxRange, int affectedPlayers)
    {
        if (skill && (sourceid == GM.HIDE || sourceid == SuperGM.HIDE))
        {
            applyto.toggleHide(false);
            return true;
        }

        if (primary && isHeal())
        {
            affectedPlayers = applyBuff(applyfrom, useMaxRange);
        }

        int hpchange = calcHPChange(applyfrom, primary, affectedPlayers);
        int mpchange = calcMPChange(applyfrom, primary);
        if (primary)
        {
            if (itemConNo != 0)
            {
                if (!applyto.getAbstractPlayerInteraction().hasItem(itemCon, itemConNo))
                {
                    applyto.sendPacket(PacketCreator.enableActions());
                    return false;
                }
                InventoryManipulator.removeById(applyto.Client, ItemConstants.getInventoryType(itemCon), itemCon, itemConNo, false, true);
            }
        }
        else
        {
            if (isResurrection())
            {
                hpchange = applyto.ActualMaxHP;
                applyto.broadcastStance(applyto.isFacingLeft() ? 5 : 4);
            }
        }

        if (isDispel() && makeChanceResult())
        {
            applyto.dispelDebuffs();
        }
        else if (isCureAllAbnormalStatus())
        {
            applyto.purgeDebuffs();
        }
        else if (isComboReset())
        {
            applyto.setCombo(0);
        }
        /*if (applyfrom.getMp() < getMpCon()) {
         AutobanFactory.MPCON.addPoint(applyfrom.getAutobanManager(), "mpCon hack for skill:" + sourceid + "; Player MP: " + applyto.getMp() + " MP Needed: " + getMpCon());
         } */

        if (!applyto.applyHpMpChange(hpCon, hpchange, mpchange))
        {
            applyto.sendPacket(PacketCreator.enableActions());
            return false;
        }

        if (EffectTemplate is TownScrollItemTemplate townScroll)
        {
            if (townScroll.MoveTo != applyto.getMapId())
            {
                IMap target;
                Portal pt;

                if (townScroll.MoveTo == MapId.NONE)
                {
                    target = applyto.getMap().getReturnMap();
                    pt = target.getRandomPlayerSpawnpoint();
                }
                else
                {
                    target = applyto.getChannelServer().getMapFactory().getMap(townScroll.MoveTo);
                    int targetid = target.getId() / 10000000;
                    if (targetid != 60
                        && applyto.getMapId() / 10000000 != 61
                        && targetid != applyto.getMapId() / 10000000
                        && targetid != 21
                        && targetid != 20
                        && targetid != 12
                        && (applyto.getMapId() / 10000000 != 10
                        && applyto.getMapId() / 10000000 != 12))
                    {
                        return false;
                    }

                    pt = target.getRandomPlayerSpawnpoint();
                }

                applyto.changeMap(target, pt);
            }
            else
            {
                return false;
            }
        }
        if (isShadowClaw())
        {
            short projectileConsume = this.getBulletConsume();  // noticed by shavit

            Inventory use = applyto.getInventory(InventoryType.USE);

            Item? projectile = null;
            for (int i = 1; i <= use.getSlotLimit(); i++)
            { // impose order...
                var item = use.getItem((short)i);
                if (item != null)
                {
                    if (ItemConstants.isThrowingStar(item.getItemId()) && item.getQuantity() >= projectileConsume)
                    {
                        projectile = item;
                        break;
                    }
                }
            }
            if (projectile == null)
            {
                return false;
            }
            else
            {
                InventoryManipulator.removeFromSlot(applyto.Client, InventoryType.USE, projectile.getPosition(), projectileConsume, false, true);
            }
        }
        var summonMovementType = getSummonMovementType();
        if (overTime || isCygnusFA() || summonMovementType != null)
        {
            if (summonMovementType != null && pos != null)
            {
                applyto.cancelBuffStats(summonMovementType.Value == SummonMovementType.STATIONARY ? BuffStat.PUPPET : BuffStat.SUMMON);
                applyto.sendPacket(PacketCreator.enableActions());
            }

            applyBuffEffect(applyfrom, applyto, primary);
        }

        if (primary)
        {
            if (overTime)
            {
                applyBuff(applyfrom, useMaxRange);
            }

            if (isMonsterBuff())
            {
                applyMonsterBuff(applyfrom);
            }
        }

        if (EffectTemplate is MonsterCardItemTemplate monsterCardItem)
        {
            applyto.Monsterbook.addCard(monsterCardItem.TemplateId);
        }

        if (EffectTemplate is IItemStatEffectMC mc)
        {
            if (mc.CP != 0)
            {
                applyto.gainCP(mc.CP);
            }

            if (mc.CPSkill != 0 && applyto.Party > 0 && applyto.getMap().isCPQMap())
            {
                // added by Drago (Dragohe4rt)
                var skill = CarnivalFactory.getInstance().getSkill(mc.CPSkill);
                if (skill != null)
                {
                    var dis = skill.getDisease();
                    var opposition = applyfrom.MCTeam!.Enemy!;
                    if (skill.targetsAll)
                    {
                        foreach (var chrApp in opposition.Team.EligibleMembers)
                        {
                            if (chrApp.IsOnlined && chrApp.getMap().isCPQMap())
                            {
                                if (dis == null)
                                {
                                    chrApp.dispel();
                                }
                                else
                                {
                                    MobSkill mobSkill = skill.getSkill();
                                    chrApp.giveDebuff(dis, mobSkill);
                                }
                            }
                        }
                    }
                    else
                    {
                        var chrApp = applyfrom.getMap().getCharacterById(opposition.GetRandomMemberId());
                        if (chrApp != null && chrApp.getMap().isCPQMap())
                        {
                            if (dis == null)
                            {
                                chrApp.dispel();
                            }
                            else
                            {
                                MobSkill mobSkill = skill.getSkill();
                                chrApp.giveDebuff(dis, mobSkill);
                            }
                        }
                    }
                }
            }
        }

        if (EffectTemplate is IStatEffectIncMountFatigue mountFatigue && mountFatigue.IncFatigue != 0 && applyto.MountModel != null)
        {
            applyto.MountModel.setTiredness(applyto.MountModel.getTiredness() + mountFatigue.IncFatigue);
        }

        if (summonMovementType != null && pos != null)
        {
            Summon tosummon = new Summon(applyfrom, sourceid, pos.Value, summonMovementType.Value);
            applyfrom.getMap().spawnSummon(tosummon);
            applyfrom.addSummon(sourceid, tosummon);
            tosummon.addHP(x);
            if (isBeholder())
            {
                tosummon.addHP(1);
            }
        }
        if (isMagicDoor() && !FieldLimit.DOOR.check(applyto.getMap().getFieldLimit()))
        {
            // Magic Door
            int y = applyto.getFh();
            if (y == 0)
            {
                y = applyto.getMap().getGroundBelow(applyto.getPosition()).Y;    // thanks Lame for pointing out unusual cases of doors sending players on ground below
            }
            Point doorPosition = new Point(applyto.getPosition().X, y);

            var createDoorCode = Door.TryCreateDoor(applyto, doorPosition, out var door);
            if (createDoorCode == 0 && door != null)
            {
                applyto.applyPartyDoor(door);

                door.getTarget().spawnDoor(door.getAreaDoor());
                door.getTown().spawnDoor(door.getTownDoor());
            }
            else
            {
                applyto.GainItem(ItemId.MAGIC_ROCK, 1);

                if (createDoorCode == -2)
                {
                    applyto.Pink("Mystic Door cannot be cast on a slope, try elsewhere.");
                }
                else if (createDoorCode == -1)
                {
                    applyto.Pink("There are no door portals available for the town at this moment. Try again later.");
                }

                applyto.cancelBuffStats(BuffStat.SOULARROW);  // cancel door buff
            }
        }
        else if (isMist())
        {
            Rectangle bounds = calculateBoundingBox(sourceid == NightWalker.POISON_BOMB ? pos.Value : applyfrom.getPosition(), applyfrom.isFacingLeft());
            var mist = new PlayerMist(bounds, applyfrom, this);
            applyfrom.getMap().spawnMist(mist, getDuration(), mist.isPoisonMist(), false, mist.isRecoveryMist());
        }
        else if (isTimeLeap())
        {
            applyto.removeAllCooldownsExcept(Buccaneer.TIME_LEAP, true);
        }
        else if (cureDebuffs.Count > 0)
        { // added by Drago (Dragohe4rt)
            foreach (Disease debuff in cureDebuffs)
            {
                applyfrom.dispelDebuff(debuff);
            }
        }
        else if (EffectTemplate is IItemStatEffectMobSkill mobSkillEffect && mobSkillEffect.MobSkill != null)
        {
            MobSkillType mobSkillType = MobSkillTypeUtils.from(mobSkillEffect.MobSkill.MobSkill);
            MobSkill ms = MobSkillFactory.getMobSkillOrThrow(mobSkillType, mobSkillEffect.MobSkill.Level);
            var dis = Disease.GetBySkillTrust(mobSkillType);

            if (mobSkillEffect.MobSkill.Target > 0)
            {
                foreach (Player chr in applyto.getMap().getAllPlayers())
                {
                    if (chr.getId() != applyto.getId())
                    {
                        chr.giveDebuff(dis, ms);
                    }
                }
            }
            else
            {
                applyto.giveDebuff(dis, ms);
            }
        }
        return true;
    }

    private int applyBuff(Player applyfrom, bool useMaxRange)
    {
        int affectedc = 1;

        if (isPartyBuff() && (applyfrom.Party > 0 || isGmBuff()))
        {
            Rectangle bounds = (!useMaxRange)
                ? calculateBoundingBox(applyfrom.getPosition(), applyfrom.isFacingLeft())
                : new Rectangle(int.MinValue / 2, int.MinValue / 2, int.MaxValue, int.MaxValue);

            List<IMapObject> affecteds = applyfrom.getMap().getMapObjectsInBox(bounds, Arrays.asList(MapObjectType.PLAYER));
            List<Player> affectedp = new(affecteds.Count);
            foreach (var affectedmo in affecteds)
            {
                Player affected = (Player)affectedmo;
                if (affected != applyfrom && (isGmBuff() || (applyfrom.getPartyId() == affected.getPartyId())))
                {
                    if (isResurrection() ^ affected.isAlive())
                    {
                        affectedp.Add(affected);
                    }
                }
            }

            affectedc += affectedp.Count;   // used for heal
            foreach (Player affected in affectedp)
            {
                applyTo(applyfrom, affected, false, null, useMaxRange, affectedc);
                affected.sendPacket(PacketCreator.showOwnBuffEffect(sourceid, 2));
                affected.getMap().broadcastMessage(affected, PacketCreator.showBuffEffect(affected.getId(), sourceid, 2), false);
            }
        }

        return affectedc;
    }

    private void applyMonsterBuff(Player applyfrom)
    {
        Rectangle bounds = calculateBoundingBox(applyfrom.getPosition(), applyfrom.isFacingLeft());
        List<IMapObject> affected = applyfrom.getMap().getMapObjectsInBox(bounds, Arrays.asList(MapObjectType.MONSTER));
        var skill_ = SkillFactory.GetSkillTrust(sourceid);
        int i = 0;
        foreach (var mo in affected)
        {
            Monster monster = (Monster)mo;
            if (isDispel())
            {
                monster.debuffMob(skill_.getId());
            }
            else if (isSeal() && monster.isBoss())
            {  // thanks IxianMace for noticing seal working on bosses
                // do nothing
            }
            else
            {
                if (makeChanceResult())
                {
                    monster.applyStatus(applyfrom, new MonsterStatusEffect(getMonsterStati(), skill_), isPoison(), getDuration());
                    if (isCrash())
                    {
                        monster.debuffMob(skill_.getId());
                    }
                }
            }
            i++;
            if (i >= mobCount)
            {
                break;
            }
        }
    }

    private Rectangle calculateBoundingBox(Point posFrom, bool facingLeft)
    {
        Point mylt;
        Point myrb;
        if (facingLeft)
        {
            mylt = new Point(lt.Value.X + posFrom.X, lt.Value.Y + posFrom.Y);
            myrb = new Point(rb.Value.X + posFrom.X, rb.Value.Y + posFrom.Y);
        }
        else
        {
            myrb = new Point(-lt.Value.X + posFrom.X, rb.Value.Y + posFrom.Y);  // thanks Conrad, April for noticing a disturbance in AoE skill behavior after a hitched refactor here
            mylt = new Point(-rb.Value.X + posFrom.X, lt.Value.Y + posFrom.Y);
        }
        Rectangle bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
        return bounds;
    }

    public int getBuffLocalDuration()
    {
        return !YamlConfig.config.server.USE_BUFF_EVERLASTING ? duration : int.MaxValue;
    }

    public void silentApplyBuff(Player chr, long localStartTime, List<BuffStatValue> appliedBuffStats)
    {
        int localDuration = getBuffLocalDuration();
        localDuration = alchemistModifyVal(chr, localDuration, false);
        //CancelEffectAction cancelAction = new CancelEffectAction(chr, this, starttime);
        //ScheduledFuture<?> schedule = TimerManager.getInstance().schedule(cancelAction, ((starttime + localDuration) - Server.getInstance().getCurrentTime()));
        var expiredAt = localStartTime + localDuration;

        chr.registerEffect(this, appliedBuffStats, localStartTime, localStartTime + localDuration, true);
        var summonMovementType = getSummonMovementType();
        if (summonMovementType != null)
        {
            Summon tosummon = new Summon(chr, sourceid, chr.getPosition(), summonMovementType.Value);
            if (!tosummon.isStationary())
            {
                chr.addSummon(sourceid, tosummon);
                tosummon.addHP(x);
            }
        }
        if (sourceid == Corsair.BATTLE_SHIP)
        {
            chr.announceBattleshipHp();
        }
    }

    public void applyComboBuff(Player applyto, int combo)
    {
        applyto.sendPacket(PacketCreator.giveBuff(sourceid, 99999, new BuffStatValue(BuffStat.ARAN_COMBO, combo)));

        long starttime = applyto.Client.CurrentServer.Node.getCurrentTime();
        //	CancelEffectAction cancelAction = new CancelEffectAction(applyto, this, starttime);
        //	ScheduledFuture<?> schedule = TimerManager.getInstance().schedule(cancelAction, ((starttime + 99999) - Server.getInstance().getCurrentTime()));
        applyto.registerEffect(this, getStatups(), starttime, long.MaxValue, false);
    }

    public void applyBeaconBuff(Player applyto, int objectid)
    {
        // thanks Thora & Hyun for reporting an issue with homing beacon autoflagging mobs when changing maps
        // 按照其他调用，第一个参数应该是souceid, 第二个参数应该是有效时间（毫秒）
        applyto.sendPacket(PacketCreator.giveBuff(1, sourceid, new BuffStatValue(BuffStat.HOMING_BEACON, objectid)));

        long starttime = applyto.Client.CurrentServer.Node.getCurrentTime();
        applyto.registerEffect(this, getStatups(), starttime, long.MaxValue, false);
    }

    public void updateBuffEffect(Player target, BuffStatValue[] activeStats, long starttime)
    {
        int localDuration = getBuffLocalDuration();
        localDuration = alchemistModifyVal(target, localDuration, false);

        long leftDuration = (starttime + localDuration) - target.Client.CurrentServer.Node.getCurrentTime();
        if (leftDuration > 0)
        {
            if (isDash() || isInfusion())
            {
                target.sendPacket(PacketCreator.givePirateBuff(activeStats, getBuffSourceId(), (int)leftDuration));
            }
            else
            {
                target.sendPacket(PacketCreator.GiveBuff(this, (int)leftDuration, activeStats));
            }
        }
    }

    private void applyBuffEffect(Player applyfrom, Player applyto, bool primary)
    {
        if (!isMonsterRiding() && !isCouponBuff() && !isMysticDoor() && !isHyperBody() && !isCombo())
        {     // last mystic door already dispelled if it has been used before.
            applyto.cancelEffect(this, true);
        }

        var localStatupList = statups.ToList();
        int localDuration = getBuffLocalDuration();
        int localsourceid = sourceid;
        int seconds = localDuration / 1000;
        Mount? givemount = null;
        if (isMonsterRiding())
        {
            int ridingMountId = 0;
            Item? mount = applyfrom.getInventory(InventoryType.EQUIPPED).getItem(-18);
            if (mount != null)
            {
                ridingMountId = mount.getItemId();
            }

            if (sourceid == Corsair.BATTLE_SHIP)
            {
                ridingMountId = ItemId.BATTLESHIP;
            }
            else if (sourceid == Beginner.SPACESHIP || sourceid == Noblesse.SPACESHIP)
            {
                ridingMountId = 1932000 + applyto.getSkillLevel(sourceid);
            }
            else if (sourceid == Beginner.YETI_MOUNT1 || sourceid == Noblesse.YETI_MOUNT1 || sourceid == Legend.YETI_MOUNT1)
            {
                ridingMountId = 1932003;
            }
            else if (sourceid == Beginner.YETI_MOUNT2 || sourceid == Noblesse.YETI_MOUNT2 || sourceid == Legend.YETI_MOUNT2)
            {
                ridingMountId = 1932004;
            }
            else if (sourceid == Beginner.WITCH_BROOMSTICK || sourceid == Noblesse.WITCH_BROOMSTICK || sourceid == Legend.WITCH_BROOMSTICK)
            {
                ridingMountId = 1932005;
            }
            else if (sourceid == Beginner.BALROG_MOUNT || sourceid == Noblesse.BALROG_MOUNT || sourceid == Legend.BALROG_MOUNT)
            {
                ridingMountId = 1932010;
            }

            // thanks inhyuk for noticing some skill mounts not acting properly for other players when changing maps
            givemount = applyto.mount(ridingMountId, sourceid);

            localDuration = sourceid;
            localsourceid = ridingMountId;

            localStatupList = [new BuffStatValue(BuffStat.MONSTER_RIDING, 0)];
        }
        else if (EffectTemplate is IStatEffectMorph morph && morph.Valid())
        {
            // 存在随机，实际使用时附加
            activeMorphId = getMorph(applyto);
            localStatupList.Add(new(BuffStat.MORPH, activeMorphId));
        }
        if (primary)
        {
            localDuration = alchemistModifyVal(applyfrom, localDuration, false);
            applyto.getMap().broadcastMessage(applyto, PacketCreator.showBuffEffect(applyto.getId(), sourceid, 1, 3), false);
        }

        if (localStatupList.Count > 0)
        {
            var localstatups = localStatupList.ToArray();
            Packet? buff = null;
            Packet? mbuff = null;
            if (this.isActive(applyto))
            {
                buff = PacketCreator.GiveBuff(this, localDuration, localstatups);
            }
            if (isDash())
            {
                buff = PacketCreator.givePirateBuff(statups, sourceid, seconds);
                mbuff = PacketCreator.giveForeignPirateBuff(applyto.getId(), sourceid, seconds, localstatups);
            }
            else if (isWkCharge())
            {
                mbuff = PacketCreator.giveForeignWKChargeEffect(applyto.getId(), sourceid, localstatups);
            }
            else if (isInfusion())
            {
                buff = PacketCreator.givePirateBuff(localstatups, sourceid, seconds);
                mbuff = PacketCreator.giveForeignPirateBuff(applyto.getId(), sourceid, seconds, localstatups);
            }
            else if (isDs())
            {
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.DARKSIGHT, 0));
            }
            else if (isWw())
            {
                List<KeyValuePair<BuffStat, int>> dsstat = Collections.singletonList(new KeyValuePair<BuffStat, int>());
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.WIND_WALK, 0));
            }
            else if (isCombo())
            {
                int comboCount = applyto.getBuffedValue(BuffStat.COMBO) ?? 0;

                var combo = new BuffStatValue(BuffStat.COMBO, comboCount);
                buff = PacketCreator.GiveBuff(this, localDuration, combo);
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), combo);
            }
            else if (isMonsterRiding())
            {
                if (sourceid == Corsair.BATTLE_SHIP)
                {//hp
                    if (applyto.getBattleshipHp() <= 0)
                    {
                        applyto.resetBattleshipHp();
                    }

                    localstatups = statups.ToArray();
                }
                buff = PacketCreator.giveBuff(localsourceid, localDuration, localstatups);
                mbuff = PacketCreator.showMonsterRiding(applyto.getId(), givemount!);
                localDuration = duration;
            }
            else if (isShadowPartner())
            {
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.SHADOWPARTNER, 0));
            }
            else if (isSoulArrow())
            {
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.SOULARROW, 0));
            }
            else if (isEnrage())
            {
                applyto.handleOrbconsume();
            }
            else if (activeMorphId > 0)
            {
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.MORPH, activeMorphId));
            }
            else if (isAriantShield())
            {
                mbuff = PacketCreator.giveForeignBuff(applyto.getId(), new BuffStatValue(BuffStat.AURA, 1));
            }

            if (buff != null)
            {
                //Thanks flav for such a simple release! :)
                //Thanks Conrad, Atoot for noticing summons not using buff icon

                applyto.sendPacket(buff);
            }

            long starttime = applyto.Client.CurrentServer.Node.getCurrentTime();
            //CancelEffectAction cancelAction = new CancelEffectAction(applyto, this, starttime);
            //ScheduledFuture<?> schedule = TimerManager.getInstance().schedule(cancelAction, localDuration);
            applyto.registerEffect(this, localstatups, starttime, starttime + localDuration, false);
            if (mbuff != null)
            {
                applyto.getMap().broadcastMessage(applyto, mbuff, false);
            }
            if (sourceid == Corsair.BATTLE_SHIP)
            {
                applyto.announceBattleshipHp();
            }
        }
    }

    private int calcHPChange(Player applyfrom, bool primary, int affectedPlayers)
    {
        int hpchange = 0;
        if (hp != 0)
        {
            if (!skill)
            {
                if (primary)
                {
                    hpchange += alchemistModifyVal(applyfrom, hp, true);
                }
                else
                {
                    hpchange += hp;
                }
                if (applyfrom.hasDisease(Disease.ZOMBIFY))
                {
                    hpchange /= 2;
                }
            }
            else
            { 
                // assumption: this is heal
                float hpHeal = hp;
                if (sourceid == Cleric.HEAL)
                    hpHeal = (applyfrom.ActualMaxHP * (float)hp / (100.0f * affectedPlayers));

                hpchange += (int)hpHeal;
                if (applyfrom.hasDisease(Disease.ZOMBIFY))
                {
                    hpchange = -hpchange;
                    hpCon = 0;
                }
            }
        }
        if (hpR != 0)
        {
            hpchange += (int)(applyfrom.ActualMaxHP * hpR) / (applyfrom.hasDisease(Disease.ZOMBIFY) ? 2 : 1);
        }
        if (primary)
        {
            if (hpCon != 0)
            {
                hpchange -= hpCon;
            }
        }
        if (isChakra())
        {
            hpchange += makeHealHP(getY() / 100.0, applyfrom.getTotalLuk(), 2.3, 3.5);
        }
        else if (sourceid == SuperGM.HEAL_PLUS_DISPEL)
        {
            hpchange += applyfrom.ActualMaxHP;
        }

        return hpchange;
    }

    private int makeHealHP(double rate, double stat, double lowerfactor, double upperfactor)
    {
        return (int)((Randomizer.nextDouble() * ((int)(stat * upperfactor * rate) - (int)(stat * lowerfactor * rate) + 1)) + (int)(stat * lowerfactor * rate));
    }

    private int calcMPChange(Player applyfrom, bool primary)
    {
        int mpchange = 0;
        if (mp != 0)
        {
            if (primary)
            {
                mpchange += alchemistModifyVal(applyfrom, mp, true);
            }
            else
            {
                mpchange += mp;
            }
        }
        if (mpR != 0)
        {
            mpchange += (int)(applyfrom.ActualMaxMP * mpR);
        }
        if (primary)
        {
            if (mpCon != 0)
            {
                double mod = 1.0;
                bool isAFpMage = applyfrom.getJob().isA(Job.FP_MAGE);
                bool isCygnus = applyfrom.getJob().isA(Job.BLAZEWIZARD2);
                bool isEvan = applyfrom.getJob().isA(Job.EVAN7);
                if (isAFpMage || isCygnus || isEvan || applyfrom.getJob().isA(Job.IL_MAGE))
                {
                    Skill amp = isAFpMage ? SkillFactory.GetSkillTrust(FPMage.ELEMENT_AMPLIFICATION) : (isCygnus ? SkillFactory.GetSkillTrust(BlazeWizard.ELEMENT_AMPLIFICATION) : (isEvan ? SkillFactory.GetSkillTrust(Evan.MAGIC_AMPLIFICATION) : SkillFactory.GetSkillTrust(ILMage.ELEMENT_AMPLIFICATION)));
                    int ampLevel = applyfrom.getSkillLevel(amp);
                    if (ampLevel > 0)
                    {
                        mod = amp.getEffect(ampLevel).getX() / 100.0;
                    }
                }
                mpchange -= (int)(mpCon * mod);

                int? curBuffValue;
                if ((curBuffValue = applyfrom.getBuffedValue(BuffStat.INFINITY)) != null)
                {
                    mpchange = 0;
                }
                else if ((curBuffValue = applyfrom.getBuffedValue(BuffStat.CONCENTRATE)) != null)
                {
                    mpchange -= (int)(mpchange * (curBuffValue.Value / 100.0));
                }
            }
        }
        if (sourceid == SuperGM.HEAL_PLUS_DISPEL)
        {
            mpchange += applyfrom.ActualMaxMP;
        }

        return mpchange;
    }

    private int alchemistModifyVal(Player chr, int val, bool withX)
    {
        if (!skill && (chr.getJob().isA(Job.HERMIT) || chr.getJob().isA(Job.NIGHTWALKER3)))
        {
            var alchemistEffect = getAlchemistEffect(chr);
            if (alchemistEffect != null)
            {
                return (int)(val * ((withX ? alchemistEffect.getX() : alchemistEffect.getY()) / 100.0));
            }
        }
        return val;
    }

    private StatEffect? getAlchemistEffect(Player chr)
    {
        int id = Hermit.ALCHEMIST;
        if (chr.isCygnus())
        {
            id = NightWalker.ALCHEMIST;
        }
        var skill = SkillFactory.GetSkillTrust(id);
        int alchemistLevel = chr.getSkillLevel(skill);
        return alchemistLevel == 0 ? null : skill.getEffect(alchemistLevel);
    }

    private bool isGmBuff()
    {
        switch (sourceid)
        {
            case Beginner.ECHO_OF_HERO:
            case Noblesse.ECHO_OF_HERO:
            case Legend.ECHO_OF_HERO:
            case Evan.ECHO_OF_HERO:
            case SuperGM.HEAL_PLUS_DISPEL:
            case SuperGM.HASTE:
            case SuperGM.HOLY_SYMBOL:
            case SuperGM.BLESS:
            case SuperGM.RESURRECTION:
            case SuperGM.HYPER_BODY:
                return true;
            default:
                return false;
        }
    }

    private bool isMonsterBuff()
    {
        if (!skill)
        {
            return false;
        }
        switch (sourceid)
        {
            case Page.THREATEN:
            case FPWizard.SLOW:
            case ILWizard.SLOW:
            case FPMage.SEAL:
            case ILMage.SEAL:
            case Priest.DOOM:
            case Hermit.SHADOW_WEB:
            case NightLord.NINJA_AMBUSH:
            case Shadower.NINJA_AMBUSH:
            case BlazeWizard.SLOW:
            case BlazeWizard.SEAL:
            case NightWalker.SHADOW_WEB:
            case Crusader.ARMOR_CRASH:
            case DragonKnight.POWER_CRASH:
            case WhiteKnight.MAGIC_CRASH:
            case Priest.DISPEL:
            case SuperGM.HEAL_PLUS_DISPEL:
                return true;
        }
        return false;
    }

    private bool isPartyBuff()
    {
        if (lt == null || rb == null)
        {
            return false;
        }
        // wk charges have lt and rb set but are neither player nor monster buffs
        return (sourceid < 1211003 || sourceid > 1211008) && sourceid != Paladin.SWORD_HOLY_CHARGE && sourceid != Paladin.BW_HOLY_CHARGE && sourceid != DawnWarrior.SOUL_CHARGE;
    }

    private bool isHeal()
    {
        return sourceid == Cleric.HEAL || sourceid == SuperGM.HEAL_PLUS_DISPEL;
    }

    /// <summary>
    /// 复活术
    /// </summary>
    /// <returns></returns>
    private bool isResurrection()
    {
        return sourceid == Bishop.RESURRECTION || sourceid == GM.RESURRECTION || sourceid == SuperGM.RESURRECTION;
    }

    private bool isTimeLeap()
    {
        return sourceid == Buccaneer.TIME_LEAP;
    }

    public bool isDragonBlood()
    {
        return skill && sourceid == DragonKnight.DRAGON_BLOOD;
    }

    public bool isBerserk()
    {
        return skill && sourceid == DarkKnight.BERSERK;
    }

    public bool isRecovery()
    {
        return sourceid == Beginner.RECOVERY || sourceid == Noblesse.RECOVERY || sourceid == Legend.RECOVERY || sourceid == Evan.RECOVERY;
    }

    public bool isMapChair()
    {
        return sourceid == Beginner.MAP_CHAIR || sourceid == Noblesse.MAP_CHAIR || sourceid == Legend.MAP_CHAIR;
    }

    public static bool isMapChair(int sourceid)
    {
        return sourceid == Beginner.MAP_CHAIR || sourceid == Noblesse.MAP_CHAIR || sourceid == Legend.MAP_CHAIR;
    }

    public static bool isHpMpRecovery(int sourceid)
    {
        return sourceid == ItemId.RUSSELLONS_PILLS || sourceid == ItemId.SORCERERS_POTION || sourceid == 2022546;
    }

    public static bool isAriantShield(int sourceid)
    {
        return sourceid == ItemId.ARPQ_SHIELD;
    }

    private bool isDs()
    {
        return skill && (sourceid == Rogue.DARK_SIGHT || sourceid == NightWalker.DARK_SIGHT);
    }

    private bool isWw()
    {
        return skill && (sourceid == WindArcher.WIND_WALK);
    }

    private bool isCombo()
    {
        return skill && (sourceid == Crusader.COMBO || sourceid == DawnWarrior.COMBO);
    }

    private bool isEnrage()
    {
        return skill && sourceid == Hero.ENRAGE;
    }

    public bool isBeholder()
    {
        return skill && sourceid == DarkKnight.BEHOLDER;
    }

    private bool isShadowPartner()
    {
        return skill && (sourceid == Hermit.SHADOW_PARTNER || sourceid == NightWalker.SHADOW_PARTNER);
    }

    private bool isChakra()
    {
        return skill && sourceid == ChiefBandit.CHAKRA;
    }

    private bool isCouponBuff()
    {
        return ItemId.isRateCoupon(sourceid);
    }

    private bool isAriantShield()
    {
        int itemid = sourceid;
        return isAriantShield(itemid);
    }

    private bool isMysticDoor()
    {
        return skill && sourceid == Priest.MYSTIC_DOOR;
    }

    public bool isMonsterRiding()
    {
        return skill && (sourceid % 10000000 == 1004 || sourceid == Corsair.BATTLE_SHIP || sourceid == Beginner.SPACESHIP || sourceid == Noblesse.SPACESHIP
                || sourceid == Beginner.YETI_MOUNT1 || sourceid == Beginner.YETI_MOUNT2 || sourceid == Beginner.WITCH_BROOMSTICK || sourceid == Beginner.BALROG_MOUNT
                || sourceid == Noblesse.YETI_MOUNT1 || sourceid == Noblesse.YETI_MOUNT2 || sourceid == Noblesse.WITCH_BROOMSTICK || sourceid == Noblesse.BALROG_MOUNT
                || sourceid == Legend.YETI_MOUNT1 || sourceid == Legend.YETI_MOUNT2 || sourceid == Legend.WITCH_BROOMSTICK || sourceid == Legend.BALROG_MOUNT);
    }

    public bool isMagicDoor()
    {
        return skill && sourceid == Priest.MYSTIC_DOOR;
    }

    public bool isPoison()
    {
        return skill && (sourceid == FPMage.POISON_MIST || sourceid == FPWizard.POISON_BREATH || sourceid == FPMage.ELEMENT_COMPOSITION || sourceid == NightWalker.POISON_BOMB || sourceid == BlazeWizard.FLAME_GEAR);
    }

    private bool isMist()
    {
        return skill
            &&
            (sourceid == FPMage.POISON_MIST
            || sourceid == Shadower.SMOKE_SCREEN
            || sourceid == BlazeWizard.FLAME_GEAR
            || sourceid == NightWalker.POISON_BOMB
            || sourceid == Evan.RECOVERY_AURA);
    }

    private bool isSoulArrow()
    {
        return skill && (sourceid == Hunter.SOUL_ARROW || sourceid == Crossbowman.SOUL_ARROW || sourceid == WindArcher.SOUL_ARROW);
    }

    private bool isShadowClaw()
    {
        return skill && sourceid == NightLord.SHADOW_STARS;
    }

    private bool isCrash()
    {
        return skill && (sourceid == DragonKnight.POWER_CRASH || sourceid == Crusader.ARMOR_CRASH || sourceid == WhiteKnight.MAGIC_CRASH);
    }

    private bool isSeal()
    {
        return skill && (sourceid == ILMage.SEAL || sourceid == FPMage.SEAL || sourceid == BlazeWizard.SEAL);
    }

    private bool isDispel()
    {
        return skill && (sourceid == Priest.DISPEL || sourceid == SuperGM.HEAL_PLUS_DISPEL);
    }

    private bool isCureAllAbnormalStatus()
    {
        if (skill)
        {
            return isHerosWill(sourceid);
        }
        else
        {
            return sourceid == ItemId.WHITE_ELIXIR;
        }
    }

    public static bool isHerosWill(int skillid)
    {
        switch (skillid)
        {
            case Hero.HEROS_WILL:
            case Paladin.HEROS_WILL:
            case DarkKnight.HEROS_WILL:
            case FPArchMage.HEROS_WILL:
            case ILArchMage.HEROS_WILL:
            case Bishop.HEROS_WILL:
            case Bowmaster.HEROS_WILL:
            case Marksman.HEROS_WILL:
            case NightLord.HEROS_WILL:
            case Shadower.HEROS_WILL:
            case Buccaneer.PIRATES_RAGE:
            case Aran.HEROS_WILL:
                return true;

            default:
                return false;
        }
    }

    private bool isWkCharge()
    {
        if (!skill)
        {
            return false;
        }

        foreach (var p in statups)
        {
            if (p.BuffState.Equals(BuffStat.WK_CHARGE))
            {
                return true;
            }
        }

        return false;
    }

    private bool isDash()
    {
        return skill && (sourceid == Pirate.DASH || sourceid == ThunderBreaker.DASH || sourceid == Beginner.SPACE_DASH || sourceid == Noblesse.SPACE_DASH);
    }

    private bool isSkillMorph()
    {
        return skill && (sourceid == Buccaneer.SUPER_TRANSFORMATION || sourceid == Marauder.TRANSFORMATION || sourceid == WindArcher.EAGLE_EYE || sourceid == ThunderBreaker.TRANSFORMATION);
    }

    private bool isInfusion()
    {
        return skill && (sourceid == Buccaneer.SPEED_INFUSION || sourceid == Corsair.SPEED_INFUSION || sourceid == ThunderBreaker.SPEED_INFUSION);
    }

    private bool isCygnusFA()
    {
        return skill && (sourceid == DawnWarrior.FINAL_ATTACK || sourceid == WindArcher.FINAL_ATTACK);
    }

    private bool isHyperBody()
    {
        return skill && (sourceid == Spearman.HYPER_BODY || sourceid == GM.HYPER_BODY || sourceid == SuperGM.HYPER_BODY);
    }

    private bool isComboReset()
    {
        return sourceid == Aran.COMBO_BARRIER || sourceid == Aran.COMBO_DRAIN;
    }

    private int getMorph(Player chr)
    {
        if (EffectTemplate is IStatEffectMorph morph && morph.Valid())
        {
            if (morph.Morph > 0)
            {
                if (morph.Morph == 1000 || morph.Morph == 1001 || morph.Morph == 1003)
                {
                    return chr.getGender() == 0 ? morph.Morph : morph.Morph + 100;
                }
                return morph.Morph;
            }

            if (morph.MorphRandom.Length > 0)
            {
                var item = new LotteryMachine<MorphRandomData, int>(morph.MorphRandom, x => x.Prob).GetRandomItem();
                return item.Morph;
            }
        }
        return 0;
    }

    private SummonMovementType? getSummonMovementType()
    {
        if (!skill)
        {
            return null;
        }
        switch (sourceid)
        {
            case Ranger.PUPPET:
            case Sniper.PUPPET:
            case WindArcher.PUPPET:
            case Outlaw.OCTOPUS:
            case Corsair.WRATH_OF_THE_OCTOPI:
                return SummonMovementType.STATIONARY;
            case Ranger.SILVER_HAWK:
            case Sniper.GOLDEN_EAGLE:
            case Priest.SUMMON_DRAGON:
            case Marksman.FROST_PREY:
            case Bowmaster.PHOENIX:
            case Outlaw.GAVIOTA:
                return SummonMovementType.CIRCLE_FOLLOW;
            case DarkKnight.BEHOLDER:
            case FPArchMage.ELQUINES:
            case ILArchMage.IFRIT:
            case Bishop.BAHAMUT:
            case DawnWarrior.SOUL:
            case BlazeWizard.FLAME:
            case BlazeWizard.IFRIT:
            case WindArcher.STORM:
            case NightWalker.DARKNESS:
            case ThunderBreaker.LIGHTNING:
                return SummonMovementType.FOLLOW;
        }
        return null;
    }

    public bool isSkill()
    {
        return skill;
    }

    public int getSourceId()
    {
        return sourceid;
    }

    public int getBuffSourceId()
    {
        return skill ? sourceid : -sourceid;
    }

    public bool makeChanceResult()
    {
        return prop == 100 || Randomizer.nextInt(100) < prop;
    }

    /*
     private static class CancelEffectAction : Runnable {

     private StatEffect effect;
     private WeakReference<Player> target;
     private long startTime;

     public CancelEffectAction(Player target, StatEffect effect, long startTime) {
     this.effect = effect;
     this.target = new WeakReference<>(target);
     this.startTime = startTime;
     }

     public override void run() {
     Player realTarget = target.get();
     if (realTarget != null) {
     realTarget.cancelEffect(effect, false, startTime);
     }
     }
     }
     */
    public short getHp()
    {
        return hp;
    }

    public short getMp()
    {
        return mp;
    }

    public double getHpRate()
    {
        return hpR;
    }

    public double getMpRate()
    {
        return mpR;
    }

    public sbyte getHpR()
    {
        return mhpR;
    }

    public sbyte getMpR()
    {
        return mmpR;
    }

    public short getHpRRate()
    {
        return mhpRRate;
    }

    public short getMpRRate()
    {
        return mmpRRate;
    }

    public short getHpCon()
    {
        return hpCon;
    }

    public short getMpCon()
    {
        return mpCon;
    }

    public short getMatk()
    {
        return matk;
    }

    public short getWatk()
    {
        return watk;
    }

    public int getDuration()
    {
        return duration;
    }

    public List<BuffStatValue> getStatups()
    {
        return statups;
    }

    public bool sameSource(StatEffect effect)
    {
        return this.sourceid == effect.sourceid && this.skill == effect.skill;
    }

    public int getX()
    {
        return x;
    }

    public int getY()
    {
        return y;
    }

    public int getDamage()
    {
        return damage;
    }

    public int getAttackCount()
    {
        return attackCount;
    }

    public int getMobCount()
    {
        return mobCount;
    }

    public int getFixDamage()
    {
        return fixdamage;
    }

    public short getBulletCount()
    {
        return bulletCount;
    }

    public short getBulletConsume()
    {
        return bulletConsume;
    }

    public int getMoneyCon()
    {
        return moneyCon;
    }

    public int getCooldown()
    {
        return cooldown;
    }

    public Dictionary<MonsterStatus, int> getMonsterStati()
    {
        return monsterStatus;
    }

    public Skill? GetSkill() => skill ? SkillFactory.getSkill(sourceid) : null;
}
