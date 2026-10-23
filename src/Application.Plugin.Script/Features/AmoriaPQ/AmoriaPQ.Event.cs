using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;

namespace Application.Plugin.Script.Features.AmoriaPQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        static readonly int[] EventMaps =
        [
            670010200, 670010300, 670010301, 670010302, 670010400,
            670010500, 670010600, 670010700, 670010750, 670010800,
        ];

        public Event() : base(AmoriaPQFeature.EventName)
        {
            MinCount = 6;
            MaxCount = 6;
            MinLevel = 40;
            MaxLevel = 255;

            RecruitMap = 670010100;
            EntryMap = 670010200;
            ExitMap = 670011000;
            ClearMap = 670010800;

            MinMap = 670010200;
            MaxMap = 670010800;

            EventTime = 75 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            eim.setIntProperty("marriedGroup", 0);
            eim.setIntProperty("missCount", 0);
            for (var i = 1; i <= 6; i++)
            {
                eim.setIntProperty($"statusStg{i}", -1);
            }
            eim.setIntProperty("statusStgBonus", 0);

            foreach (var mapId in new[] { 670010200, 670010300, 670010301, 670010302 })
            {
                (await eim.getInstanceMap(mapId))?.toggleDrops();
            }

            var map200 = await eim.getInstanceMap(670010200);
            if (map200 != null)
            {
                await map200.instanceMapForceRespawn();
            }

            var map500 = await eim.getInstanceMap(670010500);
            if (map500 != null)
            {
                await map500.instanceMapForceRespawn();
            }

            (await eim.getInstanceMap(670010750))?.shuffleReactors();
            (await eim.getInstanceMap(670010800))?.shuffleReactors();

            // 婚礼蛋糕 Boss
            var map700 = await eim.getInstanceMap(670010700);
            if (map700 != null)
            {
                var mob = LifeFactory.Instance.GetMonsterTrust(9400536);
                await map700.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(942, 478));
            }
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);

            if (IsTeamAllCouple(eim))
            {
                eim.setIntProperty("marriedGroup", 1);
            }
        }

        /// <summary>
        /// 队伍里每个人都必须是与队内成员结成的伴侣。
        /// </summary>
        static bool IsTeamAllCouple(AbstractEventInstanceManager eim)
        {
            foreach (var chr in eim.getPlayers())
            {
                var partnerId = getPartnerId(chr);
                if (partnerId <= 0 || eim.getPlayerById(partnerId) == null)
                {
                    return false;
                }
            }

            return true;
        }

        static int getPartnerId(Player chr)
        {
            return chr.getMarriageRing()?.getPartnerChrId() ?? 0;
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var map = await eim.getInstanceMap(EntryMap);
            if (map == null)
                return;

            await chr.changeMap(map, 0);
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            // 第 6 阶段已通过：直接送出，保留奖励结算
            if (eim.getIntProperty("statusStg6") == 1)
            {
                await eim.warpEventTeam(ExitMap);
            }

            await base.OnTimeOut(eim);
        }

        public override async Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            // JS 的 playerDead 为空函数
            await base.OnPlayerDied(eim, chr);
        }

    }
}
