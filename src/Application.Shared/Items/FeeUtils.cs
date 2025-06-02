namespace Application.Shared.Items
{
    public class FeeUtils
    {
        public static int GetFee(long meso)
        {
            long fee = 0;
            if (meso >= 10000_0000)
            {
                fee = meso * 6 / 100;
            }
            else if (meso >= 2500_0000)
            {
                fee = meso * 5 / 100;
            }
            else if (meso >= 1000_0000)
            {
                fee = meso * 4 / 100;
            }
            else if (meso >= 500_0000)
            {
                fee = meso * 3 / 100;
            }
            else if (meso >= 100_0000)
            {
                fee = meso * 18 / 1000;
            }
            else if (meso >= 10_0000)
            {
                fee = meso * 8 / 1000;
            }
            return (int)fee;
        }
    }
}
