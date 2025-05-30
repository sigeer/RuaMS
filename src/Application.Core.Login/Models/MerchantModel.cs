namespace Application.Core.Login.Models
{
    public class MerchantModel
    {
        public int Meso { get; set; }
        public PlayerShopItemModel[] Items { get; set; } = [];
    }

    public class PlayerShopItemModel
    {
        public short Bundles { get; set; }
        public ItemModel Item { get; set; }
    }
}
