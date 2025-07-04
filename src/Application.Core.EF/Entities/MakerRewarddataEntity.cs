namespace Application.EF.Entities;

public partial class MakerRewardDataEntity
{
    public int Itemid { get; set; }

    public int Rewardid { get; set; }

    public short Quantity { get; set; }

    public sbyte Prob { get; set; }
}
