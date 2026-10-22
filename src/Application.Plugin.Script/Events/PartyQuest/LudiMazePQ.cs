using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 鲁塔比迷宫组队任务（LudiMazePQ）。
    /// 由 Npc/PQ_LudiMaze.cs 的 9103000~9103003 使用。
    /// </summary>
    internal class LudiMazePQ : AbstractPartyQuestEventTemplate
    {
        const int MAZE_START = 809050000;
        const int MAZE_END = 809050016;
        const int BONUS_MAP = 809050016;
        const int EXIT_MAP = 809050017;

public LudiMazePQ() : base(nameof(LudiMazePQ))
        {
            QuestId = 1203;

            MinCount = 3;
            MaxCount = 6;
            MinLevel = 51;
            MaxLevel = 70;

            EntryMap = MAZE_START;
            ExitMap = EXIT_MAP;
            RecruitMap = 220000000;
            ClearMap = BONUS_MAP;
            MinMap = MAZE_START;
            MaxMap = BONUS_MAP;

            EventTime = 15 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            for (var mapId = MAZE_START; mapId <= MAZE_END; mapId++)
            {
                var map = await eim.getInstanceMap(mapId);
                if (map == null)
                    continue;

                map.shuffleReactors();
            }
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await eim.warpEventTeam(BONUS_MAP);

            await base.ClearPQ(eim);
        }
    }
}
