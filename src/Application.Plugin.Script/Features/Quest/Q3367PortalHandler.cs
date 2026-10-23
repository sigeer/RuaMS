using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3367 相关脚本：q3367in。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3367PortalHandler : PortalPlayerInteraction
    {
        public Q3367PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 926130100 | pt: 8
        public async Task<bool> q3367in()
        {

            if (isQuestStarted(3367))
            {
                var booksDone = getQuestProgressInt(3367, 31);
                var booksInv = getItemQuantity(4031797);

                if (booksInv < booksDone)
                {
                    await gainItem(4031797, booksDone - booksInv);
                }

                await playPortalSound();
                await warp(926130102, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this room.");
                return false;
            }
        }
    }
}
