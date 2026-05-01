using Application.Core.Channel;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 废弃都市 B. 新叶城
    /// </summary>
    public class Subway : ContiMoveBase
    {
        public Subway(WorldChannel channelServer) : base(channelServer,
                103000100, 0, 600010001, 0,
                600010004, 600010002,
                600010005, 600010003,
                103000100, 600010001, 
                60 * 1000, 50 * 1000, 4 * 60 * 1000,
                4031711, 5000,
                4031713, 5000)
        {
        }
    }
}
