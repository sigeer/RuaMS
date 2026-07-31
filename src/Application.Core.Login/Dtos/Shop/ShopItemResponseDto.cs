using System.ComponentModel.DataAnnotations;

namespace Application.Core.Login.Dtos.Shop
{
    public class ShopItemResponseDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public int Price { get; set; }
        public int Position { get; set; }
    }
}
