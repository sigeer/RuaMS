using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3935PortalHandler : PortalPlayerInteraction
    {
        public Q3935PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 260000302 | pt: 8
        public async Task<bool> skyrom()
        {

            if (isQuestStarted(3935) && !haveItem(4031574, 1))
            {
                return await TryStartSoloQuest(Q3935.QuestIdValue);
            }
            else
            {
                return false;
            }
        }
    }
}
