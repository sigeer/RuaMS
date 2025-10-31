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
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Shared.Login;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class SetGenderHandler : LoginHandlerBase
{
    readonly SessionCoordinator _sessionCoordinator;
    public SetGenderHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger)
    {
        _sessionCoordinator = sessionCoordinator;
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        if (c.AccountEntity?.Gender == 10)
        {
            //Packet shouldn't come if Gender isn't 10.
            byte confirmed = p.readByte();
            if (confirmed == 0x01)
            {
                c.AccountEntity.Gender = p.ReadSByte();
                c.CurrentServer.CommitAccountEntity(c.AccountEntity);
                c.sendPacket(LoginPacketCreator.GetAuthSuccess(c));

                _server.RegisterLoginState(c);
            }
            else
            {
                _sessionCoordinator.closeSession(c);
                c.updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);
            }
        }
    }

}
