using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class ApCommand : CommandBase
{
    public ApCommand() : base(2, "ap")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.ApCommand_Syntax));
            return;
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

            await player.changeRemainingAp(newAp, false);
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

                await victim.changeRemainingAp(newAp, false);
            }
            else
            {
                await player.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            }
        }
    }
}
