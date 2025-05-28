using net.server;

namespace Application.Core.Game.Commands.Gm3;

public class ToggleCouponCommand : CommandBase
{
    public ToggleCouponCommand() : base(3, "togglecoupon")
    {
        Description = "Toggle enable/disable a coupon.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !togglecoupon <itemid>");
            return;
        }
        c.CurrentServer.Transport.SendToggleCoupon(int.Parse(paramsValue[0]));
    }
}
