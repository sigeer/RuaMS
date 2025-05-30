namespace Application.Core.Login.Models
{
    public class ShopModel
    {
        public int ShopId { get; set; }

        public int NpcId { get; set; }

        public ShopItemModel[] Items { get; set; } = [];
    }

    public class ShopItemModel
    {
        public int ItemId { get; set; }

        public int Price { get; set; }

        public int Pitch { get; set; }

        public int Position { get; set; }
        public short Buyable { get; set; }
    }
}
