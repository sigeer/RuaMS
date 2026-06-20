using Application.Core.Game.Trades;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerHiredMerchantStatus PlayerHiredMerchantStatus { get; set; }
        public IPlayerShop? VisitingShop { get; set; }

        public async Task LeaveVisitingShop()
        {
            if (VisitingShop == null)
                return;

            if (VisitingShop.IsOwner(this))
            {
                if (VisitingShop is not HiredMerchant hm)
                    await Client.CurrentServer.PlayerShopManager.CloseByPlayer(this);
                else
                    hm.OwnerLeave(this);
            }
            else
                await VisitingShop.RemoveVisitor(this);
            VisitingShop = null;
        }
    }
}
