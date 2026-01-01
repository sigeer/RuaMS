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
using Application.EF;
using Application.Utility.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

public class DeleteCharHandler : LoginHandlerBase
{
    public DeleteCharHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override async Task HandlePacket(InPacket p, ILoginClient c)
    {
        string pic = p.readString();
        int cid = p.readInt();


        if (c.CheckPic(pic))
        {
            //check for family, guild leader, pending marriage, world transfer
            // 可以让用户手动退出队伍、工会后再删除
            try
            {
                using var dbContext = new DBContext();
                var charModel = await dbContext.Characters.Where(x => x.Id == cid)
                    .Select(x => new { x.World, x.GuildId, x.GuildRank })
                    .FirstOrDefaultAsync() ?? throw new BusinessCharacterNotFoundException(cid);
                if (charModel.GuildId != 0 && charModel.GuildRank <= 1)
                {
                    c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, 0x16));
                    return;
                }

                foreach (var module in _server.Modules)
                {
                    var checkResult = module.DeleteCharacterCheck(cid);
                    if (checkResult != 0)
                    {
                        c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, checkResult));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete chrId {CharacterId}", cid);
                c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, 0x09));
                return;
            }

            if (_server.CharacterManager.DeleteChar(cid, c.AccountEntity!.Id))
            {
                _logger.LogInformation("Account {AccountName} deleted chrId {CharacterId}", c.AccountEntity!.Name, cid);
                c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, 0));
            }
            else
            {
                c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, 0x09));
            }
        }
        else
        {
            c.sendPacket(LoginPacketCreator.deleteCharResponse(cid, 0x14));
        }
    }
}
