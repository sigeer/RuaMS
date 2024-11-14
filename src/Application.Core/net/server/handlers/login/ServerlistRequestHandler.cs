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


using constants.game;
using net.packet;
using tools;

namespace net.server.handlers.login;

public class ServerlistRequestHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        Server server = Server.getInstance();
        var worlds = server.getWorlds();
        c.requestedServerlist(worlds.Count);

        foreach (var world in worlds)
        {
            c.sendPacket(PacketCreator.getServerList(world.Id, world.Name, world.Flag, world.EventMessage, world.Channels));
        }
        c.sendPacket(PacketCreator.getEndOfServerList());
        c.sendPacket(PacketCreator.selectWorld(0));//too lazy to make a check lol
        c.sendPacket(PacketCreator.sendRecommended(worlds));
    }
}