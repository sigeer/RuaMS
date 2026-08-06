using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.MTS.Channel
{
    public interface IChannelTransport
    {
        ProtoService.BuyResponse Buy(ProtoService.BuyRequest buyRequest);
        ProtoService.ChangePageResponse ChangePage(ProtoService.ChangePageRequest changePageRequest);
        ProtoService.SaleItemResponse SaleItem(ProtoService.SaleItemRequest saleItemRequest);
        ProtoService.AddItemToCartResponse SendAddCartItem(ProtoService.AddItemToCartRequest addItemToCartRequest);
        ProtoService.CancelSaleItemResponse SendCancelSale(ProtoService.CancelSaleItemRequest cancelSaleItemRequest);
        ProtoService.RemoveItemFromCartResponse SendRemoveCartItem(ProtoService.RemoveItemFromCartRequest removeItemFromCartRequest);
        ProtoService.TakeItemResponse TakeItem(ProtoService.TakeItemRequest takeItemRequest);
    }
}
