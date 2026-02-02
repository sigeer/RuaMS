using Application.Core.Game.Items;
using client.inventory;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private Pet?[] pets = new Pet?[3];
        public Pet?[] getPets()
        {
            return Arrays.copyOf(pets, pets.Length);
        }

        public Pet? getPet(int index)
        {
            if (index < 0)
            {
                return null;
            }

            return pets[index];
        }

        public sbyte getPetIndex(long petId)
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == petId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public sbyte getPetIndexByItemId(int itemId)
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getItemId() == itemId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public sbyte getPetIndex(Pet pet)
        {
            return getPetIndex(pet.PetId);
        }
        public void addPet(Pet pet)
        {
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] == null)
                {
                    pets[i] = pet;
                    return;
                }
            }
        }

        public void removePet(Pet pet, bool shift_left)
        {
            int slot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == pet.getUniqueId())
                    {
                        pets[i] = null;
                        slot = i;
                        break;
                    }
                }
            }
            if (shift_left)
            {
                if (slot > -1)
                {
                    for (int i = slot; i < 3; i++)
                    {
                        if (i != 2)
                        {
                            pets[i] = pets[i + 1];
                        }
                        else
                        {
                            pets[i] = null;
                        }
                    }
                }
            }
        }

        public void unequipAllPets()
        {
            for (int i = 0; i < 3; i++)
            {
                var pet = getPet(i);
                if (pet != null)
                {
                    unequipPet(pet, true);
                }
            }
        }

        public void unequipPet(Pet pet, bool shift_left, bool hunger = false)
        {
            sbyte petIdx = this.getPetIndex(pet);
            Pet? chrPet = this.getPet(petIdx);

            if (chrPet != null)
            {
                chrPet.setSummoned(false);
            }

            Client.CurrentServer.PetHungerManager.unregisterPetHunger(this, petIdx);
            MapModel.broadcastMessage(this, PacketCreator.showPet(this, pet, true, hunger), true);

            removePet(pet, shift_left);
            commitExcludedItems();

            sendPacket(PacketCreator.petStatUpdate(this));
            sendPacket(PacketCreator.enableActions());
        }

        public void runFullnessSchedule(int petSlot)
        {
            Pet? pet = getPet(petSlot);
            if (pet == null)
            {
                return;
            }

            int newFullness = pet.Fullness - pet.SourceTemplate.Hungry;
            if (newFullness <= 5)
            {
                pet.Fullness = 15;
                unequipPet(pet, true);
                dropMessage(6, "Your pet grew hungry! Treat it some pet food to keep it healthy!");
            }
            else
            {
                pet.Fullness = newFullness;
                Item? petz = getInventory(InventoryType.CASH).getItem(pet.getPosition());
                if (petz != null)
                {
                    forceUpdateItem(petz);
                }
            }
        }

        public int getNoPets()
        {
            return pets.Count(x => x != null);
        }

        public void shiftPetsRight()
        {
            if (pets[2] == null)
            {
                pets[2] = pets[1];
                pets[1] = pets[0];
                pets[0] = null;
            }
        }

    }
}
