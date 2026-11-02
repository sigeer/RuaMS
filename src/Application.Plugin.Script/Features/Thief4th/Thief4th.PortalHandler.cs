using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Thief4th
{
    internal sealed class Thief4thPortalHandler : PortalPlayerInteraction
    {
        public Thief4thPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 101030104 | pt: 7
        public async Task<bool> s4hitman()
        {

            if (isQuestStarted(Thief4th.Assassinate))
            {
                return await TryStartSoloQuest(Thief4th.Assassinate);
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }

        // Map: 220010001 | pt: 9
        public async Task<bool> s4tornado_enter()
        {

            if (isQuestStarted(6230) || isQuestStarted(6231) || haveItem(4001110))
            {
                return await TryStartSoloQuest(Thief4th.NinjaStorm);
            }

            return false;
        }
    }
}
