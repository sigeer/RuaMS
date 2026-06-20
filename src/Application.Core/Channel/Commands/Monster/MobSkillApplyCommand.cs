using Application.Core.Game.Life;
using server.life;

namespace Application.Core.Channel.Commands
{
    internal class MobSkillApplyCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(MobSkillApplyCommand);
        Monster _mob;
        MobSkill _skill;
        Player _target;

        public MobSkillApplyCommand(Monster mob, MobSkill skill, Player target)
        {
            _mob = mob;
            _skill = skill;
            _target = target;
        }

        public async Task Execute(WorldChannel ctx)
        {
            if (_mob.isAlive())
            {
                await _skill.applyEffect(_target, _mob, true, null);
            }
        }
    }
}
