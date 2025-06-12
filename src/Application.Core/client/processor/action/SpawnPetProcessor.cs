/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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

namespace client.processor.action;



/**
 * @author RonanLana - just added locking on OdinMS' SpawnPetHandler method body
 */
public class SpawnPetProcessor
{
    private static DataProvider dataRoot = DataProviderFactory.getDataProvider(WZFiles.ITEM);

    public static void processSpawnPet(IChannelClient c, byte slot, bool lead)
    {
        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;
                var item = chr.getInventory(InventoryType.CASH).getItem(slot);
                if (item == null || item is not Pet pet)
                    return;

                int petItemId = pet.getItemId();
                if (petItemId == ItemId.DRAGON_PET || petItemId == ItemId.ROBO_PET)
                {
                    if (chr.haveItem(petItemId + 1))
                    {
                        chr.dropMessage(5, "You can't hatch your " + (petItemId == ItemId.DRAGON_PET ? "Dragon egg" : "Robo egg") + " if you already have a Baby " + (petItemId == ItemId.DRAGON_PET ? "Dragon." : "Robo."));
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                    else
                    {
                        int evolveid = DataTool.getInt("info/evol1", dataRoot.getData("Pet/" + petItemId + ".img"));
                        long expiration = item.getExpiration();
                        InventoryManipulator.removeById(c, InventoryType.CASH, petItemId, 1, false, false);
                        InventoryManipulator.addById(c, evolveid, 1, expiration: expiration);

                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                }
                TogglePet(chr, pet, lead);

            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void TogglePet(IPlayer chr, Pet pet, bool lead)
    {
        if (chr.getPetIndex(pet) != -1)
        {
            chr.unequipPet(pet, true);
        }
        else
        {
            var defaultPet = chr.getPet(0);
            if (chr.getSkillLevel(SkillFactory.GetSkillTrust(8)) == 0 && defaultPet != null)
            {
                chr.unequipPet(defaultPet, false);
            }
            if (lead)
            {
                chr.shiftPetsRight();
            }
            Point pos = chr.getPosition();
            pos.Y -= 12;
            pet.setPos(pos);
            pet.setFh(chr.getMap().getFootholds().findBelow(pet.getPos()).getId());
            pet.setStance(0);
            pet.setSummoned(true);
            chr.addPet(pet);
            chr.getMap().broadcastMessage(chr, PacketCreator.showPet(chr, pet, false, false), true);
            chr.Client.sendPacket(PacketCreator.petStatUpdate(chr));
            chr.Client.sendPacket(PacketCreator.enableActions());

            chr.commitExcludedItems();
            chr.Client.CurrentServerContainer.PetHungerManager.registerPetHunger(chr, chr.getPetIndex(pet));
        }
    }
}
