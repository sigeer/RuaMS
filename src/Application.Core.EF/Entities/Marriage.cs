namespace Application.EF.Entities;

public class MarriageEntity
{
    protected MarriageEntity(){}
    public MarriageEntity(int marriageid, int husbandid, int wifeid, int status, DateTimeOffset time0, DateTimeOffset? time1, DateTimeOffset? time2, int ringSourceId)
    {
        Marriageid = marriageid;
        Husbandid = husbandid;
        Wifeid = wifeid;
        Status = status;
        Time0 = time0;
        Time1 = time1;
        Time2 = time2;
        RingSourceId = ringSourceId;
    }

    public int Marriageid { get; set; }

    public int Husbandid { get; set; }

    public int Wifeid { get; set; }

    /// <summary>
    /// 0. 订婚 1. 结婚 2. 离婚
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 订婚时间
    /// </summary>
    public DateTimeOffset Time0 { get; set; }
    /// <summary>
    /// 结婚时间
    /// </summary>
    public DateTimeOffset? Time1 { get; set; }
    /// <summary>
    /// 离婚时间
    /// </summary>
    public DateTimeOffset? Time2 { get; set; }
    /// <summary>
    /// 结婚时获取
    /// </summary>
    public int RingSourceId { get; set; }
}
