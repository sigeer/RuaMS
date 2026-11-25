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
using Application.Core.Game.Trades;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;



public class NoteActionHandler : ChannelHandlerBase
{
    readonly CashShopManager _cashShopManager;
    readonly ILogger<NoteActionHandler> _logger;

    public NoteActionHandler(CashShopManager cashShopManager, ILogger<NoteActionHandler> logger)
    {
        _cashShopManager = cashShopManager;
        _logger = logger;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int action = p.readByte();

        // 回礼只能在商城里做：未读礼物条数记在商城会话（CashShopServer）上，不在 Player 里
        if (action == 0 && c.TradeServer is CashShopServer cashShop && cashShop.Notes > 0)
        {
            // Reply to gift in cash shop
            string charname = p.readString();
            string message = p.readString();

            await c.SendPacket(PacketCreator.showCashInventory(c.AccountEntity!.Id, _cashShopManager.GetState(c)));

            bool sendNoteSuccess = await c.CurrentServer.Node.Transport.SendNormalNoteMessage(c.OnlinedCharacter.Id, charname, message);
            if (sendNoteSuccess)
            {
                cashShop.Notes--;
            }
        }
        else if (action == 1)
        {
            // Discard notes in game
            int num = p.readByte();
            p.readByte();
            p.readByte();
            int fame = 0;
            for (int i = 0; i < num; i++)
            {
                int id = p.readInt();
                p.readByte(); //Fame, but we read it from the database :)

                var discardedNote = c.CurrentServer.Node.Transport.DeleteNoteMessage(id);
                if (discardedNote == null)
                {
                    _logger.LogWarning("Note with id {NoteId} not able to be discarded. Already discarded?", id);
                    continue;
                }

                fame += discardedNote.Fame;
            }
            if (fame > 0)
            {
                await c.OnlinedCharacter.gainFame(fame);
            }
        }
    }
}
