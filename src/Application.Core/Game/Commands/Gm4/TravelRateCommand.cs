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
            player.yellowMessage("Syntax: !travelrate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out int d))
        {
            player.YellowMessageI18N(nameof(ClientMessage.DataTypeIncorrect), player.GetMessageByKey(nameof(ClientMessage.DataType_Number)));
            return;
        }

        int travelrate = Math.Max(d, 1);
        await c.getChannelServer().Container.Transport.SendWorldConfig(new Config.WorldConfig { TravelRate = travelrate });
    }
}
