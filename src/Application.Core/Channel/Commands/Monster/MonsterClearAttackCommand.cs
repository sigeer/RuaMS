using Application.Core.Game.Life;
using Application.Core.Game.Skills;
using server.life;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class MonsterClearAttackCommand : IWorldChannelCommand
    {
        Monster _mob;
        int _attackPos;

        public MonsterClearAttackCommand(Monster mob, int attackPos)
        {
            _mob = mob;
            _attackPos = attackPos;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.clearAttack(_attackPos);
        }
    }

    internal class MonsterClearSkillCommand : IWorldChannelCommand
    {
        Monster _mob;
        MobSkill _mobSkill;

        public MonsterClearSkillCommand(Monster mob, MobSkill mobSkill)
        {
            _mob = mob;
            _mobSkill = mobSkill;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mob.clearSkill(_mobSkill.getId());
        }
    }

    internal class MonsterClearEffectCommand : IWorldChannelCommand
    {
        Monster _mob;
        Element _ele;

        public MonsterClearEffectCommand(Monster mob, Element element)
        {
            _mob = mob;
            _ele = element;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var stats = _mob.getStats();
            stats.removeEffectiveness(_ele);
            stats.setEffectiveness(_ele, stats.getEffectiveness(_ele));
        }
    }
}
