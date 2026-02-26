namespace Application.Utility
{
    public interface IStoreUnit<out TRecord>
    {
        /// <summary>
        /// 仅在Flag=Remove时可能为null
        /// </summary>
        TRecord? Data { get; }
        StoreFlag Flag { get; set; }
        void Remove();
        void Update();
    }

    public record StoreUnit<TRecord> : IStoreUnit<TRecord>
        where TRecord : class
    {
        public StoreUnit(StoreFlag method, TRecord? data)
        {
            Data = data;
            Flag = method;
        }

        public TRecord? Data { get; private set; }
        public StoreFlag Flag { get; set; }
        public void Remove()
        {
            Flag = StoreFlag.Remove;
            Data = null;
        }
        public void Update()
        {
            Flag = StoreFlag.AddOrUpdate;
        }
    }

    public enum StoreFlag : byte
    {
        Cached,
        AddOrUpdate,
        Remove
    }

}
