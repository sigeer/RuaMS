using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3230ReactorActHandler : ReactorActScriptBase
    {
        public Q3230ReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2200000 
        public async Task go221024400()
        {
            await this.Pink(this.GetTalkMessage(ScriptMessage.Q3230ReactorActHandler_CloseTry));
            await warp(221024400, 1);
        }

        // Reactor: 2202000 
        public async Task ludiquest0()
        {
            await dropItems();
        }
    }
}
