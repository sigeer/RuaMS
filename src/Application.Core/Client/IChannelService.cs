
using client.inventory;
using server;

namespace Application.Core.Client
{
    public interface IChannelService
    {
        Item GenerateCouponItem(int itemId, short quantity);
        void RemovePlayerIncomingInvites(int id);
        void SaveChar(Player player);
        Item CashItem2Item(CashShop.CashItem cashItem);
    }
}
