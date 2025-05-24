using Application.Core.Game.Maps;

namespace Application.Core.Game.Trades
{
    public interface IPlayerShop : IMapObject
    {
        int Channel { get; }
        bool isOpen();
        bool hasItem(int itemid);
        string TypeName { get; }
    }
}
