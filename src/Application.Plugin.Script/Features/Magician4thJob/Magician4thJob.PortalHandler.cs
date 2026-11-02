using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Magician4thJob
{
    internal sealed class Magician4thJobPortalHandler : PortalPlayerInteraction
    {
        public Magician4thJobPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 230040300 | pt: 8
        public async Task<bool> s4resurrection()
        {

            if (haveItem(4001108))
            {
                return await TryStartSoloQuest(Magician4thJobEvent6132.QuestIdValue);
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }



        // Map: 220070400 | pt: 11
        public async Task<bool> s4resur_enter()
        {

            if (isQuestStarted(6134))
            {
                await playPortalSound();
                await warp(922020000, 0);
                return true;
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }

        // Map: 922020000 | pt: 8
        public async Task<bool> s4resur_out()
        {

            if (isQuestStarted(6134))
            {
                if (canHold(4031448))
                {
                    await gainItem(4031448, 1);
                    await playPortalSound();
                    await warp(220070400, 3);

                    return true;
                }
                else
                {
                    await Pink("Make room on your ETC to receive the quest item.");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(220070400, 3);
                return true;
            }
        }

        // Map: 921100300 | pt: 7
        /// <summary>
        /// 通用出口：持有 4031495 时去 921100301，否则回 211040100。
        /// </summary>
        public async Task<bool> s4common1_exit()
        {
            if (hasItem(4031495))
            {
                await playPortalSound();
                await warp(921100301);
            }
            else
            {
                await playPortalSound();
                await warp(211040100);
            }

            return true;
        }
    }
}
