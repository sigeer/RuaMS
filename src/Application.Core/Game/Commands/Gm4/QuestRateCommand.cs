using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm4;

public class QuestRateCommand : CommandBase
{
    public QuestRateCommand() : base(4, "questrate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !questrate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
        {
            await player.Yellow(ClientMessage.DataTypeIncorrect, player.GetMessageByKey(ClientMessage.DataType_Number));
            return;
        }

        int questrate = Math.Max(d, 1);
        await c.getChannelServer().Node.Transport.SendWorldConfig(new ProtoModel.WorldConfig { QuestRate = questrate });
    }
}
