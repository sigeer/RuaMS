using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.EF.Entities.Items
{
    public class ItemEntityPair
    {
        public ItemEntityPair(Inventoryitem item, Inventoryequipment? equip)
        {
            Item = item;
            Equip = equip;
        }

        public Inventoryitem Item { get; set; }
        public Inventoryequipment? Equip { get; set; }
    }
}
