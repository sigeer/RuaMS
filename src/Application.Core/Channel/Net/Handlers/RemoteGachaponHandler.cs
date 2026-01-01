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


using Application.Core.Channel.ServerData;
using client.autoban;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Generic
 */
public class RemoteGachaponHandler : ChannelHandlerBase
{
    readonly AutoBanDataManager _autoBanManager;

    public RemoteGachaponHandler(AutoBanDataManager autoBanManager)
    {
        _autoBanManager = autoBanManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int ticket = p.readInt();
        int gacha = p.readInt();
        if (ticket != ItemId.REMOTE_GACHAPON_TICKET)
        {
            await _autoBanManager.Alert(AutobanFactory.GENERAL, c.OnlinedCharacter, " Tried to use RemoteGachaponHandler with item id: " + ticket);
            await c.Disconnect(false, false);
            return;
        }
        else if (gacha < 0 || gacha > 11)
        {
            await _autoBanManager.Alert(AutobanFactory.GENERAL, c.OnlinedCharacter, " Tried to use RemoteGachaponHandler with mode: " + gacha);
            await c.Disconnect(false, false);
            return;
        }
        else if (c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(ticket)).countById(ticket) < 1)
        {
            await _autoBanManager.Alert(AutobanFactory.GENERAL, c.OnlinedCharacter, " Tried to use RemoteGachaponHandler without a ticket.");
            await c.Disconnect(false, false);
            return;
        }
        int npcId = NpcId.GACHAPON_HENESYS;
        if (gacha != 8 && gacha != 9)
        {
            npcId += gacha;
        }
        else
        {
            npcId = gacha == 8 ? NpcId.GACHAPON_NLC : NpcId.GACHAPON_NAUTILUS;
        }
        c.CurrentServer.NPCScriptManager.start(c, npcId, "gachaponRemote", null);
    }
}
