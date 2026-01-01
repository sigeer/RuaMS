using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class LevelCommand : CommandBase
{
    public LevelCommand() : base(2, "level")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.LevelCommand_Syntax));
            return Task.CompletedTask;
        }

        if (!int.TryParse(paramsValue[0], out var newlevel))
        {
            player.YellowMessageI18N(nameof(ClientMessage.LevelCommand_Syntax));
            return Task.CompletedTask;
        }

        player.loseExp(player.getExp(), false, false);
        player.setLevel(Math.Min(newlevel, player.getMaxClassLevel()) - 1);

        player.levelUp(false);
        return Task.CompletedTask;
    }
}
