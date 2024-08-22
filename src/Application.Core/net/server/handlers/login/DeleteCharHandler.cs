/*
 This file is part of the OdinMS Maple Story Server
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


using client;
using net.packet;
using tools;

namespace net.server.handlers.login;

public class DeleteCharHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        string pic = p.readString();
        int cid = p.readInt();
        if (c.checkPic(pic))
        {
            //check for family, guild leader, pending marriage, world transfer
            try
            {
                using var dbContext = new DBContext();
                var charModel = dbContext.Characters.Where(x => x.Id == cid).Select(x => new { x.World, x.GuildId, x.GuildRank, x.FamilyId }).FirstOrDefault();
                if (charModel == null)
                    throw new BusinessWarningException("Character record does not exist.");

                if (charModel.GuildId != 0 && charModel.GuildRank <= 1)
                {
                    c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x16));
                    return;
                }
                else if (charModel.FamilyId != -1)
                {
                    var family = Server.getInstance().getWorld(charModel.World)!.getFamily(charModel.FamilyId);
                    if (family != null && family.getTotalMembers() > 1)
                    {
                        c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x1D));
                        return;
                    }
                }

                var worldTranferCount = dbContext.Worldtransfers.Where(x => x.Characterid == cid && x.CompletionTime == null).Count();

                if (worldTranferCount > 0)
                {
                    c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x1A));
                    return;
                }

            }
            catch (Exception e)
            {
                log.Error(e, "Failed to delete chrId {CharacterId}", cid);
                c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x09));
                return;
            }
            if (c.deleteCharacter(cid, c.getAccID()))
            {
                log.Information("Account {AccountName} deleted chrId {CharacterId}", c.getAccountName(), cid);
                c.sendPacket(PacketCreator.deleteCharResponse(cid, 0));
            }
            else
            {
                c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x09));
            }
        }
        else
        {
            c.sendPacket(PacketCreator.deleteCharResponse(cid, 0x14));
        }
    }
}
