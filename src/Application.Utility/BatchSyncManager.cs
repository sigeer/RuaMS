namespace Application.Utility
{
    public class BatchSyncManager<TKey, TModel>
    {
        private readonly object _lock = new();
        private readonly Dictionary<TKey, TModel> _itemMap = new();
        private readonly Func<TModel, TKey> _keySelector;
        private readonly int _delayMs;
        private readonly int _maxBatchSize;
        private readonly Action<List<TModel>> _processBatch;
        private Task _lastFlushTask = Task.CompletedTask;
        private Timer? _timer;

        public BatchSyncManager(int delayMs, int maxBatchSize, Func<TModel, TKey> keySelector, Action<List<TModel>> processBatch)
        {
            _delayMs = delayMs;
            _maxBatchSize = maxBatchSize;
            _keySelector = keySelector;
            _processBatch = processBatch;
        }

        public void Enqueue(TModel item)
        {
            lock (_lock)
            {
                var key = _keySelector(item);
                _itemMap[key] = item; // 覆盖旧数据，保留最后一次

                if (_itemMap.Count >= _maxBatchSize)
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
                _itemMap.Clear();
                _timer?.Dispose();
                _timer = null;
            }
        }

        private void FlushNow()
        {
            List<TModel> itemsToFlush;

            lock (_lock)
            {
                if (_itemMap.Count == 0)
                    return;

                itemsToFlush = new List<TModel>(_itemMap.Values);
                _itemMap.Clear();

                _timer?.Dispose();
                _timer = null;
            }

            _lastFlushTask = _lastFlushTask.ContinueWith(_ =>
            {
                _processBatch(itemsToFlush);
            });
        }
    }

}
