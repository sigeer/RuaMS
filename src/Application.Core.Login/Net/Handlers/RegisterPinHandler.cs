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


using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Core.Servers;
using Application.Shared.Login;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Login.Net.Handlers;

/*
 * @author Rob
 */
public class RegisterPinHandler : LoginHandlerBase
{
    readonly SessionCoordinator _sessionCoordinator;
    public RegisterPinHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator )
        : base(server, accountManager, logger)
    {
        _sessionCoordinator = sessionCoordinator;
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        byte c2 = p.readByte();
        if (c2 == 0)
        {
            _sessionCoordinator.closeSession(c);
            c.updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);
        }
        else
        {
            string pin = p.readString();
            if (pin != null)
            {
                if (c.AccountEntity != null)
                {
                    c.AccountEntity.Pin = pin;
                    c.sendPacket(PacketCreator.pinRegistered());
                }

                _sessionCoordinator.closeSession(c);
                c.updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);
            }
        }
    }
}
