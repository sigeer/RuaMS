using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeatureEvent6410 : AbstractPartyQuestEventTemplate
    {
        internal const int QuestIdValue = 6410;

        const int ENTRY_MAP = 925010300;
        const int FRIENDLY_MOB = 9300162;

        public Pirate4thJobFeatureEvent6410() : base(Pirate4thJobFeature.MindEventName)
        {
            MinCount = 1;
            MaxCount = 2;
            MinLevel = 120;
            MaxLevel = 255;

            RecruitMap = 925010200;
            EntryMap = ENTRY_MAP;
            ExitMap = 925010200;
            ClearMap = 925010200;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 6 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map == null)
                return;

            map.toggleDrops();
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            // JS scheduledTimeout：清场 → showClearEffect → clearPQ
            var map = await eim.getInstanceMap(ENTRY_MAP);
            if (map != null)
            {
                await map.killAllMonstersNotFriendly();
            }

            await eim.showClearEffect();
            await ClearPQ(eim);
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // 被护送的戴利死亡 → 任务失败
            if (mob.getId() == FRIENDLY_MOB)
            {
                await End(eim, TerminationReason.Failure);
            }
        }
    }
}
