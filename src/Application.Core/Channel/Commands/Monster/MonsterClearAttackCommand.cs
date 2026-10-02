using Application.Core.Game.Life;
using Application.Core.Server.life;

namespace Application.Core.Channel.Commands
{

    internal class MonsterClearEffectCommand : IWorldChannelCommand
    {
        public string Name => nameof(MonsterClearEffectCommand);
        Monster _mob;
        Element _ele;

        public MonsterClearEffectCommand(Monster mob, Element element)
        {
            _mob = mob;
            _ele = element;
        }

        public void Execute(WorldChannel ctx)
        {
            var stats = _mob.getStats();
            stats.removeEffectiveness(_ele);
            stats.setEffectiveness(_ele, stats.getEffectiveness(_ele));
        }
    }
}
