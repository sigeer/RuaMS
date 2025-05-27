using server;

namespace Application.Core.Game.Commands.Gm2;

public class GmShopCommand : CommandBase
{
    public GmShopCommand() : base(2, "gmshop")
    {
        Description = "Open the GM shop.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.CurrentServer.ShopFactory.getShop(1337)!.sendShop(c);
    }
}
