using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm2;

public class GmShopCommand : CommandBase
{
    readonly ShopManager _shopManager;
    public GmShopCommand(ShopManager shopManager) : base(2, "gmshop")
    {
        Description = "Open the GM shop.";
        _shopManager = shopManager;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        _shopManager.getShop(1337)!.sendShop(c);
        return Task.CompletedTask;
    }
}
