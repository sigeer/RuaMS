using Application.Core.ServerTransports;
using Application.Shared.Items;
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
        public CreateItemTransactionRequest BeginTransaction(IPlayer chr, List<Item> items, int meso = 0)
        {
            foreach (var item in items)
            {
                chr.RemoveItemById(item.getInventoryType(), item.getItemId(), item.getQuantity());
            }
            chr.gainMeso(-meso, show: false);

            var request = new CreateItemTransactionRequest();
            request.PlayerId = chr.Id;
            request.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(items));
            request.Meso = meso;
            return request;
        }

        public void HandleTransaction(ItemTransaction transaction)
        {
            var chr = _server.FindPlayerById(transaction.PlayerId);
            if (chr != null && chr.IsOnlined)
            {
                var status = (ItemTransactionStatus)transaction.Status;
                if (status == ItemTransactionStatus.PendingForRollback)
                {
                    _itemDistributor.Distribute(chr, _mapper.Map<List<Item>>(transaction.Items), transaction.Meso, "系统消耗失败返还");
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
                        _itemDistributor.Distribute(chr, _mapper.Map<List<Item>>(transaction.Items), transaction.Meso, "系统消耗失败返还");
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
