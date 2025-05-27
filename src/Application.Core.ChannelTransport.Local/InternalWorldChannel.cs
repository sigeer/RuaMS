using Application.Core.Channel;
using Application.Core.Login.Servers;

namespace Application.Core.ChannelTransport.Local
{
    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(WorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ServerConfig)
        {
            WorldChannel = worldChannel;
        }

        public WorldChannel WorldChannel { get; }
    }
}
