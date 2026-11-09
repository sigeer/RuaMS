using Application.Core.Game.Life;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 阿尔卡德诺（Alcadno）分支：<c>jnr*</c> 前缀，地图基数 926110000，
    /// 护送罗密欧、击破弱 / 强 Frankenroid 9300151 / 9300152。
    /// </summary>
    internal sealed class AlcadnoEvent : MagatiaPQEventBase
    {
        public AlcadnoEvent() : base(MagatiaPQFeature.AlcadnoEventName, MagatiaPQFeature.AlcadnoMapBase)
        {
            RecruitMap = 261000021;
        }

        internal override int MapBase => MagatiaPQFeature.AlcadnoMapBase;
        internal override bool IsAlcadno => true;

        internal override int YuleteOfficeNpc => 2112010;
        internal override int YuleteDeadNpc => 2112011;
        internal override int EscortNpc => 2112005;

        internal override int WeakBossId => 9300151;
        internal override int StrongBossId => 9300152;

        internal override string ExitReactorName => "jnr6_out";
    }
}
