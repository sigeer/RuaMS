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


using Application.Templates.Item.Consume;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author XoticStory
 * <p>
 * Modified by -- kevintjuh93, Ronan
 */
public class UseSolomonHandler : UseItemHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();

        {
            await c.tryacquireClient();
            try
            {
                var chr = c.OnlinedCharacter;

                if (!TryGetItem(chr, slot , itemId, 1, out var slotItem))
                {
                    return;
                }

                if (slotItem.SourceTemplate is not SolomenItemTemplate itemTemplate
                    || chr.getLevel() > itemTemplate.MaxLevel
                    || itemTemplate.Exp + chr.getGachaExp() > int.MaxValue)
                    return;

                await chr.addGachaExp(itemTemplate.Exp);

                await RemoveItemStrictly(chr, slotItem);
            }
            finally
            {
                c.releaseClient();
            }
        }

        await c.SendPacket(PacketCreator.enableActions());
    }
}
