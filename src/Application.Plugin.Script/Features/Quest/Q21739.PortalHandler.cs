using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21739PortalHandler : PortalPlayerInteraction
    {
        public Q21739PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 200060000 | pt: 7
        // Map 200060000
        public async Task<bool> enterNepenthes()
        {
            if (isQuestActive(Q21739.QuestIdValue))
            {
                return await TryStartSoloQuest(Q21739.QuestIdValue);
            }
            else
            {
                await playPortalSound();
                await warp(200060001, 2);
                return true;
            }
        }
    }
}
