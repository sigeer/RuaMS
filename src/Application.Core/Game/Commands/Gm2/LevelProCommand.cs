namespace Application.Core.Game.Commands.Gm2;

public class LevelProCommand : ParamsCommandBase
{
    public LevelProCommand() : base(["<newlevel>"], 2, "levelpro")
    {
        Description = "Set your level, one by one.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        var newLevel = GetIntParam("newlevel");
        var targetLevel = Math.Min(player.getMaxClassLevel(), newLevel);
        while (player.getLevel() < targetLevel)
        {
            player.levelUp(false);
        }
    }
}
