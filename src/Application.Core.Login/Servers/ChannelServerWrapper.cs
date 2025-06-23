using Application.Shared.Servers;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper
    {
        protected ChannelServerWrapper(string serverName, List<ChannelConfig> serverConfigs)
        {
            ServerName = serverName;
            ServerConfigs = serverConfigs;
        }
        public string ServerName { get; protected set; }
        public List<ChannelConfig> ServerConfigs { get; }

        public abstract void BroadcastMessage(string type, object message);
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
