namespace Application.Core.Login.Dtos.Item
{
    public class CreateItemRequestDto
    {
        public int ItemId { get; set; }
        public short Quantity { get; set; }
        public bool SetOwner { get; set; }
        public long Expired { get; set; }
        public int Flag { get; set; }
        public CreateEquipRequestDto? EquipInfo { get; set; }
    }
}
