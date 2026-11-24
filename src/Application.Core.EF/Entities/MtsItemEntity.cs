using Application.Core.EF;

namespace Application.EF.Entities;

public partial class MtsItemEntity : IKeyedEntity<int>
{
    public MtsItemEntity(int tab, int type, int seller, int price, long releaseDate, long sellEnds, byte[] itemBlob)
    {
        Tab = tab;
        Type = type;
        Seller = seller;
        Price = price;
        ReleaseDate = releaseDate;
        SellEnds = sellEnds;
        ItemBlob = itemBlob;
    }

    protected MtsItemEntity()
    {
    }

    public int Id { get; set; }

    public int Tab { get; set; }

    public int Type { get; set; }

    public int Seller { get; set; }

    public int Price { get; set; }

    public int BidIncre { get; set; }

    /// <summary>
    /// 拍卖的一口价
    /// </summary>
    public int BuyNow { get; set; }

    /// <summary>
    /// 拍卖：当前最高出价者（masterId）。到期结算与“不能连续出价”都靠它。
    /// </summary>
    public int Bidder { get; set; }

    /// <summary>
    /// 拍卖：出价人数。客户端列表按 WORD 读这一项的对应封包字段，这里只累计成功出价的次数。
    /// </summary>
    public int BidCount { get; set; }

    /// <summary>
    /// 上架结束时间（unix 毫秒）
    /// </summary>
    public long SellEnds { get; set; }
    /// <summary>
    /// 上架时间
    /// </summary>
    public long ReleaseDate { get; set; }

    public int Transfer { get; set; }

    public byte[]? ItemBlob { get; set; }
}
