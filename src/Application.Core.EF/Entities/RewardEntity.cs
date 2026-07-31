using Application.Core.EF;

namespace Application.EF;

public partial class RewardEntity
{
    private RewardEntity() { }
    public RewardEntity(string? title, string? desc, string? code, DateTimeOffset startTime, DateTimeOffset? endTime, int maxCount, bool isAccountOnce)
    {
        Title = title;
        Description = desc;
        Code = code;
        StartTime = startTime;
        EndTime = endTime;
        MaxCount = maxCount;
        AccountOnce = isAccountOnce;
    }

    public int Id { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// 有值时用于CDK兑换
    /// </summary>
    public string? Code { get; set; }
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>
    /// null 永久
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 可领取次数 -1：无限次（但是也仅限每个玩家一次）1.（能被MaxCount个玩家领取）
    /// </summary>
    public int MaxCount { get; set; } = -1;
    /// <summary>
    /// true: 每个账号限1次，false: 每个角色限1次
    /// </summary>
    public bool AccountOnce { get; set; }
    public bool IsDeleted { get; set; }
}

public partial class RewardItemEntity
{
    private RewardItemEntity() { }
    public RewardItemEntity(int codeid, int item, int quantity)
    {
        CodeId = codeid;
        ItemId = item;
        Quantity = quantity;
    }

    public int Id { get; set; }

    public int CodeId { get; set; }
    /// <summary>
    /// 0.meso -1.maplepoint -2.nxPrepaid -3.nxcredit. itemid
    /// </summary>
    public int ItemId { get; set; }

    public int Quantity { get; set; }
}

public class RewardRecordEntity : IKeyedEntity<int>
{
    private RewardRecordEntity() { }
    public RewardRecordEntity(int id, int codeId, int recipientId, DateTimeOffset recipientTime)
    {
        Id = id;
        CodeId = codeId;
        RecipientId = recipientId;
        RecipientTime = recipientTime;
    }

    public int Id { get; set; }
    public int CodeId { get; set; }
    public int RecipientId { get; set; }
    public DateTimeOffset RecipientTime { get; set; } = DateTimeOffset.UtcNow;
}