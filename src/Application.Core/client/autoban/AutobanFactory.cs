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



using net.server;
using tools;

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

    private static HashSet<int> ignoredChrIds = new();

    private int points;
    private long expiretime;

    AutobanFactory() : this(1, -1)
    {

    }

    AutobanFactory(int points)
    {
        this.points = points;
        this.expiretime = -1;
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

    public void addPoint(AutobanManager ban, string reason)
    {
        ban.addPoint(this, reason);
    }

    public void alert(Character chr, string reason)
    {
        if (YamlConfig.config.server.USE_AUTOBAN)
        {
            if (chr != null && isIgnored(chr.getId()))
            {
                return;
            }
            Server.getInstance().broadcastGMMessage((chr != null ? chr.getWorld() : 0), PacketCreator.sendYellowTip((chr != null ? Character.makeMapleReadable(chr.getName()) : "") + " caused " + this.name() + " " + reason));
        }
        if (YamlConfig.config.server.USE_AUTOBAN_LOG)
        {
            string chrName = chr != null ? Character.makeMapleReadable(chr.getName()) : "";
            Log.Logger.Information("Autoban alert - chr {CharacterName} caused {AutoBanType}-{AutoBanReason}", chrName, this.name(), reason);
        }
    }

    public void autoban(Character chr, string value)
    {
        if (YamlConfig.config.server.USE_AUTOBAN)
        {
            chr.autoban("Autobanned foreach(" + this.name() + " in " + value + ")");
            //chr.sendPolice("You will be disconnected foreach(" + this.name() + " in " + value + ")");
        }
    }

    /**
     * Toggle ignored status for a character id.
     * An ignored character will not trigger GM alerts.
     *
     * @return new status. true if the chrId is now ignored, otherwise false.
     */
    public static bool toggleIgnored(int chrId)
    {
        if (ignoredChrIds.Contains(chrId))
        {
            ignoredChrIds.Remove(chrId);
            return false;
        }
        else
        {
            ignoredChrIds.Add(chrId);
            return true;
        }
    }

    private static bool isIgnored(int chrId)
    {
        return ignoredChrIds.Contains(chrId);
    }

    public static ICollection<int> getIgnoredChrIds()
    {
        return ignoredChrIds;
    }
}
