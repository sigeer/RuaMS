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


using Application.Core.scripting.npc;
using net.packet;
using scripting.npc;
using scripting.quest;

namespace net.server.channel.handlers;

/**
 * @author Matze
 */
public class NPCMoreTalkHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        sbyte lastMsg = p.ReadSByte(); // 00 (last msg type I think)
        sbyte action = p.ReadSByte(); // 00 = end chat, 01 == follow
        if (lastMsg == 2)
        {
            if (action != 0)
            {
                string returnText = p.readString();
                if (c.NPCConversationManager != null)
                {
                    c.NPCConversationManager.setGetText(returnText);
                    if (c.NPCConversationManager is QuestActionManager q)
                    {
                        if (q.isStart())
                        {
                            QuestScriptManager.getInstance().start(c, action, lastMsg, -1);
                        }
                        else
                        {
                            QuestScriptManager.getInstance().end(c, action, lastMsg, -1);
                        }
                    }
                    else if (c.NPCConversationManager is TempConversation temp)
                    {
                        temp.Handle(action, lastMsg, -1);
                    }
                    else
                    {
                        NPCScriptManager.getInstance().action(c, action, lastMsg, -1);
                    }
                }
            }
            else
            {
                c.NPCConversationManager?.dispose();
            }
        }
        else
        {
            int selection = -1;
            if (p.available() >= 4)
            {
                selection = p.readInt();
            }
            else if (p.available() > 0)
            {
                selection = p.readByte();
            }
            if (c.NPCConversationManager is QuestActionManager q)
            {
                if (q.isStart())
                {
                    QuestScriptManager.getInstance().start(c, action, lastMsg, selection);
                }
                else
                {
                    QuestScriptManager.getInstance().end(c, action, lastMsg, selection);
                }
            }
            else if (c.NPCConversationManager is TempConversation temp)
            {
                temp.Handle(action, lastMsg, selection);
            }
            else if (c.NPCConversationManager != null)
            {
                NPCScriptManager.getInstance().action(c, action, lastMsg, selection);
            }
        }
    }
}