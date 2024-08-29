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

namespace net.server.channel.handlers;

public class CancelDebuffHandler : AbstractPacketHandler
{//TIP: BAD STUFF LOL!

    public override void HandlePacket(InPacket p, IClient c)
    {
        /*List<Disease> diseases = c.OnlinedCharacter.getDiseases();
         List<Disease> diseases_ = new <Disease>();
         foreach(Disease disease in diseases) {
         List<Disease> disease_ = new <Disease>();
         disease_.Add(disease);
         diseases_.Add(disease);
         c.sendPacket(PacketCreator.cancelDebuff(disease_));
         c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.cancelForeignDebuff(c.OnlinedCharacter.getId(), disease_), false);
         }
         foreach(Disease disease in diseases_) {
         c.OnlinedCharacter.removeDisease(disease);
         }*/
    }
}