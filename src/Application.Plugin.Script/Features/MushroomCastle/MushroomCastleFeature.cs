namespace Application.Plugin.Script.Features.MushroomCastle
{
    /// <summary>
    /// 蘑菇城堡：总理官邸讨伐（两个阶段的事件模板），以及进入官邸的 NPC 与 2333 任务脚本。
    /// 同区域还有蛋塔王的诅咒（<c>MK_PepeKing</c>）副本，其事件模板、地图脚本与出口传送门也在此。
    /// 总理官邸两个模板共用 boss 3300008 与招募地图 106021402。
    /// </summary>
    internal static class MushroomCastleFeature
    {
        internal const string EventName = "MK_PrimeMinister";
        internal const string Event2Name = "MK_PrimeMinister2";

        /// <summary>蛋塔王的诅咒事件名（入口 NPC 亦可开启该副本）。</summary>
        internal const string PepeKingEventName = "MK_PepeKing";

        /// <summary>蛋塔王副本的三只子 Boss 以 3300005 起递增，首入时随机召唤其一。</summary>
        internal const int PepeKingFirstSubBossId = 3300005;

        /// <summary>三只雪人全部击杀后发放的任务道具。</summary>
        internal const int PepeKingQuestRewardItem = 4032388;

        /// <summary>蛋塔王副本出口地图。</summary>
        internal const int PepeKingExitMapId = 106021400;

        /// <summary>首入脚本写入、进入特效读取的子 Boss 属性。</summary>
        internal const string PepeKingBossIdProperty = "BossId";

        internal const int PrimeMinisterMobId = 3300008;
        internal const int RecruitMapId = 106021402;

    }
}
