namespace Application.Templates.Item
{
    public class MesoTemplate : AbstractItemTemplate
    {
        public MesoTemplate() : base(0)
        {
            AccountSharable = true;
            TradeAvailable = true;

            TradeBlock = false;
            Cash = false;
            ExpireOnLogout = false;
            Quest = false;
            SlotMax = int.MaxValue;
            Price = 1;
            Time = int.MaxValue;
            TimeLimited = false;
            Only = false;
        }
    }
}
