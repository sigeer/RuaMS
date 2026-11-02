using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q6002Event : SoloQuestEventTemplate
    {
        public Q6002Event() : base(Q6002Feature.QuestIdValue)
        {
            MaxLobbys = 7;
            EventTime = 5 * 60;
            EntryMap = 923010000;
            ExitMap = 923010100;
            MinMap = 923010000;
            MaxMap = 923010000;
        }

        public override async Task OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
        {
            if (mob.getHp() < mob.getMaxHp() / 2.0)
            {
                await eim.Notice("小心！外星人想抓走猪猪去烤乳猪！快挡住它们！");
            }
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            await End(eim, Core.scripting.Events.Abstraction.TerminationReason.Failure);
        }

    }
}
