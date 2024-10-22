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



using Application.Core.Game.Trades;
using net.packet;
using tools;

namespace net.server.channel.handlers;

/**
 * @author kevintjuh93 - :3
 */
public class RemoteStoreHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        var chr = c.OnlinedCharacter;
        var hm = getMerchant(c);
        if (hm != null && hm.isOwner(chr))
        {
            if (hm.getChannel() == chr.getClient().getChannel())
            {
                hm.visitShop(chr);
            }
            else
            {
                c.sendPacket(PacketCreator.remoteChannelChange((byte)(hm.getChannel() - 1)));
            }
            return;
        }
        else
        {
            chr.dropMessage(1, "You don't have a Merchant open.");
        }
        c.sendPacket(PacketCreator.enableActions());
    }

    private static HiredMerchant? getMerchant(IClient c)
    {
        if (c.OnlinedCharacter.hasMerchant())
        {
            return c.getWorldServer().getHiredMerchant(c.OnlinedCharacter.getId());
        }
        return null;
    }
}
