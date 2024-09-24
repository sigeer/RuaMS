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


using net.packet;
using net.server.coordinator.session;
using tools;

namespace net.server.handlers.login;

public class AfterLoginHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        byte c2 = p.readByte();
        byte c3 = 5;
        if (p.available() > 0)
        {
            c3 = p.readByte();
        }
        if (c2 == 1 && c3 == 1)
        {
            if (c.getPin() == null || c.getPin().Equals(""))
            {
                c.sendPacket(PacketCreator.registerPin());
            }
            else
            {
                c.sendPacket(PacketCreator.requestPin());
            }
        }
        else if (c2 == 1 && c3 == 0)
        {
            string pin = p.readString();
            if (c.checkPin(pin))
            {
                c.sendPacket(PacketCreator.pinAccepted());
            }
            else
            {
                c.sendPacket(PacketCreator.requestPinAfterFailure());
            }
        }
        else if (c2 == 2 && c3 == 0)
        {
            string pin = p.readString();
            if (c.checkPin(pin))
            {
                c.sendPacket(PacketCreator.registerPin());
            }
            else
            {
                c.sendPacket(PacketCreator.requestPinAfterFailure());
            }
        }
        else if (c2 == 0 && c3 == 5)
        {
            SessionCoordinator.getInstance().closeSession(c);
            c.updateLoginState(Client.LOGIN_NOTLOGGEDIN);
        }
    }
}
