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


using constants.skills;
using provider;
using provider.wz;
using server;
using server.life;
using System.Text;

namespace client;


public class SkillFactory
{
    private static volatile Dictionary<int, Skill> skills = [];
    private static DataProvider datasource = DataProviderFactory.getDataProvider(WZFiles.SKILL);

    public static Skill? getSkill(int id)
    {
        return skills.GetValueOrDefault(id);
    }

    public static Skill GetSkillTrust(int id)
    {
        return skills.GetValueOrDefault(id) ?? throw new BusinessResException($"SkillFactory.getSkill({id})");
    }

    public static void loadAllSkills()
    {
        Dictionary<int, Skill> loadedSkills = [];
        DataDirectoryEntry root = datasource.getRoot();
        foreach (DataFileEntry topDir in root.getFiles())
        {
            var topDirName = topDir.getName();
            // Loop thru jobs
            if (topDirName?.Length <= 8)
            {
                foreach (var data in datasource.getData(topDirName))
                { 
                    // Loop thru each jobs
                    if (data.getName() == "skill")
                    {
                        foreach (Data data2 in data)
                        { 
                            // Loop thru each jobs
                            if (data2 != null)
                            {
                                if (int.TryParse(data2.getName(), out var skillId))
                                    loadedSkills.AddOrUpdate(skillId, loadFromData(skillId, data2));
                            }
                        }
                    }
                }
            }
        }

        skills = loadedSkills;
    }

