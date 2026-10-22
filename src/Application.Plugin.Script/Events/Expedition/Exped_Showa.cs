using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Battle;

namespace Application.Plugin.Script.Events
{
    internal class Exped_Showa : AbstractExpeditionEventTemplate
    {
        public Exped_Showa()
            : base(9400300, nameof(Exped_Showa), nameof(Exped_Showa))
        {
            MinCount = 3;
            MaxCount = 30;

            MinLevel = 100;
            MaxLevel = 255;

            EntryMap = 801040100;
            ExitMap = 801040004;
            ClearMap = 801040101;
            RecruitMap = 801040004;

            MinMap = 801040100;
            MaxMap = 801040101;

            EventTime = 60 * 60;
            RegistrationTime = 5 * 60;

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
                            new(4003000, 40),
                            new(4000030, 40),
                            new(4006000, 100),
                            new(4006001, 100),
                            new(4005000, 2),
                            new(4005001, 2),
                            new(4005002, 2),
                            new(4005003, 2),
                            new(4005004, 1),
                            new(2022016, 100),
                            new(2022263, 100),
                            new(2022264, 100),
                            new(2022015, 40),
                            new(2022306, 40),
                            new(2022307, 40),
                            new(2022306, 40),
                            new(2022113, 40),
                        ],
                        [],
                        []
                    )
                },
            };
        }

        public override Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setProperty("canJoin", 1);
            eim.setIntProperty("playerDied", 0);

            return base.OnSetup(eim, level, lobbyId);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            if (eim.getIntProperty("playerDied") == 0)
            {
                var eventMap = await eim.getInstanceMap(801040100);
                if (eventMap != null)
                {
                    eventMap.allowSummonState(false);
                    await eventMap.killAllMonsters();
                }

                var map = await eim.getMapInstance(801040101);
                await map.spawnMonsterOnGroundBelow(9400114, 500, -50);
                await eim.Pink(
                    "Konpei: The Boss has been defeated with no casualties, well done! We found a suspicious machine inside, we're moving it out."
                );
            }
        }

        public override Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            eim.setIntProperty("playerDied", 1);
            return base.OnPlayerDied(eim, chr);
        }

        public override async Task OnMobKilled(
            AbstractEventInstanceManager eim,
            Monster mob,
            ICombatantObject? killer
        )
        {
            if (mob.getId() == 8830000)
            {
                await eim.showClearEffect();
                await eim.clearPQ();
            }
        }
    }
}
