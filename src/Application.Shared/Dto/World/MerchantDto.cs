using Application.Shared.Items;

namespace Application.Shared.Dto.World
{
    public class MerchantDto
    {
        public int Meso { get; set; }
        public PlayerShopItemDto[] Items { get; set; }
    }

    public class PlayerShopItemDto
    {
        public short Bundles { get; set; }
        public ItemDto Item { get; set; }
    }
}
