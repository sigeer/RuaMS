namespace Application.Core.tools
{
    public abstract class DataStorage<TData>
    {
        protected Dictionary<int, TData> _dataSource;
        protected DataStorage() : this(new Dictionary<int, TData>())
        {
        }

        protected DataStorage(Dictionary<int, TData> dataSource)
        {
            _dataSource = dataSource;
        }

        public TData? this[int id] => _dataSource.GetValueOrDefault(id);
    }
}
