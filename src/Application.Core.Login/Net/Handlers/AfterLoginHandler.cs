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


using Application.Core.Login;
using Application.Core.Login.Client;
using Application.Core.Login.Net;
using Application.Core.Login.Session;
using Application.Shared.Login;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Net.Handlers;

public class AfterLoginHandler : LoginHandlerBase
{
    readonly SessionCoordinator _sessionCoordinator;
    public AfterLoginHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger)
    {
        _sessionCoordinator = sessionCoordinator;
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        byte c2 = p.readByte();
        byte c3 = 5;
        if (p.available() > 0)
        {
            c3 = p.readByte();
        }
        if (c2 == 1 && c3 == 1)
        {
            if (string.IsNullOrEmpty(c.AccountEntity?.Pin))
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
            if (c.CheckPin(pin))
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
            if (c.CheckPin(pin))
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
            _sessionCoordinator.closeSession(c);
            c.updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);
        }
    }
}
