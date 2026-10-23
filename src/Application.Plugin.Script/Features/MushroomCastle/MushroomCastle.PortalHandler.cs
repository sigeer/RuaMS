using Application.Core.Client;
using Application.Core.Server.maps;
using Application.Utility;
using scripting.portal;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class MushroomCastlePortalHandler : PortalPlayerInteraction
    {
        public MushroomCastlePortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 106020300 | pt: 9
        // Map: 106020300
        public async Task<bool> investigate1()
        {

            if (isQuestActive(2314) || isQuestCompleted(2314))
            {
                await openNpc(1300014);
                return true;
            }
            return false;
        }

        // Map: 106020500 | pt: 9
        // Map: 106020500
        public async Task<bool> investigate2()
        {

            if (isQuestActive(2322) || isQuestCompleted(2322))
            {
                await openNpc(1300014);
                return true;
            }
            return false;
        }

        // Map: 106021500 | pt: 8
        /// <summary>
        /// 出口：结束时清理副本；集齐三只雪人任务进度则补发任务道具。
        /// </summary>
        public async Task<bool> out_pepeking()
        {
            var eim = getEventInstance();
            if (eim != null && eim.EventName == MushroomCastleFeature.PepeKingEventName)
            {
                await eim.stopEventTimer();
                await eim.DisposeAsync();
            }

            var questProgress = getQuestProgressInt(2330, MushroomCastleFeature.PepeKingFirstSubBossId)
                + getQuestProgressInt(2330, MushroomCastleFeature.PepeKingFirstSubBossId + 1)
                + getQuestProgressInt(2330, MushroomCastleFeature.PepeKingFirstSubBossId + 2);

            if (questProgress == 3 && !hasItem(MushroomCastleFeature.PepeKingQuestRewardItem))
            {
                if (!canHold(MushroomCastleFeature.PepeKingQuestRewardItem))
                {
                    await Pink("请确保背包其它物品栏还有可用空间。");
                    return false;
                }

                await Pink("你已经拿到了结婚礼堂的钥匙。企鹅国王肯定是把它丢掉了。");
                await gainItem(MushroomCastleFeature.PepeKingQuestRewardItem, 1);
            }

            await playPortalSound();
            await warp(MushroomCastleFeature.PepeKingExitMapId, 2);
            return true;
        }
        // Map: 106021400 | pt: 7
        // 106021400 东侧城塔
        public async Task<bool> TD_MC_Egate()
        {

            await playPortalSound();
            await warp(106021300, 1);
            return true;
        }

        // Map: 106021400 | pt: 7
        // 106021400 东侧城塔
        public async Task<bool> TD_MC_enterboss1()
        {
            await openNpc(1300012);
            return false;
        }

        // Map: 106021402 | pt: 7
        // 106021402 最后的城塔
        public async Task<bool> TD_MC_enterboss2()
        {
            await openNpc(1300013);
            return false;
        }


        // Map: 100000002 | pt: 8
        public async Task<bool> TD_MC_first()
        {

            if (IsQuestNotStarted(2300))
            {
                await playPortalSound();
                await warp(106020001, 0);
                return true;
            }

            if (isQuestCompleted(2260) ||
        isQuestStarted(2300) || isQuestCompleted(2300) ||
        isQuestStarted(2301) || isQuestCompleted(2301) ||
        isQuestStarted(2302) || isQuestCompleted(2302) ||
        isQuestStarted(2303) || isQuestCompleted(2303) ||
        isQuestStarted(2304) || isQuestCompleted(2304) ||
        isQuestStarted(2305) || isQuestCompleted(2305) ||
        isQuestStarted(2306) || isQuestCompleted(2306) ||
        isQuestStarted(2307) || isQuestCompleted(2307) ||
        isQuestStarted(2308) || isQuestCompleted(2308) ||
        isQuestStarted(2309) || isQuestCompleted(2309) ||
        isQuestStarted(2310) || isQuestCompleted(2310))
            {
                await playPortalSound();
                await warp(106020000, 0);
                return true;
            }
            await Pink("一股神秘力量阻挡了你的进入。");
            return false;
        }

        // Map: 106020403 | pt: 9
        // Map: 106020403
        public async Task<bool> TD_MC_jump()
        {
            if (!isQuestCompleted(2324))
            {
                await Pink("城墙上仍然长满了荆棘。");
                return false;
            }

            await playPortalSound();
            await warp(106020600 + Randomizer.NextInt(0, 2), 0);
            return true;
        }

        // Map: 106020601 | pt: 9
        /// <summary>
        /// MapId: 106020601
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TD_MC_faild()
        {
            await ShowDirectionEffect("Effect/Direction2.img/mushCatle/nugu");
            await Pink("你被警卫发现了，现在将被送到悬崖底部。");

            var chrId = getPlayer().Id;
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                await getMap().Send(async m =>
                {
                    var chr = m.getCharacterById(chrId);
                    if (chr != null)
                    {
                        await chr.changeMap(106020403);
                    }
                });
            });
            return true;
        }

        // Map: 106020400 | pt: 8
        public async Task<bool> gotocastle()
        {

            if (isQuestCompleted(2321))
            {
                await playPortalSound();
                await warp(isQuestCompleted(2324) ? 106020501 : 106020500, 0);
                return true;
            }
            else
            {
                await Pink("前路布满荆棘，无法通过！");
                return false;
            }
        }

        // Map: 106021000 | pt: 7
        // Map: 106021000
        public async Task<bool> go_secretroom()
        {

            if (!isQuestCompleted(2335) && !(isQuestStarted(2335) && hasItem(4032405)))
            {
                await Pink("门锁了，需要钥匙才能进去。");
                return false;
            }

            if (isQuestStarted(2335))
            {
                await forceCompleteQuest(2335, 1300002);
                await giveCharacterExp(5000, getPlayer());
                await gainItem(4032405, -1);
            }
            await playPortalSound();
            await warp(106021001, 1);
            return true;
        }

        // Map: 106020300 | pt: 9
        public async Task<bool> obstacle()
        {

            if (isQuestStarted(100202))
            {    //使用过奇拉蘑菇孢子后允许直接通过
                await playPortalSound();
                await warp(106020400, 2);
                return true;
            }
            else if (hasItem(4000507))
            {
                await gainItem(4000507, -1);
                await playPortalSound();
                await warp(106020400, 2);
                await Pink("消耗一个 蘑菇的毒孢子 通过了结界。");
                return true;
            }
            else
            {
                await ShowEffect("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon1");
                await Pink("似乎有一个魔力强大的结界阻止你进入。");
            }
            return false;
        }
    }
}
