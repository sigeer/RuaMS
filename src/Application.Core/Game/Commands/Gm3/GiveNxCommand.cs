using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class GiveNxCommand : CommandBase
{
    public GiveNxCommand() : base(3, "givenx")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.GiveNxCommand_Syntax));
            return;
        }

        string recv, typeStr = "nx";
        int value, type = 1;
        if (paramsValue.Length > 1)
        {
            if (paramsValue[0].Length == 2)
            {
                switch (paramsValue[0])
                {
                    case "mp":  // maplePoint
                        type = 2;
                        break;
                    case "np":  // nxPrepaid
                        type = 4;
                        break;
                    default:
                        type = 1;
                        break;
                }
                typeStr = paramsValue[0];

                if (paramsValue.Length > 2)
                {
                    recv = paramsValue[1];
                    value = int.Parse(paramsValue[2]);
                }
                else
                {
                    recv = c.OnlinedCharacter.getName();
                    value = int.Parse(paramsValue[1]);
                }
            }
            else
            {
                recv = paramsValue[0];
                value = int.Parse(paramsValue[1]);
            }
        }
        else
        {
            recv = c.OnlinedCharacter.getName();
            value = int.Parse(paramsValue[0]);
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(recv);
        if (victim != null && victim.IsOnlined)
        {
            victim.getCashShop().gainCash(type, value);
            player.YellowMessageI18N(nameof(ClientMessage.Command_Done), player.getLastCommandMessage());
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
        }
    }
}
