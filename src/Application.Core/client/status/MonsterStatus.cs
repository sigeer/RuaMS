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
namespace client.status;
public class MonsterStatus
{
    public static readonly MonsterStatus WATK = new MonsterStatus(0x1);
    public static readonly MonsterStatus WDEF = new MonsterStatus(0x2);
    public static readonly MonsterStatus NEUTRALISE = new MonsterStatus(0x2, true);
    public static readonly MonsterStatus PHANTOM_IMPRINT = new MonsterStatus(0x4, true);
    public static readonly MonsterStatus MATK = new MonsterStatus(0x4);
    public static readonly MonsterStatus MDEF = new MonsterStatus(0x8);
    public static readonly MonsterStatus ACC = new MonsterStatus(0x10);
    public static readonly MonsterStatus AVOID = new MonsterStatus(0x20);
    public static readonly MonsterStatus SPEED = new MonsterStatus(0x40);
    public static readonly MonsterStatus STUN = new MonsterStatus(0x80);
    public static readonly MonsterStatus FREEZE = new MonsterStatus(0x100);
    public static readonly MonsterStatus POISON = new MonsterStatus(0x200);
    public static readonly MonsterStatus SEAL = new MonsterStatus(0x400);
    public static readonly MonsterStatus SHOWDOWN = new MonsterStatus(0x800);
    public static readonly MonsterStatus WEAPON_ATTACK_UP = new MonsterStatus(0x1000);
    public static readonly MonsterStatus WEAPON_DEFENSE_UP = new MonsterStatus(0x2000);
    public static readonly MonsterStatus MAGIC_ATTACK_UP = new MonsterStatus(0x4000);
    public static readonly MonsterStatus MAGIC_DEFENSE_UP = new MonsterStatus(0x8000);
    public static readonly MonsterStatus DOOM = new MonsterStatus(0x10000);
    public static readonly MonsterStatus SHADOW_WEB = new MonsterStatus(0x20000);
    public static readonly MonsterStatus WEAPON_IMMUNITY = new MonsterStatus(0x40000);
    public static readonly MonsterStatus MAGIC_IMMUNITY = new MonsterStatus(0x80000);
    public static readonly MonsterStatus HARD_SKIN = new MonsterStatus(0x200000);
    public static readonly MonsterStatus NINJA_AMBUSH = new MonsterStatus(0x400000);
    public static readonly MonsterStatus NUELEMENTAL_ATTRIBUTELL = new MonsterStatus(0x800000);
    public static readonly MonsterStatus VENOMOUS_WEAPON = new MonsterStatus(0x1000000);
    public static readonly MonsterStatus BLIND = new MonsterStatus(0x2000000);
    public static readonly MonsterStatus SEAL_SKILL = new MonsterStatus(0x4000000);
    public static readonly MonsterStatus INERTMOB = new MonsterStatus(0x10000000);
    public static readonly MonsterStatus WEAPON_REFLECT = new MonsterStatus(0x20000000, true);
    public static readonly MonsterStatus MAGIC_REFLECT = new MonsterStatus(0x40000000, true);

    private int value;
    private bool first;

    private MonsterStatus(int i)
    {
        this.value = i;
        this.first = false;
    }

    private MonsterStatus(int i, bool first)
    {
        this.value = i;
        this.first = first;
    }

    public static explicit operator int(MonsterStatus obj)
    {
        return obj.value;
    }

    public bool isFirst()
    {
        return first;
    }

    public int getValue()
    {
        return value;
    }
}