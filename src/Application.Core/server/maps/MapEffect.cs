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


using Application.Utility.Tickables;
using tools;

namespace server.maps;

public class MapEffect: ILifedTickable
{
    private string msg;
    private int itemId;
    private bool active = true;

    public MapEffect(string msg, int itemId, long expiredAt)
    {
        this.msg = msg;
        this.itemId = itemId;
        ExpiredAt = expiredAt;
    }



    public Packet makeDestroyData()
    {
        return PacketCreator.removeMapEffect();
    }

    public Packet makeStartData()
    {
        return PacketCreator.startMapEffect(msg, itemId, active);
    }

    public void sendStartData(IChannelClient client)
    {
        client.sendPacket(makeStartData());
    }

    public long ExpiredAt { get; }

    public TickableStatus Status { get; private set; }
    public void OnTick(long now)
    {
        if (!this.IsAvailable())
        {
            return;
        }

        if (ExpiredAt <= now)
        {
            Status = TickableStatus.Remove;
            return;
        }
    }
}
