using Application.Core.EF;

namespace Application.EF;

public partial class CdkCodeEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    /// <summary>
    /// 过期时间：-1 永久
    /// </summary>
    public long Expiration { get; set; }
    /// <summary>
    /// 可领取次数 -1：无限次（但是也仅限每个玩家一次）1.（能被MaxCount个玩家领取）
    /// </summary>
    public int MaxCount { get; set; }
}

public partial class CdkItemEntity
{
    private CdkItemEntity() { }
    public CdkItemEntity(int codeid, int type, int item, int quantity)
    {
        CodeId = codeid;
        Type = type;
        ItemId = item;
        Quantity = quantity;
    }

    public int Id { get; set; }

    public int CodeId { get; set; }
    /// <summary>
    /// 0.meso 1.maplepoint 2.nxPrepaid 3. ？ 4.nxcredit other. itemid
    /// </summary>
    public int Type { get; set; }
    public int ItemId { get; set; }

    public int Quantity { get; set; }
}

public class CdkRecordEntity : IKeyedEntity<int>
{
    private CdkRecordEntity() { }
    public CdkRecordEntity(int id, int codeId, int recipientId, DateTimeOffset recipientTime)
    {
        Id = id;
        CodeId = codeId;
        RecipientId = recipientId;
        RecipientTime = recipientTime;
    }

    public int Id { get; set; }
    public int CodeId { get; set; }
    public int RecipientId { get; set; }
    public DateTimeOffset RecipientTime { get; set; }
}