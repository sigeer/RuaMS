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

/*
 * @author Rob
 */
public class RegisterPinHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        byte c2 = p.readByte();
        if (c2 == 0)
        {
            SessionCoordinator.getInstance().closeSession(c, null);
            c.updateLoginState(Client.LOGIN_NOTLOGGEDIN);
        }
        else
        {
            string pin = p.readString();
            if (pin != null)
            {
                c.setPin(pin);
                c.sendPacket(PacketCreator.pinRegistered());

                SessionCoordinator.getInstance().closeSession(c, null);
                c.updateLoginState(Client.LOGIN_NOTLOGGEDIN);
            }
        }
    }
}
