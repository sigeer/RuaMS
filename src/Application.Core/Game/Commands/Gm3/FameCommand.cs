using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class FameCommand : CommandBase
{
    public FameCommand() : base(3, "fame")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.YellowMessageI18N(nameof(ClientMessage.FameCommand_Syntax));
            return Task.CompletedTask;
        }

        if (!int.TryParse(paramsValue[1], out var fame))
        {
            player.YellowMessageI18N(nameof(ClientMessage.FameCommand_Syntax));
            return Task.CompletedTask;
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            victim.setFame(fame);
            victim.updateSingleStat(Stat.FAME, victim.getFame());
            player.MessageI18N(nameof(ClientMessage.FameCommand_FameGiven));
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel));
        }
        return Task.CompletedTask;
    }
}
