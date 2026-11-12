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

            // JS expStages = [2000, 4000, 6000, 8000, 9000, 11000]
            StageClearRewards = new()
            {
                { 1, new(2000, 0) },
                { 2, new(4000, 0) },
                { 3, new(6000, 0) },
                { 4, new(8000, 0) },
                { 5, new(9000, 0) },
                { 6, new(11000, 0) },
            };
        }

        /// <summary>
        /// JS 的准入校验：全队必须已婚，且同时包含两种性别（mask == 3）。
        /// 基类只校验招募图 / 人数 / 等级，这里补上夫妻副本特有的门槛。
        /// </summary>
        public override List<Player> GetEligibleParty(Player leader)
        {
            var members = base.GetEligibleParty(leader);
            if (members.Count == 0)
                return members;

            // onlyMarriedPlayers = true：任何一人未婚即整队不合格
            if (members.Any(x => x.getMarriageRing() == null))
                return [];

            var mask = 0;
            foreach (var chr in members)
            {
                mask |= 1 << (int)chr.getGender();
            }

            return mask == 3 ? members : [];
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

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            // 第 6 阶段已通过：直接送出，保留奖励结算
            if (eim.getIntProperty("statusStg6") == 1)
            {
                await eim.warpEventTeam(ExitMap);
            }

            await base.OnTimeOut(eim);
        }
    }
}
