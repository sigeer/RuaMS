using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Features.Exped_Scarga
{
    internal sealed class Event : AbstractExpeditionEventTemplate
    {
        const int ENTRY_MAP = 551030200;
        const int EXIT_MAP = 551030100;

        const int SCARGA_BEAR = 9420544;
        const int SCARGA_LION = 9420549;

        public Event()
            : base(SCARGA_BEAR, Exped_ScargaFeature.EventName, ExpeditionEntryType.SCARGA.Name)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 100;
            MaxLevel = 255;

            RecruitMap = EXIT_MAP;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 60 * 60;
            RegistrationTime = 5 * 60;

            // JS setEventRewards：29 项通关奖励
            AllClearRewards = new()
            {
                {
                    1,
                    new Core.model.RewardPools(
                        [
                            new(1102145, 1),
                            new(1102084, 1),
                            new(1102085, 1),
                            new(1102086, 1),
                            new(1102087, 1),
                            new(1052165, 1),
                            new(1052166, 1),
                            new(1052167, 1),
                            new(1402013, 1),
                            new(1332030, 1),
                            new(1032030, 1),
                            new(1032070, 1),
                            new(4003000, 50),
                            new(4000030, 50),
                            new(4006000, 120),
                            new(4006001, 120),
                            new(4005000, 4),
                            new(4005001, 4),
                            new(4005002, 4),
                            new(4005003, 4),
                            new(4005004, 2),
                            new(2022016, 125),
                            new(2022263, 125),
                            new(2022264, 125),
                            new(2022015, 30),
                            new(2022306, 30),
                            new(2022307, 30),
                            new(2022306, 30),
                            new(2022113, 30),
                        ],
                        [],
                        []
                    )
                },
            };
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("defeatedBoss", 0);
            eim.setIntProperty("canJoin", 1);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var mobId = mob.getId();
            if (mobId != SCARGA_BEAR && mobId != SCARGA_LION)
                return;

            var killed = eim.getIntProperty("defeatedBoss");
            eim.setIntProperty("defeatedBoss", killed + 1);

            if (killed >= 1)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.clearPQ();
            }
        }

    }
}
