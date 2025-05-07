using Application.Core.Game.TheWorld;

namespace Application.Core.Servers
{
    public abstract class ChannelServerWrapper
    {
        protected ChannelServerWrapper(string instanceId, ChannelServerConfig serverNetInfo)
        {
            InstanceId = instanceId;
            ServerConfig = serverNetInfo;
        }

        public string InstanceId { get; protected set; }
        public ChannelServerConfig ServerConfig { get; protected set; }
    }

    public class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(IWorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ServerConfig)
        {
            WorldChannel = worldChannel;
        }

        public IWorldChannel WorldChannel { get; }
    }
    public class RemoteWorldChannel : ChannelServerWrapper
    {
        public RemoteWorldChannel(string instanceId, ChannelServerConfig serverNetInfo) : base(instanceId, serverNetInfo)
        {
        }
    }
}
