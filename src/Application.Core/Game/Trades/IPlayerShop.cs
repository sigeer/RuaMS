using Application.Core.Game.Maps;
using Application.Shared.MapObjects;

namespace Application.Core.Game.Trades
{
    public interface IPlayerShop: IMapObject
    {
        int Channel { get; }
        bool isOpen();
        bool hasItem(int itemid);
        string TypeName { get;}
    }
}
