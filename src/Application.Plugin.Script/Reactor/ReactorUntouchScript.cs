using Application.Core.Client;
using scripting.reactor;
using server.maps;

namespace Application.Plugin.Script
{
    internal class ReactorUntouchScript : ReactorActionManager
    {
        public ReactorUntouchScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }
    }
}
