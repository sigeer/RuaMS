using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class FameCommand : CommandBase
{
    public FameCommand() : base(3, "fame")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.FameCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[1], out var fame))
        {
            await player.Yellow(nameof(ClientMessage.FameCommand_Syntax));
            return;
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            victim.setFame(fame);
            await victim.updateSingleStat(Stat.FAME, victim.getFame());
            await player.Pink(nameof(ClientMessage.FameCommand_FameGiven));
        }
        else
        {
            await player.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel));
        }
    }
}
