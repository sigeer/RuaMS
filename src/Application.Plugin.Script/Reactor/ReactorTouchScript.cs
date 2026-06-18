using Application.Core.Client;
using scripting.reactor;
using server.maps;

namespace Application.Plugin.Script
{
    internal class ReactorTouchScript : ReactorActionManager
    {
        public ReactorTouchScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }
        // Reactor: 6109014 
        public async Task glpqflame0()
        {
            string[] flames = ["a1", "a2", "b1", "b2", "c1", "c2"];
            for (var i = 0; i < flames.Length; i++)
            {
                await getMap().toggleEnvironment(flames[i]);
            }

        }
    }
}
