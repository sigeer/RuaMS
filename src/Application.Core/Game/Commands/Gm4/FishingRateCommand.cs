namespace Application.Core.Game.Commands.Gm4;

public class FishingRateCommand : CommandBase
{
    public FishingRateCommand() : base(4, "fishrate")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !fishrate <newrate>");
            return Task.CompletedTask;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return Task.CompletedTask;

        int fishrate = Math.Max(d, 1);
        c.getChannelServer().Container.Transport.SendWorldConfig(new Config.WorldConfig { FishingRate = fishrate });
        return Task.CompletedTask;
    }
}
