namespace Application.Core.Game.Commands.Gm3
{
    public class LearnSkillCommand : ParamsCommandBase
    {
        public LearnSkillCommand(string[] arugments, int level, params string[] syntax) : base(["<skillid>"], 3, "learnskill")
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var skillId = GetIntParam("skillid");

            client.OnlinedCharacter.LearnSkill(skillId);
        }
    }
}
