using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q20718PortalHandler : PortalPlayerInteraction
    {
        public Q20718PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 101000000 | pt: 7
        public async Task<bool> enterMagiclibrar()
        {
            if (isQuestStarted(Q20718.QuestIdValue))
            {
                return await TryStartSoloQuest(Q20718.QuestIdValue);
            }
            else
            {
                await playPortalSound();
                await warp(101000003, 8);
            }
            return true;
        }


        // Map: 910110000 | pt: 7
        // Map: 910110000
        public async Task<bool> outMagiclib()
        {
            if (getMap().countMonster(2220100) > 0)
            {
                await Pink("Cannot leave until all Blue Mushrooms have been defeated.");
                return false;
            }
            else
            {
                await playPortalSound();
                // 离开任务副本时会自行关闭副本
                await warp(101000000, 26);

                if (isQuestCompleted(Q20718.QuestIdValue))
                {
                    await openNpc(1103003, "MaybeItsGrendel_end");
                }

                return true;
            }
        }
    }
}
