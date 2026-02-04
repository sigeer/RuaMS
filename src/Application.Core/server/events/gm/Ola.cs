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
using Application.Resources.Messages;
using tools;

namespace server.events.gm;



/// <summary>
/// TODO: 待重构，让其与WorldChannel关联或EventInstance
/// </summary>
public class Ola
{
    private Player chr;
    private long time = 0;
    private long timeStarted = 0;
    private ScheduledFuture? schedule = null;

    public Ola(Player chr)
    {
        this.chr = chr;
        this.schedule = chr.Client.CurrentServer.Node.TimerManager.schedule(() =>
        {
            chr.Client.CurrentServer.Post(new EventOlaTimeoutCommand(this));
        }, 360_000);
    }

    public void ProcessTimeout()
    {
        if (MapId.isOlaOla(chr.getMapId()))
        {
            chr.changeMap(chr.getMap().getReturnMap());
        }
        resetTimes();
    }

    public void startOla()
    { // TODO: Messages
        chr.getMap().startEvent();
        chr.sendPacket(PacketCreator.getClock(360));
        this.timeStarted = chr.getChannelServer().Node.getCurrentTime();
        this.time = 360000;

        chr.getMap().getPortal("join00")!.setPortalStatus(true);
        chr.Notice(nameof(ClientMessage.Notice_EventStart));
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
        return time - (chr.getChannelServer().Node.getCurrentTime() - timeStarted);
    }
}
