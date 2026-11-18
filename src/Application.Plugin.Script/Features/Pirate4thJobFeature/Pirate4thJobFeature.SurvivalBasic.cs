using Application.Core.Game.Players;
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

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            await base.OnSetup(eim, level, lobbyId);

            eim.Schedule(playerCanLeave, 60_000);
            eim.Schedule(PlayerSurvived, 120_000);
        }

        Task playerCanLeave(AbstractEventInstanceManager eim)
        {
            eim.setIntProperty("canLeave", 1);
            return Task.CompletedTask;
        }

        async Task PlayerSurvived(AbstractEventInstanceManager eim)
        {
            var chr = eim.getLeader()!;
            if (chr.isAlive())
            {
                await eim.clearPQ();
            }
        }

        public override async Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            await base.OnPlayerDied(eim, chr);

            if (eim.getTimeLeft() > 120_000)
            {
                await eim.Pink("凯琳：考验失败啦~别哭丧着脸嘛，休息会儿再试试？");
            }
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            eim.setIntProperty("canLeave", 2);
            await eim.Pink("凯琳：试炼通过。最终考验——你能抵达那边的出口吗？");

            var chr = eim.getLeader()!;
            await chr.setQuestProgress(QuestIdValue, 6331, "2");

        }
    }
}
