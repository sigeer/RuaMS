using Application.Core.Game.Trades;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerHiredMerchantStatus PlayerHiredMerchantStatus { get; set; }
        public IPlayerShop? VisitingShop { get; set; }

        public void LeaveVisitingShop()
        {
            if (VisitingShop == null)
                return;

            if (VisitingShop.IsOwner(this))
            {
                if (VisitingShop is not HiredMerchant hm)
                    Client.CurrentServer.PlayerShopManager.CloseByPlayer(this);
                else
                    hm.OwnerLeave(this);
            }
            else
                VisitingShop.RemoveVisitor(this);
            VisitingShop = null;
        }
    }
}
