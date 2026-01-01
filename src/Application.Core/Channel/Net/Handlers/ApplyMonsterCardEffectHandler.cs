using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers;

public class ApplyMonsterCardEffectHandler : ChannelHandlerBase
{

    readonly ILogger<ApplyMonsterCardEffectHandler> _logger;

    public ApplyMonsterCardEffectHandler(ILogger<ApplyMonsterCardEffectHandler> logger)
    {
        _logger = logger;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        // 始终是0
        // var available = p.available();
        return Task.CompletedTask;
    }
}