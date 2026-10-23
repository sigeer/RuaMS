using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class FirstDHPortalHandler : PortalPlayerInteraction
    {
        public FirstDHPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 130020000 | pt: 7
        /// <summary>
        /// 1 号演武场入口：按当前进行中的适应训练任务（20701/20702/20703）路由。
        /// </summary>
        public async Task<bool> enterFirstDH()
        {
            var currentQuestId = 0;
            if (getQuestStatus(FirstDHEvent20701.QuestIdValue) == 1)
            {
                currentQuestId = FirstDHEvent20701.QuestIdValue;
            }
            else if (getQuestStatus(FirstDHEvent20702.QuestIdValue) == 1)
            {
                currentQuestId = FirstDHEvent20702.QuestIdValue;
            }
            else if (getQuestStatus(FirstDHEvent20703.QuestIdValue) == 1)
            {
                currentQuestId = FirstDHEvent20703.QuestIdValue;
            }

            if (currentQuestId > 0)
            {
                return await TryStartSoloQuest(currentQuestId);
            }
            else
            {
                await Pink("只有参加Kiku的适应训练，才能进入1号演武场。");
                return false;
            }
        }
    }
}
