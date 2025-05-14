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


using server;
using server.life;
using server.maps;
using tools;

namespace Application.Core.Game.Life;

public class NPC : AbstractLifeObject
{
    private NPCStats stats;

    public NPC(int id, NPCStats stats) : base(id)
    {
        this.stats = stats;
    }

    public bool hasShop()
    {
        return ShopFactory.getInstance().getShopForNPC(getId()) != null;
    }

    public void sendShop(IChannelClient c)
    {
        ShopFactory.getInstance().getShopForNPC(getId())?.sendShop(c);
    }

    public override void sendSpawnData(IChannelClient client)
    {
        client.sendPacket(PacketCreator.spawnNPC(this));
        client.sendPacket(PacketCreator.spawnNPCRequestController(this, true));
    }

    public override void sendDestroyData(IChannelClient client)
    {
        client.sendPacket(PacketCreator.removeNPCController(getObjectId()));
        client.sendPacket(PacketCreator.removeNPC(getObjectId()));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.NPC;
    }

    public string getName()
    {
        return stats.getName();
    }
}
