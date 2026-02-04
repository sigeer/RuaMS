namespace Application.Core.Game.Commands.Gm4;

public class ExpRateCommand : CommandBase
{
    public ExpRateCommand() : base(4, "exprate")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !exprate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int exprate = Math.Max(d, 1);
        _ = c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { ExpRate = exprate });
    }
}
