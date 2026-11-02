using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21610PortalHandler : PortalPlayerInteraction
    {
        public Q21610PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 211050000 | pt: 7
        public async Task<bool> enterRider()
        {
            if (isQuestStarted(Q21610Feature.QuestIdValue) && !haveItem(4001193, 1))
            {
                return await TryStartSoloQuest(Q21610Feature.QuestIdValue);
            }
            else
            {
                await Pink("领取了任务《斯卡德的建议》才可以进入这里");
                return false;
            }
        }
    }
}
