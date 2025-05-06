namespace Application.Shared.Servers
{
    public class ActualServerConfig
    {
        public string ServerMessage { get; set; }
        public int Port { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// 用于服务器内部交流的地址，建议使用内网IP
        /// </summary>
        public string GrpcServiceEndPoint { get; set; } = "192.168.0.1:7878";
    }
}
