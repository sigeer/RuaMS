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


using Application.Core.Game.Items;
using Application.Core.Game.Skills;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class SpawnPetHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt();
        byte slot = p.readByte();
        p.readByte();
        bool lead = p.readByte() == 1;

        {
            await c.tryacquireClient();
            try
            {
                var chr = c.OnlinedCharacter;
                var item = chr.getInventory(InventoryType.CASH).getItem(slot);
                if (item == null || item is not Pet pet)
                    return;

                int petItemId = pet.getItemId();
                if (petItemId == ItemId.DRAGON_PET || petItemId == ItemId.ROBO_PET)
                {
                    var evolveid = pet.SourceTemplate.Evol1;
                    if (chr.haveItem(evolveid))
                    {
                        await chr.dropMessage(5, "You can't hatch your " + (petItemId == ItemId.DRAGON_PET ? "Dragon egg" : "Robo egg") + " if you already have a Baby " + (petItemId == ItemId.DRAGON_PET ? "Dragon." : "Robo."));
                        await c.SendPacket(PacketCreator.enableActions());
                        return;
                    }
                    else
                    {
                        long expiration = item.getExpiration();
                        await InventoryManipulator.removeFromSlot(c, InventoryType.CASH, slot, 1, false, false);
                        await chr.GainItem(evolveid, 1, nextSetter: i => i.setExpiration(expiration));
                        await c.SendPacket(PacketCreator.enableActions());
                        return;
                    }
                }
                else
                {
                    await TogglePet(chr, pet, lead);
                }


            }
            finally
            {
                c.releaseClient();
            }
        }

        static async Task TogglePet(Player chr, Pet pet, bool lead)
        {
            var mapPet = chr.GetPetById(pet.PetId);
            if (mapPet != null)
            {
                await mapPet.Recall();
            }
            else
            {
                var defaultPet = chr.getPet(0);
                if (chr.getSkillLevel(SkillFactory.GetSkillTrust(8)) == 0 && defaultPet != null)
                {
                    await defaultPet.Recall();
                }

                if (lead)
                {
                    chr.shiftPetsRight();
                }

                await chr.SummonPet(pet);
            }
        }
    }

}
