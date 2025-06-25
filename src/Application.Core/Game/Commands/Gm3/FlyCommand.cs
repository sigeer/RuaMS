namespace Application.Core.Game.Commands.Gm3;
public class FlyCommand : ParamsCommandBase
{
    public FlyCommand() : base(["[on|off]"], 3, "fly")
    {
        Description = "Enable/disable fly feature.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.SetFly(GetParamByIndex(0)?.Equals("on", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
