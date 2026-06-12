using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class q6002 : AbstractSoloEventTemplate
    {
        public q6002() : base(nameof(q6002))
        {
            MaxLobbys = 7;
            EventTime = 5 * 60;
            EntryMap = 923010000;
            ExitMap = 923010100;
            MinMap = 923010000;
            MaxMap = 923010000;
        }

        public override void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
        {
            if (mob.getHp() < mob.getMaxHp() / 2.0)
            {
                eim.Notice("小心！外星人想抓走猪猪去烤乳猪！快挡住它们！");
            }
        }

        public override void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            End(eim);
        }
    }
}
