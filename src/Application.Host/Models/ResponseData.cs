namespace Application.Host.Models
{
    public class ResponseData<TData>
    {
        private ResponseData() { }
        public ResponseData(TData? data)
        {
            Data = data;
        }

        public int Code { get; set; }
        public TData? Data { get; set; }
    }
}
