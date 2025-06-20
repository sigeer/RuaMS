using Application.Core.Login.ServerTransports;
using Application.Shared.Servers;
using Application.Shared.Team;
using Config;
using Dto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper 
    {
        protected ChannelServerWrapper(string serverName, List<WorldChannelConfig> serverConfigs)
        {
            ServerName = serverName;
            ServerConfigs = serverConfigs;
        }
        public string ServerName { get; protected set; }
        public List<WorldChannelConfig> ServerConfigs { get; }

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
