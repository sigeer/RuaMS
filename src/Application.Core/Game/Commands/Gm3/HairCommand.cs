using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class HairCommand : CommandBase
{
    public HairCommand() : base(3, "hair")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.HairCommand_Syntax));
            return;
        }

        Player? targetPlayer = c.OnlinedCharacter;
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
            await player.Yellow(nameof(ClientMessage.HairCommand_Syntax));
            return;
        }

        if (!ItemInformationProvider.getInstance().IsHair(hairId))
        {
            await player.Yellow(nameof(ClientMessage.HairNotFound));
            return;
        }

        if (targetPlayer == null)
        {
            await player.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            return;
        }

        targetPlayer.setHair(hairId);
        await targetPlayer.updateSingleStat(Stat.HAIR, hairId);
        await targetPlayer.equipChanged();
    }
}
