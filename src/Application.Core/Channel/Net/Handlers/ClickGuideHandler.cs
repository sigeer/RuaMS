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

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class ClickGuideHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.OnlinedCharacter.getJob().Equals(Job.NOBLESSE))
        {
            c.CurrentServer.NPCScriptManager.start(c, NpcId.MIMO, null);
        }
        else
        {
            c.CurrentServer.NPCScriptManager.start(c, NpcId.LILIN, null);
        }
    }

}
