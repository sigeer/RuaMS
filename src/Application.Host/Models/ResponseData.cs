namespace Application.Host.Models
{
    public class ResponseData<TData>
    {
        public ResponseData(TData? data)
        {
            Data = data;
        }

        public int Code { get; set; }
        public TData? Data { get; set; }
    }
}
