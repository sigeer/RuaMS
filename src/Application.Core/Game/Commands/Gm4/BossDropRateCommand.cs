namespace Application.Core.Game.Commands.Gm4;

public class BossDropRateCommand : CommandBase
{
    public BossDropRateCommand() : base(4, "bossdroprate")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !bossdroprate <newrate>");
            return;
        }

        if (int.TryParse(paramsValue[0], out var d))
        {
            int bossdroprate = Math.Max(d, 1);
            c.CurrentServerContainer.Transport.SendWorldConfig(new Config.WorldConfig() { BossDropRate = bossdroprate });
        }

    }
}
