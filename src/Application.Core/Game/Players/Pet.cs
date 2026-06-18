using Application.Core.Game.Items;
using Application.Core.Game.Maps.AnimatedObjects;
using tools;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private MapPet?[] pets = new MapPet?[3];
        public MapPet?[] getPets()
        {
            return Arrays.copyOf(pets, pets.Length);
        }

        public MapPet? getPet(int index)
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

        public MapPet? GetPetById(long petId)
        {
            return pets.AsValueEnumerable().FirstOrDefault(x => x?.getUniqueId() == petId);
        }

        public sbyte getPetIndexByItemId(int itemId)
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i]?.getItemId() == itemId)
                {
                    return i;
                }
            }
            return -1;
        }

        public MapPet? addPet(Pet pet)
        {
            for (sbyte i = 0; i < 3; i++)
            {
                if (pets[i] == null)
                {
                    return pets[i] = new MapPet(pet);
                }
            }
            return null;
        }

        public async Task SummonPet(Pet pet)
        {
            var mapPet = addPet(pet);
            if (mapPet != null)
            {
                await MapModel.AddMapObject(mapPet, c => mapPet.sendSpawnData(c));

                await SendPacket(PacketCreator.petStatUpdate(this));
                await SendPacket(PacketCreator.enableActions());

                await commitExcludedItems();
            }
        }

        public void removePet(long petId, bool shift_left)
        {
            int slot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (pets[i] != null)
                {
                    if (pets[i]!.getUniqueId() == petId)
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

        //public void unequipAllPets()
        //{
        //    for (int i = 0; i < 3; i++)
        //    {
        //        var pet = getPet(i);
        //        if (pet != null)
        //        {
        //            unequipPet(pet, true);
        //        }
        //    }
        //}

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
