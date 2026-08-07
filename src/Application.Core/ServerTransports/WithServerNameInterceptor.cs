using Application.Core.Channel.Configs;
using Application.Protos;
using Application.Shared.Servers;
using Microsoft.Extensions.Options;

namespace Application.Core.ServerTransports
{
    public class WithServerNameInterceptor : GlobalHeaderInterceptor
    {
        public WithServerNameInterceptor(IOptions<ChannelNodeConfig> options) : base("x-server-name", options.Value.ServerName)
        {
        }
    }
}
