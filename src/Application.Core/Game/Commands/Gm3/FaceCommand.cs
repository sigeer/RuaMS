using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class FaceCommand : CommandBase
{
    public FaceCommand() : base(3, "face")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.FaceCommand_Syntax));
            return;
        }

        Player? targetPlayer = c.OnlinedCharacter;
        string? faceStr;
        if (paramsValue.Length == 1)
        {
            faceStr = paramsValue[0];
        }
        else
        {
            faceStr = paramsValue[1];
            targetPlayer = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        }

        if (!int.TryParse(faceStr, out var faceId))
        {
            await player.Yellow(nameof(ClientMessage.FaceCommand_Syntax));
            return;
        }

        if (!ItemInformationProvider.getInstance().IsFace(faceId))
        {
            await player.Yellow(nameof(ClientMessage.FaceNotFound));
            return;
        }

        if (targetPlayer == null)
        {
            await player.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            return;
        }

        targetPlayer.setFace(faceId);
        await targetPlayer.updateSingleStat(Stat.FACE, faceId);
        await targetPlayer.equipChanged();
    }
}
