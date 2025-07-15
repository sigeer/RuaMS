using Application.Shared.Items;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class ItemTransactionManager
    {
        ConcurrentDictionary<long, ItemProto.ItemTransaction> _dataSource = [];

        readonly IMapper _mapper;
        readonly MasterServer _server;
        readonly ILogger<ItemTransactionManager> _logger;

        public ItemTransactionManager(IMapper mapper, MasterServer server, ILogger<ItemTransactionManager> logger)
        {
            _mapper = mapper;
            _server = server;
            _logger = logger;
        }

        public ItemProto.ItemTransaction CreateTransaction(ItemProto.CreateItemTransactionRequest request, ItemTransactionStatus status)
        {
            var tsc = new ItemProto.ItemTransaction()
            {
                PlayerId = request.PlayerId,
                Status = (int)status,
                TransactionId = Yitter.IdGenerator.YitIdHelper.NextId(),
                Meso = request.Meso,
                CashType = request.CashType,
                CashValue = request.CashValue,
            };
            tsc.Items.AddRange(request.Items);
            _dataSource.TryAdd(tsc.TransactionId, tsc);
            return tsc;
        }

        public void Finish(ItemProto.FinishTransactionRequest request)
        {
            _dataSource.TryRemove(request.TransactionId, out _);
        }

        public List<ItemProto.ItemTransaction> GetPlayerPendingTransactions(int chrId)
        {
            return _dataSource.Values.Where(x => x.PlayerId == chrId).ToList();
        }
    }
}
