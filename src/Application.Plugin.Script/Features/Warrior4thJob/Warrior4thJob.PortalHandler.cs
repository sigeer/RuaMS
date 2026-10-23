using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Warrior4thJob
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 105090800 | pt: 7
        public async Task<bool> s4berserk()
        {

            if (isQuestStarted(Warrior4thJob.Berserk) && haveItem(4031475))
            {
                return await TryStartSoloQuest(Warrior4thJob.Berserk);
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }

        // Map: 910500200 | pt: 7
        public async Task<bool> s4berserk_move()
        {

            if (getPlayer().getMap().countMonsters() == 0)
            {
                await playPortalSound();
                await warp(105090800, "out00");
                return true;
            }
            await Pink("You must defeat all the monsters first.");
            return true;
        }


        // Map: 105090700 | pt: 7
        public async Task<bool> s4rush()
        {

            if (isQuestStarted(Warrior4thJob.Rush))
            {
                await playPortalSound();
                await warp(910500100, 0);
                return true;
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }
    }
}
