namespace Application.Shared.Items
{
    public class SpecialCashItem
    {
        private int sn;
        private int modifier;
        private byte info; //?

        public SpecialCashItem(int sn, int modifier, byte info)
        {
            this.sn = sn;
            this.modifier = modifier;
            this.info = info;
        }

        public int getSN()
        {
            return sn;
        }

        public int getModifier()
        {
            return modifier;
        }

        public byte getInfo()
        {
            return info;
        }
    }
}
