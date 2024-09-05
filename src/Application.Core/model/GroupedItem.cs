using client.inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.model
{
    public record GroupedItem(ItemInventoryType ItemInventoryType, List<int> GroupQuantity)
    {
        
    }
}
