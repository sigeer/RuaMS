namespace Application.Core.Login.Models
{
    public class ItemModel
    {
        public int InventoryItemId { get; set; }
        public byte Type { get; set; }

        public int? Characterid { get; set; }

        public int? Accountid { get; set; }

        public int Itemid { get; set; }

        public sbyte InventoryType { get; set; }

        public short Position { get; set; }

        public short Quantity { get; set; }

        public string Owner { get; set; } = null!;

        public short Flag { get; set; }

        public long Expiration { get; set; }

        public string GiftFrom { get; set; } = null!;

        public EquipModel? EquipInfo { get; set; }
        public PetModel? PetInfo { get; set; }
    }
}
