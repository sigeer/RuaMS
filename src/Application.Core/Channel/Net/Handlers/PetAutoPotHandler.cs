/*
	This file is part of the OdinMS Maple Story NewServer
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


using Application.Core.Channel.DataProviders;
using client.processor.action;

namespace Application.Core.Channel.Net.Handlers;

public class PetAutoPotHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var petId = p.readLong();
        p.readByte();

        var tick = p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();

        var chr = c.OnlinedCharacter;
        var stat = ItemInformationProvider.getInstance().getItemEffect(itemId);
        if (stat.getHp() > 0 || stat.getHpRate() > 0.0)
        {
            float estimatedHp = ((float)chr.HP) / chr.MaxHP;
            chr.setAutopotHpAlert(estimatedHp + 0.05f);
        }

        if (stat.getMp() > 0 || stat.getMpRate() > 0.0)
        {
            float estimatedMp = ((float)chr.MP) / chr.MaxMP;
            chr.setAutopotMpAlert(estimatedMp + 0.05f);
        }

        PetAutopotProcessor.runAutopotAction(c, slot, itemId);
    }

}
