using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
