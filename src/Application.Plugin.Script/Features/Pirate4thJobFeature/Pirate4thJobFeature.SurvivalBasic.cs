using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    /// <summary>耐力试炼（基础）：超时存活即推进 6330。</summary>
    internal sealed class Pirate4thJobFeatureSurvivalBasic : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6330;

        internal const int ENTRY_MAP = 912010000;
        internal const int EXIT_MAP = 120000101;
        internal const int SURVIVE_MAP = 912010200;

        public Pirate4thJobFeatureSurvivalBasic() : base(QuestIdValue)
        {
            EventTime = 4 * 60 + 1;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            MinMap = ENTRY_MAP;
            MaxMap = SURVIVE_MAP;
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader();
            if (chr?.isAlive() ?? false)
            {
                await chr.setQuestProgress(QuestIdValue, 6331, "2");
                await chr.changeMap(SURVIVE_MAP);
            }
        }
    }
}
