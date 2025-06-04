using Application.Core.Login.ServerTransports;
using Application.Shared.Servers;
using Application.Shared.Team;
using Dto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper: IChannelBroadcast
    {
        protected ChannelServerWrapper(string instanceId, ChannelServerConfig serverNetInfo)
        {
            InstanceId = instanceId;
            ServerConfig = serverNetInfo;
        }
        public string InstanceId { get; protected set; }
        public ChannelServerConfig ServerConfig { get; protected set; }

        public abstract void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target);
    }


    //public class RemoteWorldChannel : ChannelServerWrapper
    //{
    //    public RemoteWorldChannel(string instanceId, ChannelServerConfig serverNetInfo) : base(instanceId, serverNetInfo)
    //    {
    //    }

    //    public override void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
