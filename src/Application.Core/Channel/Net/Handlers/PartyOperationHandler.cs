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



public class PartyOperationHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int operation = p.readByte();
        var player = c.OnlinedCharacter;
        switch (operation)
        {
            case 1:
                { // create
                    await c.CurrentServerContainer.TeamManager.CreateTeam(player);
                    break;
                }
            case 2:
                { 
                    // leave/disband
                    await c.CurrentServerContainer.TeamManager.LeaveParty(player);
                    break;
                }
            case 3:
                { // join
                    int partyid = p.readInt();

                    await c.CurrentServerContainer.TeamManager.AnswerInvite(player, partyid, true);
                    break;
                }
            case 4:
                {
                    // invite
                    string name = p.readString();
                    await c.CurrentServerContainer.TeamManager.CreateInvite(player, name);
                    break;
                }
            case 5:
                {
                    // expel
                    int cid = p.readInt();
                    await c.CurrentServerContainer.TeamManager.ExpelFromParty(player, cid);
                    break;
                }
            case 6:
                {
                    // change leader
                    int newLeader = p.readInt();
                    await c.CurrentServerContainer.TeamManager.ChangeLeader(player, newLeader);
                    break;
                }
        }
    }
}