using Application.Core.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.Players
{
    public interface IClientPlayer : IClientMessenger
    {
        IChannelClient Client { get; }
        void sendPacket(Packet packet);
    }
}
