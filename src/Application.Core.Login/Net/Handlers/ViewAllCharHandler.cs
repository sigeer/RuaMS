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
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

public class ViewAllCharHandler : LoginHandlerBase
{
    private static int CHARACTER_LIMIT = 60; // IClient will crash if sending 61 or more characters

    public ViewAllCharHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override Task HandlePacket(InPacket p, ILoginClient c)
    {
        try
        {
            if (!c.CanRequestCharlist())
            {
                // client breaks if the charlist request pops too soon
                c.sendPacket(LoginPacketCreator.showAllCharacter(0, 0));
                return Task.CompletedTask;
            }

            var worldChrs = c.LoadCharactersView();

            int totalWorlds = worldChrs.Count;
            c.sendPacket(LoginPacketCreator.showAllCharacter(totalWorlds, worldChrs.Count));

            bool usePic = YamlConfig.config.server.ENABLE_PIC && !c.CanBypassPic();
            c.sendPacket(LoginPacketCreator.showAllCharacterInfo(c, 0, worldChrs, usePic));
            c.UpdateRequestCharListTick();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        return Task.CompletedTask;
    }
}
