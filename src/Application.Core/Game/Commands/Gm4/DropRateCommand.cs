namespace Application.Core.Game.Commands.Gm4;

public class DropRateCommand : CommandBase
{
    public DropRateCommand() : base(4, "droprate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !droprate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int droprate = Math.Max(d, 1);
        await c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { DropRate = droprate });
    }
}
