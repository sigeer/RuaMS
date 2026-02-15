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

public class CreateCharHandler : LoginHandlerBase
{
    public CreateCharHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        string name = p.readString();
        int job = p.readInt();
        int face = p.readInt();

        int hair = p.readInt();
        int haircolor = p.readInt();
        int skincolor = p.readInt();

        int top = p.readInt();
        int bottom = p.readInt();
        int shoes = p.readInt();
        int weapon = p.readInt();
        int gender = p.readByte();

        if (job < 0 || job > 2)
        {
            c.sendPacket(LoginPacketCreator.deleteCharResponse(0, 9));
            return;
        }

        if (c.AccountEntity == null)
        {
            // 账号信息不存在
            c.sendPacket(LoginPacketCreator.deleteCharResponse(0, 9));
            return;
        }

        var model = c.CurrentServer.CreatePlayerService.CreateCharacter(c.AccountEntity, job, name, gender, face, hair + haircolor, skincolor, top, bottom, shoes, weapon);
        if (model == null)
        {
            c.sendPacket(LoginPacketCreator.deleteCharResponse(0, 9));
            return;
        }

        c.sendPacket(LoginPacketCreator.AddNewCharEntry(model));
    }
}