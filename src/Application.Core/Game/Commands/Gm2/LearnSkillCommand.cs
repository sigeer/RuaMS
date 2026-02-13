using Application.Core.Game.Skills;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.Commands.Gm2
{
    internal class LearnSkillCommand : ParamsCommandBase
    {
        public LearnSkillCommand() : base(["<skillId>"], 2, "learn")
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var skillId = GetIntParam("skillId");

            var skill = SkillFactory.getSkill(skillId);
            if (skill == null)
            {
                client.OnlinedCharacter.Yellow($"没有找到技能{skillId}");
                return;
            }

            client.OnlinedCharacter.LearnExtraSkill(skillId);
            client.OnlinedCharacter.Notice($"学习【{client.CurrentCulture.GetSkillName(skillId)}】成功，技能设置在=键上");
        }
    }
}
