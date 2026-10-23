using Application.Core.Client;
using Application.Core.Server.maps;

namespace scripting.reactor;

/// <summary>
/// 反应堆被点击（Act）时触发的脚本。
/// </summary>
public abstract class ReactorActScriptBase : ReactorActionManager
{
    protected ReactorActScriptBase(IChannelClient c, Reactor reactor) : base(c, reactor)
    {
    }
}
