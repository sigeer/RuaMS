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
using Application.Shared.Constants.Skill;
using Application.Templates.Skill;

namespace Application.Shared.MapObjects.Summons;

public enum SummonMovementType
{
    /// <summary>
    /// 不可移动
    /// </summary>
    STATIONARY = 0,
    /// <summary>
    /// 跟随
    /// </summary>
    FOLLOW = 1,
    /// <summary>
    /// ？
    /// </summary>
    CIRCLE_FOLLOW = 3
}

public static class SummonMovementTypeExtensions
{
    public static SummonMovementType GetSummonMovementType(this SkillSummonData template)
    {
        if (!template.CanMove && !template.CanFly)
        {
            return SummonMovementType.STATIONARY;
        }
        else if (template.CanMove)
        {
            return SummonMovementType.FOLLOW;
        }
        else
        {
            return SummonMovementType.CIRCLE_FOLLOW;
        }
    }

    /// <summary>
    /// 根据测试，只有 Bishop.BAHAMUT=2321003 与新逻辑不同，是否是因为原代码就是错的？
    /// </summary>
    /// <param name="sourceid"></param>
    /// <returns></returns>
    public static SummonMovementType? GetSummonMovementTypeFromSkillId(int sourceid)
    {
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
}