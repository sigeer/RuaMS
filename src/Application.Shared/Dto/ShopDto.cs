using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Dto
{
    public class ShopDto
    {
        public int ShopId { get; set; }

        public int NpcId { get; set; }

        public ShopItemDto[] Items { get; set; } = [];
    }

    public class ShopItemDto
    {
        public int ItemId { get; set; }

        public int Price { get; set; }

        public int Pitch { get; set; }

        public int Position { get; set; }
        public short Buyable { get; set; }
    }
}
