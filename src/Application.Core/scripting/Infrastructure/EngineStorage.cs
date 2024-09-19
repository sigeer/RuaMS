namespace Application.Core.Scripting.Infrastructure
{
    public class EngineStorate<TKey> : DataStorage<TKey, IEngine>, IDisposable where TKey : notnull
    {
        public void Dispose()
        {
            Clear();
        }

        public override void Clear()
        {
            foreach (var item in _dataSource.Values)
            {
                item?.Dispose();
            }
            _dataSource.Clear();
        }

        public override IEngine? Remove(TKey key)
        {
            var removedEngine = base.Remove(key);
            if (removedEngine != null)
                removedEngine.Dispose();
            return removedEngine;
        }
    }

    public class EngineStorage : EngineStorate<string>
    {
    }
}
