using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadShopsCommand : CommandBase
{
    readonly ShopManager _shopManager;
    public ReloadShopsCommand(ShopManager shopManager) : base(3, "reloadshops")
    {
        Description = "Reload popup shops and NPC shops.";
        _shopManager = shopManager;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        _shopManager.reloadShops();
    }
}
