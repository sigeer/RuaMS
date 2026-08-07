using Application.Shared.Servers;

namespace Application.Core.Login.Servers
{
    public class RegisteredChannelInfo : ChannelConfig
    {
        public string ServerHost { get; set; } = "127.0.0.1";
        public string ServerName { get; set; } = null!;
    }
}
