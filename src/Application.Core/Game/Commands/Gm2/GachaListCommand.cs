namespace Application.Core.Game.Commands.Gm2;

/**
 * @author Ronan
 */
public class GachaListCommand : CommandBase
{
    public GachaListCommand() : base(2, "gachalist")
    {
        Description = "Show gachapon rewards.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OpenNpc(10000, "gachaponInfo");
    }
}
