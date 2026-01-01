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


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Shared.Net;

namespace Application.Module.BBS.Channel.Net.Handlers;

public class BBSOperationHandler : ChannelHandlerBase
{

    readonly BBSManager _manager;

    public BBSOperationHandler(BBSManager manager)
    {
        _manager = manager;
    }

    private string correctLength(string inValue, int maxSize)
    {
        return inValue.Length > maxSize ? inValue.Substring(0, maxSize) : inValue;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.OnlinedCharacter.getGuildId() < 1)
        {
            return;
        }
        byte mode = p.readByte();
        int localthreadid = 0;
        switch (mode)
        {
            case 0:
                bool bEdit = p.readByte() == 1;
                if (bEdit)
                {
                    localthreadid = p.readInt();
                }
                bool bNotice = p.readByte() == 1;
                string title = correctLength(p.readString(), 25);
                string text = correctLength(p.readString(), 600);
                int icon = p.readInt();
                if (icon >= 0x64 && icon <= 0x6a)
                {
                    if (!c.OnlinedCharacter.haveItemWithId(5290000 + icon - 0x64, false))
                    {
                        return;
                    }
                }
                else if (icon < 0 || icon > 3)
                {
                    return;
                }
                if (!bEdit)
                {
                    _manager.newBBSThread(c.OnlinedCharacter, title, text, icon, bNotice);
                }
                else
                {
                    _manager.editBBSThread(c.OnlinedCharacter, title, text, icon, localthreadid);
                }
                break;
            case 1:
                localthreadid = p.readInt();
                _manager.deleteBBSThread(c.OnlinedCharacter, localthreadid);
                break;
            case 2:
                int start = p.readInt();
                _manager.listBBSThreads(c.OnlinedCharacter, start * 10);
                break;
            case 3: // list thread + reply, following by id (int)
                localthreadid = p.readInt();
                _manager.ShowThread(c.OnlinedCharacter, localthreadid);
                break;
            case 4: // reply
                localthreadid = p.readInt();
                text = correctLength(p.readString(), 25);
                _manager.PostReply(c.OnlinedCharacter, localthreadid, text);
                break;
            case 5: // delete reply
                int unknown1 = p.readInt(); // we don't use this
                int replyid = p.readInt();
                _manager.deleteBBSReply(c.OnlinedCharacter, replyid);
                break;
            default:
                //Console.WriteLine("Unhandled BBS mode: " + slea.ToString());
                break;
        }
    }


}
