using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Dtos.Shop
{
    public class ShopDetailDto : ShopResponseDto
    {
        public List<ShopItemResponseDto> Items { get; set; } = [];
    }
}
