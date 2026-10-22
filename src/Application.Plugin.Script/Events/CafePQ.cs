using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 咖啡厅组队任务基类（CafePQ_1 ~ CafePQ_6）。
    /// 对应 JS scripts/event/CafePQ_{1..6}.js —— 六份脚本结构一致，仅地图、优惠券数量与掉落数量不同。
    /// 通关由 NPC 1052013 收取优惠券后调用 clearPQ 触发（JS 的 monsterKilled 只负责掉落）。
    /// </summary>
    internal abstract class CafePQ : AbstractPartyQuestEventTemplate
    {
        /// <summary>任务优惠券。</summary>
        const int COUPON_ITEM = 4001007;
        const int RECRUIT_MAP = 193000000;

        readonly int[] _eventMaps;
        readonly int _couponsNeeded;
        /// <summary>怪物等级 &gt; 65 时的掉落数量。</summary>
        readonly int _highLevelDrop;

        protected CafePQ(string name, int entryMap, int[] eventMaps, int couponsNeeded, int highLevelDrop)
            : base(name)
        {
            MinCount = 3;
            MaxCount = 6;
            MinLevel = 21;
            MaxLevel = 120;

            RecruitMap = RECRUIT_MAP;
            EntryMap = entryMap;
            ExitMap = RECRUIT_MAP;
            ClearMap = RECRUIT_MAP;

            MinMap = eventMaps[0];
            MaxMap = eventMaps[^1];

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;

            _eventMaps = eventMaps;
            _couponsNeeded = couponsNeeded;
            _highLevelDrop = highLevelDrop;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("stage", 0);
            eim.setIntProperty("couponsNeeded", _couponsNeeded);

            foreach (var mapId in _eventMaps)
            {
                var map = await eim.getInstanceMap(mapId);
                if (map == null)
                    continue;

                map.toggleDrops();
            }
        }

        /// <summary>
        /// 击杀怪物掉落优惠券，数量按怪物等级。
        /// </summary>
        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (eim.isEventCleared())
                return;

            var qty = mob.getLevel() > 65 ? _highLevelDrop : (mob.getLevel() > 40 ? 2 : 1);
            var dropper = eim.getPlayers().FirstOrDefault();
            if (dropper == null)
                return;

            var item = Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance()
                .GenerateVirtualItemById(COUPON_ITEM, (short)qty);
            await mob.getMap().spawnItemDrop(mob, dropper, item, mob.getPosition(), true, false);
        }

        /// <summary>
        /// 通关：发关卡奖励、清空全部地图怪物并播放通关特效。
        /// </summary>
        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await eim.stopEventTimer();
            await eim.setEventCleared();
            await eim.GiveStageClearRewardAll(1);

            foreach (var mapId in _eventMaps)
            {
                var map = await eim.getInstanceMap(mapId);
                if (map == null)
                    continue;

                await map.killAllMonstersNotFriendly();
                await eim.showClearEffect(mapId);
            }
        }
    }

    internal class CafePQ_1 : CafePQ
    {
        public CafePQ_1() : base(nameof(CafePQ_1), 190000000, [190000000, 190000001, 190000002], 400, 5) { }
    }

    internal class CafePQ_2 : CafePQ
    {
        public CafePQ_2() : base(nameof(CafePQ_2), 191000000, [191000000, 191000001], 350, 5) { }
    }

    internal class CafePQ_3 : CafePQ
    {
        public CafePQ_3() : base(nameof(CafePQ_3), 192000000, [192000000, 192000001], 350, 3) { }
    }

    internal class CafePQ_4 : CafePQ
    {
        public CafePQ_4() : base(nameof(CafePQ_4), 195000000, [195000000, 195010000, 195020000, 195030000], 450, 5) { }
    }

    internal class CafePQ_5 : CafePQ
    {
        public CafePQ_5() : base(nameof(CafePQ_5), 196000000, [196000000, 196010000], 500, 5) { }
    }

    internal class CafePQ_6 : CafePQ
    {
        public CafePQ_6() : base(nameof(CafePQ_6), 197000000, [197000000, 197010000], 300, 5) { }
    }
}
