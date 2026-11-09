using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script
{
    internal partial class ReactorTouchScript : ReactorTouchScriptBase
    {
        public ReactorTouchScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }
    }
}
