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
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

/*
 * @author David
 */
public class GuestLoginHandler : LoginHandlerBase
{
    readonly LoginPasswordHandler _loginHandler;
    public GuestLoginHandler(MasterServer server, ILogger<LoginHandlerBase> logger, LoginPasswordHandler loginPasswordHandler)
        : base(server, logger)
    {
        _loginHandler = loginPasswordHandler;
    }

    public override async Task HandlePacket(InPacket p, ILoginClient c)
    {
        c.sendPacket(LoginPacketCreator.sendGuestTOS());
        //Console.WriteLine(slea.ToString());
        await _loginHandler.HandlePacket(p, c);
    }
}
