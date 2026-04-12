using Application.Core.Channel;

namespace Application.Core.Game.ContiMove
{
    internal class AirPlane : ContiMoveBase
    {
        public AirPlane(WorldChannel channelServer)
            : base(channelServer, 103000000, 7, 540010000, 0,
                  540010100, 540010001,
                  540010101, 540010002,
                  -1, -1,
                  5 * 60 * 1000, 4 * 60 * 1000, 1 * 60 * 1000,
                  4031731, 1)
        {
        }
    }
}
