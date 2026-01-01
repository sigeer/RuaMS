using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class UnJailCommand : CommandBase
{
    public UnJailCommand() : base(2, "unjail")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.UnJailCommand_Syntax));
            return Task.CompletedTask;
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            if (victim.getJailExpirationTimeLeft() <= 0)
            {
                player.YellowMessageI18N(nameof(ClientMessage.UnjailCommand_AlreadyFree));
                return Task.CompletedTask;
            }
            victim.removeJailExpirationTime();
            victim.MessageI18N(nameof(ClientMessage.Unjail_Notify));
            player.YellowMessageI18N(nameof(ClientMessage.Command_Done), player.getLastCommandMessage());
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
        }
        return Task.CompletedTask;
    }
}
