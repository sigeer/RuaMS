using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    /// <summary>
    /// 巴洛古远征：普通难度与简单难度共用同一张地图组（105100xxx），
    /// 事件模板、祭坛反应堆与首入召唤集中在此。
    /// 简单难度对应 JS <c>scripts/event/BalrogBattle_Easy.js</c>。
    /// </summary>
    internal static class Exped_BalrogFeature
    {
        /// <summary>远征（普通难度）事件注册名。</summary>
        internal const string EventName = "Exped_Balrog";

        /// <summary>简单难度事件注册名。必须与历史注册名保持一致。</summary>
        internal const string EasyEventName = "Exped_Balrog_Easy";



        #region Easy
        /// <summary>
        /// 简单难度：封印形态第一只 8830007，后续 8830008 / 8830009 以偏移识别
        /// （<c>mobId - EasySealedBossId</c> 对应 0..2）。
        /// </summary>
        internal const int EasySealedBossId = 8830007;

        /// <summary>简单难度：巴洛古本体，被击杀后 10 秒重新召唤封印形态。</summary>
        internal const int EasyBossMobId = 8830010;

        /// <summary>
        /// 8830009 -> 8830012
        /// </summary>
        internal const int EasyLeftClawMobId = 8830009;

        /// <summary>
        /// 8830006 -> 8830001 -> 8830004
        /// </summary>
        internal const int EasySealed0RightClawMobId = 8830013;
        internal const int EasyRightClawMobId = 8830008;
        internal const int EasySealed1RightClawMobId = 8830011;
        #endregion

        internal static readonly Point SpawnPoint = new(412, 258);

        #region Normal
        internal const int SealedBossId = 8830000;

        internal const int BossMobId = 8830003;

        /// <summary>
        /// 8830006 -> 8830001 -> 8830004
        /// </summary>
        internal const int Sealed0RightClawMobId = 8830006;
        internal const int RightClawMobId = 8830001;
        internal const int Sealed1RightClawMobId = 8830004;

        /// <summary>
        /// 8830002 -> 8830005
        /// </summary>
        internal const int LeftClawMobId = 8830002;
        #endregion

    }
}
