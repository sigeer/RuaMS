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


using constants.id;
using tools;

namespace server.events.gm;



/**
 * @author kevintjuh93
 */
public class Ola
{
    private IPlayer chr;
    private long time = 0;
    private long timeStarted = 0;
    private ScheduledFuture? schedule = null;

    public Ola(IPlayer chr)
    {
        this.chr = chr;
        this.schedule = TimerManager.getInstance().schedule(() =>
        {
            if (MapId.isOlaOla(chr.getMapId()))
            {
                chr.changeMap(chr.getMap().getReturnMap());
            }
            resetTimes();
        }, 360000);
    }

    public void startOla()
    { // TODO: Messages
        chr.getMap().startEvent();
        chr.sendPacket(PacketCreator.getClock(360));
        this.timeStarted = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        this.time = 360000;

        chr.getMap().getPortal("join00")!.setPortalStatus(true);
        chr.sendPacket(PacketCreator.serverNotice(0, "The portal has now opened. Press the up arrow key at the portal to enter."));
    }

    public bool isTimerStarted()
    {
        return time > 0 && timeStarted > 0;
    }

    public long getTime()
    {
        return time;
    }

    public void resetTimes()
    {
        this.time = 0;
        this.timeStarted = 0;
        schedule?.cancel(false);
    }

    public long getTimeLeft()
    {
        return time - (DateTimeOffset.Now.ToUnixTimeMilliseconds() - timeStarted);
    }
}
