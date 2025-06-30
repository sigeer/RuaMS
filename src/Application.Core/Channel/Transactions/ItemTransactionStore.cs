using Application.Core.Channel.Services;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using client.inventory;
using System.Collections.Concurrent;

namespace Application.Core.Channel.Transactions
{
    public class ItemTransactionStore
    {

        readonly IItemDistributeService _itemDistributor;
        readonly IChannelServerTransport _transport;

        public ItemTransactionStore(IItemDistributeService itemDistribute, IChannelServerTransport transport)
        {
            _itemDistributor = itemDistribute;
            _transport = transport;
        }

        /// <summary>
        /// 添加事务 
        /// </summary>
        public ItemTransaction BeginTransaction(IPlayer chr, List<Item> items, int meso = 0)
        {
            var transaction = new ItemTransaction();
            foreach (var item in items)
            {
                chr.RemoveItemById(item.getInventoryType(), item.getItemId(), item.getQuantity());
            }
            chr.gainMeso(-meso);
            return transaction;
        }

        public void ProcessTransaction(ItemTransaction transaction)
        {
            switch (transaction.Status)
            {
                case ItemTransactionStatus.Pending:
                    break;
                case ItemTransactionStatus.PendingForCommit:
                    CommitTransaction(transaction);
                    break;
                case ItemTransactionStatus.PendingForRollback:
                    RollbackTransaction(transaction);
                    break;
                case ItemTransactionStatus.Completed:
                    break;
                default:
                    break;
            }

            _transport.FinishTransaction(new ItemDto.FinishTransactionRequest
            {
                TransactionId = transaction.TransactionId,
            });
        }

        void RollbackTransaction(ItemTransaction transaction)
        {
            if (transaction.Player.IsOnlined)
            {
                _itemDistributor.Distribute(transaction.Player, transaction.CostItems, transaction.CostMeso, "系统消耗失败返还");
            }
            else
            {
                transaction.Status = ItemTransactionStatus.PendingForRollback;
            }
        }

        void CommitTransaction(ItemTransaction transaction)
        {
            if (!transaction.Player.IsOnlined)
            {
                transaction.Status = ItemTransactionStatus.PendingForCommit;
            }
        }
    }
}
