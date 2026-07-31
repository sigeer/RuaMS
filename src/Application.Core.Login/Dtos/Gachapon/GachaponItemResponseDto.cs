namespace Application.Core.Login.Dtos.Gachapon
{
    public class GachaponItemResponseDto
    {
        public int Id { get; set; }
        public int PoolId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public int Level { get; set; }
    }
}
