namespace Application.Utility
{

    public record StoreUnit<TRecord>
        where TRecord : class
    {
        public StoreUnit(StoreFlag method, TRecord data)
        {
            Data = data;
            Flag = method;
        }

        public TRecord Data { get; set; }
        public StoreFlag Flag { get; set; }
    }

    public enum StoreFlag : byte
    {
        Cached,
        AddOrUpdate,
        Remove
    }

}
