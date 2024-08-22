namespace Application.EF.Entities;

public partial class Nxcoupon
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public int Rate { get; set; }

    public int Activeday { get; set; }

    public int Starthour { get; set; }

    public int Endhour { get; set; }
}
