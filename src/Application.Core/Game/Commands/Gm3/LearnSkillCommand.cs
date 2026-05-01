namespace Application.Core.Game.Commands.Gm3
{
    public class LearnSkillCommand : ParamsCommandBase
    {
        public LearnSkillCommand() : base(["<skillid>", "<skilllevel>"], 3, "learnskill")
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var skillId = GetIntParam("skillid");
            var skillLevel = TryGetIntParam("skilllevel", -1);

            client.OnlinedCharacter.LearnSkill(skillId);
        }
    }
}
