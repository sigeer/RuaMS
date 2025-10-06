namespace Application.Core.Game.Commands.Gm4;

public class DropRateCommand : CommandBase
{
    public DropRateCommand() : base(4, "droprate")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !droprate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int droprate = Math.Max(d, 1);
        c.getChannelServer().Container.Transport.SendWorldConfig(new Config.WorldConfig { DropRate = droprate });

    }
}
