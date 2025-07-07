using Microsoft.Extensions.Logging;
using MTSProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.MTS.Channel
{
    public interface IChannelTransport
    {
        BuyResponse Buy(BuyRequest buyRequest);
        ChangePageResponse ChangePage(ChangePageRequest changePageRequest);
        SaleItemResponse SaleItem(SaleItemRequest saleItemRequest);
        AddItemToCartResponse SendAddCartItem(AddItemToCartRequest addItemToCartRequest);
        CancelSaleItemResponse SendCancelSale(CancelSaleItemRequest cancelSaleItemRequest);
        RemoveItemFromCartResponse SendRemoveCartItem(RemoveItemFromCartRequest removeItemFromCartRequest);
        TakeItemResponse TakeItem(TakeItemRequest takeItemRequest);
    }
}
