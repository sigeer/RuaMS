using Application.Resources.Messages;
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
                await this.Pink(this.GetTalkMessage(ScriptMessage.Q21610PortalHandler_EnterOnly));
                return false;
            }
        }
    }
}
