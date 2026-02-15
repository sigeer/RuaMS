using Application.Shared.Items;

namespace Application.Core.Login.Models
{
    public class ItemModel
    {
        public byte Type { get; set; }

        public int? Characterid { get; set; }

        public int? Accountid { get; set; }

        public int Itemid { get; set; }

        public sbyte InventoryType { get; set; }

        public short Position { get; set; }

        public short Quantity { get; set; }

        public string? Owner { get; set; }

        public short Flag { get; set; }

        public long Expiration { get; set; }

        public string? GiftFrom { get; set; }

        public EquipModel? EquipInfo { get; set; }
        public PetModel? PetInfo { get; set; }

        public static ItemModel NewEtcItem(int itemId, short quantity)
        {
            return new ItemModel
            {
                Type = (int)ItemType.Inventory,
                InventoryType = (int)Application.Shared.Constants.Inventory.InventoryType.ETC,
                Itemid = itemId,
                Quantity = quantity,
                Expiration = -1
            };
        }
    }
}
