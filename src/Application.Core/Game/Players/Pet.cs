using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Items;
using Application.Core.Game.Maps.AnimatedObjects;
using System.Diagnostics.CodeAnalysis;
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

        public MapPet? GetPetByIndex(int index)
        {
            if (index < 0)
            {
                return null;
            }

            return pets[index];
        }

        public (sbyte PetSlot, MapPet? MapPet) GetPetById(long petId)
        {
            for (sbyte index = 0; index < getPets().Length; index++)
            {
                var petObj = pets[index];
                if (petObj?.PetId == petId)
                {
                    return (index, petObj);
                }
            }
            return (-1, null);
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

        public MapPet? SetPet(sbyte slot, Pet? pet)
        {
            if (slot < 0)
                return null;

            if (pet == null)
                return pets[slot] = null;

            return pets[slot] = new MapPet(pet);
        }

        public async Task SummonPet(Pet? petItem, sbyte petSlot = -1, bool isEvolve = false)
        {
            var oldPet = GetPetByIndex(petSlot);
            if (oldPet != null)
            {
                await MapModel.RemoveMapObject(oldPet, mapChr => mapChr.SendPacket(oldPet.EncodeHidePet(petSlot, 0)));
            }

            if (petItem != null)
            {
                var mapPet = petSlot == -1 ? addPet(petItem) : SetPet(petSlot, petItem);
                if (mapPet != null)
                {
                    Point pos;
                    if (oldPet == null)
                    {
                        pos = getPosition();
                        pos.Y -= 12;
                    }
                    else
                    {
                        pos = oldPet.getPosition();
                    }
                    mapPet.setPosition(pos);
                    mapPet.setStance(oldPet?.getStance() ?? 0);

                    await MapModel.AddMapObject(mapPet, c => mapPet.sendSpawnData(c));

                    if (isEvolve)
                    {
                        await SendPacket(EffectPacket.PetEvolution(petSlot));
                        await BroadcastMap(EffectPacket.ForeignPetEvolution(Id, petSlot), Id);
                    }
                }
            }

            await CommitExcludedItemsAll();

            await SendPacket(PacketCreator.petStatUpdate(this));
            // await SendPacket(PacketCreator.enableActions());
        }


        public void removePet(long petId, bool shift_left)
        {
            sbyte slot = -1;
            for (sbyte i = 0; i < 3; i++)
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
                    for (sbyte i = slot; i < 3; i++)
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

        public async Task CommitExcludedItemsAll()
        {
            for (sbyte i = 0; i < getPets().Length; i++)
            {
                await CommitExcludedItems(i);
            }
        }

        public async Task CommitExcludedItems(sbyte petSlot)
        {
            var petObj = GetPetByIndex(petSlot);
            if (petObj != null)
            {
                var list = petObj.ExcludeItems.ToList();
                await SendPacket(PacketCreator.loadExceptionList(Id, petObj.PetId, petSlot, list));
            }
        }

        public async Task ExportExcludedItems(Player another)
        {
            for (sbyte petSlot = 0; petSlot < another.getPets().Length; petSlot++)
            {
                var petObj = another.GetPetByIndex(petSlot);
                if (petObj != null)
                {
                    var list = petObj.ExcludeItems.ToList();
                    await SendPacket(PacketCreator.loadExceptionList(another.Id, petObj.PetId, petSlot, list));
                }
            }
        }
    }
}
