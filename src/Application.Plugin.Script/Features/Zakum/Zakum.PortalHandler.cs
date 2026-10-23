using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Zakum
{
    /// <summary>
    /// Zakum：Zakum05。
    /// </summary>
    internal sealed class ZakumPortalHandler : PortalPlayerInteraction
    {
        public ZakumPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 211042300 | pt: 7
        public async Task<bool> Zakum05()
        {
            var questStatus = getQuestStat(ZakumFeature.QuestId);
            if (questStatus == client.QuestStatus.Status.NOT_STARTED)
            {
                await Pink("你需要得到大师们的准许才能挑战扎昆BOSS,你现在没有资格进入。");
                return false;
            }

            else if (questStatus == client.QuestStatus.Status.STARTED)
            {
                await Pink("你必须完成所有试炼任务才有资格进入。");
                return false;
            }

            if (!haveItem(4001017))
            {
                await Pink("扎昆祭台需要 火焰之眼 ，否则无法召唤扎昆BOSS，请准备好所需物品再来挑战。");
                return false;
            }

            var react = getMap().getReactorById(2118002);
            if (react != null && react.getState() > 0)
            {
                await Pink("入口目前已被封锁，无法进入。");
                return false;
            }

            await playPortalSound();
            await warp(211042400, "west00");
            return true;
        }
    }
}
