using Application.Shared.Servers;

namespace Application.Core.Game.TheWorld
{
    public abstract class ServerChannelWrapper
    {
        protected ServerChannelWrapper(string instanceId, ActualServerConfig serverNetInfo)
        {
            InstanceId = instanceId;
            ServerConfig = serverNetInfo;
        }

        public string InstanceId { get; protected set; }
        public ActualServerConfig ServerConfig { get; protected set; }
    }

    public class InternalWorldChannel : ServerChannelWrapper
    {
        public InternalWorldChannel(IWorldChannel worldChannel) : base(worldChannel.InstanceId, worldChannel.ServerConfig)
        {
            WorldChannel = worldChannel;
        }

        public IWorldChannel WorldChannel { get; }
    }
    public class RemoteWorldChannel : ServerChannelWrapper
    {
        public RemoteWorldChannel(string instanceId, ActualServerConfig serverNetInfo) : base(instanceId, serverNetInfo)
        {
        }
    }
}
