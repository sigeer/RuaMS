using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.LudiMazePQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int MAZE_START = 809050000;
        const int MAZE_END = 809050016;
        const int BONUS_MAP = 809050016;
        const int EXIT_MAP = 809050017;

        public Event() : base(LudiMazePQFeature.EventName)
        {
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

            // JS setEventRewards：47 项通关奖励
            AllClearRewards = new()
            {
                {
                    1,
                    new Core.model.RewardPools(
                        [
                            new(1442017, 1),
                            new(1322025, 1),
                            new(1032013, 1),
                            new(1302016, 1),
                            new(1072263, 1),
                            new(1032043, 1),
                            new(2000005, 1),
                            new(2000004, 5),
                            new(2001001, 5),
                            new(2001002, 5),
                            new(2020008, 20),
                            new(2020010, 20),
                            new(2030008, 20),
                            new(2030010, 20),
                            new(2030009, 20),
                            new(2022000, 50),
                            new(2001000, 50),
                            new(2022019, 50),
                            new(2020007, 100),
                            new(2020006, 100),
                            new(2020009, 100),
                            new(2000006, 100),
                            new(2040601, 1),
                            new(2040605, 1),
                            new(2040602, 1),
                            new(2041027, 1),
                            new(2041028, 1),
                            new(2041004, 1),
                            new(2041029, 1),
                            new(2041017, 1),
                            new(2041020, 1),
                            new(2040008, 1),
                            new(2040001, 1),
                            new(2040009, 1),
                            new(2040002, 1),
                            new(2040504, 1),
                            new(2040511, 1),
                            new(2040505, 1),
                            new(2040501, 1),
                            new(2040904, 1),
                            new(2040901, 1),
                            new(2040905, 1),
                            new(2040902, 1),
                            new(2040404, 1),
                            new(2040401, 1),
                            new(2040405, 1),
                            new(2040402, 1),
                        ],
                        [],
                        []
                    )
                },
            };
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            // JS：随机在 entryMap + 0..14 中挑一张作为起点
            var map = await eim.getMapInstance(MAZE_START + Random.Shared.Next(15));
            if (map != null)
                await chr.changeMap(map, 0);

            await base.OnPlayerEntry(eim, chr);
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
