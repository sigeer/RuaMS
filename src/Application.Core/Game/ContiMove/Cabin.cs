using Application.Core.Channel;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 天空之城 - B. 神木村
    /// </summary>
    internal class Cabin : ContiMoveBase
    {
        public Cabin(WorldChannel channelServer)
            : base(channelServer,
                  200000100, 0, 240000100, 0,
                  200000132, 240000111,
                  200090200, 200090210,
                  200000131, 240000110,
                  5 * 60 * 1000, 4 * 60 * 1000, 5 * 60 * 1000,
                  4031331, 30000,
                  4031045, 30000)
        {
        }
    }
}
