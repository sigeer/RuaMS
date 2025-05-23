namespace Application.Core.Game.Commands.Gm2;

public class MaxStatCommand : CommandBase
{
    public MaxStatCommand() : base(2, "maxstat")
    {
        Description = "Max out all character stats.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.MaxStat();
        c.OnlinedCharacter.yellowMessage("Stats maxed out.");
    }
}
