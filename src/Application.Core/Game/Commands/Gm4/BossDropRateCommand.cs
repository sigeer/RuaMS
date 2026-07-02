namespace Application.Core.Game.Commands.Gm4;

public class BossDropRateCommand : CommandBase
{
    public BossDropRateCommand() : base(4, "bossdroprate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !bossdroprate <newrate>");
        }

        if (int.TryParse(paramsValue[0], out var d))
        {
            int bossdroprate = Math.Max(d, 1);
            await c.CurrentServer.Node.Transport.SendWorldConfig(new Config.WorldConfig() { BossDropRate = bossdroprate });
        }

    }
}
