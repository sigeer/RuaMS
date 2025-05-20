using Application.Shared.Items;

namespace Application.Shared.Characters
{
    public class CashShopDto
    {
        public int FactoryType { get; set; }
        public ItemDto[] Items { get; set; }
        public int[] WishItems { get; set; }
    }
}
