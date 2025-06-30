using Application.Core.Login.Models;
using Application.Core.Login.Models.Transactions;
using Application.Shared.Items;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class ItemTransactionManager
    {
        ConcurrentDictionary<long, ItemDto.ItemTransaction> _dataSource = [];

        readonly IMapper _mapper;
        readonly MasterServer _server;
        readonly ILogger<ItemTransactionManager> _logger;

        public ItemTransactionManager(IMapper mapper, MasterServer server, ILogger<ItemTransactionManager> logger)
        {
            _mapper = mapper;
            _server = server;
            _logger = logger;
        }

        public ItemDto.ItemTransaction CreateTransaction(ItemDto.ItemTransaction tsc, ItemTransactionStatus status)
        {
            tsc.TransactionId = Yitter.IdGenerator.YitIdHelper.NextId();
            tsc.Status = (int)status;
            _dataSource.TryAdd(tsc.TransactionId, tsc);
            return tsc;
        }

        public void Finish(ItemDto.FinishTransactionRequest request)
        {
            _dataSource.TryRemove(request.TransactionId, out _);
        }
    }
}
