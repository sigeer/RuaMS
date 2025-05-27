using server;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadShopsCommand : CommandBase
{
    public ReloadShopsCommand() : base(3, "reloadshops")
    {
        Description = "Reload popup shops and NPC shops.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.CurrentServer.ShopFactory.reloadShops();
    }
}
