using Application.Core.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    ///  A. 天空之城 - b. 阿里安特
    /// </summary>
    internal class Genie : ContiMoveBase
    {
        public Genie(WorldChannel channelServer) : 
            base(channelServer,
                200000100, 0, 260000100, 0,
                200000152, 260000110,
                200090400, 200090410,
                200000151, 260000100,
                5 * 60 * 1000, 4 * 60 * 1000, 5 * 60 * 1000,
                4031045, 1)
        {
        }
    }
}
