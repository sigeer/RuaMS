namespace Application.Core.Game.Commands.Gm2;

public class SetStatCommand : CommandBase
{
    public SetStatCommand() : base(2, "setstat")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !setstat <newstat>");
            return;
        }

        try
        {
            int x = int.Parse(paramsValue[0]);

            if (x > NumericConfig.MaxStat)
            {
                x = NumericConfig.MaxStat;
            }
            else if (x < NumericConfig.MinStat)
            {
                // thanks Vcoc for pointing the minimal allowed stat value here
                x = NumericConfig.MinStat;
            }

            await player.updateStrDexIntLuk(x);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}
