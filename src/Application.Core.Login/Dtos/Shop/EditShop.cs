using System.ComponentModel.DataAnnotations;

namespace Application.Core.Login.Dtos.Shop
{
    public class CreateShopRequestDto
    {
        public int ShopId { get; set; }
        public int NpcId { get; set; }
    }

    public class EditShopRequestDto
    {
        public int ShopId { get; set; }
        public List<EditShopItemRequestDto> Items { get; set; } = [];
    }
    public class EditShopItemRequestDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        [Range(1_000_000, 9_999_999)]
        public int ItemId { get; set; }
        [Range(1, int.MaxValue)]
        public int Price { get; set; }
        public int Position { get; set; }
    }
}