    private static Skill loadFromData(int id, Data data)
    {
        Skill ret = new Skill(id);
        bool isBuff = false;
        int skillType = DataTool.getInt("skillType", data, -1);
        var elem = DataTool.getString("elemAttr", data);
        if (elem != null)
        {
            ret.setElement(Element.getFromChar(elem.ElementAt(0)));
        }
        else
        {
            ret.setElement(Element.NEUTRAL);
        }
        var effect = data.getChildByPath("effect");
        if (skillType != -1)
        {
            if (skillType == 2)
            {
                isBuff = true;
            }
        }
        else
        {
            var action_ = data.getChildByPath("action");
            bool action = false;
            if (action_ == null)
            {
                if (data.getChildByPath("prepare/action") != null)
                {
                    action = true;
                }
                else
                {
                    switch (id)
                    {
                        case Gunslinger.INVISIBLE_SHOT:
                        case Corsair.HYPNOTIZE:
                            action = true;
                            break;
                    }
                }
            }
            else
            {
                action = true;
            }
            ret.setAction(action);
            var hit = data.getChildByPath("hit");
            var ball = data.getChildByPath("ball");
            isBuff = effect != null && hit == null && ball == null;
            isBuff |= action_ != null && DataTool.getString("0", action_) == "alert2";
            switch (id)
            {
                case Hero.RUSH:
                case Paladin.RUSH:
                case DarkKnight.RUSH:
                case DragonKnight.SACRIFICE:
                case FPMage.EXPLOSION:
                case FPMage.POISON_MIST:
                case Cleric.HEAL:
                case Ranger.MORTAL_BLOW:
                case Sniper.MORTAL_BLOW:
                case Assassin.DRAIN:
                case Hermit.SHADOW_WEB:
                case Bandit.STEAL:
                case Shadower.SMOKE_SCREEN:
                case SuperGM.HEAL_PLUS_DISPEL:
                case Hero.MONSTER_MAGNET:
                case Paladin.MONSTER_MAGNET:
                case DarkKnight.MONSTER_MAGNET:
                case Evan.ICE_BREATH:
                case Evan.FIRE_BREATH:
                case Gunslinger.RECOIL_SHOT:
                case Marauder.ENERGY_DRAIN:
                case BlazeWizard.FLAME_GEAR:
                case NightWalker.SHADOW_WEB:
                case NightWalker.POISON_BOMB:
                case NightWalker.VAMPIRE:
                case ChiefBandit.CHAKRA:
                case Aran.COMBAT_STEP:
                case Evan.RECOVERY_AURA:
                    isBuff = false;
                    break;
                case Beginner.RECOVERY:
                case Beginner.NIMBLE_FEET:
                case Beginner.MONSTER_RIDER:
                case Beginner.ECHO_OF_HERO:
                case Beginner.MAP_CHAIR:
                case Warrior.IRON_BODY:
                case Fighter.AXE_BOOSTER:
                case Fighter.POWER_GUARD:
                case Fighter.RAGE:
                case Fighter.SWORD_BOOSTER:
                case Crusader.ARMOR_CRASH:
                case Crusader.COMBO:
                case Hero.ENRAGE:
                case Hero.HEROS_WILL:
                case Hero.MAPLE_WARRIOR:
                case Hero.STANCE:
                case Page.BW_BOOSTER:
                case Page.POWER_GUARD:
                case Page.SWORD_BOOSTER:
                case Page.THREATEN:
                case WhiteKnight.BW_FIRE_CHARGE:
                case WhiteKnight.BW_ICE_CHARGE:
                case WhiteKnight.BW_LIT_CHARGE:
                case WhiteKnight.MAGIC_CRASH:
                case WhiteKnight.SWORD_FIRE_CHARGE:
                case WhiteKnight.SWORD_ICE_CHARGE:
                case WhiteKnight.SWORD_LIT_CHARGE:
                case Paladin.BW_HOLY_CHARGE:
                case Paladin.HEROS_WILL:
                case Paladin.MAPLE_WARRIOR:
                case Paladin.STANCE:
                case Paladin.SWORD_HOLY_CHARGE:
                case Spearman.HYPER_BODY:
                case Spearman.IRON_WILL:
                case Spearman.POLEARM_BOOSTER:
                case Spearman.SPEAR_BOOSTER:
                case DragonKnight.DRAGON_BLOOD:
                case DragonKnight.POWER_CRASH:
                case DarkKnight.AURA_OF_BEHOLDER:
                case DarkKnight.BEHOLDER:
                case DarkKnight.HEROS_WILL:
                case DarkKnight.HEX_OF_BEHOLDER:
                case DarkKnight.MAPLE_WARRIOR:
                case DarkKnight.STANCE:
                case Magician.MAGIC_GUARD:
                case Magician.MAGIC_ARMOR:
                case FPWizard.MEDITATION:
                case FPWizard.SLOW:
                case FPMage.SEAL:
                case FPMage.SPELL_BOOSTER:
                case FPArchMage.HEROS_WILL:
                case FPArchMage.INFINITY:
                case FPArchMage.MANA_REFLECTION:
                case FPArchMage.MAPLE_WARRIOR:
                case ILWizard.MEDITATION:
                case ILMage.SEAL:
                case ILWizard.SLOW:
                case ILMage.SPELL_BOOSTER:
                case ILArchMage.HEROS_WILL:
                case ILArchMage.INFINITY:
                case ILArchMage.MANA_REFLECTION:
                case ILArchMage.MAPLE_WARRIOR:
                case Cleric.INVINCIBLE:
                case Cleric.BLESS:
                case Priest.DISPEL:
                case Priest.DOOM:
                case Priest.HOLY_SYMBOL:
                case Priest.MYSTIC_DOOR:
                case Bishop.HEROS_WILL:
                case Bishop.HOLY_SHIELD:
                case Bishop.INFINITY:
                case Bishop.MANA_REFLECTION:
                case Bishop.MAPLE_WARRIOR:
                case Archer.FOCUS:
                case Hunter.BOW_BOOSTER:
                case Hunter.SOUL_ARROW:
                case Ranger.PUPPET:
                case Bowmaster.CONCENTRATE:
                case Bowmaster.HEROS_WILL:
                case Bowmaster.MAPLE_WARRIOR:
                case Bowmaster.SHARP_EYES:
                case Crossbowman.CROSSBOW_BOOSTER:
                case Crossbowman.SOUL_ARROW:
                case Sniper.PUPPET:
                case Marksman.BLIND:
                case Marksman.HEROS_WILL:
                case Marksman.MAPLE_WARRIOR:
                case Marksman.SHARP_EYES:
                case Rogue.DARK_SIGHT:
                case Assassin.CLAW_BOOSTER:
                case Assassin.HASTE:
                case Hermit.MESO_UP:
                case Hermit.SHADOW_PARTNER:
                case NightLord.HEROS_WILL:
                case NightLord.MAPLE_WARRIOR:
                case NightLord.NINJA_AMBUSH:
                case NightLord.SHADOW_STARS:
                case Bandit.DAGGER_BOOSTER:
                case Bandit.HASTE:
                case ChiefBandit.MESO_GUARD:
                case ChiefBandit.PICKPOCKET:
                case Shadower.HEROS_WILL:
                case Shadower.MAPLE_WARRIOR:
                case Shadower.NINJA_AMBUSH:
                case Pirate.DASH:
                case Marauder.TRANSFORMATION:
                case Buccaneer.SUPER_TRANSFORMATION:
                case Corsair.BATTLE_SHIP:
                case GM.HIDE:
                case SuperGM.HASTE:
                case SuperGM.HOLY_SYMBOL:
                case SuperGM.BLESS:
                case SuperGM.HIDE:
                case SuperGM.HYPER_BODY:
                case Noblesse.BLESSING_OF_THE_FAIRY:
                case Noblesse.ECHO_OF_HERO:
                case Noblesse.MONSTER_RIDER:
                case Noblesse.NIMBLE_FEET:
                case Noblesse.RECOVERY:
                case Noblesse.MAP_CHAIR:
                case DawnWarrior.COMBO:
                case DawnWarrior.FINAL_ATTACK:
                case DawnWarrior.IRON_BODY:
                case DawnWarrior.RAGE:
                case DawnWarrior.SOUL:
                case DawnWarrior.SOUL_CHARGE:
                case DawnWarrior.SWORD_BOOSTER:
                case BlazeWizard.ELEMENTAL_RESET:
                case BlazeWizard.FLAME:
                case BlazeWizard.IFRIT:
                case BlazeWizard.MAGIC_ARMOR:
                case BlazeWizard.MAGIC_GUARD:
                case BlazeWizard.MEDITATION:
                case BlazeWizard.SEAL:
                case BlazeWizard.SLOW:
                case BlazeWizard.SPELL_BOOSTER:
                case WindArcher.BOW_BOOSTER:
                case WindArcher.EAGLE_EYE:
                case WindArcher.FINAL_ATTACK:
                case WindArcher.FOCUS:
                case WindArcher.PUPPET:
                case WindArcher.SOUL_ARROW:
                case WindArcher.STORM:
                case WindArcher.WIND_WALK:
                case NightWalker.CLAW_BOOSTER:
                case NightWalker.DARKNESS:
                case NightWalker.DARK_SIGHT:
                case NightWalker.HASTE:
                case NightWalker.SHADOW_PARTNER:
                case ThunderBreaker.DASH:
                case ThunderBreaker.ENERGY_CHARGE:
                case ThunderBreaker.ENERGY_DRAIN:
                case ThunderBreaker.KNUCKLER_BOOSTER:
                case ThunderBreaker.LIGHTNING:
                case ThunderBreaker.SPARK:
                case ThunderBreaker.LIGHTNING_CHARGE:
                case ThunderBreaker.SPEED_INFUSION:
                case ThunderBreaker.TRANSFORMATION:
                case Legend.BLESSING_OF_THE_FAIRY:
                case Legend.AGILE_BODY:
                case Legend.ECHO_OF_HERO:
                case Legend.RECOVERY:
                case Legend.MONSTER_RIDER:
                case Legend.MAP_CHAIR:
                case Aran.MAPLE_WARRIOR:
                case Aran.HEROS_WILL:
                case Aran.POLEARM_BOOSTER:
                case Aran.COMBO_DRAIN:
                case Aran.SNOW_CHARGE:
                case Aran.BODY_PRESSURE:
                case Aran.SMART_KNOCKBACK:
                case Aran.COMBO_BARRIER:
                case Aran.COMBO_ABILITY:
                case Evan.BLESSING_OF_THE_FAIRY:
                case Evan.RECOVERY:
                case Evan.NIMBLE_FEET:
                case Evan.HEROS_WILL:
                case Evan.ECHO_OF_HERO:
                case Evan.MAGIC_BOOSTER:
                case Evan.MAGIC_GUARD:
                case Evan.ELEMENTAL_RESET:
                case Evan.MAPLE_WARRIOR:
                case Evan.MAGIC_RESISTANCE:
                case Evan.MAGIC_SHIELD:
                case Evan.SLOW:
                    isBuff = true;
                    break;
            }
        }

        foreach (var level in data.getChildByPath("level"))
        {
            ret.addLevelEffect(StatEffect.loadSkillEffectFromData(level, id, isBuff));
        }
        ret.setAnimationTime(0);
        if (effect != null)
        {
            foreach (var effectEntry in effect)
            {
                ret.incAnimationTime(DataTool.getIntConvert("delay", effectEntry, 0));
            }
        }
        return ret;
    }

    public static string? getSkillName(int skillid)
    {
        var data = DataProviderFactory.getDataProvider(WZFiles.STRING).getData("Skill.img");
        StringBuilder skill = new StringBuilder();
        skill.Append(skillid);
        if (skill.Length == 4)
        {
            skill.Remove(0, 4);
            skill.Append("000").Append(skillid);
        }

        var inValue = data?.getChildByPath(skill.ToString());
        if (inValue != null)
        {
            foreach (Data skilldata in inValue)
            {
                if (skilldata.getName() == "name")
                {
                    return DataTool.getString(skilldata);
                }
            }
        }

        return null;
    }
}
