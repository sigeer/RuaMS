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


using Application.Core.Login.Client;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Login.Net.Handlers;

public class CharlistRequestHandler : LoginHandlerBase
{
    public CharlistRequestHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        p.readByte();
        int world = p.readByte();

        if (_server.IsWorldCapacityFull())
        {
            c.sendPacket(PacketCreator.getServerStatus(2));
            return;
        }

        int channel = p.readByte() + 1;
        var ch = _server.GetChannel(channel);
        if (ch == null)
        {
            c.sendPacket(PacketCreator.getServerStatus(2));
            return;
        }

        c.SelectedChannel = channel;

        c.SendCharList();
    }
}