namespace Application.Utility
{
    public record UpdateField<TModel>
    {
        public UpdateField(UpdateMethod method, TModel data)
        {
            Data = data;
            Method = method;
        }

        public TModel Data { get; set; }
        public UpdateMethod Method { get; set; }
    }

    public enum UpdateMethod
    {
        Add,
        Update,
        Remove
    }

}
