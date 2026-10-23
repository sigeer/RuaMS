using Application.Core.Client;
using Application.Core.Server.maps;

namespace scripting.reactor;

/// <summary>
/// 玩家离开反应堆范围时触发的脚本。
/// </summary>
public abstract class ReactorUntouchScriptBase : ReactorActionManager
{
    protected ReactorUntouchScriptBase(IChannelClient c, Reactor reactor) : base(c, reactor)
    {
    }
}
