using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Archer4thJob
{
    internal sealed class Archer4thJobPortalHandler : PortalPlayerInteraction
    {
        public Archer4thJobPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 211042200 | pt: 8
        public async Task<bool> s4firehawk()
        {

            if (isQuestStarted(Archer4thJobEvent6240.QuestIdValue))
            {
                return await TryStartSoloQuest(Archer4thJobEvent6240.QuestIdValue);
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }

        // Map: 240010700 | pt: 7
        public async Task<bool> s4nest()
        {

            if (isQuestStarted(6241) || isQuestStarted(6243))
            {
                return await TryStartSoloQuest(Archer4thJobEvent6241.QuestIdValue);
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }

        // Map: 211040700 | pt: 8
        public async Task<bool> s4iceeagle()
        {

            if (isQuestStarted(Archer4thJobEvent6242.QuestIdValue))
            {
                return await TryStartSoloQuest(Archer4thJobEvent6242.QuestIdValue);
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }
    }
}
