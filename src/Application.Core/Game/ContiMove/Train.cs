using Application.Core.Channel;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 天空之城 - B. 玩具城
    /// </summary>
    internal class Train : ContiMoveBase
    {
        public Train(WorldChannel channelServer)
            : base(channelServer, 
                  200000100, 0, 220000100, 0, 
                  200000122, 220000111, 
                  200090110, 200090100, 
                  200000121, 220000110,
                  5 * 60 * 1000, 4 * 60 * 1000, 5 * 60 * 1000,
                  4031045, 1)
        {
        }
    }
}
