namespace Application.Shared.Items
{
    public class CouponBuffEntry
    {
        public CouponBuffEntry(int count, int rate)
        {
            Count = count;
            Rate = rate;
        }

        public int Count { get; set; }
        public int Rate { get; set; }
    }
}
