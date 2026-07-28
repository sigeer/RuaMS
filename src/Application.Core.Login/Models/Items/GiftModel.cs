namespace Application.Core.Login.Models.Items
{
    public class GiftModel
    {
        public int Id { get; set; }

        public int ToId { get; set; }

        public int FromId { get; set; }

        public string Message { get; set; } = "";

        public int Sn { get; set; }

        public int RingSourceId { get; set; }
        public DateTimeOffset? ClaimTime { get; set; }
    }
}
