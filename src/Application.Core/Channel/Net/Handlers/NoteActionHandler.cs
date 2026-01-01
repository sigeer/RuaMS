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


using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;



public class NoteActionHandler : ChannelHandlerBase
{
    readonly ILogger<NoteActionHandler> _logger;

    public NoteActionHandler(ILogger<NoteActionHandler> logger)
    {
        _logger = logger;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        int action = p.readByte();
        if (action == 0 && c.OnlinedCharacter.getCashShop().getAvailableNotes() > 0)
        {
            // Reply to gift in cash shop
            string charname = p.readString();
            string message = p.readString();
            if (c.OnlinedCharacter.getCashShop().isOpened())
            {
                c.sendPacket(PacketCreator.showCashInventory(c));
            }

            bool sendNoteSuccess = c.CurrentServerContainer.Transport.SendNormalNoteMessage(c.OnlinedCharacter.Id, charname, message);
            if (sendNoteSuccess)
            {
                c.OnlinedCharacter.getCashShop().decreaseNotes();
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

                var discardedNote = c.CurrentServerContainer.Transport.DeleteNoteMessage(id);
                if (discardedNote == null)
                {
                    _logger.LogWarning("Note with id {NoteId} not able to be discarded. Already discarded?", id);
                    continue;
                }

                fame += discardedNote.Fame;
            }
            if (fame > 0)
            {
                c.OnlinedCharacter.gainFame(fame);
            }
        }
        return Task.CompletedTask;
    }
}
