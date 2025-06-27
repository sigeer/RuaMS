using client.inventory;
using System.Collections.Concurrent;

namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 跨服道具事务缓冲区
    /// </summary>
    public class PendingItemStore
    {
        // 事务ID -> 事务数据
        private readonly ConcurrentDictionary<Guid, PendingItemTransaction> _transactions = new();

        /// <summary>
        /// 添加事务
        /// </summary>
        public bool TryAdd(Guid transactionId, PendingItemTransaction transaction)
        {
            return _transactions.TryAdd(transactionId, transaction);
        }

        /// <summary>
        /// 完成事务
        /// </summary>
        public bool TryRemove(Guid transactionId, out PendingItemTransaction transaction)
        {
            return _transactions.TryRemove(transactionId, out transaction);
        }

        /// <summary>
        /// 获取指定玩家的所有事务
        /// </summary>
        public IEnumerable<PendingItemTransaction> GetPlayerTransactions(long playerId)
        {
            return _transactions.Values.Where(t => t.PlayerId == playerId);
        }

        /// <summary>
        /// 检查玩家是否可以切频道（存在挂起事务则禁止）
        /// </summary>
        public bool CanSwitchChannel(long playerId)
        {
            return !GetPlayerTransactions(playerId).Any();
        }

        /// <summary>
        /// 提交事务处理失败，执行回滚
        /// </summary>
        public void RollbackTransaction(PendingItemTransaction transaction)
        {
            // 退还道具逻辑，具体实现根据项目定制
            transaction.Status = TransactionStatus.Rollback;
            // TODO: 退还道具给玩家
        }

        /// <summary>
        /// 重试事务（用于发放失败或补偿）
        /// </summary>
        public void RetryTransaction(PendingItemTransaction transaction)
        {
            // TODO: 重新尝试发放或提交
        }

        /// <summary>
        /// 定期上报长时间未完成的事务
        /// </summary>
        public IEnumerable<PendingItemTransaction> GetStaleTransactions(TimeSpan maxPendingTime)
        {
            var cutoff = DateTime.UtcNow - maxPendingTime;
            return _transactions.Values.Where(t => t.CreatedAt <= cutoff);
        }
    }

    /// <summary>
    /// 单个道具事务
    /// </summary>
    public class PendingItemTransaction
    {
        public Guid TransactionId { get; init; }
        public long PlayerId { get; init; }
        public List<Item> Item { get; init; }
        public int Meso { get; set; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string OperationType { get; init; } // Consume / Grant
    }

    /// <summary>
    /// 道具事务状态
    /// </summary>
    public enum TransactionStatus
    {
        Pending,
        Success,
        Failed,
        Rollback
    }
}
