using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Services;
using Application.Core.Game.Players;
using Application.Module.MTS.Channel.Net;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using MTSProto;
using tools;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Module.MTS.Channel
{
    public class MTSManager
    {
        readonly IChannelTransport _transport;
        readonly ILogger<MTSManager> _logger;
        readonly IMapper _mapper;
        readonly ItemTransactionService _itemTransaction;
        readonly WorldChannelServer _server;

        public MTSManager(IChannelTransport transport, ILogger<MTSManager> logger, IMapper mapper, ItemTransactionService itemTransaction, WorldChannelServer server)
        {
            _transport = transport;
            _logger = logger;
            _mapper = mapper;
            _itemTransaction = itemTransaction;
            _server = server;
        }

        public void Query(IPlayer chr, int tab, int type, int page)
        {
            chr.changePage(page);

            if (tab != chr.getCurrentTab() || type != chr.getCurrentType())
            {
                chr.setSearch(null);
            }
            var request = new MTSProto.ChangePageRequest
            {
                MasterId = chr.Id,
                Tab = tab,
                Type = type,
                Page = page,
                SearchType = chr.getCurrentCI(),
                SearchText = chr.getSearch(),
            };
            if (request.SearchType != 0 && !string.IsNullOrEmpty(chr.getSearch()))
            {
                request.FilterItemId.AddRange(ItemInformationProvider.getInstance().getAllItems().Where(itemPair => itemPair.Name.Contains(chr.getSearch()!, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id));
            }
            MTSProto.ChangePageResponse res = _transport.ChangePage(request);

            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.Items),
                res.Tab, res.Type, res.Page, res.TotalPages));

            chr.changeTab(tab);
            chr.changeType(type);

            chr.Client.enableCSActions();
            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.InTransfer)));
            chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.OnSale)));
        }


        public void Search(IPlayer chr, int tab, int type, int searchType, string? searchText)
        {
            chr.changeCI(searchType);
            chr.setSearch(searchText);
            chr.changeTab(tab);
            chr.changeType(type);

            Query(chr, tab, type, 0);
        }

        private MTSProto.MTSQuery GetQueryModel(IPlayer chr)
        {
            return new MTSQuery { MasterId = chr.Id, Tab = chr.getCurrentTab(), Type = chr.getCurrentType(), Page = chr.getCurrentPage() };
        }


        public void AddItemToSale(IPlayer chr, Item toSale, int price)
        {
            if (!_itemTransaction.TryBeginTransaction(chr, [toSale], 5000, out var transaction))
            {
                return;
            }

            MTSProto.SaleItemResponse res = _transport.SaleItem(new MTSProto.SaleItemRequest
            {
                MasterId = chr.Id,
                Item = _mapper.Map<Dto.ItemDto>(toSale),
                Price = price,
                Transaction = transaction
            });

            if (res.Code == 0)
            {
                chr.sendPacket(MTSPacketCreator.MTSConfirmSell());
                chr.Client.enableCSActions();
            }
            else
            {
                if (res.Code == 1)
                {
                    chr.dropMessage(1, "You already have 10 items up for auction!");
                }
            }

            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.PageData.Items),
                res.PageData.Tab, res.PageData.Type, res.PageData.Page, res.PageData.TotalPages));

            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.InTransfer)));
            chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.OnSale)));

            _itemTransaction.HandleTransaction(res.Transaction);
        }

        public void CancelSaleItem(IPlayer chr, int productId)
        {
            MTSProto.CancelSaleItemResponse data = _transport.SendCancelSale(new MTSProto.CancelSaleItemRequest { MasterId = chr.Id, ProductId = productId });

            chr.Client.enableCSActions();
            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(data.PageData.Items),
                data.PageData.Tab, data.PageData.Type, data.PageData.Page, data.PageData.TotalPages));
            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.InTransfer)));
            chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.OnSale)));
        }


        public void AddCartItem(IPlayer chr, int productId)
        {
            MTSProto.AddItemToCartResponse data = _transport.SendAddCartItem(new AddItemToCartRequest { Query = GetQueryModel(chr), ProductId = productId });

            chr.Client.enableCSActions();
            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(data.PageData.Items),
                data.PageData.Tab, data.PageData.Type, data.PageData.Page, data.PageData.TotalPages));
            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.InTransfer)));
            chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.OnSale)));
        }

        public void RemoveCartItem(IPlayer chr, int productId)
        {
            var query = GetQueryModel(chr);
            query.Tab = 4;
            query.Type = 0;
            query.Page = 0;
            MTSProto.RemoveItemFromCartResponse data = _transport.SendRemoveCartItem(new RemoveItemFromCartRequest { Query = GetQueryModel(chr), ProductId = productId });

            chr.Client.enableCSActions();
            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(data.PageData.Items),
                data.PageData.Tab, data.PageData.Type, data.PageData.Page, data.PageData.TotalPages));
            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.InTransfer)));
            chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(data.MyMTSInfo.OnSale)));
        }

        public void TakeItemFromTransferInv(IPlayer chr, int productId)
        {
            TakeItemResponse res = _transport.TakeItem(new TakeItemRequest { Query = GetQueryModel(chr), ProductId = productId });

            var item = _mapper.Map<Item>(res.Item);
            InventoryManipulator.addFromDrop(chr.Client, item, false);
            chr.Client.enableCSActions();
            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.CartItems.Items),
                res.CartItems.Tab, res.CartItems.Type, res.CartItems.Page, res.CartItems.TotalPages));
            chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.PageData.Items),
                res.PageData.Tab, res.PageData.Type, res.PageData.Page, res.PageData.TotalPages));
            chr.sendPacket(MTSPacketCreator.MTSConfirmTransfer(item.getQuantity(), item.getPosition()));
            chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.InTransfer)));
        }

        public void Buy(IPlayer chr, int productId, bool fromCart)
        {
            BuyResponse res = _transport.Buy(new BuyRequest { Query = GetQueryModel(chr), ProductId = productId });

            if (res.Code == 0)
            {
                chr.CashShopModel.NxPrepaid = res.UpdatedValue;
                chr.Client.enableCSActions();
                if (fromCart)
                    chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.CartItems.Items),
                        res.CartItems.Tab, res.CartItems.Type, res.CartItems.Page, res.CartItems.TotalPages));
                else
                    chr.sendPacket(MTSPacketCreator.sendMTS(_mapper.Map<List<MTSItemInfo>>(res.PageData.Items),
                        res.PageData.Tab, res.PageData.Type, res.PageData.Page, res.PageData.TotalPages));
                chr.sendPacket(MTSPacketCreator.MTSConfirmBuy());
                chr.sendPacket(MTSPacketCreator.showMTSCash(chr));
                chr.sendPacket(MTSPacketCreator.transferInventory(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.InTransfer)));
                chr.sendPacket(MTSPacketCreator.notYetSoldInv(_mapper.Map<List<MTSItemInfo>>(res.MyMTSInfo.OnSale)));
                chr.sendPacket(PacketCreator.enableActions());
            }
            else
            {
                chr.sendPacket(MTSPacketCreator.MTSFailBuy());
            }

        }
    }
}
