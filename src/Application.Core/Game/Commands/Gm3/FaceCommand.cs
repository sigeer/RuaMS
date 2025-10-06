using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class FaceCommand : CommandBase
{
    public FaceCommand() : base(3, "face")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.FaceCommand_Syntax));
            return;
        }

        IPlayer? targetPlayer = c.OnlinedCharacter;
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
            player.YellowMessageI18N(nameof(ClientMessage.FaceCommand_Syntax));
            return;
        }

        if (!ItemInformationProvider.getInstance().IsFace(faceId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.FaceNotFound));
            return;
        }

        if (targetPlayer == null)
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            return;
        }

        targetPlayer.setFace(faceId);
        targetPlayer.updateSingleStat(Stat.FACE, faceId);
        targetPlayer.equipChanged();
    }
}
