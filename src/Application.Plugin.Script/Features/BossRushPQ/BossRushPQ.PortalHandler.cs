using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using Application.Resources.Messages;
using scripting.portal;

namespace Application.Plugin.Script.Features.BossRushPQ
{
    /// <summary>
    /// 休息室出口：队长必须等队员先离开；领取通关奖励后回到招募地图。
    /// </summary>
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 970030001, 970030002, 970030003, 970030004, 970030005, 970030006 … (共 10 张) | pt: 7
        public async Task<bool> raid_rest()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != BossRushPQFeature.EventName)
            {
                return false;
            }

            if (eim.isEventLeader(getPlayer()) && eim.getPlayerCount() > 1)
            {
                await Pink("Being the party leader, you cannot leave before your teammates leave first or you pass leadership.");
                return false;
            }

            if (await eim.GiveClearReward(getPlayer()) != ClaimRewardResult.Success)
            {
                await Pink(c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.Redeem_InventoryFull)));
                return false;
            }

            await playPortalSound();
            await warp(BossRushPQFeature.RecruitMapId);
            return true;
        }

        // Map: 970030000 | pt: 7
        public async Task<bool> raidout()
        {

            var map = getPlayer().getSavedLocation("BOSSPQ");
            if (map == -1)
            {
                map = 100000000;
            }

            await playPortalSound();
            await warp(map, 0);
            return true;
        }

        // Map: 970030100, 970030101, 970030102, 970030103, 970030104, 970030105 … (共 802 张) | pt: 7
        public async Task<bool> raid_stage()
        {

            if (getMap().countMonsters() == 0)
            {
                int nextStage;

                if (getMapId() % 500 >= 100)
                {
                    nextStage = getMapId() + 100;
                }
                else
                {
                    nextStage = 970030001 + (int)(Math.Floor((getMapId() - 970030100) / 500.0));
                }

                await playPortalSound();
                await warp(nextStage);
                return true;
            }
            else
            {
                await LightBlue("Defeat all monsters before proceeding to the next stage.");
                return false;
            }
        }
    }
}
