using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class SpCommand : CommandBase
{
    public SpCommand() : base(2, "sp")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(ClientMessage.SpCommand_Syntax);
            return;
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

            await player.updateRemainingSp(newSp);
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

                await victim.updateRemainingSp(newSp);

                await player.MessageI18N(ClientMessage.Command_Done, player.getLastCommandMessage());
            }
            else
            {
                await player.Yellow(ClientMessage.PlayerNotFoundInChannel, paramsValue[0]);
            }
        }
    }
}
