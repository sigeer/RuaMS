using Application.Core.ServerTransports;
using AutoMapper;
using client.inventory;
using ItemProto;

namespace Application.Core.Channel.Services
{
    public class ItemTransactionService
    {
        readonly IItemDistributeService _itemDistributor;
        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;

        public ItemTransactionService(IItemDistributeService itemDistribute, IChannelServerTransport transport, IMapper mapper, WorldChannelServer server)
        {
            _itemDistributor = itemDistribute;
            _transport = transport;
            _mapper = mapper;
            _server = server;
        }

        /// <summary>
        /// 添加事务 
        /// </summary>
        public bool TryBuyCash(IPlayer chr, int type, CashItem cashItem, out CreateItemTransactionRequest request)
        {
            bool isSuccess = true;
            request = new CreateItemTransactionRequest();
            request.PlayerId = chr.Id;

            if (!cashItem.isOnSale())
                return false;

            if (chr.getCashShop().BuyCashItem(type, cashItem))
            {
                request.CashType = type;
                request.CashValue = cashItem.getPrice();
            }
            else
            {
                isSuccess = false;
            }

            if (isSuccess)
                return true;
            else
            {
                chr.dropMessage("你没有足够的现金");
                return false;
            }
        }

        /// <summary>
        /// 添加事务 
        /// </summary>
        public bool TryBeginTransaction(IPlayer chr, List<Item> items, int meso, out CreateItemTransactionRequest request)
        {
            bool isSuccess = true;
            request = new CreateItemTransactionRequest();
            request.PlayerId = chr.Id;
            foreach (var item in items)
            {
                var copyItem = item.copy();
                if (item.getPosition() > 0)
                {
                    if (chr.RemoveItemBySlot(item.getInventoryType(), item.getPosition(), item.getQuantity()))
                        request.Items.Add(_mapper.Map<Dto.ItemDto>(copyItem));
                    else
                    {
                        isSuccess = false;
                        break;
                    }
                }
                else
                {
                    if (chr.RemoveItemById(item.getInventoryType(), item.getItemId(), item.getQuantity()))
                        request.Items.Add(_mapper.Map<Dto.ItemDto>(copyItem));
                    else
                    {
                        isSuccess = false;
                        break;
                    }
                }
            }

            if (meso != 0)
            {
                if (chr.getMeso() >= meso)
                {
                    chr.gainMeso(-meso, show: false);
                    request.Meso = meso;
                }
                else
                {
                    isSuccess = false;
                }

            }


            if (isSuccess)
                return true;
            else
            {
                chr.dropMessage("你没有足够的金币或者道具");
                _itemDistributor.Distribute(chr, _mapper.Map<List<Item>>(request.Items), meso, 0, 0, "系统消耗失败返还");
                return false;
            }
        }

        public void HandleTransaction(ItemTransaction transaction)
        {
            var chr = _server.FindPlayerById(transaction.PlayerId);
            if (chr != null && chr.IsOnlined)
            {
                var status = (ItemTransactionStatus)transaction.Status;
                if (status == ItemTransactionStatus.PendingForRollback)
                {
                    _itemDistributor.Distribute(chr, _mapper.Map<List<Item>>(transaction.Items), transaction.Meso, transaction.CashType, transaction.CashValue,
                        "系统消耗失败返还");
                }

                _transport.FinishTransaction(new FinishTransactionRequest
                {
                    TransactionId = transaction.TransactionId,
                });
            }
        }

        public void HandleTransactions(IPlayer chr, IEnumerable<ItemTransaction> transactions)
        {
            if (chr != null && chr.IsOnlined)
            {
                foreach (var transaction in transactions)
                {
                    var status = (ItemTransactionStatus)transaction.Status;
                    if (status == ItemTransactionStatus.PendingForRollback)
                    {
                        _itemDistributor.Distribute(chr, _mapper.Map<List<Item>>(transaction.Items), transaction.Meso, transaction.CashType, transaction.CashValue,
                            "系统消耗失败返还");
                    }

                    _transport.FinishTransaction(new FinishTransactionRequest
                    {
                        TransactionId = transaction.TransactionId,
                    });
                }
            }
        }
    }
}
