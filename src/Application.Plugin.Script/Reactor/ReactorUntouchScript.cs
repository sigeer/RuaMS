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
    }
}
