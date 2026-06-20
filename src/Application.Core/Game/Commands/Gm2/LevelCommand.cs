using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class LevelCommand : CommandBase
{
    public LevelCommand() : base(2, "level")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.LevelCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[0], out var newlevel))
        {
            await player.Yellow(nameof(ClientMessage.LevelCommand_Syntax));
            return;
        }

        await player.loseExp(player.getExp(), false, false);
        player.setLevel(Math.Min(newlevel, player.getMaxClassLevel()) - 1);

        await player.levelUp(false);
    }
}
