namespace Application.EF;

public partial class CdkCodeEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public long Expiration { get; set; }
    /// <summary>
    /// 可领取次数 -1：无限次（但是也仅限每个玩家一次）1.（能被10个玩家领取）
    /// </summary>
    public int MaxCount { get; set; }
}

public class CdkRecordEntity
{
    private CdkRecordEntity() { }
    public CdkRecordEntity(int codeId, int recipientId, DateTimeOffset recipientTime)
    {
        CodeId = codeId;
        RecipientId = recipientId;
        RecipientTime = recipientTime;
    }

    public int Id { get; set; }
    public int CodeId { get; set; }
    public int RecipientId { get; set; }
    public DateTimeOffset RecipientTime { get; set; }
}