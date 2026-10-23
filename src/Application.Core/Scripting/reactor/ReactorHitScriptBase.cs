using Application.Core.Client;
using Application.Core.Server.maps;

namespace scripting.reactor;

/// <summary>
/// 反应堆被击打时触发的脚本。
///
/// 四个反应堆脚本种类各有一个基类，扫描时据此判定种类，无需额外标注。
/// </summary>
public abstract class ReactorHitScriptBase : ReactorActionManager
{
    protected ReactorHitScriptBase(IChannelClient c, Reactor reactor) : base(c, reactor)
    {
    }
}
