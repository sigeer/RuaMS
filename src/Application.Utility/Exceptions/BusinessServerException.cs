namespace Application.Utility.Exceptions
{
    public class BusinessServerException : BusinessException
    {
        public BusinessServerException(string message) : base(message)
        {

        }
    }

    public class BusinessServerOfflineException : BusinessServerException
    {
        public BusinessServerOfflineException() : base("频道服务器已与主服务器断开连接")
        {

        }
    }
}
