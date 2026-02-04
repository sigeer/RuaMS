namespace Application.Core.Game.Commands.Gm4;

public class FishingRateCommand : CommandBase
{
    public FishingRateCommand() : base(4, "fishrate")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !fishrate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int fishrate = Math.Max(d, 1);
        _ = c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { FishingRate = fishrate });
    }
}
