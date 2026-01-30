using Application.Protos;
using Application.Shared.Servers;
using Microsoft.Extensions.Options;

namespace Application.Core.ServerTransports
{
    public class WithServerNameInterceptor : GlobalHeaderInterceptor
    {
        public WithServerNameInterceptor(IOptions<ChannelServerConfig> options) : base("x-server-name", options.Value.ServerName)
        {
        }
    }
}
