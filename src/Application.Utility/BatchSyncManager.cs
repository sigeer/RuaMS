namespace Application.Utility
{
    public class BatchSyncManager<T>
    {
        private readonly object _lock = new();
        private readonly List<T> _batchItems = new();
        private readonly int _delayMs;
        private readonly int _maxBatchSize;
        private readonly Action<List<T>> _processBatch;
        private Task _lastFlushTask = Task.CompletedTask;
        private Timer? _timer;

        public BatchSyncManager(int delayMs, int maxBatchSize, Action<List<T>> processBatchAsync)
        {
            _delayMs = delayMs;
            _maxBatchSize = maxBatchSize;
            _processBatch = processBatchAsync;
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _batchItems.Add(item);

                if (_batchItems.Count >= _maxBatchSize)
                {
                    FlushNow();
                    return;
                }

                if (_timer == null)
                {
                    _timer = new Timer(_ => FlushNow(), null, _delayMs, Timeout.Infinite);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _lastFlushTask = Task.CompletedTask;

                _batchItems.Clear();

                _timer?.Dispose();
                _timer = null;
            }
        }

        private void FlushNow()
        {
            List<T> itemsToFlush;

            lock (_lock)
            {
                if (_batchItems.Count == 0)
                    return;

                itemsToFlush = new List<T>(_batchItems);
                _batchItems.Clear();

                _timer?.Dispose();
                _timer = null;
            }

            // 异步处理批量数据（避免阻塞调用线程）
            _lastFlushTask = _lastFlushTask.ContinueWith(_ =>
            {
                _processBatch(itemsToFlush);
            });
        }
    }
}
