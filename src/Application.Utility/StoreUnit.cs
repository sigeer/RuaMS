namespace Application.Utility
{
    public record StoreUnit<TModel> where TModel : class
    {
        public StoreUnit(StoreFlag method, TModel? data)
        {
            Data = data;
            Flag = method;
        }

        /// <summary>
        /// Remove时可能为null
        /// </summary>
        public TModel? Data { get; set; }
        public StoreFlag Flag { get; set; }
    }

    public enum StoreFlag
    {
        Cached,
        AddOrUpdate,
        Remove
    }

}
