namespace Application.Core.Game.Commands.Gm4;

public class ExpRateCommand : CommandBase
{
    public ExpRateCommand() : base(4, "exprate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !exprate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int exprate = Math.Max(d, 1);
        await c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { ExpRate = exprate });
    }
}
