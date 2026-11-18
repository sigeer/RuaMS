using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm4;

public class TravelRateCommand : CommandBase
{
    public TravelRateCommand() : base(4, "travelrate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !travelrate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out int d))
        {
            await player.Yellow(ClientMessage.DataTypeIncorrect, player.GetMessageByKey(ClientMessage.DataType_Number));
            return;
        }

        int travelrate = Math.Max(d, 1);
        await c.getChannelServer().Node.Transport.SendWorldConfig(new ProtoModel.WorldConfig { TravelRate = travelrate });
    }
}
