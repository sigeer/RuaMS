using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class HairCommand : CommandBase
{
    public HairCommand() : base(3, "hair")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.HairCommand_Syntax));
            return;
        }

        IPlayer? targetPlayer = c.OnlinedCharacter;
        string? hairStr;
        if (paramsValue.Length == 1)
        {
            hairStr = paramsValue[0];
        }
        else
        {
            hairStr = paramsValue[1];
            targetPlayer = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        }

        if (!int.TryParse(hairStr, out var hairId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.HairCommand_Syntax));
            return;
        }

        if (!ItemInformationProvider.getInstance().IsHair(hairId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.HairNotFound));
            return;
        }

        if (targetPlayer == null)
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            return;
        }

        targetPlayer.setHair(hairId);
        targetPlayer.updateSingleStat(Stat.HAIR, hairId);
        targetPlayer.equipChanged();
    }
}
