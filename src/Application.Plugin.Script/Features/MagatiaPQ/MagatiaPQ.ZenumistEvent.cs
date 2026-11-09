namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 泽尼姆斯特（Zenumist）分支：<c>rnj*</c> 前缀，地图基数 926100000，
    /// 护送朱丽叶、击破弱 / 强 Frankenroid 9300139 / 9300140。
    /// </summary>
    internal sealed class ZenumistEvent : MagatiaPQEventBase
    {
        public ZenumistEvent() : base(MagatiaPQFeature.ZenumistEventName, MagatiaPQFeature.ZenumistMapBase)
        {
            RecruitMap = 261000011;
        }

        internal override int MapBase => MagatiaPQFeature.ZenumistMapBase;
        internal override bool IsAlcadno => false;

        internal override int YuleteOfficeNpc => 2112000;
        internal override int YuleteDeadNpc => 2112001;
        internal override int EscortNpc => 2112006;

        internal override int WeakBossId => 9300139;
        internal override int StrongBossId => 9300140;

        internal override string ExitReactorName => "rnj6_out";
    }
}
