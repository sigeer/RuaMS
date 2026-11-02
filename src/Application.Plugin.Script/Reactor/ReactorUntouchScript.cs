using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script
{
    internal class ReactorUntouchScript : ReactorUntouchScriptBase
    {
        public ReactorUntouchScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }

        // Reactor: 2406000 — 龙蛋：JS 的 untouch() 为空函数
        public Task hontaleNPC0() => Task.CompletedTask;
    }
}
