namespace Application.Core.Game.Commands.Gm4;

public class MesoRateCommand : CommandBase
{
    public MesoRateCommand() : base(4, "mesorate")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !mesorate <newrate>");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return;

        int mesorate = Math.Max(d, 1);
        _ = c.getChannelServer().Node.Transport.SendWorldConfig(new Config.WorldConfig { MesoRate = mesorate });
    }
}
