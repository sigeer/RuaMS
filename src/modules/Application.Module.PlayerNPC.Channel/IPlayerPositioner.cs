using Application.Core.Game.Maps;
using System.Drawing;

namespace Application.Module.PlayerNPC.Channel
{
    public interface IPlayerPositioner
    {
        int NextPositionData { get; }
        Point? GetNextPlayerNpcPosition(IMap map);
    }
}
