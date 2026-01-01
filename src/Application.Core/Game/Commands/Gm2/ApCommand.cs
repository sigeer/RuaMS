using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class ApCommand : CommandBase
{
    public ApCommand() : base(2, "ap")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ApCommand_Syntax));
            return Task.CompletedTask;
        }

        if (paramsValue.Length < 2)
        {
            int newAp = int.Parse(paramsValue[0]);
            if (newAp < 0)
            {
                newAp = 0;
            }
            else if (newAp > YamlConfig.config.server.MAX_AP)
            {
                newAp = YamlConfig.config.server.MAX_AP;
            }

            player.changeRemainingAp(newAp, false);
        }
        else
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null && victim.IsOnlined)
            {
                int newAp = int.Parse(paramsValue[1]);
                if (newAp < 0)
                {
                    newAp = 0;
                }
                else if (newAp > YamlConfig.config.server.MAX_AP)
                {
                    newAp = YamlConfig.config.server.MAX_AP;
                }

                victim.changeRemainingAp(newAp, false);
            }
            else
            {
                player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            }
        }
        return Task.CompletedTask;
    }
}
