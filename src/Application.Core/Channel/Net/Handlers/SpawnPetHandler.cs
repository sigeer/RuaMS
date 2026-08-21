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
using client.inventory.manipulator;

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
                if (item == null || item is not Pet petItem)
                    return;

                var mapPet = chr.GetPetById(petItem.UniqueId);
                if (mapPet != null)
                {
                    // 已经召唤了，召回
                    await mapPet.Recall();
                }
                else
                {
                    var defaultPet = chr.getPet(0);
                    if (chr.getSkillLevel(chr.JobModel.Type.GetMultiPetSkillId()) == 0 && defaultPet != null)
                    {
                        // 已经召唤主宠，但是没有学习群宠，召回主宠
                        await defaultPet.Recall();
                    }

                    int petItemId = petItem.getItemId();
                    if (petItemId == ItemId.DRAGON_PET || petItemId == ItemId.ROBO_PET)
                    {
                        var nextPetItem = petItem.EvolvePet(chr);
                        if (nextPetItem != null)
                        {
                            await InventoryManipulator.removeFromSlot(c, InventoryType.CASH, slot, 1, false, false);
                            await InventoryManipulator.addFromDrop(c, nextPetItem, false);
                            petItem = nextPetItem;
                        }
                    }

                    if (lead)
                    {
                        chr.shiftPetsRight();
                    }

                    await chr.SummonPet(petItem);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

}
