using Application.Shared.Items;

namespace Application.Shared.Characters
{
    public class CashShopDto
    {
        public int NxCredit { get; set; }
        public int MaplePoint { get; set; }
        public int NxPrepaid { get; set; }
        public int FactoryType { get; set; }
        public ItemDto[] Items { get; set; }
        public int[] WishItems { get; set; }
    }
}
