using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.LatanicaBattle
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 541010060 | pt: 7
        public async Task<bool> captinsg00()
        {

            if (!haveItem(4000381))
            {
                await this.Dialog(this.GetTalkMessage(ScriptMessage.LatanicaPortalHandler_DontHave), 9270033);
                return false;
            }
            else
            {
                var em = GetEventManager(LatanicaBattleFeature.EventName);
                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else
                {
                    await this.Dialog(em.HandleCreateInstanceResult(r, c), 9270033);
                    return false;
                }
            }
        }
    }
}
