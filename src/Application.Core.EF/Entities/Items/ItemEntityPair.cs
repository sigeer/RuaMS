using Application.EF;
using Application.EF.Entities;

namespace Application.Core.EF.Entities.Items
{
    public class ItemEntityPair
    {
        public ItemEntityPair(Inventoryitem item, Inventoryequipment? equip, PetEntity? pet)
        {
            Item = item;
            Equip = equip;
            Pet = pet;
        }

        public Inventoryitem Item { get; set; }
        public Inventoryequipment? Equip { get; set; }
        public PetEntity? Pet { get; set; }
    }
}
