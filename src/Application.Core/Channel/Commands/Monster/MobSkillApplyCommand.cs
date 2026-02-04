using Application.Core.Game.Life;
using server.life;

namespace Application.Core.Channel.Commands
{
    internal class MobSkillApplyCommand : IWorldChannelCommand
    {
        Monster _mob;
        MobSkill _skill;
        Player _target;

        public MobSkillApplyCommand(Monster mob, MobSkill skill, Player target)
        {
            _mob = mob;
            _skill = skill;
            _target = target;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_mob.isAlive())
            {
                _skill.applyEffect(_target, _mob, true, null);
            }
        }
    }
}
