using Application.Shared.Characters;

namespace Application.Shared.Items
{
    public class ItemDto
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

        public int Petid { get; set; }

        public short Flag { get; set; }

        public long Expiration { get; set; }

        public string GiftFrom { get; set; } = null!;

        public EquipDto? EquipInfo { get; set; }
        public PetDto? PetInfo { get; set; }
    }
}
