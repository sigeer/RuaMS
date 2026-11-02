using Application.Core.Client;
using Application.Core.Server.maps;

namespace scripting.reactor;

/// <summary>
/// 玩家踏入反应堆范围时触发的脚本。
/// </summary>
public abstract class ReactorTouchScriptBase : ReactorActionManager
{
    protected ReactorTouchScriptBase(IChannelClient c, Reactor reactor) : base(c, reactor)
    {
    }
}
