using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class CdkCodeModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public long Expiration { get; set; }
        public List<CdkItemModel> Items { get; set; } = [];
        public int MaxCount { get; set; }
        public List<CdkRecordModel> Histories { get; set; } = [];
    }
    public class CdkItemModel
    {
        public int Type { get; set; }

        public int ItemId { get; set; }

        public int Quantity { get; set; }
    }

    public class CdkRecordModel
    {
        public int RecipientId { get; set; }
        public DateTimeOffset RecipientTime { get; set; }
    }
}
