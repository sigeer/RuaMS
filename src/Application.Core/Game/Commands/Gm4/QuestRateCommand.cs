using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm4;
public class QuestRateCommand : CommandBase
{
    public QuestRateCommand() : base(4, "questrate")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !questrate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
        {
            player.YellowMessageI18N(nameof(ClientMessage.DataTypeIncorrect), player.GetMessageByKey(nameof(ClientMessage.DataType_Number)));
            return;
        }

        int questrate = Math.Max(d, 1);
        _ = c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { QuestRate = questrate });
    }
}
