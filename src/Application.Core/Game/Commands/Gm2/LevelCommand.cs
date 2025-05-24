namespace Application.Core.Game.Commands.Gm2;

public class LevelCommand : CommandBase
{
    public LevelCommand() : base(2, "level")
    {
        Description = "Set your level.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !level <newlevel>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var newlevel))
        {
            player.yellowMessage("Syntax: <newlevel> invalid");
            return;
        }

        player.loseExp(player.getExp(), false, false);
        player.setLevel(Math.Min(newlevel, player.getMaxClassLevel()) - 1);

        player.levelUp(false);
    }
}
