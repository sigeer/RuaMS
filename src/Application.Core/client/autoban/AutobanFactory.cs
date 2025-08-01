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

namespace client.autoban;

/**
 * @author kevintjuh93
 */
public class AutobanFactory : EnumClass
{
    public static readonly AutobanFactory MOB_COUNT = new AutobanFactory();
    public static readonly AutobanFactory GENERAL = new AutobanFactory();
    public static readonly AutobanFactory FIX_DAMAGE = new AutobanFactory();
    public static readonly AutobanFactory DAMAGE_HACK = new(15, 60 * 1000);
    public static readonly AutobanFactory DISTANCE_HACK = new(10, 120 * 1000);
    public static readonly AutobanFactory PORTAL_DISTANCE = new(5, 30000);
    public static readonly AutobanFactory PACKET_EDIT = new();
    public static readonly AutobanFactory ACC_HACK = new();
    public static readonly AutobanFactory CREATION_GENERATOR = new();
    public static readonly AutobanFactory HIGH_HP_HEALING = new();
    public static readonly AutobanFactory FAST_HP_HEALING = new(15);
    public static readonly AutobanFactory FAST_MP_HEALING = new(20, 30000);
    public static readonly AutobanFactory GACHA_EXP = new();
    public static readonly AutobanFactory TUBI = new(20, 15000);
    public static readonly AutobanFactory SHORT_ITEM_VAC = new();
    public static readonly AutobanFactory ITEM_VAC = new();
    public static readonly AutobanFactory FAST_ITEM_PICKUP = new(5, 30000);
    public static readonly AutobanFactory FAST_ATTACK = new(10, 30000);
    public static readonly AutobanFactory MPCON = new(25, 30000);

    private int points;
    private long expiretime;

    AutobanFactory() : this(1, -1)
    {

    }

    AutobanFactory(int points) : this(points, -1)
    {
    }

    AutobanFactory(int points, long expire)
    {
        this.points = points;
        this.expiretime = expire;
    }

    public int getMaximum()
    {
        return points;
    }

    public long getExpire()
    {
        return expiretime;
    }
}
