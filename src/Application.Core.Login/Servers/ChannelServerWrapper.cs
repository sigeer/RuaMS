using Application.Core.Login.ServerTransports;
using Application.Shared.Servers;
using Application.Shared.Team;
using Config;
using Dto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper : IChannelBroadcast
    {
        protected ChannelServerWrapper(string serverName, List<WorldChannelConfig> serverConfigs)
        {
            ServerName = serverName;
            ServerConfigs = serverConfigs;
        }
        public string ServerName { get; protected set; }
        public List<WorldChannelConfig> ServerConfigs { get; }


        public abstract void SendTeamUpdate(int teamId, PartyOperation operation, TeamMemberDto target);

        public abstract void BroadcastJobChanged(int type, int[] players, string name, int jobId);
        public abstract void BroadcastLevelChanged(int type, int[] value, string name, int level);
        #region GuildBroadcast
        public abstract void SendGuildUpdate(UpdateGuildResponse response);
        public abstract void SendAllianceUpdate(UpdateAllianceResponse response);

        #endregion
        public abstract void UpdateCouponConfig(CouponConfig config);
        public abstract void SendMultiChat(int type, string nameFrom, int[] value, string chatText);
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
