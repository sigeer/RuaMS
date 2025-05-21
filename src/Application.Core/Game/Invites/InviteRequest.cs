using Application.Core.Game.Maps;
using net.server;

namespace Application.Core.Game.Invites
{
    public class InviteRequest
    {
        public InviteRequest(IPlayer from, IPlayer to)
        {
            CreationTime = from.Client.CurrentServer.getCurrentTime();
            From = from;
            To = to;
        }

        public long CreationTime { get; set; }
        public IPlayer From { get; }
        public IPlayer To { get; }
    }

    public class GuildInviteRequest: InviteRequest
    {
        public GuildInviteRequest(IPlayer from, IPlayer to) : base(from, to)
        {
            GuildId = from.GuildId;
        }

        public int GuildId { get; set; }
    }

    public class AllianceInviteRequest: InviteRequest
    {
        public AllianceInviteRequest(IPlayer from, IPlayer to) : base(from, to)
        {
            AllianceId = from.AllianceModel!.AllianceId;
        }

        public int AllianceId { get; set; }
    }

    public class TeamInviteRequest : InviteRequest
    {
        public TeamInviteRequest(IPlayer from, IPlayer to) : base(from, to)
        {
            TeamId = from.TeamModel!.getId();
        }

        public int TeamId { get; set; }
    }

    public class ChatInviteRequest : InviteRequest
    {
        public ChatInviteRequest(IPlayer from, IPlayer to, int messngerId) : base(from, to)
        {
            ChatRoomId = messngerId;
        }

        public int ChatRoomId { get; set; }
    }

    public class FamilySummonInviteRequest : InviteRequest
    {
        public FamilySummonInviteRequest(IPlayer from, IPlayer to) : base(from, to)
        {
            Map = from.getMap();
        }

        public IMap Map { get; set; }
    }
}
