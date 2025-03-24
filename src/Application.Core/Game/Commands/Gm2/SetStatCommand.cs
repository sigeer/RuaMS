namespace Application.Core.Game.Commands.Gm2;
public class SetStatCommand : CommandBase
{
    public SetStatCommand() : base(2, "setstat")
    {
        Description = "Set all primary stats to new value.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !setstat <newstat>");
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

            player.updateStrDexIntLuk(x);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}
