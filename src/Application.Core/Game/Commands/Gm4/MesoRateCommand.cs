namespace Application.Core.Game.Commands.Gm4;

public class MesoRateCommand : CommandBase
{
    public MesoRateCommand() : base(4, "mesorate")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !mesorate <newrate>");
            return Task.CompletedTask;
        }

        if (!int.TryParse(paramsValue[0], out var d))
            return Task.CompletedTask;

        int mesorate = Math.Max(d, 1);
        c.getChannelServer().Container.Transport.SendWorldConfig(new Config.WorldConfig { MesoRate = mesorate });
        return Task.CompletedTask;
    }
}
