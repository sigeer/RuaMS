using Application.Core.Game.Items;
using client.inventory;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private object petLock = new object();
        private Pet?[] pets = new Pet?[3];
        public Pet?[] getPets()
        {
            Monitor.Enter(petLock);
            try
            {
                return Arrays.copyOf(pets, pets.Length);
            }
            finally
            {
                Monitor.Exit(petLock);
            }
        }

        public Pet? getPet(int index)
        {
            if (index < 0)
            {
                return null;
            }

            Monitor.Enter(petLock);
            try
            {
                return pets[index];
            }
            finally
            {
                Monitor.Exit(petLock);
            }
        }

        public sbyte getPetIndex(int petId)
        {
            Monitor.Enter(petLock);
            try
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
            finally
            {
                Monitor.Exit(petLock);
            }
        }

        public sbyte getPetIndex(Pet pet)
        {
            Monitor.Enter(petLock);
            try
            {
                for (sbyte i = 0; i < 3; i++)
                {
                    if (pets[i] != null)
                    {
                        if (pets[i]!.getUniqueId() == pet.getUniqueId())
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
            finally
            {
                Monitor.Exit(petLock);
            }
        }
        public void addPet(Pet pet)
        {
            Monitor.Enter(petLock);
            try
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
            finally
            {
                Monitor.Exit(petLock);
            }
        }

        public void removePet(Pet pet, bool shift_left)
        {
            Monitor.Enter(petLock);
            try
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
            finally
            {
                Monitor.Exit(petLock);
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
                chrPet.saveToDb();
            }

            this.getClient().getWorldServer().unregisterPetHunger(this, petIdx);
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

            int newFullness = pet.Fullness - PetDataFactory.getHunger(pet.getItemId());
            if (newFullness <= 5)
            {
                pet.Fullness = 15;
                pet.saveToDb();
                unequipPet(pet, true);
                dropMessage(6, "Your pet grew hungry! Treat it some pet food to keep it healthy!");
            }
            else
            {
                pet.Fullness = newFullness;
                pet.saveToDb();
                Item? petz = getInventory(InventoryType.CASH).getItem(pet.getPosition());
                if (petz != null)
                {
                    forceUpdateItem(petz);
                }
            }
        }

    }
}
