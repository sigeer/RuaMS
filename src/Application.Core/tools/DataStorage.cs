namespace Application.Core.tools
{
    public abstract class DataStorage<TData>: DataStorage<int, TData> where TData : class
    {
        protected DataStorage() : this(new Dictionary<int, TData?>())
        {
        }

        protected DataStorage(Dictionary<int, TData?> dataSource)
        {
            _dataSource = dataSource;
        }
    }

    public abstract class DataStorage<TKey, TData> where TKey : notnull where TData : class
    {
        protected Dictionary<TKey, TData?> _dataSource;
        protected DataStorage() : this(new Dictionary<TKey, TData?>())
        {
        }

        protected DataStorage(Dictionary<TKey, TData?> dataSource)
        {
            _dataSource = dataSource;
        }

        public TData? this[TKey key]
        {
            get => _dataSource.GetValueOrDefault(key);
            set => _dataSource[key] = value;
        }

        public virtual TData? Remove(TKey key)
        {
            if (_dataSource.Remove(key, out var d))
                return d;

            return null;
        }

        public virtual void Clear()
        {
            _dataSource.Clear();
        }
    }
}
