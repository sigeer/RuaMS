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


using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Templates.Npc;
using tools;

namespace Application.Core.Game.Life;

public class NPC : AbstractLifeObject
{
    public NpcTemplate SourceTemplate { get; }

    public override Player? Controller => throw new NotImplementedException();

    public NPC(NpcTemplate npcTempate, IMap map, Point pos) : base(npcTempate.TemplateId, map, pos, 0)
    {
        SourceTemplate = npcTempate;
    }

    public bool hasShop(IChannelClient c)
    {
        return c.CurrentServer.NodeService.ShopManager.getShopForNPC(getId()) != null;
    }

    public async Task sendShop(IChannelClient c)
    {
        var shop = c.CurrentServer.NodeService.ShopManager.getShopForNPC(getId());
        if (shop != null)
            await shop.sendShop(c);
    }

    public override async Task sendSpawnData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.spawnNPC(this));
        await client.SendPacket(PacketCreator.spawnNPCRequestController(this, true));
    }

    public override async Task sendDestroyData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.removeNPCController(getObjectId()));
        await client.SendPacket(PacketCreator.removeNPC(getObjectId()));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.NPC;
    }

    public string getName()
    {
        return ClientCulture.SystemCulture.GetNpcName(getId());
    }

    public override string GetName()
    {
        return getName();
    }

    public override string GetReadableName(IChannelClient c)
    {
        return c.CurrentCulture.GetNpcName(getId());
    }

    public override int GetSourceId()
    {
        return getId();
    }
}
