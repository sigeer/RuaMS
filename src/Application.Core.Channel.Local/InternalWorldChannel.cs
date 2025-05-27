using Application.Core.Game.TheWorld;
using Application.Core.Servers;

namespace Application.Core.Channel.Local
{
    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(IWorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ServerConfig)
        {
            WorldChannel = worldChannel;
        }

        public IWorldChannel WorldChannel { get; }
    }
}
