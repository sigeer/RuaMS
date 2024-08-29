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


using client.inventory;
using client.inventory.manipulator;
using constants.id;
using provider;
using provider.wz;
using tools;

namespace client.processor.action;



/**
 * @author RonanLana - just added locking on OdinMS' SpawnPetHandler method body
 */
public class SpawnPetProcessor
{
    private static DataProvider dataRoot = DataProviderFactory.getDataProvider(WZFiles.ITEM);

    public static void processSpawnPet(IClient c, byte slot, bool lead)
    {
        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;
                var pet = chr.getInventory(InventoryType.CASH).getItem(slot)?.getPet();
                if (pet == null)
                {
                    return;
                }

                int petid = pet.getItemId();
                if (petid == ItemId.DRAGON_PET || petid == ItemId.ROBO_PET)
                {
                    if (chr.haveItem(petid + 1))
                    {
                        chr.dropMessage(5, "You can't hatch your " + (petid == ItemId.DRAGON_PET ? "Dragon egg" : "Robo egg") + " if you already have a Baby " + (petid == ItemId.DRAGON_PET ? "Dragon." : "Robo."));
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                    else
                    {
                        int evolveid = DataTool.getInt("info/evol1", dataRoot.getData("Pet/" + petid + ".img"));
                        int petId = Pet.createPet(evolveid);
                        if (petId == -1)
                        {
                            return;
                        }
                        long expiration = chr.getInventory(InventoryType.CASH).getItem(slot).getExpiration();
                        InventoryManipulator.removeById(c, InventoryType.CASH, petid, 1, false, false);
                        InventoryManipulator.addById(c, evolveid, 1, null, petId, expiration: expiration);

                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                }
                if (chr.getPetIndex(pet) != -1)
                {
                    chr.unequipPet(pet, true);
                }
                else
                {
                    if (chr.getSkillLevel(SkillFactory.GetSkillTrust(8)) == 0 && chr.getPet(0) != null)
                    {
                        chr.unequipPet(chr.getPet(0)!, false);
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
                    pet.saveToDb();
                    chr.addPet(pet);
                    chr.getMap().broadcastMessage(c.getPlayer(), PacketCreator.showPet(chr, pet, false, false), true);
                    c.sendPacket(PacketCreator.petStatUpdate(chr));
                    c.sendPacket(PacketCreator.enableActions());

                    chr.commitExcludedItems();
                    chr.getClient().getWorldServer().registerPetHunger(chr, chr.getPetIndex(pet));
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
