using Application.Core.Servers;

namespace Application.Core.Channel.Local
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
