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

namespace Application.Shared.GameProps;

/// <summary>
/// mob 的 buff（客户端 <c>CMob</c> 的 MobStat 掩码）。
/// </summary>
public enum MonsterStatus
{
    WATK = 0,
    WDEF = 1,
    MATK = 2,

    MDEF = 3,
    ACC = 4,
    AVOID = 5,
    SPEED = 6,
    STUN = 7,
    FREEZE = 8,
    POISON = 9,
    SEAL = 10,
    SHOWDOWN = 11,
    WEAPON_ATTACK_UP = 12,
    WEAPON_DEFENSE_UP = 13,
    MAGIC_ATTACK_UP = 14,
    MAGIC_DEFENSE_UP = 15,
    DOOM = 16,
    SHADOW_WEB = 17,
    WEAPON_IMMUNITY = 18,
    MAGIC_IMMUNITY = 19,
    HARD_SKIN = 21,
    NINJA_AMBUSH = 22,
    NUELEMENTAL_ATTRIBUTELL = 23,
    VENOMOUS_WEAPON = 24,
    BLIND = 25,
    SEAL_SKILL = 26,
    INERTMOB = 28,
    WEAPON_REFLECT = 29,
    MAGIC_REFLECT = 30,

    Toss = 32,
    NEUTRALISE = 33,
    PHANTOM_IMPRINT = 34,
}
