using Application.Core.Game.Items;
using client.inventory;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private Lock petLock = new ();
        private Pet?[] pets = new Pet?[3];
        public Pet?[] getPets()
        {
            petLock.Enter();
            try
            {
                return Arrays.copyOf(pets, pets.Length);
            }
            finally
            {
                petLock.Exit();
            }
        }

        public Pet? getPet(int index)
        {
            if (index < 0)
            {
                return null;
            }

            petLock.Enter();
            try
            {
                return pets[index];
            }
            finally
            {
                petLock.Exit();
            }
        }

        public sbyte getPetIndex(long petId)
        {
            petLock.Enter();
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
                petLock.Exit();
            }
        }

        public sbyte getPetIndex(Pet pet)
        {
            return getPetIndex(pet.PetId);
        }
        public void addPet(Pet pet)
        {
            petLock.Enter();
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
                petLock.Exit();
            }
        }

        public void removePet(Pet pet, bool shift_left)
        {
            petLock.Enter();
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
                petLock.Exit();
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

            Client.CurrentServerContainer.PetHungerManager.unregisterPetHunger(this, petIdx);
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

    }
}
