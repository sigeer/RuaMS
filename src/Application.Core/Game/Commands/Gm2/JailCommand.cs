using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class JailCommand : CommandBase
{
    public JailCommand() : base(2, "jail")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.JailCommand_Syntax));
            return Task.CompletedTask;
        }

        int minutesJailed = 5;
        if (paramsValue.Length >= 2)
        {
            if (!int.TryParse(paramsValue[1], out minutesJailed))
            {
                player.YellowMessageI18N(nameof(ClientMessage.JailCommand_Syntax));
                return Task.CompletedTask;
            }
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            victim.addJailExpirationTime(minutesJailed * 60 * 1000);

            if (victim.getMapId() != MapId.JAIL)
            {
                // those gone to jail won't be changing map anyway
                var target = c.getChannelServer().getMapFactory().getMap(MapId.JAIL);
                var targetPortal = target.getPortal(0);
                victim.saveLocationOnWarp();
                victim.changeMap(target, targetPortal);
                player.MessageI18N(nameof(ClientMessage.Jail_Result), victim.getName(), minutesJailed.ToString());
            }
            else
            {
                player.MessageI18N(nameof(ClientMessage.Jail_ExtendResult), victim.getName(), minutesJailed.ToString());
            }

        }
        else
        {
            player.MessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel));
        }
        return Task.CompletedTask;
    }
}
