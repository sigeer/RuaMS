using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class SpCommand : CommandBase
{
    public SpCommand() : base(2, "sp")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.SpCommand_Syntax));
            return Task.CompletedTask;
        }

        if (paramsValue.Length == 1)
        {
            int newSp = int.Parse(paramsValue[0]);
            if (newSp < 0)
            {
                newSp = 0;
            }
            else if (newSp > YamlConfig.config.server.MAX_AP)
            {
                newSp = YamlConfig.config.server.MAX_AP;
            }

            player.updateRemainingSp(newSp);
        }
        else
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null && victim.IsOnlined)
            {
                int newSp = int.Parse(paramsValue[1]);
                if (newSp < 0)
                {
                    newSp = 0;
                }
                else if (newSp > YamlConfig.config.server.MAX_AP)
                {
                    newSp = YamlConfig.config.server.MAX_AP;
                }

                victim.updateRemainingSp(newSp);

                player.MessageI18N(nameof(ClientMessage.Command_Done), player.getLastCommandMessage());
            }
            else
            {
                player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            }
        }
        return Task.CompletedTask;
    }
}
