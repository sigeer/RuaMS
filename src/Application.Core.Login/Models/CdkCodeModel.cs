using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class CdkCodeModel
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public long Expiration { get; set; }
        public List<CdkItemModel> Items { get; set; } = [];
        public int MaxCount { get; set; }
    }
    public class CdkItemModel
    {
        public int ItemId { get; set; }

        public int Quantity { get; set; }
    }

    public class CdkRecordModel
    {
        public int Id { get; set; }
        public int CodeId { get; set; }
        public int RecipientId { get; set; }
        public DateTimeOffset RecipientTime { get; set; }
    }
}
