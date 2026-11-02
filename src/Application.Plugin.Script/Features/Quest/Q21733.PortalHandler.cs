using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 出口：击败傀儡师后才可离开。
    /// </summary>
    internal sealed class Q21733PortalHandler : PortalPlayerInteraction
    {
        public Q21733PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 104000000 | pt: 7
        // Map: 910400000
        public async Task<bool> enterInfo()
        {
            if (isQuestStarted(Q21733.QuestIdValue) && getQuestProgressInt(Q21733.QuestIdValue, 21762) != 2)
            {
                return await TryStartSoloQuest(Q21733.QuestIdValue);
            }
            else
            {
                await playPortalSound();
                await warp(104000004);
                return true;
            }
        }
    }
}
