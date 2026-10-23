using Application.Core.Game.Players;
using Application.Core.Game.Skills;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.RescueGaga
{
    internal sealed class Event : AbstractSoloEventTemplate
    {
        const int ENTRY_MAP = 922240000;
        const int EXIT_MAP = 922240200;
        const int GOAL_MAP = 922240100;
        const int GRANDPA_BUNNY = 9001105;
        /// <summary>营救过程中佩戴的加速道具（HOORAY &lt;3）。</summary>
        const int RESCUE_BUFF_ITEM = 2360002;

        public Event() : base(RescueGagaFeature.EventName)
        {
            MinLevel = 12;
            MaxLevel = 255;

            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = GOAL_MAP;

            EventTime = 3 * 60;
            Type = Shared.Events.EventInstanceType.Regular;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);
            eim.setIntProperty("stage", 0);
            eim.setIntProperty("falls", 0);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await base.OnPlayerEntry(eim, chr);

            await chr.getAbstractPlayerInteraction().ShowEffect("event/space/start");
            await chr.startMapEffect("请在时间限制内营救佳佳。", 5120027);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (mapid == GOAL_MAP)
            {
                // 到达终点 → 通关
                await ClearPQ(eim);
            }
            else if (mapid == ENTRY_MAP)
            {
                // 刚进入挑战地图，给上加速道具
                await player.getAbstractPlayerInteraction().useItem(RESCUE_BUFF_ITEM);
            }
        }

        public override async Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            await chr.CancelBuffFromSourceId(RESCUE_BUFF_ITEM);
            await base.OnPlayerUnregister(eim, chr);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            foreach (var chr in eim.getPlayers())
            {
                chr.Events[eim.EventName] += 1;
            }

            // 10 秒后在终点地图生成兔子爷爷
            eim.Schedule(SpawnGrandpaBunny, 10_000);
        }

        static async Task SpawnGrandpaBunny(AbstractEventInstanceManager eim)
        {
            var map = await eim.getInstanceMap(GOAL_MAP);
            if (map != null)
            {
                await map.SpawnNpc(GRANDPA_BUNNY, new System.Drawing.Point(175, -20));
            }
        }

        public async Task GiveSkill(Player chr)
        {
            int skillid = 0;
            switch (chr.getJobType())
            {
                case 0:
                    skillid = 1013;
                    break;
                case 1:
                case 2:
                    skillid = 10001014;
                    break;
            }

            var completed = chr.Events[RescueGagaFeature.EventName];
            long expiration = chr.getChannelServer().Node.GetCurrentTimeDateTimeOffset().AddDays(20).ToUnixTimeMilliseconds(); //20 days
            if (completed < 20)
            {
                await chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid), 1, 1, expiration);
                await chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid + 1), 1, 1, expiration);
                await chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid + 2), 1, 1, expiration);
            }
            else
            {
                await chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid), 2, 2, chr.getSkillExpiration(skillid));
            }
        }
    }
}
