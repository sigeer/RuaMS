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
namespace client.inventory;

public class WeaponType : EnumClass
{
    public static readonly WeaponType NOT_A_WEAPON = new(0);
    public static readonly WeaponType GENERAL1H_SWING = new(4.4);
    public static readonly WeaponType GENERAL1H_STAB = new(3.2);
    public static readonly WeaponType GENERAL2H_SWING = new(4.8);
    public static readonly WeaponType GENERAL2H_STAB = new(3.4);
    public static readonly WeaponType BOW = new(3.4);
    public static readonly WeaponType CLAW = new(3.6);
    public static readonly WeaponType CROSSBOW = new(3.6);
    public static readonly WeaponType DAGGER_THIEVES = new(3.6);
    public static readonly WeaponType DAGGER_OTHER = new(4);
    public static readonly WeaponType GUN = new(3.6);
    public static readonly WeaponType KNUCKLE = new(4.8);
    public static readonly WeaponType POLE_ARM_SWING = new(5.0);
    public static readonly WeaponType POLE_ARM_STAB = new(3.0);
    public static readonly WeaponType SPEAR_STAB = new(5.0);
    public static readonly WeaponType SPEAR_SWING = new(3.0);
    public static readonly WeaponType STAFF = new(3.6);
    public static readonly WeaponType SWORD1H = new(4.0);
    public static readonly WeaponType SWORD2H = new(4.6);
    public static readonly WeaponType WAND = new(3.6);
    private double damageMultiplier;

    private WeaponType(double maxDamageMultiplier)
    {
        this.damageMultiplier = maxDamageMultiplier;
    }

    public double getMaxDamageMultiplier()
    {
        return damageMultiplier;
    }

    public static explicit operator double(WeaponType d)
    {
        return d.damageMultiplier;
    }
}
